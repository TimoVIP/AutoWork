using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TimoControl;

namespace AutoAppSign
{
    public delegate void Write(string msg);//写lv消息
    public delegate void ClsListItem();
    public partial class Form1 : Form
    {
        static string platname;
        static int interval;
        NotifyIcon notify;
        static string[] AutoCls;
        static string aname;
        static string[] Act4Stander;
        static string MaxID = "0";
        public Form1()
        {
            InitializeComponent();
            platname = appSittingSet.readAppsettings("platname");
            interval = int.Parse(appSittingSet.readAppsettings("Interval"));
            AutoCls = appSittingSet.readAppsettings("AutoCls").Split('|');
            aname = appSittingSet.readAppsettings("aname");
            Act4Stander = appSittingSet.readAppsettings("Act4Stander").Split(new char[] { '|', '@' }, StringSplitOptions.RemoveEmptyEntries);



            notify = new NotifyIcon();
            notify.BalloonTipTitle = "提示";
            notify.BalloonTipText = "程序已经最小化";
            notify.BalloonTipIcon = ToolTipIcon.Info;
            notify.Text = "程序已经最小化";
            notify.Click += Notify_Click;
            notify.Icon = new System.Drawing.Icon(Properties.Resources.favicon, 256, 256);
            notify.BalloonTipClicked += Notify_BalloonTipClicked;

            this.Icon = new System.Drawing.Icon(Properties.Resources.favicon, 256, 256);
            Text = platname;
        }

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

        private void Form1_Load(object sender, EventArgs e)
        {
            MyWrite = Write;
            mycls = ClsListItem;

            //////先登陆一次
            MyJob1 myjob1 = new MyJob1();
            myjob1.Execute(null);
            ////开始调度
            start();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            //登录一遍
            MyJob1 myjob1 = new MyJob1();
            myjob1.Execute(null);
            appSittingSet.txtLog("手动操作登录");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            appSittingSet.showLogFile();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string filePath = Application.ExecutablePath + ".config";
            System.Diagnostics.Process.Start("notepad.exe", filePath);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("应用程序重启", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1) == DialogResult.OK)
            {
                notify.Dispose();
                Application.Restart();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (sched != null)
            {
                sched.Shutdown();
            }
            appSittingSet.sendEmail(platname + " 程序关闭", "程序关闭 ");

            notify.Dispose();
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();   //隐藏窗体
                notify.Visible = true; //使托盘图标可见
                notify.ShowBalloonTip(6000);
            }
        }

        #region 写UI ListView 消息

