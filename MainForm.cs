using System.Drawing.Drawing2D;
using System.Security.Principal;
using WifiManager.Core;
using WifiManager.Models;

namespace WifiManager
{
    public partial class MainForm : Form
    {
        // ── Backend ──────────────────────────────────────────────────────
        private readonly ArpTool     _arp;
        private readonly DeviceStore _store;
        private readonly DnsMonitor  _dns;
        private CancellationTokenSource? _scanCts, _blockCts;
        private Task? _blockTask;
        private DeviceInfo? _sel;
        private string?     _gwMAC;
        private enum St { Idle, Scanning, Blocking, Monitoring }
        private St _state = St.Idle;

        // ── Renk sabitleri (Paint handler için) ──────────────────────────
        static readonly Color BG     = Color.FromArgb(10, 10, 10);
        static readonly Color BLUE   = Color.FromArgb(0, 112, 243);
        static readonly Color GREEN  = Color.FromArgb(0, 219, 131);
        static readonly Color RED    = Color.FromArgb(243, 83, 96);
        static readonly Color YELLOW = Color.FromArgb(245, 166, 35);
        static readonly Color PURPLE = Color.FromArgb(138, 74, 243);
        static readonly Color MUTED  = Color.FromArgb(130, 130, 130);
        static readonly Color TEXT   = Color.FromArgb(215, 215, 215);
        static readonly Color BTN_TEXT_ENABLED  = Color.FromArgb(242, 244, 250);
        static readonly Color BTN_TEXT_DISABLED = Color.FromArgb(238, 240, 245);

        // ================================================================
        public MainForm()
        {
            _store   = new DeviceStore();
            _arp = new ArpTool(_store);
            _dns = new DnsMonitor(_arp, _store);
            _dns.OnUpdate += e => AppendLog(
                $"  [{e.Time}]  {e.Label,-13} ({e.IP})  →  {e.Domain}", GREEN);
            _dns.OnLog += (msg, isErr) =>
            {
                AppendLog(msg, isErr ? RED : YELLOW);
                if (isErr) SetSt(St.Idle);
            };
            InitializeComponent();
            ApplyNavigationButtonStyle();
            log.Text = "  — Uygulama hazır. Ağı taramak için ⟳ Ağı Tara butonuna basın. —\n\n";
            RefreshInfoLabels();
        }

