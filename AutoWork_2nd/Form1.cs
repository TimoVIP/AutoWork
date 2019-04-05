using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TimoControl;

namespace AutoWork_2nd
{
    public delegate void Write(string msg);//写lv消息
    public delegate void ClsListItem();
    public partial class Form1 : Form
    {
        static string platname;
        static int interval;
        NotifyIcon notify;
        static string[] AutoCls;
        static string uid;
        static string[] actInfo;
        public Form1()
        {
            InitializeComponent();
            platname = appSittingSet.readAppsettings("platname");
            interval = int.Parse(appSittingSet.readAppsettings("Interval"));
            AutoCls = appSittingSet.readAppsettings("AutoCls").Split('|');
            actInfo = appSittingSet.readAppsettings("actInfo").Split(new char[] { '|', '@' }, StringSplitOptions.RemoveEmptyEntries);
            uid = appSittingSet.readAppsettings("uid");

            notify = new NotifyIcon();
            notify.BalloonTipTitle = "提示";
            notify.BalloonTipText = "程序已经最小化";
            notify.BalloonTipIcon = ToolTipIcon.Info;
            notify.Text = "程序已经最小化";
            notify.Click += Notify_Click;
            notify.Icon = new System.Drawing.Icon(Properties.Resources.favicon, 256, 256);
            notify.BalloonTipClicked += Notify_BalloonTipClicked;

            Text = platname;

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < actInfo.Length; i += 3)
            {
                sb.Append(" " + actInfo[i + 1]);
                sb.Append(actInfo[i + 2] == "1" ? "(开)" : "(关)");
            }

            this.Text = platname;
            this.toolStripStatusLabel1.Text = sb.ToString();

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

            //5秒一次 处理数据库数据
            sched.ScheduleJob(JobBuilder.Create<MyJob2>().Build(), TriggerBuilder.Create().WithSimpleSchedule(x => x.WithIntervalInSeconds(interval).RepeatForever()).Build());

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
                string msg = string.Format("BB登录{0} ", platBB.loginBB() ? "成功" : "失败");
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

        [DisallowConcurrentExecution]
        public class MyJob2 : IJob
        {

            public void Execute(IJobExecutionContext context)
            {
                Gpk_UserDetail userifo = null;
                List<betData> list = getDataFromDB();
                bool r = false;

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
                        //不存在直接拒绝
                        item.msg = "请检查账号是否正确，申请不通过R";
                        item.passed = false;
                        r = confimToDB(item);
                        if (r)
                            MyWrite(item.username +"-"+item.msg);
                        else
                            appSittingSet.Log(item.username +"-"+ item.msg +"-处理失败");
                        continue;
                    }

                    //判断一天一次

                    //判断注单是否合法


                    //分析不同的活动 不同的条件
                    if (item.aid==actInfo[0])
                    {
                        //消除奖
                        if (actInfo[2]=="0")
                        {
                            //关闭活动 单号
                        }
                    }
                    else if (item.aid==actInfo[3])
                    {
                        //幸运尾数 单号

                    }
                    else if (item.aid==actInfo[6])
                    {
                        //以小博大 单号

                    }
                    else if (item.aid==actInfo[9])
                    {
                        //首存送礼

                    }
                    else if (item.aid==actInfo[12])
                    {
                        //体验金

                    }
                    else if (item.aid==actInfo[15])
                    {
                        //笔笔救援

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
                        string msg = string.Format("用户{0}处理，存入金额{1}，活动名称{2} ", item.username, item.betMoney, item.aname);
                        MyWrite(msg);
                        appSittingSet.Log(msg);
                        if (item.aname == "DB")
                        {
                            //如果是数据库来的数据 更改数据库数据状态
                            string sql = string.Format("UPDATE t_data SET [state] = 2 ,subtime=getdate() WHERE  oid = '{0}'", item.betno);
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

                                msg = string.Format("用户{0}确认{1}", item.username, b);
                                //MyWrite(msg);
                                appSittingSet.Log(msg);
                                return;
                            }
                            //存入数据库，已经处理完毕
                            string sql = string.Format("INSERT INTO t_data (oid  ,username ,deposit  ,state，subtime) VALUES ('{0}' ,'{1}' ,{2} ,2,getdate())", item.betno, item.username, item.betMoney);
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

        #endregion

        #region 方法
        /// <summary>
        /// 从数据库获取信息 
        /// 0 未处理 1通过 2 不通过
        /// </summary>
        /// <returns></returns>
        private static List<betData> getDataFromDB()
        {
            List<betData> list = new List<betData>();
            string sql = "SELECT id ,aid ,username ,value,  FROM_UNIXTIME(addtime) as addtime  FROM  e_submissions where status=0";
            DataTable dt = sqlHelper.ExecuteSelectDataTable(sql);

            foreach (DataRow item in dt.Rows)
            {
                list.Add(new betData() { Id =item["aid"].ToString(),   aid= item["aid"].ToString(), username = item["username"].ToString(), msg=item["value"].ToString() , betTime=item["addtime"].ToString()  });
            }
            return list;
        }

        private static bool confimToDB(betData bb)
        {
            string sql = " update e_submissions set status=@status ,uid=@uid, message=@message,handletime=getdate() where id = @id";
            int i = sqlHelper.ExecuteNonQuery(sql, new SqlParameter[] { new SqlParameter("@id", bb.Id), new SqlParameter("@status", bb.passed ? 1 : 0), new SqlParameter("@message", bb.msg),new SqlParameter("@uid", bb.Id) });
            return i > 0;
        }


        #endregion

        private void bB后台ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string msg = string.Format("BB站登录{0} ",platBB.loginBB() ? "成功" : "失败");
            appSittingSet.Log(msg);
            MyWrite(msg);
        }

        private void gPK后台ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string msg = string.Format("GPK站登录{0} ", platGPK.loginGPK() ? "成功" : "失败");
            appSittingSet.Log(msg);
            MyWrite(msg);
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            appSittingSet.showLogFile();
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            string filePath = Application.ExecutablePath + ".config";
            System.Diagnostics.Process.Start("notepad.exe", filePath);
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("应用程序重启", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1) == DialogResult.OK)
            {
                notify.Dispose();
                Application.Restart();
            }
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            ClsListItem();
        }

    }
}