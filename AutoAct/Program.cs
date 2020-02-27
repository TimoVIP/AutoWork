using BaseFun;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TimoControl;

namespace AutoAct
{
    class Program
    {
        //static decimal max = 0;
        //static int Interval = 0;
        static Hashtable myConfig = new Hashtable();
        static bool b = false;
        static string msg = "";
        private static string[] FiliterGroups;
        //private static string[] prefix;
        static void Main(string[] args)
        {
            myConfig = appSittingSet.readConfig();
            Console.Title = myConfig["platname"].ToString();
            FiliterGroups = myConfig["FiliterGroups"].ToString().Split('|');
            //prefix = myConfig["prefix"].ToString().Split('|');
            Console.TreatControlCAsInput = true;

            //先登录一遍
            按时登陆 myjob1 = new 按时登陆();
            myjob1.Execute(null);

            if (b)
                TestAsyncJob();


            Console.ReadLine();
        }
        private static void MyWrite(string v)
        {
            Console.WriteLine(v + " " + DateTime.Now.ToString());
        }

        static async Task TestAsyncJob()
        {

            StdSchedulerFactory schedFact = new StdSchedulerFactory();
            IScheduler sched = await schedFact.GetScheduler();
            await sched.Start();

            //清除一周前的数据、日志文件
            if (Convert.ToUInt16(myConfig["清除N天前日志"]) > 0)
            {
                await sched.ScheduleJob(JobBuilder.Create<清除日志>().Build(), TriggerBuilder.Create().WithCronSchedule("1 0 8 * * ? ").Build());
            }
            //登陆
            if (myConfig["定时登陆账号"].ToString() == "1")
            {
                await sched.ScheduleJob(JobBuilder.Create<按时登陆>().Build(), TriggerBuilder.Create().WithCronSchedule("10 1 0,6,12,18 * * ? ").Build());
            }

            ////周岁红包
            //if ( myConfig["zshb开关"].ToString() == "1")
            //{
            //    await sched.ScheduleJob(JobBuilder.Create<周岁红包>().Build(), TriggerBuilder.Create().WithSimpleSchedule(x => x.WithIntervalInSeconds(Convert.ToInt16(myConfig["zshb间隔秒"])).RepeatForever()).Build());
            //}

            ////满月彩金
            //if ( myConfig["mycj开关"].ToString() == "1")
            //{
            //    await sched.ScheduleJob(JobBuilder.Create<满月彩金>().Build(), TriggerBuilder.Create().WithSimpleSchedule(x => x.WithIntervalInSeconds(Convert.ToInt16(myConfig["mycj间隔秒"])).RepeatForever()).Build());
            //}

            ////棋牌闯关
            //if ( myConfig["qpcg开关"].ToString() == "1")
            //{
            //    await sched.ScheduleJob(JobBuilder.Create<棋牌闯关>().Build(), TriggerBuilder.Create().WithSimpleSchedule(x => x.WithIntervalInSeconds(Convert.ToInt16(myConfig["qpcg间隔秒"])).RepeatForever()).Build());
            //}

            ////捕鱼闯关
            //if ( myConfig["bycg开关"].ToString() == "1")
            //{
            //    await sched.ScheduleJob(JobBuilder.Create<捕鱼闯关>().Build(), TriggerBuilder.Create().WithDescription("bycg").WithSimpleSchedule(x => x.WithIntervalInSeconds(Convert.ToInt16(myConfig["bycg间隔秒"])).RepeatForever()).Build());
            //}

            //方法二
            foreach (DictionaryEntry item in myConfig)
            {
                if (item.Key.ToString().EndsWith("开关"))
                {
                    if (item.Value.ToString() == "1")
                    {
                        string pre = item.Key.ToString().Replace("开关", "");
                        Type t = Type.GetType("AutoAct.Program+" + pre);
                        await sched.ScheduleJob(JobBuilder.Create(t).Build(), TriggerBuilder.Create().WithDescription(pre).WithSimpleSchedule(x => x.WithIntervalInSeconds(Convert.ToInt16(myConfig[pre + "间隔秒"])).RepeatForever()).Build());
                    }
                }
            }

            //方法一
            //foreach (var item in prefix)
            //{
            //    if (myConfig[item+"开关"].ToString() == "1")
            //    {
            //        //Type t= assembly.GetType("AutoAct.Program+"+item);
            //        Type t = Type.GetType("AutoAct.Program+"+item);
            //        await sched.ScheduleJob(JobBuilder.Create(t).Build(), TriggerBuilder.Create().WithDescription(item).WithSimpleSchedule(x => x.WithIntervalInSeconds(Convert.ToInt16(myConfig[item+"间隔秒"])).RepeatForever()).Build());
            //    }
            //}

        }

        /// <summary>
        /// 0 6 12 18 小时1:10s 执行 登陆
        /// </summary>
        public class 按时登陆 : IJob
        {
            public Task Execute(IJobExecutionContext context)
            {
                //bool b = platQPGV2.login();
                //string msg = $"青苹果登陆{(b ? "成功" : "失败")} {DateTime.Now.ToString()}";
                //MyWrite(msg);
                //appSittingSet.Log(msg);

                b = ActFromDB.loginActivity();
                msg = $"活动站登录/连接{(b ? "成功" : "失败")} ";
                appSittingSet.Log(msg);
                MyWrite(msg);


                b = platGPK.loginGPK();
                msg = $"GPK登陆{(b ? "成功" : "失败")}";
                MyWrite(msg);
                appSittingSet.Log(msg);
                return Task.CompletedTask;
            }
        }

        /// <summary>
        /// 每天8:00:01 执行 删除一周前的日志 数据库一周前的数据
        /// </summary>
        public class 清除日志 : IJob
        {
            public Task Execute(IJobExecutionContext context)
            {
                int diff = Convert.ToInt16(myConfig["清除N天前日志"]);
                string sql = "delete from record where subtime < '" + DateTime.Now.AddDays(-diff).Date.ToString("yyyy-MM-dd") + "'";
                //appSittingSet.execSql(sql);
                SQLiteHelper.SQLiteHelper.execSql(sql);
                appSittingSet.Log("清除一周前的数据");
                appSittingSet.clsLogFiles(diff);
                appSittingSet.Log("清除一周前的日志");
                return Task.CompletedTask;
            }
        }

