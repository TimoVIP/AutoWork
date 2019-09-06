using BaseFun;
using MySQLHelper;
using Quartz;
using Quartz.Impl;
using SQLiteHelper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using TimoControl;

namespace AutoAppSign
{
    public delegate void Write(string msg);//写lv消息
    public delegate void ClsListItem();
    public partial class Form1 : Form
    {
        static string platname;
        static int[] interval;
        NotifyIcon notify;
        static string[] AutoCls;
        static string[] aname;
        static string[] Act4Stander;
        static string[] connectionString;
        static decimal rate = 0;
        static string[] FiliterGroups;
        static string uid;
        //static string sql_hb_select;
        //static string sql_hb_upadte;
        static string[] KindCategories;
        static Hashtable myConfig;
        public Form1()
        {
            InitializeComponent();
            myConfig = appSittingSet.readConfig();

            platname = myConfig["platname"].ToString();

            interval = Array.ConvertAll(myConfig["Interval"].ToString().Split('|'), int.Parse);
            AutoCls = myConfig["AutoCls"].ToString().Split('|');
            aname = myConfig["aname"].ToString().Split(new char[] { '|', '@' }, StringSplitOptions.RemoveEmptyEntries);
            Act4Stander = myConfig["Act4Stander"].ToString().Split(new char[] { '|', '@' }, StringSplitOptions.RemoveEmptyEntries);
            connectionString = myConfig["MySqlConnect"].ToString().Split('|');
            rate=Convert.ToDecimal(myConfig["Rate"]);
            FiliterGroups = myConfig["FiliterGroups"].ToString().Split('|');
            uid = myConfig["uid"].ToString();
            //sql_hb_select = myConfig["sql_hb_select"].ToString();
            //sql_hb_upadte = myConfig["sql_hb_upadte"].ToString();

            KindCategories = myConfig["KindCategories"].ToString().Split('|');//游戏分类

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
            MyJobLogin myjob1 = new MyJobLogin();
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
            //加入作业调度池中
            sched = schedf.GetScheduler();
            //清除一周前的数据、日志文件
            if (AutoCls[1] == "1")
            {
                sched.ScheduleJob(JobBuilder.Create<MyJobCls>().Build(), TriggerBuilder.Create().WithCronSchedule("1 0 8 1/1 * ? ").Build());
            }
            //0 6 12 18 小时10:10执行 登陆
            if (AutoCls[2] == "1")
            {
                sched.ScheduleJob(JobBuilder.Create<MyJobLogin>().Build(), TriggerBuilder.Create().WithCronSchedule("10 10 0,6,12,18 * * ? ").Build());
            }


            if (aname[1]=="1")
            {
                //app签到
                sched.ScheduleJob(JobBuilder.Create<MyJob1>().Build(), TriggerBuilder.Create().WithSimpleSchedule(x => x.WithIntervalInSeconds(interval[0]).RepeatForever()).Build());
                //if (myConfig["platno"].ToString()!="3")
                //{
                //    //初始化app签到数据 重要
                //    MySQLHelper.connectionString = connectionString[0];
                //    MySQLHelper.MySQLHelper.ExecuteSql("update cs_zhangdan set apply=1 WHERE type='dh' and apply=0 ;");
                //}

            }
            if (aname[3]=="1")
            {
                //快速充值
                sched.ScheduleJob(JobBuilder.Create<MyJob2>().Build(), TriggerBuilder.Create().WithSimpleSchedule(x => x.WithIntervalInSeconds(interval[1]).RepeatForever()).Build());
            }
            if (aname[5]=="1")
            {
                //红包
                sched.ScheduleJob(JobBuilder.Create<MyJob3>().Build(), TriggerBuilder.Create().WithSimpleSchedule(x => x.WithIntervalInSeconds(interval[2]).RepeatForever()).Build());
            }
            //新红包平台
            if (aname[7]=="1")
            {
                //红包
                sched.ScheduleJob(JobBuilder.Create<MyJob4>().Build(), TriggerBuilder.Create().WithSimpleSchedule(x => x.WithIntervalInSeconds(interval[3]).RepeatForever()).Build());
            }
            //开始运行
            sched.Start();
        }
        /// <summary>
        /// 每天8:00:01 执行 删除一周前的日志 数据库一周前的数据
        /// </summary>
        public class MyJobCls : IJob
        {
            public void Execute(IJobExecutionContext context)
            {
                int diff = int.Parse(AutoCls[0]);
                appSittingSet.clsLogFiles(diff);
                appSittingSet.Log("清除一周前的日志");

                //SQLiteHelper.SQLiteHelper.execSql("delete  from record where type=1003 and  addtime <date(CURRENT_TIMESTAMP,'localtime')");
                string sql = "delete from record where addtime < '" + DateTime.Now.AddDays(-diff).Date.ToString("yyyy-MM-dd") + "'";
                SQLiteHelper.SQLiteHelper.execSql(sql);
            }
        }

