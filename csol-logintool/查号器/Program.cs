﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace 查号器
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                if (args.Length >= 1 && args[0] == "-autostart")
                {
                    System.Environment.CurrentDirectory = System.Windows.Forms.Application.StartupPath;
                    Application.Run(new csCheckTool(true));
                }
                else
                {
                    Application.Run(new csCheckTool(false));
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString(), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    public static class Global
    {
        public static CLogger logger;
    }
}
