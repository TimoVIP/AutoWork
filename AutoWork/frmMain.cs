using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Quartz;
using Quartz.Impl;
using TimoControl;
using System.Text.RegularExpressions;
using System.Text;

namespace AutoWork_Plat1
{

    public delegate void Write(string msg);//写lv消息

    public delegate void ClsListItem();
    public partial class frmMain : Form
    {

        static string platname;
        static int interval;
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
        static string[] ComfirMsg;
        static List<betData> list_temp = new List<betData>();
        public frmMain()
        {
            InitializeComponent();
            try
            {
                interval = int.Parse(appSittingSet.readAppsettings("Interval"));

                FiliterGroups = appSittingSet.readAppsettings("FiliterGroups").Split('|');

                sLuckNum = appSittingSet.readAppsettings("LuckNum").Split(new char[] { '|', '@' }, StringSplitOptions.RemoveEmptyEntries);//获取幸运数字数组

                gameNames = appSittingSet.readAppsettings("gameName1").Split('|');

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
                ComfirMsg = appSittingSet.readAppsettings("msg").Split(new char[] { '|', '@' }, StringSplitOptions.RemoveEmptyEntries);

                StringBuilder sb = new StringBuilder();
                //sb.Append(platname);

                for (int i = 0; i < actInfo.Length; i+=3)
                {
                    sb.Append(" " + actInfo[i + 1]);
                    sb.Append( actInfo[i + 2] == "1" ? "(开)" : "(关)");
                }
                sb.Append(" " + Prob[0] + "0%随机拒绝" + (Prob[1] == "1" ? "(开)" : "(关)"));
                this.Text = platname;
                this.toolStripStatusLabel1.Text = sb.ToString();
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

            //清除一周前的数据、日志文件
            if (AutoCls[1]=="1")
            {
                sched.ScheduleJob(JobBuilder.Create<MyJob0>().Build(), TriggerBuilder.Create().WithCronSchedule("1 0 8 * * ? ").Build());
            }
            //0 6 12 18 小时1:10s 执行 登陆
            sched.ScheduleJob(JobBuilder.Create<MyJob1>().Build(), TriggerBuilder.Create().WithCronSchedule("10 1 0,6,12,18 * * ? ").Build());
            //5秒一次 消除奖
            if (actInfo[2] == "1")
            {
                sched.ScheduleJob(JobBuilder.Create<MyJob2>().Build(), TriggerBuilder.Create().WithSimpleSchedule(x => x.WithIntervalInSeconds(interval).RepeatForever()).Build());
            }
            //8秒一次 幸运尾数
            if (actInfo[5] == "1")
            {
                sched.ScheduleJob(JobBuilder.Create<MyJob3>().Build(), TriggerBuilder.Create().WithSimpleSchedule(x => x.WithIntervalInSeconds(interval + 3).RepeatForever()).Build());
            }
            //6秒一次 以小博大
            if (actInfo[8] == "1")
            {
                sched.ScheduleJob(JobBuilder.Create<MyJob4>().Build(), TriggerBuilder.Create().WithSimpleSchedule(x => x.WithIntervalInSeconds(interval + 1).RepeatForever()).Build());
            }
            //7秒一次 首存17送20
            if (actInfo[11] == "1")
            {
                sched.ScheduleJob(JobBuilder.Create<MyJob5>().Build(), TriggerBuilder.Create().WithSimpleSchedule(x => x.WithIntervalInSeconds(interval + 2).RepeatForever()).Build());
            }

            //开始运行
            sched.Start();
        }
  
        /// <summary>
        /// 每天8:00:01 执行 删除一周前的日志 数据库一周前的数据
        /// </summary>
        public class MyJob0: IJob
        {
            public void Execute(IJobExecutionContext context)
            {
                int diff =  int.Parse(AutoCls[0]);
                string sql = "delete from record where subminttime < '"+DateTime.Now.AddDays(-diff).Date.ToString("yyyy-MM-dd")+"'";
                appSittingSet.execSql(sql);
                appSittingSet.txtLog("清除一周前的数据");
                appSittingSet.clsLogFiles(diff);
                appSittingSet.txtLog("清除一周前的日志");
            }
        }

        /// <summary>
        /// /0 6 12 18 小时登录
        /// </summary>
        public class MyJob1: IJob
        {
            public void Execute(IJobExecutionContext context)
            {
                //清除listbox 信息
                mycls();

                //1.登录优惠大厅 一次 保存cookie 
                string msg = string.Format("活动站登录{0} ", platACT.loginActivity() ? "成功" : "失败");
                appSittingSet.txtLog(msg);
                MyWrite(msg);

                msg = string.Format("BB站登录{0} ", platBB.loginBB() ? "成功" : "失败");
                appSittingSet.txtLog(msg);
                MyWrite(msg);

                msg = string.Format("GPK站登录{0} ", platGPK.loginGPK() ? "成功" : "失败");
                appSittingSet.txtLog(msg);
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
                    MyWrite(actInfo[1] + " 没有获取到新的注单，等待下次执行 " );
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

                    //0 注单号不合法 驳回
                    if (item.betno != "" && Regex.IsMatch(item.betno, @"^\d{12}$"))
                    {

                    }
                    else
                    {
                        //回填失败
                        item.passed = false;
                        //item.msg = "请提供正确的注单号 R";
                        item.msg = ComfirMsg[1];
                        bool b1 = platACT.confirmAct(item);
                        if (b1)
                        {
                            string msg = string.Format("用户{0}处理完毕，注单号{1}，处理为 {2}，回复消息 {3}", item.username, item.betno, item.passed ? "通过" : "不通过", item.msg);
                            MyWrite(msg);
                            appSittingSet.txtLog(msg);
                        }
                        return;
                    }

                    //2.获取注单详情信息 bb
                    //betData bd = platBB.getBetDetail(item);
                    betData bb = platBB.getBet_Details(item);
                    //3.是否匹配要求
                    if (bb == null)
                    {
                        return;
                    }

                    //查询不到信息 注单号不存在 情况+ 网站维护
                    if (!bb.passed)
                    {

                        bool b = platACT.confirmAct(bb);
                        if (b)
                        {
                            string msg = string.Format("用户{0}处理完毕，注单号{1}，处理为 {2}，回复消息 {3}", bb.username, bb.betno, bb.passed ? "通过" : "不通过", bb.msg);
                            MyWrite(msg);
                            appSittingSet.txtLog(msg);
                        }
                        return;
                    }

                    //判断 美东时间
                    DateTime bt = Convert.ToDateTime(bb.betTime).Date;
                    DateTime now_d = DateTime.Now.AddHours(-12).Date;
                    if (bt != now_d)
                    {
                        bb.passed = false;
                        //bb.msg = "您好，非美东时间当天注单，申请不通过！R";
                        bb.msg = ComfirMsg[2];
                        bool b2 = platACT.confirmAct(bb);
                        if (b2)
                        {
                            string msg = string.Format("用户{0}处理完毕，注单号{1}，处理为 {2}，回复消息 {3}", bb.username, bb.betno, bb.passed ? "通过" : "不通过", bb.msg);
                            MyWrite(msg);
                            appSittingSet.txtLog(msg);
                            //appSittingSet.recorderDb(bb,actInfo[0]);
                        }
                        return;
                    }

                    //判断 游戏 名称
                    bool flag2 = false;
                    foreach (var g in gameNames)
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
                        //bb.msg = string.Format("此活动仅限{0}游戏 R",gameNames);
                        bb.msg = string.Format(ComfirMsg[3],gameNames);
                        bool b3 = platACT.confirmAct(bb);
                        if (b3)
                        {
                            string msg = string.Format("用户{0}处理完毕，注单号{1}，处理为 {2}，回复消息 {3}", bb.username, bb.betno, bb.passed ? "通过" : "不通过", bb.msg);
                            MyWrite(msg);
                            appSittingSet.txtLog(msg);
                            //appSittingSet.recorderDb(bb, actInfo[0]);
                        }
                        return;
                    }

                    //免费游戏直接驳回 不通过
                    if (bb.betMoney == 0)
                    {
                        bb.passed = false;
                        //bb.msg = "你好，免费游戏注单不享受此活动！R";
                        bb.msg = ComfirMsg[4];
                        bool b4 = platACT.confirmAct(bb);
                        if (b4)
                        {
                            string msg = string.Format("用户{0}处理完毕，注单号{1}，处理为 {2}，回复消息 {3}", bb.username, bb.betno, bb.passed ? "通过" : "不通过", bb.msg);
                            MyWrite(msg);
                            appSittingSet.txtLog(msg);
                            //appSittingSet.recorderDb(bb, actInfo[0]);
                        }
                        return;
                    }

                    //判断是否提交过 同一用户 同类游戏 一天只能一次
                    string sql = "select * from record where (betno='" + bb.betno + "' and pass=1 and aid="+actInfo[0]+" ) or (pass=1  and aid="+actInfo[0]+" and  username='" + bb.username + "' and gamename='"+bb.gamename+"'  and subminttime > '" + DateTime.Now.AddHours(-12).ToString("yyyy-MM-dd") + " 00:00:01' and  subminttime < '" + DateTime.Now.AddHours(-12).ToString("yyyy-MM-dd") + " 23:59:59') ";
                    if (appSittingSet.recorderDbCheck(sql))
                    {
                        bb.passed = false;
                        //bb.msg = "您好，同一游戏一天内只能申请一次，申请不通过！R";
                        bb.msg =ComfirMsg[5];
                        bool b5 = platACT.confirmAct(bb);
                        if (b5)
                        {
                            string msg = string.Format("用户{0}处理完毕，注单号{1}，处理为 {2}，回复消息 {3}", bb.username, bb.betno, bb.passed ? "通过" : "不通过", bb.msg);
                            MyWrite(msg);
                            appSittingSet.txtLog(msg);
                            //appSittingSet.recorderDb(bb, actInfo[0]);
                        }
                        return;
                    }

                    //获取次数
                    betData bb2= platBB.getBetDetail_Bet_Times(bb);
                    if (!bb2.passed)
                    {
                        bool b5 = platACT.confirmAct(bb2);
                        if (b5)
                        {
                            string msg = string.Format("用户{0}处理完毕，注单号{1}，处理为 {2}，回复消息 {3}", bb2.username, bb2.betno, bb2.passed ? "通过" : "不通过", bb2.msg);
                            MyWrite(msg);
                            appSittingSet.txtLog(msg);
                        }
                        return;
                    }


                    //计算规则 多少次 送多少倍
                    if (bb2.betTimes >= rolltimes[0] && bb2.betTimes < rolltimes[1])
                    {
                        bb2.betMoney *= rolltimes[2];
                    }
                    else if (bb2.betTimes >= rolltimes[3] && bb2.betTimes < rolltimes[4])
                    {
                        bb2.betMoney *= rolltimes[5];
                    }
                    else if (bb2.betTimes >= rolltimes[6] && bb2.betTimes < rolltimes[7])
                    {
                        bb2.betMoney *= rolltimes[8];
                    }
                    else if (bb2.betTimes >= rolltimes[9])
                    {
                        bb2.betMoney *= rolltimes[11];
                    }
                    else
                    {
                        bb2.betMoney *= 0;
                        //bb2.msg = string.Format("此活动仅限消除{0}次及以上，您提供的注单消除次数为{1}次，申请不通过！R", rolltimes[0], bb2.betTimes);
                        bb2.msg = string.Format(ComfirMsg[6], rolltimes[0], bb2.betTimes);
                        bb2.passed = false;

                        bool b7 = platACT.confirmAct(bb);
                        if (b7)
                        {
                            string msg = string.Format("用户{0}处理完毕，注单号{1}，处理为 {2}，回复消息 {3}", bb.username, bb.betno, bb.passed ? "通过" : "不通过", bb.msg);
                            MyWrite(msg);
                            appSittingSet.txtLog(msg);
                            //appSittingSet.recorderDb(bb2, actInfo[0]);
                        }
                        return;
                    }

                    //if (bb.aname.Length>0 && bb.wallet.Length>0)
                    //{

                    //}
                    //else
                    //{
                    //    //钱包没有查出来
                    //    return;
                    //}

                    //判断注单是否是用户的 检查钱包 会员组别
                    betData bd2 = platGPK.checkInGPK(bb);
                    if (bd2 == null)
                    {
                        return;
                    }

                    //判断 层级是否在 列表之中
                    foreach (var s in FiliterGroups)
                    {
                        if (bd2.level == s)
                        {
                            bd2.passed = false;
                            //bd2.msg = "经查询，您的账号目前不享有此优惠！ R";
                            bd2.msg = ComfirMsg[7];
                            //回填失败
                            bool b6 = platACT.confirmAct(bd2);
                            if (b6)
                            {
                                string msg = string.Format("用户{0}处理完毕，注单号{1}，处理为 {2}，回复消息 {3}", bd2.username, bd2.betno, bd2.passed ? "通过" : "不通过", bd2.msg);
                                MyWrite(msg);
                                appSittingSet.txtLog(msg);
                                //appSittingSet.recorderDb(bd2, actInfo[0]);
                                return;
                            }
                            break;
                        }
                    }
                    //钱包 层级通过

                   
                    //次数符合
                    //提交充值 加钱 
                    bool fr = platGPK.submitToGPK(bb2, actInfo[1]);
                    if (fr)
                    {
                        string msg = string.Format("用户{0}处理，存入金额{1}，注单号{2}，活动名称{3} ", bd2.username, bd2.betMoney, bd2.betno, actInfo[1]);
                        MyWrite(msg);
                        appSittingSet.txtLog(msg);
                        //记录到数据库
                        appSittingSet.recorderDb(bd2, actInfo[0]);
                        bd2.msg = string.Format(ComfirMsg[0],actInfo[1]);
                        bool b8 = platACT.confirmAct(bd2);
                        if (b8)
                        {
                           msg = string.Format("用户{0}处理完毕，注单号{1}，处理为 {2}，回复消息 {3}", bd2.username, bd2.betno, bd2.passed ? "通过" : "不通过", bd2.msg);
                            MyWrite(msg);
                            appSittingSet.txtLog(msg);
                            return;
                        }
                    }
                    else
                    {
                        //充钱失败的情况 ？ 不做处理 下一次处理
                        return;
                    }

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
                    MyWrite(actInfo[4] + " 没有获取到新的注单，等待下次执行 " );
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
                        if (!item.betno.EndsWith(sLuckNum[0]))
                        //if (!item.betno.Substring(item.betno.Length - 1).Contains(sLuckNum[0]))
                        {
                            //回填失败
                            item.passed = false;
                            item.msg = "注单号不含活动尾数数字，请提供正确的注单号 R";
                            bool b = platACT.confirmAct(item);
                            if (b)
                            {
                                string msg = string.Format("用户{0}处理完毕，注单号{1}，处理为 {2}，回复消息 {3}", item.username, item.betno, item.passed ? "通过" : "不通过", item.msg);
                                MyWrite(msg);
                                appSittingSet.txtLog(msg);
                            }
                            return;
                        }


                        //获取注单信息 是否存在 
                        betData bb = platBB.getBet_Details(item);
                        if (bb == null)
                        {
                            return;
                        }

                        //查询不到信息 注单号不存在 情况 网站维护
                        if (!bb.passed)
                        {

                            bool b = platACT.confirmAct(bb);
                            if (b)
                            {
                                string msg = string.Format("用户{0}处理完毕，注单号{1}，处理为 {2}，回复消息 {3}", bb.username, bb.betno, bb.passed ? "通过" : "不通过", bb.msg);
                                MyWrite(msg);
                                appSittingSet.txtLog(msg);
                            }
                            return;
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
                                appSittingSet.txtLog(msg);
                            }
                            return;
                        }

                        //判断 游戏 名称
                        bool flag2 = false;
                        foreach (var g in gameNames)
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
                            bb.msg = "此活动仅限BBIN电子游艺“连环夺宝”“连环夺宝2”“糖果派对”“糖果派对2”四款游戏 R";
                            bool b = platACT.confirmAct(bb);
                            if (b)
                            {
                                string msg = string.Format("用户{0}处理完毕，注单号{1}，处理为 {2}，回复消息 {3}", bb.username, bb.betno, bb.passed ? "通过" : "不通过", bb.msg);
                                MyWrite(msg);
                                appSittingSet.txtLog(msg);
                            }
                            return;
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
                                appSittingSet.txtLog(msg);
                            }
                            return;
                        }

