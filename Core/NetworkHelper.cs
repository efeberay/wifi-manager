using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using SharpPcap;
using SharpPcap.LibPcap;

namespace WifiManager.Core
{
    /// <summary>
    /// Yerel IP, MAC, gateway ve en uygun ağ arayüzünü tespit eder.
    /// </summary>
    public static class NetworkHelper
    {
        // ----------------------------------------------------------------
        // Yerel IP
        // ----------------------------------------------------------------
        public static string GetLocalIP()
        {
            // Socket trick Windows routing table + metric'leri otomatik uygular
            string routedIP = "";
            try
            {
                using var s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                s.Connect("8.8.8.8", 80);
                routedIP = ((IPEndPoint)s.LocalEndPoint!).Address.ToString();
            }
            catch { }

            // Routing table'ın seçtiği IP fiziksel adaptöre aitse doğrudan kullan
            if (!string.IsNullOrEmpty(routedIP) && IsPhysicalAdapterIP(routedIP))
                return routedIP;

            // VPN/sanal adaptör seçildiyse: fiziksel, gateway'i olan adaptörü tercih et
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != OperationalStatus.Up) continue;
                if (ni.NetworkInterfaceType is NetworkInterfaceType.Loopback
                                            or NetworkInterfaceType.Tunnel
                                            or NetworkInterfaceType.Ppp) continue;
                if (IsVirtualAdapter(ni)) continue;

                var props = ni.GetIPProperties();
                if (!props.GatewayAddresses.Any(g =>
                        g.Address.AddressFamily == AddressFamily.InterNetwork &&
                        !g.Address.Equals(IPAddress.Any))) continue;

                var ua = props.UnicastAddresses.FirstOrDefault(a =>
                    a.Address.AddressFamily == AddressFamily.InterNetwork &&
                    !IPAddress.IsLoopback(a.Address));
                if (ua != null) return ua.Address.ToString();
            }