        /// <summary>
        /// 周岁红包
        /// </summary>
        [DisallowConcurrentExecution]
        public class zshb : IJob
        {
            public Task Execute(IJobExecutionContext context)
            {
                string prefix = context.Trigger.Description;

                List<betData> list = ActFromDB.getActData(myConfig[prefix + "活动编号"].ToString());
                if (list == null)
                {
                    MyWrite(myConfig[prefix + "活动名称"].ToString() + " 没有获取到记录，等待下次执行 ");
                    return Task.CompletedTask;
                }
                if (list.Count == 0)
                {
                    MyWrite(myConfig[prefix + "活动名称"].ToString() + "没有新的记录，等待下次执行 ");
                    return Task.CompletedTask;
                }
                foreach (var item in list)
                {
                    item.aname = myConfig[prefix + "活动名称"].ToString();
                    //判断是否提交过 同一用户  只能一次 网上查询 新平台 
                    string sql = string.Format(appSittingSet.readConfig()["sql_give_select"].ToString(), item.username, item.aid);
                    if (MySQLHelper.MySQLHelper.Exsist(sql))
                    {
                        item.passed = false;
                        item.msg = "您好，同一账号只能申请一次，申请不通过！R";
                        bool b = ActFromDB.confirmAct(item);
                        if (b)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        continue;
                    }

                    //判断是否提交过 同一用户  只能一次 网上查询 优惠大厅旧平台  活动编号不一样 ！！
                    //sql = string.Format(appSittingSet.readConfig()["sql_e_submissions_select"].ToString(), item.username, item.aid);
                    //MySQLHelper.MySQLHelper.connectionString = appSittingSet.readConfig()["MySqlConnect2"].ToString();
                    //if (MySQLHelper.MySQLHelper.Exsist(sql))
                    //{
                    //    item.passed = false;
                    //    item.msg = "您好，同一账号只能申请一次，申请不通过！R";
                    //    bool b = ActFromDB.confirmAct(item);
                    //    if (b)
                    //    {
                    //        string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                    //        MyWrite(msg);
                    //        appSittingSet.Log(msg);
                    //    }
                    //    continue;
                    //}

                    //获取详细信息
                    Gpk_UserDetail userinfo = platGPK.GetUserDetail(item.username);
                    if (userinfo == null)
                    {
                        //账号不存在？
                        item.passed = false;
                        item.msg = "经查询，您的账号有误！ R";
                        ActFromDB.confirmAct(item);
                        string msg = $"活动{item.aname}用户{item.username}处理完毕，处理为 {(item.passed ? "通过" : "不通过")}，回复消息 {item.msg}";
                        MyWrite(msg);
                        continue;
                    }

                    //判断 层级是否在 列表之中
                    foreach (var s in FiliterGroups)
                    {
                        if (userinfo.MemberLevelSettingId == s)
                        {
                            item.passed = false;
                            item.msg = "经查询，您的账号目前不享有此优惠！ R";
                            break;
                        }
                    }
                    if (!item.passed)
                    {
                        //回填失败
                        ActFromDB.confirmAct(item);
                        MyWrite(item.msg);
                        continue;
                    }
                    //注册时间必须要一年以上 注意美东时间-12
                    if (userinfo.JoinTime < DateTime.Now.Date.AddYears(-1).AddHours(12))
                    {

                    }
                    else
                    {
                        item.passed = false;
                        item.msg = "经查询，您的账号注册不满一年！ R";
                        ActFromDB.confirmAct(item);
                        string msg = $"活动{item.aname}用户{item.username}处理完毕，处理为 {(item.passed ? "通过" : "不通过")}，回复消息 {item.msg}";
                        MyWrite(msg);
                        continue;
                    }

                    betData bb = item;
                    //总存款要5w +
                    bb = platGPK.MemberGetDepositWithdrawInfo(userinfo);
                    if (bb.DepositTotal < Convert.ToDecimal(myConfig["zshb总存款"]))
                    {
                        item.passed = false;
                        item.msg = "经查询，统计期间您并没有达到最低存款" + myConfig[prefix + "总存款"] + "元的最低标准！ R";
                        ActFromDB.confirmAct(item);
                        string msg = $"活动{item.aname}用户{item.username}处理完毕，处理为 {(item.passed ? "通过" : "不通过")}，回复消息 {item.msg}";
                        MyWrite(msg);
                        continue;
                    }

                    //platGPK.getde
                    //20w+通过申请  赠送金额588


                    item.gamename = null;
                    item.lastCashTime = DateTime.Now.AddMonths(-2).ToString("yyyy/MM/dd");
                    item.lastOprTime = DateTime.Now.Date.ToString("yyyy/MM/dd");

                    foreach (var s in appSittingSet.readAppsettings(prefix + "游戏类别").Split(','))
                    {
                        item.gamename += "&types=" + string.Join("&types=", platGPK.KindCategories[int.Parse(s)].Replace("\"", "").Split(',').ToArray());
                    }

                    bb = platGPK.GetDetailInfo(item);
                    if (bb.Commissionable < Convert.ToDecimal(myConfig[prefix + "有效投注"]))
                    {
                        item.passed = false;
                        item.msg = "经查询，统计期间您并没有达到近2个月有效投注" + myConfig[prefix + "有效投注"] + "的标准！ R";
                        ActFromDB.confirmAct(item);
                        string msg = $"活动{item.aname}用户{item.username}处理完毕，处理为 {(item.passed ? "通过" : "不通过")}，回复消息 {item.msg}";
                        MyWrite(msg);
                        continue;
                    }


                    //加钱  提交数据
                    item.betMoney = Convert.ToDecimal(myConfig[prefix + "赠送金额"]);
                    item.AuditType = "Discount";
                    item.Audit = item.betMoney;
                    item.Memo = item.aname;
                    item.Type = 5;

                    bool fr = platGPK.MemberDepositSubmit(item);
                    //回填
                    item.msg = "恭喜您，您申请的<" + item.aname + ">已通过活动专员的检验";
                    ActFromDB.confirmAct(item);
                    MyWrite(item.msg);
                    continue;

                }

                return Task.CompletedTask;

            }


        }

