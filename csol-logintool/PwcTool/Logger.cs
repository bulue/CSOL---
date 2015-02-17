using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;

namespace CommonQ
{
    public enum eLoggerLevel
    {
        DEBUG,
        INFO,
        WARN,
        ERROR,
        FATAL,
    };

    public class CLogger
    {
        string _loggerName;
        string _logFileName;
        string _Folder;

        Thread _writeThread;
        Action<eLoggerLevel, string> _showFunc;

        List<logContent> _lineList;
        StreamWriter _Writer;

        int _ByteCount;         //文件最大字节
        int _Stop;
        int _LastFlushTime;
        bool _IsNeedFlush;

        const int _MaxByteCount = 500000;
        const int _MaxFlushInterval = 100000;

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

        static List<CLogger> _loggers = new List<CLogger>();

        static public CLogger FromFolder(string FolderName, Action<eLoggerLevel, string> showFunc = null)
        {
            lock (_loggers)
            {
                foreach (CLogger logger in _loggers)
                {
                    if (logger._loggerName == FolderName)
                    {
                        return logger;
                    }
                }

                CLogger newLogger = new CLogger(FolderName, showFunc);
                _loggers.Add(newLogger);
                return newLogger;
            }
        }

        public void SetShowLogFunction(Action<eLoggerLevel, string> showLogfuc)
        {
            _showFunc = showLogfuc;
        }

        //文件夹名,日志文件名前缀,文件名后缀
        private CLogger(string Folder, Action<eLoggerLevel, string> showFunc = null)
        {
            _loggerName = Folder;
            _Folder = @".\" + Folder + string.Format("{0:yyMMdd}", DateTime.Now);
            _logFileName = string.Format(@"{0:yyMMdd-HHmmss}.log", DateTime.Now);

            if (!Directory.Exists(_Folder))
            {
                Directory.CreateDirectory(_Folder);
            }

            _Writer = new StreamWriter(_Folder + @"\" + _logFileName, true, System.Text.Encoding.GetEncoding("GB2312"));
            _lineList = new List<logContent>();

            if (showFunc == null)
            {
                _showFunc = new Action<eLoggerLevel, string>(show_Func);
            }
            else
            {
                _showFunc = showFunc;
            }

            _writeThread = new Thread(new ThreadStart(this.Run));
            _writeThread.Start();
            _Stop = 0;
            _LastFlushTime = Environment.TickCount;
            _IsNeedFlush = false;
        }

        private void Run()
        {
            do
            {
                if (_Stop == 1)
                {
                    _Writer.Flush();
                    _Writer.Dispose();
                    _Writer.Close();
                    break;
                }

                lock (_lineList)
                {
                    if (_lineList.Count > 0)
                    {
                        foreach (logContent c in _lineList)
                        {
                            _Writer.WriteLine(c.line);
                            //_showFunc.Invoke(c.lvl, c.line);

                            if (c.lvl >= eLoggerLevel.ERROR)
                            {
                                _Writer.Flush();
                                _LastFlushTime = Environment.TickCount;
                                _IsNeedFlush = false;
                            }
                            else
                                _IsNeedFlush = true;

                            _ByteCount += c.line.Length;
                        }
                    }
                    _lineList.Clear();
                }

                if (_ByteCount > _MaxByteCount)
                {
                    _Writer.Flush();
                    _Writer.Dispose();
                    _Writer.Close();

                    _logFileName = string.Format(@"{0:yyMMdd-HHmmss}.log", DateTime.Now);
                    _Writer = new StreamWriter(_Folder + @"\" + _logFileName, true, System.Text.Encoding.GetEncoding("GB2312"));
                    _ByteCount = 0;
                }

                if (_IsNeedFlush && Environment.TickCount - _LastFlushTime > _MaxFlushInterval)
                {
                    _Writer.Flush();
                    _IsNeedFlush = false;
                    _LastFlushTime = Environment.TickCount;
                }

                Thread.Sleep(90);
            } while (true);
        }

        public void Stop()
        {
            _Stop = 1;
        }

        public void Debug(string s)
        {
            addlog(eLoggerLevel.DEBUG, s);
        }

        public void Debug(string format, params object[] args)
        {
            log(eLoggerLevel.DEBUG, format, args);
        }

        public void Info(string s)
        {
            addlog(eLoggerLevel.INFO, s);
        }

        public void Info(string format, params object[] args)
        {
            log(eLoggerLevel.INFO, format, args);
        }

        public void Warn(string s)
        {
            addlog(eLoggerLevel.WARN, s);
        }

        public void Warn(string format, params object[] args)
        {
            log(eLoggerLevel.WARN, format, args);
        }

        public void Error(string s)
        {
            addlog(eLoggerLevel.ERROR, s);
        }

        public void Error(string format, params object[] args)
        {
            log(eLoggerLevel.ERROR, format, args);
        }

        public void log(eLoggerLevel lvl, string fmt, object[] args)
        {
            addlog(lvl, string.Format(fmt, args));
        }

        public void log(eLoggerLevel lvl, string fmt, object arg0)
        {
            addlog(lvl, string.Format(fmt, arg0));
        }

        public void log(eLoggerLevel lvl, string fmt, object arg0, object arg1)
        {
            addlog(lvl, string.Format(fmt, arg0, arg1));
        }

        private void addlog(eLoggerLevel lvl, string msg)
        {
            lock (_lineList)
            {
                string format_msg = string.Format("{0:yy-MM-dd HH:mm:ss} {1} {2}", DateTime.Now, lvl, msg);
                _showFunc.Invoke(lvl, format_msg);
                _lineList.Add(new logContent(lvl, format_msg));
            }
        }

        private void show_Func(eLoggerLevel lvl, string s)
        {
            Console.WriteLine(s);
        }
    }
}