                        //判断是否提交过 同一用户 所有游戏 一天只能一次
                        string sql = "select * from record where (betno='" + bb.betno + "' and pass=1 and aid =" + actInfo[3] + ") or ( pass=1 and aid =" + actInfo[3] + " and username='" + bb.username + "'   and subminttime > '" + DateTime.Now.AddHours(-12).ToString("yyyy-MM-dd") + " 00:00:01' and  subminttime < '" + DateTime.Now.AddHours(-12).ToString("yyyy-MM-dd") + " 23:59:59') ";
                        if (appSittingSet.recorderDbCheck(sql))
                        {
                            bb.passed = false;
                            bb.msg = "您好，同一游戏一天内只能申请一次，申请不通过！R";
                            bool b = platACT.confirmAct(bb);
                            if (b)
                            {
                                string msg = string.Format("用户{0}处理完毕，注单号{1}，处理为 {2}，回复消息 {3}", bb.username, bb.betno, bb.passed ? "通过" : "不通过", bb.msg);
                                MyWrite(msg);
                                appSittingSet.txtLog(msg);
                            }
                            return;
                        }

                        //if (bb.aname.Length > 0 && bb.wallet.Length > 0)
                        //{

                        //}
                        //else
                        //{
                        //    //钱包没有查出来
                        //    return;
                        //}

