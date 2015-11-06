using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CommonQ;
using SharpPcap;

namespace popkart_capture
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public class PopKartCaptcha
        {
            public string account;
            public System.Net.IPAddress remoteip;
            public uint acknumber;
            public byte[] httpBuf;
            public string httpGet;
            public byte[] imgBuf;
        }

        public class PopKartRecord
        {
            private string _time;
            private string _record;
            private BitmapImage _img;

            public string Time
            {
                get { return _time; }
                set { _time = value; }
            }

            public string Record
            {
                get { return _record; }
                set { _record = value; }
            }

            public BitmapImage Img
            {
                get { return _img; }
                set { _img = value; }
            }
        }

        static MainWindow _mainWinow = null;
        static Dictionary<string, PopKartCaptcha> m_dPopKartCaptcha = new Dictionary<string, PopKartCaptcha>(); //<account, popkart_captcha>
        static CLogger logger;
        static Computer _Compputer;
        static string _rKey;

        public MainWindow()
        {
            InitializeComponent();
            _rKey = RandomString.Next(8, "a-zA-Z0-9");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                logger = CLogger.FromFolder(@"log/log");
                /* Retrieve the device list */
                var devices = CaptureDeviceList.Instance;

                /*If no device exists, print error */
                if (devices.Count < 1)
                {
                    MessageBox.Show("网络适配器异常!", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                int i = 0;
                foreach (var dev in devices)
                {
                    /* Description */
                    logger.Debug("{0}) {1} {2}", i, dev.Name, dev.Description);
                    i++;
                }
                _Compputer = Computer.Instance();

                _mainWinow = this;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StopAllDevice()
        {
            btnStop.IsEnabled = false;
            bool hasStartDevice = false;
            var devices = CaptureDeviceList.Instance;
            for (int i = 0; i < devices.Count; ++i)
            {
                var device = devices[i];
                if (device.Started)
                {
                    hasStartDevice = true;
                    device.StopCaptureTimeout = new System.TimeSpan(0, 0, 0, 5);
                    device.Close();
                }
            }
            if (!hasStartDevice)
            {
                btnStop.IsEnabled = true;
            }
        }

        private void Button_Click_Stop(object sender, RoutedEventArgs e)
        {
            StopAllDevice();
        }

        private void Button_Click_Start(object sender, RoutedEventArgs e)
        {
            var devices = CaptureDeviceList.Instance;
            for (int i = 0; i < devices.Count; ++i)
            {
                var device = devices[i];
                device.OnPacketArrival +=
                   new PacketArrivalEventHandler(device_OnPacketArrival);
                device.OnCaptureStopped += new CaptureStoppedEventHandler(delegate
                {
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        bool allstop = true;
                        for (int ii = 0; ii < devices.Count; ++ii)
                        {
                            if (devices[ii].Started)
                            {
                                allstop = false;
                            }
                        }
                        if (allstop)
                        {
                            btnStart.IsEnabled = true;
                            btnStop.IsEnabled = true;
                        }
                    }));
                });
                // Open the device for capturing
                int readTimeoutMilliseconds = 1000;
                device.Open(DeviceMode.Promiscuous, readTimeoutMilliseconds);

                //tcpdump filter to capture only TCP/IP packets
                string filter = "ip and tcp";
                device.Filter = filter;

                device.StartCapture();
            }

            btnStart.IsEnabled = false;
        }

        private static void device_OnPacketArrival(object sender, CaptureEventArgs e)
        {
            var time = e.Packet.Timeval.Date;
            var len = e.Packet.Data.Length;

            var packet = PacketDotNet.Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data);

            var tcpPacket = (PacketDotNet.TcpPacket)packet.Extract(typeof(PacketDotNet.TcpPacket));
            if (tcpPacket != null)
            {
                var ipPacket = (PacketDotNet.IpPacket)tcpPacket.ParentPacket;
                System.Net.IPAddress srcIp = ipPacket.SourceAddress;
                System.Net.IPAddress dstIp = ipPacket.DestinationAddress;
                int srcPort = tcpPacket.SourcePort;
                int dstPort = tcpPacket.DestinationPort;

                if (dstPort == 80)
                {
                    if (ipPacket.TotalLength - 40 >= 3)
                    {
                        if (tcpPacket.Bytes[20] == 'G'
                             && tcpPacket.Bytes[20 + 1] == 'E'
                             && tcpPacket.Bytes[20 + 2] == 'T')
                        {
                            string s = System.Text.Encoding.Default.GetString(tcpPacket.Bytes, 20, tcpPacket.Bytes.Length - 20);
                            string pattern = "GET /client.ashx\\?code=0&fid=SVG013&uid=(\\w+) HTTP/1.1";
                            Match mt = Regex.Match(s, pattern);
                            if (mt.Groups.Count == 2)
                            {
                                PopKartCaptcha t = new PopKartCaptcha();
                                t.account = mt.Groups[1].ToString();
                                t.acknumber = tcpPacket.SequenceNumber + (uint)ipPacket.TotalLength - 40;
                                t.httpGet = s;
                                t.remoteip = dstIp;
                                t.httpBuf = null;
                                t.imgBuf = null;
                                m_dPopKartCaptcha[t.account] = t;
                            }
                        }
                    }
                }

                if (srcPort == 80)
                {
                    if (tcpPacket.Bytes.Length > 20)
                    {
                        foreach (var t in m_dPopKartCaptcha.Values)
                        {
                            //var t = g;
                            if (t.acknumber == tcpPacket.AcknowledgmentNumber
                                && t.remoteip.Equals(srcIp))
                            {
                                if (t.httpBuf == null)
                                {
                                    t.httpBuf = new byte[tcpPacket.Bytes.Length - 20];
                                    Array.Copy(tcpPacket.Bytes, 20, t.httpBuf, 0, tcpPacket.Bytes.Length - 20);
                                }
                                else
                                {
                                    byte[] oldBuf = t.httpBuf;
                                    t.httpBuf = new byte[oldBuf.Length + tcpPacket.Bytes.Length - 20];
                                    Array.Copy(oldBuf, 0, t.httpBuf, 0, oldBuf.Length);
                                    Array.Copy(tcpPacket.Bytes, 20, t.httpBuf, oldBuf.Length, tcpPacket.Bytes.Length - 20);
                                }

                                try
                                {
                                    if (t.imgBuf == null)
                                    {
                                        TryGetImage(t.httpBuf, t.account);
                                    }
                                }
                                catch
                                {

                                }
                            }
                        }
                    }
                }
            }
        }

        static void TryGetImage(byte[] mt, string account)
        {
            for (int ii = 0; ii < mt.Length; ++ii)
            {
                if (mt.Length - ii >= 4)
                {
                    if (mt[ii] == '\r'
                        && mt[ii + 1] == '\n'
                        && mt[ii + 2] == '\r'
                        && mt[ii + 3] == '\n')
                    {
                        byte[] tmp = new byte[ii + 3];
                        Array.Copy(mt, 0, tmp, 0, tmp.Length);
                        string s = System.Text.Encoding.Default.GetString(tmp);
                        string[] sp = s.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                        bool L = false;
                        bool R = false;
                        bool C = false;
                        bool image = false;
                        for (int x = 0; x < sp.Length; x++)
                        {
                            if (sp[x].IndexOf("image/Jpeg") >= 0)
                            {
                                image = true;
                            }
                            if (sp[x].IndexOf("L:") >= 0)
                            {
                                L = true;
                            }
                            if (sp[x].IndexOf("R:") >= 0)
                            {
                                R = true;
                            }
                            if (sp[x].IndexOf("C:") >= 0)
                            {
                                C = true;
                            }
                        }
                        if (image && L && R && C)
                        {
                            byte[] ck = new byte[mt.Length - ii - 4];
                            Array.Copy(mt, ii + 4, ck, 0, ck.Length);
                            byte[] img_data = TryGetChuckData(ck);
                            m_dPopKartCaptcha[account].imgBuf = img_data;

                            try
                            {
                                string cpu = "" + _Compputer.CpuID + "----" + _Compputer.MacAddress + "----" + _Compputer.DiskID;
                                string encode = MyDes.Encode(cpu, _rKey);

                                //string url = "http://localhost/popkart_captcha.php";
                                string url = "http://121.42.148.243/popkart_captcha.php";
                                HttpWebRequest request = System.Net.WebRequest.Create(url) as HttpWebRequest;
                                request.ServicePoint.Expect100Continue = false;
                                request.Timeout = 2000;
                                request.ContentType = "application/octet-stream ";
                                request.ContentLength = img_data.Length;
                                request.Method = "POST";
                                request.Headers["C"] = encode;
                                request.Headers["K"] = _rKey;
                                request.Headers["U"] = _Compputer.LoginUserName;
                                request.Headers["S"] = _Compputer.SystemType;
                                Stream stm = request.GetRequestStream();
                                stm.Write(img_data, 0, img_data.Length);
                                stm.Flush();
                                WebResponse respone = request.GetResponse();
                                StreamReader reader = new StreamReader(respone.GetResponseStream(), Encoding.GetEncoding("GB2312"));
                                string ss = reader.ReadToEnd();
                                string[] split = ss.Split(new string[] { "----" },StringSplitOptions.RemoveEmptyEntries);
                                if (split.Length == 3)
                                {
                                    logger.Debug(ss);
                                    File.WriteAllText("code.txt", split[2]);

                                    _mainWinow.AddNewCode(split[2], img_data);
                                }
                            }
                            catch(Exception ex)
                            {
                                logger.Debug(ex.ToString());
                            }
                        }
                        break;
                    }
                }
            }
        }

        private void AddNewCode(string r, byte[] img)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                PopKartRecord q = new PopKartRecord();
                q.Time = DateTime.Now.ToLocalTime().ToShortTimeString();
                q.Record = r;
                BitmapImage bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.StreamSource = new MemoryStream(img);
                bmp.EndInit();
                q.Img = bmp;
                if (_mainWinow.lvRecord.Items.Count > 4)
                {
                    _mainWinow.lvRecord.Items.Clear();
                }
                _mainWinow.lvRecord.Items.Add(q);
            }));
        }

        static public string cs_md5(string src)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] strbytes = Encoding.Default.GetBytes(src);
            byte[] md5_bytes = md5.ComputeHash(strbytes, 0, strbytes.Length);
            string md5_str = BitConverter.ToString(md5_bytes).Replace("-", "");
            return md5_str;
        }

        static public string cs_md5(byte[] src)
        {
             MD5 md5 = new MD5CryptoServiceProvider();
            byte[] strbytes = src;
            byte[] md5_bytes = md5.ComputeHash(strbytes, 0, strbytes.Length);
            string md5_str = BitConverter.ToString(md5_bytes).Replace("-", "");
            return md5_str;
        }


        static byte[] TryGetChuckData(byte[] chuck)
        {
            byte[] ret = new byte[0];
            int chuck_size = 0;
            for (int i = 0; i < chuck.Length; i++)
            {
                if (chuck.Length - i >= 2)
                {
                    if (chuck[i] == '\r'
                         && chuck[i + 1] == '\n')
                    {
                        byte[] tmp = new byte[i];
                        Array.Copy(chuck, 0, tmp, 0, tmp.Length);
                        chuck_size = Convert.ToInt32(System.Text.Encoding.Default.GetString(tmp), 16);

                        if (chuck_size > 0)
                        {
                            if (chuck_size <= chuck.Length - i - 2 - 2)
                            {
                                byte[] newchuck = new byte[chuck.Length - i - 2 - chuck_size - 2];
                                Array.Copy(chuck, i + 2 + chuck_size + 2, newchuck, 0, newchuck.Length);
                                byte[] r = TryGetChuckData(newchuck);
                                ret = new byte[r.Length + chuck_size];
                                Array.Copy(chuck, i + 2, ret, 0, chuck_size);
                                Array.Copy(r, 0, ret, chuck_size, r.Length);
                            }
                            else
                            {
                                throw new Exception("This is a segment!");
                            }
                            break;
                        }
                        else if (chuck_size == 0)
                        {
                            break;
                        }
                    }
                }
            }
            return ret;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            StopAllDevice();
            CLogger.StopAllLoggers();
        }
    }
}
