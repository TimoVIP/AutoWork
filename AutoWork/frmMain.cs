using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Quartz;
using Quartz.Impl;
using TimoControl;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading;
using System.IO;
using System.Linq;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace AutoWork_Plat1
{

    public delegate void Write(string msg);//写lv消息

    public delegate void ClsListItem();
    public partial class frmMain : Form
    {

        static string platname;
        static int[] interval;
        static int maxValue;
        private static string[] FiliterGroups;
        static string[] actInfo;
        static string[] sLuckNum;
        static string[] gameNames;
        static int[] rolltimes;
        static int[] Act4Stander;
        static string[] memberLevel;
        static string[] AutoCls;
        static int[] Act4Set;
        static string[] Prob;
        static string[] mailbody;
        static string[] KindCategories;
        static List<betData> list_temp = new List<betData>();
        NotifyIcon notify;
        public frmMain()
        {
            InitializeComponent();
            try
            {
                string[] its =appSittingSet.readAppsettings("Interval").Split('|');
                interval = new int[its.Length];
                for (int i = 0; i < its.Length; i++)
                {
                    interval[i] = int.Parse(its[i]);
                }

                FiliterGroups = appSittingSet.readAppsettings("FiliterGroups").Split('|');
                //sLuckNum = appSittingSet.readAppsettings("LuckNum").Split(new char[] { '|', '@' }, StringSplitOptions.RemoveEmptyEntries);//获取幸运数字数组
                sLuckNum = appSittingSet.readAppsettings("LuckNum").Split('|');//格式  33333333@888@88888|3@1@88 
                gameNames = appSittingSet.readAppsettings("gameName").Split('|');
                string[] sa = appSittingSet.readAppsettings("RollTimes").Split(new char[] { '|', '@' }, StringSplitOptions.RemoveEmptyEntries);
                rolltimes = new int[sa.Length];
                for (int i = 0; i < sa.Length; i++)
                {
                    rolltimes[i] = int.Parse(sa[i]);
                }

                string[] a4 = appSittingSet.readAppsettings("Act4Stander").Split(new char[] { '|', '@' }, StringSplitOptions.RemoveEmptyEntries);
                Act4Stander = new int[a4.Length];
                for (int i = 0; i < a4.Length; i++)
                {
                    Act4Stander[i] = int.Parse(a4[i]);
                }

                string[] a41 = appSittingSet.readAppsettings("Act4Set").Split(new char[] { '|', '@' }, StringSplitOptions.RemoveEmptyEntries);
                Act4Set = new int[a41.Length];
                for (int i = 0; i < a41.Length; i++)
                {
                    Act4Set[i] = int.Parse(a41[i]);
                }

                actInfo = appSittingSet.readAppsettings("actInfo").Split(new char[] { '|', '@' }, StringSplitOptions.RemoveEmptyEntries);
                maxValue = int.Parse(appSittingSet.readAppsettings("MaxValue"));
                memberLevel = appSittingSet.readAppsettings("MemberLevel").Split(new char[] { '|', '@' }, StringSplitOptions.RemoveEmptyEntries);
                platname = appSittingSet.readAppsettings("platname");
                AutoCls = appSittingSet.readAppsettings("AutoCls").Split('|');
                Prob = appSittingSet.readAppsettings("Prob").Split('|');
                mailbody = appSittingSet.readAppsettings("mailbody").Split(new char[] { '|', '@' }, StringSplitOptions.RemoveEmptyEntries);
                KindCategories = appSittingSet.readAppsettings("KindCategories").Split('|');//游戏分类
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < actInfo.Length; i += 3)
                {
                    sb.Append(actInfo[i + 1]);
                    sb.Append(actInfo[i + 2] == "1" ? "开 " : "关 ");
                }
                //sb.Append(" " + Prob[0] + "0%随机拒绝" + (Prob[1] == "1" ? "开" : "关"));
                this.Text = platname;
                this.toolStripStatusLabel1.Text = sb.ToString();
                this.Icon = new System.Drawing.Icon(AutoWork.Properties.Resources.favicon, 256, 256);

                notify = new NotifyIcon();
                notify.BalloonTipTitle = "提示";
                notify.BalloonTipText = "程序已经最小化";
                notify.BalloonTipIcon = ToolTipIcon.Info;
                notify.Text = "程序已经最小化";
                notify.Click += Notify_Click;
                notify.Icon = new System.Drawing.Icon(AutoWork.Properties.Resources.favicon, 256, 256);
                notify.BalloonTipClicked += Notify_BalloonTipClicked;

                //创建日志目录
                if (!Directory.Exists(appSittingSet.logPath))
                {
                    Directory.CreateDirectory(appSittingSet.logPath);
                }

            }
            catch (Exception ex)
            {
                MyWrite("获取配置文件失败，请配置文件后重新启动程序" + ex.Message);
                return;
            }
        }


        #region 调度

        static IScheduler sched;
        /// <summary>
        /// 开始调度
        /// </summary>
        public static void start()
        {
            //创建一个作业调度池
            ISchedulerFactory schedf = new StdSchedulerFactory();
            sched = schedf.GetScheduler();

            //1小时一次发送心跳
            sched.ScheduleJob(JobBuilder.Create<MyJobHeartBeat>().Build(), TriggerBuilder.Create().WithSimpleSchedule(x => x.WithIntervalInHours(1).RepeatForever()).Build());

            //清除一周前的数据、日志文件
            if (AutoCls[1] == "1")
            {
                sched.ScheduleJob(JobBuilder.Create<MyJob_cls>().Build(), TriggerBuilder.Create().WithCronSchedule("1 0 8 * * ? ").Build());
            }
            if (AutoCls[2] == "1")
            {
                //0 6 12 18 小时1:10s 执行 登陆
                sched.ScheduleJob(JobBuilder.Create<MyJob_login>().Build(), TriggerBuilder.Create().WithCronSchedule("10 1 0,6,12,18 * * ? ").Build());
            }
            //5秒一次 消除奖
            if (actInfo[2] == "1")
            {
                sched.ScheduleJob(JobBuilder.Create<MyJob2>().Build(), TriggerBuilder.Create().WithSimpleSchedule(x => x.WithIntervalInSeconds(interval[0]).RepeatForever()).Build());
            }
            //8秒一次 幸运尾数
            if (actInfo[5] == "1")
            {
                sched.ScheduleJob(JobBuilder.Create<MyJob3>().Build(), TriggerBuilder.Create().WithSimpleSchedule(x => x.WithIntervalInSeconds(interval[1]).RepeatForever()).Build());
            }
            //6秒一次 以小博大
            if (actInfo[8] == "1")
            {
                sched.ScheduleJob(JobBuilder.Create<MyJob4>().Build(), TriggerBuilder.Create().WithSimpleSchedule(x => x.WithIntervalInSeconds(interval[2]).RepeatForever()).Build());
            }
            //7秒一次 首存17送20
            if (actInfo[11] == "1")
            {
                sched.ScheduleJob(JobBuilder.Create<MyJob5>().Build(), TriggerBuilder.Create().WithSimpleSchedule(x => x.WithIntervalInSeconds(interval[3]).RepeatForever()).Build());
            }
            //8秒一次 体验金活动
            if (actInfo[14] == "1")
            {
                sched.ScheduleJob(JobBuilder.Create<MyJob6>().Build(), TriggerBuilder.Create().WithSimpleSchedule(x => x.WithIntervalInSeconds(interval[4]).RepeatForever()).Build());
            }
            //笔笔救援活动
            if (actInfo[17] == "1")
            {
                platGPK.needsocket = true;
                sched.ScheduleJob(JobBuilder.Create<MyJob7>().Build(), TriggerBuilder.Create().WithSimpleSchedule(x => x.WithIntervalInSeconds(interval[5]).RepeatForever()).Build());
            }
            ////登陆有礼
            //if (actInfo[20] == "1")
            //{
            //    sched.ScheduleJob(JobBuilder.Create<MyJob8>().Build(), TriggerBuilder.Create().WithSimpleSchedule(x => x.WithIntervalInSeconds(interval[6]).RepeatForever()).Build());
            //}
            ////存款即送
            //if (actInfo[23] == "1")
            //{
            //    sched.ScheduleJob(JobBuilder.Create<MyJob9>().Build(), TriggerBuilder.Create().WithSimpleSchedule(x => x.WithIntervalInSeconds(interval[7]).RepeatForever()).Build());
            //}


            //登陆有礼
            if (actInfo[20] == "1")
            {
                sched.ScheduleJob(JobBuilder.Create<MyJob10>().Build(), TriggerBuilder.Create().WithSimpleSchedule(x => x.WithIntervalInSeconds(interval[6]).RepeatForever()).Build());
            }
            //存款即送
            if (actInfo[23] == "1")
            {
                sched.ScheduleJob(JobBuilder.Create<MyJob11>().Build(), TriggerBuilder.Create().WithSimpleSchedule(x => x.WithIntervalInSeconds(interval[7]).RepeatForever()).Build());
            }

            //开始运行
            sched.Start();
        }
        /// <summary>
        /// 规定时间发送心跳
        /// </summary>
        public class MyJobHeartBeat : IJob
        {
            public void Execute(IJobExecutionContext context)
            {
                //发送心跳
                if (platGPK.wsk != null && platGPK.wsk.ReadyState == WebSocketSharp.WebSocketState.Open)
                {
                    platGPK.wsk.Ping();
                }
            }
        }

        /// <summary>
        /// 每天8:00:01 执行 删除一周前的日志 数据库一周前的数据
        /// </summary>
        public class MyJob_cls : IJob
        {
            public void Execute(IJobExecutionContext context)
            {
                int diff = int.Parse(AutoCls[0]);
                string sql = "delete from record where subminttime < '" + DateTime.Now.AddDays(-diff).Date.ToString("yyyy-MM-dd") + "'";
                appSittingSet.execSql(sql);
                appSittingSet.Log("清除一周前的数据");
                appSittingSet.clsLogFiles(diff);
                appSittingSet.Log("清除一周前的日志");
            }
        }

        /// <summary>
        /// /0 6 12 18 小时登录
        /// </summary>
        public class MyJob_login : IJob
        {
            public void Execute(IJobExecutionContext context)
            {
                mycls(); //清除listbox 信息

                //1.登录优惠大厅 一次 保存cookie 
                string msg = string.Format("活动站登录{0} ", platACT.loginActivity() ? "成功" : "失败");
                appSittingSet.Log(msg);
                MyWrite(msg);

                msg = string.Format("BB站登录{0} ", platBB.loginBB() ? "成功" : "失败");
                appSittingSet.Log(msg);
                MyWrite(msg);

                msg = string.Format("GPK站登录{0} ", platGPK.loginGPK() ? "成功" : "失败");
                appSittingSet.Log(msg);
                MyWrite(msg);
                //登陆活动站2 28
                msg = string.Format("活动站2登录{0} ", platACT2.login() ? "成功" : "失败");
                appSittingSet.Log(msg);
                MyWrite(msg);
            }
        }


        /// <summary>
        /// 作业 处理注单 活动1 消除奖
        /// </summary>
        [DisallowConcurrentExecution]
        public class MyJob2 : IJob
        {
            public void Execute(IJobExecutionContext context)
            {
                //获取注单号码  yh
                List<betData> list = platACT.getActData(actInfo[0]);
                if (list == null)
                {
                    MyWrite(actInfo[1] + " 没有获取到新的注单，等待下次执行 ");
                    return;
                }
                if (list.Count == 0)
                {
                    MyWrite(actInfo[1] + " 没有新的注单，等待下次执行 ");
                    return;
                }

                //获取注单详情
                foreach (var item in list)
                {
                    item.aid = actInfo[0];
                    item.aname = actInfo[1];
                    //0 注单号不合法 驳回
                    if (item.betno != "" && Regex.IsMatch(item.betno, @"^\d{12}$"))
                    {

                    }
                    else
                    {
                        //回填失败
                        item.passed = false;
                        item.msg = "请提供正确的注单号 R";
                        bool b1 = platACT.confirmAct(item);
                        if (b1)
                        {
                            string msg = string.Format("用户{0}处理完毕，注单号{1}，处理为 {2}，回复消息 {3}", item.username, item.betno, item.passed ? "通过" : "不通过", item.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        continue;
                    }

                    //2.获取注单详情信息 bb
                    betData bb = platBB.getBet_Details(item);
                    //3.是否匹配要求
                    if (bb == null)
                    {
                        continue;
                    }

                    //查询不到信息 注单号不存在 情况+ 网站维护
                    if (!bb.passed)
                    {

                        bool b = platACT.confirmAct(bb);
                        if (b)
                        {
                            string msg = string.Format("用户{0}处理完毕，注单号{1}，处理为 {2}，回复消息 {3}", bb.username, bb.betno, bb.passed ? "通过" : "不通过", bb.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        continue;
                    }

                    //判断 美东时间
                    DateTime bt = Convert.ToDateTime(bb.betTime).Date;
                    DateTime now_d = DateTime.Now.AddHours(-12).Date;
                    if (bt != now_d)
                    {
                        bb.passed = false;
                        bb.msg = "您好，非美东时间当天注单，申请不通过！R";
                        bool b2 = platACT.confirmAct(bb);
                        if (b2)
                        {
                            string msg = string.Format("用户{0}处理完毕，注单号{1}，处理为 {2}，回复消息 {3}", bb.username, bb.betno, bb.passed ? "通过" : "不通过", bb.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        continue;
                    }

                    //判断 游戏 名称
                    bool flag2 = false;
                    foreach (var g in gameNames[0].Split('@'))
                    {
                        if (g == bb.gamename)
                        {
                            flag2 = true;
                            break;
                        }
                    }
                    if (!flag2)
                    {
                        bb.passed = false;
                        bb.msg = string.Format("此活动仅限{0}游戏 R", gameNames[0].Replace("@",","));
                        bool b3 = platACT.confirmAct(bb);
                        if (b3)
                        {
                            string msg = string.Format("用户{0}处理完毕，注单号{1}，处理为 {2}，回复消息 {3}", bb.username, bb.betno, bb.passed ? "通过" : "不通过", bb.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        continue;
                    }

                    //免费游戏直接驳回 不通过
                    if (bb.betMoney == 0)
                    {
                        bb.passed = false;
                        bb.msg = "你好，免费游戏注单不享受此活动！R";
                        bool b4 = platACT.confirmAct(bb);
                        if (b4)
                        {
                            string msg = string.Format("用户{0}处理完毕，注单号{1}，处理为 {2}，回复消息 {3}", bb.username, bb.betno, bb.passed ? "通过" : "不通过", bb.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        continue;
                    }

                    //判断是否提交过 同一用户 同类游戏 一天只能一次
                    string sql = "select * from record where (betno='" + bb.betno + "' and pass=1 and aid=" + bb.aid + " ) or (pass=1  and aid=" +bb.aid + " and  username='" + bb.username + "' and gamename='" + bb.gamename + "'  and subminttime > '" + DateTime.Now.AddHours(-12).ToString("yyyy-MM-dd") + " 00:00:01' and  subminttime < '" + DateTime.Now.AddHours(-12).ToString("yyyy-MM-dd") + " 23:59:59') ";
                    if (appSittingSet.recorderDbCheck(sql))
                    {
                        bb.passed = false;
                        bb.msg = "您好，同一游戏一天内只能申请一次，申请不通过！R";
                        bool b5 = platACT.confirmAct(bb);
                        if (b5)
                        {
                            string msg = string.Format("用户{0}处理完毕，注单号{1}，处理为 {2}，回复消息 {3}", bb.username, bb.betno, bb.passed ? "通过" : "不通过", bb.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        continue;
                    }

                    //获取次数
                    bb = platBB.getBetDetail_Bet_Times(bb);
                    if (!bb.passed)
                    {
                        bool b5 = platACT.confirmAct(bb);
                        if (b5)
                        {
                            string msg = string.Format("用户{0}处理完毕，注单号{1}，处理为 {2}，回复消息 {3}", bb.username, bb.betno, bb.passed ? "通过" : "不通过", bb.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        continue;
                    }


                    //计算规则 多少次 送多少倍
                    if (bb.betTimes >= rolltimes[0] && bb.betTimes < rolltimes[1])
                    {
                        bb.betMoney *= rolltimes[2];
                    }
                    else if (bb.betTimes >= rolltimes[3] && bb.betTimes < rolltimes[4])
                    {
                        bb.betMoney *= rolltimes[5];
                    }
                    else if (bb.betTimes >= rolltimes[6] && bb.betTimes < rolltimes[7])
                    {
                        bb.betMoney *= rolltimes[8];
                    }
                    else if (bb.betTimes >= rolltimes[9])
                    {
                        bb.betMoney *= rolltimes[11];
                    }
                    else
                    {
                        bb.betMoney *= 0;
                        bb.msg = string.Format("此活动仅限消除{0}次及以上，您提供的注单消除次数为{1}次，申请不通过！R", rolltimes[0], bb.betTimes);
                        bb.passed = false;

                        bool b7 = platACT.confirmAct(bb);
                        if (b7)
                        {
                            string msg = string.Format("用户{0}处理完毕，注单号{1}，处理为 {2}，回复消息 {3}", bb.username, bb.betno, bb.passed ? "通过" : "不通过", bb.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        continue;
                    }

                    bb = platGPK.checkInGPK(bb);
                    if (bb == null)
                    {
                        continue;
                    }
                    else
                    {
                        if (!bb.passed)
                        {
                            bb.msg = "经查询，您的账号或者钱包不正确！ R";
                            bool b6 = platACT.confirmAct(bb);
                            if (b6)
                            {
                                string msg = string.Format("用户{0}处理完毕，注单号{1}，处理为 {2}，回复消息 {3}", bb.username, bb.betno, bb.passed ? "通过" : "不通过", bb.msg);
                                MyWrite(msg);
                                appSittingSet.Log(msg);
                                continue;
                            }
                        }
                    }

                    //判断 层级是否在 列表之中
                    foreach (var s in FiliterGroups)
                    {
                        if (bb.level== s)
                        {
                            bb.passed = false;
                            bb.msg = "经查询，您的账号目前不享有此优惠！ R";
                            break;
                        }
                    }
                    if (!bb.passed)
                    {
                        //回填失败
                        bool b6 = platACT.confirmAct(bb);
                        if (b6)
                        {
                            string msg = string.Format("用户{0}处理完毕，注单号{1}，处理为 {2}，回复消息 {3}", bb.username, bb.betno, bb.passed ? "通过" : "不通过", bb.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                            continue;
                        }
                    }

                    //大于 max 不处理
                    if (bb.betMoney > maxValue)
                    {
                        bb.passed = false;
                        bb.msg = "无法通过，请联系客服处理 R";
                        bool b = platACT.confirmAct(bb);
                        if (b)
                        {
                            string msg = string.Format("用户{0}处理完毕，注单号{1}，处理为 {2}，回复消息 {3}", bb.username, bb.betno, bb.passed ? "通过" : "不通过", bb.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        continue;
                    }
                    //次数符合
                    //提交充值 加钱 
                    if (bb.passed)
                    {
                        bb.AuditType = "Discount";
                        bb.Audit = bb.betMoney;
                        bb.Memo = bb.aname;
                        bb.Type = 5;

                        bool fr = platGPK.MemberDepositSubmit(bb);
                        if (fr)
                        {
                            //记录到数据库
                            appSittingSet.recorderDb(bb);
                            bb.msg = "恭喜您，您申请的<" + bb.aname + ">已通过活动专员的检验 R";
                            bool b8 = platACT.confirmAct(bb);
                        }
                        else
                        {
                            //充钱失败的情况 ？ 不做处理 下一次处理
                            continue;
                        }
                    }

                    string msg1 = string.Format("用户{0}处理完毕，注单号{1}，处理为 {2}，回复消息 {3}", bb.username, bb.betno, bb.passed ? "通过" : "不通过", bb.msg);
                    MyWrite(msg1);
                    appSittingSet.Log(msg1);

                }

            }
        }

        /// <summary>
        /// 作业 处理注单 活动2 幸运尾数
        /// </summary>
        [DisallowConcurrentExecution]
        public class MyJob3 : IJob
        {
            public void Execute(IJobExecutionContext context)
            {
                //获取注单号码  yh
                List<betData> list = platACT.getActData(actInfo[3]);
                if (list == null)
                {
                    MyWrite(actInfo[4] + " 没有获取到注单，等待下次执行 ");
                    return;
                }
                if (list.Count == 0)
                {
                    MyWrite(actInfo[4] + "没有新的注单，等待下次执行 ");
                    return;
                }


                //获取注单详情
                foreach (var item in list)
                {
                    //0 注单号不合法 驳回
                    if (item.betno != "" && Regex.IsMatch(item.betno, @"^\d{12}$"))
                    {
                        //字后一位是否含有幸运数字 str=str.Substring(str.Length-i)
                        if (!item.betno.EndsWith(sLuckNum[sLuckNum.Length-1].Split('@')[0]))
                        {
                            //回填失败
                            item.passed = false;
                            item.msg = "注单号不含活动尾数数字，请提供正确的注单号 R";
                            bool b = platACT.confirmAct(item);
                            if (b)
                            {
                                string msg = string.Format("用户{0}处理完毕，注单号{1}，处理为 {2}，回复消息 {3}", item.username, item.betno, item.passed ? "通过" : "不通过", item.msg);
                                MyWrite(msg);
                                appSittingSet.Log(msg);
                            }
                            continue;
                        }


                        //获取注单信息 是否存在 
                        betData bb = platBB.getBet_Details(item);
                        if (bb == null)
                        {
                            continue;
                        }

                        bb.aid = actInfo[3];
                        bb.aname = actInfo[4];
                        //查询不到信息 注单号不存在 情况 网站维护
                        if (!bb.passed)
                        {

                            bool b = platACT.confirmAct(bb);
                            if (b)
                            {
                                string msg = string.Format("用户{0}处理完毕，注单号{1}，处理为 {2}，回复消息 {3}", bb.username, bb.betno, bb.passed ? "通过" : "不通过", bb.msg);
                                MyWrite(msg);
                                appSittingSet.Log(msg);
                            }
                            continue;
                        }

                        //判断 美东时间
                        DateTime bt = Convert.ToDateTime(bb.betTime).Date;
                        DateTime now_d = DateTime.Now.AddHours(-12).Date;
                        if (bt != now_d)
                        {
                            bb.passed = false;
                            bb.msg = "您好，非美东时间当天注单，申请不通过！R";
                            bool b = platACT.confirmAct(bb);
                            if (b)
                            {
                                string msg = string.Format("用户{0}处理完毕，注单号{1}，处理为 {2}，回复消息 {3}", bb.username, bb.betno, bb.passed ? "通过" : "不通过", bb.msg);
                                MyWrite(msg);
                                appSittingSet.Log(msg);
                            }
                            continue;
                        }

                        //判断 游戏 名称
                        bool flag2 = false;
                        foreach (var g in gameNames[1].Split('@'))
                        {
                            if (g == bb.gamename)
                            {
                                flag2 = true;
                                break;
                            }
                        }
                        if (!flag2)
                        {
                            bb.passed = false;
                            bb.msg = string.Format("此活动仅限{0}游戏 R", gameNames[1].Replace("@", ","));
                            bool b = platACT.confirmAct(bb);
                            if (b)
                            {
                                string msg = string.Format("用户{0}处理完毕，注单号{1}，处理为 {2}，回复消息 {3}", bb.username, bb.betno, bb.passed ? "通过" : "不通过", bb.msg);
                                MyWrite(msg);
                                appSittingSet.Log(msg);
                            }
                            continue;
                        }

                        //免费游戏直接驳回 不通过
                        if (bb.betMoney == 0)
                        {
                            bb.passed = false;
                            bb.msg = "你好，免费游戏注单不享受此活动！R";
                            bool b = platACT.confirmAct(bb);
                            if (b)
                            {
                                string msg = string.Format("用户{0}处理完毕，注单号{1}，处理为 {2}，回复消息 {3}", bb.username, bb.betno, bb.passed ? "通过" : "不通过", bb.msg);
                                MyWrite(msg);
                                appSittingSet.Log(msg);
                            }
                            continue;
                        }

                        //判断是否提交过 同一用户 所有游戏 一天只能一次
                        string sql = "select * from record where (betno='" + bb.betno + "' and pass=1 and aid =" + bb.aid + ") or ( pass=1 and aid =" + bb.aid + " and username='" + bb.username + "'   and subminttime > '" + DateTime.Now.AddHours(-12).ToString("yyyy-MM-dd") + " 00:00:01' and  subminttime < '" + DateTime.Now.AddHours(-12).ToString("yyyy-MM-dd") + " 23:59:59') ";
                        if (appSittingSet.recorderDbCheck(sql))
                        {
                            bb.passed = false;
                            bb.msg = "您好，同一游戏一天内只能申请一次，申请不通过！R";
                            bool b = platACT.confirmAct(bb);
                            if (b)
                            {
                                string msg = string.Format("用户{0}处理完毕，注单号{1}，处理为 {2}，回复消息 {3}", bb.username, bb.betno, bb.passed ? "通过" : "不通过", bb.msg);
                                MyWrite(msg);
                                appSittingSet.Log(msg);
                            }
                            continue;
                        }

                        //判断注单是否是用户的 检查钱包 会员组别
                        bb = platGPK.checkInGPK(bb);
                        if (bb == null)
                        {
                            continue;
                        }
                        else
                        {
                            if (!bb.passed)
                            {
                                bb.msg = "经查询，您的账号或者钱包不正确！ R";
                                bool b6 = platACT.confirmAct(bb);
                                if (b6)
                                {
                                    string msg = string.Format("用户{0}处理完毕，注单号{1}，处理为 {2}，回复消息 {3}", bb.username, bb.betno, bb.passed ? "通过" : "不通过", bb.msg);
                                    MyWrite(msg);
                                    appSittingSet.Log(msg);
                                    continue;
                                }
                            }
                        }

                        //判断 层级是否在 列表之中
                        foreach (var s in FiliterGroups)
                        {
                            if (bb.level == s)
                            {
                                bb.passed = false;
                                bb.msg = "经查询，您的账号目前不享有此优惠！ R";
                                break;
                            }
                        }
                        if (!bb.passed)
                        {
                            //回填失败
                            bool b6 = platACT.confirmAct(bb);
                            if (b6)
                            {
                                string msg = string.Format("用户{0}处理完毕，注单号{1}，处理为 {2}，回复消息 {3}", bb.username, bb.betno, bb.passed ? "通过" : "不通过", bb.msg);
                                MyWrite(msg);
                                appSittingSet.Log(msg);
                            }
                            continue;
                        }


                        //计算 需要加钱 的数字 
                        foreach (string s in sLuckNum)
                        {
                            if (bb.betno.EndsWith(s.Split('@')[0]))
                            {
                                bb.betMoney *= decimal.Parse(s.Split('@')[1]);
                                if (bb.betMoney> decimal.Parse(s.Split('@')[2]))
                                {
                                    bb.betMoney = decimal.Parse(s.Split('@')[2]);
                                }
                                break;
                            }
                        }

                        /*
                        for (int i = sLuckNum.Length / 3; i >= 0; i--)
                        {
                            if (bb.betno.Substring(bb.betno.Length - i) == sLuckNum[(i - 1) * 3])
                            {
                                bb.betMoney *= decimal.Parse(sLuckNum[(i - 1) * 3 + 1]);
                                if (bb.betMoney > decimal.Parse(sLuckNum[(i - 1) * 3 + 2]))
                                {
                                    bb.betMoney = decimal.Parse(sLuckNum[(i - 1) * 3 + 2]);
                                }
                                break;
                            }
                        }
                        */

                        //大于max 不处理
                        if (bb.betMoney > maxValue)
                        {
                            bb.passed = false;
                            bb.msg = "无法通过，请联系客服处理 R";
                            bool b = platACT.confirmAct(bb);
                            if (b)
                            {
                                string msg = string.Format("用户{0}处理完毕，注单号{1}，处理为 {2}，回复消息 {3}", bb.username, bb.betno, bb.passed ? "通过" : "不通过", bb.msg);
                                MyWrite(msg);
                                appSittingSet.Log(msg);
                            }
                            continue;
                        }

                        if (bb.passed)
                        {
                            //加钱  提交数据
                            bb.AuditType = "Discount";
                            bb.Audit = bb.betMoney;
                            bb.Memo = bb.aname;
                            bb.Type = 5;

                            bool fr = platGPK.MemberDepositSubmit(bb);
                            if (fr)
                            {
                                string msg = string.Format("用户{0}处理，存入金额{1}，注单号{2}，活动名称{3}", bb.username, bb.betMoney, bb.betno, bb.aname);
                                MyWrite(msg);
                                appSittingSet.Log(msg);
                                //记录到sqlite数据库
                                appSittingSet.recorderDb(bb);
                            }
                            else
                            {
                                //充钱失败的情况 ？
                                continue;
                            }

                            //回填
                            bb.msg = "恭喜您，您申请的<" + bb.aname + ">已通过活动专员的检验";
                            bool r = platACT.confirmAct(bb);
                            if (r)
                            {
                                string msg = string.Format("用户{0}处理完毕，注单号{1}，处理为 {2}，回复消息 {3}", bb.username, bb.betno, bb.passed ? "通过" : "不通过", bb.msg);
                                MyWrite(msg);
                                appSittingSet.Log(msg);
                            }
                        }

                    }
                    else
                    {
                        //回填失败
                        item.passed = false;
                        item.msg = "请提供正确的注单号 R";
                        bool b = platACT.confirmAct(item);
                        if (b)
                        {
                            string msg = string.Format("用户{0}处理完毕，注单号{1}，处理为 {2}，回复消息 {3}", item.username, item.betno, item.passed ? "通过" : "不通过", item.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                            continue;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 作业4 活动 以小博大 
        /// </summary>
        [DisallowConcurrentExecution]
        public class MyJob4 : IJob
        {
            public void Execute(IJobExecutionContext context)
            {
                //获取注单号码  yh
                List<betData> list = platACT.getActData2(actInfo[6]);
                if (list == null)
                {
                    MyWrite(actInfo[7] + " 没有获取到新的注单，等待下次执行 ");
                    return;
                }
                if (list.Count == 0)
                {
                    MyWrite(actInfo[7] + "没有新的注单，等待下次执行 ");
                    return;
                }

                foreach (var item in list)
                {
                    
                    //获取上一次的时间
                    item.aid = actInfo[6];
                    betData bb = platACT.getActData2_time(item);
                    if (bb == null)
                    {
                        return;
                    }
                    bb.aid = actInfo[6];
                    bb.aname= actInfo[7];
                    //判断是否提交过 同一用户 所有游戏 一天只能一次
                    string sql = "select * from record where pass=1 and aid =" +bb.aid + " and LOWER(username)='" + bb.username.ToLower() + "'   and subminttime > '" + DateTime.Now.AddHours(-12).ToString("yyyy-MM-dd") + " 00:00:01' and  subminttime < '" + DateTime.Now.AddHours(-12).ToString("yyyy-MM-dd") + " 23:59:59' ";
                    if (appSittingSet.recorderDbCheck(sql))
                    {
                        bb.passed = false;
                        bb.msg = "您好，同一账号一天内只能申请一次，申请不通过！R";
                        bool b = platACT.confirmAct(bb);
                        if (b)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", bb.username, bb.passed ? "通过" : "不通过", bb.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        return;
                    }

                    //查询gpk 记录 级别 存款金额、总次数 账户 ？？余额?? 最后存款时间
                    bb = platGPK.MemberTransactionSearch(bb);
                    if (bb == null)
                    {
                        return;
                    }
                    if (!bb.passed)
                    {
                        //账号不存在？
                        bb.msg = "经查询，您的账号有误！或者此时间段无交易记录 R";
                        bool r1 = platACT.confirmAct(bb);
                        if (r1)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", bb.username, bb.passed ? "通过" : "不通过", bb.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        return;
                    }


                    //判断 层级是否在 列表之中
                    foreach (var s in FiliterGroups)
                    {
                        if (bb.level == s)
                        {
                            bb.passed = false;
                            bb.msg = "经查询，您的账号目前不享有此优惠！ R";
                            break;
                        }
                    }
                    if (!bb.passed)
                    {
                        //不满足条件 级别
                        bool r1 = platACT.confirmAct(bb);
                        if (r1)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", bb.username, bb.passed ? "通过" : "不通过", bb.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        return;
                    }


                    //计算 活动赠送金额
                    if ((bb.betMoney >= Act4Stander[4] && bb.betMoney <= Act4Stander[4] + 1) && (bb.total_money >= Act4Stander[7] && bb.betTimes >= Act4Stander[6]))
                    {
                        //满足6次 200 
                        bb.betMoney = Act4Stander[5];
                        bb.passed = true;
                    }
                    else if ((bb.betMoney >= Act4Stander[0] && bb.betMoney <= Act4Stander[0] + 1) && (bb.total_money >= Act4Stander[3] && bb.betTimes >= Act4Stander[2]))
                    {
                        //10的 满足 3次 100
                        bb.betMoney = Act4Stander[1];
                        bb.passed = true;
                    }
                    else
                    {
                        bb.passed = false;
                        bb.msg = "您的 存款次数、金额 没有达到活动要求 R";
                        //不满足条件 活动要求
                        bool r1 = platACT.confirmAct(bb);
                        if (r1)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", bb.username, bb.passed ? "通过" : "不通过", bb.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        return;
                    }

                    //判断是否有进行投注 起始时间 要加上 处理的时间间隔 
                    object ba = platGPK.BetRecordSearch(bb);
                    if (ba == null)
                    {
                        return;//查询失败，什么都不干
                    }
                    if (!(bool)ba)
                    {
                        bb.passed = false;
                        bb.msg = "申请期间账户余额不能有任何变动！R";
                        bool b = platACT.confirmAct(bb);
                        if (b)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", bb.username, bb.passed ? "通过" : "不通过", bb.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        return;
                    }





                    //三天前只能投注电子的 2019年5月13日 Timo
                    List<string> _list = platGPK.KindCategories;
                    foreach (var s in KindCategories)
                    {
                        _list.RemoveAt(int.Parse(s));
                    }
                    bb.gamename = "";
                    foreach (var _s in _list)
                    {
                        bb.gamename += "&types=" + string.Join("&types=", _s.Replace("\"", "").Split(',').Skip(1).ToArray());
                    }

                    bb.lastCashTime = DateTime.Now.Date.AddDays(-4).ToString("yyyy/MM/dd");
                    bb.lastOprTime = DateTime.Now.Date.AddDays(-2).ToString("yyyy/MM/dd");

                    bb = platGPK.GetDetailInfo(bb);
                    if (bb == null)
                    {
                        return;//查询失败，什么都不干
                    }
                    if (!bb.passed || bb.total_money >0)
                    {
                        bb.passed = false;
                        bb.msg = "近3天有效投注必须为电子！R";
                        bool b = platACT.confirmAct(bb);
                        if (b)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", bb.username, bb.passed ? "通过" : "不通过", bb.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        return;
                    }


                    //更新一下 钱包 ？？OK
                    bool b000 = platGPK.autoUpadateMemberAcc(bb.Id);

                    //获取详细信息
                    Gpk_UserDetail userinfo = platGPK.GetUserDetail(bb.username);

                    if ((userinfo.Wallet>= Act4Stander[0] && userinfo.Wallet <= (Act4Stander[0] + 5)) || (userinfo.Wallet >= Act4Stander[4] && userinfo.Wallet <= (Act4Stander[4] + 5)))
                    {

                    }
                    else
                    {
                        bb.passed = false;
                        bb.msg = "经查询，您的账号余额不符合要求！ R";
                        bool r1 = platACT.confirmAct(bb);
                        if (r1)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", bb.username, bb.passed ? "通过" : "不通过", bb.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        return;
                    }

                    //更新操作 加钱
                    //bb.aname = actInfo[7];
                    //bb.aid = actInfo[6];
                    bb.AuditType = "None";
                    bb.Audit = bb.betMoney;
                    bb.Memo = bb.aname;
                    bb.Type = 5;

                    bool fr = platGPK.MemberDepositSubmit(bb);
                    if (fr)
                    {
                        string msg = string.Format("用户{0}处理，存入金额{1} ,活动名称{2} ", bb.username, bb.betMoney, bb.aname);
                        MyWrite(msg);
                        appSittingSet.Log(msg);

                        //记录到sqlite数据库
                        appSittingSet.recorderDb(bb);
                    }
                    else
                    {
                        //充钱失败的情况 ？
                        return;
                    }

                    //更新等级
                    userinfo.Province = memberLevel[1];
                    userinfo.RegisterDevice = memberLevel[0];
                    platGPK.UpadateMemberLevel(userinfo);                        

                    //回填 操作结果
                    bb.msg = "恭喜您，您申请的<" + bb.aname + ">已通过活动专员的检验";
                    bool r = platACT.confirmAct(bb);
                    if (r)
                    {
                        string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", bb.username, bb.passed ? "通过" : "不通过", bb.msg);
                        MyWrite(msg);
                        appSittingSet.Log(msg);
                    }

                }

            }
        }

        /// <summary>
        /// 作业5 活动 首存17送20
        /// </summary>
        [DisallowConcurrentExecution]
        public class MyJob5 : IJob
        {
            public void Execute(IJobExecutionContext context)
            {
                List<betData> list = platACT.getActData(actInfo[9]);
                if (list == null)
                {
                    MyWrite(actInfo[10] + " 没有获取到新的注单，等待下次执行 ");
                    return;
                }
                if (list.Count == 0)
                {
                    MyWrite(actInfo[10] + "没有新的注单，等待下次执行 ");
                    return;
                }

                foreach (var item in list)
                {
                    //记录一个list 去除重复后 的第9，10 条 直接拒绝掉

                    if (Prob[1] == "1")
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

                        if (list_temp.Count > (10 - int.Parse(Prob[0])))
                        {
                            //直接拒绝
                            item.passed = false;
                            item.msg = "RR 同IP其他会员已申请过";
                            bool r1 = platACT.confirmAct(item);
                            if (r1)
                            {
                                string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                                MyWrite(msg);
                                appSittingSet.Log(msg);
                            }
                            continue;
                        }
                    }

                    //判断是否提交过 同一用户 所有游戏 一天只能一次
                    string sql = "select * from record where pass=1 and aid =" + actInfo[9] + " and LOWER(username)='" + item.username.ToLower() + "'   and subminttime > '" + DateTime.Now.AddHours(-12).ToString("yyyy-MM-dd") + " 00:00:01' and  subminttime < '" + DateTime.Now.AddHours(-12).ToString("yyyy-MM-dd") + " 23:59:59' ";
                    if (appSittingSet.recorderDbCheck(sql))
                    {
                        item.passed = false;
                        item.msg = "您好，同一账号一天内只能申请一次，申请不通过！R";
                        bool b = platACT.confirmAct(item);
                        if (b)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        continue;
                    }

                    //固定存款17-18元 存款次数1次
                    betData bb = platGPK.MemberTransactionSearch(item);

                    if (bb == null)
                    {
                        continue;
                    }
                    bb.aid = actInfo[9];
                    bb.aname = actInfo[10];

                    if (!bb.passed)
                    {
                        //账号不存在？
                        bb.msg = "经查询，您的账号有误！或者此时间段无交易记录 R";
                        bool r1 = platACT.confirmAct(bb);
                        if (r1)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", bb.username, bb.passed ? "通过" : "不通过", bb.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        continue;
                    }
                    if (bb.betTimes == 1 && (bb.betMoney >= Act4Set[0] && bb.betMoney < Act4Set[0] + 1))
                    {
                        bb.betMoney = Act4Set[1];//20元
                        bb.passed = true;
                    }
                    else
                    {
                        bb.passed = false;
                        bb.msg = "此活动仅限首次存款固定金额" + Act4Set[0] + "元！ R";
                        bool r1 = platACT.confirmAct(bb);
                        if (r1)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", bb.username, bb.passed ? "通过" : "不通过", bb.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        continue;
                    }


                    //获取详细信息
                    Gpk_UserDetail userinfo = platGPK.GetUserDetail(bb.username);
                    /*
                    //手机号是否匹配 12点44分 2019年4月2日
                    if (userinfo.Mobile.Trim() != item.PortalMemo.Trim())
                    {
                        //回填拒绝
                        item.passed = false;
                        item.msg = "您申请填入的手机号资料与注册资料中的手机号不匹配";
                        bool r1 = platACT.confirmAct(item);
                        if (r1)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        //更新等级 拒绝
                        userinfo.Province = memberLevel[7];
                        userinfo.RegisterDevice = memberLevel[6];
                        platGPK.UpadateMemberLevel(userinfo);
                        continue;
                    }

                    */
                    //历史记录 注册 和绑定银行卡的时间间隔 大于1分钟 机器注册
                    string sr = platGPK.GetUserLoadHistory(userinfo, "申请加入会员,建立银行帐户资讯", new Random().Next(Act4Set[3], Act4Set[4]));
                    if (sr.Contains("同IP其他") || sr.Contains("未绑定银行卡"))
                    {
                        bb.passed = false;
                        bb.msg = sr;
                        bool r1 = platACT.confirmAct(bb);
                        if (r1)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", bb.username, bb.passed ? "通过" : "不通过", bb.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        //更新等级
                        userinfo.Province = memberLevel[7];
                        userinfo.RegisterDevice = memberLevel[6];
                        platGPK.UpadateMemberLevel(userinfo);
                        continue;
                    }

                    //同ip几个人申请 5
                    int cu = platGPK.GetUserCountSameIP(userinfo.LatestLogin_IP);
                    if (cu >= Act4Set[2])
                    {
                        bb.passed = false;
                        bb.msg = "同IP其他会员已申请过！R";
                        bool r1 = platACT.confirmAct(bb);
                        if (r1)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", bb.username, bb.passed ? "通过" : "不通过", bb.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        //更新等级
                        userinfo.Province = memberLevel[7];
                        userinfo.RegisterDevice = memberLevel[6];
                        platGPK.UpadateMemberLevel(userinfo);
                        continue;
                    }

                    //更新操作 加钱
                    bb.AuditType = "None";
                    bb.Audit = bb.betMoney;
                    bb.Memo = bb.aname;
                    bb.Type = 5;

                    bool fr = platGPK.MemberDepositSubmit(bb);
                    if (fr)
                    {
                        string msg = string.Format("用户{0}处理，存入金额{1} ,活动名称{2} ", bb.username, bb.betMoney, bb.aname);
                        MyWrite(msg);
                        appSittingSet.Log(msg);

                        //记录到sqlite数据库
                        appSittingSet.recorderDb(bb);
                    }
                    else
                    {
                        //充钱失败的情况 ？
                        continue;
                    }

                    //更新等级
                    userinfo.Province = memberLevel[3];
                    userinfo.RegisterDevice = memberLevel[2];
                    platGPK.UpadateMemberLevel(userinfo);

                    //发送站内信 14点09分 2019年4月2日
                    if (mailbody[0]=="1")
                    {
                        SendMailBody mail = new SendMailBody() {Subject = mailbody[1], MailBody = mailbody[2],  SendMailType = "1", MailRecievers = userinfo.Account };
                        platGPK.SiteMailSendMail(mail);
                    }

                    //回填 操作结果
                    bb.msg = "恭喜您，您申请的<" + bb.aname + ">已通过活动专员的检验 R";
                    bool r = platACT.confirmAct(bb);
                    if (r)
                    {
                        string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", bb.username, bb.passed ? "通过" : "不通过", bb.msg);
                        MyWrite(msg);
                        appSittingSet.Log(msg);
                    }

                }

            }
        }

        /// <summary>
        /// 作业6  体验金   送28
        /// </summary>
        [DisallowConcurrentExecution]
        public class MyJob6 : IJob
        {
            public void Execute(IJobExecutionContext context)
            {
                List<betData> list = platACT2.getActData();
                if (list == null)
                {
                    MyWrite(actInfo[13] + " 没有获取到新的注单，等待下次执行 ");
                    return;
                }
                if (list.Count == 0)
                {
                    MyWrite(actInfo[13] + "没有新的注单，等待下次执行 ");
                    return;
                }

                foreach (var item in list)
                {
                    //记录一个list 去除重复后 的第9，10 条 直接拒绝掉

                    if (Prob[3] == "1")
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

                        if (list_temp.Count > (10 - int.Parse(Prob[2])))
                        {
                            //直接拒绝
                            item.passed = false;
                            item.msg = "RR 同IP其他会员已申请过";
                            bool r1 = platACT2.confirmAct(item);
                            if (r1)
                            {
                                string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                                MyWrite(msg);
                                appSittingSet.Log(msg);
                            }
                            continue;
                        }

                    }

                    //判断是否提交过 同一用户  只能一次
                    string sql = "select * from record where pass=1 and aid =" + actInfo[12] + " and LOWER(username)='" + item.username.ToLower() + "'  ";
                    if (appSittingSet.recorderDbCheck(sql))
                    {
                        item.passed = false;
                        item.msg = "您好，同一账号只能申请一次，申请不通过！R";
                        bool b = platACT2.confirmAct(item);
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
                        //回填拒绝
                        item.passed = false;
                        item.msg = "账号有误";
                        bool r1 = platACT2.confirmAct(item);
                        if (r1)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        //更新等级 拒绝
                        userinfo.Province = memberLevel[9];
                        userinfo.RegisterDevice = memberLevel[8];
                        platGPK.UpadateMemberLevel(userinfo);
                        continue;
                    }

                    //手机号是否匹配 12点44分 2019年4月2日
                    if (userinfo.Mobile!=item.PortalMemo)
                    {
                        //回填拒绝
                        item.passed = false;
                        item.msg = "您申请填入的手机号资料与注册资料中的手机号不匹配";
                        bool r1 = platACT2.confirmAct(item);
                        if (r1)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        //更新等级 拒绝
                        userinfo.Province = memberLevel[9];
                        userinfo.RegisterDevice = memberLevel[8];
                        platGPK.UpadateMemberLevel(userinfo);
                        continue;
                    }

                    //判断是否有存款记录 added by shine 2019年1月26日 15点48分
                    betData bb = platGPK.MemberTransactionSearch(item);
                    if (bb == null)
                    {
                        continue;//异常
                    }
                    if (bb.betTimes > 0 || bb.betMoney > 0)
                    {
                        //不过
                        item.passed = false;
                        item.msg = "有过存款记录，不符合条件";
                        bool r1 = platACT2.confirmAct(item);
                        if (r1)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        //更新等级 拒绝
                        userinfo.Province = memberLevel[9];
                        userinfo.RegisterDevice = memberLevel[8];
                        platGPK.UpadateMemberLevel(userinfo);
                        continue;
                    }

                    //历史记录 注册 和绑定银行卡的时间间隔 大于1分钟 机器注册
                    string sr = platGPK.GetUserLoadHistory(userinfo, "申请加入会员,建立银行帐户资讯", new Random().Next(Act4Set[8], Act4Set[9]));
                    if (sr.Contains("ERR"))
                    {
                        continue;
                    }
                    if (sr.Contains("同IP其他") || sr.Contains("未绑定"))
                    {
                        item.passed = false;
                        item.msg = sr;
                        bool r1 = platACT2.confirmAct(item);
                        if (r1)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        //更新等级 拒绝
                        userinfo.Province = memberLevel[9];
                        userinfo.RegisterDevice = memberLevel[8];
                        platGPK.UpadateMemberLevel(userinfo);
                        continue;
                    }

                    //同ip几个人申请 5
                    int cu = platGPK.GetUserCountSameIP(userinfo.LatestLogin_IP);
                    if (cu >= Act4Set[7])
                    {
                        item.passed = false;
                        item.msg = "同IP其他会员已申请过！R";
                        bool r1 = platACT2.confirmAct(item);
                        if (r1)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        //更新等级 拒绝
                        userinfo.Province = memberLevel[9];
                        userinfo.RegisterDevice = memberLevel[8];
                        platGPK.UpadateMemberLevel(userinfo);
                        continue;
                    }

                    //更新操作 加钱
                    item.betMoney = Act4Set[6];//先写死 不会更改
                    item.aid = actInfo[12];
                    item.aname = actInfo[13];
                    item.AuditType = "None";
                    item.Audit = bb.betMoney;
                    item.Memo = bb.aname;
                    item.Type = 5;
                    item.passed = true;
                    bool fr = platGPK.MemberDepositSubmit(item);
                    if (fr)
                    {
                        string msg = string.Format("用户{0}处理，存入金额{1} ,活动名称{2} ", item.username, item.betMoney, item.aname);
                        MyWrite(msg);
                        appSittingSet.Log(msg);

                        //记录到sqlite数据库
                        appSittingSet.recorderDb(item);
                    }
                    else
                    {
                        //充钱失败的情况 ？
                        continue;
                    }

                    //更新等级
                    userinfo.Province = memberLevel[5];
                    userinfo.RegisterDevice = memberLevel[4];
                    platGPK.UpadateMemberLevel(userinfo);

                    //开启 不可跨区登陆  2019年2月28日11点43分
                    userinfo.SexString = "true";
                    platGPK.UpdateCrossRegionLogin(userinfo);

                    //发送站内信 14点09分 2019年4月2日
                    if (mailbody[3] == "1")
                    {
                        SendMailBody mail = new SendMailBody() { Subject = mailbody[4], MailBody = mailbody[5], SendMailType = "1", MailRecievers = userinfo.Account };
                        platGPK.SiteMailSendMail(mail);
                    }

                    //回填 操作结果
                    item.msg = "恭喜您，您申请的<" + item.aname+ ">已通过活动专员的检验 R";
                    item.passed = true;
                    bool r = platACT2.confirmAct(item);
                    if (r)
                    {
                        string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                        MyWrite(msg);
                        appSittingSet.Log(msg);
                    }

                }

            }
        }

        /// <summary>
        /// 作业7 活动  笔笔救援
        /// </summary>
        [DisallowConcurrentExecution]
        public class MyJob7 : IJob
        {
            public void Execute(IJobExecutionContext context)
            {

                List<betData> list = platACT.getActData2(actInfo[15]);
                //list.Add(new betData() { username = "wlf5577558", betTime = "2018-12-29 21:50:59", bbid = "738215", passed = true, aid = "40" });//默认等于合格
                if (list == null)
                {
                    MyWrite(actInfo[16] + " 没有获取到新的注单，等待下次执行 ");
                    return;
                }
                if (list.Count == 0)
                {
                    MyWrite(actInfo[16] + "没有新的注单，等待下次执行 ");
                    return;
                }

                //如果最后一条的时间大于10分钟就重新连接
                DateTime dt = DateTime.Parse(list[list.Count - 1].betTime);
                if (platGPK.wsk == null)
                {
                    appSittingSet.Log("连接websocekt");
                    platGPK.WebSocketConnect();
                }
                else
                {
                    if (DateTime.Now.AddMinutes(10) > dt || list.Count > 5 || platGPK.wsk.ReadyState != WebSocketSharp.WebSocketState.Open)
                    {
                        appSittingSet.Log("连接websocekt");
                        platGPK.WebSocketConnect();
                    }
                }


                //if ((DateTime.Now.AddMinutes(10) > dt || list.Count > 5) && (platGPK.wsk != null && platGPK.wsk.ReadyState != WebSocketSharp.WebSocketState.Open))
                //{
                //    appSittingSet.Log("连接websocekt");
                //    platGPK.WebSocketConnect();       
                //}

                //2019年4月19日 如果大于10条 说明socket有问题，重连
                //if (list.Count > 10 || platGPK.wsk == null)
                //{
                //    appSittingSet.Log("连接websocekt");
                //    platGPK.WebSocketConnect();
                //}

                //方法2
                //if (list.Count > 10 || platGPK.cws==null)
                //{
                //    platGPK.WebSocketConnect2();
                //}

                foreach (var item in list)
                {
                    //先删除 表里面的socket 记录
                    appSittingSet.execSql("delete from record where aid=1002");

                    //记录一个list 去除重复后 的第9，10 条 直接拒绝掉

                    //if (Prob[1] == "1")
                    //{
                    //    //如果达到10条清除掉
                    //    if (list_temp.Count == 10)
                    //    {
                    //        list_temp.Clear();
                    //    }

                    //    if (!list_temp.Exists(x => x.bbid == item.bbid))
                    //    {
                    //        list_temp.Add(item);
                    //    }

                    //    if (list_temp.Count > (10 - int.Parse(Prob[0])))
                    //    {
                    //        //直接拒绝
                    //        item.passed = false;
                    //        item.msg = "RR 同IP其他会员已申请过";
                    //        bool r1 = platACT.confirmAct(item);
                    //        if (r1)
                    //        {
                    //            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                    //            MyWrite(msg);
                    //            appSittingSet.txtLog(msg);
                    //        }
                    //        continue;
                    //    }
                    //}



                    //固定存款>=50 
                    betData bb = platGPK.MemberTransactionSearch(item);
                    if (bb == null)
                    {
                        continue;
                    }
                    if (!bb.passed)
                    {
                        //账号不存在？
                        bb.msg = "经查询，您的账号有误！或者此时间段无交易记录 R";
                        bool r1 = platACT.confirmAct(bb);
                        if (r1)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", bb.username, bb.passed ? "通过" : "不通过", bb.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        continue;
                    }
                    if (bb.betMoney >= 50)
                    {
                        bb.betMoney = bb.betMoney * (decimal)0.05;
                        bb.passed = true;
                    }
                    else
                    {
                        bb.passed = false;
                        bb.msg = "仅限于单笔存款50元以上申请R";
                        bool r1 = platACT.confirmAct(bb);
                        if (r1)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", bb.username, bb.passed ? "通过" : "不通过", bb.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        continue;
                    }

                    //一天可以多次 不同的时间
                    string sql = "select * from record where pass=1 and aid =" + actInfo[15] + " and LOWER(username)='" + bb.username.ToLower() + "'   and msg = '" + bb.lastCashTime + "' ";
                    if (appSittingSet.recorderDbCheck(sql))
                    {
                        item.passed = false;
                        item.msg = "单笔存款仅限领取一次，此存款您已领取救援彩金了呢！R";
                        bool b = platACT.confirmAct(item);
                        if (b)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        continue;
                    }

                    //判断 美东时间
                    DateTime bt = Convert.ToDateTime(bb.lastCashTime).Date;
                    DateTime now_d = DateTime.Now.AddHours(-12).Date;
                    if (bt != now_d)
                    {
                        bb.passed = false;
                        bb.msg = "活动数据于美东时间00:00更新，仅计算当日的亏损！R";
                        bool b2 = platACT.confirmAct(bb);
                        if (b2)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", bb.username, bb.passed ? "通过" : "不通过", bb.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        continue;
                    }

                    //获取详细信息
                    Gpk_UserDetail userinfo = platGPK.GetUserDetail(bb.username);
                    //钱包<10元
                    if (userinfo.Wallet > 11)
                    {
                        bb.passed = false;
                        bb.msg = "请账户低于10元的时候立刻申请！R";
                        bool r1 = platACT.confirmAct(bb);
                        if (r1)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", bb.username, bb.passed ? "通过" : "不通过", bb.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        continue;
                    }

                    #region 接口关闭了
                    /*

                    //2次的投注、派彩是否相等 2019年4月19日 
                    SoketObjetRecordQuery sor1 = platGPK.BetRecordGetInfo(bb);//全部
                    foreach (var s in KindCategories)
                    {
                        bb.GameCategories += platGPK.KindCategories[int.Parse(s)] + ",";
                    }
                    bb.GameCategories = "["+  bb.GameCategories.TrimEnd(',') + "]";
                    SoketObjetRecordQuery sor2 = platGPK.BetRecordGetInfo(bb);//部分游戏
                    if (sor1.Count==0 || sor2.Count==0)
                    {
                        //没有记录 直接不过
                        bb.passed = false;
                        bb.msg = "派彩尚未完成，请等待派彩完毕后再申请！R";
                        bool b = platACT.confirmAct(bb);
                        if (b)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", bb.username, bb.passed ? "通过" : "不通过", bb.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        continue;
                    }

                    //对比两次结果是否一样
                    if (Math.Abs(sor1.Count -sor2.Count) <10 && Math.Abs(sor1.TotalBetAmount - sor2.TotalBetAmount) < 10 && Math.Abs(sor1.TotalPayoff - sor2.TotalPayoff) < 10 )
                    {
                    }
                    else
                    {
                        bb.passed = false;
                        bb.msg = "此活动仅计算电子游戏所产生的亏损以及投注数据！R";
                        bool b = platACT.confirmAct(bb);
                        if (b)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", bb.username, bb.passed ? "通过" : "不通过", bb.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        continue;
                    }

                    */
                    #endregion

                    //2次投注记录 有效投注 是否相等 同一个socket_id 取bbid
                    //platGPK.socket_id = DateTime.Now.Millisecond.ToString() + DateTime.Now.Second.ToString();
                    platGPK.socket_id = bb.bbid;
                    //启动sokect
                    //platGPK.SaveSocket2DB();

                    //if (platGPK.wsk == null)
                    //{
                    //    platGPK.WebSocketConnect();                        
                    //}


                    Thread.Sleep(500);
                    //发送一次 全部的查询 默认空为全部 soket 好像没有数据
                    //bb.GameCategories = "[" + platGPK.KindCategories[0] + "," + platGPK.KindCategories[1] + "," + platGPK.KindCategories[2] + "," + platGPK.KindCategories[3] + "," + platGPK.KindCategories[4] + "," + platGPK.KindCategories[5] + "]";
                    object o1 = platGPK.BetRecordSearch(bb);
                    Thread.Sleep(500);
                    //发送一次 电子的查询
                    bb.GameCategories = null;
                    foreach (var s in KindCategories)
                    {
                        bb.GameCategories += platGPK.KindCategories[int.Parse(s)] + ",";
                    }
                    bb.GameCategories = "[" + bb.GameCategories.TrimEnd(',') + "]";
                    //bb.GameCategories = "[" + platGPK.KindCategories[3] + "," + platGPK.KindCategories[5] + "]";
                    object o2 = platGPK.BetRecordSearch(bb);
                    //if (o1!=null && o2!=null)
                    //{
                    //    if ((bool)o1)
                    //    {
                    //        //所有的没有记录

                    //    }
                    //    if (!(bool)o1 && (bool)o2)
                    //    {
                    //        //所有的有 电子的没有
                    //    }
                    //}
                    if (o2 != null)
                    {
                        if ((bool)o2)
                        {
                            //没有电子的记录 直接不过
                            bb.passed = false;
                            bb.msg = "派彩尚未完成，请等待派彩完毕后再申请！R";
                            bool b = platACT.confirmAct(bb);
                            if (b)
                            {
                                string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", bb.username, bb.passed ? "通过" : "不通过", bb.msg);
                                MyWrite(msg);
                                appSittingSet.Log(msg);
                            }
                            continue;
                        }
                    }
                    Thread.Sleep(500);
                    //查询一次数据库 看是否符合
                    decimal chargeMoney = 0;
                    object o = platGPK.getSoketDataFromDbCompare(out chargeMoney);
                    if (o != null)
                    {
                        if (!(bool)o)
                        {
                            bb.passed = false;
                            bb.msg = "此活动仅计算电子游戏所产生的亏损以及投注数据！R";
                            bool b = platACT.confirmAct(bb);
                            if (b)
                            {
                                string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", bb.username, bb.passed ? "通过" : "不通过", bb.msg);
                                MyWrite(msg);
                                appSittingSet.Log(msg);
                            }
                            continue;
                        }
                    }
                    else
                    {
                        continue;
                    }

                    //总派彩金额和账户余额相差50以内
                    /*
                        if (Math.Abs( bb.subtotal -   chargeMoney)>50 )
                        {
                            bb.passed = false;
                            bb.msg = "账户存取款记录不符合要求！R";
                            bool b = platACT.confirmAct(bb);
                            if (b)
                            {
                                string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", bb.username, bb.passed ? "通过" : "不通过", bb.msg);
                                MyWrite(msg);
                                appSittingSet.Log(msg);
                            }
                            continue;
                        }

                    */


                    //bb= platGPK.BetRecordSearch(bb); //不能获取 统计部分的 有效投注和  派彩金额
                    //派彩是否大于 充值的金额-账户余额 大于才给

                    //如果有其他的投注 不给
                    //bb.GameCategories= "[\"BBINbbsport\",\"BBINlottery\",\"BBINvideo\",\"SabaSport\",\"SabaNumber\",\"SabaVirtualSport\",\"AgBr\",\"Mg2Real\",\"Pt2Real\",\"GpiReal\",\"SingSport\",\"AllBetReal\",\"IgLottery\",\"IgLotto\",\"Rg2Real\",\"Rg2Board\",\"Rg2Lottery\",\"Rg2Lottery2\",\"JdbBoard\",\"EvoReal\",\"BgReal\",\"GdReal\",\"Pt3Real\",\"SunbetReal\",\"CmdSport\",\"Sunbet2Real\",\"Mg3Real\",\"KgBoard\",\"LxLottery\",\"EBetReal\",\"ImEsport\",\"OgReal\",\"VrLottery\",\"City761Board\",\"FsBoard\",\"SaReal\",\"ImsSport\",\"IboSport\",\"NwBoard\",\"JsBoard\",\"ThBoard\"]";

                    //object o = platGPK.BetRecordSearch(bb);
                    //if (o==null)
                    //{
                    //    continue;
                    //}
                    //else
                    //{
                    //    if (!(bool)o)
                    //    {
                    //        bb.passed = false;
                    //        bb.msg = "此活动仅计算电子游戏所产生的亏损以及投注数据！R";
                    //        bool b = platACT.confirmAct(bb);
                    //        if (b)
                    //        {
                    //            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", bb.username, bb.passed ? "通过" : "不通过", bb.msg);
                    //            MyWrite(msg);
                    //            appSittingSet.txtLog(msg);
                    //        }
                    //        return;
                    //    }
                    //}

                    /*
                    //电子以外的不给  大于0
                    bb = platGPK.GetDetailInfo_withoutELE(bb);
                    if (bb.passed == false)
                    {
                        //跳过
                        continue;
                    }
                    if (bb.total_money > 0)
                    {
                        bb.passed = false;
                        bb.msg = "此活动仅计算电子游戏所产生的亏损以及投注数据！R";
                        bool r1 = platACT.confirmAct(bb);
                        if (r1)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", bb.username, bb.passed ? "通过" : "不通过", bb.msg);
                            MyWrite(msg);
                            appSittingSet.txtLog(msg);
                        }
                        continue;
                    }
                    */

                    //更新操作 加钱
                    bb.aname = actInfo[16];
                    bb.AuditType = "Discount";
                    bb.Audit = bb.betMoney * 5;
                    bb.Memo = bb.aname;
                    bb.Type = 5;
                    bb.aid = actInfo[15];
                    bool fr = platGPK.MemberDepositSubmit(bb);
                    if (fr)
                    {
                        string msg = string.Format("用户{0}处理，存入金额{1} ,活动名称{2} ", bb.username, bb.betMoney, bb.aname);
                        MyWrite(msg);
                        appSittingSet.Log(msg);

                        //记录到sqlite数据库
                        bb.msg = bb.lastCashTime;//用到了这个判断单笔时间
                        appSittingSet.recorderDb(bb);
                    }
                    else
                    {
                        //充钱失败的情况 ？
                        continue;
                    }

                    //更新等级
                    //userinfo.MemberLevelSettingId = memberLevel[2];
                    //platGPK.UpadateMemberLevel(memberLevel[3], userinfo);
                    //回填 操作结果
                    bb.msg = "恭喜您，您申请的<" + bb.aname + ">已通过活动专员的检验 R";
                    bool r = platACT.confirmAct(bb);
                    if (r)
                    {
                        string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", bb.username, bb.passed ? "通过" : "不通过", bb.msg);
                        MyWrite(msg);
                        appSittingSet.Log(msg);
                    }

                }

            }
        }
        /// <summary>
        /// 五一七天乐 登陆有礼
        /// </summary>
        [DisallowConcurrentExecution]
        public class MyJob8 : IJob
        {
            public void Execute(IJobExecutionContext context)
            {
                //DateTime dt = DateTime.Parse("2019/05/01 00:00:01");
                DateTime dt = DateTime.Parse("2019/05/18 00:00:01");
                List<betData> list = platACT.getActData2(actInfo[18]);
                if (list == null)
                {
                    MyWrite(actInfo[19] + " 没有获取到新的注单，等待下次执行 ");
                    return;
                }
                if (list.Count == 0)
                {
                    MyWrite(actInfo[19] + "没有新的注单，等待下次执行 ");
                    return;
                }

                foreach (var item in list)
                {
                    //item.aid = actInfo[18];
                    item.aname = actInfo[19];
                    //判断是否提交过 同一用户  只能一次
                    string sql = "select * from record where pass=1 and aid =" + item.aid+ " and LOWER(username)='" + item.username.ToLower() + "'  ";
                    if (appSittingSet.recorderDbCheck(sql))
                    {
                        item.passed = false;
                        item.msg = "您好，同一账号只能申请一次，申请不通过！R";
                        bool b = platACT.confirmAct(item);
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
                        //回填拒绝
                        item.passed = false;
                        item.msg = "账号有误";
                        bool r1 = platACT.confirmAct(item);
                        if (r1)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        //更新等级 拒绝
                        //userinfo.Province = memberLevel[9];
                        //userinfo.RegisterDevice = memberLevel[8];
                        //platGPK.UpadateMemberLevel(userinfo);
                        continue;
                    }
                    else
                    {
                        if (userinfo.JoinTime>dt.AddHours(-12))
                        {
                            //回填拒绝
                            item.passed = false;
                            item.msg = "注册时间必须要美东时间"+dt.ToString("yyyy/MM/dd HH:mm:ss") + "之前 R";
                            bool r1 = platACT.confirmAct(item);
                            if (r1)
                            {
                                string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                                MyWrite(msg);
                                appSittingSet.Log(msg);
                            }
                            //更新等级 拒绝
                            //userinfo.Province = memberLevel[9];
                            //userinfo.RegisterDevice = memberLevel[8];
                            //platGPK.UpadateMemberLevel(userinfo);
                            continue;
                        }
                        if (userinfo.BankAccount == null || userinfo.BankAccount=="")
                        {
                            //回填拒绝
                            item.passed = false;
                            item.msg = "未绑定银行卡 R";
                            bool r1 = platACT.confirmAct(item);
                            if (r1)
                            {
                                string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                                MyWrite(msg);
                                appSittingSet.Log(msg);
                            }
                            //更新等级 拒绝
                            //userinfo.Province = memberLevel[9];
                            //userinfo.RegisterDevice = memberLevel[8];
                            //platGPK.UpadateMemberLevel(userinfo);
                            continue;
                        }
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
                        //不满足条件 级别
                        bool r1 = platACT.confirmAct(item);
                        if (r1)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        return;
                    }

                    //判断是否有存款记录 5.1前>=1
                    item.lastOprTime = "";
                    item.betTime = dt.AddHours(12).ToString("yyyy/MM/dd HH:mm:ss");
                    betData bb = platGPK.MemberTransactionSearch(item);
                    if (bb == null)
                    {
                        continue;//异常
                    }
                    if (!bb.passed || bb.total_money<1)
                    {
                        //没有交易记录 存款小于1 不过
                        item.passed = false;
                        item.msg = dt.ToString("yyyy/MM/dd") +"前存款不足，不符合条件";
                        bool r1 = platACT.confirmAct(item);
                        if (r1)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        //更新等级 拒绝
                        //userinfo.Province = memberLevel[9];
                        //userinfo.RegisterDevice = memberLevel[8];
                        //platGPK.UpadateMemberLevel(userinfo);
                        continue;
                    }

                    /*
                    //历史记录 注册 和绑定银行卡的时间间隔 大于1分钟 机器注册
                    string sr = platGPK.GetUserLoadHistory(userinfo, "申请加入会员,建立银行帐户资讯", new Random().Next(Act4Set[8], Act4Set[9]));
                    if (sr.Contains("ERR"))
                    {
                        continue;
                    }
                    if (sr.Contains("同IP其他") || sr.Contains("未绑定"))
                    {
                        item.passed = false;
                        item.msg = sr;
                        bool r1 = platACT.confirmAct(item);
                        if (r1)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        //更新等级 拒绝
                        userinfo.Province = memberLevel[9];
                        userinfo.RegisterDevice = memberLevel[8];
                        platGPK.UpadateMemberLevel(userinfo);
                        continue;
                    }

                    //同ip几个人申请 5
                    int cu = platGPK.GetUserCountSameIP(userinfo.LatestLogin_IP);
                    if (cu >= Act4Set[7])
                    {
                        item.passed = false;
                        item.msg = "同IP其他会员已申请过！R";
                        bool r1 = platACT.confirmAct(item);
                        if (r1)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        //更新等级 拒绝
                        userinfo.Province = memberLevel[9];
                        userinfo.RegisterDevice = memberLevel[8];
                        platGPK.UpadateMemberLevel(userinfo);
                        continue;
                    }
                    */
                    //更新操作 加钱
                    item.betMoney = 18;//先写死 不会更改
                    item.AuditType = "None";
                    item.Audit = bb.betMoney;
                    item.Memo = bb.aname;
                    item.Type = 5;
                    item.passed = true;
                    bool fr = platGPK.MemberDepositSubmit(item);
                    if (fr)
                    {
                        string msg = string.Format("用户{0}处理，存入金额{1} ,活动名称{2} ", item.username, item.betMoney, item.aname);
                        MyWrite(msg);
                        appSittingSet.Log(msg);

                        //记录到sqlite数据库
                        appSittingSet.recorderDb(item);
                    }
                    else
                    {
                        //充钱失败的情况 ？
                        continue;
                    }

                    //更新等级
                    //userinfo.Province = memberLevel[5];
                    //userinfo.RegisterDevice = memberLevel[4];
                    //platGPK.UpadateMemberLevel(userinfo);

                    //开启 不可跨区登陆  2019年2月28日11点43分
                    //userinfo.SexString = "true";
                    //platGPK.UpdateCrossRegionLogin(userinfo);

                    //发送站内信 14点09分 2019年4月2日
                    //if (mailbody[3] == "1")
                    //{
                    //    SendMailBody mail = new SendMailBody() { Subject = mailbody[4], MailBody = mailbody[5], SendMailType = "1", MailRecievers = userinfo.Account };
                    //    platGPK.SiteMailSendMail(mail);
                    //}

                    //回填 操作结果
                    item.msg = "恭喜您，您申请的<" + item.aname + ">已通过活动专员的检验 R";
                    item.passed = true;
                    bool r = platACT.confirmAct(item);
                    if (r)
                    {
                        string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                        MyWrite(msg);
                        appSittingSet.Log(msg);
                    }

                }

            }
        }

        /// <summary>
        /// 五一七天乐 存款即送
        /// </summary>
        [DisallowConcurrentExecution]
        public class MyJob9 : IJob
        {
            public void Execute(IJobExecutionContext context)
            {
                //DateTime dt = DateTime.Parse("2019/05/01 00:00:01");
                DateTime dt = DateTime.Parse("2019/05/18 00:00:01");
                List<betData> list = platACT.getActData2(actInfo[21]);
                //List<betData> list = new List<betData>();
                //list.Add(new betData() { username = "zr97987", bbid = "1470906", betTime = "2019-04-29 18:47", aid = actInfo[21] });
                //list.Add(new betData() { username = "mg1995", bbid = "1492880", betTime = "2019-05-01 20:50", aid = actInfo[21] });
                if (list == null)
                {
                    MyWrite(actInfo[22] + " 没有获取到新的注单，等待下次执行 ");
                    return;
                }
                if (list.Count == 0)
                {
                    MyWrite(actInfo[22] + "没有新的注单，等待下次执行 ");
                    return;
                }

                foreach (var item in list)
                {
                    item.aid = actInfo[21];
                    item.aname = actInfo[22];
                    //判断是否提交过 同一用户  只能一次
                    string sql = "select * from record where pass=1 and aid =" + item.aid+ " and LOWER(username)='" + item.username.ToLower() + "'  ";
                    if (appSittingSet.recorderDbCheck(sql))
                    {
                        item.passed = false;
                        item.msg = "您好，同一账号只能申请一次，申请不通过！R";
                        bool b = platACT.confirmAct(item);
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
                        //回填拒绝
                        item.passed = false;
                        item.msg = "账号有误";
                        bool r1 = platACT.confirmAct(item);
                        if (r1)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        //更新等级 拒绝
                        //userinfo.Province = memberLevel[9];
                        //userinfo.RegisterDevice = memberLevel[8];
                        //platGPK.UpadateMemberLevel(userinfo);
                        continue;
                    }
                    else
                    {
                        if (userinfo.JoinTime>dt.AddHours(-12))
                        {
                            //回填拒绝
                            item.passed = false;
                            item.msg = "注册时间必须要 "+dt.ToString("yyyy/MM/dd HH:mm:ss") + " 之前 R";
                            bool r1 = platACT.confirmAct(item);
                            if (r1)
                            {
                                string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                                MyWrite(msg);
                                appSittingSet.Log(msg);
                            }
                            //更新等级 拒绝
                            //userinfo.Province = memberLevel[9];
                            //userinfo.RegisterDevice = memberLevel[8];
                            //platGPK.UpadateMemberLevel(userinfo);
                            continue;
                        }
                        if (userinfo.BankAccount == null || userinfo.BankAccount=="")
                        {
                            //回填拒绝
                            item.passed = false;
                            item.msg = "未绑定银行卡 R";
                            bool r1 = platACT.confirmAct(item);
                            if (r1)
                            {
                                string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                                MyWrite(msg);
                                appSittingSet.Log(msg);
                            }
                            //更新等级 拒绝
                            //userinfo.Province = memberLevel[9];
                            //userinfo.RegisterDevice = memberLevel[8];
                            //platGPK.UpadateMemberLevel(userinfo);
                            continue;
                        }
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
                        //不满足条件 级别
                        bool r1 = platACT.confirmAct(item);
                        if (r1)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        return;
                    }



                    //这一次存款的数目 和时间
                    //查询gpk 记录 级别 存款金额、总次数 账户 ？？余额?? 最后存款时间 
                    item.lastOprTime = dt.AddHours(12).AddMinutes(-2).ToString("yyyy/MM/dd HH:mm:ss") ;
                    //item.betTime = "";
                    //item.isReal = false; //测试 正式取消掉 或者设置为true 默认true
                    betData bb= platGPK.MemberTransactionSearch(item);
                    if (bb == null)
                    {
                        continue;//异常
                    }
                    if (!bb.passed)
                    {
                        //账号不存在？
                        bb.msg = "经查询，您的账号此时间段无交易记录 R";
                        bool r1 = platACT.confirmAct(bb);
                        if (r1)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", bb.username, bb.passed ? "通过" : "不通过", bb.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        continue;
                    }

                    //计算送的钱
                    decimal hb = 0;
                    bool ok = false;
                    if (bb.betMoney>=10 )
                    {
                        hb= 10;
                        ok = true;
                        bb.Audit = (bb.betMoney + 10) * 5;
                    }
                    if (bb.betMoney >= 20 )
                    {
                        hb= 20;
                        ok = true;
                        bb.Audit = (bb.betMoney + 20) * 9;
                    }


                    //判断是否有进行投注 起始时间 要加上 处理的时间间隔  @@账户变动@@
                    item.lastOprTime = item.betTime;
                    object ba = platGPK.BetRecordSearch(item);
                    if (ba == null)
                    {
                        continue;//查询失败，什么都不干
                    }
                    if (!(bool)ba)
                    {
                        item.passed = false;
                        item.msg = "申请期间账户余额不能有任何变动！R";
                        bool b = platACT.confirmAct(item);
                        if (b)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        continue;
                    }




                    //判断是否有存款记录 5.1前>=1
                    item.lastOprTime = "";
                    item.betTime = dt.AddHours(12).ToString("yyyy/MM/dd HH:mm:ss");
                    bb = platGPK.MemberTransactionSearch(item);
                    if (bb == null)
                    {
                        continue;//异常
                    }
                    if (!bb.passed )
                    {
                        //没有记录 
                        if (ok)
                        {
                            //送 10
                            bb.betMoney = 10;
                            bb.passed = true;
                        }
                        else
                        {
                            //存款不符合要求
                            item.passed = false;
                            item.msg = "此次存款不符合条件 最低10元 R";
                            bool r1 = platACT.confirmAct(item);
                            if (r1)
                            {
                                string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                                MyWrite(msg);
                                appSittingSet.Log(msg);
                            }
                            //更新等级 拒绝
                            //userinfo.Province = memberLevel[9];
                            //userinfo.RegisterDevice = memberLevel[8];
                            //platGPK.UpadateMemberLevel(userinfo);
                            continue;
                        }
                    }
                    else
                    {
                        if (bb.total_money>=1)
                        {
                            //有记录>1 >20 就送
                            if (hb==20)
                            {
                                //送 20
                                bb.betMoney = 20;
                                bb.passed = true;
                            }
                            else
                            {
                                //存款不足 5.1号前有过存款记录，此次最低存20元才又送
                                item.passed = false;
                                item.msg = "此次需要最低存款20元才能参与活动，不符合条件 R";
                                bool r1 = platACT.confirmAct(item);
                                if (r1)
                                {
                                    string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                                    MyWrite(msg);
                                    appSittingSet.Log(msg);
                                }
                                //更新等级 拒绝
                                //userinfo.Province = memberLevel[9];
                                //userinfo.RegisterDevice = memberLevel[8];
                                //platGPK.UpadateMemberLevel(userinfo);
                                continue;
                            }
                        }
                        else
                        {
                            //有记录 不大于1元 不送
                            item.passed = false;
                            item.msg = dt.ToString("yyyy/MM/dd") +"前存款不足，不符合条件R";
                            bool r1 = platACT.confirmAct(item);
                            if (r1)
                            {
                                string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                                MyWrite(msg);
                                appSittingSet.Log(msg);
                            }
                            //更新等级 拒绝
                            //userinfo.Province = memberLevel[9];
                            //userinfo.RegisterDevice = memberLevel[8];
                            //platGPK.UpadateMemberLevel(userinfo);
                            continue;
                        }
                    }

                    /*
                    //历史记录 注册 和绑定银行卡的时间间隔 大于1分钟 机器注册
                    string sr = platGPK.GetUserLoadHistory(userinfo, "申请加入会员,建立银行帐户资讯", new Random().Next(Act4Set[8], Act4Set[9]));
                    if (sr.Contains("ERR"))
                    {
                        continue;
                    }
                    if (sr.Contains("同IP其他") || sr.Contains("未绑定"))
                    {
                        item.passed = false;
                        item.msg = sr;
                        bool r1 = platACT.confirmAct(item);
                        if (r1)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        //更新等级 拒绝
                        userinfo.Province = memberLevel[9];
                        userinfo.RegisterDevice = memberLevel[8];
                        platGPK.UpadateMemberLevel(userinfo);
                        continue;
                    }

                    //同ip几个人申请 5
                    int cu = platGPK.GetUserCountSameIP(userinfo.LatestLogin_IP);
                    if (cu >= Act4Set[7])
                    {
                        item.passed = false;
                        item.msg = "同IP其他会员已申请过！R";
                        bool r1 = platACT.confirmAct(item);
                        if (r1)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        //更新等级 拒绝
                        userinfo.Province = memberLevel[9];
                        userinfo.RegisterDevice = memberLevel[8];
                        platGPK.UpadateMemberLevel(userinfo);
                        continue;
                    }
                    */
                    //更新操作 加钱

                    bb.AuditType = "Discount";
                    //bb.Audit = bb.betMoney;
                    bb.Memo = bb.aname;
                    bb.Type = 5;

                    bool fr = platGPK.MemberDepositSubmit(item);
                    if (fr)
                    {
                        string msg = string.Format("用户{0}处理，存入金额{1} ,活动名称{2} ", item.username, item.betMoney, item.aname);
                        MyWrite(msg);
                        appSittingSet.Log(msg);

                        //记录到sqlite数据库
                        appSittingSet.recorderDb(item);
                    }
                    else
                    {
                        //充钱失败的情况 ？
                        continue;
                    }

                    //更新等级
                    //userinfo.Province = memberLevel[5];
                    //userinfo.RegisterDevice = memberLevel[4];
                    //platGPK.UpadateMemberLevel(userinfo);

                    //开启 不可跨区登陆  2019年2月28日11点43分
                    //userinfo.SexString = "true";
                    //platGPK.UpdateCrossRegionLogin(userinfo);

                    //发送站内信 14点09分 2019年4月2日
                    //if (mailbody[3] == "1")
                    //{
                    //    SendMailBody mail = new SendMailBody() { Subject = mailbody[4], MailBody = mailbody[5], SendMailType = "1", MailRecievers = userinfo.Account };
                    //    platGPK.SiteMailSendMail(mail);
                    //}

                    //回填 操作结果
                    item.msg = "恭喜您，您申请的<" + item.aname + ">已通过活动专员的检验 R";
                    item.passed = true;
                    bool r = platACT.confirmAct(item);
                    if (r)
                    {
                        string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                        MyWrite(msg);
                        appSittingSet.Log(msg);
                    }

                }

            }
        }


        /// <summary>
        /// 520礼惠五月 登陆有礼
        /// </summary>
        [DisallowConcurrentExecution]
        public class MyJob10 : IJob
        {
            public void Execute(IJobExecutionContext context)
            {
                //DateTime dt = DateTime.Parse("2019/05/01 00:00:01");
                DateTime dt = DateTime.Parse("2019/05/20 00:00:01");


                List<betData> list = platACT.getActData2(actInfo[18]);
                //_list = ActFromDB.getActData(actInfo[18],_list);


                if (list == null)
                {
                    MyWrite(actInfo[19] + " 没有获取到新的注单，等待下次执行 ");
                    return;
                }
                if (list.Count == 0)
                {
                    MyWrite(actInfo[19] + "没有新的注单，等待下次执行 ");
                    return;
                }

                MySQLHelper.connectionString = appSittingSet.readAppsettings("MySqlConnect").Split('|')[0];
                foreach (var item in list)
                {
                    //item.aid = actInfo[18];
                    item.aname = actInfo[19];
                    //判断是否之前提交过 5.1活动
                    string sql = $"select count(id) from e_submissions where aid= {item.aid}  and LOWER(username)='{item.username.ToLower()}' and status=1;";
                    object o = MySQLHelper.GetScalar(sql);
                    if (int.Parse(o.ToString()) > 1)
                    {
                        item.passed = false;
                        item.msg = "您好，同一账号只能申请一次，申请不通过！R";
                        bool b = platACT.confirmAct(item);
                        if (b)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        continue;
                    }

                    //判断是否提交过 同一用户  只能一次
                    sql = $"select * from record where pass=1 and aid ={item.aid } and LOWER(username)='{ item.username.ToLower() }'  ";
                    if (appSittingSet.recorderDbCheck(sql))
                    {
                        item.passed = false;
                        item.msg = "您好，同一账号只能申请一次，申请不通过！R";
                        bool b = platACT.confirmAct(item);
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
                        //回填拒绝
                        item.passed = false;
                        item.msg = "账号有误";
                        bool r1 = platACT.confirmAct(item);
                        if (r1)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        //更新等级 拒绝
                        //userinfo.Province = memberLevel[9];
                        //userinfo.RegisterDevice = memberLevel[8];
                        //platGPK.UpadateMemberLevel(userinfo);
                        continue;
                    }
                    else
                    {
                        if (userinfo.JoinTime>dt.AddHours(-12))
                        {
                            //回填拒绝
                            item.passed = false;
                            item.msg = "注册时间必须要美东时间"+dt.ToString("yyyy/MM/dd HH:mm:ss") + "之前 R";
                            bool r1 = platACT.confirmAct(item);
                            if (r1)
                            {
                                string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                                MyWrite(msg);
                                appSittingSet.Log(msg);
                            }
                            //更新等级 拒绝
                            //userinfo.Province = memberLevel[9];
                            //userinfo.RegisterDevice = memberLevel[8];
                            //platGPK.UpadateMemberLevel(userinfo);
                            continue;
                        }
                        if (userinfo.BankAccount == null || userinfo.BankAccount=="")
                        {
                            //回填拒绝
                            item.passed = false;
                            item.msg = "未绑定银行卡 R";
                            bool r1 = platACT.confirmAct(item);
                            if (r1)
                            {
                                string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                                MyWrite(msg);
                                appSittingSet.Log(msg);
                            }
                            //更新等级 拒绝
                            //userinfo.Province = memberLevel[9];
                            //userinfo.RegisterDevice = memberLevel[8];
                            //platGPK.UpadateMemberLevel(userinfo);
                            continue;
                        }
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
                        //不满足条件 级别
                        bool r1 = platACT.confirmAct(item);
                        if (r1)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        return;
                    }

                    //判断在xx之前 是否有存款记录 >=1
                    //item.lastOprTime = "";
                    item.betTime = dt.AddHours(12).ToString("yyyy/MM/dd HH:mm:ss");
                    betData bb = platGPK.MemberTransactionSearch(item);
                    if (bb == null)
                    {
                        continue;//异常
                    }
                    if (!bb.passed || bb.total_money<1)
                    {
                        //没有交易记录 存款小于1 不过
                        item.passed = false;
                        item.msg = dt.ToString("yyyy/MM/dd") +"前存款不足，不符合条件";
                        bool r1 = platACT.confirmAct(item);
                        if (r1)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        //更新等级 拒绝
                        //userinfo.Province = memberLevel[9];
                        //userinfo.RegisterDevice = memberLevel[8];
                        //platGPK.UpadateMemberLevel(userinfo);
                        continue;
                    }

                    /*
                    //历史记录 注册 和绑定银行卡的时间间隔 大于1分钟 机器注册
                    string sr = platGPK.GetUserLoadHistory(userinfo, "申请加入会员,建立银行帐户资讯", new Random().Next(Act4Set[8], Act4Set[9]));
                    if (sr.Contains("ERR"))
                    {
                        continue;
                    }
                    if (sr.Contains("同IP其他") || sr.Contains("未绑定"))
                    {
                        item.passed = false;
                        item.msg = sr;
                        bool r1 = platACT.confirmAct(item);
                        if (r1)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        //更新等级 拒绝
                        userinfo.Province = memberLevel[9];
                        userinfo.RegisterDevice = memberLevel[8];
                        platGPK.UpadateMemberLevel(userinfo);
                        continue;
                    }

                    //同ip几个人申请 5
                    int cu = platGPK.GetUserCountSameIP(userinfo.LatestLogin_IP);
                    if (cu >= Act4Set[7])
                    {
                        item.passed = false;
                        item.msg = "同IP其他会员已申请过！R";
                        bool r1 = platACT.confirmAct(item);
                        if (r1)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        //更新等级 拒绝
                        userinfo.Province = memberLevel[9];
                        userinfo.RegisterDevice = memberLevel[8];
                        platGPK.UpadateMemberLevel(userinfo);
                        continue;
                    }
                    */
                    //更新操作 加钱
                    item.betMoney = 5.2M;//先写死 不会更改
                    item.AuditType = "Discount";
                    item.Audit = bb.betMoney * 5;
                    item.Memo = bb.aname;
                    item.Type = 5;
                    item.passed = true;
                    bool fr = platGPK.MemberDepositSubmit(item);
                    if (fr)
                    {
                        string msg = string.Format("用户{0}处理，存入金额{1} ,活动名称{2} ", item.username, item.betMoney, item.aname);
                        MyWrite(msg);
                        appSittingSet.Log(msg);

                        //记录到sqlite数据库
                        appSittingSet.recorderDb(item);
                    }
                    else
                    {
                        //充钱失败的情况 ？
                        continue;
                    }

                    //更新等级
                    //userinfo.Province = memberLevel[5];
                    //userinfo.RegisterDevice = memberLevel[4];
                    //platGPK.UpadateMemberLevel(userinfo);

                    //开启 不可跨区登陆  2019年2月28日11点43分
                    //userinfo.SexString = "true";
                    //platGPK.UpdateCrossRegionLogin(userinfo);

                    //发送站内信 14点09分 2019年4月2日
                    //if (mailbody[3] == "1")
                    //{
                    //    SendMailBody mail = new SendMailBody() { Subject = mailbody[4], MailBody = mailbody[5], SendMailType = "1", MailRecievers = userinfo.Account };
                    //    platGPK.SiteMailSendMail(mail);
                    //}

                    //回填 操作结果
                    item.msg = "恭喜您，您申请的<" + item.aname + ">已通过活动专员的检验 R";
                    item.passed = true;
                    bool r = platACT.confirmAct(item);
                    if (r)
                    {
                        string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                        MyWrite(msg);
                        appSittingSet.Log(msg);
                    }

                }

            }
        }



        /// <summary>
        /// 520礼惠五月 存款即送
        /// </summary>
        [DisallowConcurrentExecution]
        public class MyJob11 : IJob
        {
            public void Execute(IJobExecutionContext context)
            {
                //DateTime dt = DateTime.Parse("2019/05/01 00:00:01");
                DateTime dt = DateTime.Parse("2019/05/20 00:00:01");
                List<betData> list = platACT.getActData2(actInfo[21]);
                //List<betData> list = new List<betData>();
                //list.Add(new betData() { username = "zr97987", bbid = "1470906", betTime = "2019-04-29 18:47", aid = actInfo[21] });
                //list.Add(new betData() { username = "mg1995", bbid = "1492880", betTime = "2019-05-01 20:50", aid = actInfo[21] });
                if (list == null)
                {
                    MyWrite(actInfo[22] + " 没有获取到新的注单，等待下次执行 ");
                    return;
                }
                if (list.Count == 0)
                {
                    MyWrite(actInfo[22] + "没有新的注单，等待下次执行 ");
                    return;
                }


                //MySQLHelper.connectionString = appSittingSet.readAppsettings("MySqlConnect").Split('|')[0];
                foreach (var item in list)
                {
                    //item.aid = actInfo[21];
                    item.aname = actInfo[22];

                    //判断5.1 是否有提交
                    string sql = $"select count(id) from e_submissions where aid= {item.aid}  and LOWER(username)='{item.username}' and status=1;";
                    object o = MySQLHelper.GetScalar(sql);
                    if (int.Parse(o.ToString())> 1)
                    {
                        item.passed = false;
                        item.msg = "您好，同一账号只能申请一次，申请不通过！R";
                        bool b = platACT.confirmAct(item);
                        if (b)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        continue;
                    }

                    //判断是否提交过 同一用户  只能一次
                    sql = $"select * from record where pass=1 and aid ={item.aid } and LOWER(username)='{ item.username.ToLower() }'  ";
                    if (appSittingSet.recorderDbCheck(sql))
                    {
                        item.passed = false;
                        item.msg = "您好，同一账号只能申请一次，申请不通过！R";
                        bool b = platACT.confirmAct(item);
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
                        //回填拒绝
                        item.passed = false;
                        item.msg = "账号有误";
                        bool r1 = platACT.confirmAct(item);
                        if (r1)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        //更新等级 拒绝
                        //userinfo.Province = memberLevel[9];
                        //userinfo.RegisterDevice = memberLevel[8];
                        //platGPK.UpadateMemberLevel(userinfo);
                        continue;
                    }
                    else
                    {
                        if (userinfo.JoinTime>dt.AddHours(-12))
                        {
                            //回填拒绝
                            item.passed = false;
                            item.msg = "注册时间必须要 "+dt.ToString("yyyy/MM/dd HH:mm:ss") + " 之前 R";
                            bool r1 = platACT.confirmAct(item);
                            if (r1)
                            {
                                string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                                MyWrite(msg);
                                appSittingSet.Log(msg);
                            }
                            //更新等级 拒绝
                            //userinfo.Province = memberLevel[9];
                            //userinfo.RegisterDevice = memberLevel[8];
                            //platGPK.UpadateMemberLevel(userinfo);
                            continue;
                        }
                        if (userinfo.BankAccount == null || userinfo.BankAccount=="")
                        {
                            //回填拒绝
                            item.passed = false;
                            item.msg = "未绑定银行卡 R";
                            bool r1 = platACT.confirmAct(item);
                            if (r1)
                            {
                                string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                                MyWrite(msg);
                                appSittingSet.Log(msg);
                            }
                            //更新等级 拒绝
                            //userinfo.Province = memberLevel[9];
                            //userinfo.RegisterDevice = memberLevel[8];
                            //platGPK.UpadateMemberLevel(userinfo);
                            continue;
                        }
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
                        //不满足条件 级别
                        bool r1 = platACT.confirmAct(item);
                        if (r1)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        return;
                    }



                    //这一次存款的数目 和时间
                    //查询gpk 记录 级别 存款金额、总次数 账户 ？？余额?? 最后存款时间 
                    item.lastOprTime = dt.AddHours(12).AddMinutes(-2).ToString("yyyy/MM/dd HH:mm:ss") ;
                    //item.betTime = "";
                    //item.isReal = false; //测试 正式取消掉 或者设置为true 默认true
                    betData bb= platGPK.MemberTransactionSearch(item);
                    if (bb == null)
                    {
                        continue;//异常
                    }
                    if (!bb.passed)
                    {
                        //账号不存在？
                        bb.msg = "经查询，您的账号此时间段无交易记录 请联系客服R";
                        bool r1 = platACT.confirmAct(bb);
                        if (r1)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", bb.username, bb.passed ? "通过" : "不通过", bb.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        continue;
                    }
                    if (bb.betMoney==15 )
                    {
                        bb.betMoney = 20;
                    }
                    //if (bb.betMoney-15>(decimal)0.01 && bb.betMoney-15 <1)
                    //{
                    //    bb.betMoney = 20;
                    //}
                    else
                    {
                        //存款不符合要求？
                        bb.passed = false;
                        bb.msg = "经查询，你的存款不符合要求，需存款15元才能参与活动 R";
                        bool r1 = platACT.confirmAct(bb);
                        if (r1)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", bb.username, bb.passed ? "通过" : "不通过", bb.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        continue;
                    }


                    //判断是否有进行投注 起始时间 要加上 处理的时间间隔  @@账户变动@@
                    item.lastOprTime = item.betTime;
                    //lastCashTime
                    object ba = platGPK.BetRecordSearch(item);
                    if (ba == null)
                    {
                        continue;//查询失败，什么都不干
                    }
                    if (!(bool)ba)
                    {
                        item.passed = false;
                        item.msg = "申请期间账户余额不能有任何变动！R";
                        bool b = platACT.confirmAct(item);
                        if (b)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        continue;
                    }


                    /*
                    //历史记录 注册 和绑定银行卡的时间间隔 大于1分钟 机器注册
                    string sr = platGPK.GetUserLoadHistory(userinfo, "申请加入会员,建立银行帐户资讯", new Random().Next(Act4Set[8], Act4Set[9]));
                    if (sr.Contains("ERR"))
                    {
                        continue;
                    }
                    if (sr.Contains("同IP其他") || sr.Contains("未绑定"))
                    {
                        item.passed = false;
                        item.msg = sr;
                        bool r1 = platACT.confirmAct(item);
                        if (r1)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        //更新等级 拒绝
                        userinfo.Province = memberLevel[9];
                        userinfo.RegisterDevice = memberLevel[8];
                        platGPK.UpadateMemberLevel(userinfo);
                        continue;
                    }

                    //同ip几个人申请 5
                    int cu = platGPK.GetUserCountSameIP(userinfo.LatestLogin_IP);
                    if (cu >= Act4Set[7])
                    {
                        item.passed = false;
                        item.msg = "同IP其他会员已申请过！R";
                        bool r1 = platACT.confirmAct(item);
                        if (r1)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                        }
                        //更新等级 拒绝
                        userinfo.Province = memberLevel[9];
                        userinfo.RegisterDevice = memberLevel[8];
                        platGPK.UpadateMemberLevel(userinfo);
                        continue;
                    }
                    */
                    //更新操作 加钱

                    bb.AuditType = "None";
                    //bb.Audit = bb.betMoney;
                    bb.Memo = bb.aname;
                    bb.Type = 5;

                    bool fr = platGPK.MemberDepositSubmit(item);
                    if (fr)
                    {
                        string msg = string.Format("用户{0}处理，存入金额{1} ,活动名称{2} ", item.username, item.betMoney, item.aname);
                        MyWrite(msg);
                        appSittingSet.Log(msg);

                        //记录到sqlite数据库
                        appSittingSet.recorderDb(item);
                    }
                    else
                    {
                        //充钱失败的情况 ？
                        continue;
                    }

                    //更新等级
                    //userinfo.Province = memberLevel[5];
                    //userinfo.RegisterDevice = memberLevel[4];
                    //platGPK.UpadateMemberLevel(userinfo);

                    //开启 不可跨区登陆  2019年2月28日11点43分
                    //userinfo.SexString = "true";
                    //platGPK.UpdateCrossRegionLogin(userinfo);

                    //发送站内信 14点09分 2019年4月2日
                    //if (mailbody[3] == "1")
                    //{
                    //    SendMailBody mail = new SendMailBody() { Subject = mailbody[4], MailBody = mailbody[5], SendMailType = "1", MailRecievers = userinfo.Account };
                    //    platGPK.SiteMailSendMail(mail);
                    //}

                    //回填 操作结果
                    item.msg = "恭喜您，您申请的<" + item.aname + ">已通过活动专员的检验 R";
                    item.passed = true;
                    bool r = platACT.confirmAct(item);
                    if (r)
                    {
                        string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                        MyWrite(msg);
                        appSittingSet.Log(msg);
                    }

                }

            }
        }

        #endregion

        #region 公共方法
        public static Write MyWrite;
        void Write(object msg)
        {
            if (lvRecorder.IsHandleCreated)
            {
                //lvRecorder.Invoke((MethodInvoker)delegate
                //{
                //    lvRecorder.Items.Insert(0, msg.ToString());
                //});
                lvRecorder.BeginInvoke(new Action(() =>
                {
                    lvRecorder.Items.Insert(0, msg.ToString() + "  " + DateTime.Now.ToLongTimeString());
                }));
            }
        }

        public static ClsListItem mycls;
        void ClsListItem()
        {
            if (lvRecorder.IsHandleCreated)
            {
                lvRecorder.BeginInvoke(new Action(() =>
                {
                    lvRecorder.Items.Clear();
                }));
            }
        }

        /// <summary>
        /// 更新等级
        /// </summary>
        /// <param name="swich"></param>
        /// <param name="userinfo"></param>
        //public static void updateLevel(string swich, Gpk_UserDetail userinfo)
        //{
        //    if (swich == "1")
        //    {
        //        bool r4 = platGPK.UpadateMemberLevel(userinfo.Id, userinfo.MemberLevelSettingId);
        //        if (!r4)
        //        {
        //            r4 = platGPK.UpadateMemberLevel(userinfo.Id, userinfo.MemberLevelSettingId);
        //            string msg = string.Format("用户{0}更新等级失败，需要手动更新", userinfo.Account);
        //            appSittingSet.txtLog(msg);
        //        }
        //    }
        //}

        #endregion

        #region 窗体事件
        private void Notify_BalloonTipClicked(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void Notify_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
            }
        }
        private void frmMain_Load(object sender, EventArgs e)
        {
            //Control.CheckForIllegalCrossThreadCalls = false;
            MyWrite = Write;
            mycls = ClsListItem;

            //先登录一遍
            MyJob_login myjob1 = new MyJob_login();
            myjob1.Execute(null);

            //启动调度
            start();

        }

        //关闭窗口，停止调度
        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (sched != null)
            {
                sched.Shutdown();
            }
            //appSittingSet.sendEmail(platname + " 程序关闭", "程序关闭 ");
            notify.Dispose();
        }

        private void frmMain_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();   //隐藏窗体
                notify.Visible = true; //使托盘图标可见
                notify.ShowBalloonTip(60000);
            }
        }

        private void 日志_Click(object sender, EventArgs e)
        {
                appSittingSet.showLogFile();
        }

        private void 配置_Click(object sender, EventArgs e)
        {
                string filePath = Application.ExecutablePath + ".config";
                System.Diagnostics.Process.Start("notepad.exe", filePath);
        }

        private void 重启_Click(object sender, EventArgs e)
        {
                if (MessageBox.Show("应用程序重启", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1) == DialogResult.OK)
                {
                    notify.Dispose();
                    Application.Restart();
                }
        }

        private void 清空_Click(object sender, EventArgs e)
        {
                ClsListItem();
        }

        private void 登陆优惠大厅_Click(object sender, EventArgs e)
        {
            //1.登录优惠大厅 一次 保存cookie 
            string msg = string.Format("活动站登录{0} ", platACT.loginActivity() ? "成功" : "失败");
            appSittingSet.Log(msg);
            MyWrite(msg);
        }

        private void 登陆BB后台_Click(object sender, EventArgs e)
        {
            string msg = string.Format("BB站登录{0} ", platBB.loginBB() ? "成功" : "失败");
            appSittingSet.Log(msg);
            MyWrite(msg);
        }

        private void 登陆GPK_Click(object sender, EventArgs e)
        {
            bool b = platGPK.loginGPK();
            string msg = string.Format("GPK站登录{0} ", b? "成功" : "失败");
            appSittingSet.Log(msg);
            //if (b)
            //{
            //    platGPK.WebSocketConnect();//重连websocket 
            //    //platGPK.WebSocketConnect2();//重连websocket                
            //}
            MyWrite(msg);
        }

        private void 登陆彩金后台_Click(object sender, EventArgs e)
        {
            //登陆活动站2 28
            string msg = string.Format("活动站2登录{0} ", platACT2.login() ? "成功" : "失败");
            appSittingSet.Log(msg);
            MyWrite(msg);
        }

        #endregion
    }

}