        /// <summary>
        /// /0 6 12 18 小时登录
        /// </summary>
        public class MyJobLogin : IJob
        {
            public void Execute(IJobExecutionContext context)
            {
                //清除listbox 信息
                mycls();
                //string msg = string.Format("登录{0} ", platRedEnvelope.login() ? "成功" : "失败");
                //appSittingSet.txtLog(msg);
                //MyWrite(msg);

                string msg = string.Format("GPK站登录{0} ", platGPK.loginGPK() ? "成功" : "失败");
                appSittingSet.Log(msg);
                MyWrite(msg);
            }
        }

        /// <summary>
        ///处理app 签到 积分兑换  提交活动 从数据库获取数据，然后gpk后台提交充值
        /// </summary>
        [DisallowConcurrentExecution]
        public class MyJob1 : IJob
        {
            public void Execute(IJobExecutionContext context)
            {
                //更改状态 status 1未处理默认，2已经处理 不符合的也是1  ， 但是apply 默认为0，处理后为1（手动处理的数据 0 ）
                MySQLHelper.MySQLHelper.connectionString = connectionString[0];
                //查询数据库 获取待处理的数据
                DataTable dt = MySQLHelper.MySQLHelper.Query(myConfig["sql_qd_select"].ToString()).Tables[0];
                if (dt.Rows.Count < 1)
                {
                    //没有记录
                    MyWrite(aname[0] + "没有新的信息，等待下次执行 ");
                    //SQLiteHelper.SQLiteHelper.execSql("delete  from record where type=1003 and  addtime <date(CURRENT_TIMESTAMP,'localtime')");
                    return;
                }

                //先把dt 的内容 放入list 里面 去重
                List<betData> list = new List<betData>();
                foreach (DataRow dr in dt.Rows)
                {
                    //去重复 如果不存在 加到list
                    string re = SQLiteHelper.SQLiteHelper.execScalarSql($"select status from record where  bbid={dr[0].ToString()}  and type=1003;");
                    if (re=="")
                    {
                        betData b = new betData()
                        {
                            bbid = dr[0].ToString(),
                            username = dr[1].ToString(),
                            wallet = dr[2].ToString(),
                            aname = aname[0],
                            Memo="积分兑换-"+ dr[0].ToString(),
                        };
                        list.Add(b);
                    }
                    else if(re=="1")
                    {
                        MySQLHelper.MySQLHelper.ExecuteSql(string.Format(myConfig["sql_qd_upadte_succeed"].ToString(), dr[0].ToString()));
                    }
                    else
                    {
                        MySQLHelper.MySQLHelper.ExecuteSql(string.Format(myConfig["sql_qd_upadte_failure"].ToString(), dr[0].ToString()));
                    }
                }

                /*
                //线程循环
                Parallel.ForEach(list, (item) =>
                {

                    //SQLite数据库是否存在
                    sql = string.Format("select * from record where  bbid={0}  and type=1003", item.bbid);
                    if (appSittingSet.recorderDbCheck(sql))
                    {
                        //更改为status2 已处理
                        sql = "update cs_zhangdan set status=2,apply=1 where id= " + item.bbid + ";";
                        MySQLHelper.MySQLHelper.ExecuteSql(sql);
                        return;
                        //continue;
                    }

                    //当月存款大于100
                    item.lastOprTime = DateTime.Now.AddDays(-DateTime.Now.Day + 1).ToString("yyyy/MM/dd") + " 12:00:00";
                    item.betTime = DateTime.Now.AddHours(12).ToString("yyyy/MM/dd HH:mm:ss");
                    item = platGPK.MemberTransactionSearch(item);
                    if (item == null)
                    {
                        return;
                        //continue;
                    }

                    if (!item.passed || item.total_money < 100)
                    {
                        item.passed = false;
                        item.msg = "存款不足";
                        sql = "update cs_zhangdan set   apply=1 where id=" + item.bbid + ";";
                        MySQLHelper.MySQLHelper.ExecuteSql(sql);
                        //拒绝
                        //continue;
                        return;
                    }

                    //当月电子有效投注500以上
                    item = platGPK.GetDetailInfo(item);
                    if (item == null)
                    {
                        return;
                        //continue;
                    }
                    if (!item.passed || item.total_money < 500)
                    {
                        item.passed = false;
                        item.msg = "投注不足";
                        sql = "update cs_zhangdan set  apply=1 where id=" + item.bbid + ";";
                        MySQLHelper.MySQLHelper.ExecuteSql(sql);
                        //拒绝
                        //continue;
                        return;
                    }

                    //判断 层级是否在 列表之中
                    foreach (var s in FiliterGroups)
                    {
                        if (item.level == s)
                        {
                            item.passed = false;
                            item.msg = "层级不符合";
                            break;
                        }
                    }
                    if (!item.passed)
                    {
                        sql = "update cs_zhangdan set apply=1 where id=" + item.bbid + ";";
                        MySQLHelper.MySQLHelper.ExecuteSql(sql);
                        //拒绝
                        //continue;
                        return;
                    }

                    //计算 需要加钱 的数字 
                    for (int i = Act4Stander.Length - 1; i > 0; i -= 2)
                    {
                        if (item.wallet.ToString() == Act4Stander[i - 1])
                        {
                            item.betMoney = decimal.Parse(Act4Stander[i]);
                            break;
                        }
                    }

                    //加钱
                    item.AuditType = "Discount";
                    item.Audit = item.betMoney;
                    item.Memo = item.aname + item.bbid;
                    item.Type = 5;
                    bool b = false;

                    //充钱
                    b = platGPK.MemberDepositSubmit(item);
                    if (b)
                    {
                        //更改状态 回填
                        sql = "update cs_zhangdan set status=2,apply=1 where id= " + item.bbid + ";";
                        MySQLHelper.MySQLHelper.ExecuteSql(sql);
                        //记录到数据库SQLite
                        sql = string.Format("insert into record values({0},'{1}','{2}',datetime(CURRENT_TIMESTAMP,'localtime'),1003)  ", item.bbid, bb.Memo, item.username);
                        SQLiteHelper.SQLiteHelper.execSql(sql);
                    }

                    //写消息
                    appSittingSet.Log(item.aname + "编号" + item.bbid + "用户" + item.username + "处理成功");
                    MyWrite(item.aname + "编号" + item.bbid + "用户" + item.username + "处理成功");
                });

                */
                foreach (var item in list)
                {
                    betData bb = item;
                    string log;
                    //SQLite数据库是否存在
                    //if (appSittingSet.recorderDbCheck($"select * from record where  bbid={bb.bbid}  and type=1003"))
                    //{
                    //    //更改为status2 已处理
                    //    bb.msg = "已经处理过";
                    //    bb.passed = false;
                    //    sql = "update cs_zhangdan set status=2,apply=1 where id= " + bb.bbid + ";";
                    //    MySQLHelper.MySQLHelper.ExecuteSql(sql);
                    //    log = string.Format("{4}编号{0}用户{1}处理为{2}{3}", bb.bbid, bb.username, bb.passed ? "通过" : "不通过", bb.msg,bb.aname);
                    //    appSittingSet.Log(log);
                    //    MyWrite(log);
                    //    //拒绝 重新查datatable
                    //    //return;
                    //    continue;
                    //}

                    //当月存款大于100
                    item.lastOprTime = DateTime.Now.AddDays(-DateTime.Now.Day + 1).ToString("yyyy/MM/dd") + " 12:00:00";
                    item.betTime = DateTime.Now.AddHours(12).ToString("yyyy/MM/dd HH:mm:ss");
                    bb = platGPK.MemberTransactionSearch(item);
                    if (bb == null)
                    {
                        //return;
                        continue;
                    }

                    if (!bb.passed || bb.total_money < 100)
                    {
                        bb.passed = false;
                        bb.msg = "存款不足";
                        //sql = $"update cs_zhangdan set   apply=1 where id={bb.bbid};";
                        //sql = string.Format("update qd_bonus_record set distribute_status = 2 where id = {0}", bb.bbid);
                        //$"update qd_bonus_record set distribute_status = 2 where id = { bb.bbid }";

                        int eff = MySQLHelper.MySQLHelper.ExecuteSql(string.Format(myConfig["sql_qd_upadte_failure"].ToString(), bb.bbid));
                        //记录到数据库SQLite 失败状态
                        SQLiteHelper.SQLiteHelper.execSql($"insert into record values({bb.bbid},'{bb.Memo}','{bb.username}',datetime(CURRENT_TIMESTAMP,'localtime'),1003,0);");
                        log = $"{bb.aname}-编号-{bb.bbid}-用户-{bb.username}-处理为-{(bb.passed ? "通过" : "不通过")}-{bb.msg}";
                        appSittingSet.Log(log);
                        MyWrite(log);
                        //拒绝
                        continue;
                        //return;
                    }

                    //当月电子有效投注500以上
                    bb.gamename = null;
                    bb.lastCashTime = DateTime.Now.AddDays(-DateTime.Now.Day + 1).ToString("yyyy/MM/dd");
                    bb.lastOprTime = DateTime.Now.Date.ToString("yyyy/MM/dd");
                    foreach (var s in KindCategories)
                    {
                        bb.gamename += "&types=" + string.Join("&types=", platGPK.KindCategories[int.Parse(s)].Replace("\"", "").Split(',').Skip(1).ToArray());
                    }
                    bb = platGPK.GetDetailInfo(bb);

                    if (bb == null)
                    {
                        return;
                        //continue;
                    }
                    if (!bb.passed || bb.total_money < 500)
                    {
                        bb.passed = false;
                        bb.msg = "投注不足";
                        //sql = "update cs_zhangdan set  apply=1 where id=" + bb.bbid + ";";
                        MySQLHelper.MySQLHelper.ExecuteSql(string.Format(myConfig["sql_qd_upadte_failure"].ToString(), bb.bbid));
                        //记录到数据库SQLite 失败状态
                        SQLiteHelper.SQLiteHelper.execSql($"insert into record values({bb.bbid},'{bb.Memo}','{bb.username}',datetime(CURRENT_TIMESTAMP,'localtime'),1003,0);");
                        log = $"{bb.aname}-编号-{bb.bbid}-用户-{bb.username}-处理为-{(bb.passed ? "通过" : "不通过")}-{bb.msg}";
                        appSittingSet.Log(log);
                        MyWrite(log);
                        //拒绝
                        continue;
                        //return;
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
                        //sql = "update cs_zhangdan set apply=1 where id=" + bb.bbid + ";";
                        //MySQLHelper.MySQLHelper.ExecuteSql(sql);
                        MySQLHelper.MySQLHelper.ExecuteSql(string.Format(myConfig["sql_qd_upadte_failure"].ToString(), bb.bbid));
                        //记录到数据库SQLite 失败状态
                        SQLiteHelper.SQLiteHelper.execSql($"insert into record values({bb.bbid},'{bb.Memo}','{bb.username}',datetime(CURRENT_TIMESTAMP,'localtime'),1003,0);");
                        log = $"{bb.aname}-编号-{bb.bbid}-用户-{bb.username}-处理为-{(bb.passed ? "通过" : "不通过")}-{bb.msg}";
                        appSittingSet.Log(log);
                        MyWrite(log);
                        //拒绝
                        continue;
                        //return;
                    }

                    //计算 需要加钱 的数字 
                    bb.wallet = Math.Abs(int.Parse(bb.wallet)).ToString();
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
                    bb.Memo = bb.aname + bb.bbid;
                    bb.Type = 5;
                    //bb.passed = true;
                    bool b = false;
                    bb.msg = "充值";
                    //先查询一下 优惠活动的记录是否有送 如果有记录就不送
                    //bb.isReal = false;
                    //bb.Types = new string[] { "Bonus" };
                    //bb.lastOprTime = DateTime.Now.ToString();
                    //bb.betTime = null;
                    //platGPK.MemberTransactionSearch(bb);
                    //string memo  = bb.PortalMemo;





                    //加钱 充值的部分 
                    b = platGPK.MemberDepositSubmit(bb);
                    MySQLHelper.MySQLHelper.ExecuteSql(string.Format(myConfig["sql_qd_upadte_succeed"].ToString(), bb.bbid));
                    //记录到数据库SQLite 成功
                    string sql = $"insert into record values({bb.bbid},'{bb.Memo}','{bb.username}',datetime(CURRENT_TIMESTAMP,'localtime'),1003,1);";
                    SQLiteHelper.SQLiteHelper.execSql(sql);
                    log = $"{bb.aname}-编号-{bb.bbid}-用户-{bb.username}-处理为-{(bb.passed ? "通过" : "不通过")}-{bb.msg}";
                    appSittingSet.Log(log);
                    MyWrite(log);

                    /*
                    sql = "select * from cs_zhangdan where apply=1 and id= " + bb.bbid + ";";
                    b = MySQLHelper.Exsist(sql);
                    if (!b)
                    {
                        b = platGPK.MemberDepositSubmit(bb);
                        //更改状态 回填
                        sql = "update cs_zhangdan set status=2,apply=1 where id= " + bb.bbid + ";";
                        MySQLHelper.MySQLHelper.ExecuteSql(sql);
                    }

                    //记录到数据库SQLite
                    sql = string.Format("insert into record values({0},'{1}','{2}',datetime(CURRENT_TIMESTAMP,'localtime'),1003)  ", bb.bbid, bb.Memo, bb.username);
                    SQLiteHelper.SQLiteHelper.execSql(sql);

                    log = string.Format("{4}编号{0}用户{1}处理为{2}{3}", bb.bbid, bb.username, bb.passed ? "通过" : "不通过", bb.msg, bb.aname);
                    appSittingSet.Log(log);
                    MyWrite(log);

                */
                }
            }
        }

