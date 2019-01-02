using TimoControl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutoUpdateGPKAcc
{
    class Program
    {

        static CookieContainer ct_gpk = new CookieContainer();
        static string url_gpk_base;

        static void Main(string[] args)
        {


            string s3 = appSittingSet.readAppsettings("GPK");
            string gpk_acc = s3.Split('|')[0];
            string gpk_pwd = s3.Split('|')[1];
            url_gpk_base = s3.Split('|')[2];


            bool f = platGPK.loginGPK();
            if (f)
            {
                Console.WriteLine("登录成功");

                //36500 条 
                for (int i = 1; i <= 100; i++)
                {

                    Thread th = new Thread(dowork);
                    th.IsBackground = true;
                    th.Name = "线程" + i;
                    th.Start(i);
                    //th.Join();

                    //new Thread(dowork).Start(i);
                }


                //Thread th = new Thread(dowork);
                //th.IsBackground = true;
                //th.Start();

                /*
                string msg = "";
                for (int i = 22431; i < 36464; i++)
                {
                    List<info> list = platGPK.getInfor(i);
                    if (list == null)
                    {
                        msg = string.Format(" 操作 页数 {0}   {1}", i, "失败");
                    }
                    else if (list.Count == 0)
                    {
                        msg = string.Format("操作 页数 {0}   {1}", i, "失败" + DateTime.Now.ToLongTimeString());
                    }
                    else
                    {
                        msg = string.Format("操作 页数 {0}   {1}", i, "成功" + DateTime.Now.ToLongTimeString());
                        //Console.WriteLine(msg);
                        //foreach (var item in list)
                        //{
                        //    //获取详细资料
                        //    bool b =  platGPK.getInfor_2(item.Account);
                        //    msg = string.Format("操作 账号 {0}   {1}",  i,item.Account, b? "完毕" : "失败");                        
                        //}
                    }

                    Console.WriteLine(msg);

                }

            */

                //查询账户信息

                //根据账户 查询详细信息


                Console.WriteLine("执行完毕");
                Console.Read();
            }
        }


        private static void dowork(object index)
        {
            string msg = "";

            /*
            for (int i = 22313; i < 36464; i++)
            {
                List<info> list = platGPK.getInfor(i);
                if (list == null)
                {
                    msg = string.Format(" 操作 页数 {0}   {1}",  i, "失败");
                }
                else if (list.Count == 0)
                {
                    msg = string.Format("操作 页数 {0}   {1}", i, "失败"+DateTime.Now.ToLongTimeString());
                }
                else
                {
                    msg = string.Format("操作 页数 {0}   {1}",  i, "成功"+DateTime.Now.ToLongTimeString());
                    //Console.WriteLine(msg);
                    //foreach (var item in list)
                    //{
                    //    //获取详细资料
                    //    bool b =  platGPK.getInfor_2(item.Account);
                    //    msg = string.Format("操作 账号 {0}   {1}",  i,item.Account, b? "完毕" : "失败");                        
                    //}
                }

                Console.WriteLine(msg);
                //Thread.Sleep(100);
            }

    */
            /*
                    for (int i = 150 * (index_ - 1) + 1; i < 150 * index_; i++)
                    {
                        //string  b=  platGPK.autoUpadateMemberAcc(i.ToString());
                        //string msg = string.Format("线程 {2} 操作 id {0} 更新 {1}", i, b == "true" ? "完毕" : b,index_);
                        bool bf = platGPK.autoUpadateMemberAcc(i.ToString());
                        string msg = string.Format("线程 {2} 操作 id {0} 更新 {1}", i, bf ? "完毕" : "失败", index_);
                        Console.WriteLine(msg);
                        appSittingSet.txtLog(msg);
                    }

        */

            /*
            object obj = new object();
            for (int i = 30786; i < 150000; i++)
            {
                lock (obj)
                {
                    if (i % (int)index == 0)
                    {
                        bool b = platGPK.autoUpadateMemberAcc(i.ToString());
                        string msg1 = string.Format(" 线程{2} 更新id {0}-{1}-{3}", i, b ? "完毕":"失败" ,index,DateTime.Now.ToString());
                        Console.WriteLine(msg1);
                        appSittingSet.txtLog(msg1);

                        Monitor.PulseAll(obj);
                    }
                    else
                    {
                        i--;
                        Monitor.Wait(obj);
                    }
                }
            }

            */
            for (int i = 149000; i < 150000; i++)
            {

                if (i % (int)index == 0)
                {
                    bool b = platGPK.autoUpadateMemberAcc(i.ToString());
                    string msg1 = string.Format(" 线程{2} 更新id {0}-{1}-{3}", i, b ? "完毕" : "失败", index, DateTime.Now.ToString());
                    Console.WriteLine(msg1);
                    appSittingSet.txtLog(msg1);
                }
            }
        }

    }
}