                        //判断注单是否是用户的 检查钱包 会员组别
                        betData bd2 = platGPK.checkInGPK(bb);
                        if (bd2 == null)
                        {
                            return;
                        }

                        //判断 层级是否在 列表之中
                        foreach (var s in FiliterGroups)
                        {
                            if (bd2.level == s)
                            {
                                bd2.passed = false;
                                bd2.msg = "经查询，您的账号目前不享有此优惠！ R";
                                break;
                            }
                        }
                        if (!bd2.passed)
                        {
                            //回填失败
                            bool b6 = platACT.confirmAct(bd2);
                            if (b6)
                            {
                                string msg = string.Format("用户{0}处理完毕，注单号{1}，处理为 {2}，回复消息 {3}", bd2.username, bd2.betno, bd2.passed ? "通过" : "不通过", bd2.msg);
                                MyWrite(msg);
                                appSittingSet.txtLog(msg);
                                //appSittingSet.recorderDb(bd2, actInfo[0]);
                            }
                            return;
                        }


                            //计算 需要加钱 的数字 
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

                            //大于max 不处理
                            if (bb.betMoney > maxValue)
                            {
                                bb.passed = false;
                                bb.msg = "奖励金额大于"+maxValue+"，请联系在线客服处理 R";
                                bool b = platACT.confirmAct(bb);
                                if (b)
                                {
                                    string msg = string.Format("用户{0}处理完毕，注单号{1}，处理为 {2}，回复消息 {3}", bb.username, bb.betno, bb.passed ? "通过" : "不通过", bb.msg);
                                    MyWrite(msg);
                                    appSittingSet.txtLog(msg);
                                }
                                return;
                            }