        /// <summary>
        /// 处理快速充值 从数据库获取数据，然后gpk后台提交充值
        /// 切记不要改线程循环 gpk会提交多次 原因不明 数据库状态未更改过来 如果是网页版 可以用线程循环
        /// </summary>
        [DisallowConcurrentExecution]
        public class MyJob2 : IJob
        {
            public void Execute(IJobExecutionContext context)
            {
                MySQLHelper.MySQLHelper.connectionString = connectionString[1];
                string sql = "";
                //查询数据库 获取待处理的数据
                //string sql = "SELECT a.`id`, `order_no`,`username`,`order_amount`,title,addtime FROM `e_order` a left join e_bank b on a.bid=b.id WHERE a.`status`=2  ORDER BY id DESC LIMIT 100;";
                DataTable dt = MySQLHelper.MySQLHelper.Query(myConfig["sql_cz_select"].ToString()).Tables[0];

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
                            Memo = dr["title"].ToString() + "-" + dr["order_no"].ToString(),
                            aname = aname[2],
                            betTime = dr["addtime"].ToString(),
                        };

                        //判断用户是否存在
                        //betData bbt = new betData() {username = "yang133983" };//测试
                        Gpk_UserDetail user = platGPK.GetUserDetail(bb.username);
                        if (user == null)
                        {
                            //更改状态 为 100
                            //sql = "update `e_order` set `status`=100 , uid='" + uid + "',handletime =unix_timestamp(now()) where id= " + bb.bbid + ";";
                            //sql = $"update `e_order` set `status`=100 , uid='{uid}',handletime =unix_timestamp(now()) where id= {bb.bbid};";
                            //三号台子
                            //sql = "update `e_order` set `status`=100 , uid='" + uid + "',handletime =unix_timestamp(date_add(now(), interval 12 hour)) where id= " + bb.bbid + ";";
                            sql = string.Format(myConfig["sql_cz_upadte"].ToString(), uid, bb.bbid, 100);
                            int eff1 = MySQLHelper.MySQLHelper.ExecuteSql(sql);
                            continue;
                        }


