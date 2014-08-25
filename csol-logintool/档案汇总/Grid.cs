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

namespace 档案汇总
{
    public partial class Grid : Form
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

            textBox_IP.Text = Sever.GetLocalIp();

            //Tag = Width.ToString() + "," + Height.ToString();
            //SizeChanged += Form1_SizeChanged;
            dgvUserData.DragDrop += dataGridView_DragDrop;
            dgvUserData.DragEnter += dataGridView_DragEnter;
            dgvUserData.SortCompare += dataGridView_SortCompare;
            dgvUserData.RowPostPaint += dgvData_RowPostPaint;
            dgvUserData.CellMouseDown +=new DataGridViewCellMouseEventHandler(dataGridView_CellMouseUp);
            dgvUserData.DataError +=new DataGridViewDataErrorEventHandler(dataGridView_DataError);
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            switch (m.Msg)
            {
                case WM_HOTKEY: //窗口消息-热键
                    switch (m.WParam.ToInt32())
                    {
                        case 32: //热键ID
                            //PauseBtn_Click(null, null);
                            MessageBox.Show("热键触发成功");
                            break;
                        default:
                            break;
                    }
                    break;
                case WM_CREATE: //窗口消息-创建
                    RegKey(Handle, Space, MOD_CONTROL, VK_N); //注册热键
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
            //string[] tmp = ((Form)sender).Tag.ToString().Split(',');
            //float width = (float)((Form)sender).Width / (float)Convert.ToInt16(tmp[0]);
            //float heigth = (float)((Form)sender).Height / (float)Convert.ToInt16(tmp[1]);

            //   Single currentSize = Convert.ToSingle((float)Convert.ToInt16(tmp[2]));

            //((Form)sender).Tag = ((Form)sender).Width.ToString() + "," + ((Form)sender).Height;

            //foreach (Control control in ((Form)sender).Controls)
            //{
            //    control.Scale(new SizeF(width, heigth));
                //  control.Font = new Font(control.Font.Name, currentSize, control.Font.Style, control.Font.Unit); 

            //}
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
                                dgvUserData.Rows.Add(userName, userPwd);
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SaveData();
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

            for (int row = 0; row < dgvUserData.Rows.Count; ++row)
            {
                XmlElement item = doc.CreateElement("Item");
                for (int i = 0; i < dgvUserData.Rows[row].Cells.Count; ++i)
                {
                    if (dgvUserData.Rows[row].Cells[i].Value != null)
                    {
                        item.SetAttribute(dgvUserData.Columns[i].HeaderText, dgvUserData.Rows[row].Cells[i].Value.ToString());
                    }
                    else
                    {
                        //item.SetAttribute(dataGridView.Columns[i].HeaderText, "");
                    }
                }
                root.AppendChild(item);
            }

            doc.Save("Data.xml");
        }

        private void LoadData()
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load("Data.xml");

                XmlNode root = doc.SelectSingleNode("Data");

                for (XmlNode item = root.FirstChild; item != null; item = item.NextSibling)
                {
                    bool bDelete = true;
                    int newIdx = dgvUserData.Rows.Add();
                    for (int i = 0; i < dgvUserData.Columns.Count; ++i)
                    {
                        string HeaderText = dgvUserData.Columns[i].HeaderText;
                        if (item.Attributes[HeaderText] != null)
                        {
                            string s = item.Attributes[HeaderText].Value;
                            dgvUserData.Rows[newIdx].Cells[i].Value = s;
                            bDelete = false;
                        }
                    }
                    if (bDelete)
                    {
                        dgvUserData.Rows.Remove(dgvUserData.Rows[newIdx]);
                    }
                }

