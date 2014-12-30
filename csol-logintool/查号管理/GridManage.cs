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
using System.Net.Mail;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace 查号管理
{

    public struct userinfo
    {
        public string username;            //用户名
        public string password;            //密码
        public int failedcount;            //失败次数
        public string checktime;           //签到时间
        public string status;              //状态
        public int bocheck;                //是否成功签到
        public int checkedday;             //签到天数
        public string loginip;             //登陆机ip
        public string logincode;           //登陆代号
        public int chip_num;               //芯片数
        public int chest_num;              //宝箱数
        public Bitmap chip_bmp;            //芯片
        public Bitmap chest_bmp;           //宝箱
    }

    public partial class GridManage : Form
    {
        #region API
        [DllImport("user32.dll")]
        private static extern int RegisterHotKey(IntPtr hwnd, int id, int fsModifiers, int vk);
        [DllImport("user32.dll")]
        private static extern int UnregisterHotKey(IntPtr hwnd, int id);
        int Space = 32; //热键ID
        private const int WM_HOTKEY = 0x312; //窗口消息-热键
        private const int WM_CREATE = 0x1; //窗口消息-创建
        private const int WM_DESTROY = 0x2; //窗口消息-销毁
        private const int MOD_ALT = 0x1; //ALT
        private const int MOD_CONTROL = 0x2; //CTRL
        private const int MOD_SHIFT = 0x4; //SHIFT
        private const int VK_SPACE = 0x20; //SPACE
        private const int VK_F12 = 123;
        private const int VK_N = 78;


        /// <summary>
        /// 注册热键
        /// </summary>
        /// <param name="hwnd">窗口句柄</param>
        /// <param name="hotKey_id">热键ID</param>
        /// <param name="fsModifiers">组合键</param>
        /// <param name="vk">热键</param>
        private void RegKey(IntPtr hwnd, int hotKey_id, int fsModifiers, int vk)
        {
            bool result;
            if (RegisterHotKey(hwnd, hotKey_id, fsModifiers, vk) == 0)
            {
                result = false;
            }
            else
            {
                result = true;
            }
            if (!result)
            {
                MessageBox.Show("注册热键失败！");
            }
        }
        /// <summary>
        /// 注销热键
        /// </summary>
        /// <param name="hwnd">窗口句柄</param>
        /// <param name="hotKey_id">热键ID</param>
        private void UnRegKey(IntPtr hwnd, int hotKey_id)
        {
            UnregisterHotKey(hwnd, hotKey_id);
        }
        #endregion

        public GridManage()
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

            textBox_IP.Text = Sever.GetLocalIp();

            //Tag = Width.ToString() + "," + Height.ToString();
            //SizeChanged += Form1_SizeChanged;
            dgvUserData.DragDrop += dataGridView_DragDrop;
            dgvUserData.DragEnter += dataGridView_DragEnter;
            dgvUserData.SortCompare += dataGridView_SortCompare;
            dgvUserData.RowPostPaint += dgvData_RowPostPaint;
            dgvUserData.CellMouseDown +=new DataGridViewCellMouseEventHandler(dataGridView_CellMouseUp);
            dgvUserData.DataError +=new DataGridViewDataErrorEventHandler(dataGridView_DataError);

            Global.logger = new CLogger("manage_log/log", new ShowLog(ShowLogFunc));


            if (Directory.Exists("数字集"))
            {
                list_Shuzi.Clear();
                for (int i = 0; i <= 9; i++)
                {
                    Bitmap bmp = (Bitmap)Image.FromFile("数字集/" + "数字" + i + ".bmp");
                    list_Shuzi.Add(bmp);
                }
            }

            if (Directory.Exists("数字集"))
            {
                list_N.Clear();
                for (int i = 0; i <= 9; i++)
                {
                    Bitmap bmp = (Bitmap)Image.FromFile("数字集/" + "N" + i + ".bmp");
                    list_N.Add(bmp);
                }
            }
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            switch (m.Msg)
            {
                case WM_HOTKEY: //窗口消息-热键
                    switch (m.WParam.ToInt32())
                    {
                        //case 32: //热键ID
                        //    //PauseBtn_Click(null, null);
                        //    MessageBox.Show("热键触发成功");
                        //    break;
                       // default:
                        //    break;
                    }
                    break;
                case WM_CREATE: //窗口消息-创建
                    //RegKey(Handle, Space, MOD_CONTROL, VK_N); //注册热键
                    break;
                case WM_DESTROY: //窗口消息-销毁
                    UnRegKey(Handle, Space); //销毁热键
                    break;
                default:
                    break;
            }
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
                m_runIdx = 0;
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

        private void IniWriteValue(string Section, string Key, string Value)
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

                if (info.checkedday != 0)
                {
                    item.SetAttribute("签到天数", Convert.ToString(info.checkedday));
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

            doc.Save("Data.xml");

            Print("存档成功!!");

            m_boNeedSaveData = false;
            _lastSaveDataTime = DateTime.Now;
        }

        Dictionary<string, userinfo> m_userinfos = new Dictionary<string, userinfo>();
        Dictionary<string, userinfo> m_checkuserinfos;
        List<userinfo> m_userinfolist = new List<userinfo>();
        int m_runIdx = 0;

        private void LoadData()
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load("Data.xml");

                XmlNode root = doc.SelectSingleNode("Data");

                //for (XmlNode item = root.FirstChild; item != null; item = item.NextSibling)
                //{

                //    bool bDelete = true;
                //    int newIdx = dgvUserData.Rows.Add();
                //    for (int i = 0; i < dgvUserData.Columns.Count; ++i)
                //    {
                //        string HeaderText = dgvUserData.Columns[i].HeaderText;
                //        if (item.Attributes[HeaderText] != null)
                //        {
                //            string s = item.Attributes[HeaderText].Value;
                //            dgvUserData.Rows[newIdx].Cells[i].Value = s;
                //            bDelete = false;
                //        }
                //    }
                //    if (bDelete)
                //    {
                //        dgvUserData.Rows.Remove(dgvUserData.Rows[newIdx]);
                //    }
                //}

                //dgvUserData.AutoResizeColumns();
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

                    if (item.Attributes["状态"] != null)
                    {
                        info.status = item.Attributes["状态"].Value;
                    }

                    if (item.Attributes["签到"] != null)
                    {
                        info.bocheck = Convert.ToInt32(item.Attributes["签到"].Value);
                    }

                    if (item.Attributes["签到天数"] != null)
                    {
                        info.checkedday = Convert.ToInt32(item.Attributes["签到天数"].Value);
                    }

                    if (item.Attributes["登陆机IP"] != null)
                    {
                        info.loginip = item.Attributes["登陆机IP"].Value;
                    }

                    if (item.Attributes["登陆机代号"] != null)
                    {
                        info.logincode = item.Attributes["登陆机代号"].Value;
                    }

                    //if (File.Exists("芯片/" + info.username + ".bmp"))
                    //{
                    //    info.chip_bmp = (Bitmap)Image.FromFile("芯片/" + info.username + ".bmp");
                    //}

                    //if (File.Exists("密码箱/" + info.username + ".bmp"))
                    //{
                    //    info.chest_bmp = (Bitmap)Image.FromFile("密码箱/" + info.username + ".bmp");
                    //}

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

        List<Bitmap> list_N = new List<Bitmap>();
        List<Bitmap> list_Shuzi = new List<Bitmap>();

        struct NumXpoint
        {
            public int num;
            public int xpoint;
        };

        private int Analysis(Bitmap raw,List<Bitmap> bitmaps)
        {
            raw = new Bitmap(raw);
            List<NumXpoint> numbers = new List<NumXpoint>();
            for (int i = 0; i <= 9; ++i)
            {
                try
                {
                    bool isFind;
                    do
                    {
                        isFind = false;
                        Image<Bgr, Byte> img;
                        Image<Bgr, Byte> templ;

                        img = new Image<Bgr, Byte>(raw);
                        templ = new Image<Bgr, Byte>(bitmaps[i]);
                        TM_TYPE tmType = TM_TYPE.CV_TM_CCORR_NORMED;
                        Image<Gray, float> imageResult = img.MatchTemplate(templ, tmType);

                        double[] minValues, maxValues;
                        Point[] minLocations, maxLocations;
                        imageResult.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);
                        //对于平方差匹配和归一化平方差匹配，最小值表示最好的匹配；其他情况下，最大值表示最好的匹配            
                        for (int idx = 0; idx < maxValues.Length; ++idx)
                        {
                            if (maxValues[idx] > 0.99)
                            {
                                NumXpoint number = new NumXpoint();
                                number.num = i;
                                number.xpoint = maxLocations[idx].X;
                                numbers.Add(number);

                                int beginX = maxLocations[idx].X;
                                int beginY = maxLocations[idx].Y;
                                int endX = templ.Size.Width + beginX;
                                int endY = templ.Size.Height + beginY;
                                for (int x = beginX; x < endX; ++x)
                                {
                                    for (int y = beginY; y < endY; ++y)
                                    {
                                        raw.SetPixel(x, y, Color.Black);
                                    }
                                }

                                isFind = true;
                            }
                        }

                        img.Dispose();
                        templ.Dispose();
                        imageResult.Dispose();
                    } while (isFind);

                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }

            numbers.Sort(delegate(NumXpoint a, NumXpoint b)
            {
                return a.xpoint - b.xpoint;
            });

            int retNumber = 0;
            for (int i = 0; i < numbers.Count; ++i)
            {
                retNumber *= 10;
                retNumber += numbers[i].num;
            }

            return retNumber;
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
                this.BeginInvoke(new RefreshGrideView(refreshGridView),showtype);
            }
            else
            {
                tabControl1.SelectedIndex = 1;
                dgvUserData.Rows.Clear();

                List<userinfo> userlist = new List<userinfo>();
                List<userinfo> changes = new List<userinfo>();
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

                    userinfo newinfo = info;

                    bool change = false;
                    if (newinfo.chip_bmp == null && File.Exists("芯片/" + info.username + ".bmp"))
                    {
                        newinfo.chip_bmp = (Bitmap)Image.FromFile("芯片/" + newinfo.username + ".bmp");
                        newinfo.chip_num = Analysis(newinfo.chip_bmp,list_N);

                        change = true;
                    }

                    if (newinfo.chest_bmp == null && File.Exists("密码箱/" + info.username + ".bmp"))
                    {
                        newinfo.chest_bmp = (Bitmap)Image.FromFile("密码箱/" + newinfo.username + ".bmp");
                        newinfo.chest_num = Analysis(newinfo.chest_bmp,list_Shuzi);

                        change = true;
                    }
                    userlist.Add(newinfo);

                    if (change)
                    {
                        changes.Add(newinfo);
                    }
                }

                foreach (userinfo info in changes)
                {
                    m_userinfos[info.username] = info;
                }

                int newidx = 0;
                if (userlist.Count <=0)
                {
                    return;
                }
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

                    if (info.failedcount != null)
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

                    if (info.bocheck != 0)
                    {
                        dgvUserData.Rows[newidx].Cells["Checked"].Value = info.bocheck;
                    }

                    if (info.checkedday != 0)
                    {
                        dgvUserData.Rows[newidx].Cells["Days"].Value = info.checkedday;
                    }

                    if (info.loginip != null)
                    {
                        dgvUserData.Rows[newidx].Cells["IP"].Value = info.loginip;
                    }

                    if (info.logincode != null)
                    {
                        dgvUserData.Rows[newidx].Cells["Code"].Value = info.logincode;
                    }

                    //if (info.chip_num != 0)
                    {
                        dgvUserData.Rows[newidx].Cells[2].Value = info.chip_num;
                    }

                    //if (info.chest_num != 0)
                    {
                        dgvUserData.Rows[newidx].Cells[3].Value = info.chest_num;
                    }

                    if (info.chip_bmp != null)
                    {
                        dgvUserData.Rows[newidx].Cells[4].Value = info.chip_bmp;
                    }

                    if (info.chest_bmp != null)
                    {
                        dgvUserData.Rows[newidx].Cells[5].Value = info.chest_bmp;
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

        private void listenBtn_Click(object sender, EventArgs e)
        {
            try
            {
                Sever s = new Sever();
                s.BeginListen();
                Session.m_msgHandle = OnMsg;
                Session.m_logHandle = Print;
                Print("开始监听...");

                timer_StatusBarRefresh.Start();
                timer_FlushTextbox.Start();
                tmReboot.Start();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void SendSystemMail()
        {
            string from = "332916616@qq.com";
            string fromer = "发件人";
            string to = "648784195@qq.com";
            string toer = "收件人";
            string Subject = "邮件标题";
            string file = "";//"附件地址";
            string Body = "发送内容";
            string SMTPHost = "smtp.qq.com";
            string SMTPuser = "332916616@qq.com";
            string SMTPpass = "19881226tm";
            sendmail(from, fromer, to, toer, Subject, Body, file, SMTPHost, SMTPuser, SMTPpass);
        }

        /// <summary>
        /// C#发送邮件函数
        /// </summary>
        /// <param name="from">发送者邮箱</param>
        /// <param name="fromer">发送人</param>
        /// <param name="to">接受者邮箱</param>
        /// <param name="toer">收件人</param>
        /// <param name="Subject">主题</param>
        /// <param name="Body">内容</param>
        /// <param name="file">附件</param>
        /// <param name="SMTPHost">smtp服务器</param>
        /// <param name="SMTPuser">邮箱</param>
        /// <param name="SMTPpass">密码</param>

        /// <returns></returns>
        public bool sendmail(string sfrom, string sfromer, string sto, string stoer, string sSubject, string sBody, string sfile, string sSMTPHost, string sSMTPuser, string sSMTPpass)
        {
            ////设置from和to地址
            MailAddress from = new MailAddress(sfrom, sfromer);
            MailAddress to = new MailAddress(sto, stoer);

            ////创建一个MailMessage对象
            MailMessage oMail = new MailMessage(from, to);

            //// 添加附件
            if (sfile != "")
            {
                oMail.Attachments.Add(new Attachment(sfile));
            }



            ////邮件标题
            oMail.Subject = sSubject;


            ////邮件内容
            oMail.Body = sBody;

            ////邮件格式
            oMail.IsBodyHtml = false;

            ////邮件采用的编码
            oMail.BodyEncoding = System.Text.Encoding.GetEncoding("GB2312");

            ////设置邮件的优先级为高
            oMail.Priority = MailPriority.High;

            ////发送邮件
            SmtpClient client = new SmtpClient();
            ////client.UseDefaultCredentials = false; 
            client.Host = sSMTPHost;
            client.Credentials = new NetworkCredential(sSMTPuser, sSMTPpass);
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            try
            {
                client.Send(oMail);
                return true;
            }
            catch (Exception err)
            {
                Global.logger.Debug(err.Message.ToString());
                return false;
            }
            finally
            {
                ////释放资源
                oMail.Dispose();
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

        private void OnMsg_safe(string s,Session c)
        {
            try
            {
                
                Print("Recv:" + s);

                string[] split = s.Split(new char[] { '$' }, StringSplitOptions.RemoveEmptyEntries);
                switch (split[0])
                {
                    case "100":
                        {
                            string mac = split[1];
                            string code = split[2];

                            c.m_code = code;
                            c.m_mac = mac;

                            bool x = false;
                            foreach (DataGridViewRow row  in dgvSession.Rows)
                            {
                                if (row.IsNewRow) continue;
                                string rMac = (string)row.Cells["sMac"].Value;
                                if (rMac == mac)
                                {
                                    row.Cells["sCode"].Value = code;
                                    row.Cells["sIP"].Value = c.handle.RemoteEndPoint.ToString();
                                    x = true;
                                }
                            }

                            if (x == false)
                            {
                                dgvSession.Rows.Add(c.handle.RemoteEndPoint.ToString(), code, mac, "已连接",null ,null, DateTime.Now.ToLocalTime(), "断开","重启");
                            }
                        }break;
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
                                switch(_working_state)
                                {
                                    case working_state.normal:
                                        {
                                            int nLoop;

                                            // 根据选项优先分配失败1次的的账号
                                            nLoop = 0;
                                            foreach (var info in m_checkuserinfos.Values)
                                            {
                                                if (nLoop++ > 50) break;
                                                if (info.bocheck == 0 && info.status == "签到失败" && info.failedcount <= 1 && !m_Token.ContainsValue(info.username))
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
                                                    if (nLoop++ > 50) break;
                                                    if (info.bocheck == 0 && (info.status == null || info.status == "") && !m_Token.ContainsValue(info.username))
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
                                                    if (nLoop++ > 50) break;
                                                    if (info.bocheck == 0 && info.status == "签到失败" && !m_Token.ContainsValue(info.username))
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
                                        }break;
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
                                        }break;
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
                                c.Send("2$" + accName + "$" + passWord);
                            }

                        } break;
                    case "3":
                        {
                            string accName = split[1];
                            string isOk = split[2];
                            string b1 = null, b2 = null;
                            Bitmap img1 = null;
                            Bitmap img2 = null;
                            if (split.Length == 5)
                            {
                                b1 = split[3];
                                b2 = split[4];

                                if (b1 != null)
                                {
                                    byte[] imageBytes = Convert.FromBase64String(b1);
                                    //读入MemoryStream对象
                                    MemoryStream memoryStream = new MemoryStream(imageBytes, 0, imageBytes.Length);
                                    //转成图片
                                    Bitmap bmp = new Bitmap(memoryStream);
                                    img1 = new Bitmap(bmp.Size.Width,bmp.Size.Height);
                                    Graphics draw = Graphics.FromImage(img1);
                                    draw.DrawImage(bmp, 0, 0);

                                    draw.Dispose();
                                    memoryStream.Close();
                                } while (false) ;

                                if (b2 != null)
                                {
                                    byte[] imageBytes = Convert.FromBase64String(b2);
                                    //读入MemoryStream对象
                                    MemoryStream memoryStream = new MemoryStream(imageBytes, 0, imageBytes.Length);
                                    //转成图片
                                    Bitmap bmp = new Bitmap(memoryStream);
                                    img2 = new Bitmap(bmp.Size.Width, bmp.Size.Height);
                                    Graphics draw = Graphics.FromImage(img2);
                                    draw.DrawImage(bmp, 0, 0);

                                    draw.Dispose();
                                    memoryStream.Close();
                                }
                            }

                            if (m_accountRecord.ContainsKey(accName))
                            {
                                long recordTime = m_accountRecord[accName];
                                if (DateTime.Now.Ticks - recordTime < 10)
                                {
                                    return;
                                }
                            }

                            m_accountRecord[accName]=DateTime.Now.Ticks;

                            if (isOk == "OK")
                            {
                                if (m_checkuserinfos.ContainsKey(accName) && m_userinfos.ContainsKey(accName))
                                {
                                    userinfo info = m_checkuserinfos[accName];
                                    info.bocheck = 1;
                                    info.checktime = DateTime.Now.ToString();
                                    info.failedcount = 0;
                                    info.status = "签到完成";

                                    info.chip_bmp = img1;
                                    info.chest_bmp = img2;

                                    if (!Directory.Exists("芯片"))
                                    {
                                        Directory.CreateDirectory("芯片");
                                    }
                                    if (!Directory.Exists("密码箱"))
                                    {
                                        Directory.CreateDirectory("密码箱");
                                    }
                                    img1.Save(@"./芯片/" + accName + @".bmp");
                                    img2.Save(@"./密码箱/" + accName + @".bmp");

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
                                        info.checkedday,
                                        info.loginip,
                                        info.logincode,
                                        img1,
                                        img2
                                        );
                                }
                            }

                            sbTotalCount.Text = "进度:" + (m_userinfos.Count - m_checkuserinfos.Count) + "/" + m_userinfos.Count;

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
                        }break;
                    case "5":
                        {
                            string accName = split[1];
                            string Day = split[2];
                            if (m_checkuserinfos.ContainsKey(accName))
                            {
                                userinfo info = m_checkuserinfos[accName];
                                info.checkedday = Convert.ToInt32(Day);
                                m_checkuserinfos[accName] = info;
                            }

                            if (m_userinfos.ContainsKey(accName))
                            {
                                userinfo info = m_userinfos[accName];
                                info.checkedday = Convert.ToInt32(Day);
                                m_userinfos[accName] = info;
                            }                 
                        }break; 
                    case "6":
                        {
                            //遇到验证码
                            //给路由发送命令重启

                            if (cbRoutineIp.Checked)
                            {
                                if (Environment.TickCount - lastRountineChangeTime > 60 * 1000)
                                {
                                    Global.logger.Debug("登陆机遇到验证码,尝试换ip");
                                    lastRountineChangeTime = Environment.TickCount;
                                    ChangeRoutineIp();
                                }
                            }
                        }break;

                }
            }
            catch (System.InvalidCastException)
            {

            }
            catch (System.Exception ex)
            {
                Print(ex.ToString());
            }
        }

        int lastRountineChangeTime = 0;

        private void ShowLogFunc(CLogger.eLoggerLevel c, string s)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new ShowLog(ShowLogFunc), c, s);
            }
            else
            {
                if (lbLog.Items.Count > 300)
                {
                    lbLog.Items.Clear();
                }
                if (s.Length > 400)
                {
                    s = s.Substring(0, 100) + "...";
                }
                lbLog.Items.Add(s);
            }
        }

        private void Print(string s)
        {
            Global.logger.Debug(s);
        }

        Dictionary<string, string> m_Token = new Dictionary<string, string>();
        static string logFileName = String.Format("{0:yyyyMMdd_HHmmss}.txt", DateTime.Now);
        string m_textBoxBuffer = "";

        private void button1_Click(object sender, EventArgs e)
        {
            lbLog.Items.Clear();
        }

        private DateTime _lastClearDateTime = DateTime.Now;

        private void timer_StatusBarRefresh_Tick(object sender, EventArgs e)
        {
            lock(Sever.m_Clinets)
            {
                for (int i = 0; i < Sever.m_Clinets.Count; ++i)
                {
                    Session s = Sever.m_Clinets[i];
                    s.Send("0");
                }

                if (Sever.bChanged)
                {
                    Sever.bChanged = false;

                    StatusLab_SessionNum.Text = "当前连接:" + Sever.m_Clinets.Count;

                    foreach (DataGridViewRow row in dgvSession.Rows)
                    {
                        if (row.IsNewRow) 
                            continue;

                        bool x = false;
                        for (int i = 0; i < Sever.m_Clinets.Count; ++i)
                        {
                            if (Sever.m_Clinets[i].m_mac == row.Cells["sMac"].Value.ToString())
                            {
                                row.Cells["sState"].Value = "良好";
                                x = true;
                            }
                        }

                        if (x == false)
                        {
                            row.Cells["sState"].Value = "已断开";
                        }
                    }
                }
            }

            if (this.cbClearData.Checked == true)
            {
                if (DateTime.Now.Hour == 6 && DateTime.Now.Minute == 0)
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
                (DateTime.Now - _lastSaveDataTime).Seconds > 30)
            {
                SaveData();
            }
        }

        private DateTime _lastSaveDataTime = DateTime.Now;
        private  bool m_boNeedSaveData = false;

        private void timer_FlushTextbox_Tick(object sender, EventArgs e)
        {
            //if (m_textBoxBuffer.Length > 0)
           // {
                //FlushToTextBox();
            //}
        }

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

                            if (row.Cells[i].Value != null && dgvUserData.Columns[i].CellType.ToString().IndexOf("DataGridViewTextBoxCell") >= 0)
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

                    //Stream file = dlg.OpenFile();
                    //StreamWriter write = new StreamWriter(file);

                    //string fileTxt = "";
                    //foreach (userinfo info in m_userinfos.Values)
                    //{
                    //    fileTxt += info.username;

                    //    fileTxt += "----";
                    //    fileTxt += info.password == null ? "(NULL)" : info.password;

                    //    fileTxt += "----";
                    //    fileTxt += info.failedcount;

                    //    fileTxt += "----";
                    //    fileTxt += info.checktime == null ? "(NULL)" : info.checktime;

                    //    fileTxt += "----";
                    //    fileTxt += info.checktime == null ? "(NULL)" : info.checktime;

                    //    write.WriteLine(fileTxt);

                    //    fileTxt = "";
                    //}

                    write.Flush();
                    write.Close();
                    file.Close();

                    //Stream file = dlg.OpenFile();
                    //StreamWriter write = new StreamWriter(file);
                    //byte[] bytes = System.Text.Encoding.Default.GetBytes(fileTxt);
                    //file.Write(bytes, 0, bytes.Length);
                    //file.Close();
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

            sbTotalCount.Text = "进度:" + (m_userinfos.Count - m_checkuserinfos.Count) + "/" + m_userinfos.Count;
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
                                        if (s.handle.RemoteEndPoint.ToString() == remotePoint)
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
                SaveData();
            base.OnClosing(e);
            Global.logger.Stop();
        }


        private void btnRoutineIp_Click(object sender, EventArgs e)
        {
            ChangeRoutineIp();
        }

        private void ChangeRoutineIp()
        {
            BeginInvoke(new ChangeRoutineIp(RoutineIp_1));
        }

        private void RoutineIp_1()
        {
            do
            {
                byte[] recv = new byte[10024];

                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect("192.168.0.1", 80);
                socket.Send(loginData);

                socket.Receive(recv);
                socket.Receive(recv);
            } while (false);

            do
            {
                byte[] recv = new byte[10024];

                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect("192.168.0.1", 80);
                socket.Send(disconnectData);

                socket.Receive(recv);
                socket.Receive(recv);
            } while (false);

            Thread.Sleep(10000);

            do
            {
                byte[] recv = new byte[10024];

                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect("192.168.0.1", 80);
                socket.Send(loginData);

                socket.Receive(recv);
                socket.Receive(recv);
            } while (false);

            do
            {
                byte[] recv = new byte[10024];

                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect("192.168.0.1", 80);
                socket.Send(connectData);

                socket.Receive(recv);
                socket.Receive(recv);

            } while (false); 
        }


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

        private void lbLog_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListBox lb = (ListBox)sender;
            if (lb != null && lb.SelectedItem != null)
            {
                Clipboard.SetDataObject(lb.SelectedItem.ToString());
            }
        }
    }

    delegate void ChangeRoutineIp();
    delegate void Delegate_Flush();
    delegate void RefreshGrideView(GridManage.ShowType showtype);
    delegate void Delegate_Print(string s);
    delegate void Delegate<T>(T t); 
    delegate void Delegate<T1,T2>(T1 t1,T2 t2);
}
