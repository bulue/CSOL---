using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;


namespace 档案汇总
{
    delegate void OnClinetMsg(byte[] msg, AmSession session);

    class AmServer : QzServer
    {
        public List<AmSession> m_Sessions = new List<AmSession>();
        private bool m_boSessionNumChanged;

        private Dictionary<string, int> macCheck = new Dictionary<string, int>();
        private Dictionary<string, string> m_Token = new Dictionary<string, string>();
        private Dictionary<string, userinfo> m_userinfos = new Dictionary<string, userinfo>();
        private Dictionary<string, userinfo> m_checkuserinfos;
        private Dictionary<string, long> m_accountRecord = new Dictionary<string, long>(); //<账号,时间>
        private Dictionary<string, userinfo> m_userinfosavelist = new Dictionary<string, userinfo>();

        Grid m_mainForm;

        enum working_state
        {
            normal = 0,
            advanced = 1,
            finish = 2,
        }
        working_state _working_state = working_state.normal;
        bool m_boNeedSaveData;
        int m_NextSaveTick;
        int _lastCheckSessionActive;

        List<Tuple<byte[], AmSession>> m_msglist = new List<Tuple<byte[], AmSession>>();
        Thread m_WorkThread;
        bool m_serverIsStop;

        private const string _saveDBFile = "Data.db";
        private const string _saveDBTable = "userdb";

        public Dictionary<string, userinfo> UserInfos
        {
            get { return m_userinfos; }
        }

        public int MacCount
        {
            get { return macCheck.Count; }
        }

        public bool IsSessionNumChanged
        {
            get { return m_boSessionNumChanged; }
            set { m_boSessionNumChanged = value; }
        }

        public AmServer(IPAddress localaddr, int port)
            : base(localaddr, port)
        {
            m_serverIsStop = false;
        }

        public void SetMainForm(Form form)
        {
            m_mainForm = (Grid)form;
        }

        public override void Start()
        {
            try
            {
                base.Start();
                m_WorkThread = new Thread(new ThreadStart(WorkThreadRun));
                m_WorkThread.Start();
            }
            catch (Exception ex)
            {
                Global.logger.Error(ex.ToString());
            }
        }

        public override void Stop()
        {
            try
            {
                SaveData();
                if (!m_WorkThread.Join(3000))
                {
                    m_WorkThread.Abort();
                }
                base.Stop();
            }
            catch (Exception ex)
            {
                Global.logger.Debug(ex.ToString());
            }
        }