        /// <summary>
        /// 满月彩金
        /// </summary>
        [DisallowConcurrentExecution]
        public class mycj : IJob
        {
            public Task Execute(IJobExecutionContext context)
            {
                string prefix = context.Trigger.Description;

                List<betData> list = ActFromDB.getActData(myConfig[prefix + "活动编号"].ToString());
                if (list == null)
                {
                    MyWrite(myConfig[prefix + "活动名称"].ToString() + " 没有获取到记录，等待下次执行 ");
                    return Task.CompletedTask;
                }
                if (list.Count == 0)
                {
                    MyWrite(myConfig[prefix + "活动名称"].ToString() + "没有新的记录，等待下次执行 ");
                    return Task.CompletedTask;
                }
                foreach (var item in list)
                {
                    item.aname = myConfig[prefix + "活动名称"].ToString();

                    //判断是否提交过 同一用户  只能一次 网上查询 新平台 
                    string sql = string.Format(appSittingSet.readConfig()["sql_give_select"].ToString(), item.username, item.aid);

                    if (MySQLHelper.MySQLHelper.Exsist(sql))
                    {
                        item.passed = false;
                        item.msg = "您好，同一账号只能申请一次，申请不通过！R";
                        bool b = ActFromDB.confirmAct(item);
                        if (b)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        continue;
                    }

                    //判断是否提交过 同一用户  只能一次 网上查询 优惠大厅旧平台 活动编号不一样 ！！
                    //sql = string.Format(appSittingSet.readConfig()["sql_e_submissions_select"].ToString(), item.username, item.aid);
                    //MySQLHelper.MySQLHelper.connectionString = appSittingSet.readConfig()["MySqlConnect2"].ToString();
                    //if (MySQLHelper.MySQLHelper.Exsist(sql))
                    //{
                    //    //还原连接字符串!!!
                    //    MySQLHelper.MySQLHelper.connectionString = null;
                    //    item.passed = false;
                    //    item.msg = "您好，同一账号只能申请一次，申请不通过！R";
                    //    bool b = ActFromDB.confirmAct(item);
                    //    if (b)
                    //    {
                    //        string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                    //        MyWrite(msg);
                    //        appSittingSet.Log(msg);
                    //    }
                    //    continue;
                    //}

                    //获取详细信息
                    Gpk_UserDetail userinfo = platGPK.GetUserDetail(item.username);
                    if (userinfo == null)
                    {
                        //账号不存在？
                        item.passed = false;
                        item.msg = "经查询，您的账号有误！ R";
                        ActFromDB.confirmAct(item);
                        string msg = $"活动{item.aname}用户{item.username}处理完毕，处理为 {(item.passed ? "通过" : "不通过")}，回复消息 {item.msg}";
                        MyWrite(msg);
                        continue;
                    }

                    //判断 层级是否在 列表之中
                    foreach (var s in FiliterGroups)
                    {
                        if (userinfo.MemberLevelSettingId == s)
                        {
                            item.passed = false;
                            item.msg = "经查询，您的账号目前不享有此优惠！ R";
                            break;
                        }
                    }
                    if (!item.passed)
                    {
                        //回填失败
                        ActFromDB.confirmAct(item);
                        MyWrite(item.msg);
                        continue;
                    }
                    //注册时间必须要90天以上
                    if ( userinfo.JoinTime<DateTime.Now.Date.AddDays(-90))
                    {

                    }
                    else
                    {
                        item.passed = false;
                        item.msg = "经查询，您的账号注册不满3个月！ R";
                        ActFromDB.confirmAct(item);
                        string msg = $"活动{item.aname}用户{item.username}处理完毕，处理为 {(item.passed ? "通过" : "不通过")}，回复消息 {item.msg}";
                        MyWrite(msg);
                        continue;
                    }
                    betData bb = item;
                    // 最近一个月 总存款要1000 +
                    item.lastOprTime = DateTime.Now.AddMonths(-1).ToString("yyyy/MM/dd 00:00:00");
                    item.betTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                    bb = platGPK.MemberTransactionSearch(item,false);
                    //bb = platGPK.MemberGetDepositWithdrawInfo(userinfo);
                    if (bb.total_money < Convert.ToDecimal(myConfig[prefix + "总存款"]))
                    {
                        item.passed = false;
                        item.msg = "经查询，统计期间您并没有达到最低存款" + myConfig[prefix + "总存款"] + "元的最低标准！ R";
                        ActFromDB.confirmAct(item);
                        string msg = $"活动{item.aname}用户{item.username}处理完毕，处理为 {(item.passed ? "通过" : "不通过")}，回复消息 {item.msg}";
                        MyWrite(msg);
                        continue;
                    }
                    //最近一个月
                    item.gamename = null;
                    item.lastCashTime = DateTime.Now.AddMonths(-1).ToString("yyyy/MM/dd");
                    item.lastOprTime = DateTime.Now.Date.ToString("yyyy/MM/dd");

                    foreach (var s in appSittingSet.readAppsettings(prefix + "游戏类别").Split(','))
                    {
                        item.gamename += "&types=" + string.Join("&types=", platGPK.KindCategories[int.Parse(s)].Replace("\"", "").Split(',').ToArray());
                    }
                    bb = platGPK.GetDetailInfo(item);
                    if (bb.Commissionable < Convert.ToDecimal(myConfig[prefix + "有效投注"]))
                    {
                        item.passed = false;
                        item.msg = "经查询，统计期间您并没有达到有效投注" + myConfig[prefix + "有效投注"] + "的标准！ R";
                        ActFromDB.confirmAct(item);
                        string msg = $"活动{item.aname}用户{item.username}处理完毕，处理为 {(item.passed ? "通过" : "不通过")}，回复消息 {item.msg}";
                        MyWrite(msg);
                        continue;
                    }


                    //加钱  提交数据
                    item.betMoney = Convert.ToDecimal(myConfig[prefix + "赠送金额"]);
                    //item.AuditType = "Discount";
                    item.AuditType = "None";
                    item.Audit = item.betMoney;
                    item.Memo = item.aname;
                    item.Type = 5;

                    bool fr = platGPK.MemberDepositSubmit(item);
                    //回填
                    item.gamename = null;
                    item.msg = "恭喜您，您申请的<" + item.aname + ">已通过活动专员的检验";
                    ActFromDB.confirmAct(item);
                    MyWrite(item.msg);
                    continue;

                }

                return Task.CompletedTask;

            }


        }

