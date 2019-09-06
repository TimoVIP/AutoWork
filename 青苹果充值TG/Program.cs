using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using TimoControl;
using BaseFun;
namespace GAC_load
{
    class Program
    {
          //static  string plat = "青苹果";
        static decimal max = 0;
        static void Main(string[] args)
        {
            max = decimal.Parse(appSittingSet.readAppsettings("MaxValue"));
            Console.Title = appSittingSet.readAppsettings("platname") ;
            Console.TreatControlCAsInput = true;


            //先登录一遍
            job_log myjob1 = new job_log();
            myjob1.Execute(null);

            //测试
            //bool b = platQPGV2.changeStatus(new betData() { bbid = "jgnszkhilz4", msg = "机器人测试", passed = false });


            //执行
            TestAsyncJob();

            Console.ReadLine();
        }

        public class job_log : IJob
        {
            public Task Execute(IJobExecutionContext context)
            {
                //青苹果
                bool b = platQPGV2.login();
                string msg = $"青苹果登陆{(b ? "成功" : "失败")} {DateTime.Now.ToString()}";
                Console.WriteLine(msg);
                appSittingSet.Log(msg);

                //TG平台
                b = platTGaming.login();
                msg = $"TG登陆{(b ? "成功" : "失败")} {DateTime.Now.ToString()}";
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
                    //操作成功 确认 测试
                    //item.passed = true;
                    //platQPGV2.confirmAct(item);
                    //continue;

                    //金额是否超出 
                    if (item.betMoney > max)
                    {
                        item.passed = false;
                        item.msg = "金额超出范围，请人工处理";
                        platQPGV2.confirmAct(item);
                        continue;
                    }

                    //充钱到TG
                    platTGaming.recharge(item);

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
            ITrigger anothertrigger = TriggerBuilder.Create().WithIdentity("EricAnotherTrigger", "EricGroup").WithSimpleSchedule(x => x.WithIntervalInSeconds(10).RepeatForever()).Build();
            await sched.ScheduleJob(anotherjob, anothertrigger);
        }
    }
}
