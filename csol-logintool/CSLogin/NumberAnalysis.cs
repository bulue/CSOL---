using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System.IO;
using System.Windows.Forms;

namespace CSLogin
{
    class NumberAnalysis
    {
        public List<Bitmap> list_N = new List<Bitmap>();
        public List<Bitmap> list_Shuzi = new List<Bitmap>();

        struct NumXpoint
        {
            public int num;
            public int xpoint;
        };

        public NumberAnalysis()
        {
            if (Directory.Exists("数字集"))
            {
                list_Shuzi.Clear();
                for (int i = 0; i <= 9; i++)
                {
                    Bitmap bmp = (Bitmap)Image.FromFile("数字集/" + "数字" + i + ".bmp");
                    list_Shuzi.Add(bmp);
                }
            }

            if (Directory.Exists("数字集"))
            {
                list_N.Clear();
                for (int i = 0; i <= 9; i++)
                {
                    Bitmap bmp = (Bitmap)Image.FromFile("数字集/" + "N" + i + ".bmp");
                    list_N.Add(bmp);
                }
            }
        }

        public int Analysis(Bitmap raw)
        {
            List<Bitmap> bitmaps = list_N;

            raw = new Bitmap(raw);
            List<NumXpoint> numbers = new List<NumXpoint>();
            for (int i = 0; i <= 9; ++i)
            {
                try
                {
                    bool isFind;
                    do
                    {
                        isFind = false;
                        Image<Bgr, Byte> img;
                        Image<Bgr, Byte> templ;

                        img = new Image<Bgr, Byte>(raw);
                        templ = new Image<Bgr, Byte>(bitmaps[i]);
                        TemplateMatchingType tmType = TemplateMatchingType.CcorrNormed;
                        Image<Gray, float> imageResult = img.MatchTemplate(templ, tmType);

                        double[] minValues, maxValues;
                        Point[] minLocations, maxLocations;
                        imageResult.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);
                        //对于平方差匹配和归一化平方差匹配，最小值表示最好的匹配；其他情况下，最大值表示最好的匹配            
                        for (int idx = 0; idx < maxValues.Length; ++idx)
                        {
                            if (maxValues[idx] > 0.96)
                            {
                                NumXpoint number = new NumXpoint();
                                number.num = i;
                                number.xpoint = maxLocations[idx].X;
                                numbers.Add(number);

                                int beginX = maxLocations[idx].X;
                                int beginY = maxLocations[idx].Y;
                                int endX = templ.Size.Width + beginX;
                                int endY = templ.Size.Height + beginY;
                                for (int x = beginX; x < endX; ++x)
                                {
                                    for (int y = beginY; y < endY; ++y)
                                    {
                                        raw.SetPixel(x, y, Color.Black);
                                    }
                                }

                                isFind = true;
                            }
                        }

                        img.Dispose();
                        templ.Dispose();
                        imageResult.Dispose();
                    } while (isFind);

                }
                catch (System.Exception ex)
                {
                    
                }
            }

            numbers.Sort(delegate(NumXpoint a, NumXpoint b)
            {
                return a.xpoint - b.xpoint;
            });

            int retNumber = 0;
            if (numbers.Count > 0)
            {
                for (int i = 0; i < numbers.Count; ++i)
                {
                    retNumber *= 10;
                    retNumber += numbers[i].num;
                }
            }
            else
            {
                retNumber = -1;
            }

            return retNumber;
        }
    }
}