        protected void WorkThreadRun()
        {
            do
            {
                int runBeginTick = System.Environment.TickCount;
                try
                {
                    lock (m_msglist)
                    {
                        foreach (var item in m_msglist)
                        {
                            OnClientMsg(item.Item1, item.Item2);
                        }
                        m_msglist.Clear();
                    }
                    if (m_boNeedSaveData && System.Environment.TickCount > m_NextSaveTick)
                    {
                        SaveData();
                        m_NextSaveTick = System.Environment.TickCount + 30 * 1000;
                    }

                    if (System.Environment.TickCount - _lastCheckSessionActive > 10 * 1000)
                    {
                        _lastCheckSessionActive = System.Environment.TickCount;
                        lock (m_Sessions)
                        {
                            Global.logger.Debug("开始校验连接活跃,发送心跳包");
                            foreach (AmSession session in m_Sessions)
                            {
                                if (String.IsNullOrEmpty(session.m_mac) 
                                    && (Environment.TickCount - session.m_CreateTime > 10 * 1000))
                                {
                                    Global.logger.Debug("连接:{0},长时间未验证,主动断开!", session.RemoteIp);
                                    session.Close();
                                }
                                //else if (Environment.TickCount - session.m_LastCheckActiveTime > 30 * 1000)
                                //{
                                //    Global.logger.Debug("连接:{0},长时间未回复!", session.RemoteIp);
                                //    session.Close();
                                //}
                                else
                                {
                                    session.Send("0");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Global.logger.Error(ex.ToString());
                }
                int run_tick = System.Environment.TickCount - runBeginTick;
                Thread.Sleep(50 - run_tick > 0 ? 50 - run_tick : 0);
            } while (true);
        }

        protected override QzTcpClinet CreateClient(TcpClient client)
        {
            AmSession session = new AmSession(client);
            session.OnMsgRcive = (msg) =>
            {
                lock (this)
                {
                    m_msglist.Add(new Tuple<byte[], AmSession>(msg, session));
                }
            };
            session.OnDisconnet = () =>
            {
                lock (m_Sessions)
                {
                    m_Sessions.Remove(session);
                }
            };

            lock (m_Sessions)
            {
                m_Sessions.Add(session);
            }
            return session;
        }

        protected void OnClientMsg(byte[] msgbytes, AmSession c)
        {
            string s = Encoding.Default.GetString(msgbytes);
            Global.logger.Debug("Recv:" + s);

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

                m_mainForm.UpdateSessionState(mac, code, c.RemoteIp, ver);

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
                                    Global.logger.Debug("异常提示>>>" + "待分配的账号已经剔除!!!!!");
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
                                    info.loginip = c.RemoteIp;

                                    m_checkuserinfos[accName] = info;
                                    m_userinfos[accName] = info;
                                }

                            }

                            if (accName == "" || passWord == "")
                            {
                                if (_working_state == working_state.finish
                                    || _working_state == working_state.advanced)
                                {
                                    Global.logger.Debug("所有账号已经登陆完毕>>>>>>");
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
                                int zone = m_mainForm.GetSelectZoneId();
                                int state = m_mainForm.GetSelectState();
                                c.Send("2$" + accName + "$" + passWord + "$" + zone + "$" + state);

                                Global.logger.Debug("socket:{0};ip:{1};({2}->{3})", c.getTcpClient().GetHashCode()
                                    , c.RemoteIp
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
                                m_userinfosavelist[accName] = info;

                                m_mainForm.UpdateUserInfo(info);
                            }

                            m_mainForm.UpdateProcessState(String.Format("进度:({0}/{1})",
                                (m_userinfos.Count - m_checkuserinfos.Count), m_userinfos.Count));

                            //SaveData();
                            m_boNeedSaveData = true;

                            m_mainForm.UpdateSessionCompleteState(c.m_mac, isOk);
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
                            if (m_mainForm.GetRoutineIpCheckd())
                            {
                                string[] sp1 = c.RemoteIp.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
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
                        } break;
                    case "1008":
                        {
                            Global.logger.Debug("心跳包");
                        } break;
                    case "7001":
                        {
                            const string errDir = "错误记录";
                            string imgstr = split[1];
                            byte[] bytes = Convert.FromBase64String(imgstr);
                            if (!Directory.Exists(errDir))
                            {
                                Directory.CreateDirectory(errDir);
                            }
                            DirectoryInfo di = new DirectoryInfo(errDir);
                            FileInfo[] files = di.GetFiles();
                            if (files.Length >= 200)
                            {
                                Array.Sort(files, (a, b) => a.LastWriteTime.CompareTo(b.LastWriteTime));
                                File.Delete(files[0].FullName);
                            }
                            File.WriteAllBytes("错误记录/" + Path.GetRandomFileName() + ".bmp", bytes);
                        } break;
                    case "7002":
                        {

                        } break;
                    default:
                        {
                            Global.logger.Debug("收到异常消息:{0}", String.IsNullOrEmpty(s) ? "" : s);
                            //c.handle.Shutdown(SocketShutdown.Both);
                            //c.handle.Disconnect(false);
                            // c.handle.Dispose();
                            //lock (Sever.m_Clinets)
                            //{
                            //Sever.bChanged = true;
                            //Sever.m_Clinets.Remove(c);
                            //}
                        } break;
                };
            }
        }


        public void LoadData()
        {
            try
            {
                if (!File.Exists(_saveDBFile))
                {
                    SQLiteConnection.CreateFile(_saveDBFile);
                }
                SQLiteConnectionStringBuilder connstr = new SQLiteConnectionStringBuilder();
                connstr.DataSource = _saveDBFile;
                using (SQLiteConnection sqlcon = new SQLiteConnection())
                {
                    sqlcon.ConnectionString = connstr.ToString();
                    sqlcon.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand(sqlcon))
                    {
                        cmd.CommandText = "select count(*) from sqlite_master where type = 'table' and name = '" + _saveDBTable + "'";
                        Int64 result = (Int64)cmd.ExecuteScalar();
                        if (result == 0)
                        {
                            cmd.CommandText = "CREATE TABLE " + _saveDBTable + @"( 账号 varchar(30) primary key,密码 varchar(50),失败次数 integer
                                    ,签到时间 varchar(50),状态 varchar(50), 签到 integer, 芯片数量 integer, 一线牵神器 varchar(50), 一线牵积分 varchar(50)
                                    ,登陆机IP varchar(50), 登陆机代号 varchar(50))";
                            cmd.ExecuteNonQuery();
                        }
                    }

                    using (SQLiteCommand cmd = new SQLiteCommand(sqlcon))
                    {
                        cmd.CommandText = "SELECT * FROM " + _saveDBTable;
                        SQLiteDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            userinfo info = new userinfo();
                            info.username = (string)reader["账号"].ToString();
                            info.password = (string)reader["密码"].ToString();
                            info.failedcount = (int)(Int64)(reader["失败次数"] is DBNull ? (Int64)0 : reader["失败次数"]);
                            info.checktime = (string)reader["签到时间"].ToString();
                            info.status = (string)reader["状态"].ToString();
                            info.bocheck = (int)(Int64)(reader["签到"] is DBNull ? (Int64)0 : reader["签到"]);
                            info.chipcout = (int)(Int64)(reader["芯片数量"] is DBNull ? (Int64)0 : reader["芯片数量"]);
                            info.gun = (string)reader["一线牵神器"].ToString();
                            info.jifen = (string)reader["一线牵积分"].ToString();
                            info.loginip = (string)reader["登陆机IP"].ToString();
                            info.logincode = reader["登陆机代号"].ToString();

                            if (info.username != "" && info.password != "")
                            {
                                if (!m_userinfos.ContainsKey(info.username))
                                {
                                    m_userinfos.Add(info.username, info);
                                }
                            }
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
                m_mainForm.UpdateProcessState("进度:" + (m_userinfos.Count - m_checkuserinfos.Count) + "/" + m_userinfos.Count);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public void SaveData()
        {
            try
            {
                int tick = System.Environment.TickCount;
                if (!File.Exists(_saveDBFile))
                {
                    SQLiteConnection.CreateFile(_saveDBFile);
                }
                SQLiteConnectionStringBuilder connstr = new SQLiteConnectionStringBuilder();
                connstr.DataSource = _saveDBFile;
                using (SQLiteConnection sqlcon = new SQLiteConnection())
                {
                    sqlcon.ConnectionString = connstr.ToString();
                    sqlcon.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand(sqlcon))
                    {
                        cmd.CommandText = "select count(*) from sqlite_master where type = 'table' and name = '" + _saveDBTable + "'";
                        Int64 result = (Int64)cmd.ExecuteScalar();
                        if (result == 0)
                        {
                            cmd.CommandText = "CREATE TABLE " + _saveDBTable + @"( 账号 varchar(30) primary key,密码 varchar(50),失败次数 integer
                                    ,签到时间 varchar(50),状态 varchar(50), 签到 integer, 芯片数量 integer, 一线牵神器 varchar(50), 一线牵积分 varchar(50)
                                    ,登陆机IP varchar(50), 登陆机代号 varchar(50))";
                            cmd.ExecuteNonQuery();
                        }
                    }

                    using (SQLiteTransaction ts = sqlcon.BeginTransaction())
                    {
                        int nSaved = 0;
                        foreach (userinfo info in m_userinfosavelist.Values)
                        {
                            using (SQLiteCommand cmd = new SQLiteCommand(sqlcon))
                            {
                                cmd.CommandText = "UPDATE " + _saveDBTable + @" SET 密码=@密码,失败次数=@失败次数,签到时间=@签到时间,状态=@状态,
                                 签到=@签到, 芯片数量=@芯片数量, 一线牵神器=@一线牵神器, 一线牵积分=@一线牵积分, 登陆机IP=@登陆机IP, 登陆机代号=@登陆机代号
                                 WHERE 账号=@账号";

                                cmd.Parameters.Add(new SQLiteParameter("@账号", info.username));
                                cmd.Parameters.Add(new SQLiteParameter("@密码", info.password));
                                cmd.Parameters.Add(new SQLiteParameter("@失败次数", info.failedcount));
                                cmd.Parameters.Add(new SQLiteParameter("@签到时间", info.checktime));
                                cmd.Parameters.Add(new SQLiteParameter("@签到", info.bocheck));
                                cmd.Parameters.Add(new SQLiteParameter("@状态", info.status));
                                cmd.Parameters.Add(new SQLiteParameter("@芯片数量", info.chipcout));
                                cmd.Parameters.Add(new SQLiteParameter("@一线牵神器", info.gun));
                                cmd.Parameters.Add(new SQLiteParameter("@一线牵积分", info.jifen));
                                cmd.Parameters.Add(new SQLiteParameter("@登陆机IP", info.loginip));
                                cmd.Parameters.Add(new SQLiteParameter("@登陆机代号", info.logincode));
                                if (cmd.ExecuteNonQuery() == 0)
                                {
                                    cmd.Parameters.Clear();
                                    cmd.CommandText = "INSERT INTO " + _saveDBTable + @"(账号,密码,失败次数,签到时间,状态,签到,芯片数量,一线牵神器,一线牵积分,
                                    登陆机IP, 登陆机代号) VALUES(@账号,@密码,@失败次数,@签到时间,@状态,@签到,@芯片数量,@一线牵神器,@一线牵积分,@登陆机IP, @登陆机代号)";

                                    cmd.Parameters.Add(new SQLiteParameter("@账号", info.username));
                                    cmd.Parameters.Add(new SQLiteParameter("@密码", info.password));
                                    cmd.Parameters.Add(new SQLiteParameter("@失败次数", info.failedcount));
                                    cmd.Parameters.Add(new SQLiteParameter("@签到时间", info.checktime));
                                    cmd.Parameters.Add(new SQLiteParameter("@状态", info.status));
                                    cmd.Parameters.Add(new SQLiteParameter("@签到", info.bocheck));
                                    cmd.Parameters.Add(new SQLiteParameter("@芯片数量", info.chipcout));
                                    cmd.Parameters.Add(new SQLiteParameter("@一线牵神器", info.gun));
                                    cmd.Parameters.Add(new SQLiteParameter("@一线牵积分", info.jifen));
                                    cmd.Parameters.Add(new SQLiteParameter("@登陆机IP", info.loginip));
                                    cmd.Parameters.Add(new SQLiteParameter("@登陆机代号", info.logincode));
                                    if (cmd.ExecuteNonQuery() == 0)
                                    {
                                        Global.logger.Error("存档错误:{0}", info.username);
                                    }
                                    else
                                    {
                                        nSaved++;
                                    }
                                }
                                else
                                {
                                    nSaved++;
                                }
                            }
                        }
                        Global.logger.Debug("存档成功，更新数据{0}条,耗时{1}秒,错误{2}条", nSaved, System.Environment.TickCount - tick
                            , m_userinfosavelist.Count - nSaved);
                        m_userinfosavelist.Clear();
                        ts.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "错误");
            }

            m_boNeedSaveData = false;
        }

        public void ServerClearLoginData()
        {
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

            m_userinfosavelist.Clear();
            _working_state = working_state.normal;


            if (!File.Exists(_saveDBFile))
            {
                SQLiteConnection.CreateFile(_saveDBFile);
            }
            SQLiteConnectionStringBuilder connstr = new SQLiteConnectionStringBuilder();
            connstr.DataSource = _saveDBFile;
            using (SQLiteConnection sqlcon = new SQLiteConnection())
            {
                sqlcon.ConnectionString = connstr.ToString();
                sqlcon.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(sqlcon))
                {
                    cmd.CommandText = "UPDATE " + _saveDBTable + @" SET 失败次数=@失败次数,签到时间=@签到时间,状态=@状态,
                                 签到=@签到, 芯片数量=@芯片数量, 一线牵神器=@一线牵神器, 一线牵积分=@一线牵积分, 登陆机IP=@登陆机IP, 登陆机代号=@登陆机代号
                                 ";
                    cmd.Parameters.Add(new SQLiteParameter("@失败次数", 0));
                    cmd.Parameters.Add(new SQLiteParameter("@签到时间", ""));
                    cmd.Parameters.Add(new SQLiteParameter("@签到", 0));
                    cmd.Parameters.Add(new SQLiteParameter("@状态", ""));
                    cmd.Parameters.Add(new SQLiteParameter("@芯片数量", 0));
                    cmd.Parameters.Add(new SQLiteParameter("@一线牵神器", ""));
                    cmd.Parameters.Add(new SQLiteParameter("@一线牵积分", 0));
                    cmd.Parameters.Add(new SQLiteParameter("@登陆机IP", ""));
                    cmd.Parameters.Add(new SQLiteParameter("@登陆机代号", ""));
                    cmd.ExecuteNonQuery();
                }
            }
            m_mainForm.ClearLoginData();
        }
    }
}
