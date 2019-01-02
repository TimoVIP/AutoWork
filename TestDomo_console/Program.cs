using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace TestDomo_console
{
    class Program
    {
        static void Main(string[] args)
        {
            FileInfo[] fs = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "log").GetFiles("*.txt", SearchOption.AllDirectories);
            string filepath= AppDomain.CurrentDomain.BaseDirectory + "log\\等级更新失败的记录.md";
            foreach (var file in fs)
            {
                File.AppendAllLines(filepath, from line in File.ReadAllLines(file.FullName, Encoding.Default) where (line.Contains("需要手动更新")) select line, Encoding.Default);
            }
            /*
            StringBuilder sb = new StringBuilder();
            foreach (var file in fs)
            {

                string[] lines = File.ReadAllLines(file.FullName, Encoding.Default);
                foreach (var line in lines)
                {
                    if (line.Contains("需要手动更新"))
                    {
                        sb.AppendLine(line);
                    }
                }
                Console.WriteLine(file.Name);
            }
            File.AppendAllText(filepath, sb.ToString(), Encoding.Default);
            */
            Console.WriteLine("处理完毕");
            Console.ReadLine();
        }
    }
}
