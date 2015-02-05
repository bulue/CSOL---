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

namespace PwcTool
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            m_logger = CLogger.FromFolder("pwclog");

            //load_uidtxt(uidtxt, m_uidqueue);
            m_logger.Debug("uid:" + m_uidqueue.Count);

            statusGrid.ItemsSource = m_statuslist;
        }

        CLogger m_logger;
        Random m_rgen = new Random(System.Environment.TickCount);

        const string lgxml = "lg.xml";
        const string pwxml = "pw.xml";
        const string uidtxt = "uid.txt";
        const string md5js = "md5.js";
        const string datasource = "uid.db";
        const string uidtable = "uidlogin";

        Dictionary<string, string> m_lgcaptcha = new Dictionary<string, string>();
        Dictionary<string, string> m_pwcaptcha = new Dictionary<string, string>();
        Queue<Tuple<string, string>> m_uidqueue = new Queue<Tuple<string, string>>();

        class loginstatus
        {
            public string uid;
            public string pwd;
            public string status;

            public loginstatus(string u, string p, string s)
            {
                uid = u;
                pwd = p;
                status = s;
            }
        }

        List<loginstatus> m_statuslist = new List<loginstatus>();
        string m_fixpwd = "";
        string m_randompwd = "";

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

            RandomPwdTextbox.IsEnabled = false;
            FixPwdTextbox.IsEnabled = false;
            button1.IsEnabled = false;

            if (!File.Exists(lgxml))
            {
                MessageBox.Show(lgxml + "文件不存在", "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!File.Exists(pwxml))
            {
                MessageBox.Show(lgxml + "文件不存在", "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!File.Exists(md5js))
            {
                MessageBox.Show(lgxml + "文件不存在", "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            load_captcha_xml(lgxml, m_lgcaptcha);
            load_captcha_xml(pwxml, m_pwcaptcha);
            m_logger.Debug("lg:" + m_lgcaptcha.Count + " pw:" + m_pwcaptcha.Count);

            CpWorker worker = new CpWorker(m_lgcaptcha, m_pwcaptcha);
            worker.TryTaskChangePwd("q6218100", "gsrhmp1", "gsrhmp");
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

        void load_uidtxt(string uidtxt, Queue<Tuple<string, string>> uidlist)
        {
            try
            {
                if (File.Exists(uidtxt))
                {
                    System.Data.SQLite.SQLiteConnection.CreateFile(datasource);
                    System.Data.SQLite.SQLiteConnection conn = new System.Data.SQLite.SQLiteConnection();
                    System.Data.SQLite.SQLiteConnectionStringBuilder connstr = new System.Data.SQLite.SQLiteConnectionStringBuilder();
                    System.Data.SQLite.SQLiteCommand cmd = new System.Data.SQLite.SQLiteCommand();

                    connstr.DataSource = datasource;
                    conn.ConnectionString = connstr.ToString();
                    conn.Open();
                    cmd.CommandText = "select count(*) from sqlite_master where type = 'table' and name = '" + uidtable + "'";
                    cmd.Connection = conn;

                    if (0 == cmd.ExecuteNonQuery())
                    {
                        cmd.CommandText = "CREATE TABLE " + uidtable + "(uid varchar(20),password varchar(20))";
                        cmd.ExecuteNonQuery();
                    }
                    else
                    {
                        cmd.CommandText = "DELETE FROM " + uidtable;
                        cmd.ExecuteNonQuery();
                    }

                    cmd.CommandText = "INSERT INTO " + uidtable + " (uid,password)" + " VALUES (@uid,@passowrd)";
                    SQLiteParameter[] parameters = { new SQLiteParameter("@uid", DbType.String, 20),
                                                 new SQLiteParameter("@passowrd", DbType.String, 20)};

                    string[] lines = File.ReadAllLines(uidtxt, Encoding.GetEncoding("GB2312"));
                    string pattern = @"[0-9]*[^a-zA-z0-9]*(\w+)[^a-zA-z0-9]*(\w+)";

                    foreach (string line in lines)
                    {
                        Match mt = Regex.Match(line, pattern);
                        if (mt.Groups.Count == 3)
                        {
                            string uid = mt.Groups[1].ToString();
                            string pw = mt.Groups[2].ToString();
                            uidlist.Enqueue(new Tuple<string, string>(uid, pw));

                            cmd.Parameters.Clear();
                            cmd.Parameters.Add(new SQLiteParameter("@uid", uid));
                            cmd.Parameters.Add(new SQLiteParameter("@passowrd", pw));
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            try
            {
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
                string datasource = "test.db";
                System.Data.SQLite.SQLiteConnection.CreateFile(datasource);
                //连接数据库
                System.Data.SQLite.SQLiteConnection conn = new System.Data.SQLite.SQLiteConnection();
                System.Data.SQLite.SQLiteConnectionStringBuilder connstr = new System.Data.SQLite.SQLiteConnectionStringBuilder();
                connstr.DataSource = datasource;
                //connstr.Password = "1";//设置密码，SQLite ADO.NET实现了数据库密码保护
                conn.ConnectionString = connstr.ToString();
                conn.Open();
                //创建表
                System.Data.SQLite.SQLiteCommand cmd = new System.Data.SQLite.SQLiteCommand();
                string sql = "CREATE TABLE test(username varchar(20),password varchar(20))";
                cmd.CommandText = sql;
                cmd.Connection = conn;
                cmd.ExecuteNonQuery();
                //插入数据
                sql = "INSERT INTO test VALUES('a','b')";
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
                //取出数据
                sql = "SELECT * FROM test";
                cmd.CommandText = sql;
                System.Data.SQLite.SQLiteDataReader reader = cmd.ExecuteReader();
                StringBuilder sb = new StringBuilder();
                while (reader.Read())
                {
                    sb.Append("username:").Append(reader.GetString(0)).Append("n")
                    .Append("password:").Append(reader.GetString(1));
                }
                MessageBox.Show(sb.ToString());
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
            else e.Effects = DragDropEffects.None;             

            //if (e.Data.GetDataPresent(DataFormats.FileDrop))
            //    e.Effects = DragDropEffects.Link;
            //else
            //    e.Effects = DragDropEffects.None;
        }

        private void statusGrid_Drop(object sender, DragEventArgs e)
        {
            string fileName = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            load_uidtxt(fileName, m_uidqueue);
        }
    }
}
