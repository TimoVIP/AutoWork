using BaseFun;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace scheduleExecuteSQL
{
    class Program
    {
        static Hashtable config;
        static void Main(string[] args)
        {
            config = appSittingSet.readConfig("appconfig");
            Console.Title = config["platname"].ToString();

            //执行
            TestAsyncJob();

            Console.ReadLine();
        }



        [DisallowConcurrentExecution]
        public class job_log : IJob
        {
            public Task Execute(IJobExecutionContext context)
            {

                //定时执行sql
                MySQLHelper.MySQLHelper.connectionString = config["connectString"].ToString();
                int ir = MySQLHelper.MySQLHelper.ExecuteSql(config["sql"].ToString());
                string msg = $"执行{(ir > 0).ToString()}";
                Console.WriteLine(msg);
                appSittingSet.Log(msg);
                return Task.CompletedTask;
            }
        }

        public class job_log_write : IJob
        {
            public Task Execute(IJobExecutionContext context)
            {
                Console.WriteLine($"正常运行{DateTime.Now.ToString()}");
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

            //每个月1号的00.00.00执行
            IJobDetail job_log = JobBuilder.Create<job_log>().WithIdentity("EricJob", "EricGroup").Build();
            ITrigger trigger = TriggerBuilder.Create().WithIdentity("EricTrigger", "EricGroup").WithCronSchedule("0 0 0 1 1-12 ? ").Build();
            await sched.ScheduleJob(job_log, trigger);


            //间隔10秒
            IJobDetail anotherjob = JobBuilder.Create<job_log_write>().WithIdentity("EricAnotherJob", "EricGroup").Build();
            ITrigger anothertrigger = TriggerBuilder.Create().WithIdentity("EricAnotherTrigger", "EricGroup").WithSimpleSchedule(x => x.WithIntervalInHours(1).RepeatForever()).Build();
            await sched.ScheduleJob(anotherjob, anothertrigger);
        }




    }
}
