using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TimoControl;

namespace AutoDeposit
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
            toolStripStatusLabel1.Text = aname;
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

            string msg = string.Format("数据库连接{0} ", sqlHelper.ConnState() ? "成功" : "失败");
            //appSittingSet.Log(msg);
            MyWrite(msg);
            //开始调度
            start();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (sched != null)
            {
                sched.Shutdown();
            }
            //appSittingSet.sendEmail(platname + " 程序关闭", "程序关闭 ");

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
                appSittingSet.Log("手动操作登录");
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

            //5秒一次 处理入款
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
                string msg = string.Format("WSB登录{0} ", platWSB.login() ? "成功" : "失败");
                appSittingSet.Log(msg);
                MyWrite(msg);

                msg = string.Format("GPK站登录{0} ", platGPK.loginGPK() ? "成功" : "失败");
                appSittingSet.Log(msg);
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
                appSittingSet.Log("清除一周前的日志");
            }
        }


        /// <summary>
        /// 遍历 处理网页+数据库 数据
        /// 数据库 0存入客服需要改 |改后1 需要加款  |2加款完毕 3客服改了名字但是失败
        /// </summary>
        [DisallowConcurrentExecution]
        public class MyJob2 : IJob
        {
            List<betData> list = new List<betData>();
            public void Execute(IJobExecutionContext context)
            {
                Gpk_UserDetail userifo = null;

                list = platWSB.getData();
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
                foreach (var item in list)
                {
                    //1.判断用户名是否存在 
                    userifo = platGPK.GetUserDetail(item.username);
                    if (userifo == null)
                    {
                        if (item.aname == "WSB" && item.links.Length>10)
                        {
                            //来自网页 存入数据库，确认掉
                            string sql = string.Format("INSERT INTO [t_data] ([oid]  ,[username] ,[deposit]  ,[state]，[subtime] ) VALUES ('{0}' ,'{1}' ,{2} ,0,getdate())", item.betno, item.username, item.betMoney);
                            int i = sqlHelper.ExecuteNonQuery(sql);
                            bool b = platWSB.confirm(item);
                            if (b)
                            {
                                string msg = string.Format("上游订单" + item.betno + "已经确认，存入数据库，等待数据完善再处理");
                                appSittingSet.Log(msg);
                            }
                        }
                        else
                        {
                            //来自数据库 用户名填错
                            string sql = string.Format("UPDATE [t_data] SET [state] = 3 WHERE  [oid] = '{0}'", item.betno);
                            int i = sqlHelper.ExecuteNonQuery(sql);
                        }
                        continue;
                    }
                    //2.提交充值 加钱 
                    //item.aname = aname;
                    item.AuditType = "Deposit";
                    item.Audit = item.betMoney;
                    item.Memo = item.betno;
                    item.PortalMemo = item.betno;
                    item.Type = 4;
                    bool fr = platGPK.MemberDepositSubmit(item);
                    fr = true;

                    if (fr)
                    {
                        string msg = string.Format("用户{0}处理，存入金额{1}，活动名称{2} ", item.username, item.betMoney, aname);
                        MyWrite(msg);
                        appSittingSet.Log(msg);
                        if (item.aname == "DB")
                        {
                            //如果是数据库来的数据 更改数据库数据状态
                            string sql = string.Format("UPDATE [t_data] SET [state] = 2 ,subtime=getdate() WHERE  [oid] = '{0}'", item.betno);
                            int i = sqlHelper.ExecuteNonQuery(sql);
                            if (i == 0)
                            {
                                msg = string.Format("用户" + item.username + "确认失败");
                                appSittingSet.Log(msg);
                                return;
                            }
                        }
                        //如果是网页来的数据 确认掉
                        else if (item.aname == "WSB")
                        {
                            bool b = platWSB.confirm(item);
                            if (!b)
                            {
                                b = platWSB.confirm(item);

                                msg = string.Format("用户{0}确认{1}", item.username ,b);
                                //MyWrite(msg);
                                appSittingSet.Log(msg);
                                return;
                            }
                            //存入数据库，已经处理完毕
                            string sql = string.Format("INSERT INTO [t_data] ([oid]  ,[username] ,[deposit]  ,[state]，[subtime] ) VALUES ('{0}' ,'{1}' ,{2} ,2,getdate())", item.betno, item.username, item.betMoney);
                            int i = sqlHelper.ExecuteNonQuery(sql);
                            if (i == 0)
                            {
                                msg = string.Format("用户" + item.username + "确认失败");
                                appSittingSet.Log(msg);
                                return;
                            }
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
        /// 处理网页数据，然后gpk后台提交充值
        /// </summary>
        [DisallowConcurrentExecution]
        public class MyJob100 : IJob
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
                    string sql = "";
                    StringBuilder sbsql = new StringBuilder("UPDATE applyList SET status = '1' WHERE status = '0'  and  id in(");
                    //从数据库读取list 加入现有list 失败的在里面
                    list_db = platRedEnvelope.getLits_db();


                    foreach (var item in list_db)
                    {
                        //提交充值 加钱 
                        item.aname = aname;
                        item.AuditType = "Discount";
                        item.Audit = item.betMoney;
                        item.Memo = item.aname;
                        item.Type = 5;
                        bool fr = platGPK.MemberDepositSubmit(item);
                        if (fr)
                        {
                            string msg = string.Format("用户{0}处理，存入金额{1}，活动名称{2} ", item.username, item.betMoney, aname);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
                            //更改数据库标识0-1
                            sql = "UPDATE applyList SET status = '1' WHERE status = '0'  and  id =" + item.bbid;
                            b = appSittingSet.execSql(sql);
                            if (!b)
                            {
                                //数据库改状态失败 钱已经送出了 会导致送两遍
                                b = appSittingSet.execSql(sql);
                            }
                        }
                        else
                        {
                            //充钱失败的情况 ？ 不做处理 下一次处理
                            continue;
                        }
                    }
                    //去掉线程循环
                    /*
                    Parallel.ForEach(list_db, (item) =>
                    {
                        //提交充值 加钱 
                        item.aname = aname;
                        item.AuditType = "Discount";
                        item.Audit = item.betMoney;
                        item.Memo = item.aname;
                        item.Type = 5;
                        bool fr = platGPK.MemberDepositSubmit(item);
                        if (fr)
                        {
                            string msg = string.Format("用户{0}处理，存入金额{1}，活动名称{2} ", item.username, item.betMoney, aname);
                            MyWrite(msg);
                            appSittingSet.Log(msg);
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
                    sql = sbsql.ToString().Replace(",)", ")");
                    b = appSittingSet.execSql(sql);
                    if (!b)
                    {
                        //数据库改状态失败 钱已经送出了 会导致送两遍
                        b = appSittingSet.execSql(sql);
                    }
                    */

                }
            }
        }

        #endregion
    }
}