        /// <summary>
        /// 棋牌闯关
        /// </summary>
        [DisallowConcurrentExecution]
        public class qpcg : IJob
        {
            public Task Execute(IJobExecutionContext context)
            {
                string prefix = context.Trigger.Description;

                List<betData> list = ActFromDB.getActData(myConfig[prefix + "活动编号"].ToString());
                if (list == null)
                {
                    MyWrite(myConfig[prefix + "活动名称"].ToString() + " 没有获取到记录，等待下次执行 ");
                    return Task.CompletedTask;
                }
                if (list.Count == 0)
                {
                    MyWrite(myConfig[prefix + "活动名称"].ToString() + "没有新的记录，等待下次执行 ");
                    return Task.CompletedTask;
                }
                foreach (var item in list)
                {
                    item.aname = myConfig[prefix + "活动名称"].ToString();



                    //判断是否提交过 同一用户  只能一次 网上查询 优惠大厅旧平台 活动编号不一样 ！！
                    //sql = string.Format(appSittingSet.readConfig()["sql_e_submissions_select"].ToString(), item.username, item.aid);
                    //MySQLHelper.MySQLHelper.connectionString = appSittingSet.readConfig()["MySqlConnect2"].ToString();
                    //if (MySQLHelper.MySQLHelper.Exsist(sql))
                    //{
                    //    //还原连接字符串!!!
                    //    MySQLHelper.MySQLHelper.connectionString = null;
                    //    item.passed = false;
                    //    item.msg = "您好，同一账号只能申请一次，申请不通过！R";
                    //    bool b = ActFromDB.confirmAct(item);
                    //    if (b)
                    //    {
                    //        string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                    //        MyWrite(msg);
                    //        appSittingSet.Log(msg);
                    //    }
                    //    continue;
                    //}

                    //获取详细信息
                    Gpk_UserDetail userinfo = platGPK.GetUserDetail(item.username);
                    if (userinfo == null)
                    {
                        //账号不存在？
                        item.passed = false;
                        item.msg = "经查询，您的账号有误！ R";
                        ActFromDB.confirmAct(item);
                        string msg = $"活动{item.aname}用户{item.username}处理完毕，处理为 {(item.passed ? "通过" : "不通过")}，回复消息 {item.msg}";
                        MyWrite(msg);
                        continue;
                    }

                    //判断 层级是否在 列表之中
                    foreach (var s in FiliterGroups)
                    {
                        if (userinfo.MemberLevelSettingId == s)
                        {
                            item.passed = false;
                            item.msg = "经查询，您的账号目前不享有此优惠！ R";
                            break;
                        }
                    }
                    if (!item.passed)
                    {
                        //回填失败
                        ActFromDB.confirmAct(item);
                        MyWrite(item.msg);
                        continue;
                    }




                    //注册时间必须要一月以上
                    //if (userinfo.JoinTime > DateTime.Now.Date.AddMonths(-1))
                    //{
                    //    item.passed = false;
                    //    item.msg = "经查询，您的账号注册不满一个月！ R";
                    //    ActFromDB.confirmAct(item);
                    //    string msg = $"活动{item.aname}用户{item.username}处理完毕，处理为 {(item.passed ? "通过" : "不通过")}，回复消息 {item.msg}";
                    //    MyWrite(msg);
                    //    continue;
                    //}


                    ////总存款要1000 +
                    //item.lastOprTime = DateTime.Now.AddMonths(-1).ToString("yyyy/MM/dd 00:00:00");
                    //item.betTime =DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                    //bb = platGPK.MemberTransactionSearch(item);
                    ////bb = platGPK.MemberGetDepositWithdrawInfo(userinfo);
                    //if (bb.total_money < Convert.ToDecimal(myConfig["qpcg总存款"]))
                    //{
                    //    item.passed = false;
                    //    item.msg = "经查询，统计期间您并没有达到最低存款"+ myConfig["qpcg总存款"] + "元的最低标准！ R";
                    //    ActFromDB.confirmAct(item);
                    //    string msg = $"活动{item.aname}用户{item.username}处理完毕，处理为 {(item.passed ? "通过" : "不通过")}，回复消息 {item.msg}";
                    //    MyWrite(msg);
                    //    continue;
                    //}

                    //查询投注记录  
                    betData bb = item;
                    item.gamename = null; //
                    item.lastCashTime = DateTime.Now.AddHours(-12).Date.ToString("yyyy/MM/dd");
                    item.lastOprTime = DateTime.Now.AddHours(-12).Date.ToString("yyyy/MM/dd");
                    foreach (var s in appSittingSet.readAppsettings(prefix + "游戏类别").Split(','))
                    {
                        item.gamename += "&types=" + string.Join("&types=", platGPK.KindCategories[int.Parse(s)].Replace("\"", "").Split(',').ToArray());
                    }

                    decimal[] arr = Array.ConvertAll(myConfig[prefix + "有效投注"].ToString().Split('|'), decimal.Parse);
                    decimal[] arr2 = Array.ConvertAll(myConfig[prefix + "赠送金额"].ToString().Split('|'), decimal.Parse);
                    decimal[] arr3 = Array.ConvertAll(myConfig[prefix + "累计赠送金额"].ToString().Split('|'), decimal.Parse);
                    bb = platGPK.GetDetailInfo(item);
                    if (bb == null)
                    {
                        continue;
                    }
                    if (bb.Commissionable < arr[arr.Length - 1])
                    {
                        item.passed = false;
                        item.msg = "经查询，统计期间您并没有达到有效投注标准！ R";
                        ActFromDB.confirmAct(item);
                        string msg = $"活动{item.aname}用户{item.username}处理完毕，处理为 {(item.passed ? "通过" : "不通过")}，回复消息 {item.msg}";
                        MyWrite(msg);
                        continue;
                    }

                    //string sql = string.Format(appSittingSet.readConfig()["sql_give_select"].ToString(), item.username, item.aid);
                    //if (MySQLHelper.MySQLHelper.Exsist(sql))
                    //{
                    //    item.passed = false;
                    //    item.msg = "您好，同一账号只能申请一次，申请不通过！R";
                    //    bool b = ActFromDB.confirmAct(item);
                    //    if (b)
                    //    {
                    //        string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                    //        MyWrite(msg);
                    //        appSittingSet.Log(msg);
                    //    }
                    //    continue;
                    //}


                    //判断是否提交过 同一用户  上次送的 本地查询 

                    string dt_ = DateTime.Now.AddHours(-12).ToString("yyyy-MM-dd");
                    string sql = $"select  betno from record where pass=1 and aid ={ item.aid} and LOWER(username)='{ item.username.ToLower()}'   and subtime > '{dt_ } 00:00:01' and  subtime < '{ dt_ } 23:59:59' order by rowid desc limit 1; ";
                    //string lastLevel = SQLiteHelper.SQLiteHelper.execScalarSql(sql);
                    int lastLevel_ = SQLiteHelper.SQLiteHelper.execScalarSql<int>(sql);
                    for (int i = 0; i < arr.Length; i++)
                    {
                        if (bb.Commissionable >= arr[i])
                        {
                            item.betno = (arr.Length - i).ToString();
                            if (lastLevel_ == 0)
                            {
                                item.betMoney = arr3[i];
                            }
                            else
                            {
                                //item.betMoney = arr3[i] - arr2[arr.Length - lastLevel_];
                                //item.betMoney = arr3[i] - arr3[i + 1];
                                item.betMoney = arr2[i];
                            }
                            break;
                        }
                    }


                    //判断上次的关卡数
                    if (lastLevel_ > 0 && lastLevel_ >= int.Parse(item.betno))
                    {
                        //上次的关卡大于这次 不送
                        item.passed = false;
                        item.msg = "经查询，您已经申请过第" + lastLevel_ + "关的优惠，本次申请不通过！ R";
                        //回填失败
                        ActFromDB.confirmAct(item);
                        MyWrite(item.msg);
                        continue;
                    }

                    //加钱  提交数据
                    item.AuditType = "None";
                    //item.Audit = item.betMoney;
                    item.Memo = item.aname;
                    item.Type = 5;

                    bool fr = platGPK.MemberDepositSubmit(item);
                    //回填
                    item.msg = "恭喜您，您申请的<" + item.aname + ">已通过活动专员的检验";
                    item.gamename = "";
                    ActFromDB.confirmAct(item);
                    item.msg = $"活动{item.aname}用户{item.username}处理完毕，处理为 {(item.passed ? "通过" : "不通过")}，回复消息 {item.msg}";
                    MyWrite(item.msg);
                    continue;

                }

                return Task.CompletedTask;

            }


        }