                            //加钱  提交数据
                            bool fr = platGPK.submitToGPK(bd2, actInfo[4]);
                            if (fr)
                            {
                                string msg = string.Format("用户{0}处理，存入金额{1}，注单号{2}，活动名称{3}", bd2.username, bd2.betMoney, bd2.betno, actInfo[4]);
                                MyWrite(msg);
                                appSittingSet.txtLog(msg);


                                //记录到sqlite数据库
                                appSittingSet.recorderDb(bd2, actInfo[3]);
                            }
                            else
                            {
                                //充钱失败的情况 ？
                                return;
                            }

                            //回填
                            bd2.msg = "恭喜您，您申请的<" + actInfo[4] + ">已通过活动专员的检验";
                            bool r = platACT.confirmAct(bd2);
                            if (r)
                            {
                                string msg = string.Format("用户{0}处理完毕，注单号{1}，处理为 {2}，回复消息 {3}", bd2.username, bd2.betno, bd2.passed ? "通过" : "不通过", bd2.msg);
                                MyWrite(msg);
                                appSittingSet.txtLog(msg);
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
                            appSittingSet.txtLog(msg);
                            return;
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
                //最好每次处理一条
                //betData item = platACT.getActData2_top(actInfo[6]);
                //if (item == null)
                //{
                //    MyWrite(aname3 + " 没有新的注单，等待下次执行 " );
                //    return;
                //}
                //测试数据 zyp5200 2018-12-03 18:14
                //betData bb222 = new betData() { betTime = "2018-12-06 18:13:59", username = "binyi135" };
                //bb222 = platACT.getActData2_time(actInfo[6], bb222);
                //bb222 = platGPK.checkInGPK_transaction(bb222);

                //获取注单号码  yh
                List<betData> list = platACT.getActData2(actInfo[6]);
                if (list == null)
                {
                    MyWrite(actInfo[7] + " 没有获取到新的注单，等待下次执行 " );
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
                    betData bb = platACT.getActData2_time(actInfo[6], item);
                    if (bb==null)
                    {
                        return;
                    }

                    //判断是否提交过 同一用户 所有游戏 一天只能一次
                    string sql = "select * from record where pass=1 and aid =" + actInfo[6] + " and username='" + bb.username + "'   and subminttime > '" + DateTime.Now.AddHours(-12).ToString("yyyy-MM-dd") + " 00:00:01' and  subminttime < '" + DateTime.Now.AddHours(-12).ToString("yyyy-MM-dd") + " 23:59:59' ";
                    if (appSittingSet.recorderDbCheck(sql))
                    {
                        bb.passed = false;
                        bb.msg = "您好，同一账号一天内只能申请一次，申请不通过！R";
                        bool b = platACT.confirmAct(bb);
                        if (b)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", bb.username, bb.passed ? "通过" : "不通过", bb.msg);
                            MyWrite(msg);
                            appSittingSet.txtLog(msg);
                        }
                        return;
                    }





                    //查询gpk 记录 级别 存款金额、总次数 账户 ？？余额?? 最后存款时间
                    betData bb1 = platGPK.checkInGPK_transaction(bb);
                    if (bb1==null)
                    {
                        return;
                    }
                    if (!bb.passed)
                    {
                        //账号不存在？
                        bb1.msg = "经查询，您的账号有误！或者此时间段无交易记录 R";
                        bool r1 = platACT.confirmAct(bb1);
                        if (r1)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", bb1.username, bb1.passed ? "通过" : "不通过", bb1.msg);
                            MyWrite(msg);
                            appSittingSet.txtLog(msg);
                        }
                        return;
                    }


