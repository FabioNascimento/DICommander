namespace DIC
{
    partial class frmMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            Fireball.Windows.Forms.LineMarginRender lineMarginRender1 = new Fireball.Windows.Forms.LineMarginRender();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tabCommandHost = new System.Windows.Forms.TabControl();
            this.tabShell = new System.Windows.Forms.TabPage();
            this.Shell = new UILibrary.ShellControl();
            this.tabTrace = new System.Windows.Forms.TabPage();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnClearTraceWindow = new System.Windows.Forms.ToolStripButton();
            this.TraceShell = new UILibrary.ShellControl();
            this.tabDefine = new System.Windows.Forms.TabControl();
            this.tabSessions = new System.Windows.Forms.TabPage();
            this.label4 = new System.Windows.Forms.Label();
            this.txtDefaultSession = new System.Windows.Forms.TextBox();
            this.btn_SetDefaultSession = new System.Windows.Forms.Button();
            this.btn_CloseSession = new System.Windows.Forms.Button();
            this.lstSessions = new System.Windows.Forms.ListBox();
            this.label3 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.input_dbservertype = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.input_forceDB = new System.Windows.Forms.CheckBox();
            this.input_usetrusted = new System.Windows.Forms.CheckBox();
            this.button_refreshcompany = new System.Windows.Forms.Button();
            this.img_Login = new System.Windows.Forms.ImageList(this.components);
            this.input_sessionhandle = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label_licensesrv = new System.Windows.Forms.Label();
            this.input_licensesrv = new System.Windows.Forms.TextBox();
            this.input_serverpassword = new System.Windows.Forms.TextBox();
            this.input_serveruserid = new System.Windows.Forms.TextBox();
            this.input_server = new System.Windows.Forms.TextBox();
            this.input_password = new System.Windows.Forms.TextBox();
            this.input_username = new System.Windows.Forms.TextBox();
            this.button_OK = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label_serveruserid = new System.Windows.Forms.Label();
            this.input_company = new System.Windows.Forms.ComboBox();
            this.label_Company = new System.Windows.Forms.Label();
            this.label_server = new System.Windows.Forms.Label();
            this.label_password = new System.Windows.Forms.Label();
            this.label_username = new System.Windows.Forms.Label();
            this.tabCode = new System.Windows.Forms.TabPage();
            this.toolStrip2 = new System.Windows.Forms.ToolStrip();
            this.btnExecute = new System.Windows.Forms.ToolStripButton();
            this.btnClear = new System.Windows.Forms.ToolStripButton();
            this.btnCopyAll = new System.Windows.Forms.ToolStripButton();
            this.codeEditor = new Fireball.Windows.Forms.CodeEditorControl();
            this.syntaxDocument1 = new Fireball.Syntax.SyntaxDocument(this.components);
            this.tabReference = new System.Windows.Forms.TabPage();
            this.label20 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.lblObjectTypeName = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.lstEnumValues = new System.Windows.Forms.ListBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.cmbEnumInfo = new System.Windows.Forms.ComboBox();
            this.cmbRefEnum = new System.Windows.Forms.ComboBox();
            this.cmbRefObjects = new System.Windows.Forms.ComboBox();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tabCommandHost.SuspendLayout();
            this.tabShell.SuspendLayout();
            this.tabTrace.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.tabDefine.SuspendLayout();
            this.tabSessions.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tabCode.SuspendLayout();
            this.toolStrip2.SuspendLayout();
            this.tabReference.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Location = new System.Drawing.Point(0, 728);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(615, 22);
            this.statusStrip1.TabIndex = 0;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 24);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tabCommandHost);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tabDefine);
            this.splitContainer1.Size = new System.Drawing.Size(615, 704);
            this.splitContainer1.SplitterDistance = 280;
            this.splitContainer1.TabIndex = 6;
            // 
            // tabCommandHost
            // 
            this.tabCommandHost.Controls.Add(this.tabShell);
            this.tabCommandHost.Controls.Add(this.tabTrace);
            this.tabCommandHost.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabCommandHost.Location = new System.Drawing.Point(0, 0);
            this.tabCommandHost.Name = "tabCommandHost";
            this.tabCommandHost.SelectedIndex = 0;
            this.tabCommandHost.Size = new System.Drawing.Size(615, 280);
            this.tabCommandHost.TabIndex = 0;
            // 
            // tabShell
            // 
            this.tabShell.Controls.Add(this.Shell);
            this.tabShell.Location = new System.Drawing.Point(4, 22);
            this.tabShell.Name = "tabShell";
            this.tabShell.Padding = new System.Windows.Forms.Padding(3);
            this.tabShell.Size = new System.Drawing.Size(607, 254);
            this.tabShell.TabIndex = 0;
            this.tabShell.Text = "Shell";
            this.tabShell.UseVisualStyleBackColor = true;
            // 
            // Shell
            // 
            this.Shell.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Shell.Location = new System.Drawing.Point(3, 3);
            this.Shell.Name = "Shell";
            this.Shell.Prompt = ">>>";
            this.Shell.ShellTextBackColor = System.Drawing.Color.Black;
            this.Shell.ShellTextFont = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Shell.ShellTextForeColor = System.Drawing.Color.LawnGreen;
            this.Shell.Size = new System.Drawing.Size(601, 248);
            this.Shell.TabIndex = 2;
            this.Shell.Load += new System.EventHandler(this.Shell_Load);
            // 
            // tabTrace
            // 
            this.tabTrace.Controls.Add(this.toolStrip1);
            this.tabTrace.Controls.Add(this.TraceShell);
            this.tabTrace.Location = new System.Drawing.Point(4, 22);
            this.tabTrace.Name = "tabTrace";
            this.tabTrace.Padding = new System.Windows.Forms.Padding(3);
            this.tabTrace.Size = new System.Drawing.Size(607, 257);
            this.tabTrace.TabIndex = 1;
            this.tabTrace.Text = "Trace";
            this.tabTrace.UseVisualStyleBackColor = true;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnClearTraceWindow});
            this.toolStrip1.Location = new System.Drawing.Point(3, 3);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(601, 25);
            this.toolStrip1.TabIndex = 2;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // btnClearTraceWindow
            // 
            this.btnClearTraceWindow.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnClearTraceWindow.Image = ((System.Drawing.Image)(resources.GetObject("btnClearTraceWindow.Image")));
            this.btnClearTraceWindow.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnClearTraceWindow.Name = "btnClearTraceWindow";
            this.btnClearTraceWindow.Size = new System.Drawing.Size(38, 22);
            this.btnClearTraceWindow.Text = "Clear";
            this.btnClearTraceWindow.Click += new System.EventHandler(this.btnClearTraceWindow_Click);
            // 
            // TraceShell
            // 
            this.TraceShell.Location = new System.Drawing.Point(3, 22);
            this.TraceShell.Name = "TraceShell";
            this.TraceShell.Prompt = "!>";
            this.TraceShell.ShellTextBackColor = System.Drawing.Color.Black;
            this.TraceShell.ShellTextFont = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TraceShell.ShellTextForeColor = System.Drawing.Color.OrangeRed;
            this.TraceShell.Size = new System.Drawing.Size(601, 232);
            this.TraceShell.TabIndex = 1;
            // 
            // tabDefine
            // 
            this.tabDefine.Controls.Add(this.tabSessions);
            this.tabDefine.Controls.Add(this.tabCode);
            this.tabDefine.Controls.Add(this.tabReference);
            this.tabDefine.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabDefine.Location = new System.Drawing.Point(0, 0);
            this.tabDefine.Name = "tabDefine";
            this.tabDefine.SelectedIndex = 0;
            this.tabDefine.Size = new System.Drawing.Size(615, 420);
            this.tabDefine.TabIndex = 0;
            this.tabDefine.Tag = "";
            // 
            // tabSessions
            // 
            this.tabSessions.Controls.Add(this.label4);
            this.tabSessions.Controls.Add(this.txtDefaultSession);
            this.tabSessions.Controls.Add(this.btn_SetDefaultSession);
            this.tabSessions.Controls.Add(this.btn_CloseSession);
            this.tabSessions.Controls.Add(this.lstSessions);
            this.tabSessions.Controls.Add(this.label3);
            this.tabSessions.Controls.Add(this.panel1);
            this.tabSessions.Location = new System.Drawing.Point(4, 22);
            this.tabSessions.Name = "tabSessions";
            this.tabSessions.Padding = new System.Windows.Forms.Padding(3);
            this.tabSessions.Size = new System.Drawing.Size(607, 394);
            this.tabSessions.TabIndex = 1;
            this.tabSessions.Text = "B1 Sessions";
            this.tabSessions.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(10, 73);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(82, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Default session:";
            // 
            // txtDefaultSession
            // 
            this.txtDefaultSession.Enabled = false;
            this.txtDefaultSession.Location = new System.Drawing.Point(115, 70);
            this.txtDefaultSession.Name = "txtDefaultSession";
            this.txtDefaultSession.Size = new System.Drawing.Size(260, 20);
            this.txtDefaultSession.TabIndex = 5;
            // 
            // btn_SetDefaultSession
            // 
            this.btn_SetDefaultSession.Enabled = false;
            this.btn_SetDefaultSession.Location = new System.Drawing.Point(381, 38);
            this.btn_SetDefaultSession.Name = "btn_SetDefaultSession";
            this.btn_SetDefaultSession.Size = new System.Drawing.Size(136, 24);
            this.btn_SetDefaultSession.TabIndex = 4;
            this.btn_SetDefaultSession.Text = "Set as default session";
            this.btn_SetDefaultSession.UseVisualStyleBackColor = true;
            this.btn_SetDefaultSession.Click += new System.EventHandler(this.btn_SetDefaultSession_Click);
            // 
            // btn_CloseSession
            // 
            this.btn_CloseSession.Enabled = false;
            this.btn_CloseSession.Location = new System.Drawing.Point(381, 12);
            this.btn_CloseSession.Name = "btn_CloseSession";
            this.btn_CloseSession.Size = new System.Drawing.Size(136, 24);
            this.btn_CloseSession.TabIndex = 3;
            this.btn_CloseSession.Text = "Drop session";
            this.btn_CloseSession.UseVisualStyleBackColor = true;
            this.btn_CloseSession.Click += new System.EventHandler(this.btn_CloseSession_Click);
            // 
            // lstSessions
            // 
            this.lstSessions.FormattingEnabled = true;
            this.lstSessions.Location = new System.Drawing.Point(115, 12);
            this.lstSessions.Name = "lstSessions";
            this.lstSessions.Size = new System.Drawing.Size(260, 56);
            this.lstSessions.TabIndex = 2;
            this.lstSessions.SelectedIndexChanged += new System.EventHandler(this.lstSessions_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 12);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(99, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "Active B1 sessions:";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.input_dbservertype);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.input_forceDB);
            this.panel1.Controls.Add(this.input_usetrusted);
            this.panel1.Controls.Add(this.button_refreshcompany);
            this.panel1.Controls.Add(this.input_sessionhandle);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.label_licensesrv);
            this.panel1.Controls.Add(this.input_licensesrv);
            this.panel1.Controls.Add(this.input_serverpassword);
            this.panel1.Controls.Add(this.input_serveruserid);
            this.panel1.Controls.Add(this.input_server);
            this.panel1.Controls.Add(this.input_password);
            this.panel1.Controls.Add(this.input_username);
            this.panel1.Controls.Add(this.button_OK);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.label_serveruserid);
            this.panel1.Controls.Add(this.input_company);
            this.panel1.Controls.Add(this.label_Company);
            this.panel1.Controls.Add(this.label_server);
            this.panel1.Controls.Add(this.label_password);
            this.panel1.Controls.Add(this.label_username);
            this.panel1.Location = new System.Drawing.Point(6, 95);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(511, 278);
            this.panel1.TabIndex = 0;
            // 
            // input_dbservertype
            // 
            this.input_dbservertype.FormattingEnabled = true;
            this.input_dbservertype.Items.AddRange(new object[] {
            "MSSQL2005",
            "MSSQL",
            "DB2",
            "SYBASE"});
            this.input_dbservertype.Location = new System.Drawing.Point(111, 216);
            this.input_dbservertype.Name = "input_dbservertype";
            this.input_dbservertype.Size = new System.Drawing.Size(176, 21);
            this.input_dbservertype.TabIndex = 41;
            this.input_dbservertype.Text = "MSSQL2005";
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(22, 217);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(82, 16);
            this.label5.TabIndex = 40;
            this.label5.Text = "Db server type";
            // 
            // input_forceDB
            // 
            this.input_forceDB.Location = new System.Drawing.Point(304, 138);
            this.input_forceDB.Name = "input_forceDB";
            this.input_forceDB.Size = new System.Drawing.Size(79, 24);
            this.input_forceDB.TabIndex = 38;
            this.input_forceDB.Text = "DB direct";
            this.input_forceDB.CheckedChanged += new System.EventHandler(this.input_forceDB_CheckedChanged);
            // 
            // input_usetrusted
            // 
            this.input_usetrusted.Checked = true;
            this.input_usetrusted.CheckState = System.Windows.Forms.CheckState.Checked;
            this.input_usetrusted.Location = new System.Drawing.Point(304, 85);
            this.input_usetrusted.Name = "input_usetrusted";
            this.input_usetrusted.Size = new System.Drawing.Size(104, 24);
            this.input_usetrusted.TabIndex = 37;
            this.input_usetrusted.Text = "Use trusted";
            this.input_usetrusted.CheckedChanged += new System.EventHandler(this.input_usetrusted_CheckedChanged);
            // 
            // button_refreshcompany
            // 
            this.button_refreshcompany.ImageIndex = 0;
            this.button_refreshcompany.ImageList = this.img_Login;
            this.button_refreshcompany.Location = new System.Drawing.Point(389, 111);
            this.button_refreshcompany.Name = "button_refreshcompany";
            this.button_refreshcompany.Size = new System.Drawing.Size(32, 32);
            this.button_refreshcompany.TabIndex = 36;
            this.button_refreshcompany.Click += new System.EventHandler(this.button_refreshcompany_Click);
            // 
            // img_Login
            // 
            this.img_Login.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("img_Login.ImageStream")));
            this.img_Login.TransparentColor = System.Drawing.Color.Transparent;
            this.img_Login.Images.SetKeyName(0, "");
            // 
            // input_sessionhandle
            // 
            this.input_sessionhandle.Location = new System.Drawing.Point(111, 7);
            this.input_sessionhandle.Name = "input_sessionhandle";
            this.input_sessionhandle.Size = new System.Drawing.Size(176, 20);
            this.input_sessionhandle.TabIndex = 35;
            this.input_sessionhandle.Text = "session1";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(23, 7);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(80, 16);
            this.label2.TabIndex = 34;
            this.label2.Text = "Session handle";
            // 
            // label_licensesrv
            // 
            this.label_licensesrv.Location = new System.Drawing.Point(23, 190);
            this.label_licensesrv.Name = "label_licensesrv";
            this.label_licensesrv.Size = new System.Drawing.Size(80, 16);
            this.label_licensesrv.TabIndex = 33;
            this.label_licensesrv.Text = "License server";
            // 
            // input_licensesrv
            // 
            this.input_licensesrv.Enabled = false;
            this.input_licensesrv.Location = new System.Drawing.Point(111, 190);
            this.input_licensesrv.Name = "input_licensesrv";
            this.input_licensesrv.Size = new System.Drawing.Size(176, 20);
            this.input_licensesrv.TabIndex = 28;
            // 
            // input_serverpassword
            // 
            this.input_serverpassword.Enabled = false;
            this.input_serverpassword.Location = new System.Drawing.Point(111, 164);
            this.input_serverpassword.Name = "input_serverpassword";
            this.input_serverpassword.PasswordChar = '*';
            this.input_serverpassword.Size = new System.Drawing.Size(176, 20);
            this.input_serverpassword.TabIndex = 27;
            // 
            // input_serveruserid
            // 
            this.input_serveruserid.Enabled = false;
            this.input_serveruserid.Location = new System.Drawing.Point(111, 138);
            this.input_serveruserid.Name = "input_serveruserid";
            this.input_serveruserid.Size = new System.Drawing.Size(176, 20);
            this.input_serveruserid.TabIndex = 26;
            // 
            // input_server
            // 
            this.input_server.Location = new System.Drawing.Point(111, 85);
            this.input_server.Name = "input_server";
            this.input_server.Size = new System.Drawing.Size(176, 20);
            this.input_server.TabIndex = 23;
            // 
            // input_password
            // 
            this.input_password.Location = new System.Drawing.Point(111, 59);
            this.input_password.Name = "input_password";
            this.input_password.PasswordChar = '*';
            this.input_password.Size = new System.Drawing.Size(176, 20);
            this.input_password.TabIndex = 20;
            // 
            // input_username
            // 
            this.input_username.Location = new System.Drawing.Point(111, 33);
            this.input_username.Name = "input_username";
            this.input_username.Size = new System.Drawing.Size(176, 20);
            this.input_username.TabIndex = 18;
            // 
            // button_OK
            // 
            this.button_OK.Location = new System.Drawing.Point(111, 239);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(106, 24);
            this.button_OK.TabIndex = 31;
            this.button_OK.Text = "Open session";
            this.button_OK.Click += new System.EventHandler(this.button_Login_Click);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(23, 164);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 16);
            this.label1.TabIndex = 30;
            this.label1.Text = "DB password";
            // 
            // label_serveruserid
            // 
            this.label_serveruserid.Location = new System.Drawing.Point(23, 138);
            this.label_serveruserid.Name = "label_serveruserid";
            this.label_serveruserid.Size = new System.Drawing.Size(80, 16);
            this.label_serveruserid.TabIndex = 29;
            this.label_serveruserid.Text = "DB userid";
            // 
            // input_company
            // 
            this.input_company.Location = new System.Drawing.Point(111, 111);
            this.input_company.Name = "input_company";
            this.input_company.Size = new System.Drawing.Size(272, 21);
            this.input_company.TabIndex = 24;
            // 
            // label_Company
            // 
            this.label_Company.Location = new System.Drawing.Point(23, 111);
            this.label_Company.Name = "label_Company";
            this.label_Company.Size = new System.Drawing.Size(80, 13);
            this.label_Company.TabIndex = 25;
            this.label_Company.Text = "Company";
            // 
            // label_server
            // 
            this.label_server.Location = new System.Drawing.Point(23, 85);
            this.label_server.Name = "label_server";
            this.label_server.Size = new System.Drawing.Size(80, 13);
            this.label_server.TabIndex = 22;
            this.label_server.Text = "Server";
            // 
            // label_password
            // 
            this.label_password.Location = new System.Drawing.Point(23, 59);
            this.label_password.Name = "label_password";
            this.label_password.Size = new System.Drawing.Size(80, 13);
            this.label_password.TabIndex = 21;
            this.label_password.Text = "Password";
            // 
            // label_username
            // 
            this.label_username.Location = new System.Drawing.Point(23, 33);
            this.label_username.Name = "label_username";
            this.label_username.Size = new System.Drawing.Size(80, 16);
            this.label_username.TabIndex = 19;
            this.label_username.Text = "User name";
            // 
            // tabCode
            // 
            this.tabCode.Controls.Add(this.toolStrip2);
            this.tabCode.Controls.Add(this.codeEditor);
            this.tabCode.Location = new System.Drawing.Point(4, 22);
            this.tabCode.Name = "tabCode";
            this.tabCode.Padding = new System.Windows.Forms.Padding(3);
            this.tabCode.Size = new System.Drawing.Size(607, 398);
            this.tabCode.TabIndex = 0;
            this.tabCode.Text = "Code";
            this.tabCode.UseVisualStyleBackColor = true;
            // 
            // toolStrip2
            // 
            this.toolStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnExecute,
            this.btnClear,
            this.btnCopyAll});
            this.toolStrip2.Location = new System.Drawing.Point(3, 3);
            this.toolStrip2.Name = "toolStrip2";
            this.toolStrip2.Size = new System.Drawing.Size(601, 25);
            this.toolStrip2.TabIndex = 7;
            this.toolStrip2.Text = "toolStrip2";
            // 
            // btnExecute
            // 
            this.btnExecute.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnExecute.Image = ((System.Drawing.Image)(resources.GetObject("btnExecute.Image")));
            this.btnExecute.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnExecute.Name = "btnExecute";
            this.btnExecute.Size = new System.Drawing.Size(51, 22);
            this.btnExecute.Text = "Execute";
            this.btnExecute.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // btnClear
            // 
            this.btnClear.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnClear.Image = ((System.Drawing.Image)(resources.GetObject("btnClear.Image")));
            this.btnClear.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(38, 22);
            this.btnClear.Text = "Clear";
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnCopyAll
            // 
            this.btnCopyAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnCopyAll.Image = ((System.Drawing.Image)(resources.GetObject("btnCopyAll.Image")));
            this.btnCopyAll.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnCopyAll.Name = "btnCopyAll";
            this.btnCopyAll.Size = new System.Drawing.Size(56, 22);
            this.btnCopyAll.Text = "Copy All";
            this.btnCopyAll.Click += new System.EventHandler(this.btnCopyAll_Click);
            // 
            // codeEditor
            // 
            this.codeEditor.ActiveView = Fireball.Windows.Forms.CodeEditor.ActiveView.BottomRight;
            this.codeEditor.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.codeEditor.AutoListPosition = null;
            this.codeEditor.AutoListSelectedText = "a123";
            this.codeEditor.AutoListVisible = false;
            this.codeEditor.CopyAsRTF = false;
            this.codeEditor.Document = this.syntaxDocument1;
            this.codeEditor.InfoTipCount = 1;
            this.codeEditor.InfoTipPosition = null;
            this.codeEditor.InfoTipSelectedIndex = 1;
            this.codeEditor.InfoTipVisible = false;
            lineMarginRender1.Bounds = new System.Drawing.Rectangle(19, 0, 19, 16);
            this.codeEditor.LineMarginRender = lineMarginRender1;
            this.codeEditor.Location = new System.Drawing.Point(3, 31);
            this.codeEditor.LockCursorUpdate = false;
            this.codeEditor.Name = "codeEditor";
            this.codeEditor.Saved = false;
            this.codeEditor.ShowScopeIndicator = false;
            this.codeEditor.Size = new System.Drawing.Size(601, 364);
            this.codeEditor.SmoothScroll = false;
            this.codeEditor.SplitviewH = -4;
            this.codeEditor.SplitviewV = -4;
            this.codeEditor.TabGuideColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(243)))), ((int)(((byte)(234)))));
            this.codeEditor.TabIndex = 1;
            this.codeEditor.Text = "codeEditorControl1";
            this.codeEditor.WhitespaceColor = System.Drawing.SystemColors.ControlDark;
            // 
            // syntaxDocument1
            // 
            this.syntaxDocument1.Lines = new string[] {
        ""};
            this.syntaxDocument1.MaxUndoBufferSize = 1000;
            this.syntaxDocument1.Modified = false;
            this.syntaxDocument1.UndoStep = 0;
            // 
            // tabReference
            // 
            this.tabReference.Controls.Add(this.label20);
            this.tabReference.Controls.Add(this.label19);
            this.tabReference.Controls.Add(this.label18);
            this.tabReference.Controls.Add(this.label17);
            this.tabReference.Controls.Add(this.label16);
            this.tabReference.Controls.Add(this.label15);
            this.tabReference.Controls.Add(this.label14);
            this.tabReference.Controls.Add(this.lblObjectTypeName);
            this.tabReference.Controls.Add(this.label13);
            this.tabReference.Controls.Add(this.label12);
            this.tabReference.Controls.Add(this.label11);
            this.tabReference.Controls.Add(this.lstEnumValues);
            this.tabReference.Controls.Add(this.label10);
            this.tabReference.Controls.Add(this.label9);
            this.tabReference.Controls.Add(this.label8);
            this.tabReference.Controls.Add(this.label7);
            this.tabReference.Controls.Add(this.label6);
            this.tabReference.Controls.Add(this.cmbEnumInfo);
            this.tabReference.Controls.Add(this.cmbRefEnum);
            this.tabReference.Controls.Add(this.cmbRefObjects);
            this.tabReference.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.tabReference.Location = new System.Drawing.Point(4, 22);
            this.tabReference.Name = "tabReference";
            this.tabReference.Padding = new System.Windows.Forms.Padding(3);
            this.tabReference.Size = new System.Drawing.Size(607, 398);
            this.tabReference.TabIndex = 2;
            this.tabReference.Text = "Cheat Sheet";
            this.tabReference.UseVisualStyleBackColor = true;
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Italic);
            this.label20.Location = new System.Drawing.Point(11, 346);
            this.label20.MaximumSize = new System.Drawing.Size(260, 0);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(117, 15);
            this.label20.TabIndex = 21;
            this.label20.Text = "myStrBldr.ToString()";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Italic);
            this.label19.Location = new System.Drawing.Point(11, 331);
            this.label19.MaximumSize = new System.Drawing.Size(260, 0);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(161, 15);
            this.label19.TabIndex = 20;
            this.label19.Text = "myStrBldr.Append(\"World !\")";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Italic);
            this.label18.Location = new System.Drawing.Point(11, 316);
            this.label18.MaximumSize = new System.Drawing.Size(260, 0);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(155, 15);
            this.label18.TabIndex = 19;
            this.label18.Text = "myStrBldr.Append(\"Hello,\")";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Italic);
            this.label17.Location = new System.Drawing.Point(11, 301);
            this.label17.MaximumSize = new System.Drawing.Size(260, 0);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(216, 15);
            this.label17.TabIndex = 18;
            this.label17.Text = "myStrBldr=System.Text.StringBuilder()";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label16.Location = new System.Drawing.Point(8, 214);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(140, 13);
            this.label16.TabIndex = 17;
            this.label16.Text = "Using .NET 2.0 classes";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(8, 227);
            this.label15.MaximumSize = new System.Drawing.Size(260, 0);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(260, 65);
            this.label15.TabIndex = 16;
            this.label15.Text = resources.GetString("label15.Text");
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(7, 68);
            this.label14.MaximumSize = new System.Drawing.Size(260, 0);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(256, 26);
            this.label14.TabIndex = 15;
            this.label14.Text = "You can use these shortcut expressions and their DI API equivalents interchangeab" +
    "ly.";
            // 
            // lblObjectTypeName
            // 
            this.lblObjectTypeName.AutoSize = true;
            this.lblObjectTypeName.Location = new System.Drawing.Point(7, 121);
            this.lblObjectTypeName.MaximumSize = new System.Drawing.Size(0, 240);
            this.lblObjectTypeName.MinimumSize = new System.Drawing.Size(240, 0);
            this.lblObjectTypeName.Name = "lblObjectTypeName";
            this.lblObjectTypeName.Size = new System.Drawing.Size(240, 13);
            this.lblObjectTypeName.TabIndex = 14;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.Location = new System.Drawing.Point(7, 151);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(53, 13);
            this.label13.TabIndex = 13;
            this.label13.Text = "SBObob";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(8, 164);
            this.label12.MaximumSize = new System.Drawing.Size(260, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(260, 39);
            this.label12.TabIndex = 12;
            this.label12.Text = "You can access the SBObob object of the default session by the shortcut expressio" +
    "n BOB. For instance: BOB.GetItemList()";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(270, 68);
            this.label11.MaximumSize = new System.Drawing.Size(260, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(253, 26);
            this.label11.TabIndex = 11;
            this.label11.Text = "The shortcuts support three case variants: as in DI API (tYES), lowercase (tyes) " +
    "and uppercase (TYES).";
            // 
            // lstEnumValues
            // 
            this.lstEnumValues.FormattingEnabled = true;
            this.lstEnumValues.Location = new System.Drawing.Point(273, 194);
            this.lstEnumValues.Name = "lstEnumValues";
            this.lstEnumValues.Size = new System.Drawing.Size(259, 108);
            this.lstEnumValues.TabIndex = 10;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Location = new System.Drawing.Point(270, 151);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(156, 13);
            this.label10.TabIndex = 9;
            this.label10.Text = "DI API Enumeration Types";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(270, 26);
            this.label9.MaximumSize = new System.Drawing.Size(260, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(254, 39);
            this.label9.TabIndex = 8;
            this.label9.Text = "When you need enumeration values, you can either use these shortcut expressions o" +
    "r their DI API equivalents.";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 26);
            this.label8.MaximumSize = new System.Drawing.Size(260, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(260, 39);
            this.label8.TabIndex = 7;
            this.label8.Text = "This list contains all the valid objecttype shortcuts that can be used as targets" +
    " for the following functions: get(), add(), update(), delete(), close() and canc" +
    "el()";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(269, 4);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(133, 13);
            this.label7.TabIndex = 5;
            this.label7.Text = "Enumeration shortcuts";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(7, 4);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(82, 13);
            this.label6.TabIndex = 4;
            this.label6.Text = "Object Types";
            // 
            // cmbEnumInfo
            // 
            this.cmbEnumInfo.FormattingEnabled = true;
            this.cmbEnumInfo.Location = new System.Drawing.Point(273, 167);
            this.cmbEnumInfo.Name = "cmbEnumInfo";
            this.cmbEnumInfo.Size = new System.Drawing.Size(259, 21);
            this.cmbEnumInfo.TabIndex = 3;
            this.cmbEnumInfo.SelectedIndexChanged += new System.EventHandler(this.cmbEnumInfo_SelectedIndexChanged);
            // 
            // cmbRefEnum
            // 
            this.cmbRefEnum.FormattingEnabled = true;
            this.cmbRefEnum.Location = new System.Drawing.Point(272, 97);
            this.cmbRefEnum.Name = "cmbRefEnum";
            this.cmbRefEnum.Size = new System.Drawing.Size(259, 21);
            this.cmbRefEnum.TabIndex = 1;
            this.cmbRefEnum.SelectedIndexChanged += new System.EventHandler(this.cmbRefEnum_SelectedIndexChanged);
            // 
            // cmbRefObjects
            // 
            this.cmbRefObjects.FormattingEnabled = true;
            this.cmbRefObjects.Location = new System.Drawing.Point(6, 97);
            this.cmbRefObjects.Name = "cmbRefObjects";
            this.cmbRefObjects.Size = new System.Drawing.Size(259, 21);
            this.cmbRefObjects.TabIndex = 0;
            this.cmbRefObjects.SelectedIndexChanged += new System.EventHandler(this.cmbRefObjects_SelectedIndexChanged);
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Right;
            this.splitContainer2.Location = new System.Drawing.Point(614, 24);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.splitContainer2.Size = new System.Drawing.Size(1, 704);
            this.splitContainer2.SplitterDistance = 300;
            this.splitContainer2.TabIndex = 7;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(615, 24);
            this.menuStrip1.TabIndex = 8;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fToolStripMenuItem
            // 
            this.fToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.fToolStripMenuItem.Name = "fToolStripMenuItem";
            this.fToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fToolStripMenuItem.Text = "File";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(92, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.aboutToolStripMenuItem.Text = "About DI Commander";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(615, 750);
            this.Controls.Add(this.splitContainer2);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Name = "frmMain";
            this.Text = "DI Commander R2.0";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.tabCommandHost.ResumeLayout(false);
            this.tabShell.ResumeLayout(false);
            this.tabTrace.ResumeLayout(false);
            this.tabTrace.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.tabDefine.ResumeLayout(false);
            this.tabSessions.ResumeLayout(false);
            this.tabSessions.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.tabCode.ResumeLayout(false);
            this.tabCode.PerformLayout();
            this.toolStrip2.ResumeLayout(false);
            this.toolStrip2.PerformLayout();
            this.tabReference.ResumeLayout(false);
            this.tabReference.PerformLayout();
            this.splitContainer2.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TabControl tabDefine;
        private System.Windows.Forms.TabPage tabCode;
        private System.Windows.Forms.TabPage tabSessions;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label_licensesrv;
        private System.Windows.Forms.TextBox input_licensesrv;
        private System.Windows.Forms.TextBox input_serverpassword;
        private System.Windows.Forms.TextBox input_serveruserid;
        private System.Windows.Forms.TextBox input_server;
        private System.Windows.Forms.TextBox input_password;
        private System.Windows.Forms.TextBox input_username;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label_serveruserid;
        private System.Windows.Forms.ComboBox input_company;
        private System.Windows.Forms.Label label_Company;
        private System.Windows.Forms.Label label_server;
        private System.Windows.Forms.Label label_password;
        private System.Windows.Forms.Label label_username;
        private System.Windows.Forms.ListBox lstSessions;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox input_sessionhandle;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox input_forceDB;
        private System.Windows.Forms.CheckBox input_usetrusted;
        private System.Windows.Forms.Button button_refreshcompany;
        private System.Windows.Forms.ImageList img_Login;
        private System.Windows.Forms.Button btn_CloseSession;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.TabControl tabCommandHost;
        private System.Windows.Forms.TabPage tabShell;
        public UILibrary.ShellControl Shell;
        private System.Windows.Forms.TabPage tabTrace;
        private UILibrary.ShellControl TraceShell;
        private Fireball.Syntax.SyntaxDocument syntaxDocument1;
        private Fireball.Windows.Forms.CodeEditorControl codeEditor;
        private System.Windows.Forms.ToolStrip toolStrip2;
        private System.Windows.Forms.ToolStripButton btnExecute;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.Button btn_SetDefaultSession;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtDefaultSession;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ToolStripButton btnClear;
        private System.Windows.Forms.ToolStripButton btnCopyAll;
        private System.Windows.Forms.TabPage tabReference;
        private System.Windows.Forms.ComboBox cmbRefObjects;
        private System.Windows.Forms.ComboBox cmbRefEnum;
        private System.Windows.Forms.ComboBox cmbEnumInfo;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btnClearTraceWindow;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.ListBox lstEnumValues;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label lblObjectTypeName;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.ComboBox input_dbservertype;
    }
}

