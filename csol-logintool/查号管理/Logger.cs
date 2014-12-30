using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;

namespace 查号管理
{
    public class CLogger
    {
        string _logFileName;
        string _Folder;

        Thread _writeThread;
        ShowLog _showFunc;

        List<logContent> _lineList;
        StreamWriter _Writer;

        int _lineCountCheck;
        int _Stop;
        const int _maxLineCount = 50000;

        public struct logContent
        {
            public eLoggerLevel lvl;
            public string line;

            public logContent(eLoggerLevel lv, string li)
            {
                lvl = lv;
                line = li;
            }
        }

        public enum eLoggerLevel
        {
            DEBUG,
            INFO,
            WARN,
            ERROR,
            FATAL,
        };

        //文件夹名,日志文件名前缀,文件名后缀
        public CLogger(string Folder, ShowLog showFunc = null)
        {
            _Folder = @".\" + Folder + string.Format("{0:yyMMdd}", DateTime.Now);
            _logFileName = string.Format(@"{0:yyMMdd-HHmmss}.log", DateTime.Now);

            if (!Directory.Exists(_Folder))
            {
                Directory.CreateDirectory(_Folder);
            }

            _Writer = new StreamWriter(_Folder + @"\" + _logFileName, true);
            _lineList = new List<logContent>();

            if (showFunc == null)
            {
                _showFunc = new ShowLog(show_Func);
            }
            else
            {
                _showFunc = showFunc;
            }

            _writeThread = new Thread(new ThreadStart(this.Run));
            _writeThread.Start();
            _Stop = 0;
        }

        void Run()
        {
            do
            {
                lock (this)
                {
                    if (_Stop == 1)
                    {
                        _Writer.Flush();
                        _Writer.Dispose();
                        _Writer.Close();
                        break;
                    }
                    if (_lineList.Count > 0)
                    {
                        foreach (logContent c in _lineList)
                        {
                            _Writer.WriteLine(c.line);
                            _showFunc(c.lvl, c.line);
                        }
                        _lineCountCheck += _lineList.Count;
                    }

                    if (_lineCountCheck > _maxLineCount)
                    {
                        _Writer.Dispose();
                        _Writer.Close();

                        _logFileName = string.Format(@"{0:yyMMdd-HHmmss}.log", DateTime.Now);
                        _Writer = new StreamWriter(_Folder + @"\" + _logFileName, true);
                        _lineCountCheck = 0;
                    }
                    _lineList.Clear();
                }
                Thread.Sleep(90);
            } while (true);
        }

        public void Stop()
        {
            _Stop = 1;
        }

        public void Debug(string format, params object[] args)
        {
            log(eLoggerLevel.DEBUG, format, args);
        }

        public void Info(string format, params object[] args)
        {
            log(eLoggerLevel.INFO, format, args);
        }

        public void Warn(string format, params object[] args)
        {
            log(eLoggerLevel.INFO, format, args);
        }

        public void log(eLoggerLevel lvl, string fmt, object[] args)
        {
            loggg(lvl, string.Format(fmt, args));
        }

        public void log(eLoggerLevel lvl, string fmt, object arg0)
        {
            loggg(lvl, string.Format(fmt, arg0));
        }

        public void log(eLoggerLevel lvl, string fmt, object arg0, object arg1)
        {
            loggg(lvl, string.Format(fmt, arg0, arg1));
        }

        private void loggg(eLoggerLevel lvl, string msg)
        {
            lock (this)
            {
                string format_msg = string.Format("{0:yy-MM-dd HH:mm:ss} {1} {2}", DateTime.Now, lvl, msg);
                _lineList.Add(new logContent(lvl, format_msg));
            }
        }

        private void show_Func(eLoggerLevel lvl, string s)
        {

        }
    }

    public delegate void ShowLog(CLogger.eLoggerLevel c, string s);
}
