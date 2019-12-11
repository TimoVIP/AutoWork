using BaseFun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace queryInfor
{
    public partial class Form2 : Form
    {
        Hashtable config = null;
        bool islogin = false;
        string uid_old = "";
        string uid = "";
        DateTime d1;
        DateTime d2;

        public Form2()
        {
            InitializeComponent();
            config = appSittingSet.readConfig("appconfig");
            //一层账号 用于验证
            //platBW.acc1 = config["bw1"].ToString().Split('|')[0];
            //platBW.pwd1 = config["bw1"].ToString().Split('|')[1];
            //platBW.urlbase = config["bw1"].ToString().Split('|')[2];
        }

        private void calcnum2_TextChanged(object sender, EventArgs e)
        {
            custemcalc();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            //加载二层账号

            acc.Text = config["bw2"].ToString().Split('|')[0];
            pwd.Text = config["bw2"].ToString().Split('|')[1];

            otp.Text = "123456";
            otp.Focus();
            otp.SelectAll();
            this.ActiveControl = otp;
            //dateTimePicker1.Value = dateTimePicker2.Value.AddDays(-3);
            DateTimeChoser.AddTo(textBox1, DateTime.Now.AddDays(-3));
            DateTimeChoser.AddTo(textBox2, DateTime.Now);
        }
        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (otp.Text.Trim().Length < 6)
            {
                MessageBox.Show("填入otp");
                otp.SelectAll();
                otp.Focus();
                return;
            }
            platBW.acc2 = acc.Text.Trim();
            platBW.pwd2 = pwd.Text.Trim();
            platBW.otp = otp.Text.Trim();

            //保存到config
            appSittingSet.writeAppsettings("BW2", $"{platBW.acc2}|{platBW.pwd2}|{platBW.urlbase}");

            //打开浏览器 登陆
            string msg = "";
            islogin = platBW.login(out msg);
            lbmsg.Text = msg;
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }


        private void button5_Click(object sender, EventArgs e)
        {
            if (!islogin)
            {
                MessageBox.Show("请先登陆");

                return;
            }
            if (tbUserID.Text.Trim()=="")
            {
                tbUserID.Focus();
                return;
            }
            uid = tbUserID.Text.Trim();
            //d1= Convert.ToDateTime(textBox1.Text);
            //d2= Convert.ToDateTime(textBox2.Text);

            d1 = DateTime.Parse(textBox1.Text);
            d2 = DateTime.Parse(textBox2.Text);
            #region 显示记录

            string s = "";

            //显示基础信息
            if (uid != uid_old)
            {
                //s = platBW.getUserinfor(uid);
                Hashtable ht = platBW.getUserinfor(uid, out s);
                if (s.Contains("用户不存在"))
                {
                    return;
                }
                //s += "所属渠道：" + platBW.getChannel(uid);
                uid_old = uid;
                tbInfor.Text = s;
            }

            //显示金币尊送记录
            //dataGridView1.DataSource = platBW.getJBZS(uid, d1, d2, out s);
            //groupBox4.Text = "赠送记录       " + s;



            //1显示兑换记录
            dataGridView3.DataSource = platBW.getYHDH(uid, d1, d2,out s);
            groupBox3.Text = "兑换记录  "+s;
            //2显示充值记录
            s = "";
            dataGridView2.DataSource = platBW.getYHCZ(uid, d1, d2,out s);
            groupBox5.Text = "充值记录  " +s;


            //显示 总游戏记录
            dataGridView4.DataSource = platBW.getYHZYXJL(uid, d1, d2);
            //显示 用户游戏记录
            //dataGridView5.DataSource = platBW.getYHYXJL(uid, d1, d2);
            //显示 用户游戏记录统计
            //dataGridView6.DataSource = platBW.getYHDH(uid, d1, d2);

            #endregion

            #region 隐藏不需要的列

            
            foreach (Control a in tableLayoutPanel3.Controls)
            {
                foreach (Control b in a.Controls)
                {
                    if (b is DataGridView)
                    {
                        DataGridView gv = (DataGridView)b;
                        gv.RowHeadersVisible = false;
                        gv.RowsDefaultCellStyle.BackColor = Color.WhiteSmoke;// 设置表格背景色
                        gv.AlternatingRowsDefaultCellStyle.BackColor = Color.LightSkyBlue; // 设置交替行的背景色
                    }
                }
            }

            //dataGridView1.Columns[0].Visible = false;
            //dataGridView1.Columns[1].Visible = false;
            //dataGridView1.Columns[3].Visible = false;


            for (int i = 0; i < dataGridView2.Columns.Count; i++)
            {
                if (i == 6 || i ==7|| i==10 || i==12)
                {
                    dataGridView2.Columns[i].Visible = true;
                }
                else
                {
                    dataGridView2.Columns[i].Visible = false;
                }
            }
            

            for (int i = 0; i < dataGridView3.Columns.Count; i++)
            {
                if ( i==13 || i==14|| i==15 || i==16|| i==19 || i==20 ||i==21)
                {
                    dataGridView3.Columns[i].Visible = true;
                }
                else
                {
                    dataGridView3.Columns[i].Visible = false;
                }
            }



            #endregion
        }

        private void button4_Click(object sender, EventArgs e)
        {
            custemcalc();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            foreach (Control item in groupBox2.Controls)
            {
                if (item is TextBox)
                {
                    item.Text = "";
                }
            }
        }

        private void calcnum1_TextChanged(object sender, EventArgs e)
        {
            custemcalc();
        }

        private void custemcalc() {
            if (calcnum1.Text.Trim() == "")
                calcnum1.Text = "0";
            if (calcnum2.Text.Trim() == "")
                calcnum2.Text = "0";
            resault1.Text = (Convert.ToDouble(calcnum1.Text) + Convert.ToDouble(calcnum2.Text)).ToString();
            resault2.Text =(Convert.ToDouble( calcnum1.Text) - Convert.ToDouble(calcnum2.Text)).ToString();
            resault3.Text =(Convert.ToDouble( calcnum1.Text) * Convert.ToDouble(calcnum2.Text)).ToString();
            resault4.Text =(Convert.ToDouble( calcnum1.Text) / Convert.ToDouble(calcnum2.Text)).ToString();
        }

        private void otp_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode== Keys.Enter)
            {
                btnLogin_Click(sender, e);
            }
        }

        private void dataGridView4_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            dataGridView4.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            DataGridViewRow row = dataGridView4.Rows[e.RowIndex];
            //if (platBW.dic_gameids.Count==0)
            //{
            //    return;
            //}

            string t1 = row.Cells[0].Value.ToString();
            //string t2 = row.Cells[20].ToString();
            d1 = DateTime.Parse(t1);
            //d2 = DateTime.Parse(t2);
            string gameid = platBW.dic_gameids[row.Cells[2].Value.ToString()];
            dataGridView5.DataSource = platBW.getYHYXJL(uid, d1, d2,gameid);

            for (int i = 0; i < dataGridView5.Columns.Count; i++)
            {
                if (i == 1 || i == 2 || i == 3 || i == 8 || i == 11)
                {
                    dataGridView5.Columns[i].Visible = true;
                }
                else
                {
                    dataGridView5.Columns[i].Visible = false;
                }
            }
        }


    }
}