                dgvUserData.AutoResizeColumns();
            }
            catch (System.IO.FileNotFoundException)
            {

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
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

        private void OnMsg(string s,Session c)
        {
            try
            {
                
                Print("Thread:" + Thread.CurrentThread.ManagedThreadId + "Recv:" + s);

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
                                dgvSession.Rows.Add(c.handle.RemoteEndPoint.ToString(), code, mac, "已连接", "0", DateTime.Now.ToLocalTime(), "断开","重启");
                            }
                        }break;
                    case "1":
                        {
                            string token = split[1];
                            string code = split[2];

                            string accName = "";
                            string passWord = "";
                            if (!m_Token.ContainsKey(token))
                            {
                                ///-- 根据选项优先分配失败1次的的账号
                                if (this.cbFailedFirst.Checked == true)
                                {
                                    foreach (DataGridViewRow row in dgvUserData.Rows)
                                    {
                                        if (row.IsNewRow) { continue; }
                                        if ((row.Cells["Checked"].Value == null
                                            || row.Cells["Checked"].Value.ToString() != "True"))
                                        {
                                            if (row.Cells["State"].Value != null 
                                                && row.Cells["State"].Value.ToString() == "签到失败")
                                            {
                                                if (row.Cells["FailedCount"].Value == null
                                                    || row.Cells["FailedCount"].Value.ToString() == "1"
                                                    || row.Cells["FailedCount"].Value.ToString() == "0")
                                                {
                                                    accName = row.Cells["Account"].Value as string;
                                                    passWord = row.Cells["Password"].Value as string;
                                                    if (accName != null && accName != "" && passWord != null && passWord != "")
                                                    {
                                                        row.Cells["State"].Value = "已经分配";
                                                        row.Cells["IP"].Value = c.handle.RemoteEndPoint.ToString();
                                                        row.Cells["Code"].Value = code;
                                                        m_Token[token] = accName;
                                                    }
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }

                                //优先分配,未分配过的账号
                                if (accName == null || accName == "" || passWord == null || passWord == "")
                                {
                                    foreach (DataGridViewRow row in dgvUserData.Rows)
                                    {
                                        if (row.IsNewRow) { continue; }
                                        if ((row.Cells["Checked"].Value == null || row.Cells["Checked"].Value.ToString() != "True"))
                                        {
                                            if (row.Cells["State"].Value == null
                                                || row.Cells["State"].Value.ToString() == "")
                                            {
                                                accName = row.Cells["Account"].Value as string;
                                                passWord = row.Cells["Password"].Value as string;
                                                if (accName != null && accName != "" && passWord != null && passWord != "")
                                                {
                                                    row.Cells["State"].Value = "已经分配";
                                                    row.Cells["IP"].Value = c.handle.RemoteEndPoint.ToString();
                                                    row.Cells["Code"].Value = code;
                                                    m_Token[token] = accName;
                                                }
                                                break;
                                            }
                                        }
                                    }
                                }

                                //其次分配，签到失败账号
                                if (accName == null || accName == "" || passWord == null || passWord == "")
                                {
                                    DataGridViewRow dstRow = null;
                                    foreach (DataGridViewRow row in dgvUserData.Rows)
                                    {
                                        if (row.IsNewRow) { continue; }
                                        if ((row.Cells["Checked"].Value == null || row.Cells["Checked"].Value.ToString() != "True"))
                                        {
                                            if (row.Cells["State"].Value.ToString() == "签到失败")
                                            {
                                                if (accName != null && accName != "" && passWord != null && passWord != "")
                                                {
                                                    if (dstRow == null)
                                                    {
                                                        dstRow = row;
                                                    }
                                                    else
                                                    {
                                                        int c1 = 0;
                                                        int c2 = 0;
                                                        try {
                                                            int.TryParse(dstRow.Cells["FailedCount"].ToString(),out c1);
                                                            int.TryParse(row.Cells["FailedCount"].ToString(), out c2);
                                                        }
                                                        catch { }
                                                        if (c1 > c2)
                                                        {
                                                            dstRow = row;
                                                        }
                                                    }
                                                }
                                                break;
                                            }
                                        }
                                    }

                                    if (dstRow != null)
                                    {
                                        accName = dstRow.Cells["Account"].Value as string;
                                        passWord = dstRow.Cells["Password"].Value as string;

                                        dstRow.Cells["State"].Value = "已经分配";
                                        dstRow.Cells["IP"].Value = c.handle.RemoteEndPoint.ToString();
                                        dstRow.Cells["Code"].Value = code;
                                        m_Token[token] = accName;
                                    }
                                }


                                if (accName == null || accName == "" || passWord == null || passWord == "")
                                {
                                    foreach (DataGridViewRow row in dgvUserData.Rows)
                                    {
                                        if (row.IsNewRow) { continue; }
                                        if ((row.Cells["Checked"].Value == null || row.Cells["Checked"].Value.ToString() != "True")
                                            && row.Cells["State"].Value.ToString() != "已经分配")
                                        {
                                            accName = row.Cells["Account"].Value as string;
                                            passWord = row.Cells["Password"].Value as string;
                                            if (accName != null && accName != "" && passWord != null && passWord != "")
                                            {
                                                row.Cells["State"].Value = "已经分配";
                                                row.Cells["IP"].Value = c.handle.RemoteEndPoint.ToString();
                                                row.Cells["Code"].Value = code;
                                                m_Token[token] = accName;
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                accName = m_Token[token];
                                for (int i = 0; i < dgvUserData.Rows.Count; ++i)
                                {
                                    if (dgvUserData.Rows[i].Cells["Account"].Value != null && 
                                        dgvUserData.Rows[i].Cells["Account"].Value.ToString() == accName)
                                    {
                                        accName = dgvUserData.Rows[i].Cells["Account"].Value as string;
                                        passWord = dgvUserData.Rows[i].Cells["Password"].Value as string;
                                        break;
                                    }
                                }
                            }

                            if (accName == null || accName == "" || passWord == null || passWord == "")
                            {
                                Print("没有对应账户可以用了");
                            }
                            else
                            {
                                //SendMsg("2:" + accName + ":" + passWord, c);
                                c.Send("2$" + accName + "$" + passWord);
                            }

                        } break;
                    case "3":
                        {
                            string accName = split[1];
                            string isOk = split[2];

                            if (isOk == "OK")
                            {
                                for (int i = 0; i < dgvUserData.Rows.Count; ++i)
                                {
                                    if (dgvUserData.Rows[i].Cells["Account"].Value != null
                                        && dgvUserData.Rows[i].Cells["Account"].Value.ToString() == accName)
                                    {
                                        dgvUserData.Rows[i].Cells["Checked"].Value = true;
                                        dgvUserData.Rows[i].Cells["State"].Value = "签到完成";
                                        dgvUserData.Rows[i].Cells["CheckTime"].Value = DateTime.Now.ToString();
                                        dgvUserData.Rows[i].Cells["FailedCount"].Value = 0;
                                    }
                                }
                            }
                            else if (isOk == "Failed")
                            {
                                for (int i = 0; i < dgvUserData.Rows.Count; ++i)
                                {
                                    if (dgvUserData.Rows[i].Cells["Account"].Value != null
                                        && dgvUserData.Rows[i].Cells["Account"].Value.ToString() == accName)
                                    {
                                        dgvUserData.Rows[i].Cells["Checked"].Value = false;
                                        dgvUserData.Rows[i].Cells["State"].Value = "签到失败";
                                        dgvUserData.Rows[i].Cells["CheckTime"].Value = DateTime.Now.ToString();
                                        dgvUserData.Rows[i].Cells["FailedCount"].Value = dgvUserData.Rows[i].Cells["FailedCount"].Value == null ? 1 : (int)dgvUserData.Rows[i].Cells["FailedCount"].Value + 1;
                                    }
                                }
                            }
                            else if (isOk == "PasswordError")
                            {
                                for (int i = 0; i < dgvUserData.Rows.Count; ++i)
                                {
                                    if (dgvUserData.Rows[i].Cells["Account"].Value != null
                                        && dgvUserData.Rows[i].Cells["Account"].Value.ToString() == accName)
                                    {
                                        dgvUserData.Rows[i].Cells["Checked"].Value = false;
                                        dgvUserData.Rows[i].Cells["State"].Value = "密码错误";
                                        dgvUserData.Rows[i].Cells["CheckTime"].Value = DateTime.Now.ToString();
                                        dgvUserData.Rows[i].Cells["FailedCount"].Value = dgvUserData.Rows[i].Cells["FailedCount"].Value == null ? 1 : (int)dgvUserData.Rows[i].Cells["FailedCount"].Value + 1;
                                    }
                                }
                            }

                            foreach (DataGridViewRow row in dgvSession.Rows)
                            {
                                if (row.IsNewRow) continue;
                                if (row.Cells["sMac"].Value == c.m_mac)
                                {
                                    row.Cells["sLoginState"].Value = DateTime.Now.ToLocalTime();
                                }
                            }
                            SaveData();
                        } break;
                    case "4":
                        {
                            string token = split[1];
                            if (m_Token.ContainsKey(token))
                            {
                                string accName = m_Token[token];
                                m_Token.Remove(token);

                                for (int i = 0; i < dgvUserData.Rows.Count; ++i)
                                {
                                    if (dgvUserData.Rows[i].Cells["Account"].Value != null
                                        && dgvUserData.Rows[i].Cells["Account"].Value.ToString() == accName)
                                    {
                                        dgvUserData.Rows[i].Cells["State"].Value = "正在签到";
                                    }
                                }
                            }
                        }break;
                    case "5":
                        {
                            string accName = split[1];
                            string Day = split[2];
                            for (int i = 0; i < dgvUserData.Rows.Count; ++i)
                            {
                                if (dgvUserData.Rows[i].Cells["Account"].Value != null
                                    && dgvUserData.Rows[i].Cells["Account"].Value.ToString() == accName)
                                {
                                    dgvUserData.Rows[i].Cells["Days"].Value = Day;
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

        private void Print(string s)
        {
            string format_msg = string.Format("{0:yy-MM-dd HH:mm:ss} Thread:{1} {2} {3}", DateTime.Now, Thread.CurrentThread.ManagedThreadId, "Debug", s);
            lock (m_textBoxBuffer)
            {
                //textBox.BeginInvoke(new Delegate_Print(Print1), format_msg);
                m_textBoxBuffer += format_msg;
                m_textBoxBuffer += "\r\n";

                if (m_textBoxBuffer.Length > 1024)
                {
                    textBox.Invoke(new Delegate_Flush(FlushToTextBox));
                }
            }
        }

        private void FlushToTextBox()
        {
            lock (m_textBoxBuffer)
            {
                if (m_textBoxBuffer.Length < 1048 * 10)
                {
                    if (textBox.Text.Length > 1048 * 10)
                    {
                        textBox.Text = "";
                    }
                    textBox.Text += m_textBoxBuffer;
                }
                else
                {
                    string format_msg = string.Format("{0:yy-MM-dd HH:mm:ss} Thread:{1} {2} {3}", DateTime.Now, Thread.CurrentThread.ManagedThreadId, "Warming", "日志数量巨大...跳过打印");
                    textBox.Text += format_msg;
                    textBox.Text += "\r\n";
                }

                textBox.SelectionStart = textBox.Text.Length;  //设定光标位置
                textBox.ScrollToCaret();


                const string logDir = @".\Manage_Log";
                if (!Directory.Exists(logDir))
                {
                    Directory.CreateDirectory(logDir);
                }

                try
                {
                    using (StreamWriter writer = new StreamWriter(logDir + @"\" + logFileName, true))
                    {
                        writer.Write(m_textBoxBuffer);
                    }
                }
                catch
                {

                }

                m_textBoxBuffer = "";
            }
        }

        Dictionary<string, string> m_Token = new Dictionary<string, string>();
        static string logFileName = String.Format("{0:yyyyMMdd_HHmmss}.txt", DateTime.Now);
        string m_textBoxBuffer = "";

        private void button1_Click(object sender, EventArgs e)
        {
            textBox.Text = "";
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
        }

        private void timer_FlushTextbox_Tick(object sender, EventArgs e)
        {
            if (m_textBoxBuffer.Length > 0)
            {
                FlushToTextBox();
            }
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
                                string s = (string)row.Cells[i].Value;
                                fileTxt += s;
                            }
                            else
                            {
                                fileTxt += "(NULL)";
                            }
                        }
                        fileTxt += "\r\n";
                    }

                    Stream file = dlg.OpenFile();
                    byte[] bytes = System.Text.Encoding.Default.GetBytes(fileTxt);
                    file.Write(bytes, 0, bytes.Length);
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
                                Print("mac:" + row.Cells["sMac"].Value + " code:" + row.Cells["sCode"].Value + " " + ts.TotalSeconds + "秒未登陆!");
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
    }

    delegate void Delegate_Flush();
    delegate void Delegate_Print(string s);
    delegate void Delegate<T>(T t); 
    delegate void Delegate<T1,T2>(T1 t1,T2 t2);
}