                        //看数据库是否存在
                        //sql = $"select * from record where   username='{bb.username}' and bbid='{bb.bbid}'  and type=1001;";
                        sql = $"select * from record where  bbid='{bb.bbid}'  and type=1001;";
                        if (SQLiteHelper.SQLiteHelper.recorderDbCheck(sql))
                        {
                            //存在 改状态为通过 
                            sql = string.Format(myConfig["sql_cz_upadte"].ToString(), uid, bb.bbid, 3);
                            MySQLHelper.MySQLHelper.ExecuteSql(sql);
                            continue;
                        }


                        //记录到数据库
                        sql = $"insert into record values({bb.bbid},'{bb.Memo}','{bb.username}',datetime(CURRENT_TIMESTAMP,'localtime'),1001,1);";
                        SQLiteHelper.SQLiteHelper.execSql(sql);
                        //回填通过 更改数据库 状态为3
                        //sql = "update `e_order` set `status`=3,uid='" + uid + "',handletime =unix_timestamp(now()) where id= " + bb.bbid + ";";
                        //3号台子 需要+12小时
                        //sql = "update `e_order` set `status`=3,uid='" + uid + "',handletime =unix_timestamp(date_add(now(), interval 12 hour)) where id= " + bb.bbid + ";";
                        sql = string.Format(myConfig["sql_cz_upadte"].ToString(), uid, bb.bbid, 3);
                        int eff = MySQLHelper.MySQLHelper.ExecuteSql(sql);

