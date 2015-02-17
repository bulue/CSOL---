using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;

using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.IO;
using System.Threading;

namespace 查号器
{
    public struct RECT
    {
        public int x1;
        public int y1;
        public int x2;
        public int y2;

        public RECT(int _x1, int _y1, int _x2, int _y2)
        {
            x1 = _x1;
            y1 = _y1;
            x2 = _x2;
            y2 = _y2;
        }

    }

    class CommonApi
    {
        #region API

        const int MOUSEEVENTF_MOVE = 0x0001;     // 移动鼠标 
        const int MOUSEEVENTF_LEFTDOWN = 0x0002; //模拟鼠标左键按下 
        const int MOUSEEVENTF_LEFTUP = 0x0004; //模拟鼠标左键抬起 
        const int MOUSEEVENTF_RIGHTDOWN = 0x0008; //模拟鼠标右键按下 
        const int MOUSEEVENTF_RIGHTUP = 0x0010; //模拟鼠标右键抬起 
        const int MOUSEEVENTF_MIDDLEDOWN = 0x0020; //模拟鼠标中键按下 
        const int MOUSEEVENTF_MIDDLEUP = 0x0040; //模拟鼠标中键抬起 
        const int MOUSEEVENTF_ABSOLUTE = 0x8000; //标示是否采用绝对坐标 

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, ref RECT rect);
        [System.Runtime.InteropServices.DllImport("user32")]
        private static extern int mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("User32.dll", EntryPoint = "FindWindowEx")]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpClassName, string lpWindowName);

        const int SW_SHOWNORMAL = 1;
        const int SW_RESTORE = 9;
        const int SW_SHOWNOACTIVATE = 4;

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public const uint WM_SYSCOMMAND = 0x0112;
        public const int SC_CLOSE = 0xF060;
        public const int WM_COMMAND = 0x0111;

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
	    static extern bool EndDialog(IntPtr hDlg, out IntPtr nResult);

        #endregion


        static Dictionary<string, Image<Bgr, Byte> > m_sImage = new Dictionary<string, Image<Bgr, Byte> >();

        static public bool FindPic(int x, int y, int w, int h, string picFileName, double approximation,out int nX,out int nY)
        {
            nX = 0;
            nY = 0;

            if (x < 0 || y < 0 || w < 0 || h < 0)
            {
                return false;
            }
            try
            {
                //long ticks = System.Environment.TickCount;

                Bitmap screen = new Bitmap(w, h);

                Graphics g = Graphics.FromImage(screen);
                g.CopyFromScreen(x, y, 0, 0, screen.Size);

                Image<Bgr, Byte> img;
                Image<Bgr, Byte> templ;

                img = new Image<Bgr, Byte>(screen);
                try 
                {
                    templ = m_sImage[picFileName];
                }
                catch (System.Collections.Generic.KeyNotFoundException)
                {
                    using (Bitmap pic = (Bitmap)Bitmap.FromFile(picFileName))
                    {
                        templ = new Image<Bgr, Byte>(pic);
                        m_sImage.Add(picFileName, templ);
                    }
                }

                TM_TYPE tmType = TM_TYPE.CV_TM_CCORR_NORMED;
                Image<Gray, float> imageResult = img.MatchTemplate(templ, tmType);

                double bestValue;
                Point bestPoint;
                FindBestMatchPointAndValue(imageResult, tmType,out bestValue,out bestPoint);

                screen.Dispose();
                img.Dispose();
                //templ.Dispose();
                imageResult.Dispose();
                g.Dispose();

                //TraceInfo("图片 [" + picFileName + "] 耗时:" + (System.Environment.TickCount - ticks) + "ms.");
                if (bestValue > approximation)
                {
                    //TraceInfo("找到图片[" + picFileName + "]");
                    nX = bestPoint.X + x;
                    nY = bestPoint.Y + y;

                    //AddXY(picFileName, sMD5, nX, nY);
                    return true;
                }
                else
                {
                    //AddXY(picFileName, sMD5, -1, -1);
                    return false;
                }
            }
            catch (Emgu.CV.Util.CvException ex)
            {
            }

            return false;
        }

        static public bool FindPic(int x, int y, int w, int h, Bitmap pic, double approximation, out int nX, out int nY)
        {
            nX = 0;
            nY = 0;

            if (x < 0 || y < 0 || w < 0 || h < 0)
            {
                return false;
            }
            try
            {
                long ticks = System.Environment.TickCount;
                Bitmap screen = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format32bppRgb);


                Image<Bgr, Byte> img = new Image<Bgr, Byte>(screen);
                Image<Bgr, Byte> templ = new Image<Bgr, Byte>(pic);

                TM_TYPE tmType = TM_TYPE.CV_TM_CCORR_NORMED;
                Image<Gray, float> imageResult = img.MatchTemplate(templ, tmType);

                double bestValue;
                Point bestPoint;
                FindBestMatchPointAndValue(imageResult, tmType, out bestValue, out bestPoint);

                screen.Dispose();
                //pic.Dispose();
                img.Dispose();
                templ.Dispose();
                imageResult.Dispose();

                //TraceDebug("图片 [" + picFileName + "] 耗时:" + (System.Environment.TickCount - ticks) + "ms.");
                if (bestValue > approximation)
                {
                    nX = bestPoint.X + x;
                    nY = bestPoint.Y + y;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Emgu.CV.Util.CvException ex)
            {
                Global.logger.Debug(String.Format("FindPic({0},{1},{2},{3})  异常!!", x, y, w, h));
                Global.logger.Debug(ex.ToString());
            }

            return false;
        }

        static public Bitmap ScreenShot(int x,int y,int w,int h)
        {
            Bitmap screen = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            Graphics g = Graphics.FromImage(screen);
            g.CopyFromScreen(x, y, 0, 0, screen.Size);

            return screen;
        }

        static public void Left_Click(int nX, int nY)
        {
            int x = nX * 65536 / System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
            int y = nY * 65536 / System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
            mouse_event(MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE, x, y, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }

        static public void Double_Click(int nX, int nY)
        {
            int x = nX * 65536 / System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
            int y = nY * 65536 / System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
            mouse_event(MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE, x, y, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }

        static public void Mouse_Move(int nX, int nY)
        {
            int x = nX * 65536 / System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
            int y = nY * 65536 / System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
            mouse_event(MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE, x, y, 0, 0);
        }

        static public void Left_Click(int interval = 0)
        {
            if (interval == 0)
            {
                mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
            }
            else
            {
                mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                Thread.Sleep(interval);
                mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
            }
        }

        static public Process StartProcess(string path)
        {
            string dir = path.Substring(0, path.LastIndexOf("\\"));
            ProcessStartInfo startInfo = new ProcessStartInfo(path);

            startInfo.UseShellExecute = false;
            startInfo.WorkingDirectory = dir;
            return Process.Start(startInfo);
        }

        static public bool ShowWindow(IntPtr hWnd)
        {
            return ShowWindow(hWnd, SW_SHOWNORMAL);
        }

        static public void CloseWindow(IntPtr hWnd)
        {
            SendMessage(hWnd,WM_SYSCOMMAND,SC_CLOSE,0);
        }

        static public void CloseDialog(IntPtr hWnd)
        {
            IntPtr result;
            bool ret = EndDialog(hWnd,out result);
        }

        static public void GetWindowXYWH(IntPtr hWnd,out int x,out int y,out int w, out int h)
        {
            if (hWnd != IntPtr.Zero)
            {
                RECT rect = new RECT();
                if (GetWindowRect(hWnd,ref rect))
                {
                    x = rect.x1;
                    y = rect.y1;
                    w = rect.x2 - rect.x1;
                    h = rect.y2 - rect.y1;
                    return;
                }
            }

            x = 0;
            y = 0;
            w = 0;
            h = 0;
        }

        static private void FindBestMatchPointAndValue(Image<Gray, Single> image, TM_TYPE tmType, out double bestValue, out Point bestPoint)
        {
            bestValue = 0d;
            bestPoint = new Point(0, 0);
            double[] minValues, maxValues;
            Point[] minLocations, maxLocations;
            image.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);
            //对于平方差匹配和归一化平方差匹配，最小值表示最好的匹配；其他情况下，最大值表示最好的匹配            
            if (tmType == TM_TYPE.CV_TM_SQDIFF || tmType == TM_TYPE.CV_TM_SQDIFF_NORMED)
            {
                bestValue = minValues[0];
                bestPoint = minLocations[0];
            }
            else
            {
                bestValue = maxValues[0];
                bestPoint = maxLocations[0];
            }
        }


        public static string GetMacAddress()
        {
            try
            {
                NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface adapter in nics)
                {
                    if (!adapter.GetPhysicalAddress().ToString().Equals(""))
                    {
                        return adapter.GetPhysicalAddress().ToString();
                    }
                }

            }
            catch
            {
            }
            return "";
        }

        delegate void DelegateV<T>(T t);
        delegate void DelegateV2<T1,T2>(T1 t1,T2 t2);
        delegate R Delegate0<R>();
        delegate R Delegate1<R, T>(T t); 
    }

}
