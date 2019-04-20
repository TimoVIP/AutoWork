using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRegTest
{
    class Cracker2
    {
        Bitmap img;
        Color __c;
        private int __blackNum;
        private int b;
        private bool isWhilteLine;
        private byte w;
        private int __count;
        private int t;
        private IList<int> XList;
        private IList<int> YList;
        /// <summary>
        /// 二值化图片
        /// 就是将图像上的像素点的灰度值设置为0或255
        /// </summary>
        /// <returns>处理后的验证码</returns>
        public Bitmap BinaryZaTion()
        {
            for (int x = 0; x < img.Width; x++)
            {
                for (int y = 0; y < img.Height; y++)
                {
                    __c = img.GetPixel(x, y);
                    //灰度值
                    int __tc = (__c.R + __c.G + __c.B) / 3;
                    //大于阙值 黑色
                    if (__tc > t)
                    {
                        img.SetPixel(x, y, Color.FromArgb(__c.A, b, b, b));
                        //黑色点个数自加
                        __blackNum++;
                    }
                    //大于阙值 白色
                    else
                    {
                        img.SetPixel(x, y, Color.FromArgb(__c.A, w, w, w));
                    }
                }
            }
            return img;
        }

        /// <summary>
        /// 是否需要反色
        /// </summary>
        /// <returns>是否需要反色</returns>
        public bool IsNeedInverseColor()
        {
            if ((__blackNum * 1.0 / (img.Width * img.Height)) > 0.5)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// 反色
        /// </summary>
        /// <returns>处理后的验证码</returns>
        public Bitmap InverseColor()
        {
            for (int x = 0; x < img.Width; x++)
            {
                for (int y = 0; y < img.Height; y++)
                {
                    __c = img.GetPixel(x, y);
                    img.SetPixel(x, y, Color.FromArgb(__c.A, w - __c.R, w - __c.G, w - __c.B));
                }
            }
            return img;
        }


        /// <summary>
        /// 分割图片
        /// </summary>
        /// <returns>处理后的验证码</returns>
        public Bitmap CutImg()
        {
            //Y轴分割
            CutY();
            //区域个数
            __count = 0;
            if (XList.Count > 1)
            {
                //x起始值
                int __start = XList[0];
                //x结束值
                int __end = XList[XList.Count - 1];
                //x索引
                int __idx = 0;
                while (__start != __end)
                {
                    //区域宽度
                    int __w = __start;
                    //区域个数自加
                    __count++;
                    while (XList.Contains(__w) && __idx < XList.Count)
                    {
                        //区域宽度自加
                        __w++;
                        //x索引自加
                        __idx++;
                    }
                    //区域X轴坐标
                    int x = __start;
                    //区域Y轴坐标
                    int y = 0;
                    //区域宽度
                    int width = __w - __start;
                    //区域高度
                    int height = img.Height;
                    /*
                     * X轴分割当前区域
                     */
                    CutX(img.Clone(new Rectangle(x, y, width, height), img.PixelFormat));
                    if (YList.Count > 1 && YList.Count != img.Height)
                    {
                        int y1 = YList[0];
                        int y2 = YList[YList.Count - 1];
                        if (y1 != 1)
                        {
                            y = y1 - 1;
                        }
                        height = y2 - y1 + 1;
                    }
                    //GDI+绘图对象
                    Graphics g = Graphics.FromImage(img);
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.CompositingMode = CompositingMode.SourceOver;
                    g.PixelOffsetMode = PixelOffsetMode.HighSpeed;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    //画出验证码区域
                    g.DrawRectangle(new Pen(Brushes.Green), new Rectangle(x, y, width, height));
                    g.Dispose();
                    //起始值指向下一组
                    if (__idx < XList.Count)
                    {
                        __start = XList[__idx];
                    }
                    else
                    {
                        __start = __end;
                    }

                }
            }
            return img;
        }

        /// <summary>
        /// 得到Y轴分割点
        /// 判断每一竖行是否有黑色
        /// 有则添加
        /// </summary>
        /// <param name="img">要验证的图片</param>
        private void CutY()
        {
            XList.Clear();
            for (int x = 0; x < img.Width; x++)
            {
                isWhilteLine = false;
                for (int y = 0; y < img.Height; y++)
                {
                    __c = img.GetPixel(x, y);
                    if (__c.R == w)
                    {
                        isWhilteLine = true;
                    }
                    else
                    {
                        isWhilteLine = false;
                        break;
                    }
                }
                if (!isWhilteLine)
                {
                    XList.Add(x);
                }
            }
        }

        /// <summary>
        /// 得到X轴分割点
        /// 判断每一横行是否有黑色
        /// 有则添加
        /// </summary>
        /// <param name="tempImg">临时区域</param>
        private void CutX(Bitmap tempImg)
        {
            YList.Clear();
            for (int x = 0; x < tempImg.Height; x++)
            {
                isWhilteLine = false;
                for (int y = 0; y < tempImg.Width; y++)
                {
                    __c = tempImg.GetPixel(y, x);
                    if (__c.R == w)
                    {
                        isWhilteLine = true;
                    }
                    else
                    {
                        isWhilteLine = false;
                        break;
                    }
                }
                if (!isWhilteLine)
                {
                    YList.Add(x);
                }
            }
            tempImg.Dispose();
        }

        //识别
        /// <summary>
        /// 计算黑色像素比列
        /// </summary>
        /// <param name="tempimg"></param>
        /// <returns></returns>
        private double PixlPercent(Bitmap tempimg)
        {
            int temp = 0;
            int w_h = tempimg.Width * tempimg.Height;
            for (int x = 0; x < tempimg.Width; x++)
            {
                for (int y = 0; y < tempimg.Height; y++)
                {
                    __c = tempimg.GetPixel(x, y);
                    if (__c.R == b)
                    {
                        temp++;
                    }
                }
            }
            tempimg.Dispose();
            double result = temp * 1.0 / w_h;
            result = result.ToString().Length > 3 ? Convert.ToDouble(result.ToString().Substring(0, 3)) : result;
            return result;
        }

    }
}
