using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using IronPython.Hosting;
using SAPbobsCOM;
using Fireball.CodeEditor;
using Fireball.CodeEditor.SyntaxFiles;
using Fireball.Syntax;
using Fireball.Windows.Forms;

namespace DIC
{
    public partial class frmMain : Form
    {
        public static PythonEngine engine;

        Stream outstream;
        StringBuilder statementBuilder = new StringBuilder();
        System.Collections.Generic.List<string> B1EnumClassList = new List<string>();
        Dictionary<string, object> B1EnumList = new Dictionary<string, object>();
        Dictionary<string, object> B1EnumValues = new Dictionary<string, object>();
        Dictionary<string, object> B1ObjectList = new Dictionary<string, object>();

        public frmMain()
        {
            InitializeComponent();
            //DirectoryInfo di = new DirectoryInfo();
            outstream = new MyStream(Shell);
            Shell.CommandEntered += new UILibrary.EventCommandEntered(Shell_CommandEntered);
            engine = new PythonEngine();
            engine.AddToPath(Environment.CurrentDirectory);
            string exepath = Environment.CurrentDirectory;
            engine.Import("clr");
            engine.SetStandardOutput(outstream);
            engine.SetStandardError(new MyStream(Shell));
            engine.Execute("import clr;clr.AddReferenceToFile(\"Interop.SAPbobsCOM.dll\")");
            engine.Execute("import SAPbobsCOM;import System;import System.IO;import System.Text;import System.Collections");

            IronPython.Runtime.List objecttypes = engine.EvaluateAs<IronPython.Runtime.List>("dir(SAPbobsCOM.BoObjectTypes)");
            foreach (string otype in objecttypes)
            {
                if (otype.StartsWith("o"))
                {
                    string handle;
                    int endcut = 2;
                    if (otype.ToUpper().EndsWith("US"))
                    {
                        endcut = 1;
                    }
                    else if (otype.ToUpper().EndsWith("HES"))
                    {
                        endcut = 3;
                    }
                    else if (otype.ToUpper().EndsWith("S"))
                    {
                        endcut = 2;
                    }
                    else
                    {
                        endcut = 1;
                    }
                    handle = otype.Substring(1, otype.Length - endcut).ToUpper();
                    if (handle.EndsWith("IE"))
                    {
                        handle = handle.Substring(0, handle.Length - 2) + "Y";
                    }
                    engine.Globals[handle] = engine.EvaluateAs<BoObjectTypes>("SAPbobsCOM.BoObjectTypes." + otype);
                    B1ObjectList.Add(handle, engine.Globals[handle]);
                }
            }

            System.Collections.Generic.List<string> enums = new System.Collections.Generic.List<string>();

            Assembly[] domainAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly ass in domainAssemblies)
            {
                if (ass.FullName.Contains("SAPbobsCOM"))
                {
                    Type[] types = ass.GetTypes();
                    foreach (Type type in types)
                    {
                        if (type.IsEnum)
                        {
                            string[] enumnames = System.Enum.GetNames(type);
                            Array enumvalues = System.Enum.GetValues(type);
                            for (int enumindex = 0; enumindex < enumnames.Length; enumindex++)
                            {
                                engine.Globals[enumnames[enumindex]] = enumvalues.GetValue(enumindex);
                                if (!enumnames[enumindex].StartsWith("o"))
                                {
                                    B1EnumList.Add(enumnames[enumindex], enumvalues.GetValue(enumindex));
                                    B1EnumValues.Add(enumnames[enumindex], enumvalues.GetValue(enumindex).GetType().Name);
                                    Type enumclass = enumvalues.GetValue(enumindex).GetType();
                                    if (!B1EnumClassList.Contains(enumclass.Name))
                                    {
                                        B1EnumClassList.Add(enumclass.Name);
                                    }
                                }
                                engine.Globals[enumnames[enumindex].ToLower()] = enumvalues.GetValue(enumindex);
                                engine.Globals[enumnames[enumindex].ToUpper()] = enumvalues.GetValue(enumindex);
                                enums.Add(type.Name + ":" + enumvalues.GetValue(enumindex));
                            }
                        }
                    }
                }
            }
            engine.Globals["enumvalues"] = enums;

