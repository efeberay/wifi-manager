using System.Diagnostics;
using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;
using WifiManager.Models;

namespace WifiManager.Core
{
    public class DnsMonitor : IDisposable
    {
        public record DnsEntry(string Time, string Label, string IP, string MAC, string Domain);

        private readonly ArpTool     _arp;
        private readonly DeviceStore _store;
        private CancellationTokenSource? _cts;
        private int   _sessionId = 0;
        private Task? _runTask;

        public event Action<DnsEntry>?         OnUpdate;
        public event Action<string, bool>?     OnLog;   // mesaj, isError

        public DnsMonitor(ArpTool arp, DeviceStore store)
        {
            _arp   = arp;
            _store = store;
        }

        // ----------------------------------------------------------------
        // Tek cihaz
        // ----------------------------------------------------------------
        public void StartSingle(DeviceInfo target, string gatewayMAC)
        {
            Stop();
            var prevTask  = _runTask;
            int mySession = Interlocked.Increment(ref _sessionId);
            _cts = new CancellationTokenSource();
            var ct = _cts.Token;
            _runTask = Task.Run(() =>
            {
                // Eski Run() + MITM finally tamamlanmadan yeni oturum device'a dokunmasın
                try { prevTask?.Wait(TimeSpan.FromSeconds(5)); } catch { }
                Run(new List<DeviceInfo> { target }, gatewayMAC, ct, mySession);
            });
        }

        // ----------------------------------------------------------------
        // Tüm cihazlar
        // ----------------------------------------------------------------
        public void StartAll(List<DeviceInfo> targets, string gatewayMAC)
        {
            Stop();
            var prevTask  = _runTask;
            int mySession = Interlocked.Increment(ref _sessionId);
            _cts = new CancellationTokenSource();
            var ct = _cts.Token;
            _runTask = Task.Run(() =>
            {
                try { prevTask?.Wait(TimeSpan.FromSeconds(5)); } catch { }
                Run(targets, gatewayMAC, ct, mySession);
            });
        }

        // ----------------------------------------------------------------
        // Ana döngü
        // ----------------------------------------------------------------
        private void Run(List<DeviceInfo> targets, string gatewayMAC, CancellationToken ct, int sessionId)
        {
            // Arayüz adı yoksa hiç başlama
            if (_arp.Device == null)
            {
                OnLog?.Invoke("  [!] Ağ arayüzü bulunamadı — DNS izleme başlatılamıyor.", true);
                return;
            }

            // IP Forwarding: açılamazsa hedefin interneti kesilebilir → izleme iptal
            bool globalOk = NetworkHelper.SetIPForwarding(true);
            bool ifaceOk  = SetIpForwarding(true);

            if (!ifaceOk && !globalOk)
            {
                OnLog?.Invoke("  [!] IP yönlendirme (forwarding) açılamadı — hedefin interneti kesilebileceğinden DNS izleme başlatılmıyor.", true);
                OnLog?.Invoke("      Sebep: yönetici yetkisi yetersiz veya Windows Güvenlik Duvarı engellemiş olabilir.", false);
                return;
            }

            if (!globalOk)
                OnLog?.Invoke("  [~] Global IP yönlendirme açılamadı (registry). Yalnızca arayüz yönlendirmesi etkin.", false);
            if (!ifaceOk)
                OnLog?.Invoke("  [~] Arayüz IP yönlendirmesi açılamadı (netsh). Yalnızca global yönlendirme etkin.", false);

            OnLog?.Invoke("  [✓] IP yönlendirme aktif — hedefin interneti kesilmeyecek.", false);

            // MITM için ayrı CTS — SniffDns erken dönse de (cihaz bulunamadı, open failed vb.)
            // MITM'i kesinlikle durdurabilmek için
            using var mitmCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            var mitmTask = _arp.StartMitm(targets, gatewayMAC, mitmCts.Token);
            try
            {
                SniffDns(targets, ct);
            }
            finally
            {
                mitmCts.Cancel(); // SniffDns erken return ettiyse MITM'i burada durdur
                try { mitmTask.Wait(TimeSpan.FromSeconds(5)); } catch { }

                if (sessionId == Volatile.Read(ref _sessionId))
                {
                    SetIpForwarding(false);
                    NetworkHelper.SetIPForwarding(false);
                    OnLog?.Invoke("  [✓] IP yönlendirme kapatıldı.", false);
                }
            }
        }

