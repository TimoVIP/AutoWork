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
        static string msg="";
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
            if (Convert.ToUInt16( myConfig["清除N天前日志"] )>0)
            {
                await sched.ScheduleJob(JobBuilder.Create<清除日志>().Build(), TriggerBuilder.Create().WithCronSchedule("1 0 8 * * ? ").Build());
            }
            //登陆
            if (myConfig["清除N天前日志"].ToString() == "1")
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
                    if (item.Value.ToString()=="1")
                    {
                        string pre = item.Key.ToString().Replace("开关", "");
                        Type t = Type.GetType("AutoAct.Program+" +pre);
                        await sched.ScheduleJob(JobBuilder.Create(t).Build(), TriggerBuilder.Create().WithDescription(pre).WithSimpleSchedule(x => x.WithIntervalInSeconds(Convert.ToInt16(myConfig[pre+"间隔秒"])).RepeatForever()).Build());
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
                int diff =Convert.ToInt16(myConfig["清除N天前日志"]);
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
                    //注册时间必须要一年以上
                    if (userinfo.JoinTime > DateTime.Now.Date.AddYears(-1))
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
        public class mycj  : IJob
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
                    //注册时间必须要一月以上
                    if (userinfo.JoinTime > DateTime.Now.Date.AddMonths(-1).AddHours(-12))
                    {
                        item.passed = false;
                        item.msg = "经查询，您的账号注册不满一个月！ R";
                        ActFromDB.confirmAct(item);
                        string msg = $"活动{item.aname}用户{item.username}处理完毕，处理为 {(item.passed ? "通过" : "不通过")}，回复消息 {item.msg}";
                        MyWrite(msg);
                        continue;
                    }

                    betData bb = item;
                    //总存款要1000 +
                    item.lastOprTime = DateTime.Now.AddMonths(-1).ToString("yyyy/MM/dd 00:00:00");
                    item.betTime =DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                    bb = platGPK.MemberTransactionSearch(item);
                    //bb = platGPK.MemberGetDepositWithdrawInfo(userinfo);
                    if (bb.total_money < Convert.ToDecimal(myConfig[prefix+"总存款"]))
                    {
                        item.passed = false;
                        item.msg = "经查询，统计期间您并没有达到最低存款" + myConfig[prefix + "总存款"] + "元的最低标准！ R";
                        ActFromDB.confirmAct(item);
                        string msg = $"活动{item.aname}用户{item.username}处理完毕，处理为 {(item.passed ? "通过" : "不通过")}，回复消息 {item.msg}";
                        MyWrite(msg);
                        continue;
                    }

                    item.gamename = null;
                    item.lastCashTime = DateTime.Now.AddMonths(-2).ToString("yyyy/MM/dd");
                    item.lastOprTime = DateTime.Now.Date.ToString("yyyy/MM/dd");

                    foreach (var s in appSittingSet.readAppsettings(prefix+"游戏类别").Split(','))
                    {
                        item.gamename += "&types=" + string.Join("&types=", platGPK.KindCategories[int.Parse(s)].Replace("\"", "").Split(',').ToArray());
                    }
                    bb = platGPK.GetDetailInfo(item);
                    if (bb.Commissionable < Convert.ToDecimal(myConfig[prefix+"有效投注"]))
                    {
                        item.passed = false;
                        item.msg = "经查询，统计期间您并没有达到有效投注"+myConfig[prefix+"有效投注"] +"的标准！ R";
                        ActFromDB.confirmAct(item);
                        string msg = $"活动{item.aname}用户{item.username}处理完毕，处理为 {(item.passed ? "通过" : "不通过")}，回复消息 {item.msg}";
                        MyWrite(msg);
                        continue;
                    }


                    //加钱  提交数据
                    item.betMoney = Convert.ToDecimal(myConfig[prefix+"赠送金额"]);
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
        public class qpcg  : IJob
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
                    item.gamename = null;
                    item.lastCashTime = DateTime.Now.Date.ToString("yyyy/MM/dd");
                    item.lastOprTime = DateTime.Now.Date.ToString("yyyy/MM/dd");
                    foreach (var s in appSittingSet.readAppsettings(prefix+"游戏类别").Split(','))
                    {
                        item.gamename += "&types=" + string.Join("&types=", platGPK.KindCategories[int.Parse(s)].Replace("\"", "").Split(',').ToArray());
                    }

                    decimal[] arr = Array.ConvertAll(myConfig[prefix+"有效投注"].ToString().Split('|'),  decimal.Parse);
                    decimal[] arr2 =Array.ConvertAll(myConfig[prefix+"赠送金额"].ToString().Split('|'),  decimal.Parse);
                    decimal[] arr3 =Array.ConvertAll(myConfig[prefix +"累计赠送金额"].ToString().Split('|'),  decimal.Parse);
                    bb = platGPK.GetDetailInfo(item);
                    if (bb==null)
                    {
                        continue;
                    }
                    if (bb.Commissionable <arr[arr.Length-1] )
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
                    item.lastCashTime = DateTime.Now.Date.ToString("yyyy/MM/dd");
                    item.lastOprTime = DateTime.Now.Date.ToString("yyyy/MM/dd");
                    foreach (var s in appSittingSet.readAppsettings(prefix + "游戏类别").Split(','))
                    {
                        item.gamename += "&types=" + string.Join("&types=", platGPK.KindCategories[int.Parse(s)].Replace("\"", "").Split(',').ToArray());
                    }

                    decimal[] arr = Array.ConvertAll(myConfig[prefix + "有效投注"].ToString().Split('|'),  decimal.Parse);
                    decimal[] arr2 =Array.ConvertAll(myConfig[prefix + "赠送金额"].ToString().Split('|'),  decimal.Parse);
                    decimal[] arr3 =Array.ConvertAll(myConfig[prefix + "累计赠送金额"].ToString().Split('|'),  decimal.Parse);
                    bb = platGPK.GetDetailInfo(item);
                    if (bb==null)
                    {
                        continue;
                    }
                    if (bb.Commissionable <arr[arr.Length-1] )
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

                    for (int i = 0; i <arr.Length; i++)
                    {
                        if (bb.Commissionable>=arr[i])
                        {
                            item.betno = (arr.Length - i).ToString();
                            if (lastLevel_ ==0)
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
                    if (lastLevel_ > 0 &&  lastLevel_ >=  int.Parse(item.betno))
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
                    if (DateTime.Parse( item.betTime) > userinfo.JoinTime.AddHours(48))
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

                    foreach (var s in appSittingSet.readAppsettings(prefix+"游戏类别").Split(','))
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
                    string old_addtime = item.betTime;
                    //判断是否提交过 同一用户  只能一次 网上查询 新平台 同一天
                    string sql = string.Format(appSittingSet.readConfig()["sql_give_select"].ToString(), item.username, item.aid);
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

                    betData bb1 = item;

                    //总存款要 100/ 1000 +
                    item.lastOprTime = DateTime.Now.ToString("yyyy/MM/dd 00:00:00");
                    item.betTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                    bb1 = platGPK.MemberTransactionSearch(item);

                    /*
                    if (bb1.total_money > Convert.ToDecimal(myConfig[prefix + "有效存款2"]))
                    {
                        //如果是第二种
                        item.betTimes = 2;
                    }
                    else if (bb1.total_money > Convert.ToDecimal(myConfig[prefix + "有效存款1"]))
                    {
                        //如果是第一种
                        item.betTimes = 1;
                    }
                    else if (bb1.total_money < Convert.ToDecimal(myConfig[prefix + "有效存款1"]))
                    {
                        //如果都不满足
                        item.passed = false;
                        item.msg = "经查询，统计期间您并没有达到最低存款" + myConfig[prefix + "总存款"] + "元的最低标准！ R";
                        ActFromDB.confirmAct(item);
                        string msg = $"活动{item.aname}用户{item.username}处理完毕，处理为 {(item.passed ? "通过" : "不通过")}，回复消息 {item.msg}";
                        MyWrite(msg);
                        continue;
                    }

                    */

                    if (bb1==null)
                    {
                        continue;
                    }
                    if (bb1.total_money < Convert.ToDecimal(myConfig[prefix + "有效存款1"]))
                    {
                        //如果都不满足
                        item.passed = false;
                        item.msg = "经查询，统计期间您并没有达到最低存款" + myConfig[prefix + "总存款"] + "元的最低标准！ R";
                        ActFromDB.confirmAct(item);
                        string msg = $"活动{item.aname}用户{item.username}处理完毕，处理为 {(item.passed ? "通过" : "不通过")}，回复消息 {item.msg}";
                        MyWrite(msg);
                        continue;
                    }






                    //投注查询 >4888 or >18888
                    item.gamename = null;

                    item.lastCashTime = DateTime.Now.Date.ToString("yyyy/MM/dd");
                    item.lastOprTime = DateTime.Now.Date.ToString("yyyy/MM/dd");//现在

                    foreach (var s in appSittingSet.readAppsettings(prefix+"游戏类别").Split(','))
                    {
                        item.gamename += "&types=" + string.Join("&types=", platGPK.KindCategories[int.Parse(s)].Replace("\"", "").Split(',').ToArray());
                    }

                    betData bb2 = item;
                    bb2 = platGPK.GetDetailInfo(item);
                    if (bb2 == null)
                    {
                        continue;
                    }

                    if (bb2.Commissionable < Convert.ToDecimal(myConfig[prefix + "有效存款1"]))
                    {
                        //如果都不满足
                        item.passed = false;
                        item.msg = "经查询，统计期间您并没有达到有效投注标准！ R";
                        ActFromDB.confirmAct(item);
                        string msg = $"活动{item.aname}用户{item.username}处理完毕，处理为 {(item.passed ? "通过" : "不通过")}，回复消息 {item.msg}";
                        MyWrite(msg);
                        continue;
                    }

                    decimal[] arr1 = Array.ConvertAll(myConfig[prefix + "赠送金额1"].ToString().Split('|'), decimal.Parse);
                    decimal[] arr2 = Array.ConvertAll(myConfig[prefix + "赠送金额2"].ToString().Split('|'), decimal.Parse);


                    //从数据库查出上次是第几天 玩法 1or 2 网络数据库
                    item.betTime = old_addtime;
                    betData bb3 = ActFromDB.getActData2_time(item);
                    if (bb3.PortalMemo=="" && bb3.Memo=="")
                    {
                        bb3.PortalMemo = "0";//上次关卡
                        bb3.Memo = "1";//游戏方案 1 or 2 默认1
                    }

                    if (int.Parse(bb3.PortalMemo)<=7)
                    {
                        item.betno = (int.Parse(bb3.PortalMemo) + 1).ToString();
                    }
                    else
                    {
                        item.betno = "8";//大于7天就一只是8天
                    }

                    if (bb1.total_money > Convert.ToDecimal(myConfig[prefix + "有效存款2"])  && bb2.Commissionable> Convert.ToDecimal(myConfig[prefix + "有效投注2"]) &&  bb3.Memo == "2")
                    {
                        item.betMoney = arr2[arr2.Length - int.Parse(item.betno)];
                        item.gamename = "2";
                    }
                    else if (bb1.total_money > Convert.ToDecimal(myConfig[prefix + "有效存款1"]) && bb2.Commissionable > Convert.ToDecimal(myConfig[prefix + "有效投注1"]) &&  bb3.Memo=="1")
                    {
                        item.betMoney = arr1[arr2.Length - int.Parse(item.betno)];
                        item.gamename = "1";
                    }
                    else
                    {
                        item.passed = false;
                        item.msg = "经查询，统计期间您并没有达到有效投注标准,或者存款没有达标！ R";
                        ActFromDB.confirmAct(item);
                        string msg = $"活动{item.aname}用户{item.username}处理完毕，处理为 {(item.passed ? "通过" : "不通过")}，回复消息 {item.msg}";
                        MyWrite(msg);
                        continue;
                    }

                    /*
                     * 
                    //从数据库查出上次是第几天 玩法 1or 2
                    string dt_ = DateTime.Now.AddHours(-12).ToString("yyyy-MM-dd");
                    //sql = $"select  betno from record where pass=1 and aid ={ item.aid} and LOWER(username)='{ item.username.ToLower()}'   and subtime > '{dt_ } 00:00:01' and  subtime < '{ dt_ } 23:59:59' order by rowid desc limit 1; ";
                    //int lastLevel_ = SQLiteHelper.SQLiteHelper.execScalarSql<int>(sql);//int null 0
                    sql = $" select (CASE when (gamename IS NULL OR gamename='') then '0' else gamename end) as gamename, betno from record where pass=1 and aid ={ item.aid} and LOWER(username)='{ item.username.ToLower()}'   and subtime > '{dt_ } 00:00:01' and  subtime < '{ dt_ } 23:59:59' order by rowid desc limit 1; ";
                    DataTable dt = SQLiteHelper.SQLiteHelper.getDataTableBySql(sql);
                    int lastLevel = 0;
                    int lastLevelType = 0;
                    if (dt.Rows.Count > 0)
                    {
                        lastLevel = int.Parse(dt.Rows[0][1].ToString());
                        lastLevelType = int.Parse(dt.Rows[0][0].ToString());
                    }





                    //item.betno = (lastLevel + 1).ToString() ;

                    if (bb1.total_money > Convert.ToDecimal(myConfig[prefix + "有效存款2"])  && bb2.Commissionable> Convert.ToDecimal(myConfig[prefix + "有效投注2"]) && ( lastLevelType==0 || lastLevelType==2))
                    {
                        item.betMoney = arr2[arr2.Length-1 - lastLevel];
                        item.gamename = "2";
                    }
                    else if (bb1.total_money > Convert.ToDecimal(myConfig[prefix + "有效存款1"]) && bb2.Commissionable > Convert.ToDecimal(myConfig[prefix + "有效投注1"]) && (lastLevelType == 0 || lastLevelType == 1))
                    {
                        item.betMoney = arr1[arr2.Length-1 - lastLevel];
                        item.gamename = "1";
                    }
                    else
                    {
                        item.passed = false;
                        item.msg = "经查询，统计期间您并没有达到有效投注标准,或者存款没有达标！ R";
                        ActFromDB.confirmAct(item);
                        string msg = $"活动{item.aname}用户{item.username}处理完毕，处理为 {(item.passed ? "通过" : "不通过")}，回复消息 {item.msg}";
                        MyWrite(msg);
                        continue;
                    }



                    //大于7天 额外的奖励
                    if (lastLevel>=7)
                    {
                        if (lastLevelType==1)
                        {
                            item.betMoney += arr1[0];
                        }
                        else if (lastLevelType==2)
                        {
                            item.betMoney += arr2[0];
                        }
                    }

                    */




                    /*
                    else
                    {
                        decimal[] arr1 = Array.ConvertAll(myConfig[prefix + "赠送金额1"].ToString().Split('|'), decimal.Parse);
                        decimal[] arr2 = Array.ConvertAll(myConfig[prefix + "赠送金额2"].ToString().Split('|'), decimal.Parse);
                        //从数据库查出是第几天，计算送多少钱
                        
                        if (bb2.Commissionable > Convert.ToDecimal(myConfig[prefix + "有效存款2"]))
                        {
                            //如果是第二种
                            if (lastLevelType==2)
                            {
                                item.betMoney = arr2[arr2.Length - lastLevel];
                            }
                            else
                            {
                                item.betMoney = arr2[arr2.Length-1];
                            }

                        }
                        else if (bb2.Commissionable > Convert.ToDecimal(myConfig[prefix + "有效存款1"]))
                        {
                            //如果是第一种
                            if (lastLevelType==1)
                            {
                                item.betMoney = arr1[arr1.Length - lastLevel];
                            }
                            else
                            {
                                item.betMoney = arr1[arr1.Length-1];
                            }
                        }
                    }
                    */

                    //加钱  提交数据
                    item.AuditType = "None";
                    //item.Audit = item.betMoney;
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
    }


}
