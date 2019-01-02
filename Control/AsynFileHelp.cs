using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimoControl
{
    /// <summary>
    /// 多线程写文本文件
    /// </summary>
    public static class AsynFileHelp
    {
        private static Dictionary<long, long> lockDic = new Dictionary<long, long>();
        private static string fileName;
        /// <summary>  
        /// 获取或设置文件名称  
        /// </summary>  
        public static string FileName
        {
            get { return fileName; }
            set { fileName = value; }
        }
        /// <summary>  
        /// 构造函数  
        /// </summary>  
        /// <param name="byteCount">每次开辟位数大小，这个直接影响到记录文件的效率</param>  
        /// <param name="fileName">文件全路径名</param>  
        //public static AsynFileHelp(string filename)
        //{
        //    fileName = filename;
        //}
        /// <summary>  
        /// 创建文件  
        /// </summary>  
        /// <param name="fileName"></param>  
        public static void Create(string fileName)
        {
            if (!File.Exists(fileName))
            {
                using (FileStream fs = File.Create(fileName))
                {
                    fs.Close();
                }
            }
        }
        /// <summary>  
        /// 写入文本  
        /// </summary>  
        /// <param name="content">文本内容</param>  
        private static void Write(string content, string newLine)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new Exception("文件名不能为空！");
            }
            using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite, 8, FileOptions.Asynchronous))
            {
                Byte[] dataArray = System.Text.Encoding.Default.GetBytes(content + newLine);
                bool flag = true;
                long slen = dataArray.Length;
                long len = 0;
                while (flag)
                {
                    try
                    {
                        if (len >= fs.Length)
                        {
                            fs.Lock(len, slen);
                            lockDic[len] = slen;
                            flag = false;
                        }
                        else
                        {
                            len = fs.Length;
                        }
                    }
                    catch (Exception ex)
                    {
                        while (!lockDic.ContainsKey(len))
                        {
                            len += lockDic[len];
                        }
                    }
                }
                fs.Seek(len, SeekOrigin.Begin);
                fs.Write(dataArray, 0, dataArray.Length);
                fs.Close();
            }
        }
        /// <summary>  
        /// 写入文件内容  
        /// </summary>  
        /// <param name="content"></param>  
        public static void WriteLine(string content)
        {
            Write(content, System.Environment.NewLine);
        }
        /// <summary>  
        /// 写入文件  
        /// </summary>  
        /// <param name="content"></param>  
        public static void Write(string content)
        {
            Write(content, "");
        }
    }
}