            // Son çare: routing table sonucu (VPN olsa bile)
            return !string.IsNullOrEmpty(routedIP) ? routedIP :
                   Dns.GetHostEntry(Dns.GetHostName()).AddressList
                      .FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork)
                      ?.ToString() ?? "127.0.0.1";
        }

        private static bool IsPhysicalAdapterIP(string ip)
        {
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != OperationalStatus.Up) continue;
                if (IsVirtualAdapter(ni)) continue;
                if (ni.GetIPProperties().UnicastAddresses.Any(ua => ua.Address.ToString() == ip))
                    return true;
            }
            return false;
        }

        private static bool IsVirtualAdapter(NetworkInterface ni)
        {
            var desc = ni.Description ?? "";
            return desc.Contains("VPN",       StringComparison.OrdinalIgnoreCase) ||
                   desc.Contains("Virtual",   StringComparison.OrdinalIgnoreCase) ||
                   desc.Contains("TAP",       StringComparison.OrdinalIgnoreCase) ||
                   desc.Contains("Tunnel",    StringComparison.OrdinalIgnoreCase) ||
                   desc.Contains("Pseudo",    StringComparison.OrdinalIgnoreCase) ||
                   desc.Contains("Hyper-V",   StringComparison.OrdinalIgnoreCase) ||
                   desc.Contains("vEthernet", StringComparison.OrdinalIgnoreCase) ||
                   ni.NetworkInterfaceType is NetworkInterfaceType.Tunnel or NetworkInterfaceType.Ppp;
        }

        // ----------------------------------------------------------------
        // Yerel MAC (PhysicalAddress)
        // ----------------------------------------------------------------
        public static string GetLocalMAC(string localIP)
        {
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != OperationalStatus.Up) continue;
                foreach (var ua in ni.GetIPProperties().UnicastAddresses)
                {
                    if (ua.Address.ToString() == localIP)
                    {
                        var mac = ni.GetPhysicalAddress().GetAddressBytes();
                        if (mac.Length == 6)
                            return string.Join(":", mac.Select(b => b.ToString("x2")));
                    }
                }
            }
            return "";
        }

        // ----------------------------------------------------------------
        // En uygun LibPcapLiveDevice
        // ----------------------------------------------------------------
        public static LibPcapLiveDevice? FindBestDevice(string localIP)
        {
            var devices = LibPcapLiveDeviceList.Instance;

            // 1) ipAddress property ile eşleştir (en güvenilir)
            foreach (var d in devices)
                if (d.Addresses.Any(a => a.Addr?.ipAddress?.ToString() == localIP))
                    return d;

            // 2) Addr.ToString() ile dene
            foreach (var d in devices)
                if (d.Addresses.Any(a => a.Addr?.ToString() == localIP))
                    return d;

            // IP eşleşmesi yoksa null — yanlış arayüzden paket göndermemek için
            return null;
        }

        // ----------------------------------------------------------------
        // Gerçek subnet maskesine göre taranacak host listesi
        // ----------------------------------------------------------------
        public static List<string> GetSubnetHosts(string localIP)
        {
            IPAddress? mask = null;
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != OperationalStatus.Up) continue;
                foreach (var ua in ni.GetIPProperties().UnicastAddresses)
                {
                    if (ua.Address.ToString() == localIP && ua.IPv4Mask != null
                            && !ua.IPv4Mask.Equals(IPAddress.Any))
                    {
                        mask = ua.IPv4Mask;
                        break;
                    }
                }
                if (mask != null) break;
            }
            mask ??= IPAddress.Parse("255.255.255.0"); // fallback /24

            var ipBytes   = IPAddress.Parse(localIP).GetAddressBytes();
            var maskBytes = mask.GetAddressBytes();
            var netBytes  = new byte[4];
            var bcastBytes = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                netBytes[i]   = (byte)(ipBytes[i]  &  maskBytes[i]);
                bcastBytes[i] = (byte)(netBytes[i]  | (~maskBytes[i] & 0xFF));
            }

            var ipBytesArr = IPAddress.Parse(localIP).GetAddressBytes();
            uint net      = (uint)(netBytes[0]    << 24 | netBytes[1]    << 16 | netBytes[2]    << 8 | netBytes[3]);
            uint bcast    = (uint)(bcastBytes[0]  << 24 | bcastBytes[1]  << 16 | bcastBytes[2]  << 8 | bcastBytes[3]);
            uint localInt = (uint)(ipBytesArr[0]  << 24 | ipBytesArr[1]  << 16 | ipBytesArr[2]  << 8 | ipBytesArr[3]);

            uint totalHosts = bcast - net - 1;
            const uint maxHosts = 1022;

            uint startOffset, endOffset;
            if (totalHosts <= maxHosts)
            {
                startOffset = 1;
                endOffset   = totalHosts;
            }
            else
            {
                // Local IP'nin etrafında pencereyi ortala — büyük subnetlerde hiç taranmayan
                // bölge oluşmasın
                uint localOffset = localInt - net;
                uint half        = maxHosts / 2;
                startOffset = localOffset > half ? localOffset - half : 1;
                endOffset   = startOffset + maxHosts - 1;
                if (endOffset > totalHosts)
                {
                    endOffset   = totalHosts;
                    startOffset = Math.Max(1, endOffset - maxHosts + 1);
                }
            }

            var hosts = new List<string>((int)(endOffset - startOffset + 1));
            for (uint h = startOffset; h <= endOffset; h++)
            {
                uint addr = net + h;
                hosts.Add($"{(addr >> 24) & 0xFF}.{(addr >> 16) & 0xFF}.{(addr >> 8) & 0xFF}.{addr & 0xFF}");
            }
            return hosts;
        }

        // ----------------------------------------------------------------
        // Host adını çözümle (hata varsa "Unknown" döner)
        // ----------------------------------------------------------------
        public static string ResolveHostname(string ip)
        {
            try
            {
                return Dns.GetHostEntry(ip).HostName;
            }
            catch
            {
                return "Unknown";
            }
        }

        // ----------------------------------------------------------------
        // Windows IP Forwarding (registry)
        // ----------------------------------------------------------------
        public static bool SetIPForwarding(bool enable)
        {
            try
            {
                var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                    @"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters", writable: true);
                if (key == null) return false;
                key.SetValue("IPEnableRouter", enable ? 1 : 0,
                              Microsoft.Win32.RegistryValueKind.DWord);
                return true;
            }
            catch { return false; }
        }

        // ----------------------------------------------------------------
        // Gerçek default gateway (routing table'dan)
        // ----------------------------------------------------------------
        public static string GetDefaultGateway(string localIP)
        {
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != OperationalStatus.Up) continue;
                var props = ni.GetIPProperties();
                bool hasLocalIP = props.UnicastAddresses
                    .Any(ua => ua.Address.ToString() == localIP);
                if (!hasLocalIP) continue;
                var gw = props.GatewayAddresses
                    .Select(g => g.Address)
                    .FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
                if (gw != null) return gw.ToString();
            }
            return "";
        }

        // ----------------------------------------------------------------
        // Başlangıç temizliği — önceki oturumun bıraktığı kalıntıları temizle
        // ----------------------------------------------------------------
        public static void ResetNetworkState()
        {
            // Eski DNS İzleme oturumu IP forwarding'i açık bırakmış olabilir.
            // Bu Windows ağ yığının davranışını değiştirir ve ARP taramayı bozar.
            SetIPForwarding(false);
        }
    }
}
