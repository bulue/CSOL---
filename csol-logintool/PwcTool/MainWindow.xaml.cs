using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Xml;
using CommonQ;
using System.Net;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using Noesis.Javascript;
using System.Data.SQLite;
using System.Data;
using System.ComponentModel;
using System.IO.Compression;

namespace PwcTool
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public static byte[] Compress(byte[] raw)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                using (GZipStream gzip = new GZipStream(memory,
                CompressionMode.Compress, true))
                {
                    gzip.Write(raw, 0, raw.Length);
                }
                return memory.ToArray();
            }
        }

        public MainWindow()
        {
            InitializeComponent();


            try
            {
                m_safekey = RandomString.Next(8, "1-9A-Za-z");

                login dlg = new login();
                dlg.m_safekey = m_safekey;
                dlg.ShowDialog();
                string account = dlg.textBox1.Text;
                m_safelg = dlg.m_lg;
                m_safepw = dlg.m_pw;
                if (String.IsNullOrEmpty(account)
                    || String.IsNullOrEmpty(m_safelg)
                    || String.IsNullOrEmpty(m_safepw))
                {
                    Environment.Exit(0);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Environment.Exit(0);
            }

            m_logger = CLogger.FromFolder("pwclog");

            try
            {
                if (!File.Exists(datasource))
                {
                    System.Data.SQLite.SQLiteConnection.CreateFile(datasource);
                }
                SQLiteConnectionStringBuilder connstr = new SQLiteConnectionStringBuilder();
                connstr.DataSource = datasource;
                m_conn.ConnectionString = connstr.ToString();
                //m_conn.SetPassword("helloworld");
                m_conn.Open();

                using (SQLiteCommand cmd = new SQLiteCommand(m_conn))
                {
                    cmd.CommandText = "select count(*) from sqlite_master where type = 'table' and name = '" + uidtable + "'";
                    Int64 result = (Int64)cmd.ExecuteScalar();
                    if (result == 0)
                    {
                        cmd.CommandText = "CREATE TABLE " + uidtable + "(uid varchar(20) primary key,password varchar(20),status varchar(30))";
                        cmd.ExecuteNonQuery();
                    }

                    cmd.CommandText = "select * from " + uidtable;
                    cmd.CommandType = CommandType.Text;

                    SQLiteDataAdapter da = new SQLiteDataAdapter(cmd);
                    da.Fill(m_dataset);
                    UidGrid.ItemsSource = m_dataset.Tables[0].DefaultView;
                    UidGrid.LoadingRow += new EventHandler<DataGridRowEventArgs>(dataGrid_LoadingRow);

                    for (int i = 0; i < m_dataset.Tables[0].Rows.Count; ++i)
                    {
                        m_uid2idx.Add(m_dataset.Tables[0].Rows[i][column_uid].ToString().ToLower(), i);
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }     
        }

        void CheckAccount(string account)
        {
        }

        CLogger m_logger;
        Random m_rgen = new Random(System.Environment.TickCount);

        SQLiteConnection m_conn = new SQLiteConnection();
        DataSet m_dataset = new DataSet();

        const string lgxml = "lg.xml";
        const string pwxml = "pw.xml";
        const string uidtxt = "uid.txt";
        const string md5js = "md5.js";
        const string datasource = "uid.db";

        const string uidtable = "uidlogin";
        const string column_uid = "uid";
        const string column_password = "password";
        const string column_status = "status";

        const string status_ready = "准备";
        const string status_pwderror = "密码错误";
        const string status_ok = "成功";

        string m_safekey = "";
        string m_safelg = "";
        string m_safepw = "";
        string m_constkey = "0xab3aff";

        Dictionary<string, string> m_lgcaptcha = new Dictionary<string, string>();
        Dictionary<string, string> m_pwcaptcha = new Dictionary<string, string>();

        string m_fixpwd = "";
        string m_randompwd = "";

        bool m_bofirststart = true;

        List<CpWorker> m_workers = new List<CpWorker>();

        Dictionary<string, int> m_uid2idx = new Dictionary<string, int>();

        int m_walkiterator = 0;

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(RandomPwdTextbox.Text)
                && String.IsNullOrEmpty(FixPwdTextbox.Text))
            {
                MessageBox.Show("请先设置随机密码或者固定密码!", "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!String.IsNullOrEmpty(RandomPwdTextbox.Text))
            {
                m_randompwd = RandomPwdTextbox.Text;
            }
            else
            {
                m_fixpwd = FixPwdTextbox.Text;
            }

            //if (!File.Exists(lgxml))
            //{
            //    MessageBox.Show(lgxml + "文件不存在", "", MessageBoxButton.OK, MessageBoxImage.Error);
            //    return;
            //}

            //if (!File.Exists(pwxml))
            //{
            //    MessageBox.Show(lgxml + "文件不存在", "", MessageBoxButton.OK, MessageBoxImage.Error);
            //    return;
            //}

            if (!File.Exists(md5js))
            {
                MessageBox.Show(lgxml + "文件不存在", "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            //load_captcha_xml(lgxml, m_lgcaptcha);
            //load_captcha_xml(pwxml, m_pwcaptcha);
            if (m_bofirststart)
            {
                load_captcha_from_safetxt(m_safelg, m_constkey, m_lgcaptcha);
                load_captcha_from_safetxt(m_safepw, m_constkey, m_pwcaptcha);
                m_logger.Debug("lg:" + m_lgcaptcha.Count + " pw:" + m_pwcaptcha.Count);
            }

            int num = int.Parse(textBox1.Text);
            for (int i = 0; i < num; ++i)
            {
                if (m_workers.Count <= i)
                {
                    CpWorker worker = new CpWorker(m_lgcaptcha, m_pwcaptcha, m_safekey);
                    worker.FinishTask += new Action<CpWorker, string, string, string, string>(worker_FinishTask);
                    m_workers.Add(worker);
                }

                DataRow nextuidrow = IteratorNextRow(m_dataset.Tables[0],ref m_walkiterator);
                if (nextuidrow != null)
                {
                    string uid = (string)nextuidrow[column_uid];
                    string pwd = (string)nextuidrow[column_password];
                    string newpwd = this.GetNewPassword();
                    m_workers[i].BeginTaskChangePwd(uid.ToLower().Trim(), pwd.Trim(), newpwd);
                    nextuidrow[column_status] = status_ready;

                    m_logger.Debug("begin uid:" + uid + " pwd:" + pwd + " newpwd:" + newpwd);
                }
                else
                {
                    m_workers[i].IsWorking = false;
                }
            }

            m_bofirststart = false;
            RandomPwdTextbox.IsEnabled = false;
            FixPwdTextbox.IsEnabled = false;
            textBox1.IsEnabled = false;

            btnStart.IsEnabled = false;
            btnStop.IsEnabled = true;

            CheckCpWorkerStatus();
        }

        private void OnFixPwdKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (RandomPwdTextbox.Text != "")
            {
                RandomPwdTextbox.Text = "";
            }
        }

        private void OnRandomPwdKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (FixPwdTextbox.Text != "")
            {
                FixPwdTextbox.Text = "";
            }
        }

        private DataRow IteratorNextRow(DataTable table ,ref int iterator)
        {
            for (; iterator != table.Rows.Count; iterator++)
            {
                string status = table.Rows[iterator][column_status] == null ? "" 
                    : table.Rows[iterator][column_status] as string;
                if (String.IsNullOrEmpty(status))
                {
                    return table.Rows[iterator++];
                }
            }
            return null;
        }

        private string GetNewPassword()
        {
            if (!String.IsNullOrEmpty(m_randompwd))
            {
                return RandomString.Next(6, m_randompwd);
            }
            else
            {
                return m_fixpwd;
            }
        }

        void worker_FinishTask(CpWorker cpwoker, string uid, string pwd, string newpwd, string result)
        {
            //Dispatcher.BeginInvoke(new Action<CpWorker, string, string, string, string>(worker_FinishTask_Safe),System.Windows.Threading.DispatcherPriority.Background, cpwoker, uid, pwd, newpwd, result);
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
            worker.RunWorkerAsync(new Tuple<CpWorker, string, string, string, string>(cpwoker, uid, pwd, newpwd, result));

        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            int begintick = System.Environment.TickCount;
            var arg = e.Argument as Tuple<CpWorker, string, string, string, string>;
            CpWorker cpworker = arg.Item1;
            string uid = arg.Item2;
            string pwd = arg.Item3;
            string newpwd = arg.Item4;
            string result = arg.Item5;

            string nowpwd = pwd;
            if (result == "成功")
            {
                nowpwd = newpwd;
            }
            
            using (SQLiteCommand cmd = new SQLiteCommand(m_conn))
            {
                cmd.CommandText = "UPDATE " + uidtable + " SET " + column_status + " = @status , " + column_password + " = @pwd"
                    + " WHERE " + column_uid + " = @uid  ";

                cmd.Parameters.Add(new SQLiteParameter("@status", result));
                cmd.Parameters.Add(new SQLiteParameter("@uid", uid));
                cmd.Parameters.Add(new SQLiteParameter("@pwd", nowpwd));

                cmd.ExecuteNonQuery();
            }
            m_logger.Debug("update:" + (System.Environment.TickCount - begintick) + "ms");

            e.Result = new Tuple<CpWorker, string, string, string>(cpworker, uid, nowpwd, result);
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var arg = e.Result as Tuple<CpWorker, string, string, string>;
            CpWorker cpwoker = arg.Item1;
            string uid = arg.Item2;
            string pwd = arg.Item3;
            string result = arg.Item4;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                DataRow targetrow = m_dataset.Tables[0].Rows[m_uid2idx[uid]];
                targetrow[column_password] = pwd;
                targetrow[column_status] = result;

                try
                {
                    if (btnStop.IsEnabled == true)
                    {
                        DataRow nextuidrow = IteratorNextRow(m_dataset.Tables[0], ref m_walkiterator);
                        if (nextuidrow != null)
                        {
                            string nextuid = (string)nextuidrow[column_uid];
                            string nextpwd = (string)nextuidrow[column_password];
                            string nextnewpwd = this.GetNewPassword();
                            cpwoker.BeginTaskChangePwd(nextuid.ToLower().Trim(), nextpwd.Trim(), nextnewpwd);
                            nextuidrow[column_status] = status_ready;

                            m_logger.Debug("begin next uid:" + nextuid + " pwd:" + nextpwd + " newpwd:" + nextnewpwd);
                        }
                        else
                        {
                            cpwoker.IsWorking = false;
                            CheckCpWorkerStatus();
                        }
                    }
                    else
                    {
                        cpwoker.IsWorking = false;
                        CheckCpWorkerStatus();
                    }
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }));
        }

        void CheckCpWorkerStatus()
        {
            foreach (CpWorker w in m_workers)
            {
                if (w.IsWorking == true)
                {
                    return;
                }
            }
            btnStart.IsEnabled = true;
            btnStop.IsEnabled = true;
        }

        void load_captcha_xml(string xmlfile,Dictionary<string, string> dic)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(xmlfile);

            XmlNode root = doc.SelectSingleNode("Data");

            for (XmlNode item = root.FirstChild; item != null; item = item.NextSibling)
            {
                if (item.Attributes["name"] != null
                     && item.Attributes["value"] != null)
                {
                    dic.Add(item.Attributes["name"].Value, item.Attributes["value"].Value);
                }
            }
        }

        void load_captcha_from_safetxt(string safe_str, string key, Dictionary<string, string> dic)
        {
            string src_txt = MyDes.Decode(safe_str, key);
            string[] split = src_txt.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length > 0)
            {
                foreach (string s in split)
                {
                    string pattern = @"(\w+)=(\w+)";
                    Match mt = Regex.Match(s, pattern);
                    if (mt.Groups.Count == 3)
                    {
                        dic.Add(mt.Groups[1].ToString(), mt.Groups[2].ToString());
                    }
                }
            }
        }

        void load_uidtxt(string uidtxt)
        {
            try
            {
                if (File.Exists(uidtxt))
                {
                    using (SQLiteCommand cmd = new SQLiteCommand(m_conn))
                    {
                        m_uid2idx.Clear();
                        m_dataset.Tables[0].Clear();

                        cmd.CommandText = "DELETE FROM " + uidtable;
                        cmd.ExecuteNonQuery();

                        string[] lines = File.ReadAllLines(uidtxt, Encoding.GetEncoding("GB2312"));
                        string pattern = @"[0-9]*[^a-zA-z0-9]*(\w+)[^a-zA-z0-9]*(\w+)";

                        foreach (string line in lines)
                        {
                            Match mt = Regex.Match(line, pattern);
                            if (mt.Groups.Count == 3)
                            {
                                string uid = mt.Groups[1].ToString();
                                string pw = mt.Groups[2].ToString();

                                DataRow newRow = m_dataset.Tables[0].Rows.Add();
                                newRow[column_uid] = uid;
                                newRow[column_password] = pw;

                                if (m_uid2idx.ContainsKey(uid.ToLower()))
                                {
                                    m_uid2idx.Clear();
                                    m_dataset.Tables[0].Clear();
                                    MessageBox.Show("导入的数据中包含了相同的账号:" + uid,"Warming",MessageBoxButton.OK,MessageBoxImage.Warning);
                                }
                                m_uid2idx.Add(uid.ToLower(), m_dataset.Tables[0].Rows.IndexOf(newRow));
                            }
                        }

                        int begintick = System.Environment.TickCount;
                        cmd.CommandText = "SELECT * FROM " + uidtable;
                        SQLiteDataAdapter dataadapter = new SQLiteDataAdapter(cmd);
                        SQLiteCommandBuilder cmdbuilder = new SQLiteCommandBuilder(dataadapter);
                        dataadapter.InsertCommand = cmdbuilder.GetInsertCommand();
                        using (SQLiteTransaction ts = m_conn.BeginTransaction())
                        {
                            dataadapter.Update(m_dataset);
                            ts.Commit();
                        }
                        m_logger.Debug("数据库更新:" + (System.Environment.TickCount - begintick) + "ms");
                    }
                    m_walkiterator = 0;
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {

            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                m_conn.Close();
                m_logger.Stop();
            }
            catch (System.Exception ex)
            {

            }
            base.OnClosing(e);
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnStop.IsEnabled = false;
                CheckCpWorkerStatus();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void statusGrid_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Link;                            //WinForm中为e.Effect = DragDropEffects.Link
            else 
                e.Effects = DragDropEffects.None;             
        }

        private void statusGrid_Drop(object sender, DragEventArgs e)
        {
            string fileName = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            load_uidtxt(fileName);
        }

        public void dataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.Filter = "保存类型 |*.txt";
            dlg.FilterIndex = 1;
            if (dlg.ShowDialog() == true)
            {
                using (FileStream fs = new FileStream(dlg.FileName,FileMode.Create))
                {
                    using (StreamWriter writer = new StreamWriter(fs))
                    {
                        for (int i = 0; i < m_dataset.Tables[0].Rows.Count; ++i)
                        {
                            string uid = m_dataset.Tables[0].Rows[i][column_uid].ToString();
                            string pwd = m_dataset.Tables[0].Rows[i][column_password].ToString();
                            string status = m_dataset.Tables[0].Rows[i][column_status].ToString();

                            writer.WriteLine(uid + "----" + pwd + "----" + status);
                        }
                    }
                }
            }
        }
    }
}