                        string msg = bb.aname + "编号" + bb.bbid + "用户" + bb.username + (eff > 0 ? "处理成功" : "钱已经充值，状态更新状态失败");
                        appSittingSet.Log(msg);
                        MyWrite(msg);

                        //加钱 充值的部分 
                       bool br = platGPK.MemberDepositSubmit(bb);

                        //加钱 赠送部分目前在5‰
                        bb.betMoney = bb.betMoney * rate;
                        bb.Audit = bb.betMoney * rate;
                        bb.AuditType = "Discount";
                        bb.Type = 5;//优惠活动
                        bb.Memo = "快速充值优惠";
                        br = platGPK.MemberDepositSubmit(bb);
                        //if (br)
                        //{
                        //    br = platGPK.MemberDepositSubmit(bb);
                        //}

                    }
                }
                else
                {
                    //没有记录
                    MyWrite(aname[2] +"没有新的信息，等待下次执行 ");
                    SQLiteHelper.SQLiteHelper.execSql("delete  from record where type=1001");
                }
            }
        }

        #region 不用代码
        /// <summary>
        /// 红包活动 从数据库获取数据，然后gpk后台提交充值
        /// </summary>
        [DisallowConcurrentExecution]
        public class MyJob5 : IJob
        {
            public void Execute(IJobExecutionContext context)
            {
                MySQLHelper.MySQLHelper.connectionString = connectionString[2];
                betData bb = null;
                List<betData> list = new List<betData>();
                //查询数据库 获取待处理的数据 只有 0/1

                //string sql_cz_select = "SELECT id,username,money FROM hr_records  WHERE is_send ='0'   ORDER BY id  LIMIT 50;";
                //string sql_cz_upadte_ = sql_hb_upadte;

                //3组需要加上12小时
                //string sql_update = "update `hr_records` set `is_send`='1',addtime =date_add(now(), interval 12 hour) where id in({0});";
                //时间计算错误更正语句
                //update hr_records set addtime=date_sub(addtime, interval 12 hour) where addtime> now();
                string ids = "";
                string users = "";
                int count = 0;
                DataTable dt = MySQLHelper.MySQLHelper.Query(myConfig["sql_hb_select"].ToString()).Tables[0];
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

                        //sql_update += bb.bbid + ",";


                        //list.Add(bb);

                        //提交充值 加钱 
                        bool br = true;
                        //看数据库是否存在
                        //if (!appSittingSet.recorderDbCheck(string.Format("select * from record where   username='{0}' and bbid='{1}'  and type=1002", bb.username, bb.bbid)))
                        //{
                        //    //加钱 充值的部分 
                        //    br = platGPK.MemberDepositSubmit(bb);
                        //}
                        bool fr = platGPK.MemberDepositSubmit(bb);
                        //bool fr = true;
                        if (br)
                        {
                            ids += bb.bbid + ",";
                            count += 1;
                            users += "编号" + bb.bbid + "用户" + bb.username + ",";
                            //记录到数据库SQLite
                            //SQLiteHelper.SQLiteHelper.execSql(string.Format("insert into record values({0},'{1}','{2}',datetime(CURRENT_TIMESTAMP,'localtime'),1002)  ", bb.bbid, bb.Memo, bb.username));
                            ////回填通过 更改数据库 状态为3
                            //sql = "update `hr_records` set `is_send`='1',addtime =now() where id= " + bb.bbid + ";";
                            ////3号平台需要加12小时
                            ////sql = "update `hr_records` set `is_send`='1',addtime =date_add(now(), interval 12 hour) where id= " + item.bbid + ";";
                            //int eff1 = MySQLHelper.MySQLHelper.ExecuteSql(sql);
                            //appSittingSet.Log(bb.aname + "编号" + bb.bbid + "用户" + bb.username + (eff1 > 0 ? "处理成功" : "钱已经充值，状态更新状态失败"));
                            //MyWrite(bb.aname + "编号" + bb.bbid + "用户" + bb.username + "处理" + (eff1 > 0 ? "处理成功" : "钱已经充值，状态更新状态失败"));
                        }
                        else
                        {
                            //充钱失败的情况 ？ 不做处理 下一次处理
                            return;
                        }
                    }

                    //sql_cz_upadte_ = string.Format(myConfig["sql_hb_upadte"].ToString(),  ids.TrimEnd(','));
                    int eff = MySQLHelper.MySQLHelper.ExecuteSql(string.Format(myConfig["sql_hb_upadte"].ToString(), ids.TrimEnd(',')));
                    if (eff==count)
                    {
                        appSittingSet.Log(bb.aname+users+ "处理成功" );
                        MyWrite(bb.aname + users + "处理成功");
                    }
                    else
                    {
                        //处理数据库有问题
                        appSittingSet.Log(bb.aname + "不等-" + users + "处理成功" );
                        MyWrite(bb.aname + "不等-" + users + "处理成功");
                    }


                    /*
                    //线程循环
                    Parallel.ForEach(list, (item) =>
                    {
                        //提交充值 加钱 
                        bool fr = platGPK.MemberDepositSubmit(item);
                        if (fr)
                        {
                            //回填通过 更改数据库 状态为3
                            sql = "update `hr_records` set `is_send`='1',addtime =now() where id= " + item.bbid + ";";
                            //3号平台需要加12小时
                            //sql = "update `hr_records` set `is_send`='1',addtime =date_add(now(), interval 12 hour) where id= " + item.bbid + ";";
                            int eff1 = MySQLHelper.MySQLHelper.ExecuteSql(sql);
                            appSittingSet.Log(item.aname + "编号" + item.bbid + "用户" + item.username + (eff1 > 0 ? "处理成功" : "钱已经充值，状态更新状态失败"));
                            MyWrite(item.aname + "编号" + item.bbid + "用户" + item.username + "处理" + (eff1 > 0 ? "处理成功" : "钱已经充值，状态更新状态失败"));
                        }
                        else
                        {
                            //充钱失败的情况 ？ 不做处理 下一次处理
                            return;
                        }
                    });
                    */
                }
                else
                {
                    //没有记录
                    MyWrite(aname[4] +"没有新的信息，等待下次执行 ");
                    SQLiteHelper.SQLiteHelper.execSql("delete  from record where type=1002");
                }
            }
        }
        #endregion

        /// <summary>
        /// 红包，线程来做 只需要配置sql语句即可
        /// </summary>
        [DisallowConcurrentExecution]
        public class MyJob3 : IJob
        {
            public void Execute(IJobExecutionContext context)
            {
                MySQLHelper.MySQLHelper.connectionString = connectionString[2];
                List<betData> list = new List<betData>();
                //List<string> openw = new List<string>();
                DataTable dt = MySQLHelper.MySQLHelper.Query(myConfig["sql_hb_select"].ToString()).Tables[0];
                if (dt.Rows.Count == 0)
                {
                    //没有记录
                    MyWrite(aname[4] + "没有新的信息，等待下次执行 ");
                    SQLiteHelper.SQLiteHelper.execSql("delete  from record where type=1002");
                    return;
                }

                foreach (DataRow dr in dt.Rows)
                {
                    //去重复 如果不存在 加到list
                    if (!SQLiteHelper.SQLiteHelper.recorderDbCheck($"select * from record where  bbid={dr[0].ToString()}  and type=1002;"))
                    {
                        betData bb = new betData()
                        {
                            bbid = dr[0].ToString(),
                            betMoney = Convert.ToDecimal(dr[2]),
                            username = dr[1].ToString(),
                            AuditType = "None",
                            Audit = Convert.ToDecimal(dr[2]),
                            Type = 5,//优惠活动
                            isReal = false,
                            Memo = "红包活动-" + dr[0].ToString(),
                            aname = aname[4],
                        };
                        list.Add(bb);
                    }
                    else
                    {
                        MySQLHelper.MySQLHelper.ExecuteSql(string.Format(myConfig["sql_hb_upadte"].ToString(), dr[0].ToString()));
                    }




                    //betData bb = new betData()
                    //{
                    //    bbid = dr[0].ToString(),
                    //    betMoney = Convert.ToDecimal(dr[2]),
                    //    username = dr[1].ToString(),
                    //    AuditType = "None",
                    //    Audit = Convert.ToDecimal(dr[2]),
                    //    Type = 5,//优惠活动
                    //    isReal = false,
                    //    Memo = "红包活动",
                    //    aname= aname[4],
                    //};

                    //list.Add(bb);
                }

                //线程循环
                Parallel.ForEach(list, (item) =>
                {
                        /*
                        bool br = false;
                        //看数据库是否存在
                        if (!appSittingSet.recorderDbCheck(string.Format("select * from record where   username='{0}' and bbid='{1}'  and type=1002", item.username, item.bbid)))
                        {
                            //加钱 充值的部分 
                           br = platGPK.MemberDepositSubmit(item);
                            if (br)
                            {
                                //插入字典
                                //openw.Add(item.bbid);
                                //插入本地数据库
                                SQLiteHelper.SQLiteHelper.execSql($"insert into record values({item.bbid},'{item.Memo}','{item.username}',datetime(CURRENT_TIMESTAMP,'localtime'),1002,1);");
                                //回填通过 更改数据库 状态为3
                                int eff1 = MySQLHelper.MySQLHelper.ExecuteSql(string.Format(myConfig["sql_hb_upadte"].ToString(), item.bbid));
                                item.msg =item.aname+ item.aname + "编号" + item.bbid + "用户" + item.username + (eff1 > 0 ? "处理成功" : "钱已经充值，状态更新状态失败");
                                appSittingSet.Log(item.msg);
                                MyWrite(item.msg);
                            }
                            else
                            {
                                //充钱失败的情况 ？ 不做处理 下一次处理
                                return;
                            }
                        }
                        else
                        {
                            //回填通过 更改数据库 状态为3
                            int eff1 = MySQLHelper.MySQLHelper.ExecuteSql(string.Format(myConfig["sql_hb_upadte"].ToString(), item.bbid));
                            //item.msg = "编号" + item.bbid + "用户" + item.username + "已经充值过，跳过";
                            //appSittingSet.Log(item.msg);
                            //MyWrite(item.msg);
                            return;
                        }

                    */

                    //加钱 充值的部分 
                    platGPK.MemberDepositSubmit(item);
                    //插入本地数据库
                    SQLiteHelper.SQLiteHelper.execSql($"insert into record values({item.bbid},'{item.Memo}','{item.username}',datetime(CURRENT_TIMESTAMP,'localtime'),1002,1);");
                    //回填通过 更改数据库 状态为3
                    int eff1 = MySQLHelper.MySQLHelper.ExecuteSql(string.Format(myConfig["sql_hb_upadte"].ToString(), item.bbid));
                    item.msg = item.aname + item.aname + "编号" + item.bbid + "用户" + item.username + (eff1 > 0 ? "处理成功" : "钱已经充值，状态更新状态失败");
                    appSittingSet.Log(item.msg);
                    MyWrite(item.msg);

                });

            }
        }

        /// <summary>
        /// 新红包平台，线程来做 只需要配置sql语句即可
        /// </summary>
        [DisallowConcurrentExecution]
        public class MyJob4 : IJob
        {
            public void Execute(IJobExecutionContext context)
            {
                MySQLHelper.MySQLHelper.connectionString = myConfig["hb_constr"].ToString();
                List<betData> list = new List<betData>();
                //List<string> openw = new List<string>();
                DataTable dt = MySQLHelper.MySQLHelper.Query(myConfig["hb_select"].ToString()).Tables[0];
                if (dt.Rows.Count == 0)
                {
                    //没有记录
                    MyWrite(aname[6] + "没有新的信息，等待下次执行 ");
                    SQLiteHelper.SQLiteHelper.execSql("delete  from record where type=1004");
                    return;
                }

                foreach (DataRow dr in dt.Rows)
                {
                    //去重复 如果不存在 加到list
                    if (!SQLiteHelper.SQLiteHelper.recorderDbCheck($"select * from record where  bbid={dr[0].ToString()}  and type=1004;"))
                    {
                        betData bb = new betData()
                        {
                            bbid = dr[0].ToString(),
                            username = dr[1].ToString(),
                            betMoney = Convert.ToDecimal(dr[2]),

                            AuditType = "None",
                            Audit = Convert.ToDecimal(dr[2]),
                            Type = 5,//优惠活动
                            isReal = false,
                            Memo = "红包活动-" + dr[0].ToString(),
                            aname = aname[6],
                        };
                        list.Add(bb);
                    }
                    else
                    {
                        int eff= MySQLHelper.MySQLHelper.ExecuteSql(string.Format(myConfig["hb_update"].ToString(), dr[0].ToString()));
                    }
                }

                //线程循环
                Parallel.ForEach(list, (item) =>
                {
                    //加钱 充值的部分 
                    platGPK.MemberDepositSubmit(item);
                    //插入本地数据库
                    SQLiteHelper.SQLiteHelper.execSql($"insert into record values({item.bbid},'{item.Memo}','{item.username}',datetime(CURRENT_TIMESTAMP,'localtime'),1004,1);");
                    //回填通过 更改数据库 状态为3
                    int eff1 = MySQLHelper.MySQLHelper.ExecuteSql(string.Format(myConfig["hb_update"].ToString(), item.bbid));
                    item.msg = item.aname +"编号" + item.bbid + "用户" + item.username + (eff1 > 0 ? "处理成功" : "钱已经充值，状态更新状态失败");
                    appSittingSet.Log(item.msg);
                    MyWrite(item.msg);

                });

            }
        }
        #endregion

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            //登录一遍
            MyJobLogin myjob1 = new MyJobLogin();
            myjob1.Execute(null);
            appSittingSet.Log("手动操作登录");
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            appSittingSet.showLogFile();
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            string filePath = Application.ExecutablePath + ".config";
            //System.Diagnostics.Process.Start("notepad.exe", filePath);
            var xe = XElement.Load(filePath).Element("appconfig").Attribute("configSource").Value;
            filePath = Application.StartupPath + "\\" + xe;
            System.Diagnostics.Process.Start("notepad.exe", filePath);
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            Application.Exit();
            System.Diagnostics.Process.Start(System.Reflection.Assembly.GetExecutingAssembly().Location);
        }
        private void toolStripMenuItem6_Click(object sender, EventArgs e)
        {
            ClsListItem();
        }
        #region 不用代码
        private void APP签到_Click(object sender, EventArgs e)
        {
            if (connectionString[0]=="?" || connectionString[0]=="")
            {
                return;
            }
            //查询上次处理的ID
            string sql = "select max(id) from cs_zhangdan LIMIT 1;";
            MySQLHelper.MySQLHelper.connectionString = connectionString[0];
            string input = MySQLHelper.MySQLHelper.GetScalar(sql).ToString();
            if (ShowInputDialog(ref input, "当前处理到的ID如下") == DialogResult.OK)
            {
                //修改最大ID
                MySQLHelper.MySQLHelper.ExecuteSql("update cs_zhangdan set apply=1 WHERE type='dh' and  id<" + input + ";");
            }
        }

        #endregion
    }
}