        /// <summary>
        /// 捕鱼闯关
        /// </summary>
        [DisallowConcurrentExecution]
        public class bycg : IJob
        {
            public Task Execute(IJobExecutionContext context)
            {
                string prefix = context.Trigger.Description;

                List<betData> list = ActFromDB.getActData(myConfig[prefix + "活动编号"].ToString());
                if (list == null)
                {
                    MyWrite(myConfig[prefix + "活动名称"].ToString() + " 没有获取到记录，等待下次执行 ");
                    return Task.CompletedTask;
                }
                if (list.Count == 0)
                {
                    MyWrite(myConfig[prefix + "活动名称"].ToString() + "没有新的记录，等待下次执行 ");
                    return Task.CompletedTask;
                }
                foreach (var item in list)
                {
                    item.aname = myConfig[prefix + "活动名称"].ToString();

                    //获取详细信息
                    Gpk_UserDetail userinfo = platGPK.GetUserDetail(item.username);
                    if (userinfo == null)
                    {
                        //账号不存在？
                        item.passed = false;
                        item.msg = "经查询，您的账号有误！ R";
                        ActFromDB.confirmAct(item);
                        string msg = $"活动{item.aname}用户{item.username}处理完毕，处理为 {(item.passed ? "通过" : "不通过")}，回复消息 {item.msg}";
                        MyWrite(msg);
                        continue;
                    }

                    //判断 层级是否在 列表之中
                    foreach (var s in FiliterGroups)
                    {
                        if (userinfo.MemberLevelSettingId == s)
                        {
                            item.passed = false;
                            item.msg = "经查询，您的账号目前不享有此优惠！ R";
                            break;
                        }
                    }
                    if (!item.passed)
                    {
                        //回填失败
                        ActFromDB.confirmAct(item);
                        MyWrite(item.msg);
                        continue;
                    }


                    //查询投注记录  
                    betData bb = item;
                    item.gamename = null;
                    item.lastCashTime = DateTime.Now.AddHours(-12).Date.ToString("yyyy/MM/dd");
                    item.lastOprTime = DateTime.Now.AddHours(-12).Date.ToString("yyyy/MM/dd");
                    foreach (var s in appSittingSet.readAppsettings(prefix + "游戏类别").Split(','))
                    {
                        item.gamename += "&types=" + string.Join("&types=", platGPK.KindCategories[int.Parse(s)].Replace("\"", "").Split(',').ToArray());
                    }

                    decimal[] arr = Array.ConvertAll(myConfig[prefix + "有效投注"].ToString().Split('|'), decimal.Parse);
                    decimal[] arr2 = Array.ConvertAll(myConfig[prefix + "赠送金额"].ToString().Split('|'), decimal.Parse);
                    decimal[] arr3 = Array.ConvertAll(myConfig[prefix + "累计赠送金额"].ToString().Split('|'), decimal.Parse);
                    bb = platGPK.GetDetailInfo(item);
                    if (bb == null)
                    {
                        continue;
                    }
                    if (bb.Commissionable < arr[arr.Length - 1])
                    {
                        item.passed = false;
                        item.msg = "经查询，统计期间您并没有达到有效投注标准！ R";
                        ActFromDB.confirmAct(item);
                        string msg = $"活动{item.aname}用户{item.username}处理完毕，处理为 {(item.passed ? "通过" : "不通过")}，回复消息 {item.msg}";
                        MyWrite(msg);
                        continue;
                    }


                    //判断是否提交过 同一用户  上次送的 本地查询 

                    string dt_ = DateTime.Now.AddHours(-12).ToString("yyyy-MM-dd");
                    string sql = $"select  betno from record where pass=1 and aid ={ item.aid} and LOWER(username)='{ item.username.ToLower()}'   and subtime > '{dt_ } 00:00:01' and  subtime < '{ dt_ } 23:59:59' order by rowid desc limit 1; ";
                    int lastLevel_ = SQLiteHelper.SQLiteHelper.execScalarSql<int>(sql);//int null 0

                    for (int i = 0; i < arr.Length; i++)
                    {
                        if (bb.Commissionable >= arr[i])
                        {
                            item.betno = (arr.Length - i).ToString();
                            if (lastLevel_ == 0)
                            {
                                item.betMoney = arr3[i];
                            }
                            else
                            {
                                //item.betMoney = arr3[i] - arr2[arr.Length - lastLevel_];
                                //item.betMoney = arr3[i] - arr3[i+1];
                                item.betMoney = arr2[i];
                            }
                            break;
                        }
                    }


                    //判断上次的关卡数
                    if (lastLevel_ > 0 && lastLevel_ >= int.Parse(item.betno))
                    {
                        //上次的关卡大于这次 不送
                        item.passed = false;
                        item.msg = "经查询，您已经申请过第" + lastLevel_ + "关的优惠，本次申请不通过！ R";
                        //回填失败
                        ActFromDB.confirmAct(item);
                        MyWrite(item.msg);
                        continue;
                    }

                    //同一天内，已经送过的钱数、关卡数

                    //string dt_ = DateTime.Now.AddHours(-12).ToString("yyyy-MM-dd");
                     sql = $"select  sum(chargemoney) from record where pass=1 and aid ={ item.aid} and LOWER(username)='{ item.username.ToLower()}'   and subtime > '{dt_ } 00:00:01' and  subtime < '{ dt_ } 23:59:59' order by rowid desc limit 1; ";
                    decimal tm = SQLiteHelper.SQLiteHelper.execScalarSql<decimal>(sql);//int null 0

                    for (int i = 0; i < arr3.Length; i++)
                    {
                        if (tm>=arr3[i])
                        {
                            item.betno = (arr3.Length - i).ToString();
                            break;
                        }
                        else
                        {
                            //第一次
                            item.betno = "1";

                        }
                    }


                    //加钱  提交数据
                    //item.AuditType = "Discount";
                    item.AuditType = "None";
                    //item.Audit = item.betMoney;
                    item.Memo = item.aname;
                    item.Type = 5;

                    bool fr = platGPK.MemberDepositSubmit(item);
                    //回填
                    item.msg = "恭喜您，您申请的<" + item.aname + ">已通过活动专员的检验";
                    item.gamename = "";
                    ActFromDB.confirmAct(item);
                    item.msg = $"活动{item.aname}用户{item.username}处理完毕，处理为 {(item.passed ? "通过" : "不通过")}，回复消息 {item.msg}";
                    MyWrite(item.msg);
                    continue;

                }

                return Task.CompletedTask;

            }


        }