            foreach (string key in B1ObjectList.Keys)
            {
                this.cmbRefObjects.Items.Add(key);
            }
            foreach (string key in B1EnumList.Keys)
            {
                this.cmbRefEnum.Items.Add(key);
            }
            foreach (string key in B1EnumClassList)
            {
                this.cmbEnumInfo.Items.Add(key);
            }
            SyntaxDocument doc = new SyntaxDocument();
            doc.SyntaxFile = "Python.syn";
            this.codeEditor.Document = doc;
        }

        void Shell_CommandEntered(object sender, UILibrary.CommandEnteredEventArgs e)
        {
            try
            {
                string input = e.Command;
                ProcessCommands(input, false);
            }
            catch (Exception ex)
            {
                Shell.WriteText("An application-level error occurred. See the trace tab for details.");
                TraceShell.WriteText(ex.ToString());
            }
        }

        private void ProcessCommands(string input, bool echo)
        {
            string[] seps ={ "\r\n" };
            //Split the text received by carriage return/linefeed. Notice: commands separated by semicolons will be processed in one lump.
            string[] commands = input.Split(seps, StringSplitOptions.None);
            System.Collections.Generic.List<string> statements = new List<string>();
            bool multiline = false;
            string statement = null;
            statementBuilder = new StringBuilder();

            foreach (string command in commands)
            {
                if (!ProcessInternalCommand(command))
                {
                    if (command.Trim().Length > 0 && command.Trim().Substring(0, 1).Equals("#"))
                    {
                        this.Shell.WriteText(command);
                        this.Shell.PrintPrompt();
                        continue;
                    }
                    if (multiline && statementBuilder.Length > 0 && command.Trim().Equals(""))
                    {
                        multiline = false;
                    }
                    else
                    {
                        if (command.Length > 0)
                        {
                            statementBuilder.Append(command + seps[0]);
                            if (command.Trim().EndsWith(":"))
                            {
                                multiline = true;
                            }
                        }
                    }

                    if (!multiline)
                    {
                        statement = statementBuilder.ToString();
                        statementBuilder = new StringBuilder();
                        if (statement != null && statement.Length > 0 && !ProcessInternalCommand(statement))
                        {
                            try
                            {
                                if (echo)
                                {
                                    this.Shell.WriteText(statement);
                                }
                                else
                                {
                                    this.Shell.WriteText(System.Environment.NewLine);
                                }
                                engine.ExecuteToConsole(statement);
                                if (echo)
                                {
                                    this.Shell.PrintPrompt();
                                }
                            }
                            catch (IronPython.Runtime.Exceptions.PythonNameErrorException exc)
                            {
                                Shell.WriteText("Invalid name: " + exc.Message + "\r\n");
                            }
                            catch (IronPython.Runtime.Exceptions.ArgumentTypeException aex)
                            {
                                Shell.WriteText("Invalid argument: " + aex.Message + "\r\n");
                            }
                            catch (IronPython.Runtime.Exceptions.PythonSyntaxErrorException sex)
                            {
                                Shell.WriteText("Syntax error: " + sex.Message + "\r\n");
                            }
                            catch (System.Runtime.InteropServices.InvalidComObjectException icex)
                            {
                                Shell.WriteText("COM object " + icex.Source + " is invalid (already released?)" + icex.Message);
                            }
                            catch (System.ArgumentException aex)
                            {
                                Shell.WriteText("Argument exception:" + aex.Message);
                            }
                            catch (System.Runtime.InteropServices.COMException coex)
                            {
                                Shell.WriteText("A COMException occurred:" + coex.Message);
                            }
                            catch (Exception exc)
                            {
                                Shell.WriteText("A general error occurred. See the trace tab for details." + "\r\n");
                                TraceShell.WriteText(exc.ToString());
                            }
                        }
                    }
                }
            }
        }

        private bool ProcessInternalCommand(string command)
        {
            string originalcommand = command;
            string definition = "";
            if (command.Contains("="))
            {
                command = originalcommand.Substring(originalcommand.IndexOf("=") + 1);
                definition = originalcommand.Substring(0, originalcommand.IndexOf("=") + 1);
            }
            char[] seps = new char[] { ' ' };
            string[] tokens = command.Split(seps);
            switch (tokens[0].ToLower())
            {
                case "cls":
                    this.Shell.Clear();
                    return true;                    
                default:
                    return false;                    
            }            
        }