        // ================================================================
        // BUTON OLAYLARI
        // ================================================================
        async void BtnScan(object? s, EventArgs e)
        {
            if (_state == St.Scanning) return;
            _scanCts = new CancellationTokenSource();
            SetSt(St.Scanning);
            grid.Rows.Clear(); _sel = null;
            lblCount.Visible = false; lblCount.Text = "";
            Status("Ağ taranıyor…", BLUE);
            AppendLog("\n  ── Tarama başladı ──\n", BLUE);

            int idx = 0;
            var thisCts = _scanCts; // iptal kontrolü için capture

            Action<DeviceInfo> addDevice = dev =>
            {
                if (!grid.IsHandleCreated) return;
                grid.BeginInvoke(() =>
                {
                    // İptal edildiyse kuyruktaki bekleyen callback'leri yok say
                    if (thisCts.IsCancellationRequested) return;

                    // Aynı MAC zaten grid'de varsa ekleme (çift keşif koruması)
                    if (grid.Rows.Cast<DataGridViewRow>()
                              .Any(r => r.Tag is DeviceInfo d && d.MAC == dev.MAC))
                        return;

                    idx++;
                    var rowIdx = grid.Rows.Add(idx, dev.Label, dev.IP, dev.MAC, dev.Hostname);
                    var row    = grid.Rows[rowIdx];
                    row.Tag    = dev;
                    grid.FirstDisplayedScrollingRowIndex = rowIdx;

                    log.SelectionStart  = log.TextLength;
                    log.SelectionLength = 0;
                    log.SelectionColor  = GREEN;
                    log.AppendText($"  [+]  {dev.Label,-14}  {dev.IP,-16}  {dev.MAC}\n");
                    log.ScrollToCaret();

                    lblCount.Text    = $"  {idx} cihaz  ";
                    lblCount.Visible = true;
                });
            };

            try
            {
                await _arp.ScanAsync(addDevice, _scanCts.Token);

                _gwMAC = _arp.GetGatewayMAC();

                // Geç gelen cihazlar için: 1 sn bekle, ARP cache'i tekrar oku
                await Task.Run(() => { Thread.Sleep(1000); _arp.SupplementScan(addDevice); });

                // Tüm BeginInvoke'ların bitmesini bekle
                var tcs = new TaskCompletionSource();
                grid.BeginInvoke(() => tcs.SetResult());
                await tcs.Task;

                int count = grid.Rows.Count;
                AppendLog($"\n  ── Tarama tamamlandı: {count} cihaz bulundu ──\n", BLUE);
            }
            catch (OperationCanceledException)
            {
                // Kuyruktaki BeginInvoke'ların bitmesini bekle, sonra doğru sayıyı oku
                var tcs = new TaskCompletionSource();
                grid.BeginInvoke(() => tcs.SetResult());
                await tcs.Task;

                int count = grid.Rows.Count;
                AppendLog($"\n  ── Tarama durduruldu: {count} cihaz bulundu ──\n", MUTED);
            }
            catch (Exception ex)
            {
                AppendLog($"\n  ── Tarama hatası: {ex.Message} ──\n", RED);
            }
            finally                            { SetSt(St.Idle); }
        }

        void BtnBlock(object? s, EventArgs e)
        {
            if (_state != St.Idle) return;
            if (_sel == null || !ChkGW()) return;
            _blockCts = new CancellationTokenSource();
            SetSt(St.Blocking);
            var t = _sel; var gw = _gwMAC!; int n = 0;
            Status($"⦸  Engelleniyor: {t.Label} ({t.IP})", RED);
            AppendLog($"\n  [⦸] Engelleme başladı → {t.Label} ({t.IP})\n", RED);
            _blockTask = _arp.StartBlocking(t.IP, t.MAC, gw, _blockCts.Token, c =>
            {
                n = c;
                if (grid.IsHandleCreated)
                    grid.BeginInvoke(() => Status($"⦸  {t.Label} — Paket #{n}", RED));
            });
        }

        void BtnDns1(object? s, EventArgs e)
        {
            if (_state != St.Idle) return;
            if (_sel == null || !ChkGW()) return;
            if (!IsAdmin()) { Warn("DNS izleme için uygulamanın yönetici olarak çalışması gerekiyor.\nSağ tık → \"Yönetici olarak çalıştır\" ile açın."); return; }
            SetSt(St.Monitoring);
            AppendLog($"\n  [◉] DNS izleme başladı → {_sel.Label} ({_sel.IP})\n", GREEN);
            _dns.StartSingle(_sel, _gwMAC!);
        }

        void BtnDnsAll(object? s, EventArgs e)
        {
            if (_state != St.Idle) return;
            if (!ChkGW()) return;
            if (!IsAdmin()) { Warn("DNS izleme için uygulamanın yönetici olarak çalışması gerekiyor.\nSağ tık → \"Yönetici olarak çalıştır\" ile açın."); return; }
            var tgts = _arp.Devices.Where(d => d.IP != _arp.LocalIP && d.IP != _arp.GatewayIP).ToList();
            if (tgts.Count == 0) { Warn("İzlenecek cihaz bulunamadı."); return; }
            SetSt(St.Monitoring);
            AppendLog($"\n  [◉] DNS izleme başladı ({tgts.Count} cihaz)\n", YELLOW);
            _dns.StartAll(tgts, _gwMAC!);
        }