        /// <summary>
        /// 首次投注
        /// </summary>
        [DisallowConcurrentExecution]
        public class fb : IJob
        {
            public Task Execute(IJobExecutionContext context)
            {

                string prefix = context.Trigger.Description;

                List<betData> list = ActFromDB.getActData(myConfig[prefix + "活动编号"].ToString());
                if (list == null)
                {
                    MyWrite(myConfig[prefix + "活动名称"].ToString() + " 没有获取到记录，等待下次执行 ");
                    return Task.CompletedTask;
                }
                if (list.Count == 0)
                {
                    MyWrite(myConfig[prefix + "活动名称"].ToString() + "没有新的记录，等待下次执行 ");
                    return Task.CompletedTask;
                }
                foreach (var item in list)
                {
                    item.aname = myConfig[prefix + "活动名称"].ToString();

                    //判断是否提交过 同一用户  只能一次 网上查询 新平台 
                    string sql = string.Format(appSittingSet.readConfig()["sql_give_select"].ToString(), item.username, item.aid);

                    if (MySQLHelper.MySQLHelper.Exsist(sql))
                    {
                        item.passed = false;
                        item.msg = "您好，同一账号只能申请一次，申请不通过！R";
                        bool b = ActFromDB.confirmAct(item);
                        if (b)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        continue;
                    }

                    //获取详细信息
                    Gpk_UserDetail userinfo = platGPK.GetUserDetail(item.username);
                    if (userinfo == null)
                    {
                        //账号不存在？
                        item.passed = false;
                        item.msg = "经查询，您的账号有误！ R";
                        ActFromDB.confirmAct(item);
                        string msg = $"活动{item.aname}用户{item.username}处理完毕，处理为 {(item.passed ? "通过" : "不通过")}，回复消息 {item.msg}";
                        MyWrite(msg);
                        continue;
                    }

                    //判断 层级是否在 列表之中
                    foreach (var s in FiliterGroups)
                    {
                        if (userinfo.MemberLevelSettingId == s)
                        {
                            item.passed = false;
                            item.msg = "经查询，您的账号目前不享有此优惠！ R";
                            break;
                        }
                    }
                    if (!item.passed)
                    {
                        //回填失败
                        ActFromDB.confirmAct(item);
                        MyWrite(item.msg);
                        continue;
                    }
                    //注册时间 存款时间 要48小时内
                    if (DateTime.Parse(item.betTime) > userinfo.JoinTime.AddHours(48))
                    {
                        item.passed = false;
                        item.msg = "当天注册存款后48小时内为新会员！ R";
                        ActFromDB.confirmAct(item);
                        string msg = $"活动{item.aname}用户{item.username}处理完毕，处理为 {(item.passed ? "通过" : "不通过")}，回复消息 {item.msg}";
                        MyWrite(msg);
                        continue;
                    }

                    betData bb = item;
                    //总存款要1000 +
                    //item.lastOprTime = DateTime.Now.AddMonths(-1).ToString("yyyy/MM/dd 00:00:00");
                    //item.betTime =DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                    //bb = platGPK.MemberTransactionSearch(item);
                    ////bb = platGPK.MemberGetDepositWithdrawInfo(userinfo);
                    //if (bb.total_money < Convert.ToDecimal(myConfig["mycj总存款"]))
                    //{
                    //    item.passed = false;
                    //    item.msg = "经查询，统计期间您并没有达到最低存款"+ myConfig["mycj总存款"] + "元的最低标准！ R";
                    //    ActFromDB.confirmAct(item);
                    //    string msg = $"活动{item.aname}用户{item.username}处理完毕，处理为 {(item.passed ? "通过" : "不通过")}，回复消息 {item.msg}";
                    //    MyWrite(msg);
                    //    continue;
                    //}

                    //投注查询
                    item.gamename = null;
                    item.lastCashTime = userinfo.JoinTime.ToString("yyyy/MM/dd");//注册时间
                    item.lastOprTime = DateTime.Now.Date.ToString("yyyy/MM/dd");//现在

                    foreach (var s in appSittingSet.readAppsettings(prefix + "游戏类别").Split(','))
                    {
                        item.gamename += "&types=" + string.Join("&types=", platGPK.KindCategories[int.Parse(s)].Replace("\"", "").Split(',').ToArray());
                    }
                    bb = platGPK.GetDetailInfo(item);
                    if (bb == null)
                    {
                        continue;
                    }

                    decimal[] arr = Array.ConvertAll(myConfig[prefix + "有效投注"].ToString().Split('|'), decimal.Parse);
                    decimal[] arr2 = Array.ConvertAll(myConfig[prefix + "赠送金额"].ToString().Split('|'), decimal.Parse);
                    if (bb.Commissionable < arr[arr.Length - 1])
                    {
                        item.passed = false;
                        item.msg = "经查询，统计期间您并没有达到有效投注标准！ R";
                        ActFromDB.confirmAct(item);
                        string msg = $"活动{item.aname}用户{item.username}处理完毕，处理为 {(item.passed ? "通过" : "不通过")}，回复消息 {item.msg}";
                        MyWrite(msg);
                        continue;
                    }
                    //计算赠送金额
                    for (int i = 0; i < arr.Length; i++)
                    {
                        if (bb.Commissionable >= arr[i])
                        {
                            item.betno = (arr.Length - i).ToString();
                            item.betMoney = arr2[i];
                            break;
                        }
                    }

                    //加钱  提交数据
                    item.AuditType = "Discount";
                    item.Audit = item.betMoney;
                    item.Memo = item.aname;
                    item.Type = 5;

                    bool fr = platGPK.MemberDepositSubmit(item);
                    //回填
                    item.msg = "恭喜您，您申请的<" + item.aname + ">已通过活动专员的检验";
                    ActFromDB.confirmAct(item);
                    MyWrite(item.msg);
                    continue;

                }

                return Task.CompletedTask;

            }


        }

