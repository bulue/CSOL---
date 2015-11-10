using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace PwcTool
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public static string version =string.Format("{1}.{2}.{3}"
                , System.IO.File.GetLastWriteTime(System.Reflection.Assembly.GetExecutingAssembly().Location)
                , System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Major
                , System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Minor
                , System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Build);
        public static string subversion = "a";
    }
}