        private void input_usetrusted_CheckedChanged(object sender, System.EventArgs e)
        {
            input_serveruserid.Enabled = input_usetrusted.CheckState == CheckState.Unchecked;
            input_serverpassword.Enabled = input_usetrusted.CheckState == CheckState.Unchecked;
            input_licensesrv.Enabled = input_usetrusted.CheckState == CheckState.Unchecked;
        }

        private CompanyClass SetLoginInfo()
        {
            CompanyClass session = new CompanyClass();
            session.Server = this.input_server.Text;
            session.UserName = this.input_username.Text;
            session.Password = this.input_password.Text;
            session.DbUserName = this.input_serveruserid.Text;
            session.DbPassword = this.input_serverpassword.Text;
            session.UseTrusted = this.input_usetrusted.Checked;
            session.LicenseServer = this.input_licensesrv.Text;
            session.language = BoSuppLangs.ln_English;
            
            switch ((string)this.input_dbservertype.SelectedItem)
            {
                case "MSSQL2005":
                    session.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_MSSQL2005;
                    break;
                case "MSSQL":
                    session.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_MSSQL;
                    break;
                case "DB2":
                    session.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_DB_2;
                    break;
                case "SYBASE":
                    session.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_SYBASE;
                    break;
                default:
                    session.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_MSSQL2005;
                    break;

            }
            return session;
        }

        private void button_refreshcompany_Click(object sender, EventArgs e)
        {
            this.input_forceDB.Checked = false;

            CompanyClass db = new CompanyClass();
            db.Server = this.input_server.Text;            
            db.DbUserName = this.input_serveruserid.Text;
            db.DbPassword = this.input_serverpassword.Text;
            db.UseTrusted = this.input_usetrusted.Checked;            
            db.language = BoSuppLangs.ln_English;
            Recordset rs;
            try
            {
                rs = db.GetCompanyList();

                DataTable myTable = new DataTable("companies");
                DataColumn dbItem = new DataColumn("dbName", Type.GetType("System.String"));
                DataColumn cmpItem = new DataColumn("cmpName", Type.GetType("System.String"));
                DataColumn versItem = new DataColumn("versStr", Type.GetType("System.String"));

                myTable.Columns.Add(dbItem);
                myTable.Columns.Add(cmpItem);
                myTable.Columns.Add(versItem);
                DataView firstView = new DataView(myTable);

                int companyCount = rs.RecordCount;
                rs.MoveFirst();
                DataRow NewRow;
                for (int c = 0; c < companyCount; c++)
                {
                    NewRow = myTable.NewRow();
                    NewRow["dbName"] = rs.Fields.Item("dbName").Value;
                    NewRow["cmpName"] = rs.Fields.Item("cmpName").Value;
                    NewRow["versStr"] = rs.Fields.Item("versStr").Value;
                    myTable.Rows.Add(NewRow);
                    rs.MoveNext();
                }
                this.input_company.DataSource = firstView;
                this.input_company.DisplayMember = "cmpName";
                this.input_company.ValueMember = "dbName";
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Failed to get company list (perhaps you should try DB direct):" + ex.Message);
                return;
            }
        }

