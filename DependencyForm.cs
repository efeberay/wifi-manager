using System.Diagnostics;

namespace WifiManager
{
    public partial class DependencyForm : Form
    {
        private const string DownloadUrl = "https://npcap.com/dist/npcap-1.80.exe";
        private const string WebsiteUrl = "https://npcap.com/#download";

        public DependencyForm()
        {
            InitializeComponent();
            ApplyTexts();
        }

        private void ApplyTexts()
        {
            lblTitle.Text  = "Gereksinim Eksik";
            lblStatus.Text = "Sisteminizde Npcap sürücüsü bulunamadı.";
            lblWhy.Text =
                "Neden gerekli?\r\n" +
                "WifiManager ağdaki cihazları taramak için ARP paketi gönderip alır; " +
                "DNS izleme özelliği ise ağ trafiğini gerçek zamanlı dinler. " +
                "Bu işlemler için Windows'ta ham paket erişimi gereklidir. " +
                "Npcap bu erişimi sağlayan sürücüdür — kurulu olmadan uygulama çalışamaz.";
            lblSteps.Text =
                "1. 'Npcap İndir' butonuna tıklayın." + Environment.NewLine +
                "2. Kurulumu tamamlayın." + Environment.NewLine +
                "3. Gerekirse uygulamayı kapatıp yeniden açın." + Environment.NewLine +
                "4. Programı yönetici olarak çalıştırın.";
            btnDownload.Text = "Npcap İndir";
            btnWebsite.Text  = "Resmî Site";
            btnRetry.Text    = "Tekrar Dene";
            btnClose.Text    = "Kapat";
            Text = "Kurulum Gereksinimi";
        }

        private void BtnDownload_Click(object sender, EventArgs e)
        {
            OpenUrl(DownloadUrl, "İndirme başlatılamadı");
        }

        private void BtnWebsite_Click(object sender, EventArgs e)
        {
            OpenUrl(WebsiteUrl, "Resmî site açılamadı");
        }

        private void BtnRetry_Click(object sender, EventArgs e)
        {
            if (!Program.IsNpcapInstalled())
            {
                lblStatus.Text =
                    "Npcap hâlâ algılanamadı." + Environment.NewLine +
                    "Kurulumu tamamladıktan sonra bu ekrandan tekrar deneyin.";
                return;
            }

            MessageBox.Show(
                "Npcap algılandı. Uygulama şimdi ana ekrana geçecek.",
                "Hazır",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            Hide();
            using var mainForm = new MainForm();
            mainForm.ShowDialog();
            Close();
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void OpenUrl(string url, string errorTitle)
        {
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                lblStatus.Text =
                    "İndirme sayfası açıldı." + Environment.NewLine +
                    "Kurulumdan sonra 'Tekrar Dene' butonuna basabilirsiniz.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"{errorTitle}: {ex.Message}",
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