        async void BtnStop(object? s, EventArgs e)
        {
            if (_state == St.Idle) return;
            SetButtonVisualState(bStop, false); // çift tıklamayı önle

            _scanCts?.Cancel();
            _blockCts?.Cancel();
            _dns.Stop();

            if (_state != St.Scanning)
            {
                // Worker cleanup'larını (RestoreARP, device.Close) bekle
                var blockTask = _blockTask;
                await Task.Run(() =>
                {
                    try { blockTask?.Wait(TimeSpan.FromSeconds(5)); } catch { }
                    _dns.WaitForCleanup(TimeSpan.FromSeconds(5));
                });
                SetSt(St.Idle);
                AppendLog("  [■] Durduruldu.\n", MUTED);
            }
        }

        void BtnRen(object? s, EventArgs e)
        {
            if (_state == St.Scanning) return;
            if (_sel == null) return;
            string prompt = $"'{_sel.Label}' için yeni isim belirle:\n(Not: Telefonların 'Rastgele/Gizli MAC' özelliği açıksa IP ve isim sıfırlanabilir.)";
            var v = InputDlg(prompt, "İsim Ver", _sel.Label);
            if (string.IsNullOrWhiteSpace(v)) return;
            _arp.RenameDevice(_sel, v.Trim());
            foreach (DataGridViewRow r in grid.Rows)
                if (r.Tag is DeviceInfo d && d.MAC == _sel.MAC)
                    r.Cells[1].Value = v.Trim();
            AppendLog($"  [✎] {_sel.MAC}  →  '{v.Trim()}' kaydedildi.", PURPLE);
        }

        // ================================================================
        // DURUM
        // ================================================================
        void SetSt(St st)
        {
            _state = st;
            if (!bScan.IsHandleCreated) return;
            bScan.BeginInvoke(() =>
            {
                switch (st)
                {
                    case St.Idle:
                        bScan.Text = "⟳  Ağı Tara";
                        prog.Visible = false;
                        RefreshButtons();
                        break;
                    case St.Scanning:
                        SetButtonVisualState(bScan, false);
                        SetButtonVisualState(bBlock, false);
                        SetButtonVisualState(bDns1, false);
                        SetButtonVisualState(bDnsA, false);
                        SetButtonVisualState(bStop, true);
                        SetButtonVisualState(bRen, false);
                        prog.Visible = true;
                        break;
                    case St.Blocking:
                    case St.Monitoring:
                        SetButtonVisualState(bScan, false);
                        SetButtonVisualState(bBlock, false);
                        SetButtonVisualState(bDns1, false);
                        SetButtonVisualState(bDnsA, false);
                        SetButtonVisualState(bStop, true);
                        SetButtonVisualState(bRen, false);
                        prog.Visible = false;
                        break;
                }
            });
        }

        void RefreshButtons()
        {
            _sel = grid.SelectedRows.Count > 0 ? grid.SelectedRows[0].Tag as DeviceInfo : null;
            bool has = _sel != null, idle = _state == St.Idle;

            SetButtonVisualState(bScan, _state == St.Idle || _state == St.Scanning);
            SetButtonVisualState(bBlock, has && idle);
            SetButtonVisualState(bDns1, has && idle);
            SetButtonVisualState(bRen, has && idle);
            SetButtonVisualState(bDnsA, _arp.Devices.Count > 0 && idle);
            SetButtonVisualState(bStop, _state == St.Blocking || _state == St.Monitoring);
        }

        // Status artık log'a yazıyor (status bar yok)
        void Status(string msg, Color c) => AppendLog($"  [{msg}]", c);

        void AppendLog(string msg, Color? c = null)
        {
            if (!log.IsHandleCreated) return;
            log.BeginInvoke(() =>
            {
                log.SelectionStart  = log.TextLength;
                log.SelectionLength = 0;
                log.SelectionColor  = c ?? TEXT;
                log.AppendText(msg + "\n");
                log.ScrollToCaret();
            });
        }

