using Microsoft.Data.SqlClient;

namespace WifiManager.Core
{
    public class DeviceStore
    {
        private const string ConnStr =
            @"Server=localhost\SQLEXPRESS;Database=WifiManager;Integrated Security=true;TrustServerCertificate=true;";

        private readonly object _lock = new();

        // Bellek cache — her DB sorgusunda yeniden bağlanmamak için
        private readonly Dictionary<string, string> _names   = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> _autoIds = new(StringComparer.OrdinalIgnoreCase);
        private int _autoCounter = 0;

        public DeviceStore()
        {
            EnsureTable();
            Load();
        }

        // ----------------------------------------------------------------
        // Tablo yoksa oluştur
        // ----------------------------------------------------------------
        private void EnsureTable()
        {
            try
            {
                using var con = new SqlConnection(ConnStr);
                con.Open();
                using var cmd = con.CreateCommand();
                cmd.CommandText = @"
                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Devices')
                        CREATE TABLE Devices (
                            MAC  NVARCHAR(17)  NOT NULL PRIMARY KEY,
                            Name NVARCHAR(100) NOT NULL
                        );";
                cmd.ExecuteNonQuery();
            }
            catch { }
        }

        // ----------------------------------------------------------------
        // DB'den cache'e yükle
        // ----------------------------------------------------------------
        private void Load()
        {
            try
            {
                using var con = new SqlConnection(ConnStr);
                con.Open();
                using var cmd = new SqlCommand("SELECT MAC, Name FROM Devices", con);
                using var rdr = cmd.ExecuteReader();
                lock (_lock)
                {
                    while (rdr.Read())
                        _names[rdr.GetString(0)] = rdr.GetString(1);
                }
            }
            catch { }
        }

        // ----------------------------------------------------------------
        // İsmi çözümle
        // ----------------------------------------------------------------
        public string ResolveName(string mac)
        {
            mac = mac.ToLower().Trim();
            lock (_lock)
            {
                if (_names.TryGetValue(mac, out var custom)) return custom;
                if (_autoIds.TryGetValue(mac, out var auto)) return auto;
                _autoCounter++;
                var name = $"Cihaz-{_autoCounter}";
                _autoIds[mac] = name;
                return name;
            }
        }

        // ----------------------------------------------------------------
        // İsim ata — cache + DB
        // ----------------------------------------------------------------
        public void SetName(string mac, string name)
        {
            mac = mac.ToLower().Trim();
            lock (_lock) { _names[mac] = name; }

            try
            {
                using var con = new SqlConnection(ConnStr);
                con.Open();
                using var cmd = con.CreateCommand();
                cmd.CommandText = @"
                    MERGE Devices AS target
                    USING (VALUES (@mac, @name)) AS src (MAC, Name)
                    ON target.MAC = src.MAC
                    WHEN MATCHED     THEN UPDATE SET Name = src.Name
                    WHEN NOT MATCHED THEN INSERT (MAC, Name) VALUES (src.MAC, src.Name);";
                cmd.Parameters.AddWithValue("@mac",  mac);
                cmd.Parameters.AddWithValue("@name", name);
                cmd.ExecuteNonQuery();
            }
            catch { }
        }

        public string? GetCustomName(string mac)
        {
            lock (_lock)
            {
                return _names.TryGetValue(mac.ToLower().Trim(), out var v) ? v : null;
            }
        }
    }
}
