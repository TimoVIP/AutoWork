using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using TimoControl;

namespace GAC_load
{
    class Program
    {
          static  string plat = "青苹果";
        static decimal max = 0;
        static void Main(string[] args)
        {
            max = decimal.Parse(appSittingSet.readAppsettings("MaxValue"));
            Console.Title = appSittingSet.readAppsettings("platname")+"请勿输入，点击鼠标到窗口后，ESC退出光标";
            Console.TreatControlCAsInput = true;
            //登陆
            bool b = platACT2.login();
            if (!b)
            {
                Console.WriteLine(plat+"登陆失败");
                return;
            }
                Console.WriteLine(plat+"登陆成功");
            b = platGPK.loginGPK();
            if (!b)
            {
                Console.WriteLine("GPK登陆失败");
                return;
            }
                Console.WriteLine("GPK登陆成功");

            TestAsyncJob();

            Console.ReadLine();

            /*

            List<betData> list =  platACT2.getActData();

            betData bb = list[0];
            bb.passed = true;

            betData bb2 = list[1];
            bb2.passed = false;
            bb2.msg = "账号有误";

            platACT2.confirmAct(bb);

    */

        }

        public class job_log : IJob
        {
            public Task Execute(IJobExecutionContext context)
            {
                bool b = platACT2.login();
                string msg = $"登陆{(b ? "成功" : "失败")} {DateTime.Now.ToString()}";
                Console.WriteLine(msg);
                appSittingSet.Log(msg);
                return Task.CompletedTask;
            }
        }


        [DisallowConcurrentExecution]
        public class EricAnotherSimpleJob : IJob
        {
            public Task Execute(IJobExecutionContext context)
            {
                //加钱
                List<betData> list = platACT2.getActData();
                if (list.Count==0)
                {
                    Console.WriteLine("没有数据" + DateTime.Now.ToString());
                    return Task.CompletedTask;
                }

                foreach (var item in list)
                {
                    //查询是否在数据库
                    bool e = appSittingSet.recorderDbCheck($"select * from record where order_no='{item.bbid}';");
                    if (e)
                        continue;


                    //查询用户是否存在
                    Gpk_UserDetail userinfo = platGPK.GetUserDetail(item.username);
                    if (userinfo == null)
                    {
                        //回填拒绝
                        item.passed = false;
                        item.msg = "账号有误";
                        platACT2.confirmAct(item);
                        continue;
                    }

                    //金额是否超出 
                    if (item.betMoney > max)
                    {
                        item.passed = false;
                        item.msg = "金额超出范围，请人工处理";
                        platACT2.confirmAct(item);
                        continue;
                    }

                    //充钱
                    item.passed = true;
                    item.Memo = plat + item.Memo;
                    item.msg = $"充值{item.betMoney}";
                    item.AuditType = "Deposit";
                    item.Audit = item.betMoney;
                    item.Type = 4;//人工存入
                    item.isReal = true;
                    //加钱 充值的部分 
                    e = platGPK.MemberDepositSubmit(item);
                    if (e)
                    {
                        //插入本地数据库
                        appSittingSet.execSql($"insert into record values({0},'{item.bbid}','{item.username}',datetime(CURRENT_TIMESTAMP,'localtime'),0,1);");
                    }
                    platACT2.confirmAct(item);


                }
                /*
                //线程循环
                Parallel.ForEach(list, (item) =>
                {
                    //查询是否在数据库
                    bool e = appSittingSet.recorderDbCheck($"select * from record where order_no='{item.bbid}';");
                    if (e)
                        return;
                    //查询用户是否存在
                    Gpk_UserDetail userinfo =  platGPK.GetUserDetail(item.username);
                    if (userinfo == null)
                    {
                        //回填拒绝
                        item.passed = false;
                        item.msg = "账号有误";
                    }
                    else
                    {
                        item.passed = true;
                        item.AuditType = "Deposit";
                        item.Audit = item.betMoney;
                        item.Type = 4;//人工存入
                        item.isReal = true;
                        //加钱 充值的部分 
                        platGPK.MemberDepositSubmit(item);
                    }
                    platACT2.confirmAct(item);
                });
                */
                return Task.CompletedTask;
            }

        }



        static async Task TestAsyncJob()
        {
            var props = new NameValueCollection
            {
                { "quartz.serializer.type", "binary" }
            };
            StdSchedulerFactory schedFact = new StdSchedulerFactory(props);

            IScheduler sched = await schedFact.GetScheduler();
            await sched.Start();

            //0,12 点登陆
            IJobDetail job_log = JobBuilder.Create<job_log>().WithIdentity("EricJob", "EricGroup").Build();
            ITrigger trigger = TriggerBuilder.Create().WithIdentity("EricTrigger", "EricGroup").WithCronSchedule("10 10 0,12, * * ?").Build();
            await sched.ScheduleJob(job_log, trigger);

            //间隔10秒
            IJobDetail anotherjob = JobBuilder.Create<EricAnotherSimpleJob>().WithIdentity("EricAnotherJob", "EricGroup").Build();
            ITrigger anothertrigger = TriggerBuilder.Create().WithIdentity("EricAnotherTrigger", "EricGroup").WithSimpleSchedule(x => x.WithIntervalInSeconds(10).RepeatForever()).Build();
            await sched.ScheduleJob(anotherjob, anothertrigger);
        }
    }
}