        private void button_Login_Click(object sender, EventArgs e)
        {
            try
            {
                DICEngine b1tch = DICEngine.GetInstance();
                string selectedCompany = "";
                try
                {
                    if (input_forceDB.Checked)
                    {
                        selectedCompany = this.input_company.Text;
                    }
                    else
                    {
                        selectedCompany = this.input_company.SelectedValue.ToString();
                    }

                    if (this.lstSessions.Items.Contains(this.input_sessionhandle.Text))
                    {
                        MessageBox.Show("This handle is already in use. Please select another.");
                        return;
                    }

                    CompanyClass c = SetLoginInfo();

                    c.CompanyDB = selectedCompany;
                    if (!c.UseTrusted)
                    {
                        c.DbUserName = input_serveruserid.Text;
                        c.DbPassword = input_serverpassword.Text;
                        c.LicenseServer = input_licensesrv.Text;
                    }

                    int flag = c.Connect();
                    if (flag == 0)
                    {
                        SessionContainer sc = new SessionContainer();
                        sc.Handle = this.input_sessionhandle.Text;
                        sc.Session = c;
                        b1tch.Sessions.Add(sc);
                        if (b1tch.DefaultSession == null || (b1tch.DefaultSession != null && !b1tch.Sessions.Contains(b1tch.DefaultSession.Handle)))
                        {
                            b1tch.DefaultSession = sc;
                            txtDefaultSession.Text = sc.Handle;
                        }
                        engine.Globals[sc.Handle] = sc.Session;
                        RefreshSessionList();
                        tabCode.Select();
                        tabDefine.TabPages[1].Select();
                        SetBOB(b1tch);
                    }
                    else
                    {
                        MessageBox.Show("Login failed:" + c.GetLastErrorDescription() + "(" + c.GetLastErrorCode() + ")");
                    }
                }
                catch (System.NullReferenceException)
                {
                    MessageBox.Show("Please select a company first");
                    return;
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void RefreshSessionList()
        {
            lstSessions.DataSource = null;
            lstSessions.DataSource = DICEngine.GetInstance().Sessions;
            lstSessions.Refresh();
        }

        private void TraceShell_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (codeEditor.Selection.SelLength == 0)
                {
                    codeEditor.SelectAll();
                }
                ProcessCommands(codeEditor.Selection.Text + "\r\n", true);
                codeEditor.Selection.ClearSelection();                
            }
            catch (Exception ex)
            {
                Shell.WriteText("Error executing batch. See trace tab for details.");
                TraceShell.WriteText(ex.Message);
            }
        }

        private void btnDirectExecute_Click(object sender, EventArgs e)
        {
            try
            {
                engine.ExecuteToConsole(this.codeEditor.Selection.Text);
            }
            catch (Exception ex)
            {
                Shell.WriteText("Error executing batch. See trace tab for details.");
                TraceShell.WriteText(ex.Message);
            }
        }

        private void input_forceDB_CheckedChanged(object sender, EventArgs e)
        {
            input_company.AutoCompleteMode = input_forceDB.Checked ? AutoCompleteMode.None : AutoCompleteMode.None;
            if (input_forceDB.Checked)
            {
                input_company.DataSource = null;
                input_company.Items.Clear();
            }
        }

        private void btn_CloseSession_Click(object sender, EventArgs e)
        {
            int sessionindex = this.lstSessions.SelectedIndex;
            if (sessionindex >= 0)
            {
                DICEngine b1tch = DICEngine.GetInstance();
                SessionContainer sc = b1tch.Sessions[sessionindex] as SessionContainer;
                bool isdefault = sc.Equals(b1tch.DefaultSession.Handle);
                if (sc != null && sc.Session != null && sc.Session.Connected)
                {
                    sc.Session.Disconnect();
                }
                engine.Globals.Remove(sc.Handle);
                b1tch.Sessions.Remove(sc.Handle);
                if (isdefault)
                {
                    b1tch.DefaultSession = null;
                    b1tch.Sessions.TrimToSize();
                    if (b1tch.Sessions.Count > 0)
                    {
                        b1tch.DefaultSession = b1tch.Sessions[0] as SessionContainer;
                        txtDefaultSession.Text = b1tch.DefaultSession.Handle;
                    }
                    else
                    {
                        txtDefaultSession.Text = "";
                    }
                }
                RefreshSessionList();

            }
            else
            {
                MessageBox.Show("Please choose a session first!");
            }
        }

        private void lstSessions_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool sessionselected = IsSessionSelected();
            btn_CloseSession.Enabled = sessionselected;
            btn_SetDefaultSession.Enabled = sessionselected;
        }

        private bool IsSessionSelected()
        {
            bool hassessions = lstSessions.Items.Count > 0;
            bool sessionselected = lstSessions.SelectedIndex >= 0;
            return hassessions && sessionselected;
        }

