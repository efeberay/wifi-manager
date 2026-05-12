namespace WifiManager
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        // ── Header ────────────────────────────────────────────────────────
        private System.Windows.Forms.Panel topPanel;
        private System.Windows.Forms.Label lblAppName;
        private System.Windows.Forms.FlowLayoutPanel infoFlow;
        private System.Windows.Forms.Label lblIP;
        private System.Windows.Forms.Label lblGW;
        private System.Windows.Forms.Label lblIf;

        // ── Progress ──────────────────────────────────────────────────────
        private System.Windows.Forms.ProgressBar prog;

        // ── Body layout ───────────────────────────────────────────────────
        private System.Windows.Forms.TableLayoutPanel bodyTable;

        // ── Button panel (left column) ────────────────────────────────────
        private System.Windows.Forms.TableLayoutPanel btnPanel;
        private System.Windows.Forms.Button bScan;
        private System.Windows.Forms.Button bBlock;
        private System.Windows.Forms.Button bDns1;
        private System.Windows.Forms.Button bDnsA;
        private System.Windows.Forms.Button bStop;
        private System.Windows.Forms.Button bRen;

        // ── Content split (grid | log) ────────────────────────────────────
        private System.Windows.Forms.TableLayoutPanel contentTable;

        // ── Device list panel ─────────────────────────────────────────────
        private System.Windows.Forms.Panel gridPanel;
        private System.Windows.Forms.Panel gridHeader;
        private System.Windows.Forms.Label gridTitle;
        private System.Windows.Forms.Label lblCount;
        private System.Windows.Forms.DataGridView grid;
        private System.Windows.Forms.DataGridViewTextBoxColumn colIdx;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLabel;
        private System.Windows.Forms.DataGridViewTextBoxColumn colIP;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMAC;
        private System.Windows.Forms.DataGridViewTextBoxColumn colHost;

        // ── Console panel ─────────────────────────────────────────────────
        private System.Windows.Forms.Panel logPanel;
        private System.Windows.Forms.Panel logHeader;
        private System.Windows.Forms.Label logTitle;
        private System.Windows.Forms.Button clrBtn;
        private System.Windows.Forms.RichTextBox log;

        // ── Context menu ──────────────────────────────────────────────────
        private System.Windows.Forms.ContextMenuStrip ctxMenu;
        private System.Windows.Forms.ToolStripMenuItem ctxRename;
        private System.Windows.Forms.ToolStripMenuItem ctxBlock;
        private System.Windows.Forms.ToolStripMenuItem ctxDns;
        private System.Windows.Forms.ToolStripSeparator ctxSep;
        private System.Windows.Forms.ToolStripMenuItem ctxStop;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.SuspendLayout();

            // ── FORM ─────────────────────────────────────────────────────
            this.AutoScaleMode  = System.Windows.Forms.AutoScaleMode.Font;
            this.Text           = "WiFi Ağ Yöneticisi";
            this.ClientSize     = new System.Drawing.Size(1200, 760);
            this.MinimumSize    = new System.Drawing.Size(900, 600);
            this.StartPosition  = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.BackColor      = System.Drawing.Color.FromArgb(10, 10, 10);
            this.ForeColor      = System.Drawing.Color.White;
            this.Font           = new System.Drawing.Font("Segoe UI", 9F);
            this.DoubleBuffered = true;
            this.Name           = "MainForm";
            var iconPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.ico");
            if (System.IO.File.Exists(iconPath))
                this.Icon = new System.Drawing.Icon(iconPath);

            // ── HEADER ───────────────────────────────────────────────────
            this.topPanel           = new System.Windows.Forms.Panel();
            this.topPanel.Dock      = System.Windows.Forms.DockStyle.Top;
            this.topPanel.Height    = 64;
            this.topPanel.BackColor = System.Drawing.Color.FromArgb(18, 18, 24);
            this.topPanel.Name      = "topPanel";

            this.lblAppName           = new System.Windows.Forms.Label();
            this.lblAppName.Text      = "  WiFi Ağ Yöneticisi";
            this.lblAppName.Font      = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
            this.lblAppName.ForeColor = System.Drawing.Color.White;
            this.lblAppName.AutoSize  = true;
            this.lblAppName.Location  = new System.Drawing.Point(10, 16);
            this.lblAppName.BackColor = System.Drawing.Color.Transparent;
            this.lblAppName.Name      = "lblAppName";

            this.infoFlow               = new System.Windows.Forms.FlowLayoutPanel();
            this.infoFlow.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.infoFlow.AutoSize      = true;
            this.infoFlow.BackColor     = System.Drawing.Color.Transparent;
            this.infoFlow.WrapContents  = false;
            this.infoFlow.Padding       = new System.Windows.Forms.Padding(0);
            this.infoFlow.Name          = "infoFlow";

            this.lblIP           = new System.Windows.Forms.Label();
            this.lblIP.Text      = "IP  : —";
            this.lblIP.AutoSize  = true;
            this.lblIP.ForeColor = System.Drawing.Color.FromArgb(130, 130, 130);
            this.lblIP.Font      = new System.Drawing.Font("Segoe UI", 8F);
            this.lblIP.BackColor = System.Drawing.Color.Transparent;
            this.lblIP.Margin    = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.lblIP.Name      = "lblIP";

            this.lblGW           = new System.Windows.Forms.Label();
            this.lblGW.Text      = "GW  : —";
            this.lblGW.AutoSize  = true;
            this.lblGW.ForeColor = System.Drawing.Color.FromArgb(130, 130, 130);
            this.lblGW.Font      = new System.Drawing.Font("Segoe UI", 8F);
            this.lblGW.BackColor = System.Drawing.Color.Transparent;
            this.lblGW.Margin    = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.lblGW.Name      = "lblGW";

            this.lblIf           = new System.Windows.Forms.Label();
            this.lblIf.Text      = "Arayüz : —";
            this.lblIf.AutoSize  = true;
            this.lblIf.ForeColor = System.Drawing.Color.FromArgb(130, 130, 130);
            this.lblIf.Font      = new System.Drawing.Font("Segoe UI", 8F);
            this.lblIf.BackColor = System.Drawing.Color.Transparent;
            this.lblIf.Margin    = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.lblIf.Name      = "lblIf";

            this.infoFlow.Controls.Add(this.lblIP);
            this.infoFlow.Controls.Add(this.lblGW);
            this.infoFlow.Controls.Add(this.lblIf);
            this.topPanel.Controls.Add(this.lblAppName);
            this.topPanel.Controls.Add(this.infoFlow);

            // ── PROGRESS BAR ─────────────────────────────────────────────
            this.prog                       = new System.Windows.Forms.ProgressBar();
            this.prog.Dock                  = System.Windows.Forms.DockStyle.Top;
            this.prog.Height                = 2;
            this.prog.Style                 = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.prog.MarqueeAnimationSpeed = 25;
            this.prog.Visible               = false;
            this.prog.Name                  = "prog";

            // ── BODY TABLE (2 cols: buttons | content) ────────────────────
            this.bodyTable              = new System.Windows.Forms.TableLayoutPanel();
            this.bodyTable.Dock         = System.Windows.Forms.DockStyle.Fill;
            this.bodyTable.ColumnCount  = 2;
            this.bodyTable.RowCount     = 1;
            this.bodyTable.BackColor    = System.Drawing.Color.FromArgb(10, 10, 10);
            this.bodyTable.Padding      = new System.Windows.Forms.Padding(0);
            this.bodyTable.Margin       = new System.Windows.Forms.Padding(0);
            this.bodyTable.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.None;
            this.bodyTable.Name         = "bodyTable";
            this.bodyTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 158F));
            this.bodyTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.bodyTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));

            // ── BUTTON PANEL ─────────────────────────────────────────────
            this.btnPanel             = new System.Windows.Forms.TableLayoutPanel();
            this.btnPanel.Dock        = System.Windows.Forms.DockStyle.Fill;
            this.btnPanel.ColumnCount = 1;
            this.btnPanel.RowCount    = 7;
            this.btnPanel.BackColor   = System.Drawing.Color.FromArgb(16, 16, 22);
            this.btnPanel.Padding     = new System.Windows.Forms.Padding(10, 10, 10, 10);
            this.btnPanel.Margin      = new System.Windows.Forms.Padding(0);
            this.btnPanel.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.None;
            this.btnPanel.Name        = "btnPanel";
            this.btnPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.btnPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 96F));
            this.btnPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 96F));
            this.btnPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 96F));
            this.btnPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 96F));
            this.btnPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 96F));
            this.btnPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 96F));
            this.btnPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));

            // bScan
            this.bScan                                   = new System.Windows.Forms.Button();
            this.bScan.Text                              = "⟳  Ağı Tara";
            this.bScan.Font                              = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.bScan.FlatStyle                         = System.Windows.Forms.FlatStyle.Flat;
            this.bScan.BackColor                         = System.Drawing.Color.FromArgb(30, 30, 42);
            this.bScan.ForeColor                         = System.Drawing.Color.FromArgb(242, 244, 250);
            this.bScan.Cursor                            = System.Windows.Forms.Cursors.Hand;
            this.bScan.Enabled                           = true;
            this.bScan.Dock                              = System.Windows.Forms.DockStyle.Fill;
            this.bScan.Margin                            = new System.Windows.Forms.Padding(0, 0, 0, 6);
            this.bScan.TextAlign                         = System.Drawing.ContentAlignment.MiddleLeft;
            this.bScan.Padding                           = new System.Windows.Forms.Padding(16, 0, 10, 0);
            this.bScan.FlatAppearance.BorderSize         = 0;
            this.bScan.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(34, 35, 48);
            this.bScan.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(46, 47, 62);
            this.bScan.Name                              = "bScan";

            // bBlock
            this.bBlock                                   = new System.Windows.Forms.Button();
            this.bBlock.Text                              = "⦸  İnterneti Kes";
            this.bBlock.Font                              = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.bBlock.FlatStyle                         = System.Windows.Forms.FlatStyle.Flat;
            this.bBlock.BackColor                         = System.Drawing.Color.FromArgb(24, 24, 32);
            this.bBlock.ForeColor                         = System.Drawing.Color.FromArgb(238, 240, 245);
            this.bBlock.Cursor                            = System.Windows.Forms.Cursors.Hand;
            this.bBlock.Enabled                           = false;
            this.bBlock.Dock                              = System.Windows.Forms.DockStyle.Fill;
            this.bBlock.Margin                            = new System.Windows.Forms.Padding(0, 0, 0, 6);
            this.bBlock.TextAlign                         = System.Drawing.ContentAlignment.MiddleLeft;
            this.bBlock.Padding                           = new System.Windows.Forms.Padding(16, 0, 10, 0);
            this.bBlock.FlatAppearance.BorderSize         = 0;
            this.bBlock.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(34, 35, 48);
            this.bBlock.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(46, 47, 62);
            this.bBlock.Name                              = "bBlock";

            // bDns1
            this.bDns1                                   = new System.Windows.Forms.Button();
            this.bDns1.Text                              = "◉  DNS İzle";
            this.bDns1.Font                              = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.bDns1.FlatStyle                         = System.Windows.Forms.FlatStyle.Flat;
            this.bDns1.BackColor                         = System.Drawing.Color.FromArgb(24, 24, 32);
            this.bDns1.ForeColor                         = System.Drawing.Color.FromArgb(238, 240, 245);
            this.bDns1.Cursor                            = System.Windows.Forms.Cursors.Hand;
            this.bDns1.Enabled                           = false;
            this.bDns1.Dock                              = System.Windows.Forms.DockStyle.Fill;
            this.bDns1.Margin                            = new System.Windows.Forms.Padding(0, 0, 0, 6);
            this.bDns1.TextAlign                         = System.Drawing.ContentAlignment.MiddleLeft;
            this.bDns1.Padding                           = new System.Windows.Forms.Padding(16, 0, 10, 0);
            this.bDns1.FlatAppearance.BorderSize         = 0;
            this.bDns1.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(34, 35, 48);
            this.bDns1.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(46, 47, 62);
            this.bDns1.Name                              = "bDns1";

            // bDnsA
            this.bDnsA                                   = new System.Windows.Forms.Button();
            this.bDnsA.Text                              = "◉  Tümünü İzle (DNS)";
            this.bDnsA.Font                              = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.bDnsA.FlatStyle                         = System.Windows.Forms.FlatStyle.Flat;
            this.bDnsA.BackColor                         = System.Drawing.Color.FromArgb(24, 24, 32);
            this.bDnsA.ForeColor                         = System.Drawing.Color.FromArgb(238, 240, 245);
            this.bDnsA.Cursor                            = System.Windows.Forms.Cursors.Hand;
            this.bDnsA.Enabled                           = false;
            this.bDnsA.Dock                              = System.Windows.Forms.DockStyle.Fill;
            this.bDnsA.Margin                            = new System.Windows.Forms.Padding(0, 0, 0, 6);
            this.bDnsA.TextAlign                         = System.Drawing.ContentAlignment.MiddleLeft;
            this.bDnsA.Padding                           = new System.Windows.Forms.Padding(16, 0, 10, 0);
            this.bDnsA.FlatAppearance.BorderSize         = 0;
            this.bDnsA.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(34, 35, 48);
            this.bDnsA.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(46, 47, 62);
            this.bDnsA.Name                              = "bDnsA";

            // bStop
            this.bStop                                   = new System.Windows.Forms.Button();
            this.bStop.Text                              = "■  Durdur";
            this.bStop.Font                              = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.bStop.FlatStyle                         = System.Windows.Forms.FlatStyle.Flat;
            this.bStop.BackColor                         = System.Drawing.Color.FromArgb(24, 24, 32);
            this.bStop.ForeColor                         = System.Drawing.Color.FromArgb(238, 240, 245);
            this.bStop.Cursor                            = System.Windows.Forms.Cursors.Hand;
            this.bStop.Enabled                           = false;
            this.bStop.Dock                              = System.Windows.Forms.DockStyle.Fill;
            this.bStop.Margin                            = new System.Windows.Forms.Padding(0, 0, 0, 6);
            this.bStop.TextAlign                         = System.Drawing.ContentAlignment.MiddleLeft;
            this.bStop.Padding                           = new System.Windows.Forms.Padding(16, 0, 10, 0);
            this.bStop.FlatAppearance.BorderSize         = 0;
            this.bStop.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(34, 35, 48);
            this.bStop.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(46, 47, 62);
            this.bStop.Name                              = "bStop";

            // bRen
            this.bRen                                   = new System.Windows.Forms.Button();
            this.bRen.Text                              = "✎  İsim Ver";
            this.bRen.Font                              = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.bRen.FlatStyle                         = System.Windows.Forms.FlatStyle.Flat;
            this.bRen.BackColor                         = System.Drawing.Color.FromArgb(24, 24, 32);
            this.bRen.ForeColor                         = System.Drawing.Color.FromArgb(238, 240, 245);
            this.bRen.Cursor                            = System.Windows.Forms.Cursors.Hand;
            this.bRen.Enabled                           = false;
            this.bRen.Dock                              = System.Windows.Forms.DockStyle.Fill;
            this.bRen.Margin                            = new System.Windows.Forms.Padding(0);
            this.bRen.TextAlign                         = System.Drawing.ContentAlignment.MiddleLeft;
            this.bRen.Padding                           = new System.Windows.Forms.Padding(16, 0, 10, 0);
            this.bRen.FlatAppearance.BorderSize         = 0;
            this.bRen.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(34, 35, 48);
            this.bRen.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(46, 47, 62);
            this.bRen.Name                              = "bRen";

            this.btnPanel.Controls.Add(this.bScan,  0, 0);
            this.btnPanel.Controls.Add(this.bBlock, 0, 1);
            this.btnPanel.Controls.Add(this.bDns1,  0, 2);
            this.btnPanel.Controls.Add(this.bDnsA,  0, 3);
            this.btnPanel.Controls.Add(this.bStop,  0, 4);
            this.btnPanel.Controls.Add(this.bRen,   0, 5);

            // ── CONTENT TABLE (grid | log, 50/50, responsive) ─────────────
            this.contentTable              = new System.Windows.Forms.TableLayoutPanel();
            this.contentTable.Dock         = System.Windows.Forms.DockStyle.Fill;
            this.contentTable.ColumnCount  = 2;
            this.contentTable.RowCount     = 1;
            this.contentTable.BackColor    = System.Drawing.Color.FromArgb(10, 10, 10);
            this.contentTable.Padding      = new System.Windows.Forms.Padding(0);
            this.contentTable.Margin       = new System.Windows.Forms.Padding(0);
            this.contentTable.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.None;
            this.contentTable.Name         = "contentTable";
            this.contentTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.contentTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.contentTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));

            // ── GRID PANEL ───────────────────────────────────────────────
            this.gridPanel           = new System.Windows.Forms.Panel();
            this.gridPanel.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.gridPanel.BackColor = System.Drawing.Color.FromArgb(10, 10, 10);
            this.gridPanel.Margin    = new System.Windows.Forms.Padding(0);
            this.gridPanel.Name      = "gridPanel";

            this.gridHeader           = new System.Windows.Forms.Panel();
            this.gridHeader.Dock      = System.Windows.Forms.DockStyle.Top;
            this.gridHeader.Height    = 44;
            this.gridHeader.BackColor = System.Drawing.Color.FromArgb(22, 22, 30);
            this.gridHeader.Padding   = new System.Windows.Forms.Padding(16, 0, 16, 0);
            this.gridHeader.Name      = "gridHeader";

            this.gridTitle           = new System.Windows.Forms.Label();
            this.gridTitle.Text      = "Cihaz Listesi";
            this.gridTitle.Font      = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.gridTitle.ForeColor = System.Drawing.Color.White;
            this.gridTitle.Dock      = System.Windows.Forms.DockStyle.Left;
            this.gridTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.gridTitle.BackColor = System.Drawing.Color.Transparent;
            this.gridTitle.Name      = "gridTitle";

            this.lblCount           = new System.Windows.Forms.Label();
            this.lblCount.Text      = "";
            this.lblCount.Font      = new System.Drawing.Font("Segoe UI", 8F);
            this.lblCount.ForeColor = System.Drawing.Color.FromArgb(120, 120, 120);
            this.lblCount.Dock      = System.Windows.Forms.DockStyle.Right;
            this.lblCount.Width     = 90;
            this.lblCount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblCount.BackColor = System.Drawing.Color.Transparent;
            this.lblCount.Visible   = false;
            this.lblCount.Name      = "lblCount";

            this.gridHeader.Controls.Add(this.gridTitle);
            this.gridHeader.Controls.Add(this.lblCount);

            // Columns
            this.colIdx              = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colIdx.HeaderText   = "#";
            this.colIdx.FillWeight   = 5;
            this.colIdx.MinimumWidth = 42;
            this.colIdx.Name         = "colIdx";
            this.colIdx.DefaultCellStyle.Padding = new System.Windows.Forms.Padding(4, 0, 2, 0);

            this.colLabel              = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLabel.HeaderText   = "Etiket";
            this.colLabel.FillWeight   = 20;
            this.colLabel.MinimumWidth = 60;
            this.colLabel.Name         = "colLabel";

            this.colIP              = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colIP.HeaderText   = "IP Adresi";
            this.colIP.FillWeight   = 22;
            this.colIP.MinimumWidth = 60;
            this.colIP.Name         = "colIP";

            this.colMAC              = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colMAC.HeaderText   = "MAC";
            this.colMAC.FillWeight   = 26;
            this.colMAC.MinimumWidth = 80;
            this.colMAC.Name         = "colMAC";

            this.colHost              = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colHost.HeaderText   = "Cihaz Adı";
            this.colHost.FillWeight   = 27;
            this.colHost.MinimumWidth = 60;
            this.colHost.Name         = "colHost";

            this.grid                                                           = new System.Windows.Forms.DataGridView();
            this.grid.Dock                                                      = System.Windows.Forms.DockStyle.Fill;
            this.grid.BackgroundColor                                           = System.Drawing.Color.FromArgb(10, 10, 14);
            this.grid.GridColor                                                 = System.Drawing.Color.FromArgb(22, 22, 30);
            this.grid.AutoSizeColumnsMode                                       = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.grid.ColumnHeadersHeightSizeMode                               = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.grid.ColumnHeadersHeight                                       = 38;
            this.grid.RowTemplate.Height                                        = 44;
            this.grid.SelectionMode                                             = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grid.MultiSelect                                               = false;
            this.grid.ReadOnly                                                  = true;
            this.grid.AllowUserToAddRows                                        = false;
            this.grid.AllowUserToDeleteRows                                     = false;
            this.grid.BorderStyle                                               = System.Windows.Forms.BorderStyle.None;
            this.grid.RowHeadersVisible                                         = false;
            this.grid.CellBorderStyle                                           = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.grid.EnableHeadersVisualStyles                                 = false;
            this.grid.Font                                                      = new System.Drawing.Font("Segoe UI", 9F);
            this.grid.Margin                                                    = new System.Windows.Forms.Padding(0);
            this.grid.Name                                                      = "grid";
            this.grid.DefaultCellStyle.BackColor                                = System.Drawing.Color.FromArgb(10, 10, 14);
            this.grid.DefaultCellStyle.ForeColor                                = System.Drawing.Color.FromArgb(215, 215, 215);
            this.grid.DefaultCellStyle.SelectionBackColor                       = System.Drawing.Color.FromArgb(35, 35, 55);
            this.grid.DefaultCellStyle.SelectionForeColor                       = System.Drawing.Color.White;
            this.grid.DefaultCellStyle.Padding                                  = new System.Windows.Forms.Padding(12, 0, 4, 0);
            this.grid.DefaultCellStyle.Alignment                                = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            this.grid.DefaultCellStyle.Font                                     = new System.Drawing.Font("Segoe UI", 9F);
            this.grid.AlternatingRowsDefaultCellStyle.BackColor                 = System.Drawing.Color.FromArgb(14, 14, 20);
            this.grid.AlternatingRowsDefaultCellStyle.ForeColor                 = System.Drawing.Color.FromArgb(215, 215, 215);
            this.grid.AlternatingRowsDefaultCellStyle.SelectionBackColor        = System.Drawing.Color.FromArgb(35, 35, 55);
            this.grid.AlternatingRowsDefaultCellStyle.SelectionForeColor        = System.Drawing.Color.White;
            this.grid.AlternatingRowsDefaultCellStyle.Padding                   = new System.Windows.Forms.Padding(12, 0, 4, 0);
            this.grid.ColumnHeadersDefaultCellStyle.BackColor                   = System.Drawing.Color.FromArgb(22, 22, 30);
            this.grid.ColumnHeadersDefaultCellStyle.ForeColor                   = System.Drawing.Color.FromArgb(140, 140, 160);
            this.grid.ColumnHeadersDefaultCellStyle.Font                        = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            this.grid.ColumnHeadersDefaultCellStyle.Padding                     = new System.Windows.Forms.Padding(12, 0, 4, 0);
            this.grid.ColumnHeadersDefaultCellStyle.Alignment                   = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            this.grid.ColumnHeadersDefaultCellStyle.SelectionBackColor          = System.Drawing.Color.FromArgb(22, 22, 30);
            this.grid.ColumnHeadersDefaultCellStyle.SelectionForeColor          = System.Drawing.Color.FromArgb(140, 140, 160);
            this.grid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                this.colIdx, this.colLabel, this.colIP, this.colMAC, this.colHost });

            // Context menu
            this.ctxMenu           = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ctxMenu.BackColor = System.Drawing.Color.FromArgb(22, 22, 22);
            this.ctxMenu.ForeColor = System.Drawing.Color.White;
            this.ctxMenu.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.ctxMenu.Name      = "ctxMenu";

            this.ctxRename           = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxRename.Text      = "✎  İsim Ver";
            this.ctxRename.ForeColor = System.Drawing.Color.White;
            this.ctxRename.BackColor = System.Drawing.Color.FromArgb(28, 28, 28);
            this.ctxRename.Name      = "ctxRename";

            this.ctxBlock           = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxBlock.Text      = "⦸  İnterneti Kes";
            this.ctxBlock.ForeColor = System.Drawing.Color.White;
            this.ctxBlock.BackColor = System.Drawing.Color.FromArgb(28, 28, 28);
            this.ctxBlock.Name      = "ctxBlock";

            this.ctxDns           = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxDns.Text      = "◉  DNS İzle";
            this.ctxDns.ForeColor = System.Drawing.Color.White;
            this.ctxDns.BackColor = System.Drawing.Color.FromArgb(28, 28, 28);
            this.ctxDns.Name      = "ctxDns";

            this.ctxSep = new System.Windows.Forms.ToolStripSeparator();

            this.ctxStop           = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxStop.Text      = "■  Durdur";
            this.ctxStop.ForeColor = System.Drawing.Color.White;
            this.ctxStop.BackColor = System.Drawing.Color.FromArgb(28, 28, 28);
            this.ctxStop.Name      = "ctxStop";

            this.ctxMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.ctxRename, this.ctxBlock, this.ctxDns, this.ctxSep, this.ctxStop });
            this.grid.ContextMenuStrip = this.ctxMenu;

            this.gridPanel.Controls.Add(this.grid);
            this.gridPanel.Controls.Add(this.gridHeader);

            // ── LOG PANEL ────────────────────────────────────────────────
            this.logPanel           = new System.Windows.Forms.Panel();
            this.logPanel.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.logPanel.BackColor = System.Drawing.Color.FromArgb(10, 10, 10);
            this.logPanel.Margin    = new System.Windows.Forms.Padding(0);
            this.logPanel.Name      = "logPanel";

            this.logHeader           = new System.Windows.Forms.Panel();
            this.logHeader.Dock      = System.Windows.Forms.DockStyle.Top;
            this.logHeader.Height    = 44;
            this.logHeader.BackColor = System.Drawing.Color.FromArgb(22, 22, 30);
            this.logHeader.Padding   = new System.Windows.Forms.Padding(16, 0, 16, 0);
            this.logHeader.Name      = "logHeader";

            this.logTitle           = new System.Windows.Forms.Label();
            this.logTitle.Text      = "Konsol";
            this.logTitle.Font      = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.logTitle.ForeColor = System.Drawing.Color.White;
            this.logTitle.Dock      = System.Windows.Forms.DockStyle.Left;
            this.logTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.logTitle.BackColor = System.Drawing.Color.Transparent;
            this.logTitle.Name      = "logTitle";

            this.clrBtn                                   = new System.Windows.Forms.Button();
            this.clrBtn.Text                              = "Temizle";
            this.clrBtn.FlatStyle                         = System.Windows.Forms.FlatStyle.Flat;
            this.clrBtn.BackColor                         = System.Drawing.Color.FromArgb(26, 26, 26);
            this.clrBtn.ForeColor                         = System.Drawing.Color.FromArgb(130, 130, 130);
            this.clrBtn.Font                              = new System.Drawing.Font("Segoe UI", 8F);
            this.clrBtn.Dock                              = System.Windows.Forms.DockStyle.Right;
            this.clrBtn.Width                             = 70;
            this.clrBtn.Cursor                            = System.Windows.Forms.Cursors.Hand;
            this.clrBtn.FlatAppearance.BorderSize         = 0;
            this.clrBtn.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(36, 36, 36);
            this.clrBtn.Name                              = "clrBtn";

            this.logHeader.Controls.Add(this.clrBtn);
            this.logHeader.Controls.Add(this.logTitle);

            this.log             = new System.Windows.Forms.RichTextBox();
            this.log.Dock        = System.Windows.Forms.DockStyle.Fill;
            this.log.BackColor   = System.Drawing.Color.FromArgb(10, 10, 10);
            this.log.ForeColor   = System.Drawing.Color.FromArgb(200, 200, 200);
            this.log.Font        = new System.Drawing.Font("Consolas", 9F);
            this.log.ReadOnly    = true;
            this.log.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.log.ScrollBars  = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.log.WordWrap    = false;
            this.log.Padding     = new System.Windows.Forms.Padding(8);
            this.log.Name        = "log";

            this.logPanel.Controls.Add(this.log);
            this.logPanel.Controls.Add(this.logHeader);

            // ── ASSEMBLE ─────────────────────────────────────────────────
            this.contentTable.Controls.Add(this.gridPanel, 0, 0);
            this.contentTable.Controls.Add(this.logPanel,  1, 0);

            this.bodyTable.Controls.Add(this.btnPanel,      0, 0);
            this.bodyTable.Controls.Add(this.contentTable,  1, 0);

            // Order matters: Fill first, then Top (last Top = first rendered)
            this.Controls.Add(this.bodyTable);
            this.Controls.Add(this.prog);
            this.Controls.Add(this.topPanel);

            // ── EVENTS ───────────────────────────────────────────────────
            this.bScan.Click           += new System.EventHandler(this.BtnScan);
            this.bBlock.Click          += new System.EventHandler(this.BtnBlock);
            this.bDns1.Click           += new System.EventHandler(this.BtnDns1);
            this.bDnsA.Click           += new System.EventHandler(this.BtnDnsAll);
            this.bStop.Click           += new System.EventHandler(this.BtnStop);
            this.bRen.Click            += new System.EventHandler(this.BtnRen);
            this.clrBtn.Click          += new System.EventHandler(this.ClrBtnClick);
            this.ctxRename.Click       += new System.EventHandler(this.CtxRenameClick);
            this.ctxBlock.Click        += new System.EventHandler(this.CtxBlockClick);
            this.ctxDns.Click          += new System.EventHandler(this.CtxDnsClick);
            this.ctxStop.Click         += new System.EventHandler(this.CtxStopClick);
            this.grid.SelectionChanged += new System.EventHandler(this.GridSelectionChanged);
            this.grid.CellDoubleClick  += new System.Windows.Forms.DataGridViewCellEventHandler(this.GridCellDoubleClick);
            this.grid.MouseDown        += new System.Windows.Forms.MouseEventHandler(this.GridMouseDown);
            this.grid.CellPainting     += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.GridCellPainting);
            this.topPanel.Layout       += new System.Windows.Forms.LayoutEventHandler(this.TopPanelLayout);
            this.topPanel.Paint        += new System.Windows.Forms.PaintEventHandler(this.TopPanelPaint);
            this.FormClosing           += new System.Windows.Forms.FormClosingEventHandler(this.MainFormClosing);

            this.ResumeLayout(false);
        }
    }
}