        void RefreshInfoLabels()
        {
            lblIP.Text = $"IP : {_arp.LocalIP}   MAC : {_arp.LocalMAC}";
            lblGW.Text = $"GW : {_arp.GatewayIP}";
            lblIf.Text = $"Arayüz : {_arp.Device?.Name ?? "?"}";
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!IsAdmin())
                AppendLog("  [!] Yönetici olarak çalışmıyor! DNS izleme çalışmayacak.\n      Uygulamayı sağ tık → \"Yönetici olarak çalıştır\" ile açın.", YELLOW);
        }

        bool ChkGW() { if (_gwMAC != null) return true; Warn("Önce ağı tarayın."); return false; }
        static bool IsAdmin() => new WindowsPrincipal(WindowsIdentity.GetCurrent())
            .IsInRole(WindowsBuiltInRole.Administrator);
        static void Warn(string m) => MessageBox.Show(m, "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        // ── Designer event handlers ───────────────────────────────────────
        private void ClrBtnClick(object? s, EventArgs e)
        {
            if (log.IsHandleCreated)
                log.BeginInvoke(() => { log.Clear(); log.AppendText("  — Günlük temizlendi —\n"); });
        }
        private void CtxRenameClick(object? s, EventArgs e) => BtnRen(s, e);
        private void CtxBlockClick(object? s, EventArgs e)  => BtnBlock(s, e);
        private void CtxDnsClick(object? s, EventArgs e)     => BtnDns1(s, e);
        private void CtxStopClick(object? s, EventArgs e)   => BtnStop(s, e);

        private void GridCellPainting(object? s, DataGridViewCellPaintingEventArgs e)
        {
            // Sadece header satırını (RowIndex == -1) özel çiz
            if (e.RowIndex != -1) return;

            if (e.Graphics == null) return;

            using (var brush = new SolidBrush(Color.FromArgb(22, 22, 32)))
            {
                e.Graphics.FillRectangle(brush, e.CellBounds);
            }

            // Alt ince çizgi (separator)
            using var sepPen = new Pen(Color.FromArgb(38, 38, 52), 1);
            e.Graphics.DrawLine(sepPen,
                e.CellBounds.Left, e.CellBounds.Bottom - 1,
                e.CellBounds.Right, e.CellBounds.Bottom - 1);

            // Sütun metni
            if (e.Value != null)
            {
                using var font = new Font("Segoe UI Semibold", 8.5f, FontStyle.Bold);
                var textBounds = Rectangle.Inflate(e.CellBounds, 0, 0);
                textBounds.X += e.ColumnIndex == 0 ? 6 : 12;
                textBounds.Width -= e.ColumnIndex == 0 ? 8 : 16;

                TextRenderer.DrawText(e.Graphics, e.Value.ToString(),
                    font, textBounds,
                    Color.FromArgb(130, 130, 155),
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis
                    | TextFormatFlags.NoPadding);
            }

            e.Handled = true;
        }

        private void GridSelectionChanged(object? s, EventArgs e) => RefreshButtons();
        private void GridCellDoubleClick(object? s, DataGridViewCellEventArgs e)
            { if (e.RowIndex >= 0) BtnRen(null, null!); }
        private void GridMouseDown(object? s, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var h = grid.HitTest(e.X, e.Y);
                if (h.RowIndex >= 0) grid.Rows[h.RowIndex].Selected = true;
            }
        }
        private void TopPanelLayout(object? s, LayoutEventArgs e)
            => infoFlow.Location = new Point(topPanel.Width - infoFlow.Width - 20, 10);
        private void TopPanelPaint(object? s, PaintEventArgs e)
        {
            // Header altında belirgin indigo/mavi accent çizgisi
            using var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                new System.Drawing.Rectangle(0, topPanel.Height - 2, topPanel.Width, 2),
                Color.FromArgb(80, 120, 255),
                Color.FromArgb(138, 74, 243),
                System.Drawing.Drawing2D.LinearGradientMode.Horizontal);
            e.Graphics.FillRectangle(brush, 0, topPanel.Height - 2, topPanel.Width, 2);
        }
        private void NavigationButtonResize(object? s, EventArgs e)
        {
            if (s is Button button)
                ApplyRoundedCorners(button, 16);
        }
        private void MainFormClosing(object? s, FormClosingEventArgs e)
        {
            _scanCts?.Cancel();
            _blockCts?.Cancel();
            _dns.Stop();
        }

        // ================================================================
        // YARDIMCILAR
        // ================================================================
        static string? InputDlg(string prompt, string title, string def = "")
        {
            var dlgBg   = Color.FromArgb(18, 18, 18);
            var dlgText = Color.FromArgb(210, 210, 210);
            var dlgMut  = Color.FromArgb(120, 120, 120);
            var fL      = new Font("Segoe UI", 9);
            var fS      = new Font("Segoe UI", 8);

            var f = new Form
            {
                Text             = title,
                Size             = new Size(420, 180),
                StartPosition    = FormStartPosition.CenterParent,
                BackColor        = dlgBg,
                ForeColor        = dlgText,
                FormBorderStyle  = FormBorderStyle.FixedDialog,
                MaximizeBox      = false,
                MinimizeBox      = false
            };
            var lbl = new Label
                { Text = prompt, Left = 12, Top = 14, Width = 390, Height = 46,
                  ForeColor = dlgMut, Font = fS };
            var txt = new TextBox
                { Left = 12, Top = 64, Width = 380, Text = def,
                  BackColor = Color.FromArgb(26, 26, 26), ForeColor = dlgText,
                  BorderStyle = BorderStyle.FixedSingle, Font = fL };
            var ok = DlgBtn("Tamam", DialogResult.OK, Color.FromArgb(40, 40, 40), 222, 100);
            var cn = DlgBtn("İptal", DialogResult.Cancel, Color.FromArgb(26, 26, 26), 314, 100);
            f.Controls.AddRange([lbl, txt, ok, cn]);
            f.AcceptButton = ok; f.CancelButton = cn;
            return f.ShowDialog() == DialogResult.OK ? txt.Text : null;
        }

        void ApplyNavigationButtonStyle()
        {
            foreach (Button button in new[] { bScan, bBlock, bDns1, bDnsA, bStop, bRen })
            {
                button.Enabled = true;
                button.FlatAppearance.BorderSize = 0;
                button.FlatAppearance.BorderColor = button.BackColor;
                button.TabStop = false;
                button.Resize -= NavigationButtonResize;
                button.Resize += NavigationButtonResize;
                ApplyRoundedCorners(button, 16);
            }

            RefreshButtons();
        }
        void SetButtonVisualState(Button button, bool isActive)
        {
            button.Enabled = true;
            button.ForeColor = isActive ? BTN_TEXT_ENABLED : BTN_TEXT_DISABLED;
            button.Cursor = isActive ? Cursors.Hand : Cursors.Default;
        }

        static void ApplyRoundedCorners(Control control, int radius)
        {
            if (control.Width <= 0 || control.Height <= 0)
                return;
            using var path = CreateRoundedPath(control.ClientRectangle, radius);
            control.Region = new Region(path);
        }

        static GraphicsPath CreateRoundedPath(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            var path = new GraphicsPath();

            path.StartFigure();
            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }

        static Button DlgBtn(string t, DialogResult dr, Color bg, int x, int y)
        {
            var b = new Button
            {
                Text        = t, DialogResult = dr, Left = x, Top = y,
                Width = 78, Height = 28,
                BackColor   = bg, ForeColor = Color.White,
                FlatStyle   = FlatStyle.Flat,
                Font        = new Font("Segoe UI", 8)
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }
    }
}