        private void btn_SetDefaultSession_Click(object sender, EventArgs e)
        {
            if (IsSessionSelected())
            {
                DICEngine b1tch = DICEngine.GetInstance();
                SessionContainer sc = b1tch.Sessions[lstSessions.SelectedIndex] as SessionContainer;
                b1tch.DefaultSession = sc;
                txtDefaultSession.Text = sc.Handle;
                SetBOB(b1tch);
            }
            else
            {
                MessageBox.Show("Please select a session first!");
            }
        }

        private static void SetBOB(DICEngine b1tch)
        {
            if (b1tch.DefaultSession != null && b1tch.DefaultSession.Session != null && b1tch.DefaultSession.Session.Connected)
            {
                engine.Globals["BOB"] = b1tch.DefaultSession.Session.GetBusinessObject(BoObjectTypes.BoBridge);
            }
            else
            {
                engine.Globals["BOB"] = null;
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DICEngine b1tch = DICEngine.GetInstance();
            foreach (SessionContainer sc in b1tch.Sessions)
            {
                if (sc != null && sc.Session != null && sc.Session.Connected)
                {
                    sc.Session.Disconnect();
                }
            }
            this.Close();
            this.Dispose();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmAbout fa = new frmAbout();
            fa.Show();
        }

        private void Shell_Load(object sender, EventArgs e)
        {

        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            this.codeEditor.SelectAll();
            this.codeEditor.Delete();
        }

        private void btnCopyAll_Click(object sender, EventArgs e)
        {
            this.codeEditor.SelectAll();
            this.codeEditor.Copy();
        }

        private void cmbRefEnum_SelectedIndexChanged(object sender, EventArgs e)
        {
            object retrieved = null;
            B1EnumList.TryGetValue(this.cmbRefEnum.Text, out retrieved);
            if (retrieved != null)
            {
                this.cmbEnumInfo.Text = retrieved.GetType().Name;
            }
        }

        private void btnClearTraceWindow_Click(object sender, EventArgs e)
        {
            TraceShell.Clear();
            TraceShell.Prompt = "!>";
        }

        private void cmbEnumInfo_SelectedIndexChanged(object sender, EventArgs e)
        {
            string retrieved = this.cmbEnumInfo.Text;
            lstEnumValues.Items.Clear();
            foreach (string key in B1EnumValues.Keys)
            {
                if (B1EnumValues[key].ToString().Equals(retrieved))
                {
                    this.lstEnumValues.Items.Add(key);
                }
            }
        }

        private void cmbRefObjects_SelectedIndexChanged(object sender, EventArgs e)
        {
            lblObjectTypeName.Text = "= SAPbobsCOM.BoObjectTypes." + B1ObjectList[cmbRefObjects.Text];
        }
    }

    class MyStream : Stream
    {
        private UILibrary.ShellControl shell;

        public MyStream(UILibrary.ShellControl _shell)
        {
            shell = _shell;
        }
        #region unsupported Read + Seek members
        public override bool CanRead
        {
            get { return false; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override void Flush()
        {
            // nop
        }

        public override long Length
        {
            get { throw new NotSupportedException("Seek not supported"); } // can't seek 
        }

        public override long Position
        {
            get
            {
                throw new NotSupportedException("Seek not supported");  // can't seek 
            }
            set
            {
                throw new NotSupportedException("Seek not supported");  // can't seek 
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException("Reed not supported"); // can't read
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException("Seek not supported"); // can't seek
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException("Seek not supported"); // can't seek
        }
        #endregion

        //private StringBuilder sb = new StringBuilder();

        public override void Write(byte[] buffer, int offset, int count)
        {
            // Very bad hack: Ignore single newline char. This is because we expect the newline is following
            // previous content and we already placed a newline on that.
            if (count == 1 && buffer[offset] == '\n')
                return;
            // Code update from ShawnFa to fix case for '\r'

            var sb = new StringBuilder();
            while (count > 0)
            {
                var ch = (char)buffer[offset];
                if (ch == '\n')
                {
                    sb.Length = 0; // reset.                
                }
                else if (ch != '\r')
                {
                    sb.Append(ch);
                }
                offset++;
                count--;
            }

            // Dump remainder. @todo - need some sort of "Write" to avoid adding extra newline.
            if (sb.Length > 0)
                shell.WriteText(sb.ToString());
        }
    }
}