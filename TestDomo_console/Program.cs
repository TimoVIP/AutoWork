using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using TimoControl;

namespace TestDomo_console
{
    class Program
    {
        static void Main(string[] args)
        {
            //FileInfo[] fs = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "log").GetFiles("*.txt", SearchOption.AllDirectories);
            //string filepath= AppDomain.CurrentDomain.BaseDirectory + "log\\等级更新失败的记录.md";
            //foreach (var file in fs)
            //{
            //    File.AppendAllLines(filepath, from line in File.ReadAllLines(file.FullName, Encoding.Default) where (line.Contains("需要手动更新")) select line, Encoding.Default);
            //}
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
            //Console.WriteLine("处理完毕");



            betData bb = new betData()
            {
                bbid = "234888",
                //wallet = "100",
                username = "tanxi",
                lastOprTime = DateTime.Now.AddDays(-DateTime.Now.Day + 1).ToString("yyyy/MM/dd") + " 12:00:00",
                betTime = DateTime.Now.AddHours(12).ToString("yyyy/MM/dd HH:mm:ss"),
                betMoney = 1,
                wallet = "js33882948401"
                //实际先换算成美东时间 再获取所在的月份第一天

            };
            bool b = platGPK.loginGPK();
            //bb = platGPK.checkInGPK_transaction(bb);
            //b = platGPK.submitToGPK(bb, "测试活动 消除");
            //bb = platGPK.checkInGPK(bb);
            //bool b = platBB.loginBB();
            bb = platGPK.GetDetailInfo(bb);
            Console.ReadLine();
        }
    }
}
