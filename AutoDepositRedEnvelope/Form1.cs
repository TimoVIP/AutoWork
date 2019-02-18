using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TimoControl;

namespace AutoDepositRedEnvelope
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
        public Form1()
        {
            InitializeComponent();
            platname = appSittingSet.readAppsettings("platname");
            interval = int.Parse(appSittingSet.readAppsettings("Interval"));
            AutoCls = appSittingSet.readAppsettings("AutoCls").Split('|');
            aname = appSittingSet.readAppsettings("aname");

            notify = new NotifyIcon();
            notify.BalloonTipTitle = "提示";
            notify.BalloonTipText = "程序已经最小化";
            notify.BalloonTipIcon = ToolTipIcon.Info;
            notify.Text = "程序已经最小化";
            notify.Click += Notify_Click;
            notify.Icon = new System.Drawing.Icon(Properties.Resources.favicon, 256, 256);
            notify.BalloonTipClicked += Notify_BalloonTipClicked;

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

            //先登陆一次
            MyJob1 myjob1 = new MyJob1();
            myjob1.Execute(null);
            //开始调度
            start();
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
                    notify.Dispose();
                    Application.Restart();
                }
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

            //0 6 12 18 小时执行 登陆
            sched.ScheduleJob(JobBuilder.Create<MyJob1>().Build(), TriggerBuilder.Create().WithCronSchedule("0 0 0,6,12,18 * * ? ").Build());

            //5秒一次 读取提交列表
            sched.ScheduleJob(JobBuilder.Create<MyJob2>().Build(), TriggerBuilder.Create().WithSimpleSchedule(x => x.WithIntervalInSeconds(interval).RepeatForever()).Build());

            //5秒一次 读取提交列表
            //sched.ScheduleJob(JobBuilder.Create<MyJob3>().Build(), TriggerBuilder.Create().WithSimpleSchedule(x => x.WithIntervalInSeconds(interval).RepeatForever()).Build());

            //清除一周前的数据、日志文件
            if (AutoCls[1] == "1")
            {
                sched.ScheduleJob(JobBuilder.Create<MyJob01>().Build(), TriggerBuilder.Create().WithCronSchedule("1 0 8 1/1 * ? ").Build());
            }

            //开始运行
            sched.Start();
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
                string msg = string.Format("红包站登录{0} ", platRedEnvelope.login() ? "成功" : "失败");
                appSittingSet.txtLog(msg);
                MyWrite(msg);

                msg = string.Format("GPK站登录{0} ", platGPK.loginGPK() ? "成功" : "失败");
                appSittingSet.txtLog(msg);
                MyWrite(msg);
            }
        }
        /// <summary>
        /// 每天8:00:01 执行 删除一周前的日志 数据库一周前的数据
        /// </summary>
        public class MyJob01 : IJob
        {
            public void Execute(IJobExecutionContext context)
            {
                int diff = int.Parse(AutoCls[0]);
                appSittingSet.clsLogFiles(diff);
                appSittingSet.txtLog("清除一周前的日志");
            }
        }


        /// <summary>
        /// 遍历数据库新数据，处理网页数据
        /// </summary>
        [DisallowConcurrentExecution]
        public class MyJob2 : IJob
        {
            List<betData> list = new List<betData>();
            public void Execute(IJobExecutionContext context)
            {
                //获取网页等待处理的数据
                list = platRedEnvelope.getActData();
                if (list == null)
                {
                    MyWrite("没有获取到申请信息，等待下次执行 ");
                    return;
                }
                if (list.Count == 0)
                {
                    MyWrite("没有新的申请信息，等待下次执行 ");
                    return;
                }

                Parallel.ForEach(list, (item) =>
                {
                    //提交充值 加钱 
                    item.aname = aname;
                    item.AuditType = "None";
                    item.Audit = item.betMoney;
                    item.Memo = item.aname;
                    item.Type = 5;
                    bool fr = platGPK.submitToGPK(item);
                    if (fr)
                    {
                        string msg = string.Format("用户{0}处理，存入金额{1}，活动名称{2} ", item.username, item.betMoney, aname);
                        MyWrite(msg);
                        appSittingSet.txtLog(msg);
                        bool b = platRedEnvelope.confirm(item);
                        if (!b)
                        {
                            msg = string.Format("用户" + item.username + "处理失败，再次回填");
                            b = platRedEnvelope.confirm(item);
                            msg = string.Format("用户{0} 二次回填 {1}", item.username, b ? "成功" : "失败");
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
                });
            }
        }
        /// <summary>
        /// 处理网页数据，然后gpk后台提交充值
        /// </summary>
        [DisallowConcurrentExecution]
        public class MyJob3 : IJob
        {
            public void Execute(IJobExecutionContext context)
            {
                //获取网页等待处理的数据--已经入库
                List<betData> list = platRedEnvelope.getActData2DB();
                if (list == null)
                {
                    MyWrite("没有获取到申请信息，等待下次执行 ");
                    return;
                }
                if (list.Count == 0)
                {
                    MyWrite("没有新的申请信息，等待下次执行 ");
                    return;
                }
                //记录此次处理失败的list
                List<betData> list_db = new List<betData>();
                //先确认，再去gpk后台充钱
                bool b = platRedEnvelope.confirmList(list);
                if (b)
                {
                    //记录处理过的 list 中的元素 更改数据库标识0-1
                    StringBuilder sbsql = new StringBuilder("UPDATE applyList SET status = '1' WHERE status = '0'  and  id in(");
                    //从数据库读取list 加入现有list 失败的在里面
                    list_db = platRedEnvelope.getLits_db();

                    Parallel.ForEach(list_db, (item) =>
                    {
                        //提交充值 加钱 
                        item.aname = aname;
                        item.AuditType = "Discount";
                        item.Audit = item.betMoney;
                        item.Memo = item.aname;
                        item.Type = 5;
                        bool fr = platGPK.submitToGPK(item);
                        if (fr)
                        {
                            string msg = string.Format("用户{0}处理，存入金额{1}，活动名称{2} ", item.username, item.betMoney, aname);
                            MyWrite(msg);
                            appSittingSet.txtLog(msg);
                            //更改数据库标识0-1
                            sbsql.AppendFormat("'{0}',", item.bbid);
                        }
                        else
                        {
                            //充钱失败的情况 ？ 不做处理 下一次处理
                            return;
                        }
                    });

                    sbsql.Append(");");
                    b = appSittingSet.execSql(sbsql.ToString().Replace(",)", ")"));
                    if (!b)
                    {
                        //数据库改状态失败 钱已经送出了 会导致送两遍

                    }
                }
            }
        }

        #endregion
    }
}
