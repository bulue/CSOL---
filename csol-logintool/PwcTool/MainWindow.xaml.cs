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
using System.Threading;

namespace PwcTool
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        CLogger m_logger;
        Random m_rgen = new Random(System.Environment.TickCount);

        SQLiteConnection m_pwcconn = new SQLiteConnection();
        DataSet m_pwcdataset = new DataSet();
        SQLiteConnection m_cseconn = new SQLiteConnection();
        DataSet m_csedataset = new DataSet();

        const string lgxml = "lg.xml";
        const string pwxml = "pw.xml";
        const string uidtxt = "uid.txt";
        const string md5js = "cstc.js";
        const string pwcdb = "uid.db";
        const string csedb = "uid.db";

        static public string captchadb = "captcha";

        const string pwcuidtable = "uidlogin";
        const string column_uid = "uid";
        const string column_password = "password";
        const string column_status = "status";

        const string cse_uidtable = "uidlogin_checksafe";
        const string cse_column_uid = "uid";
        const string cse_column_password = "password";
        const string cse_column_status = "status";
        const string cse_column_se_mailbox = "mailbox";//密保邮箱
        const string cse_column_se_issue = "issue";//密保问题
        const string cse_column_se_idcard = "idcard";//实名认证
        const string cse_column_se_balance = "balance";//账户余额
        const string cse_column_se_jifen = "jifen"; //积分

        const string status_ready = "准备";
        const string status_pwderror = "密码错误";
        const string status_ok = "成功";

        string m_safekey = "";
        string m_safedbpwd = "";
        string m_safelg = "";
        string m_safepw = "";
        string m_constkey = "0xab3aff";

        Dictionary<string, string> m_lgcaptcha = new Dictionary<string, string>();
        Dictionary<string, string> m_pwcaptcha = new Dictionary<string, string>();

        string m_fixpwd = "";
        string m_randompwd = "";
        int m_randomnum = 0;

        bool m_bofirststart = true;

        Thread m_saveThread;
        List<object> m_saveObjList = new List<object>();
        List<object> m_saveSeObjList = new List<object>();
        bool m_boshutdwon;

        UidBackup m_uidbacker = new UidBackup();

        List<CpWorker> m_cpworkers = new List<CpWorker>();
        List<SeWorker> m_seworkers = new List<SeWorker>();

        Dictionary<string, int> m_uid2idx = new Dictionary<string, int>();

        int m_walkiterator = 0;
        int m_csewalkiterator = 0;

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                System.Net.ServicePointManager.DefaultConnectionLimit = 512;

                m_safekey = RandomString.Next(8, "1-9A-Za-z");

                login dlg = new login();
                dlg.m_safekey = m_safekey;
                dlg.ShowDialog();
                string account = dlg.tbxUid.Text;
                m_safelg = dlg.m_lg;
                m_safepw = dlg.m_pw;
                m_safedbpwd = dlg.m_dbpwd;
                if (String.IsNullOrEmpty(account)
                    || String.IsNullOrEmpty(m_safedbpwd))
                {
                    Environment.Exit(0);
                }

                this.Title = this.Title + " v" + App.version;
                if (!String.IsNullOrEmpty(dlg.deadline_date))
                {
                    this.Title = this.Title + " 到期时间[" + dlg.deadline_date + "]";
                }

            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Environment.Exit(0);
            }

            m_logger = CLogger.FromFolder("pwclog");
            m_saveThread = new Thread(new ThreadStart(SaveUidPwdStatus));
            m_saveThread.Start();

            InitPwcDb();
            InitCheckSafeDb();
        }

        //初始化修密 相关
        void InitPwcDb()
        {
            try
            {
                if (!File.Exists(pwcdb))
                {
                    System.Data.SQLite.SQLiteConnection.CreateFile(pwcdb);
                }
                SQLiteConnectionStringBuilder connstr = new SQLiteConnectionStringBuilder();
                connstr.DataSource = pwcdb;
                m_pwcconn.ConnectionString = connstr.ToString();
                m_pwcconn.Open();

                using (SQLiteCommand cmd = new SQLiteCommand(m_pwcconn))
                {
                    cmd.CommandText = "select count(*) from sqlite_master where type = 'table' and name = '" + pwcuidtable + "'";
                    Int64 result = (Int64)cmd.ExecuteScalar();
                    if (result == 0)
                    {
                        cmd.CommandText = "CREATE TABLE " + pwcuidtable + "(uid varchar(20) primary key,password varchar(20),status varchar(30))";
                        cmd.ExecuteNonQuery();
                    }

                    cmd.CommandText = "select * from " + pwcuidtable;
                    cmd.CommandType = CommandType.Text;

                    SQLiteDataAdapter da = new SQLiteDataAdapter(cmd);
                    da.Fill(m_pwcdataset);
                    UidGrid.ItemsSource = m_pwcdataset.Tables[0].DefaultView;
                    UidGrid.LoadingRow += new EventHandler<DataGridRowEventArgs>(dataGrid_LoadingRow);

                    for (int i = 0; i < m_pwcdataset.Tables[0].Rows.Count; ++i)
                    {
                        m_uid2idx.Add(m_pwcdataset.Tables[0].Rows[i][column_uid].ToString().ToLower(), i);
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString());
            } 
        }

        //初始化密保 相关
        void InitCheckSafeDb()
        {
            try
            {
                if (!File.Exists(csedb))
                {
                    System.Data.SQLite.SQLiteConnection.CreateFile(csedb);
                }
                SQLiteConnectionStringBuilder connstr = new SQLiteConnectionStringBuilder();
                connstr.DataSource = pwcdb;
                m_cseconn.ConnectionString = connstr.ToString();
                m_cseconn.Open();

                using (SQLiteCommand cmd = new SQLiteCommand(m_cseconn))
                {
                    cmd.CommandText = "select count(*) from sqlite_master where type = 'table' and name = '" + cse_uidtable + "'";
                    Int64 result = (Int64)cmd.ExecuteScalar();
                    if (result == 0)
                    {
                        cmd.CommandText = "CREATE TABLE " + cse_uidtable + "(uid varchar(30) primary key,password varchar(50),status varchar(50),idcard varchar(50),jifen integer,balance integer)";
                        cmd.ExecuteNonQuery();
                    }

                    cmd.CommandText = "select * from " + cse_uidtable;
                    cmd.CommandType = CommandType.Text;

                    SQLiteDataAdapter da = new SQLiteDataAdapter(cmd);
                    da.Fill(m_csedataset);
                    CheckUidSafeGrid.ItemsSource = m_csedataset.Tables[0].DefaultView;
                    CheckUidSafeGrid.LoadingRow += new EventHandler<DataGridRowEventArgs>(dataGrid_LoadingRow);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void btnStartChangePwd_Click(object sender, RoutedEventArgs e)
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
                m_randomnum = int.Parse(tbxPwdNum.Text);
            }
            else
            {
                m_fixpwd = FixPwdTextbox.Text;
            }

            if (!File.Exists(md5js))
            {
                MessageBox.Show(lgxml + "文件不存在", "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (m_bofirststart)
            {
                firstStart();
                m_bofirststart = false;
            }

            if (CpWorker.KeepRunTime != 0)
            {
                if (System.Environment.TickCount - CpWorker.StartRunTime > CpWorker.KeepRunTime)
                {
                    if (File.Exists(captchadb))
                    {
                        File.Delete(captchadb);
                    }
                    m_lgcaptcha.Clear();
                    m_pwcaptcha.Clear();

                    for (int i = 0; i < m_cpworkers.Count; ++i)
                    {
                        m_cpworkers[i].m_lgcaptcha.Clear();
                        m_cpworkers[i].m_pwcaptcha.Clear();
                    }
                    MessageBox.Show("试用到期了!!");
                    return;
                }
            }

            int num = int.Parse(tbxWorkerNumber.Text);
            for (int i = 0; i < num; ++i)
            {
                if (m_cpworkers.Count <= i)
                {
                    CpWorker worker = new CpWorker(m_lgcaptcha, m_pwcaptcha, m_safekey);
                    worker.FinishTask += new Action<CpWorker, string, string, string, string>(worker_FinishTask);
                    m_cpworkers.Add(worker);
                }

                DataRow nextuidrow = IteratorNextRow(m_pwcdataset.Tables[0],column_status,ref m_walkiterator);
                if (nextuidrow != null)
                {
                    string uid = (string)nextuidrow[column_uid];
                    string pwd = (string)nextuidrow[column_password];
                    string newpwd = this.GetNewPassword();
                    m_cpworkers[i].BeginTaskChangePwd(uid.ToLower().Trim(), pwd.Trim(), newpwd);
                    nextuidrow[column_status] = status_ready;

                    //m_logger.Debug("begin uid:" + uid + " pwd:" + pwd + " newpwd:" + newpwd);
                }
                else
                {
                    m_cpworkers[i].IsWorking = false;
                }
            }

            RandomPwdTextbox.IsEnabled = false;
            FixPwdTextbox.IsEnabled = false;
            tbxWorkerNumber.IsEnabled = false;
            tbxPwdNum.IsEnabled = false;

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

        private DataRow IteratorNextRow(DataTable table ,string columnn,ref int iterator)
        {
            for (; iterator != table.Rows.Count; iterator++)
            {
                string status = table.Rows[iterator][columnn] == null ? ""
                    : table.Rows[iterator][columnn] as string;
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
                return RandomString.Next(m_randomnum, m_randompwd);
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
                m_uidbacker.PushUid(uid, nowpwd);
            }

            lock (m_saveObjList)
            {
                m_saveObjList.Add(new Tuple<string, string, string>(uid, nowpwd, result));
            }

            //using (SQLiteCommand cmd = new SQLiteCommand(m_conn))
            //{
            //    cmd.CommandText = "UPDATE " + uidtable + " SET " + column_status + " = @status , " + column_password + " = @pwd"
            //        + " WHERE " + column_uid + " = @uid  ";

            //    cmd.Parameters.Add(new SQLiteParameter("@status", result));
            //    cmd.Parameters.Add(new SQLiteParameter("@uid", uid));
            //    cmd.Parameters.Add(new SQLiteParameter("@pwd", nowpwd));

            //    cmd.ExecuteNonQuery();
            //}
            m_logger.Debug("add update:" + (System.Environment.TickCount - begintick) + "ms");

            e.Result = new Tuple<CpWorker, string, string, string>(cpworker, uid, nowpwd, result);
        }

        void SaveUidPwdStatus()
        {
            while (true)
            {
                if (m_boshutdwon)
                {
                    lock (m_saveObjList)
                    {
                        lock (m_saveSeObjList)
                        {
                            if (m_saveObjList.Count == 0 && m_saveSeObjList.Count == 0)
                            {
                                return;
                            }
                        }
                    }
                }
                do 
                {
                    List<object> pwcwaitsavelist = new List<object>();
                    lock (m_saveObjList)
                    {
                        if (m_saveObjList.Count > 0)
                        {
                            pwcwaitsavelist = new List<object>(m_saveObjList);
                            m_saveObjList.Clear();
                        }
                    }

                    if (pwcwaitsavelist.Count > 0)
                    {
                        int begintick = System.Environment.TickCount;
                        using (SQLiteTransaction ts = m_pwcconn.BeginTransaction())
                        {
                            for (int i = 0; i < pwcwaitsavelist.Count; ++i)
                            {
                                var tuple = pwcwaitsavelist[i] as Tuple<string, string, string>;
                                string uid = tuple.Item1;
                                string pwd = tuple.Item2;
                                string status = tuple.Item3;

                                using (SQLiteCommand cmd = new SQLiteCommand(m_pwcconn))
                                {
                                    cmd.CommandText = "UPDATE " + pwcuidtable + " SET " + column_status + " = @status , " + column_password + " = @pwd"
                                        + " WHERE " + column_uid + " = @uid  ";

                                    cmd.Parameters.Add(new SQLiteParameter("@status", status));
                                    cmd.Parameters.Add(new SQLiteParameter("@uid", uid));
                                    cmd.Parameters.Add(new SQLiteParameter("@pwd", pwd));

                                    cmd.ExecuteNonQuery();
                                }
                            }
                            ts.Commit();
                        }

                        m_logger.Debug("update pwc count:" + pwcwaitsavelist.Count + " time:" + (System.Environment.TickCount - begintick) + "ms");
                    }
                } while (false);

                do 
                {
                    List<object> csewaitsavelist = new List<object>();
                    lock (m_saveSeObjList)
                    {
                        if (m_saveSeObjList.Count > 0)
                        {
                            csewaitsavelist = new List<object>(m_saveSeObjList);
                            m_saveSeObjList.Clear();
                        }
                    }

                    if (csewaitsavelist.Count > 0)
                    {
                        int begintick = System.Environment.TickCount;
                        using (SQLiteTransaction ts = m_cseconn.BeginTransaction())
                        {
                            for (int i = 0; i < csewaitsavelist.Count; ++i)
                            {
                                var tuple = csewaitsavelist[i] as Tuple<SeWorker, string, string, string, bool, int, int>;
                                SeWorker seworker = tuple.Item1;
                                string uid = tuple.Item2;
                                string pwd = tuple.Item3;
                                string status = tuple.Item4;
                                bool has_idcard = tuple.Item5;
                                int yue = tuple.Item6;
                                int userpoint = tuple.Item7;

                                if (status == "查询成功")
                                {
                                    using (SQLiteCommand cmd = new SQLiteCommand(m_cseconn))
                                    {
                                        cmd.CommandText = "UPDATE " + cse_uidtable + " SET " + cse_column_status + " = @status , " + cse_column_password + " = @pwd ,"
                                            + cse_column_se_idcard + " = @idcard, " + cse_column_se_jifen + " = @userpoint," + cse_column_se_balance + " =@balance "
                                            + " WHERE " + column_uid + " = @uid  ";

                                        cmd.Parameters.Add(new SQLiteParameter("@status", status));
                                        cmd.Parameters.Add(new SQLiteParameter("@uid", uid));
                                        cmd.Parameters.Add(new SQLiteParameter("@pwd", pwd));
                                        cmd.Parameters.Add(new SQLiteParameter("@idcard", has_idcard ? "有证" : "无证"));
                                        cmd.Parameters.Add(new SQLiteParameter("@userpoint", userpoint));
                                        cmd.Parameters.Add(new SQLiteParameter("@balance", yue));

                                        cmd.ExecuteNonQuery();
                                    }

                                }
                                else
                                {
                                    using (SQLiteCommand cmd = new SQLiteCommand(m_cseconn))
                                    {
                                        cmd.CommandText = "UPDATE " + cse_uidtable + " SET " + cse_column_status + " = @status  "
                                            + " WHERE " + column_uid + " = @uid  ";

                                        cmd.Parameters.Add(new SQLiteParameter("@status", status));
                                        cmd.Parameters.Add(new SQLiteParameter("@uid", uid));
                                        cmd.ExecuteNonQuery();
                                    }
                                }
                            }
                            ts.Commit();
                        }

                        m_logger.Debug("update cse count:" + csewaitsavelist.Count + " time:" + (System.Environment.TickCount - begintick) + "ms");
                    }
                } while (false);

                Thread.Sleep(500);
            }
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
                DataRow targetrow = m_pwcdataset.Tables[0].Rows[m_uid2idx[uid]];
                targetrow[column_password] = pwd;
                targetrow[column_status] = result;

                try
                {
                    if (CpWorker.KeepRunTime != 0 && btnStop.IsEnabled)
                    {
                        if (System.Environment.TickCount - CpWorker.StartRunTime > CpWorker.KeepRunTime)
                        {
                            btnStop.IsEnabled = false;
                            MessageBox.Show("试用时间到了!!");
                        }
                    }

                    if (btnStop.IsEnabled == true)
                    {
                        DataRow nextuidrow = IteratorNextRow(m_pwcdataset.Tables[0], column_status, ref m_walkiterator);
                        if (nextuidrow != null)
                        {
                            string nextuid = (string)nextuidrow[column_uid];
                            string nextpwd = (string)nextuidrow[column_password];
                            string nextnewpwd = this.GetNewPassword();
                            cpwoker.BeginTaskChangePwd(nextuid.ToLower().Trim(), nextpwd.Trim(), nextnewpwd);
                            nextuidrow[column_status] = status_ready;
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
            foreach (CpWorker w in m_cpworkers)
            {
                if (w.IsWorking == true)
                {
                    return;
                }
            }
            btnStart.IsEnabled = true;
            btnStop.IsEnabled = true;
            tbxWorkerNumber.IsEnabled = true;
            RandomPwdTextbox.IsEnabled = true;
            FixPwdTextbox.IsEnabled = true;
            tbxPwdNum.IsEnabled = true;
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
            if (!String.IsNullOrEmpty(safe_str))
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
        }

        void save_captcha_db_from_dic(Dictionary<string, string> dic,string dbpwd,string tablename)
        {
            if (!File.Exists(captchadb))
            {
                System.Data.SQLite.SQLiteConnection.CreateFile(captchadb);
            }
            SQLiteConnectionStringBuilder connstr = new SQLiteConnectionStringBuilder();
            connstr.DataSource = captchadb;
            SQLiteConnection captcha_dbconn = new SQLiteConnection();
            captcha_dbconn.ConnectionString = connstr.ToString();
            captcha_dbconn.SetPassword(dbpwd);
            captcha_dbconn.Open();

            using (SQLiteCommand cmd = new SQLiteCommand(captcha_dbconn))
            {
                cmd.CommandText = "select count(*) from sqlite_master where type = 'table' and name = '" + tablename + "'";
                Int64 result = (Int64)cmd.ExecuteScalar();
                if (result == 0)
                {
                    cmd.CommandText = "CREATE TABLE " + tablename + "(name varchar(50) primary key,value varchar(20))";
                    cmd.ExecuteNonQuery();
                }
            }

            using (SQLiteTransaction ts = captcha_dbconn.BeginTransaction())
            {
                using (SQLiteCommand cmd = new SQLiteCommand(captcha_dbconn))
                {
                    foreach (string name in dic.Keys)
                    {
                        string value = dic[name];

                        cmd.CommandText = "INSERT INTO " + tablename + " (name , value) VALUES(@name,@value)";

                        cmd.Parameters.Clear();
                        cmd.Parameters.Add(new SQLiteParameter("@name", name));
                        cmd.Parameters.Add(new SQLiteParameter("@value", value));

                        cmd.ExecuteNonQuery();

                    }
                }
                ts.Commit();
            }

            captcha_dbconn.Close();
        }

        void load_captcha_from_db(Dictionary<string, string> dic,string dbpwd,string tablename)
        {
            if (!File.Exists(captchadb))
            {
                return;
            }
            SQLiteConnectionStringBuilder connstr = new SQLiteConnectionStringBuilder();
            connstr.DataSource = captchadb;
            SQLiteConnection captcha_dbconn = new SQLiteConnection();
            captcha_dbconn.ConnectionString = connstr.ToString();
            captcha_dbconn.SetPassword(dbpwd);
            captcha_dbconn.Open();

            using (SQLiteCommand cmd = new SQLiteCommand(captcha_dbconn))
            {
                cmd.CommandText = "SELECT name,value FROM " + tablename;

                cmd.CommandType = CommandType.Text;

                SQLiteDataAdapter da = new SQLiteDataAdapter(cmd);
                DataSet tmpdataset = new DataSet();
                da.Fill(tmpdataset);

                if (tmpdataset.Tables[0].Rows.Count > 0)
                {
                    dic.Clear();
                    foreach (DataRow row in tmpdataset.Tables[0].Rows)
                    {
                        dic.Add(row["name"].ToString(), row["value"].ToString());
                    }
                }
            }

            captcha_dbconn.Close();
        }

        void load_uidtxt(string uidtxt)
        {
            try
            {
                if (File.Exists(uidtxt))
                {
                    using (SQLiteCommand cmd = new SQLiteCommand(m_pwcconn))
                    {
                        m_uid2idx.Clear();
                        m_pwcdataset.Tables[0].Clear();

                        cmd.CommandText = "DELETE FROM " + pwcuidtable;
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

                                DataRow newRow = m_pwcdataset.Tables[0].Rows.Add();
                                newRow[column_uid] = uid;
                                newRow[column_password] = pw;

                                if (m_uid2idx.ContainsKey(uid.ToLower()))
                                {
                                    m_uid2idx.Clear();
                                    m_pwcdataset.Tables[0].Clear();
                                    MessageBox.Show("导入的数据中包含了相同的账号:" + uid,"Warming",MessageBoxButton.OK,MessageBoxImage.Warning);
                                }
                                m_uid2idx.Add(uid.ToLower(), m_pwcdataset.Tables[0].Rows.IndexOf(newRow));
                            }
                        }

                        int begintick = System.Environment.TickCount;
                        cmd.CommandText = "SELECT * FROM " + pwcuidtable;
                        SQLiteDataAdapter dataadapter = new SQLiteDataAdapter(cmd);
                        SQLiteCommandBuilder cmdbuilder = new SQLiteCommandBuilder(dataadapter);
                        dataadapter.InsertCommand = cmdbuilder.GetInsertCommand();
                        using (SQLiteTransaction ts = m_pwcconn.BeginTransaction())
                        {
                            dataadapter.Update(m_pwcdataset);
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

        void load_cseuidtxt(string uidtxt)
        {
            try
            {
                if (File.Exists(uidtxt))
                {
                    using (SQLiteCommand cmd = new SQLiteCommand(m_cseconn))
                    {
                        m_csedataset.Tables[0].Clear();

                        cmd.CommandText = "DELETE FROM " + cse_uidtable;
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

                                DataRow newRow = m_csedataset.Tables[0].Rows.Add();
                                newRow[cse_column_uid] = uid;
                                newRow[cse_column_password] = pw;

                            }
                        }

                        int begintick = System.Environment.TickCount;
                        cmd.CommandText = "SELECT * FROM " + cse_uidtable;
                        SQLiteDataAdapter dataadapter = new SQLiteDataAdapter(cmd);
                        SQLiteCommandBuilder cmdbuilder = new SQLiteCommandBuilder(dataadapter);
                        dataadapter.InsertCommand = cmdbuilder.GetInsertCommand();
                        using (SQLiteTransaction ts = m_cseconn.BeginTransaction())
                        {
                            dataadapter.Update(m_csedataset);
                            ts.Commit();
                        }
                        m_logger.Debug("cse数据库更新:" + (System.Environment.TickCount - begintick) + "ms");
                    }
                    m_csewalkiterator = 0;
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
                m_boshutdwon = true;

                m_uidbacker.BackUp();
                Thread.Sleep(2000);
                m_pwcconn.Close();
                CLogger.StopAllLoggers();
                m_saveThread.Join(3000);
            }
            catch (System.Exception)
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

        private void btnExportCpResult_Click(object sender, RoutedEventArgs e)
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
                        for (int i = 0; i < m_pwcdataset.Tables[0].Rows.Count; ++i)
                        {
                            string uid = m_pwcdataset.Tables[0].Rows[i][column_uid].ToString();
                            string pwd = m_pwcdataset.Tables[0].Rows[i][column_password].ToString();
                            string status = m_pwcdataset.Tables[0].Rows[i][column_status].ToString();

                            writer.WriteLine(uid + "----" + pwd + "----" + status);
                        }
                    }
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UidGrid.DragEnter += new DragEventHandler(statusGrid_DragEnter);
            UidGrid.Drop += new DragEventHandler(statusGrid_Drop);

            CheckUidSafeGrid.DragEnter += new DragEventHandler(CheckUidSafeGrid_DragEnter);
            CheckUidSafeGrid.Drop += new DragEventHandler(CheckUidSafeGrid_Drop);

            int workerThreads = 0;
            int completePortsThreads = 0;
            ThreadPool.GetMaxThreads(out workerThreads, out completePortsThreads);
            ThreadPool.SetMaxThreads(workerThreads*10, completePortsThreads*10);
        }

        void CheckUidSafeGrid_Drop(object sender, DragEventArgs e)
        {
            string fileName = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            load_cseuidtxt(fileName);
        }

        void CheckUidSafeGrid_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Link;
            else
                e.Effects = DragDropEffects.None;     
        }

        void firstStart()
        {
            CpWorker.DBpwd = MyDes.Decode(m_safedbpwd, m_safekey);
            SeWorker.DBpwd = CpWorker.DBpwd;

            if (!String.IsNullOrEmpty(m_safelg)
                && !String.IsNullOrEmpty(m_safepw))
            {
                load_captcha_from_safetxt(m_safelg, m_constkey, m_lgcaptcha);
                load_captcha_from_safetxt(m_safepw, m_constkey, m_pwcaptcha);

                save_captcha_db_from_dic(m_lgcaptcha, CpWorker.DBpwd, "lg");
                save_captcha_db_from_dic(m_pwcaptcha, CpWorker.DBpwd, "pwc");
            }
            else
            {
                load_captcha_from_db(m_lgcaptcha, CpWorker.DBpwd, "lg");
                load_captcha_from_db(m_pwcaptcha, CpWorker.DBpwd, "pwc");
            }

            CpWorker.StartRunTime = System.Environment.TickCount;
            SeWorker.StartRunTime = CpWorker.StartRunTime;
        }

        private void btnStartCheckSafe_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(md5js))
            {
                MessageBox.Show(lgxml + "文件不存在", "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (m_bofirststart)
            {
                firstStart();
                m_bofirststart = false;
            }

            if (SeWorker.KeepRunTime != 0)
            {
                if (System.Environment.TickCount - SeWorker.StartRunTime > SeWorker.KeepRunTime)
                {
                    if (File.Exists(captchadb))
                    {
                        File.Delete(captchadb);
                    }
                    m_lgcaptcha.Clear();
                    m_pwcaptcha.Clear();

                    for (int i = 0; i < m_cpworkers.Count; ++i)
                    {
                        m_seworkers[i].m_lgcaptcha.Clear();
                        m_seworkers[i].m_pwcaptcha.Clear();
                    }
                    MessageBox.Show("试用到期了!!");
                    return;
                }
            }

            int num = int.Parse(tbxSeWorkerNumber.Text);
            for (int i = 0; i < num; ++i)
            {
                if (m_seworkers.Count <= i)
                {
                    SeWorker worker = new SeWorker(m_lgcaptcha, m_pwcaptcha, m_safekey);
                    worker.FinishTask += new Action<SeWorker, string, string, string, bool, int, int>(seworker_FinishTask);
                    m_seworkers.Add(worker);
                }

                DataRow nextuidrow = IteratorNextRow(m_csedataset.Tables[0], cse_column_status, ref m_csewalkiterator);
                if (nextuidrow != null)
                {
                    string uid = (string)nextuidrow[cse_column_uid];
                    string pwd = (string)nextuidrow[cse_column_password];
                    m_seworkers[i].BeginTask(uid.ToLower().Trim(), pwd.Trim(), nextuidrow);
                    nextuidrow[cse_column_status] = status_ready;

                    //m_logger.Debug("begin uid:" + uid + " pwd:" + pwd + " newpwd:" + newpwd);
                }
                else
                {
                    m_seworkers[i].IsWorking = false;
                }
            }

            btnStartCheckSafe.IsEnabled = false;
            tbxSeWorkerNumber.IsEnabled = false;

            CheckSeWorkerStatus();
        }

        void seworker_FinishTask(SeWorker seworker, string uid, string pwd, string status, bool has_idcard, int yue, int userpoint)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(seworker_OnTaskFinish);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(seworker_RunWorkerCompleted);
            worker.RunWorkerAsync(new Tuple<SeWorker, string, string, string, bool, int, int>(seworker, uid, pwd, status, has_idcard, yue, userpoint));
        }

        void seworker_OnTaskFinish(object sender, DoWorkEventArgs e)
        {
            try
            {
                var arg = e.Argument as Tuple<SeWorker, string, string, string, bool, int, int>;
                SeWorker seworker = arg.Item1;
                string uid = arg.Item2;
                string pwd = arg.Item3;
                string status = arg.Item4;
                bool has_idcard = arg.Item5;
                int yue = arg.Item6;
                int userpoint = arg.Item7;

                lock (m_saveSeObjList)
                {
                    m_saveSeObjList.Add(arg);
                }

                e.Result = arg;
            }
            catch (System.Exception ex)
            {
                m_logger.Error(ex.ToString());
            }
        }

        void seworker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var arg = e.Result as Tuple<SeWorker, string, string, string, bool, int, int>;
            SeWorker seworker = arg.Item1;
            string uid = arg.Item2;
            string pwd = arg.Item3;
            string status = arg.Item4;
            bool has_idcard = arg.Item5;
            int yue = arg.Item6;
            int userpoint = arg.Item7;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                DataRow targetrow = seworker.workArgument as DataRow;
                targetrow[cse_column_status] = status;
                if (status == "查询成功")
                {
                    targetrow[cse_column_se_idcard] = has_idcard ? "有证" : "无证";
                    targetrow[cse_column_se_jifen] = userpoint;
                    targetrow[cse_column_se_balance] = yue;
                }


                try
                {
                    if (SeWorker.KeepRunTime != 0 && btnStartCheckSafe.IsEnabled)
                    {
                        if (System.Environment.TickCount - SeWorker.StartRunTime > SeWorker.KeepRunTime)
                        {
                            btnStop.IsEnabled = false;
                            MessageBox.Show("试用时间到了!!");
                        }
                    }

                    if (btnStopCheckSafe.IsEnabled == true)
                    {
                        DataRow nextuidrow = IteratorNextRow(m_csedataset.Tables[0], cse_column_status, ref m_csewalkiterator);
                        if (nextuidrow != null)
                        {
                            string nextuid = (string)nextuidrow[cse_column_uid];
                            string nextpwd = (string)nextuidrow[cse_column_password];
                            seworker.BeginTask(nextuid.ToLower().Trim(), nextpwd.Trim(), nextuidrow);
                            nextuidrow[cse_column_status] = status_ready;
                        }
                        else
                        {
                            seworker.IsWorking = false;
                            CheckSeWorkerStatus();
                        }
                    }
                    else
                    {
                        seworker.IsWorking = false;
                        CheckSeWorkerStatus();
                    }

                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }));
        }

        void CheckSeWorkerStatus()
        {
            foreach (SeWorker w in m_seworkers)
            {
                if (w.IsWorking == true)
                {
                    return;
                }
            }
            btnStartCheckSafe.IsEnabled = true;
            btnStopCheckSafe.IsEnabled = true;
            tbxSeWorkerNumber.IsEnabled = true;
        }

        private void btnStopCheckSafe_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnStopCheckSafe.IsEnabled = false;
                CheckSeWorkerStatus();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void btnExportCheckSafe_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.Filter = "保存类型 |*.txt";
            dlg.FilterIndex = 1;
            if (dlg.ShowDialog() == true)
            {
                using (FileStream fs = new FileStream(dlg.FileName, FileMode.Create))
                {
                    using (StreamWriter writer = new StreamWriter(fs))
                    {
                        for (int i = 0; i < m_csedataset.Tables[0].Rows.Count; ++i)
                        {
                            string uid = m_csedataset.Tables[0].Rows[i][cse_column_uid].ToString();
                            string pwd = m_csedataset.Tables[0].Rows[i][cse_column_password].ToString();
                            string status = m_csedataset.Tables[0].Rows[i][cse_column_status].ToString();
                            string idcard = m_csedataset.Tables[0].Rows[i][cse_column_se_idcard].ToString();
                            string jifen = m_csedataset.Tables[0].Rows[i][cse_column_se_jifen].ToString();
                            string yue = m_csedataset.Tables[0].Rows[i][cse_column_se_balance].ToString();

                            writer.WriteLine(uid + "----" + pwd + "----" + status + "----" + idcard + "----" + jifen + "----" + yue);
                        }
                    }
                }
            }
        }
    }
}
