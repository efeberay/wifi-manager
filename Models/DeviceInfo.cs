namespace WifiManager.Models
{
    public class DeviceInfo
    {
        public string IP       { get; set; } = "";
        public string MAC      { get; set; } = "";
        public string Hostname { get; set; } = "Unknown";
        public string Label    { get; set; } = "";

        public override string ToString() => $"{Label} ({IP})";
    }
}
