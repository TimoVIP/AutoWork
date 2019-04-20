using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
namespace AutoRegTest
{
    class UnCodebase
    {
        public Bitmap bmpobj;
        public UnCodebase(Bitmap pic)
        {
            bmpobj = new Bitmap(pic);  //转换为Format32bppRgb
        }
        /// <summary>
        /// 根据RGB，计算灰度值
        /// </summary>
        /// <param name="posClr">Color值</param>
        /// <returns>灰度值，整型</returns>
        private int GetGrayNumColor(System.Drawing.Color posClr)
        {
            return (posClr.R * 19595 + posClr.G * 38469 + posClr.B * 7472) >> 16;
        }
        /// <summary>
        /// 灰度转换,逐点方式
        /// </summary>
        public void GrayByPixels()
        {
            for (int i = 0; i < bmpobj.Height; i++)
            {
                for (int j = 0; j < bmpobj.Width; j++)
                {
                    int tmpValue = GetGrayNumColor(bmpobj.GetPixel(j, i));
                    bmpobj.SetPixel(j, i, Color.FromArgb(tmpValue, tmpValue, tmpValue));
                }
            }
        }
        /// <summary>
        /// 去图形边框
        /// </summary>
        /// <param name="borderWidth"></param>
        public void ClearPicBorder(int borderWidth)
        {
            for (int i = 0; i < bmpobj.Height; i++)
            {
                for (int j = 0; j < bmpobj.Width; j++)
                {
                    if (i < borderWidth || j < borderWidth || j > bmpobj.Width - 1 - borderWidth || i > bmpobj.Height - 1 - borderWidth)
                        bmpobj.SetPixel(j, i, Color.FromArgb(255, 255, 255));
                }
            }
        }
        /// <summary>
        /// 灰度转换,逐行方式
        /// </summary>
        public void GrayByLine()
        {
            Rectangle rec = new Rectangle(0, 0, bmpobj.Width, bmpobj.Height);
            BitmapData bmpData = bmpobj.LockBits(rec, ImageLockMode.ReadWrite, bmpobj.PixelFormat);// PixelFormat.Format32bppPArgb);
                                                                                                   //  bmpData.PixelFormat = PixelFormat.Format24bppRgb;
            IntPtr scan0 = bmpData.Scan0;
            int len = bmpobj.Width * bmpobj.Height;
            int[] pixels = new int[len];
            Marshal.Copy(scan0, pixels, 0, len);
            //对图片进行处理
            int GrayValue = 0;
            for (int i = 0; i < len; i++)
            {
                GrayValue = GetGrayNumColor(Color.FromArgb(pixels[i]));
                pixels[i] = (byte)(Color.FromArgb(GrayValue, GrayValue, GrayValue)).ToArgb();  //Color转byte
            }
            bmpobj.UnlockBits(bmpData);
            ////输出
            //GCHandle gch = GCHandle.Alloc(pixels, GCHandleType.Pinned);
            //bmpOutput = new Bitmap(bmpobj.Width, bmpobj.Height, bmpData.Stride, bmpData.PixelFormat, gch.AddrOfPinnedObject());
            //gch.Free();
        }
        /// <summary>
        /// 得到有效图形并调整为可平均分割的大小
        /// </summary>
        /// <param name="dgGrayValue">灰度背景分界值</param>
        /// <param name="CharsCount">有效字符数</param>
        /// <returns></returns>
        public void GetPicValidByValue(int dgGrayValue, int CharsCount)
        {
            int posx1 = bmpobj.Width; int posy1 = bmpobj.Height;
            int posx2 = 0; int posy2 = 0;
            for (int i = 0; i < bmpobj.Height; i++)  //找有效区
            {
                for (int j = 0; j < bmpobj.Width; j++)
                {
                    int pixelValue = bmpobj.GetPixel(j, i).R;
                    if (pixelValue < dgGrayValue)  //根据灰度值
                    {
                        if (posx1 > j) posx1 = j;
                        if (posy1 > i) posy1 = i;

                        if (posx2 < j) posx2 = j;
                        if (posy2 < i) posy2 = i;
                    };
                };
            };
            // 确保能整除
            int Span = CharsCount - (posx2 - posx1 + 1) % CharsCount; //可整除的差额数
            if (Span < CharsCount)
            {
                int leftSpan = Span / 2;  //分配到左边的空列 ，如span为单数,则右边比左边大1
                if (posx1 > leftSpan)
                    posx1 = posx1 - leftSpan;
                if (posx2 + Span - leftSpan < bmpobj.Width)
                    posx2 = posx2 + Span - leftSpan;
            }
            //复制新图
            Rectangle cloneRect = new Rectangle(posx1, posy1, posx2 - posx1 + 1, posy2 - posy1 + 1);
            bmpobj = bmpobj.Clone(cloneRect, bmpobj.PixelFormat);
        }
        /// <summary>
        /// 得到有效图形,图形为类变量
        /// </summary>
        /// <param name="dgGrayValue">灰度背景分界值</param>
        /// <param name="CharsCount">有效字符数</param>
        /// <returns></returns>
        public void GetPicValidByValue(int dgGrayValue)
        {
            int posx1 = bmpobj.Width; int posy1 = bmpobj.Height;
            int posx2 = 0; int posy2 = 0;
            for (int i = 0; i < bmpobj.Height; i++)  //找有效区
            {
                for (int j = 0; j < bmpobj.Width; j++)
                {
                    int pixelValue = bmpobj.GetPixel(j, i).R;
                    if (pixelValue < dgGrayValue)  //根据灰度值
                    {
                        if (posx1 > j) posx1 = j;
                        if (posy1 > i) posy1 = i;

                        if (posx2 < j) posx2 = j;
                        if (posy2 < i) posy2 = i;
                    };
                };
            };
            //复制新图
            Rectangle cloneRect = new Rectangle(posx1, posy1, posx2 - posx1 + 1, posy2 - posy1 + 1);
            bmpobj = bmpobj.Clone(cloneRect, bmpobj.PixelFormat);
        }
        /// <summary>
        /// 得到有效图形,图形由外面传入
        /// </summary>
        /// <param name="dgGrayValue">灰度背景分界值</param>
        /// <param name="CharsCount">有效字符数</param>
        /// <returns></returns>
        public Bitmap GetPicValidByValue(Bitmap singlepic, int dgGrayValue)
        {
            int posx1 = singlepic.Width; int posy1 = singlepic.Height;
            int posx2 = 0; int posy2 = 0;
            for (int i = 0; i < singlepic.Height; i++)  //找有效区
            {
                for (int j = 0; j < singlepic.Width; j++)
                {
                    int pixelValue = singlepic.GetPixel(j, i).R;
                    if (pixelValue < dgGrayValue)  //根据灰度值
                    {
                        if (posx1 > j) posx1 = j;
                        if (posy1 > i) posy1 = i;

                        if (posx2 < j) posx2 = j;
                        if (posy2 < i) posy2 = i;
                    };
                };
            };
            //复制新图
            Rectangle cloneRect = new Rectangle(posx1, posy1, posx2 - posx1 + 1, posy2 - posy1 + 1);
            return singlepic.Clone(cloneRect, singlepic.PixelFormat);
        }
        /// <summary>
        /// 平均分割图片
        /// </summary>
        /// <param name="RowNum">水平上分割数</param>
        /// <param name="ColNum">垂直上分割数</param>
        /// <returns>分割好的图片数组</returns>
        public Bitmap[] GetSplitPics(int RowNum, int ColNum)
        {
            if (RowNum == 0 || ColNum == 0)
                return null;
            int singW = bmpobj.Width / RowNum;
            int singH = bmpobj.Height / ColNum;
            Bitmap[] PicArray = new Bitmap[RowNum * ColNum];
            Rectangle cloneRect;
            for (int i = 0; i < ColNum; i++)  //找有效区
            {
                for (int j = 0; j < RowNum; j++)
                {
                    cloneRect = new Rectangle(j * singW, i * singH, singW, singH);
                    PicArray[i * RowNum + j] = bmpobj.Clone(cloneRect, bmpobj.PixelFormat);//复制小块图
                }
            }
            return PicArray;
        }
        /// <summary>
        /// 返回灰度图片的点阵描述字串，1表示灰点，0表示背景
        /// </summary>
        /// <param name="singlepic">灰度图</param>
        /// <param name="dgGrayValue">背前景灰色界限</param>
        /// <returns></returns>
        public string GetSingleBmpCode(Bitmap singlepic, int dgGrayValue)
        {
            Color piexl;
            string code = "";
            for (int posy = 0; posy < singlepic.Height; posy++)
                for (int posx = 0; posx < singlepic.Width; posx++)
                {
                    piexl = singlepic.GetPixel(posx, posy);
                    if (piexl.R < dgGrayValue)  // Color.Black )
                        code = code + "1";
                    else
                        code = code + "0";
                }
            return code;
        }
        /// <summary>
        /// 得到灰度图像前景背景的临界值 最大类间方差法
        /// </summary>
        /// <returns>前景背景的临界值</returns>
        public int GetDgGrayValue()
        {
            int[] pixelNum = new int[256];   //图象直方图，共256个点
            int n, n1, n2;
            int total;        //total为总和，累计值
            double m1, m2, sum, csum, fmax, sb;  //sb为类间方差，fmax存储最大方差值
            int k, t, q;
            int threshValue = 1;      // 阈值
                                      //生成直方图
            for (int i = 0; i < bmpobj.Width; i++)
            {
                for (int j = 0; j < bmpobj.Height; j++)
                {
                    //返回各个点的颜色，以RGB表示
                    pixelNum[bmpobj.GetPixel(i, j).R]++;    //相应的直方图加1
                }
            }
            //直方图平滑化
            for (k = 0; k <= 255; k++)
            {
                total = 0;
                for (t = -2; t <= 2; t++)    //与附近2个灰度做平滑化，t值应取较小的值
                {
                    q = k + t;
                    if (q < 0)      //越界处理
                        q = 0;
                    if (q > 255)
                        q = 255;
                    total = total + pixelNum[q];  //total为总和，累计值
                }
                pixelNum[k] = (int)((float)total / 5.0 + 0.5);  //平滑化，左边2个+中间1个+右边2个灰度，共5个，所以总和除以5，后面加0.5是用修正值
            }
            //求阈值
            sum = csum = 0.0;
            n = 0;
            //计算总的图象的点数和质量矩，为后面的计算做准备
            for (k = 0; k <= 255; k++)
            {
                sum += (double)k * (double)pixelNum[k];  //x*f(x)质量矩，也就是每个灰度的值乘以其点数（归一化后为概率），sum为其总和
                n += pixelNum[k];      //n为图象总的点数，归一化后就是累积概率
            }
            fmax = -1.0;       //类间方差sb不可能为负，所以fmax初始值为-1不影响计算的进行
            n1 = 0;
            for (k = 0; k < 256; k++)     //对每个灰度（从0到255）计算一次分割后的类间方差sb
            {
                n1 += pixelNum[k];     //n1为在当前阈值遍前景图象的点数
                if (n1 == 0) { continue; }    //没有分出前景后景
                n2 = n - n1;       //n2为背景图象的点数
                if (n2 == 0) { break; }    //n2为0表示全部都是后景图象，与n1=0情况类似，之后的遍历不可能使前景点数增加，所以此时可以退出循环
                csum += (double)k * pixelNum[k];  //前景的“灰度的值*其点数”的总和
                m1 = csum / n1;      //m1为前景的平均灰度
                m2 = (sum - csum) / n2;    //m2为背景的平均灰度
                sb = (double)n1 * (double)n2 * (m1 - m2) * (m1 - m2); //sb为类间方差
                if (sb > fmax)     //如果算出的类间方差大于前一次算出的类间方差
                {
                    fmax = sb;      //fmax始终为最大类间方差（otsu）
                    threshValue = k;    //取最大类间方差时对应的灰度的k就是最佳阈值
                }
            }
            return threshValue;
        }
        /// <summary>
        /// 去掉杂点（适合杂点/杂线粗为1）
        /// </summary>
        /// <param name="dgGrayValue">背前景灰色界限</param>
        /// <returns></returns>
        public void ClearNoise(int dgGrayValue, int MaxNearPoints)
        {
            Color piexl;
            int nearDots = 0;
            //逐点判断
            for (int i = 0; i < bmpobj.Width; i++)
                for (int j = 0; j < bmpobj.Height; j++)
                {
                    piexl = bmpobj.GetPixel(i, j);
                    if (piexl.R < dgGrayValue)
                    {
                        nearDots = 0;
                        //判断周围8个点是否全为空
                        if (i == 0 || i == bmpobj.Width - 1 || j == 0 || j == bmpobj.Height - 1) //边框全去掉
                        {
                            bmpobj.SetPixel(i, j, Color.FromArgb(255, 255, 255));
                        }
                        else
                        {
                            if (bmpobj.GetPixel(i - 1, j - 1).R < dgGrayValue) nearDots++;
                            if (bmpobj.GetPixel(i, j - 1).R < dgGrayValue) nearDots++;
                            if (bmpobj.GetPixel(i + 1, j - 1).R < dgGrayValue) nearDots++;
                            if (bmpobj.GetPixel(i - 1, j).R < dgGrayValue) nearDots++;
                            if (bmpobj.GetPixel(i + 1, j).R < dgGrayValue) nearDots++;
                            if (bmpobj.GetPixel(i - 1, j + 1).R < dgGrayValue) nearDots++;
                            if (bmpobj.GetPixel(i, j + 1).R < dgGrayValue) nearDots++;
                            if (bmpobj.GetPixel(i + 1, j + 1).R < dgGrayValue) nearDots++;
                        }
                        if (nearDots < MaxNearPoints)
                            bmpobj.SetPixel(i, j, Color.FromArgb(255, 255, 255)); //去掉单点 && 粗细小3邻边点
                    }
                    else //背景
                        bmpobj.SetPixel(i, j, Color.FromArgb(255, 255, 255));
                }
        }
        /// <summary>
        /// 3×3中值滤波除杂
        /// </summary>
        /// <param name="dgGrayValue"></param>
        public void ClearNoise(int dgGrayValue)
        {
            int x, y;
            byte[] p = new byte[9]; //最小处理窗口3*3
            byte s;
            //byte[] lpTemp=new BYTE[nByteWidth*nHeight];
            int i, j;
            //--!!!!!!!!!!!!!!下面开始窗口为3×3中值滤波!!!!!!!!!!!!!!!!
            for (y = 1; y < bmpobj.Height - 1; y++) //--第一行和最后一行无法取窗口
            {
                for (x = 1; x < bmpobj.Width - 1; x++)
                {
                    //取9个点的值
                    p[0] = bmpobj.GetPixel(x - 1, y - 1).R;
                    p[1] = bmpobj.GetPixel(x, y - 1).R;
                    p[2] = bmpobj.GetPixel(x + 1, y - 1).R;
                    p[3] = bmpobj.GetPixel(x - 1, y).R;
                    p[4] = bmpobj.GetPixel(x, y).R;
                    p[5] = bmpobj.GetPixel(x + 1, y).R;
                    p[6] = bmpobj.GetPixel(x - 1, y + 1).R;
                    p[7] = bmpobj.GetPixel(x, y + 1).R;
                    p[8] = bmpobj.GetPixel(x + 1, y + 1).R;
                    //计算中值
                    for (j = 0; j < 5; j++)
                    {
                        for (i = j + 1; i < 9; i++)
                        {
                            if (p[j] > p[i])
                            {
                                s = p[j];
                                p[j] = p[i];
                                p[i] = s;
                            }
                        }
                    }
                    //  if (bmpobj.GetPixel(x, y).R < dgGrayValue)
                    bmpobj.SetPixel(x, y, Color.FromArgb(p[4], p[4], p[4]));  //给有效值付中值
                }
            }
        }
        /// <summary>
        /// 该函数用于对图像进行腐蚀运算。结构元素为水平方向或垂直方向的三个点，
        /// 中间点位于原点；或者由用户自己定义3×3的结构元素。
        /// </summary>
        /// <param name="dgGrayValue">前后景临界值</param>
        /// <param name="nMode">腐蚀方式：0表示水平方向，1垂直方向，2自定义结构元素。</param>
        /// <param name="structure"> 自定义的3×3结构元素</param>
        public void ErosionPic(int dgGrayValue, int nMode, bool[,] structure)
        {
            int lWidth = bmpobj.Width;
            int lHeight = bmpobj.Height;
            Bitmap newBmp = new Bitmap(lWidth, lHeight);
            int i, j, n, m;    //循环变量
            if (nMode == 0)
            {
                //使用水平方向的结构元素进行腐蚀
                // 由于使用1×3的结构元素，为防止越界，所以不处理最左边和最右边
                // 的两列像素
                for (j = 0; j < lHeight; j++)
                {
                    for (i = 1; i < lWidth - 1; i++)
                    {
                        //目标图像中的当前点先赋成黑色
                        newBmp.SetPixel(i, j, Color.Black);

                        //如果源图像中当前点自身或者左右有一个点不是黑色，
                        //则将目标图像中的当前点赋成白色
                        if (bmpobj.GetPixel(i - 1, j).R > dgGrayValue ||
                         bmpobj.GetPixel(i, j).R > dgGrayValue ||
                         bmpobj.GetPixel(i + 1, j).R > dgGrayValue)
                            newBmp.SetPixel(i, j, Color.White);
                    }
                }
            }
            else if (nMode == 1)
            {
                //使用垂真方向的结构元素进行腐蚀
                // 由于使用3×1的结构元素，为防止越界，所以不处理最上边和最下边
                // 的两行像素
                for (j = 1; j < lHeight - 1; j++)
                {
                    for (i = 0; i < lWidth; i++)
                    {
                        //目标图像中的当前点先赋成黑色
                        newBmp.SetPixel(i, j, Color.Black);
                        //如果源图像中当前点自身或者左右有一个点不是黑色，
                        //则将目标图像中的当前点赋成白色
                        if (bmpobj.GetPixel(i, j - 1).R > dgGrayValue ||
                         bmpobj.GetPixel(i, j).R > dgGrayValue ||
                         bmpobj.GetPixel(i, j + 1).R > dgGrayValue)
                            newBmp.SetPixel(i, j, Color.White);
                    }
                }
            }
            else
            {
                if (structure.Length != 9) //检查自定义结构
                    return;
                //使用自定义的结构元素进行腐蚀
                // 由于使用3×3的结构元素，为防止越界，所以不处理最左边和最右边
                // 的两列像素和最上边和最下边的两列像素
                for (j = 1; j < lHeight - 1; j++)
                {
                    for (i = 1; i < lWidth - 1; i++)
                    {
                        //目标图像中的当前点先赋成黑色
                        newBmp.SetPixel(i, j, Color.Black);
                        //如果原图像中对应结构元素中为黑色的那些点中有一个不是黑色，
                        //则将目标图像中的当前点赋成白色
                        for (m = 0; m < 3; m++)
                        {
                            for (n = 0; n < 3; n++)
                            {
                                if (!structure[m, n])
                                    continue;
                                if (bmpobj.GetPixel(i + m - 1, j + n - 1).R > dgGrayValue)
                                {
                                    newBmp.SetPixel(i, j, Color.White);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            bmpobj = newBmp;
        }
        /// <summary>
        /// 该函数用于对图像进行细化运算。要求目标图像为灰度图像
        /// </summary>
        /// <param name="dgGrayValue"></param>
        public void ThiningPic(int dgGrayValue)
        {
            int lWidth = bmpobj.Width;
            int lHeight = bmpobj.Height;
            // Bitmap newBmp = new Bitmap(lWidth, lHeight);
            bool bModified;    //脏标记  
            int i, j, n, m;    //循环变量
                               //四个条件
            bool bCondition1;
            bool bCondition2;
            bool bCondition3;
            bool bCondition4;
            int nCount;  //计数器  
            int[,] neighbour = new int[5, 5];  //5×5相邻区域像素值
            bModified = true;
            while (bModified)
            {
                bModified = false;
                //由于使用5×5的结构元素，为防止越界，所以不处理外围的几行和几列像素
                for (j = 2; j < lHeight - 2; j++)
                {
                    for (i = 2; i < lWidth - 2; i++)
                    {
                        bCondition1 = false;
                        bCondition2 = false;
                        bCondition3 = false;
                        bCondition4 = false;
                        if (bmpobj.GetPixel(i, j).R > dgGrayValue)
                        {
                            if (bmpobj.GetPixel(i, j).R < 255)
                                bmpobj.SetPixel(i, j, Color.White);
                            continue;
                        }
                        //获得当前点相邻的5×5区域内像素值，白色用0代表，黑色用1代表
                        for (m = 0; m < 5; m++)
                        {
                            for (n = 0; n < 5; n++)
                            {
                                neighbour[m, n] = bmpobj.GetPixel(i + m - 2, j + n - 2).R < dgGrayValue ? 1 : 0;
                            }
                        }
                        //逐个判断条件。
                        //判断2<=NZ(P1)<=6
                        nCount = neighbour[1, 1] + neighbour[1, 2] + neighbour[1, 3]
                          + neighbour[2, 1] + neighbour[2, 3] +
                          +neighbour[3, 1] + neighbour[3, 2] + neighbour[3, 3];
                        if (nCount >= 2 && nCount <= 6)
                        {
                            bCondition1 = true;
                        }
                        //判断Z0(P1)=1
                        nCount = 0;
                        if (neighbour[1, 2] == 0 && neighbour[1, 1] == 1)
                            nCount++;
                        if (neighbour[1, 1] == 0 && neighbour[2, 1] == 1)
                            nCount++;
                        if (neighbour[2, 1] == 0 && neighbour[3, 1] == 1)
                            nCount++;
                        if (neighbour[3, 1] == 0 && neighbour[3, 2] == 1)
                            nCount++;
                        if (neighbour[3, 2] == 0 && neighbour[3, 3] == 1)
                            nCount++;
                        if (neighbour[3, 3] == 0 && neighbour[2, 3] == 1)
                            nCount++;
                        if (neighbour[2, 3] == 0 && neighbour[1, 3] == 1)
                            nCount++;
                        if (neighbour[1, 3] == 0 && neighbour[1, 2] == 1)
                            nCount++;
                        if (nCount == 1)
                            bCondition2 = true;
                        //判断P2*P4*P8=0 or Z0(p2)!=1
                        if (neighbour[1, 2] * neighbour[2, 1] * neighbour[2, 3] == 0)
                        {
                            bCondition3 = true;
                        }
                        else
                        {
                            nCount = 0;
                            if (neighbour[0, 2] == 0 && neighbour[0, 1] == 1)
                                nCount++;
                            if (neighbour[0, 1] == 0 && neighbour[1, 1] == 1)
                                nCount++;
                            if (neighbour[1, 1] == 0 && neighbour[2, 1] == 1)
                                nCount++;
                            if (neighbour[2, 1] == 0 && neighbour[2, 2] == 1)
                                nCount++;
                            if (neighbour[2, 2] == 0 && neighbour[2, 3] == 1)
                                nCount++;
                            if (neighbour[2, 3] == 0 && neighbour[1, 3] == 1)
                                nCount++;
                            if (neighbour[1, 3] == 0 && neighbour[0, 3] == 1)
                                nCount++;
                            if (neighbour[0, 3] == 0 && neighbour[0, 2] == 1)
                                nCount++;
                            if (nCount != 1)
                                bCondition3 = true;
                        }
                        //判断P2*P4*P6=0 or Z0(p4)!=1
                        if (neighbour[1, 2] * neighbour[2, 1] * neighbour[3, 2] == 0)
                        {
                            bCondition4 = true;
                        }
                        else
                        {
                            nCount = 0;
                            if (neighbour[1, 1] == 0 && neighbour[1, 0] == 1)
                                nCount++;
                            if (neighbour[1, 0] == 0 && neighbour[2, 0] == 1)
                                nCount++;
                            if (neighbour[2, 0] == 0 && neighbour[3, 0] == 1)
                                nCount++;
                            if (neighbour[3, 0] == 0 && neighbour[3, 1] == 1)
                                nCount++;
                            if (neighbour[3, 1] == 0 && neighbour[3, 2] == 1)
                                nCount++;
                            if (neighbour[3, 2] == 0 && neighbour[2, 2] == 1)
                                nCount++;
                            if (neighbour[2, 2] == 0 && neighbour[1, 2] == 1)
                                nCount++;
                            if (neighbour[1, 2] == 0 && neighbour[1, 1] == 1)
                                nCount++;
                            if (nCount != 1)
                                bCondition4 = true;
                        }
                        if (bCondition1 && bCondition2 && bCondition3 && bCondition4)
                        {
                            bmpobj.SetPixel(i, j, Color.White);
                            bModified = true;
                        }
                        else
                        {
                            bmpobj.SetPixel(i, j, Color.Black);
                        }
                    }
                }
            }
            // 复制细化后的图像
            //  bmpobj = newBmp;
        }
        /// <summary>
        /// 锐化要启用不安全代码编译
        /// </summary>
        /// <param name="val">锐化程度。取值[0,1]。值越大锐化程度越高</param>
        /// <returns>锐化后的图像</returns>
        public void Sharpen(float val)
        {
            int w = bmpobj.Width;
            int h = bmpobj.Height;
            Bitmap bmpRtn = new Bitmap(w, h, PixelFormat.Format24bppRgb);
            BitmapData srcData = bmpobj.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData dstData = bmpRtn.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            unsafe
            {
                byte* pIn = (byte*)srcData.Scan0.ToPointer();
                byte* pOut = (byte*)dstData.Scan0.ToPointer();
                int stride = srcData.Stride;
                byte* p;
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        //取周围9点的值。位于边缘上的点不做改变。
                        if (x == 0 || x == w - 1 || y == 0 || y == h - 1)
                        {
                            //不做
                            pOut[0] = pIn[0];
                            pOut[1] = pIn[1];
                            pOut[2] = pIn[2];
                        }
                        else
                        {
                            int r1, r2, r3, r4, r5, r6, r7, r8, r0;
                            int g1, g2, g3, g4, g5, g6, g7, g8, g0;
                            int b1, b2, b3, b4, b5, b6, b7, b8, b0;
                            float vR, vG, vB;
                            //左上
                            p = pIn - stride - 3;
                            r1 = p[2];
                            g1 = p[1];
                            b1 = p[0];
                            //正上
                            p = pIn - stride;
                            r2 = p[2];
                            g2 = p[1];
                            b2 = p[0];
                            //右上
                            p = pIn - stride + 3;
                            r3 = p[2];
                            g3 = p[1];
                            b3 = p[0];
                            //左侧
                            p = pIn - 3;
                            r4 = p[2];
                            g4 = p[1];
                            b4 = p[0];
                            //右侧
                            p = pIn + 3;
                            r5 = p[2];
                            g5 = p[1];
                            b5 = p[0];
                            //右下
                            p = pIn + stride - 3;
                            r6 = p[2];
                            g6 = p[1];
                            b6 = p[0];
                            //正下
                            p = pIn + stride;
                            r7 = p[2];
                            g7 = p[1];
                            b7 = p[0];
                            //右下
                            p = pIn + stride + 3;
                            r8 = p[2];
                            g8 = p[1];
                            b8 = p[0];
                            //自己
                            p = pIn;
                            r0 = p[2];
                            g0 = p[1];
                            b0 = p[0];
                            vR = (float)r0 - (float)(r1 + r2 + r3 + r4 + r5 + r6 + r7 + r8) / 8;
                            vG = (float)g0 - (float)(g1 + g2 + g3 + g4 + g5 + g6 + g7 + g8) / 8;
                            vB = (float)b0 - (float)(b1 + b2 + b3 + b4 + b5 + b6 + b7 + b8) / 8;
                            vR = r0 + vR * val;
                            vG = g0 + vG * val;
                            vB = b0 + vB * val;
                            if (vR > 0)
                            {
                                vR = Math.Min(255, vR);
                            }
                            else
                            {
                                vR = Math.Max(0, vR);
                            }
                            if (vG > 0)
                            {
                                vG = Math.Min(255, vG);
                            }
                            else
                            {
                                vG = Math.Max(0, vG);
                            }
                            if (vB > 0)
                            {
                                vB = Math.Min(255, vB);
                            }
                            else
                            {
                                vB = Math.Max(0, vB);
                            }
                            pOut[0] = (byte)vB;
                            pOut[1] = (byte)vG;
                            pOut[2] = (byte)vR;
                        }
                        pIn += 3;
                        pOut += 3;
                    }// end of x
                    pIn += srcData.Stride - w * 3;
                    pOut += srcData.Stride - w * 3;
                } // end of y
            }
            bmpobj.UnlockBits(srcData);
            bmpRtn.UnlockBits(dstData);
            bmpobj = bmpRtn;
        }
        /// <summary>
        /// 图片二值化
        /// </summary>
        /// <param name="hsb"></param>
        public void BitmapTo1Bpp(Double hsb)
        {
            int w = bmpobj.Width;
            int h = bmpobj.Height;
            Bitmap bmp = new Bitmap(w, h, PixelFormat.Format1bppIndexed);
            BitmapData data = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadWrite, PixelFormat.Format1bppIndexed);
            for (int y = 0; y < h; y++)
            {
                byte[] scan = new byte[(w + 7) / 8];
                for (int x = 0; x < w; x++)
                {
                    Color c = bmpobj.GetPixel(x, y);
                    if (c.GetBrightness() >= hsb) scan[x / 8] |= (byte)(0x80 >> (x % 8));
                }
                Marshal.Copy(scan, 0, (IntPtr)((int)data.Scan0 + data.Stride * y), scan.Length);
            }
            bmp.UnlockBits(data);
            bmpobj = bmp;
        }
    }
}