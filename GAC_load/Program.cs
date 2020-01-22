using BaseFun;
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
        static decimal max = 0;
        static int Interval = 0;
        static void Main(string[] args)
        {
            max = decimal.Parse(appSittingSet.readAppsettings("MaxValue"));
            Interval= int.Parse(appSittingSet.readAppsettings("Interval"));
            Console.Title = appSittingSet.readAppsettings("platname") ;
            Console.TreatControlCAsInput = true;

            //先登录一遍
            job_log myjob1 = new job_log();
            myjob1.Execute(null);

            TestAsyncJob();

            Console.ReadLine();
        }

        public class job_log : IJob
        {
            public Task Execute(IJobExecutionContext context)
            {
                bool b = platQPGV2.login();
                string msg = $"青苹果登陆{(b ? "成功" : "失败")} {DateTime.Now.ToString()}";
                Console.WriteLine(msg);
                appSittingSet.Log(msg);

                b = platGPK.loginGPK();
                msg = $"GPK登陆{(b ? "成功" : "失败")} {DateTime.Now.ToString()}";
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
                List<betData> list = platQPGV2.getActData();
                if (list.Count==0)
                {
                    Console.WriteLine("没有数据" + DateTime.Now.ToString());
                    return Task.CompletedTask;
                }

                foreach (var item in list)
                {
                    //查询是否在数据库
                    bool e = SQLiteHelper.SQLiteHelper.recorderDbCheck($"select * from record where order_no='{item.bbid}';");
                    if (e)
                    {
                        item.msg = "已经处理";
                        item.passed = false;
                        platQPGV2.confirmAct(item);
                        //platQPGV2.changeStatus(item);
                        continue;
                    }



                    //查询用户是否存在
                    Gpk_UserDetail userinfo = platGPK.GetUserDetail(item.username);
                    if (userinfo == null)
                    {
                        //回填拒绝
                        item.passed = false;
                        item.msg = "账号有误";
                        platQPGV2.confirmAct(item);
                        continue;
                    }

                    //1号平台限制 层级
                    if (appSittingSet.readAppsettings("platno") == "1" && appSittingSet.readAppsettings("MemberLevelSettingId") == userinfo.MemberLevelSettingId)
                    {
                        item.Audit = item.betMoney * 10;
                    }
                    else
                    {
                        item.Audit = item.betMoney;
                    }

                    //金额是否超出 
                    if (item.betMoney > max)
                    {
                        item.passed = false;
                        item.msg = "金额超出范围，请人工处理";
                        platQPGV2.confirmAct(item);
                        continue;
                    }

                    //充钱
                    item.passed = true;
                    item.Memo =  item.Memo;
                    item.msg = $"充值{item.betMoney}";
                    item.AuditType = "Deposit";

                    item.Type = 4;//人工存入
                    item.isReal = true;
                    //加钱 充值的部分 
                    e = platGPK.MemberDepositSubmit(item);
                    if (e)
                    {
                        //插入本地数据库
                        SQLiteHelper.SQLiteHelper.execSql($"insert into record values({0},'{item.bbid}','{item.username}',datetime(CURRENT_TIMESTAMP,'localtime'),0,1);");
                    }
                    platQPGV2.confirmAct(item);


                }

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
            ITrigger anothertrigger = TriggerBuilder.Create().WithIdentity("EricAnotherTrigger", "EricGroup").WithSimpleSchedule(x => x.WithIntervalInSeconds(Interval).RepeatForever()).Build();
            await sched.ScheduleJob(anotherjob, anothertrigger);
        }
    }
}