                    //判断 层级是否在 列表之中
                    foreach (var s in FiliterGroups)
                    {
                        if (bb1.level == s)
                        {
                            bb1.passed = false;
                            bb1.msg = "经查询，您的账号目前不享有此优惠！ R";
                            break;
                        }
                    }
                    if (!bb1.passed)
                    {
                        //不满足条件 级别
                        bool r1 = platACT.confirmAct(bb1);
                        if (r1)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", bb1.username, bb1.passed ? "通过" : "不通过", bb1.msg);
                            MyWrite(msg);
                            appSittingSet.txtLog(msg);
                        }
                        return;
                    }


                    //计算 活动赠送金额
                    if ( (bb1.betMoney >= Act4Stander[4] && bb1.betMoney <= Act4Stander[4]+ 1) && (bb1.total_money >= Act4Stander[7] && bb1.betTimes >= Act4Stander[6]))
                    {
                            //满足6次 200 
                            bb1.betMoney = Act4Stander[5];
                            bb.passed = true;
                    }
                    else if ((bb1.betMoney >= Act4Stander[0] && bb1.betMoney <= Act4Stander[0] + 1) && (bb1.total_money >= Act4Stander[3] && bb1.betTimes >= Act4Stander[2]))
                    {
                            //10的 满足 3次 100
                            bb1.betMoney = Act4Stander[1];
                            bb1.passed = true;
                    }
                    else
                    {
                        bb.passed = false;
                        bb.msg = "您的 存款次数、金额 没有达到活动要求 R";
                        //不满足条件 活动要求
                        bool r1 = platACT.confirmAct(bb1);
                        if (r1)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", bb1.username, bb1.passed ? "通过" : "不通过", bb1.msg);
                        MyWrite(msg);
                        appSittingSet.txtLog(msg);
                        }
                        return;
                    }


                    //判断是否有进行投注 起始时间 要加上 处理的时间间隔 
                    //DateTime d;
                    //DateTime.TryParse(bb.betTime, out d);
                    //TimeSpan ts = DateTime.Now - d.AddHours(12);
                    //DateTime d2;
                    //DateTime.TryParse(bb.lastCashTime, out d2);          
                    //d2.AddMinutes(ts.Minutes);
                    //bb.lastCashTime = d2.ToString("G");

                    object ba = platGPK.BetRecordSearch(bb1);
                    if (ba==null)
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
                            appSittingSet.txtLog(msg);
                        }
                        return;
                    }


                    //更新一下 钱包 ？？OK
                    bool b000 = platGPK.autoUpadateMemberAcc(bb.Id);

                    //查询账户余额
                    betData bb2 = platGPK.MemberGetDetail(bb);
                    if (bb2==null)
                    {
                        return;
                    }
                    //判断账户余额是否在范围 10-15 20-25 区间
                    if ((bb.subtotal>= Act4Stander[0] && bb1.subtotal<= (Act4Stander[0] +5) )|| (bb1.subtotal>= Act4Stander[4] && bb1.subtotal<=(Act4Stander[4]+5)))
                    {

                    }
                    else
                    {
                        bb1.passed = false;
                        bb1.msg = "经查询，您的账号余额不符合要求！ R";
                        bool r1 = platACT.confirmAct(bb1);
                        if (r1)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", bb1.username, bb1.passed ? "通过" : "不通过", bb1.msg);
                            MyWrite(msg);
                            appSittingSet.txtLog(msg);
                        }
                        return;
                    }

                    //更新操作 加钱
                    bool fr = platGPK.submitToGPK(bb1, actInfo[7]);
                    if (fr)
                    {
                        string msg = string.Format("用户{0}处理，存入金额{1} ,活动名称{2} ", bb1.username, bb1.betMoney, actInfo[7]);
                        MyWrite(msg);
                        appSittingSet.txtLog(msg);

                        //记录到sqlite数据库
                        appSittingSet.recorderDb(bb1, actInfo[6]);
                    }
                    else
                    {
                        //充钱失败的情况 ？
                        return;
                    }

                    //更新等级
                    if (memberLevel[1]== "1")
                    {
                        bool sr = platGPK.UpadateMemberLevel(bb.memberId, memberLevel[0]);
                        if (!sr)
                        {
                            sr = platGPK.UpadateMemberLevel(bb.memberId, memberLevel[0]);
                            string msg = string.Format("用户{0}更新等级失败，需要手动更新", bb1.username);
                            appSittingSet.txtLog(msg);
                        }
                    }


                    //回填 操作结果
                    bb1.msg = "恭喜您，您申请的<" + actInfo[7] + ">已通过活动专员的检验";
                    bool r = platACT.confirmAct(bb1);
                    if (r)
                    {
                        string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", bb1.username,  bb1.passed ? "通过" : "不通过", bb1.msg);
                        MyWrite(msg);
                        appSittingSet.txtLog(msg);
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
                List<betData> list = platACT.getActData2(actInfo[9]);
                //list.Add(new betData() { username = "wlf5577558", betTime = "2018-12-29 21:50:59", bbid = "738215", passed = true, aid = "40" });//默认等于合格
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

                    if (Prob[1]=="1")
                    {
                        //如果达到10条清除掉
                        if (list_temp.Count==10)
                        {
                            list_temp.Clear();
                        }

                        if (!list_temp.Exists(x=> x.bbid == item.bbid))
                        {
                            list_temp.Add(item);
                        }
                    
                        if (list_temp.Count>(10 - int.Parse(Prob[0])))
                        {
                            //直接拒绝
                            item.passed = false;
                            item.msg = "RR 同IP其他会员已申请通过";
                            bool r1 = platACT.confirmAct(item);
                            if (r1)
                            {
                                string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                                MyWrite(msg);
                                appSittingSet.txtLog(msg);
                            }
                            continue;
                        }

                    }

                    //判断是否提交过 同一用户 所有游戏 一天只能一次
                    string sql = "select * from record where pass=1 and aid =" + actInfo[9] + " and username='" + item.username + "'   and subminttime > '" + DateTime.Now.AddHours(-12).ToString("yyyy-MM-dd") + " 00:00:01' and  subminttime < '" + DateTime.Now.AddHours(-12).ToString("yyyy-MM-dd") + " 23:59:59' ";
                    if (appSittingSet.recorderDbCheck(sql))
                    {
                        item.passed = false;
                        item.msg = "您好，同一账号一天内只能申请一次，申请不通过！R";
                        bool b = platACT.confirmAct(item);
                        if (b)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", item.username, item.passed ? "通过" : "不通过", item.msg);
                            MyWrite(msg);
                            appSittingSet.txtLog(msg);
                        }
                        continue;
                    }

                    //固定存款17-18元 存款次数1次
                    betData bb1 = platGPK.checkInGPK_transaction(item);
                    if (bb1 == null)
                    {
                        continue;
                    }
                    if (!bb1.passed)
                    {
                        //账号不存在？
                        bb1.msg = "经查询，您的账号有误！或者此时间段无交易记录 R";
                        bool r1 = platACT.confirmAct(bb1);
                        if (r1)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", bb1.username, bb1.passed ? "通过" : "不通过", bb1.msg);
                            MyWrite(msg);
                            appSittingSet.txtLog(msg);
                        }
                        continue;
                    }
                    if (bb1.betTimes==1 && (bb1.betMoney>= Act4Set[0] && bb1.betMoney < Act4Set[0]+1))
                    {
                        bb1.betMoney = Act4Set[1];//20元
                        bb1.passed = true;
                    }
                    else
                    {
                        bb1.passed = false;
                        bb1.msg = "此活动仅限首次存款固定金额"+ Act4Set[0] + "元！ R";
                        bool r1 = platACT.confirmAct(bb1);
                        if (r1)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", bb1.username, bb1.passed ? "通过" : "不通过", bb1.msg);
                            MyWrite(msg);
                            appSittingSet.txtLog(msg);
                        }
                        continue;
                    }

                    //获取详细信息
                    Gpk_UserDetail userinfo = platGPK.GetUserDetail(bb1.username);
                    //历史记录 注册 和绑定银行卡的时间间隔 大于1分钟 机器注册
                    string sr = platGPK.GetUserLoadHistory(userinfo.Id,userinfo.Account,Act4Set[3],Act4Set[4]);
                    if (sr!="OK")
                    {
                        bb1.passed = false;
                        bb1.msg = sr;
                        bool r1 = platACT.confirmAct(bb1);
                        if (r1)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", bb1.username, bb1.passed ? "通过" : "不通过", bb1.msg);
                            MyWrite(msg);
                            appSittingSet.txtLog(msg);
                        }
                        continue;
                    }

                    //同ip几个人申请 5
                    int cu = platGPK.GetUserCountSameIP(userinfo.LatestLogin_IP);
                    if (cu>= Act4Set[2])
                    {
                        bb1.passed = false;
                        bb1.msg = "同IP其他会员已申请过！R";
                        bool r1 = platACT.confirmAct(bb1);
                        if (r1)
                        {
                            string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", bb1.username, bb1.passed ? "通过" : "不通过", bb1.msg);
                            MyWrite(msg);
                            appSittingSet.txtLog(msg);
                        }
                        continue;
                    }

                    //更新操作 加钱
                    bool fr = platGPK.submitToGPK(bb1, actInfo[10]);
                    if (fr)
                    {
                        string msg = string.Format("用户{0}处理，存入金额{1} ,活动名称{2} ", bb1.username, bb1.betMoney, actInfo[10]);
                        MyWrite(msg);
                        appSittingSet.txtLog(msg);

                        //记录到sqlite数据库
                        appSittingSet.recorderDb(bb1, actInfo[6]);
                    }
                    else
                    {
                        //充钱失败的情况 ？
                        continue;
                    }

                    //更新等级
                    if (memberLevel[3] == "1")
                    {
                        bool r4 = platGPK.UpadateMemberLevel(userinfo.Id, memberLevel[2]);
                        if (!r4)
                        {
                            r4 = platGPK.UpadateMemberLevel(userinfo.Id, memberLevel[2]);
                            string msg = string.Format("用户{0}更新等级失败，需要手动更新", bb1.username);
                            appSittingSet.txtLog(msg);
                        }
                    }

                    //回填 操作结果
                    bb1.msg = "恭喜您，您申请的<" + actInfo[10] + ">已通过活动专员的检验 R";
                    bool r = platACT.confirmAct(bb1);
                    if (r)
                    {
                        string msg = string.Format("用户{0}处理完毕，处理为 {1}，回复消息 {2}", bb1.username, bb1.passed ? "通过" : "不通过", bb1.msg);
                        MyWrite(msg);
                        appSittingSet.txtLog(msg);
                    }

                }

            }
        }
        #endregion

        #region 写UI ListView 消息
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
                    lvRecorder.Items.Insert(0, msg.ToString()+"  " +DateTime.Now.ToLongTimeString());
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

        #endregion


        #region 窗体事件
        private void frmMain_Load(object sender, EventArgs e)
        {
            //Control.CheckForIllegalCrossThreadCalls = false;
            MyWrite = Write;
            mycls = ClsListItem;

            //先登录一遍
            MyJob1 myjob1 = new MyJob1();
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
            appSittingSet.sendEmail(platname +" 程序关闭", "程序关闭 ");
            notifyIcon1.Dispose();
        }

        private void frmMain_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();   //隐藏窗体
                notifyIcon1.Visible = true; //使托盘图标可见
                notifyIcon1.ShowBalloonTip(60000);
            }
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
            }
        }

        private void notifyIcon1_BalloonTipClicked_1(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        #endregion

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            ToolStripMenuItem tsmi = (ToolStripMenuItem)e.ClickedItem;
            if (tsmi.Name == "toolStripMenuItem1")
            {
                //登录一遍
                MyJob1 myjob1 = new MyJob1();
                myjob1.Execute(null);
                appSittingSet.txtLog("手动操作登录");
            }
            else if (tsmi.Name == "toolStripMenuItem2")
            {
                appSittingSet.showLogFile();
            }
            else if (tsmi.Name == "toolStripMenuItem3")
            {
                string filePath = Application.ExecutablePath + ".config";
                System.Diagnostics.Process.Start("notepad.exe", filePath);
            }
            else if (tsmi.Name == "toolStripMenuItem4")
            {
                if (MessageBox.Show("应用程序重启", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1) == DialogResult.OK)
                {
                    notifyIcon1.Dispose();
                    Application.Restart();
                }
            }
        }
    }

}
