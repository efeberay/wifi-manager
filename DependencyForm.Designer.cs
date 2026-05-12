namespace WifiManager
{
    partial class DependencyForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblWhy;
        private System.Windows.Forms.Label lblSteps;
        private System.Windows.Forms.Button btnDownload;
        private System.Windows.Forms.Button btnWebsite;
        private System.Windows.Forms.Button btnRetry;
        private System.Windows.Forms.Button btnClose;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            lblTitle = new System.Windows.Forms.Label();
            lblStatus = new System.Windows.Forms.Label();
            lblWhy = new System.Windows.Forms.Label();
            lblSteps = new System.Windows.Forms.Label();
            btnDownload = new System.Windows.Forms.Button();
            btnWebsite = new System.Windows.Forms.Button();
            btnRetry = new System.Windows.Forms.Button();
            btnClose = new System.Windows.Forms.Button();
            SuspendLayout();

            // lblTitle
            lblTitle.AutoSize = true;
            lblTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 14F, System.Drawing.FontStyle.Bold);
            lblTitle.ForeColor = System.Drawing.Color.FromArgb(0, 112, 243);
            lblTitle.Location = new System.Drawing.Point(20, 20);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new System.Drawing.Size(163, 25);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Gereksinim Eksik";

            // lblStatus
            lblStatus.Font = new System.Drawing.Font("Segoe UI", 10F);
            lblStatus.Location = new System.Drawing.Point(24, 58);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new System.Drawing.Size(430, 40);
            lblStatus.TabIndex = 1;
            lblStatus.Text = "Sisteminizde Npcap sürücüsü bulunamadı.";

            // lblWhy
            lblWhy.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblWhy.ForeColor = System.Drawing.Color.FromArgb(160, 160, 160);
            lblWhy.Location = new System.Drawing.Point(24, 100);
            lblWhy.Name = "lblWhy";
            lblWhy.Size = new System.Drawing.Size(430, 90);
            lblWhy.TabIndex = 7;
            lblWhy.Text =
                "Neden gerekli?\r\n" +
                "WifiManager ağdaki cihazları taramak için ARP paketi gönderip alır; " +
                "DNS izleme özelliği ise ağ trafiğini gerçek zamanlı dinler. " +
                "Bu işlemler için Windows'ta ham paket erişimi gereklidir. " +
                "Npcap bu erişimi sağlayan sürücüdür — kurulu olmadan uygulama çalışamaz.";

            // lblSteps
            lblSteps.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblSteps.ForeColor = System.Drawing.Color.FromArgb(210, 210, 210);
            lblSteps.Location = new System.Drawing.Point(24, 200);
            lblSteps.Name = "lblSteps";
            lblSteps.Size = new System.Drawing.Size(430, 88);
            lblSteps.TabIndex = 2;
            lblSteps.Text = "1. 'Npcap İndir' butonuna tıklayın.\r\n2. Kurulumu tamamlayın.\r\n3. Gerekirse uygulamayı kapatıp yeniden açın.\r\n4. Programı yönetici olarak çalıştırın.";

            // btnDownload
            btnDownload.BackColor = System.Drawing.Color.FromArgb(0, 112, 243);
            btnDownload.FlatAppearance.BorderSize = 0;
            btnDownload.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnDownload.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            btnDownload.ForeColor = System.Drawing.Color.White;
            btnDownload.Location = new System.Drawing.Point(24, 302);
            btnDownload.Name = "btnDownload";
            btnDownload.Size = new System.Drawing.Size(135, 36);
            btnDownload.TabIndex = 3;
            btnDownload.Text = "Npcap İndir";
            btnDownload.UseVisualStyleBackColor = false;
            btnDownload.Click += BtnDownload_Click;

            // btnWebsite
            btnWebsite.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            btnWebsite.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(60, 60, 60);
            btnWebsite.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnWebsite.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            btnWebsite.ForeColor = System.Drawing.Color.White;
            btnWebsite.Location = new System.Drawing.Point(171, 302);
            btnWebsite.Name = "btnWebsite";
            btnWebsite.Size = new System.Drawing.Size(135, 36);
            btnWebsite.TabIndex = 4;
            btnWebsite.Text = "Resmî Site";
            btnWebsite.UseVisualStyleBackColor = false;
            btnWebsite.Click += BtnWebsite_Click;

            // btnRetry
            btnRetry.BackColor = System.Drawing.Color.FromArgb(0, 140, 90);
            btnRetry.FlatAppearance.BorderSize = 0;
            btnRetry.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnRetry.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            btnRetry.ForeColor = System.Drawing.Color.White;
            btnRetry.Location = new System.Drawing.Point(318, 302);
            btnRetry.Name = "btnRetry";
            btnRetry.Size = new System.Drawing.Size(135, 36);
            btnRetry.TabIndex = 5;
            btnRetry.Text = "Tekrar Dene";
            btnRetry.UseVisualStyleBackColor = false;
            btnRetry.Click += BtnRetry_Click;

            // btnClose
            btnClose.BackColor = System.Drawing.Color.FromArgb(40, 40, 40);
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnClose.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            btnClose.ForeColor = System.Drawing.Color.White;
            btnClose.Location = new System.Drawing.Point(24, 350);
            btnClose.Name = "btnClose";
            btnClose.Size = new System.Drawing.Size(429, 34);
            btnClose.TabIndex = 6;
            btnClose.Text = "Kapat";
            btnClose.UseVisualStyleBackColor = false;
            btnClose.Click += BtnClose_Click;

            // DependencyForm
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.FromArgb(18, 18, 18);
            ClientSize = new System.Drawing.Size(480, 410);
            Controls.Add(btnClose);
            Controls.Add(btnRetry);
            Controls.Add(btnWebsite);
            Controls.Add(btnDownload);
            Controls.Add(lblSteps);
            Controls.Add(lblWhy);
            Controls.Add(lblStatus);
            Controls.Add(lblTitle);
            ForeColor = System.Drawing.Color.White;
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "DependencyForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Kurulum Gereksinimi";
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