        public static Write MyWrite;
        void Write(object msg)
        {
            if (lvRecorder.IsHandleCreated)
            {
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

        #endregion

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

            //加入作业调度池中

            //0 6 12 18 小时10:10执行 登陆
            sched.ScheduleJob(JobBuilder.Create<MyJob1>().Build(), TriggerBuilder.Create().WithCronSchedule("10 10 0,6,12,18 * * ? ").Build());

            //5秒一次 读取提交列表
            sched.ScheduleJob(JobBuilder.Create<MyJob2>().Build(), TriggerBuilder.Create().WithSimpleSchedule(x => x.WithIntervalInSeconds(interval).RepeatForever()).Build());

            //清除一周前的数据、日志文件
            if (AutoCls[1] == "1")
            {
                sched.ScheduleJob(JobBuilder.Create<MyJob0>().Build(), TriggerBuilder.Create().WithCronSchedule("1 0 8 1/1 * ? ").Build());
            }

            //开始运行
            sched.Start();
        }
        /// <summary>
        /// 每天8:00:01 执行 删除一周前的日志 数据库一周前的数据
        /// </summary>
        public class MyJob0 : IJob
        {
            public void Execute(IJobExecutionContext context)
            {
                int diff = int.Parse(AutoCls[0]);
                appSittingSet.clsLogFiles(diff);
                appSittingSet.txtLog("清除一周前的日志");
            }
        }

        /// <summary>
        /// /0 6 12 18 小时登录
        /// </summary>
        public class MyJob1 : IJob
        {
            public void Execute(IJobExecutionContext context)
            {
                //清除listbox 信息
                mycls();
                //string msg = string.Format("登录{0} ", platRedEnvelope.login() ? "成功" : "失败");
                //appSittingSet.txtLog(msg);
                //MyWrite(msg);

                string msg = string.Format("GPK站登录{0} ", platGPK.loginGPK() ? "成功" : "失败");
                appSittingSet.txtLog(msg);
                MyWrite(msg);
            }
        }

        /// <summary>
        /// 处理网页数据，然后gpk后台提交充值
        /// </summary>
        [DisallowConcurrentExecution]
        public class MyJob2 : IJob
        {
            public void Execute(IJobExecutionContext context)
            {
                //查询上次处理的ID
                string sql = "select maxid from cs_max_id LIMIT 1;";
                MaxID =MySQLHelper.GetScalar(sql).ToString();
                //查询数据库 获取待处理的数据
                sql = string.Format("select b.tel,a.score,a.id from cs_zhangdan a LEFT JOIN cs_user b on a.uid=  b.id where type='dh' and a.`status`=1 and a.id>{0} ORDER BY a.id DESC LIMIT 100;", MaxID);
                DataTable dt = MySQLHelper.Query(sql).Tables[0];
                if (dt.Rows.Count>0)
                {
                    betData bb = null;
                    foreach (DataRow dr in dt.Rows)
                    {
                        //去gpk 比较数据 是否符合条件
                        //当月存款大于100
                        bb = new betData()
                        {
                            bbid = dr["id"].ToString(),
                            wallet = dr["score"].ToString(),
                            username = dr["tel"].ToString(),
                            lastOprTime = DateTime.Now.AddDays(-DateTime.Now.Day + 1).ToString("yyyy/MM/dd") + " 12:00:00",
                            betTime = DateTime.Now.AddHours(12).ToString("yyyy/MM/dd HH:mm:ss")
                            //实际先换算成美东时间 再获取所在的月份第一天

                        };
                        bb= platGPK.checkInGPK_transaction(bb);
                        if (bb==null)
                        {
                            return;
                            //continue;
                        }

                        if (!bb.passed || bb.total_money<100)
                        {
                            bb.passed = false;
                            bb.msg = "存款不足";
                            MySQLHelper.ExecuteSql("update cs_max_id set maxid=" + bb.bbid + ";");
                            //拒绝
                            continue;
                        }
                        //当月电子有效投注500以上
                        bb = platGPK.GetDetailInfo(bb);
                        if (bb==null)
                        {
                            return;
                            //continue;
                        }
                        if (!bb.passed || bb.total_money<500)
                        {
                            bb.passed = false;
                            bb.msg = "投注不足";
                            MySQLHelper.ExecuteSql("update cs_max_id set maxid=" + bb.bbid + ";");
                            //拒绝
                            continue;
                        }
                        //if (!bb.passed)
                        //{
                        //    MySQLHelper.ExecuteSql("update cs_max_id set maxid=" + bb.bbid + ";");
                        //    continue;
                        //}
                        //计算 需要加钱 的数字 
                        for (int i = Act4Stander.Length - 1; i > 0; i -= 2)
                        {
                            if (bb.wallet.ToString() == Act4Stander[i - 1])
                            {
                                bb.betMoney = decimal.Parse(Act4Stander[i]);
                                break;
                            }
                        }
                        //加钱
                        bool br = platGPK.submitToGPK(bb, aname);
                        if (br)
                        {
                            //回填通过 更改数据库 状态为2 最大Id 为当前
                            string[] sqllist = {
                                "update cs_zhangdan set `status`=2 where id= " + bb.bbid + ";",
                                "update cs_max_id set maxid="+bb.bbid+";"
                            };
                            int eff1 = MySQLHelper.ExecuteSql(sqllist);

                            if (eff1<2)
                            {
                                appSittingSet.txtLog(bb.username + "编号" + bb.bbid + "更新状态失败");
                            }
                            else
                            {
                                appSittingSet.txtLog("编号" + bb.bbid + "用户" + bb.username + "处理成功");
                                MyWrite("编号" + bb.bbid + "用户" + bb.username + "处理成功");
                            }
                            //MaxID = bb.bbid;
                        }
                        //else
                        //{
                        //    //加钱失败
                        //    return;
                        //}

                    }
                    //更新最大id
                    //int eff= MySQLHelper.ExecuteSql("update cs_max_id set maxid="+MaxID+";");
                    //if (eff == 0)
                    //{
                    //    appSittingSet.txtLog("更新最大编号失败"+MaxID);
                    //}
                }
                else
                {
                    //没有记录
                    MyWrite("没有新的信息，等待下次执行 ");
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {

            //string[] UserInfo = appSittingSet.readAppsettings("UserInfo").Split('|');
            //string s = appSittingSet.readAppsettings("GPK");
            //ConfigurationOperator cc = new ConfigurationOperator();
            //            string s = cc.ReadAppSetting("GPK");
            //bool b = appSittingSet.recorderDbCheck(string.Format("SELECT id FROM User  WHERE  UserName = '{0}' AND  UserPwd = '{1}' AND  Status = '0' and date(ExpireDate) >  date('now');", UserInfo[0], UserInfo[1]));



            //betData bb = new betData()
            //{
            //    bbid = "329267",
            //    wallet = "100",
            //    username = "yuegui123",
            //    lastOprTime = DateTime.Now.AddDays(-DateTime.Now.Day + 1).ToString("yyyy/MM/dd") + " 12:00:00",
            //    //betTime = DateTime.Now.AddHours(12).ToString("yyyy/MM/dd HH:mm:ss")
            //};
            //bb = platGPK.checkInGPK_transaction(bb);




        }


        #endregion

    }

    class Exchange
    {
        int ID { get; set; }
        int UID { get; set; }
        int GID { get; set; }
        /// <summary>
        /// 已经处理1 未处理2
        /// </summary>
        int Status { get; set; }

    }
}
