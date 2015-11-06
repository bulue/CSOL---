using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CommonQ
{
    class RandomString
    {
        static Random Rgen = new Random(System.Environment.TickCount);
        static Dictionary<string, string> Built = new Dictionary<string, string>();

        static public string Next(int lenth, string charset)
        {
            if (charset == null)
            {
                charset = ".";
            }

            if (charset == "")
            {
                return "";
            }
            else
            {
                string Loopup = GetLookUp(charset);
                StringBuilder sb = new StringBuilder(lenth);
                for (int i = 0; i < lenth; ++i)
                {
                    sb.Insert(i, Loopup[Rgen.Next(0, Loopup.Length)]);
                }

                return sb.ToString();
            }
        }

        static private string GetLookUp(string charset)
        {
            try
            {
                if (!Built.ContainsKey(charset))
                {
                    StringBuilder sb = new StringBuilder(256);
                    for (int Loop = 1; Loop <= 255; ++Loop)
                    {
                        sb.Insert(Loop - 1, (char)Loop);
                    }

                    string s = Regex.Replace(sb.ToString(), "[^" + charset + "]", "");
                    Built.Add(charset, s);
                }
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
            }

            return Built[charset];
        }
    }
}
