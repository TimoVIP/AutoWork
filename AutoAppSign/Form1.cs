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
        static string[] aname;
        static string[] Act4Stander;
        static string MaxID = "0";
        static string[] connectionString;
        static decimal rate = 0;
        static string[] FiliterGroups;
        public Form1()
        {
            InitializeComponent();
            platname = appSittingSet.readAppsettings("platname");
            interval = int.Parse(appSittingSet.readAppsettings("Interval"));
            AutoCls = appSittingSet.readAppsettings("AutoCls").Split('|');
            aname = appSittingSet.readAppsettings("aname").Split(new char[] { '|', '@' }, StringSplitOptions.RemoveEmptyEntries);
            Act4Stander = appSittingSet.readAppsettings("Act4Stander").Split(new char[] { '|', '@' }, StringSplitOptions.RemoveEmptyEntries);
            connectionString = appSittingSet.readAppsettings("MySqlConnect").Split('|');
            rate=Convert.ToDecimal( appSittingSet.readAppsettings("Rate"));
            FiliterGroups = appSittingSet.readAppsettings("FiliterGroups").Split('|');

            notify = new NotifyIcon();
            notify.BalloonTipTitle = "提示";
            notify.BalloonTipText = "程序已经最小化";
            notify.BalloonTipIcon = ToolTipIcon.Info;
            notify.Text = "程序已经最小化";
            notify.Click += Notify_Click;
            notify.Icon = new System.Drawing.Icon(Properties.Resources.favicon, 256, 256);
            notify.BalloonTipClicked += Notify_BalloonTipClicked;

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < aname.Length; i += 2)
            {
                sb.Append(" " + aname[i ]);
                sb.Append(aname[i + 1] == "1" ? "(开)" : "(关)");
            }

            this.toolStripStatusLabel1.Text = sb.ToString();
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

        private static DialogResult ShowInputDialog(ref string input,string title)
        {
            System.Drawing.Size size = new System.Drawing.Size(300, 70);
            Form inputBox = new Form();

            inputBox.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            inputBox.ClientSize = size;
            inputBox.Text = title;

            System.Windows.Forms.TextBox textBox = new TextBox();
            textBox.Size = new System.Drawing.Size(size.Width - 10, 23);
            textBox.Location = new System.Drawing.Point(5, 5);
            textBox.Text = input;
            inputBox.Controls.Add(textBox);

            Button okButton = new Button();
            okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            okButton.Name = "okButton";
            okButton.Size = new System.Drawing.Size(75, 23);
            okButton.Text = "&OK";
            okButton.Location = new System.Drawing.Point(size.Width - 80 - 80, 39);
            inputBox.Controls.Add(okButton);

            Button cancelButton = new Button();
            cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new System.Drawing.Size(75, 23);
            cancelButton.Text = "&Cancel";
            cancelButton.Location = new System.Drawing.Point(size.Width - 80, 39);
            inputBox.Controls.Add(cancelButton);

            inputBox.AcceptButton = okButton;
            inputBox.CancelButton = cancelButton;

            DialogResult result = inputBox.ShowDialog();
            input = textBox.Text;
            return result;
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
            //清除一周前的数据、日志文件
            if (AutoCls[1] == "1")
            {
                sched.ScheduleJob(JobBuilder.Create<MyJob0>().Build(), TriggerBuilder.Create().WithCronSchedule("1 0 8 1/1 * ? ").Build());
            }
            if (aname[1]=="1")
            {
                //10秒一次 app签到
                sched.ScheduleJob(JobBuilder.Create<MyJob2>().Build(), TriggerBuilder.Create().WithSimpleSchedule(x => x.WithIntervalInSeconds(interval+5).RepeatForever()).Build());
            }
            if (aname[3]=="1")
            {
                //5秒一次 快速充值
                sched.ScheduleJob(JobBuilder.Create<MyJob3>().Build(), TriggerBuilder.Create().WithSimpleSchedule(x => x.WithIntervalInSeconds(interval+4).RepeatForever()).Build());
            }
            //if (aname[5]=="1")
            //{
            //    //8秒一次 金沙组红包
            //    sched.ScheduleJob(JobBuilder.Create<MyJob4>().Build(), TriggerBuilder.Create().WithSimpleSchedule(x => x.WithIntervalInSeconds(interval ).RepeatForever()).Build());
            //}
            if (aname[5]=="1")
            {
                //15秒一次 新葡京组红包
                sched.ScheduleJob(JobBuilder.Create<MyJob5>().Build(), TriggerBuilder.Create().WithSimpleSchedule(x => x.WithIntervalInSeconds(interval).RepeatForever()).Build());
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
        ///处理app提交活动 从数据库获取数据，然后gpk后台提交充值
        /// </summary>
        [DisallowConcurrentExecution]
        public class MyJob2 : IJob
        {
            public void Execute(IJobExecutionContext context)
            {
                //查询上次处理的ID
                string sql = "select maxid from cs_max_id LIMIT 1;";
                MySQLHelper.connectionString = connectionString[0];
                MaxID =MySQLHelper.GetScalar(sql).ToString();

                //查询数据库 获取待处理的数据
                sql = string.Format("select b.tel,a.score,a.id from cs_zhangdan a LEFT JOIN cs_user b on a.uid=  b.id where type='dh' and a.`status`=1 and a.id>{0} ORDER BY a.id DESC LIMIT 100;", MaxID);
                MySQLHelper.connectionString = connectionString[0];
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
                            betTime = DateTime.Now.AddHours(12).ToString("yyyy/MM/dd HH:mm:ss"),
                            //实际先换算成美东时间 再获取所在的月份第一天
                            aname = aname[0]
                        };
                        bb= platGPK.MemberTransactionSearch(bb);
                        if (bb==null)
                        {
                            return;
                            //continue;
                        }

                        if (!bb.passed || bb.total_money<100)
                        {
                            bb.passed = false;
                            bb.msg = "存款不足";
                            MySQLHelper.connectionString = connectionString[0];
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
                            MySQLHelper.connectionString = connectionString[0];
                            MySQLHelper.ExecuteSql("update cs_max_id set maxid=" + bb.bbid + ";");
                            //拒绝
                            continue;
                        }
                        //判断 层级是否在 列表之中
                        foreach (var s in FiliterGroups)
                        {
                            if (bb.level == s)
                            {
                                bb.passed = false;
                                bb.msg = "层级不符合";
                                break;
                            }
                        }
                        if (!bb.passed)
                        {
                            MySQLHelper.connectionString = connectionString[0];
                            MySQLHelper.ExecuteSql("update cs_max_id set maxid=" + bb.bbid + ";");
                            //拒绝
                            continue;
                        }
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
                        bb.AuditType = "Discount";
                        bb.Audit = bb.betMoney;
                        bb.Memo = bb.aname;
                        bb.Type = 5;
                        bool br = platGPK.MemberDepositSubmit(bb);
                        if (br)
                        {
                            //回填通过 更改数据库 状态为2 最大Id 为当前
                            string[] sqllist = {
                                "update cs_zhangdan set `status`=2 where id= " + bb.bbid + ";",
                                "update cs_max_id set maxid="+bb.bbid+";"
                            };
                            MySQLHelper.connectionString = connectionString[0];
                            int eff1 = MySQLHelper.ExecuteSql(sqllist);

                            if (eff1<2)
                            {
                                appSittingSet.txtLog(bb.username + "编号" + bb.bbid + "更新状态失败");
                            }
                            else
                            {
                                appSittingSet.txtLog(bb.aname +"编号" + bb.bbid + "用户" + bb.username + "处理成功");
                                MyWrite(bb.aname +"编号" + bb.bbid + "用户" + bb.username + "处理成功");
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
                    MyWrite(aname[0] +"没有新的信息，等待下次执行 ");
                }
            }
        }

        /// <summary>
        /// 处理快速充值 从数据库获取数据，然后gpk后台提交充值
        /// 切记不要改线程循环 gpk会提交多次 原因不明 数据库状态未更改过来 如果是网页版 可以用线程循环
        /// </summary>
        [DisallowConcurrentExecution]
        public class MyJob3 : IJob
        {
            public void Execute(IJobExecutionContext context)
            {

                //查询数据库 获取待处理的数据
                string sql = "SELECT a.`id`, `order_no`,`username`,`order_amount`,title FROM `e_order` a left join e_bank b on a.bid=b.id WHERE a.`status`=2  ORDER BY id DESC LIMIT 100;";
                MySQLHelper.connectionString = connectionString[1];
                DataTable dt = MySQLHelper.Query(sql).Tables[0];
                if (dt.Rows.Count>0)
                {
                    betData bb = null;
                    foreach (DataRow dr in dt.Rows)
                    {
                        bb = new betData()
                        {
                            bbid = dr["id"].ToString(),
                            betMoney = Convert.ToDecimal(dr["order_amount"]),
                            username = dr["username"].ToString(),
                            AuditType = "Deposit",
                            Audit = Convert.ToDecimal(dr["order_amount"]),
                            Type = 4,//人工存入
                            isReal = true,
                            Memo = dr["title"].ToString() +"-"+ dr["order_no"].ToString(),
                            aname= aname[2],
                        };

                        //判断用户是否存在
                        Gpk_UserDetail user = platGPK.GetUserDetail(bb.username);
                        if (user==null)
                        {
                            //更改状态 为 100
                            MySQLHelper.connectionString = connectionString[0];
                            MySQLHelper.ExecuteSql("update `e_order` set `status`=100 , uid='31',handletime =unix_timestamp(now()) where id= " + bb.bbid + ";");
                            continue;
                        }

                        //加钱 充值的部分 
                        bool br = platGPK.MemberDepositSubmit(bb);
                        if (br)
                        {
                            //回填通过 更改数据库 状态为3
                            MySQLHelper.connectionString = connectionString[1];
                            int eff1= MySQLHelper.ExecuteSql("update `e_order` set `status`=3,uid='31',handletime =unix_timestamp(now()) where id= " + bb.bbid + ";");
                            appSittingSet.txtLog(bb.aname + "编号" + bb.bbid + "用户" + bb.username + (eff1 > 0 ? "处理成功" : "钱已经充值，状态更新状态失败"));
                            MyWrite(bb.aname + "编号" + bb.bbid + "用户" + bb.username + "处理" + (eff1 > 0 ? "处理成功" : "钱已经充值，状态更新状态失败"));

                            //加钱 赠送部分目前在5‰
                            bb.betMoney = bb.betMoney * rate;
                            bb.Audit = bb.betMoney * rate;
                            bb.AuditType = "Discount";
                            bb.Type = 5;//优惠活动
                            bb.Memo = "快速充值优惠";
                            br = platGPK.MemberDepositSubmit(bb);
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
                else
                {
                    //没有记录
                    MyWrite(aname[2] +"没有新的信息，等待下次执行 ");
                }
            }
        }

        /// <summary>
        /// 金沙组 红包活动 从数据库获取数据，然后gpk后台提交充值
        /// </summary>
        [DisallowConcurrentExecution]
        public class MyJob4 : IJob
        {
            public void Execute(IJobExecutionContext context)
            {

                betData bb = null;
                List<betData> list = new List<betData>();
                //查询数据库 获取待处理的数据
                string sql = "SELECT id,vipname,price FROM e_gerenjilu  WHERE status =1 or status = 101  ORDER BY id DESC LIMIT 200;";
                MySQLHelper.connectionString = connectionString[2];
                DataTable dt = MySQLHelper.Query(sql).Tables[0];
                if (dt.Rows.Count>0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        bb = new betData()
                        {
                            bbid = dr["id"].ToString(),
                            betMoney = Convert.ToDecimal(dr["price"]),
                            username = dr["vipname"].ToString(),
                            AuditType = "None",
                            Audit = Convert.ToDecimal(dr["price"]),
                            Type = 5,//优惠活动
                            isReal = false,
                            Memo = "红包活动",
                            aname= aname[4],
                        };
                        list.Add(bb);

                        //加钱 
                        //bool br = platGPK.submitToGPK(bb);
                        //if (br)
                        //{
                        //    //回填通过 更改数据库 状态为3
                        //    MySQLHelper.connectionString = connectionString[2];
                        //    int eff1 = MySQLHelper.ExecuteSql("update `e_gerenjilu` set `status`=2,addtime =unix_timestamp(now()) where id= " + bb.bbid + ";");

                        //    if (eff1 > 0)
                        //    {
                        //        appSittingSet.txtLog(bb.aname + "编号" + bb.bbid + "用户" + bb.username + "处理成功");
                        //        MyWrite(bb.aname + "编号" + bb.bbid + "用户" + bb.username + "处理成功");
                        //    }
                        //    else
                        //    {
                        //        appSittingSet.txtLog(bb.aname + bb.username + "编号" + bb.bbid + "钱已经充值，状态更新状态失败");
                        //    }
                        //}
                    }

                    Parallel.ForEach(list, (item) =>
                    {
                        //提交充值 加钱 
                        bool fr = platGPK.MemberDepositSubmit(item);
                        if (fr)
                        {
                            //回填通过 更改数据库 状态为3
                            MySQLHelper.connectionString = connectionString[2];
                            int eff1 = MySQLHelper.ExecuteSql("update `e_gerenjilu` set `status`=2,addtime =unix_timestamp(now()) where id= " + item.bbid + ";");
                            appSittingSet.txtLog(item.aname + "编号" + item.bbid + "用户" + item.username +  (eff1 > 0 ? "处理成功" : "钱已经充值，状态更新状态失败"));
                            MyWrite(item.aname + "编号" + item.bbid + "用户" + item.username + "处理" + (eff1 > 0 ? "处理成功" : "钱已经充值，状态更新状态失败"));
                        }
                        else
                        {
                            //充钱失败的情况 ？ 不做处理 下一次处理
                            return;
                        }
                    });

                }
                else
                {
                    //没有记录
                    MyWrite(aname[4] +"没有新的信息，等待下次执行 ");
                }
            }
        }

        /// <summary>
        /// 新普京组 红包活动 从数据库获取数据，然后gpk后台提交充值
        /// </summary>
        [DisallowConcurrentExecution]
        public class MyJob5 : IJob
        {
            public void Execute(IJobExecutionContext context)
            {

                betData bb = null;
                List<betData> list = new List<betData>();
                //查询数据库 获取待处理的数据 只有 0/1
                string sql = "SELECT id,username,money FROM hr_records  WHERE is_send ='0'   ORDER BY id DESC LIMIT 50;";
                MySQLHelper.connectionString = connectionString[2];
                DataTable dt = MySQLHelper.Query(sql).Tables[0];
                if (dt.Rows.Count>0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        bb = new betData()
                        {
                            bbid = dr["id"].ToString(),
                            betMoney = Convert.ToDecimal(dr["money"]),
                            username = dr["username"].ToString(),
                            AuditType = "None",
                            Audit = Convert.ToDecimal(dr["money"]),
                            Type = 5,//优惠活动
                            isReal = false,
                            Memo = "红包活动",
                            aname= aname[4],
                        };
                        list.Add(bb);
                    }

                    //线程循环
                    Parallel.ForEach(list, (item) =>
                    {
                        //提交充值 加钱 
                        bool fr = platGPK.MemberDepositSubmit(item);
                        if (fr)
                        {
                            //回填通过 更改数据库 状态为3
                            MySQLHelper.connectionString = connectionString[2];
                            int eff1 = MySQLHelper.ExecuteSql("update `hr_records` set `is_send`='1',addtime =now() where id= " + item.bbid + ";");
                            appSittingSet.txtLog(item.aname + "编号" + item.bbid + "用户" + item.username + (eff1 > 0 ? "处理成功" : "钱已经充值，状态更新状态失败"));
                            MyWrite(item.aname + "编号" + item.bbid + "用户" + item.username + "处理" + (eff1 > 0 ? "处理成功" : "钱已经充值，状态更新状态失败"));
                        }
                        else
                        {
                            //充钱失败的情况 ？ 不做处理 下一次处理
                            return;
                        }
                    });

                }
                else
                {
                    //没有记录
                    MyWrite(aname[4] +"没有新的信息，等待下次执行 ");
                }
            }
        }

        #endregion

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            //登录一遍
            MyJob1 myjob1 = new MyJob1();
            myjob1.Execute(null);
            appSittingSet.txtLog("手动操作登录");
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

        private void APP签到_Click(object sender, EventArgs e)
        {
            //查询上次处理的ID
            string sql = "select maxid from cs_max_id LIMIT 1;";
            MySQLHelper.connectionString = connectionString[0];
            string input = MySQLHelper.GetScalar(sql).ToString();
            if (ShowInputDialog(ref input, "当前处理到的ID如下") == DialogResult.OK)
            {
                //修改最大ID
                MySQLHelper.connectionString = connectionString[0];
                MySQLHelper.ExecuteSql("update cs_max_id set maxid=" + input + ";");
            }
        }

        private void 快速充值_Click(object sender, EventArgs e)
        {
            return;
            /*
            //查询上次处理的ID
            string sql = "select maxid from cs_max_id LIMIT 1;";
            MySQLHelper.connectionString = connectionString[1];
            string input = MySQLHelper.GetScalar(sql).ToString();
            if (ShowInputDialog(ref input, "当前处理到的ID如下") == DialogResult.OK)
            {
                //修改最大ID
                MySQLHelper.connectionString = connectionString[1];
                MySQLHelper.ExecuteSql("update cs_max_id set maxid=" + input + ";");
            }
            */

        }
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
