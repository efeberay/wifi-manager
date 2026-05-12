using System.Diagnostics;
using System.Security.Principal;
using System.Windows.Forms;
using Microsoft.Win32;

namespace WifiManager;

internal static partial class Program
{
    [STAThread]
    static void Main()
    {
        if (!IsAdmin())
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = Environment.ProcessPath,
                    UseShellExecute = true,
                    Verb = "runas"
                };
                Process.Start(psi);
            }
            catch
            {
                MessageBox.Show(
                    "Yönetici yetkisi gerekli!\n\nSağ tık -> 'Yönetici olarak çalıştır' seçin.",
                    "Yetki Gerekli",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            return;
        }

        // Npcap kontrolünü yalnızca LoadLibrary ile yapmak güvenilir değil;
        // Npcap kurulu olsa bile DLL, arama yolunda değilse false dönebilir.
        if (!IsNpcapInstalled())
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new DependencyForm());
            return;
        }

        // Önceki oturumun bıraktığı kalıntıları temizle
        // (Eski DNS izleme IP forwarding açık bırakmış olabilir)
        try { WifiManager.Core.NetworkHelper.ResetNetworkState(); } catch { }

        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm());
    }

    internal static bool IsNpcapInstalled()
    {
        try
        {
            if (IsNpcapRegistered())
                return true;

            return HasNpcapFiles();
        }
        catch
        {
            return false;
        }
    }

    static bool IsNpcapRegistered()
    {
        using RegistryKey? npcapKey =
            Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Npcap") ??
            Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Npcap");

        if (npcapKey is null)
            return false;

        string? installPath = npcapKey.GetValue("InstallPath") as string;
        if (!string.IsNullOrWhiteSpace(installPath) && Directory.Exists(installPath))
            return true;

        string? version = npcapKey.GetValue("Version") as string;
        return !string.IsNullOrWhiteSpace(version);
    }

    static bool HasNpcapFiles()
    {
        string windowsDir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);

        string[] candidatePaths =
        [
            Path.Combine(windowsDir, "System32", "Npcap", "wpcap.dll"),
            Path.Combine(windowsDir, "SysWOW64", "Npcap", "wpcap.dll"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Npcap", "wpcap.dll"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Npcap", "wpcap.dll")
        ];

        return candidatePaths.Any(File.Exists);
    }

    static bool IsAdmin()
    {
        using var id = WindowsIdentity.GetCurrent();
        return new WindowsPrincipal(id).IsInRole(WindowsBuiltInRole.Administrator);
    }
}
