using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using TimoControl;

namespace ACT_mantissa
{
    public delegate void Write(string msg);//写lv消息 全局变量

    public partial class frmMain : Form
    {
        static int interval;//间隔时间

        static string[] sLuckNum;

        static string[] gameName_Arr;

        private static string[] FiliterGroups;

        static int aid1;
        static int aid2;
        static string aname1;

        static string aname2;

        static IScheduler sched2;
        public frmMain()
        {
            InitializeComponent();

            interval = int.Parse(appSittingSet.readAppsettings("Interval"));

            this.Text = "活动平台 " + appSittingSet.readAppsettings("ACT").Split('|')[2];

            sLuckNum = appSittingSet.readAppsettings("LuckNum").Split(new char[] { '|', '@' }, StringSplitOptions.RemoveEmptyEntries);//获取幸运数字数组

            gameName_Arr = appSittingSet.readAppsettings("gameName").Split('|');

            FiliterGroups = appSittingSet.readAppsettings("FiliterGroups").Split('|');

            aid1 = int.Parse(appSittingSet.readAppsettings("aid1"));
            aid2 = int.Parse(appSittingSet.readAppsettings("aid2"));
            aname1 = appSittingSet.readAppsettings("aname1");

            aname2 = appSittingSet.readAppsettings("aname2");

        }

        #region 调度


        /// <summary>
        /// 开始调度
        /// </summary>
        public static void start()
        {

            //创建一个作业调度池
            ISchedulerFactory schedf = new StdSchedulerFactory();
            sched2 = schedf.GetScheduler();
            //创建出一个具体的作业
            IJobDetail job1 = JobBuilder.Create<MyJob1>().Build();
            IJobDetail job2 = JobBuilder.Create<MyJob2>().Build();
            //配置一个触发器 5秒一次
            ISimpleTrigger trigger1 = (ISimpleTrigger)TriggerBuilder.Create().WithSimpleSchedule(x => x.WithIntervalInSeconds(interval).RepeatForever()).Build();
            //0 6 12 18 小时执行
            ICronTrigger trigger2 = (ICronTrigger)TriggerBuilder.Create().WithCronSchedule("0 0 0,6,12,18 * * ? ").Build();
            //加入作业调度池中
            sched2.ScheduleJob(job1, trigger1);
            sched2.ScheduleJob(job2, trigger2);
            //开始运行
            sched2.Start();
        }

        /// <summary>
        /// 作业 处理注单
        /// </summary>
        [DisallowConcurrentExecution]
        public class MyJob1 : IJob
        {
            public void Execute(IJobExecutionContext context)
            {


                //获取注单号码  yh
                List<betData> list = platACT.getActData(aid2);
                if (list == null)
                {
                    MyWrite("没有新的注单，等待下次执行 " + DateTime.Now.ToString("G"));
                    return;
                }
                if (list.Count == 0)
                {
                    MyWrite("没有新的注单，等等下次执行 " + DateTime.Now.ToString("G"));
                    return;
                }


                //获取注单详情
                foreach (var item in list)
                {

                    //0 注单号不合法 驳回

                    if (item.betno != "" && Regex.IsMatch(item.betno, @"^\d{12}$"))
                    {
                        //字后一位是否含有幸运数字 str=str.Substring(str.Length-i)
                        if (!item.betno.Substring(item.betno.Length - 1).Contains(sLuckNum[0]))
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
                        foreach (var g in gameName_Arr)
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
                        string sql = "select * from record where (betno='" + bb.betno + "' and pass=1 and aid ="+aid2+") or ( pass=1 and aid ="+aid2+" and username='" + bb.username + "'   and subminttime > '" + DateTime.Now.AddHours(-12).ToString("yyyy-MM-dd") + " 00:00:01' and  subminttime < '" + DateTime.Now.AddHours(-12).ToString("yyyy-MM-dd") + " 23:59:59') ";
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

                        if (bd2.passed)
                        {
                            //计算 需要加钱 的数字 
                            for (int i = sLuckNum.Length / 3; i >=0 ; i--)
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

                            //大于500 不处理
                            if (bb.betMoney > 500)
                            {
                                return;
                            }

                            /*
                            if (bb.betno.Substring(bb.betno.Length - 1)== sLuckNum[0])
                            {
                                bb.betMoney *= decimal.Parse( sLuckNum[1]);
                                if (bb.betMoney>decimal.Parse( sLuckNum[2]))
                                {
                                    bb.betMoney = decimal.Parse(sLuckNum[2]);
                                }
                            }
                            else if (bb.betno.Substring(bb.betno.Length - 2) == sLuckNum[3])
                            {
                                bb.betMoney *= decimal.Parse( sLuckNum[4]);
                                if (bb.betMoney>decimal.Parse( sLuckNum[5]))
                                {
                                    bb.betMoney = decimal.Parse(sLuckNum[5]);
                                }
                            }
                            */
                            //加钱 
                            //4.提交数据
                            bool fr = platGPK.submitToGPK(bd2,aname2);
                            if (fr)
                            {
                                string msg = string.Format("用户{0}处理，存入金额{1}，注单号{2}，游戏名称{3}：", bd2.username, bd2.betMoney, bd2.betno, bd2.gamename);
                                MyWrite(msg);
                                appSittingSet.txtLog(msg);

                            }
                            else
                            {
                                //充钱失败的情况 ？

                            }
                        }

                        //回填
                        bool r = platACT.confirmAct(bd2);
                        if (r)
                        {
                            string msg = string.Format("用户{0}处理完毕，注单号{1}，处理为 {2}，回复消息 {3}", bd2.username, bd2.betno, bd2.passed ? "通过" : "不通过", bd2.msg);
                            MyWrite(msg);
                            appSittingSet.txtLog(msg);
                        }
                        //记录到sqlite数据库
                        appSittingSet.recorderDb(bd2,aid2);

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
        /// /0 6 12 18 小时登录
        /// </summary>
        public class MyJob2 : IJob
        {
            public void Execute(IJobExecutionContext context)
            {
                //1.登录优惠大厅 一次 保存cookie 
                string msg = string.Format("活动站登录{0}{1}", platACT.loginActivity() ? "成功" : "失败", DateTime.Now.ToString("G"));
                appSittingSet.txtLog(msg);
                MyWrite(msg);

                msg = string.Format("BB站登录{0}{1}", platBB.loginBB() ? "成功" : "失败", DateTime.Now.ToString("G"));
                appSittingSet.txtLog(msg);
                MyWrite(msg);

                msg = string.Format("GPK站登录{0}{1}", platGPK.loginGPK() ? "成功" : "失败", DateTime.Now.ToString("G"));
                appSittingSet.txtLog(msg);
                MyWrite(msg);

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
                    lvRecorder.Items.Insert(0, msg.ToString());
                }));
            }
        }
        #endregion


        private void frmMain_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            MyWrite = Write;
            //先登录一遍
            MyJob2 myjob2 = new MyJob2();
            myjob2.Execute(null);
            //启动调度
            start();
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (sched2 != null)
            {
                sched2.Shutdown();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //登录一遍
            MyJob2 myjob2 = new MyJob2();
            myjob2.Execute(null);
        }
    }
}
