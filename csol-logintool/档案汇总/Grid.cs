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

namespace 档案汇总
{
    public partial class Grid : Form
    {
        public Grid()
        {
            InitializeComponent();

            Text += string.Format(" {0:yy-MM-dd HH:mm:ss} Version {1}.{2}"
                , System.IO.File.GetLastWriteTime(this.GetType().Assembly.Location)
                , System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Major
                , System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Minor);

            LoadData();

            textBox_IP.Text = Sever.GetLocalIp();

            Tag = Width.ToString() + "," + Height.ToString();
            SizeChanged += Form1_SizeChanged;
            dataGridView.DragDrop += dataGridView_DragDrop;
            dataGridView.DragEnter += dataGridView_DragEnter;
            dataGridView.SortCompare += dataGridView_SortCompare;
            dataGridView.CellMouseDown +=new DataGridViewCellMouseEventHandler(dataGridView_CellMouseUp);
            dataGridView.DataError +=new DataGridViewDataErrorEventHandler(dataGridView_DataError);
        }

        void dataGridView_DataError(object sender, DataGridViewDataErrorEventArgs e){}

        private void MenuItem_GouXuan_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewCell cell in dataGridView.SelectedCells)
            {
                if (dataGridView.Columns[cell.ColumnIndex].Name == "Checked")
                {
                    cell.Value = true;
                }
            }
        }

        private void MenuItem_ClearGouXuan_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewCell cell in dataGridView.SelectedCells)
            {
                if (dataGridView.Columns[cell.ColumnIndex].Name == "Checked")
                {
                    cell.Value = false;
                }
            }
        }

        private void dataGridView_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (dataGridView.SelectedRows.Count > 0)
                {
                    Menu_DeleteRow.Show(MousePosition.X, MousePosition.Y);
                }
                else if (dataGridView.SelectedCells.Count > 0)
                {
                    int x = 0;
                    int columnIndex = -1;

                    if (dataGridView.SelectedCells.Count > 0)
                    {
                        x = 1;

                        foreach (DataGridViewCell cell in dataGridView.SelectedCells)
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
                        if (dataGridView.Columns[columnIndex].Name == "Checked")
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
            foreach (DataGridViewRow r in dataGridView.SelectedRows)
            {
                if (!r.IsNewRow)
                {
                    dataGridView.Rows.Remove(r);
                }
            }
        }

        private void MenuItem_ClearCell_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewCell cell in dataGridView.SelectedCells)
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
            string[] tmp = ((Form)sender).Tag.ToString().Split(',');
            float width = (float)((Form)sender).Width / (float)Convert.ToInt16(tmp[0]);
            float heigth = (float)((Form)sender).Height / (float)Convert.ToInt16(tmp[1]);

            //   Single currentSize = Convert.ToSingle((float)Convert.ToInt16(tmp[2]));

            ((Form)sender).Tag = ((Form)sender).Width.ToString() + "," + ((Form)sender).Height;

            foreach (Control control in ((Form)sender).Controls)
            {
                control.Scale(new SizeF(width, heigth));
                //  control.Font = new Font(control.Font.Name, currentSize, control.Font.Style, control.Font.Unit); 

            }
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
                        dataGridView.Rows.Clear();
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
                                dataGridView.Rows.Add(userName, userPwd);
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

        private void SaveData()
        {
            XmlDocument doc = new XmlDocument();
            XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", "GB2312", null);
            XmlElement root = doc.CreateElement("Data");
            doc.AppendChild(root);

            for (int row = 0; row < dataGridView.Rows.Count; ++row)
            {
                XmlElement item = doc.CreateElement("Item");
                for (int i = 0; i < dataGridView.Rows[row].Cells.Count; ++i)
                {
                    if (dataGridView.Rows[row].Cells[i].Value != null)
                    {
                        item.SetAttribute(dataGridView.Columns[i].HeaderText, dataGridView.Rows[row].Cells[i].Value.ToString());
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
                    int newIdx = dataGridView.Rows.Add();
                    for (int i = 0; i < dataGridView.Columns.Count; ++i)
                    {
                        string HeaderText = dataGridView.Columns[i].HeaderText;
                        if (item.Attributes[HeaderText] != null)
                        {
                            string s = item.Attributes[HeaderText].Value;
                            dataGridView.Rows[newIdx].Cells[i].Value = s;
                            bDelete = false;
                        }
                    }
                    if (bDelete)
                    {
                        dataGridView.Rows.Remove(dataGridView.Rows[newIdx]);
                    }
                }

                dataGridView.AutoResizeColumns();
            }
            catch (System.IO.FileNotFoundException)
            {

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
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
                    case "1":
                        {
                            string token = split[1];
                            string code = split[2];

                            string accName = "";
                            string passWord = "";
                            if (!m_Token.ContainsKey(token))
                            {
                                foreach (DataGridViewRow row in dataGridView.Rows)
                                {
                                    if (row.IsNewRow){continue;}
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

                                if (accName == null || accName == "" || passWord == null || passWord == "")
                                {
                                    foreach (DataGridViewRow row in dataGridView.Rows)
                                    {
                                        if (row.IsNewRow) { continue; }
                                        if ((row.Cells["Checked"].Value == null || row.Cells["Checked"].Value.ToString() != "True"))
                                        {
                                            if (row.Cells["State"].Value.ToString() == "签到失败")
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


                                if (accName == null || accName == "" || passWord == null || passWord == "")
                                {
                                    foreach (DataGridViewRow row in dataGridView.Rows)
                                    {
                                        if (row.IsNewRow) { continue; }
                                        if ((row.Cells["Checked"].Value == null || row.Cells["Checked"].Value.ToString() != "True"))
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
                                for (int i = 0; i < dataGridView.Rows.Count; ++i)
                                {
                                    if (dataGridView.Rows[i].Cells["Account"].Value != null && 
                                        dataGridView.Rows[i].Cells["Account"].Value.ToString() == accName)
                                    {
                                        accName = dataGridView.Rows[i].Cells["Account"].Value as string;
                                        passWord = dataGridView.Rows[i].Cells["Password"].Value as string;
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
                                for (int i = 0; i < dataGridView.Rows.Count; ++i)
                                {
                                    if (dataGridView.Rows[i].Cells["Account"].Value != null
                                        && dataGridView.Rows[i].Cells["Account"].Value.ToString() == accName)
                                    {
                                        dataGridView.Rows[i].Cells["Checked"].Value = true;
                                        dataGridView.Rows[i].Cells["State"].Value = "签到完成";
                                        dataGridView.Rows[i].Cells["CheckTime"].Value = DateTime.Now.ToString();
                                        dataGridView.Rows[i].Cells["FailedCount"].Value = 0;
                                    }
                                }
                            }
                            else if (isOk == "Failed")
                            {
                                for (int i = 0; i < dataGridView.Rows.Count; ++i)
                                {
                                    if (dataGridView.Rows[i].Cells["Account"].Value != null
                                        && dataGridView.Rows[i].Cells["Account"].Value.ToString() == accName)
                                    {
                                        dataGridView.Rows[i].Cells["Checked"].Value = false;
                                        dataGridView.Rows[i].Cells["State"].Value = "签到失败";
                                        dataGridView.Rows[i].Cells["CheckTime"].Value = DateTime.Now.ToString();
                                        dataGridView.Rows[i].Cells["FailedCount"].Value = dataGridView.Rows[i].Cells["FailedCount"].Value == null ? 1 : (int)dataGridView.Rows[i].Cells["FailedCount"].Value + 1;
                                    }
                                }
                            }
                            else if (isOk == "PasswordError")
                            {
                                for (int i = 0; i < dataGridView.Rows.Count; ++i)
                                {
                                    if (dataGridView.Rows[i].Cells["Account"].Value != null
                                        && dataGridView.Rows[i].Cells["Account"].Value.ToString() == accName)
                                    {
                                        dataGridView.Rows[i].Cells["Checked"].Value = false;
                                        dataGridView.Rows[i].Cells["State"].Value = "密码错误";
                                        dataGridView.Rows[i].Cells["CheckTime"].Value = DateTime.Now.ToString();
                                        dataGridView.Rows[i].Cells["FailedCount"].Value = dataGridView.Rows[i].Cells["FailedCount"].Value == null ? 1 : (int)dataGridView.Rows[i].Cells["FailedCount"].Value + 1;
                                    }
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

                                for (int i = 0; i < dataGridView.Rows.Count; ++i)
                                {
                                    if (dataGridView.Rows[i].Cells["Account"].Value != null
                                        && dataGridView.Rows[i].Cells["Account"].Value.ToString() == accName)
                                    {
                                        dataGridView.Rows[i].Cells["State"].Value = "正在签到";
                                    }
                                }
                            }
                        }break;
                    case "5":
                        {
                            string accName = split[1];
                            string Day = split[2];
                            for (int i = 0; i < dataGridView.Rows.Count; ++i)
                            {
                                if (dataGridView.Rows[i].Cells["Account"].Value != null
                                    && dataGridView.Rows[i].Cells["Account"].Value.ToString() == accName)
                                {
                                    dataGridView.Rows[i].Cells["Days"].Value = Day;
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

        private void timer_StatusBarRefresh_Tick(object sender, EventArgs e)
        {
            lock(Sever.m_Clinets)
            {
                StatusLab_SessionNum.Text = "当前连接:" + Sever.m_Clinets.Count;

                for (int i = 0; i < Sever.m_Clinets.Count; ++i)
                {
                    Session s = Sever.m_Clinets[i];
                    s.Send("0");
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
                    foreach (DataGridViewRow row in dataGridView.Rows)
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
    }

    delegate void Delegate_Flush();
    delegate void Delegate_Print(string s);
    delegate void Delegate<T>(T t); 
    delegate void Delegate<T1,T2>(T1 t1,T2 t2);
}