        /// <summary>
        /// 七日大闯关
        /// </summary>
        [DisallowConcurrentExecution]
        public class dcg : IJob
        {
            public Task Execute(IJobExecutionContext context)
            {
                string prefix = context.Trigger.Description;

                List<betData> list = ActFromDB.getActData(myConfig[prefix + "活动编号"].ToString());
                if (list == null)
                {
                    MyWrite(myConfig[prefix + "活动名称"].ToString() + " 没有获取到记录，等待下次执行 ");
                    return Task.CompletedTask;
                }
                if (list.Count == 0)
                {
                    MyWrite(myConfig[prefix + "活动名称"].ToString() + "没有新的记录，等待下次执行 ");
                    return Task.CompletedTask;
                }
                foreach (var item in list)
                {
                    item.aname = myConfig[prefix + "活动名称"].ToString();
                    //string old_addtime = item.betTime;
                    
                    int ts_now = int.Parse( item.PortalMemo);//天数 默认0
                    int fa_now =int.Parse( item.Memo);//方案 默认1
                    if (ts_now>7 )
                    {
                        item.passed = false;
                        item.msg = "您好，请正确填写天数 1-7 ，申请不通过！R";
                        bool b = ActFromDB.confirmAct(item);
                        if (b)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        continue;
                    }

                    if (fa_now !=1 && fa_now != 2 )
                    {
                        item.passed = false;
                        item.msg = "您好，请正确填写方案 1或者 2，申请不通过！R";
                        bool b = ActFromDB.confirmAct(item);
                        if (b)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        continue;
                    }

                    //一天只能一次 判断是否提交过 同一用户  只能一次 网上查询 新平台 
                    //string sql = string.Format(appSittingSet.readConfig()["sql_give_select_date"].ToString(), item.username, item.aid,DateTime.Now.AddHours(12).Date.ToString("yyyy-MM-dd 00:00:00"),DateTime.Now.AddHours(-12).Date.ToString("yyyy-MM-dd 23:59:59"));

                    //if (MySQLHelper.MySQLHelper.Exsist(sql))
                    //{
                    //    item.passed = false;
                    //    item.msg = "您好，同一账号一天只能申请一次，申请不通过！R";
                    //    bool b = ActFromDB.confirmAct(item);
                    //    if (b)
                    //    {
                    //        string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                    //        MyWrite(msg);
                    //        appSittingSet.Log(msg);
                    //    }
                    //    continue;
                    //}

                    string sql = string.Format(appSittingSet.readConfig()["sql_give_select_time"].ToString(), item.username, item.aid);
                    object o = MySQLHelper.MySQLHelper.GetScalar(sql);
                    if (o != null)
                    {
                        DateTime dt;
                        DateTime.TryParse(o.ToString(), out dt);
                        if (dt.AddHours(-12).Date==DateTime.Now.Date)
                        {
                            item.passed = false;
                            item.msg = "您好，同一账号一天只能申请一次，申请不通过！R";
                            bool b = ActFromDB.confirmAct(item);
                            if (b)
                            {
                                string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                                MyWrite(msg);
                                appSittingSet.Log(msg);
                            }
                            continue;
                        }
                    }

                    //获取详细信息
                    Gpk_UserDetail userinfo = platGPK.GetUserDetail(item.username);
                    if (userinfo == null)
                    {
                        //账号不存在？
                        item.passed = false;
                        item.msg = "经查询，您的账号有误！ R";
                        ActFromDB.confirmAct(item);
                        string msg = $"活动{item.aname}用户{item.username}处理完毕，处理为 {(item.passed ? "通过" : "不通过")}，回复消息 {item.msg}";
                        MyWrite(msg);
                        continue;
                    }

                    //判断 层级是否在 列表之中
                    foreach (var s in FiliterGroups)
                    {
                        if (userinfo.MemberLevelSettingId == s)
                        {
                            item.passed = false;
                            item.msg = "经查询，您的账号目前不享有此优惠！ R";
                            break;
                        }
                    }
                    if (!item.passed)
                    {
                        //回填失败
                        ActFromDB.confirmAct(item);
                        MyWrite(item.msg);
                        continue;
                    }

                    //查出上次的活动方案、签到天数 网络数据库(北京时间)  北京时间前一天
                    item.betTime = DateTime.Now.Date.ToString("yyyy/MM/dd 00:00:00");
                    item.lastOprTime = DateTime.Now.AddDays(-1).Date.ToString("yyyy/MM/dd 00:00:00");
                    betData bb_last = ActFromDB.getActData2_time(item);
                    //bb_last.gamename= item.Memo;
                    if (bb_last.PortalMemo != ""  && bb_last.Memo != "")
                    {
                        if (fa_now.ToString()==bb_last.Memo)
                        {
                            if (int.Parse(bb_last.PortalMemo) < 7 )
                            {
                                //bb_last.betno = (int.Parse(bb_last.PortalMemo) + 1).ToString();
                                //累加1天
                                if (ts_now- int.Parse(bb_last.PortalMemo)>1)
                                {
                                    ts_now = int.Parse(bb_last.PortalMemo) + 1;
                                }
                            }
                            else
                            {
                                //bb_last.betno = "8";
                                //大于7天就一直是7天
                                ts_now = 7;
                            }
                        }
                        else
                        {
                            //中断 从1开始
                            ts_now = 1;
                        }

                    }
                    else
                    {
                        ts_now = 1;
                        fa_now = 1;
                    }
                    //活动方案 默认1 活动关卡 默认1

                    //总存款要 100/ 1000 +
                    item.lastOprTime = DateTime.Now.ToString("yyyy/MM/dd 00:00:00");
                    item.betTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                    betData bb1 = platGPK.MemberTransactionSearch(item,false);
                    if (bb1 == null)
                    {
                        continue;
                    }
                    
                    if (!bb1.passed || bb1.total_money < Convert.ToDecimal(myConfig[prefix + "有效存款" + fa_now]))
                    {
                        //如果没有存款 
                        item.passed = false;
                        item.msg = "经查询，统计期间您并没有达到最低存款" + myConfig[prefix + "有效存款" + fa_now] + "元的最低标准！ R";
                        ActFromDB.confirmAct(item);
                        string msg = $"活动{item.aname}用户{item.username}处理完毕，处理为 {(item.passed ? "通过" : "不通过")}，回复消息 {item.msg}";
                        MyWrite(msg);
                        continue;
                    }

                    //投注查询 >4888 or >18888
                    item.gamename = null;
                    item.lastCashTime = DateTime.Now.Date.ToString("yyyy/MM/dd");
                    item.lastOprTime = DateTime.Now.Date.ToString("yyyy/MM/dd");//现在

                    foreach (var s in appSittingSet.readAppsettings(prefix + "游戏类别").Split(','))
                    {
                        item.gamename += "&types=" + string.Join("&types=", platGPK.KindCategories[int.Parse(s)].Replace("\"", "").Split(',').ToArray());
                    }

                    betData bb2 = platGPK.GetDetailInfo(item);
                    if (bb2 == null)
                    {
                        continue;
                    }
                    
                    if (!bb2.passed || bb2.Commissionable < Convert.ToDecimal(myConfig[prefix + "有效投注"+fa_now]))
                    {
                        //如果都不满足
                        item.passed = false;
                        item.msg = "经查询，统计期间您并没有达到最低" + myConfig[prefix + "有效投注" + fa_now] + "的投注标准！ R";
                        ActFromDB.confirmAct(item);
                        string msg = $"活动{item.aname}用户{item.username}处理完毕，处理为 {(item.passed ? "通过" : "不通过")}，回复消息 {item.msg}";
                        MyWrite(msg);
                        continue;
                    }

                    //计算钱
                    //decimal[] arr1 = Array.ConvertAll(myConfig[prefix + "赠送金额1"].ToString().Split('|'), decimal.Parse);
                    //decimal[] arr2 = Array.ConvertAll(myConfig[prefix + "赠送金额2"].ToString().Split('|'), decimal.Parse);
                    decimal[] arr_tmp =  Array.ConvertAll(myConfig[prefix + "赠送金额"+fa_now].ToString().Split('|'), decimal.Parse);
                    item.betMoney = arr_tmp[arr_tmp.Length - ts_now];
                    if (ts_now == 7)
                    {
                        item.betMoney += decimal.Parse(myConfig[prefix + "额外赠送金额" + fa_now].ToString());
                    }

                    item.gamename = fa_now.ToString();
                    item.betno = ts_now.ToString();
                    //加钱  提交数据
                    item.AuditType = "None";
                    //item.Audit = item.betMoney;
                    item.Memo = item.aname;
                    item.Type = 5;

                    bool fr = platGPK.MemberDepositSubmit(item);
                    //回填
                    item.msg = "恭喜您"+item.username+"，您申请的<" + item.aname + ">已通过活动专员的检验";
                    ActFromDB.confirmAct(item);
                    MyWrite(item.msg);
                    continue;

                }
                return Task.CompletedTask;
            }
        }

