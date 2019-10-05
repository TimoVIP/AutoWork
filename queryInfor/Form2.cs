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
        public Form2()
        {
            InitializeComponent();
            config = appSittingSet.readConfig("appconfig");
            //一层账号 用于验证
            platBW.acc1 = config["bw1"].ToString().Split('|')[0];
            platBW.pwd1 = config["bw1"].ToString().Split('|')[1];
            platBW.urlbase = config["bw1"].ToString().Split('|')[2];
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
            dateTimePicker1.Value = dateTimePicker2.Value.AddDays(-3);
        }

        private void button1_Click(object sender, EventArgs e)
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
             islogin = platBW.login();
            if (islogin)
                lbmsg.Text = "登陆成功";
            else
                lbmsg.Text = "登陆失败";
        }

        private void button2_Click(object sender, EventArgs e)
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
            string uid = tbUserID.Text.Trim();

            DateTime d1 = dateTimePicker1.Value;
            DateTime d2 = dateTimePicker2.Value;
            //显示基础信息
            if(uid!=uid_old)
                tbInfor.Text= platBW.getUserinfor(uid);
                uid_old = uid;
            //显示金币尊送记录
            dataGridView1.DataSource = platBW.getJBZS(uid, d1, d2);
            //显示充值记录
            string s = "";
            dataGridView2.DataSource = platBW.getYHCZ(uid, d1, d2,out s);
            groupBox5.Text = "充值记录——" +s;
            //显示兑换记录
            dataGridView3.DataSource = platBW.getYHDH(uid, d1, d2);


            //隐藏不需要的列
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.Columns[1].Visible = false;
            dataGridView1.Columns[2].Visible = false;
            dataGridView1.Columns[3].Visible = false;

            dataGridView2.RowHeadersVisible = false;
            dataGridView2.Columns[0].Visible = false;
            dataGridView2.Columns[1].Visible = false;
            dataGridView2.Columns[2].Visible = false;
            dataGridView2.Columns[3].Visible = false;
            dataGridView2.Columns[4].Visible = false;
            dataGridView2.Columns[5].Visible = false;
            dataGridView2.Columns[6].Visible = false;
            dataGridView2.Columns[7].Visible = false;
            dataGridView2.Columns[9].Visible = false;
            dataGridView2.Columns[10].Visible = false;
            dataGridView2.Columns[12].Visible = false;
            dataGridView2.Columns[14].Visible = false;
            dataGridView2.Columns[15].Visible = false;

            //foreach (DataColumnCollection item in dataGridView3.Columns)
            //{
            //    if ()
            //    {

            //    }
            //}
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
                button1_Click(sender, e);
            }
        }
    }
}