        // ----------------------------------------------------------------
        // DNS paket yakalayıcı
        // ----------------------------------------------------------------
        private void SniffDns(List<DeviceInfo> targets, CancellationToken ct)
        {
            LibPcapLiveDevice? dev = null;
            try
            {
                var allDevs = LibPcapLiveDeviceList.New().OfType<LibPcapLiveDevice>().ToList();

                if (allDevs.Count == 0)
                {
                    OnLog?.Invoke("  [!] Sistemde hiç ağ arayüzü bulunamadı. Npcap kurulu ve çalışıyor mu?", true);
                    return;
                }

                // 1) ipAddress property ile eşleştir (WiFi/Ethernet çoğu durumda)
                dev = allDevs.FirstOrDefault(d => d.Addresses.Any(a =>
                    a.Addr?.ipAddress?.ToString() == _arp.LocalIP));

                // 2) Addr.ToString() ile dene (bazı bağlantı tiplerinde ipAddress null olur)
                dev ??= allDevs.FirstOrDefault(d => d.Addresses.Any(a =>
                    a.Addr?.ToString() == _arp.LocalIP));

                // 3) ARP ile kullanılan cihazın adıyla eşleştir (hotspot, VPN vb.)
                if (dev == null && _arp.Device != null)
                {
                    var arpName = _arp.Device.Name;
                    dev = allDevs.FirstOrDefault(d => d.Name == arpName);
                }
            }
            catch (Exception ex)
            {
                OnLog?.Invoke($"  [!] Ağ arayüzü listelenirken hata: {ex.Message}", true);
                return;
            }

            if (dev == null)
            {
                OnLog?.Invoke($"  [!] DNS izleme için ağ arayüzü bulunamadı (yerel IP: {_arp.LocalIP}).", true);
                OnLog?.Invoke("      Npcap sürücüsünü yeniden yüklemek sorunu çözebilir.", false);
                return;
            }

            OnLog?.Invoke($"  [✓] DNS yakalama arayüzü: {dev.Interface.FriendlyName ?? dev.Name}", false);

            var ipMap = targets.ToDictionary(t => t.IP, t => t);

            PacketArrivalEventHandler? handler = null;
            handler = (_, e) =>
            {
                try
                {
                    var raw = Packet.ParsePacket(e.GetPacket().LinkLayerType, e.GetPacket().Data);
                    if (raw?.PayloadPacket is not IPPacket ip) return;
                    if (!ipMap.TryGetValue(ip.SourceAddress.ToString(), out var target)) return;
                    if (ip.PayloadPacket is not UdpPacket udp) return;
                    if (udp.DestinationPort != 53) return;

                    var domain = ParseDnsQuery(udp.PayloadData);
                    if (string.IsNullOrEmpty(domain)) return;

                    OnUpdate?.Invoke(new DnsEntry(
                        DateTime.Now.ToString("HH:mm:ss"),
                        _store.ResolveName(target.MAC),
                        target.IP,
                        target.MAC,
                        domain));
                }
                catch { }
            };

            try
            {
                dev.Open(DeviceModes.Promiscuous, 1);
            }
            catch (Exception ex)
            {
                OnLog?.Invoke($"  [!] Ağ arayüzü açılamadı: {ex.Message}", true);
                OnLog?.Invoke("      Başka bir uygulama arayüzü kullanıyor olabilir veya Npcap servisi çalışmıyor.", false);
                return;
            }

            try
            {
                var filter = targets.Count == 1
                    ? $"udp dst port 53 and src host {targets[0].IP}"
                    : $"udp dst port 53 and ({string.Join(" or ", targets.Select(t => $"src host {t.IP}"))})";

                try
                {
                    dev.Filter = filter;
                }
                catch (Exception ex)
                {
                    OnLog?.Invoke($"  [!] Paket filtresi ayarlanamadı: {ex.Message}", true);
                    return;
                }

                dev.OnPacketArrival += handler;
                dev.StartCapture();

                OnLog?.Invoke("  [◉] DNS paketi bekleniyor…", false);
                ct.WaitHandle.WaitOne();
            }
            catch (Exception ex)
            {
                OnLog?.Invoke($"  [!] DNS yakalama sırasında hata: {ex.Message}", true);
            }
            finally
            {
                dev.OnPacketArrival -= handler;
                try { dev.StopCapture(); } catch { }
                try { dev.Close();       } catch { }
            }
        }

        // ----------------------------------------------------------------
        // Windows IP Forwarding — interface bazlı (netsh), bool döner
        // ----------------------------------------------------------------
        private bool SetIpForwarding(bool enable)
        {
            var ifName = _arp.Device?.Interface.FriendlyName;
            if (string.IsNullOrEmpty(ifName))
            {
                OnLog?.Invoke("  [!] Arayüz adı alınamadı, netsh IP yönlendirmesi ayarlanamıyor.", true);
                return false;
            }
            try
            {
                var value = enable ? "enabled" : "disabled";
                var psi = new ProcessStartInfo("netsh",
                    $"interface ipv4 set interface \"{ifName}\" forwarding={value}")
                {
                    CreateNoWindow         = true,
                    UseShellExecute        = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError  = true
                };
                using var p = Process.Start(psi);
                p?.WaitForExit(3000);
                return p?.ExitCode == 0;
            }
            catch (Exception ex)
            {
                OnLog?.Invoke($"  [!] netsh IP yönlendirme hatası: {ex.Message}", true);
                return false;
            }
        }

        // ----------------------------------------------------------------
        // DNS sorgu paketinden domain adını çıkar (RFC 1035)
        // ----------------------------------------------------------------
        private static string? ParseDnsQuery(byte[] payload)
        {
            try
            {
                if (payload == null || payload.Length < 13) return null;
                if ((payload[2] & 0x80) != 0) return null;

                int qdcount = (payload[4] << 8) | payload[5];
                if (qdcount == 0) return null;

                int pos = 12;
                var labels = new List<string>();

                while (pos < payload.Length && payload[pos] != 0)
                {
                    int len = payload[pos++];
                    if (len > 63 || pos + len > payload.Length) return null;
                    labels.Add(System.Text.Encoding.ASCII.GetString(payload, pos, len));
                    pos += len;
                }

                if (labels.Count == 0) return null;
                return string.Join(".", labels);
            }
            catch { return null; }
        }

        // ----------------------------------------------------------------
        // Durdur
        // ----------------------------------------------------------------
        public void Stop()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        public void WaitForCleanup(TimeSpan timeout)
        {
            try { _runTask?.Wait(timeout); } catch { }
        }

        public void Dispose()
        {
            _cts?.Cancel();
            try { _runTask?.Wait(TimeSpan.FromSeconds(5)); } catch { }
            _cts?.Dispose();
            _cts = null;
        }
    }
}