        /// <summary>
        /// 元旦送
        /// </summary>
        [DisallowConcurrentExecution]
        public class yds : IJob
        {
            public Task Execute(IJobExecutionContext context)
            {
                string prefix = context.Trigger.Description;

                List<betData> list = ActFromDB.getActData(myConfig[prefix + "活动编号"].ToString());
                if (list == null)
                {
                    MyWrite(myConfig[prefix + "活动名称"].ToString() + " 没有获取到记录，等待下次执行 ");
                    return Task.CompletedTask;
                }
                if (list.Count == 0)
                {
                    MyWrite(myConfig[prefix + "活动名称"].ToString() + "没有新的记录，等待下次执行 ");
                    return Task.CompletedTask;
                }
                foreach (var item in list)
                {
                    item.aname = myConfig[prefix + "活动名称"].ToString();

                    string sql = string.Format(appSittingSet.readConfig()["sql_give_select"].ToString(), item.username, item.aid);
                    object o = MySQLHelper.MySQLHelper.GetScalar(sql);
                    if (o != null)
                    {
                        DateTime dt;
                        DateTime.TryParse(o.ToString(), out dt);
                        if (dt.AddHours(-12).Date==DateTime.Now.Date)
                        {
                            item.passed = false;
                            item.msg = "您好，同一账号只能申请一次，申请不通过！R";
                            bool b = ActFromDB.confirmAct(item);
                            if (b)
                            {
                                string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                                MyWrite(msg);
                                appSittingSet.Log(msg);
                            }
                            continue;
                        }
                    }

                    //获取详细信息
                    Gpk_UserDetail userinfo = platGPK.GetUserDetail(item.username);
                    if (userinfo == null)
                    {
                        //账号不存在？
                        item.passed = false;
                        item.msg = "经查询，您的账号有误！ R";
                        ActFromDB.confirmAct(item);
                        string msg = $"活动{item.aname}用户{item.username}处理完毕，处理为 {(item.passed ? "通过" : "不通过")}，回复消息 {item.msg}";
                        MyWrite(msg);
                        continue;
                    }

                    //判断 层级是否在 列表之中
                    foreach (var s in FiliterGroups)
                    {
                        if (userinfo.MemberLevelSettingId == s)
                        {
                            item.passed = false;
                            item.msg = "经查询，您的账号目前不享有此优惠！ R";
                            break;
                        }
                    }
                    if (!item.passed)
                    {
                        //回填失败
                        ActFromDB.confirmAct(item);
                        MyWrite(item.msg);
                        continue;
                    }


                    //总存款要 100/ 1000 +
                    //item.lastOprTime = DateTime.Now.ToString("yyyy/MM/dd 00:00:00");
                    //item.betTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                    betData bb1 = platGPK.MemberTransactionSearch(item,false);
                    if (bb1 == null)
                    {
                        continue;
                    }


                    //计算 活动赠送金额
                    decimal ck1 = Convert.ToDecimal(myConfig[prefix + "有效存款1"]);
                    decimal ck2 = Convert.ToDecimal(myConfig[prefix + "有效存款2"]);
                    decimal zs1 = Convert.ToDecimal(myConfig[prefix + "赠送金额1"]);
                    decimal zs2 = Convert.ToDecimal(myConfig[prefix + "赠送金额2"]);
                    decimal jh1 = Convert.ToDecimal(myConfig[prefix + "稽核倍数1"]);
                    decimal jh2 = Convert.ToDecimal(myConfig[prefix + "稽核倍数2"]);

                    if (bb1.betMoney >= ck2 && bb1.betMoney <=ck2+ 1 && bb1.betTimes>=1)
                    {
                        //满足>=1次 存款金额大于1000
                        bb1.Audit = (bb1.betMoney + zs2) * jh2;
                        bb1.betMoney = zs2;
                        bb1.passed = true;

                    }
                    else if (bb1.betMoney >= ck1&& bb1.betMoney <= ck1+ 1&& bb1.betTimes >= 1)
                    {
                        //满足 >=1次 100
                        bb1.Audit = (bb1.betMoney + zs1) * jh1;
                        bb1.betMoney = zs1;
                        bb1.passed = true;
                    }
                    else
                    {
                        item.passed = false;
                        item.msg = "经查询，统计期间您并没有达到最低存款要求！ R";
                        ActFromDB.confirmAct(item);
                        string msg = $"活动{item.aname}用户{item.username}处理完毕，处理为 {(item.passed ? "通过" : "不通过")}，回复消息 {item.msg}";
                        MyWrite(msg);
                        continue;
                    }
                    

                    //加钱  提交数据
                    item.AuditType = "Discount";
                    item.Audit = bb1.Audit;
                    item.Memo = item.aname;
                    item.Type = 5;

                    bool fr = platGPK.MemberDepositSubmit(item);
                    //回填
                    item.msg = "恭喜您"+item.username+"，您申请的<" + item.aname + ">已通过活动专员的检验";
                    ActFromDB.confirmAct(item);
                    MyWrite(item.msg);
                    continue;

                }
                return Task.CompletedTask;
            }
        }

        /// <summary>
        /// 元旦送
        /// </summary>
        [DisallowConcurrentExecution]
        public class slyz : IJob
        {
            public Task Execute(IJobExecutionContext context)
            {
                string prefix = context.Trigger.Description;
                betData b;
                //获取活动列表
                List<int> list_act =  platGPK.ActGetList(myConfig[prefix + "活动编号"].ToString());
                if (list_act == null)
                {
                    MyWrite(myConfig[prefix + "活动名称"].ToString() + " 没有获取到记录，等待下次执行 ");
                    return Task.CompletedTask;
                }
                if (list_act.Count == 0)
                {
                    MyWrite(myConfig[prefix + "活动名称"].ToString() + "没有新的记录，等待下次执行 ");
                    return Task.CompletedTask;
                }

                //获取用户列表
                foreach (int item in list_act)
                {
                    b = new betData() { bbid = item.ToString(), aid = myConfig[prefix + "活动编号"].ToString() ,aname = myConfig[prefix + "活动名称"].ToString()};
                    List<int>  list_user= platGPK.ActGetRewardRecords(b);
                    /*
                    if (list_user!=null)
                    {
                        if (list_user.Count>0)
                        {
                            //批量处理
                            bool r = platGPK.ActSendRewards(list_user, b);
                            b.msg = $"{b.aname}已经处理{list_user.Count}条数据";
                            MyWrite(b.msg);
                        }
                        else
                        {
                            MyWrite(b.aname + "没有新的记录，等待下次执行 ");
                            continue;
                        }
                    }
                    else
                    {
                        MyWrite(b.aname+ " 没有获取到记录，等待下次执行 ");
                        //return Task.CompletedTask;
                        continue;
                    }
                    */

                    if (list_user == null)
                    {
                        MyWrite(b.aname+ " 没有获取到记录，等待下次执行 ");
                        //return Task.CompletedTask;
                        continue;
                    }
                    if (list_user.Count == 0)
                    {
                        MyWrite(b.aname + "没有新的记录，等待下次执行 ");
                        //return Task.CompletedTask;
                        continue;
                    }
                    //批量处理
                    bool r =  platGPK.ActSendRewards(list_user, b);
                    b.msg = $"{b.aname}已经处理{list_user.Count}条数据";
                    MyWrite(b.msg);
                }

                return Task.CompletedTask;
            }
        }
    }
}
