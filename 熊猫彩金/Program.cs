using BaseFun;
using MySQLHelper;
using Quartz;
using Quartz.Impl;
using SQLiteHelper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimoControl;

namespace 熊猫彩金
{
    class Program
    {
        static Hashtable myConfig = new Hashtable();
        static bool b = false;
        static string msg = "";
        //private static string[] FiliterGroups;
        static DateTime dt = DateTime.Now;
        static List<betData> list_temp = new List<betData>();
        static void Main(string[] args)
        {

            myConfig = appSittingSet.readConfig();
            Console.Title = myConfig["platname"].ToString();
            //FiliterGroups = myConfig["FiliterGroups"].ToString().Split('|');
            //prefix = myConfig["prefix"].ToString().Split('|');
            Console.TreatControlCAsInput = true;

            // 连接数据库
            b = ActFromDB.loginActivity();
            msg = $"活动站登录/连接{(b ? "成功" : "失败")} ";
            appSittingSet.Log(msg);
            MyWrite(msg);

            //登陆
            b = platBET02.login();
            msg = $"BET登陆{(b ? "成功" : "失败")} {DateTime.Now.ToString()}";
            Console.WriteLine(msg);
            appSittingSet.Log(msg);


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

            //方法二
            foreach (DictionaryEntry item in myConfig)
            {
                if (item.Key.ToString().EndsWith("开关"))
                {
                    if (item.Value.ToString() == "1")
                    {
                        string pre = item.Key.ToString().Replace("开关", "");
                        Type t = Type.GetType("熊猫彩金.Program+" + pre);
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
        /// 注册彩金
        /// </summary>
        [DisallowConcurrentExecution]
        public class zccj : IJob
        {
            public Task Execute(IJobExecutionContext context)
            {

                //判断时间 过了5分钟就发送查询一下
                if (dt.AddMinutes(5) < DateTime.Now)
                {
                    bool b = platBET02.findBalanceByUserAccount(new betData() { username = "wuwuliu", Memo = "测试" });
                }

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

                    //记录一个list 去除重复后 的第9，10 条 直接拒绝掉
                    int jj = int.Parse(myConfig[prefix + "拒绝率"].ToString());

                    if (jj>0)
                    {
                        //如果达到10条清除掉
                        if (list_temp.Count == 10)
                        {
                            list_temp.Clear();
                        }

                        if (!list_temp.Exists(x => x.bbid == item.bbid))
                        {
                            list_temp.Add(item);
                        }

                        if (list_temp.Count > (10 -jj))
                        {
                            //直接拒绝
                            item.passed = false;
                            item.msg = "RR 同IP其他会员已申请过";
                            ActFromDB.confirmAct(item);
                            MyWrite(item.msg);
                            continue;
                        }
                    }


                    item.aname = myConfig[prefix + "活动名称"].ToString();
                    //判断是否提交过 同一用户  只能一次 网上查询 新平台 
                    string sql = string.Format(appSittingSet.readConfig()["sql_give_select"].ToString(), item.username, item.aid);
                    if (MySQLHelper.MySQLHelper.Exsist(sql))
                    {
                        item.passed = false;
                        item.msg = "您好，同一账号只能申请一次，申请不通过！R";
                        bool b = ActFromDB.confirmAct(item);
                        string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                        MyWrite(msg);
                        appSittingSet.Log(msg);
                        continue;
                    }




                    //存款大于0 +手机号
                    betData bb = platBET02.findByMemberUserPageResult(item);
                    if (bb != null)
                    {
                        if (!bb.passed)
                        {
                            item.passed = false;
                            item.msg = "您好，您的账号有误，申请不通过！R";
                            bool b = ActFromDB.confirmAct(item);
                            string msg = $"活动{item.aname}用户{item.username}处理完毕，处理为 {(item.passed ? "通过" : "不通过")}，回复消息 {item.msg}";
                            MyWrite(msg);
                            continue;
                        }

                        //设备
                        if ( !myConfig[prefix + "允许登陆设备"].ToString().Contains( bb.Type.ToString()))
                        {
                            item.passed = false;
                            item.msg = "您好，当前登陆设备不允许参加活动，申请不通过！R";
                            string msg = $"活动{item.aname}用户{item.username}处理完毕，处理为 {(item.passed ? "通过" : "不通过")}，回复消息 {item.msg}";
                            MyWrite(msg);
                            bool b = ActFromDB.confirmAct(item);
                            continue;
                        }
                        //几率 来路不一样 不好做

                        //存款大于0
                        if (bb.DepositTotal>0)
                        {
                            item.passed = false;
                            item.msg = "您好，存款必须小于等于0元，申请不通过！R";
                            string msg = $"活动{item.aname}用户{item.username}处理完毕，处理为 {(item.passed ? "通过" : "不通过")}，回复消息 {item.msg}";
                            MyWrite(msg);
                            bool b = ActFromDB.confirmAct(item);
                            continue;
                        }
                        //手机号
                        if (bb.PortalMemo.Length<11)
                        {
                            item.passed = false;
                            item.msg = "您好，需要绑定手机号，申请不通过！R";
                            string msg = $"活动{item.aname}用户{item.username}处理完毕，处理为 {(item.passed ? "通过" : "不通过")}，回复消息 {item.msg}";
                            MyWrite(msg);
                            bool b = ActFromDB.confirmAct(item);
                            continue;
                        }
                        //24小时内
                        if (appSittingSet.unixTimeToTime( bb.betTime).AddHours(24)<DateTime.Now)
                        {
                            item.passed = false;
                            item.msg = "您好，注册时间必须24小时以内，申请不通过！R";
                            string msg = $"活动{item.aname}用户{item.username}处理完毕，处理为 {(item.passed ? "通过" : "不通过")}，回复消息 {item.msg}";
                            MyWrite(msg);
                            bool b = ActFromDB.confirmAct(item);
                            continue;
                        }

                    }
                    else
                    {
                        continue;
                    }

                    //判断 层级是否在 列表之中
                    /*
                    foreach (var s in FiliterGroups)
                    {
                        if (bb.level == s)
                        {
                            item.passed = false;
                            item.msg = "经查询，您的账号层级目前不享有此优惠！ R";
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

    */

                    //绑定银行卡，姓名
                    bb = platBET02.findByUserCard(item);
                    if (bb!=null)
                    {
                        if (!bb.passed)
                        {
                            item.passed = false;
                            item.msg = "您好，必须绑定银行账户信息，申请不通过！R";
                            string msg = $"活动{item.aname}用户{item.username}处理完毕，处理为 {(item.passed ? "通过" : "不通过")}，回复消息 {item.msg}";
                            MyWrite(msg);
                            bool b = ActFromDB.confirmAct(item);
                            continue;
                        }
                    }
                    else
                    {
                        continue;
                    }

                    //同IP 只能一个
                    int c = platBET02.findMemberUserLoginLog(item);

                    if (c > int.Parse(myConfig[prefix + "同IP最多几个"].ToString()))
                    {
                        item.passed = false;
                        item.msg = "您好，您的IP存在其他多个会员账号违反活动规则的呢！申请不通过！R";
                        bool b = ActFromDB.confirmAct(item);
                        msg = $"活动{item.aname}用户{item.username}处理完毕，处理为 {(item.passed ? "通过" : "不通过")}，回复消息 {item.msg}";
                        MyWrite(msg);
                        continue;
                    }


                    //加钱
                    if (item.passed)
                    {
                        item.betMoney = decimal.Parse(myConfig[prefix + "赠送金额"].ToString());
                        item.Audit =  decimal.Parse(myConfig[prefix + "需求打码倍数"].ToString());
                        item.Memo = myConfig[prefix + "活动名称"].ToString();
                        item.AuditType = myConfig[prefix + "打码包含余额"].ToString();
                        //b=platBET02.updateDeposit(item);
                        b = platBET02.updateHandsel(item);
                        msg = $"活动{item.aname}用户{item.username}处理完毕，充值{item.betMoney} ";
                        MyWrite(msg);

                        //修改层级
                        item.level = myConfig[prefix + "拉到层级编号"].ToString();
                        b =platBET02.updateMemberLevel(item);

                    }
                    item.msg = $"恭喜您，您申请的<{item.aname}>已通过活动专员的检验";
                         b = ActFromDB.confirmAct(item);



                    dt = DateTime.Now;
                }

                return Task.CompletedTask;

            }


        }
    }
}
