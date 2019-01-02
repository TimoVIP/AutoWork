using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TimoControl;

namespace TestDemo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        string[] Prob = new string[] { "2", "1" };
        static List<betData> list_temp = new List<betData>();
        private void Form1_Load(object sender, EventArgs e)
        {
            /*
            List<betData> list = new List<betData>();
            for (int i = 1; i <= 5; i++)
            {
                list.Add(new betData() { username = "wlf5577558", betTime = "2018-12-29 21:50:59", bbid = "7382" + new Random().Next(10, 99), passed = true, aid = "40" });//默认等于合格
                Thread.Sleep(100);
            }
            for (int i = 1; i <= 5; i++)
            {
                list.Add(new betData() { username = "wlf5577558", betTime = "2018-12-29 21:50:59", bbid = "7382" + new Random().Next(10, 99), passed = true, aid = "40" });//默认等于合格
                Thread.Sleep(10);
            }
            //list.Add(new betData() { username = "wlf5577558", betTime = "2018-12-29 21:50:59", bbid = "738215", passed = true, aid = "40" });//默认等于合格
            foreach (var item in list)
            {
                //记录一个list 去除重复后 的第9，10 条 直接拒绝掉

                if (Prob[1] == "1")
                {
                    //如果达到10条清除掉
                    if (list_temp.Count == 10)
                    {
                        list_temp.Clear();
                    }

                    if (!list_temp.Exists(x => x.bbid == item.bbid) && list_temp.Count < 10)
                    {
                        list_temp.Add(item);
                    }

                    if (list_temp.Count > (10 - int.Parse(Prob[0])))
                    {
                        //直接拒绝
                        item.msg = "同IP其他会员已申请通过 RR";
                        appSittingSet.txtLog(item.msg);
                        continue;
                    }

                }

            }
            */
        }

        private void button2_MouseMove(object sender, MouseEventArgs e)
        {
            Button b = (Button)sender;

            Random re = new Random();
            int rw = re.Next(0, this.Width - b.Width - 20);
            int rh = re.Next(0, this.Height - b.Height - 60);
            b.Location = new Point(rw, rh);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult d = MessageBox.Show("就知道你会同意的", "呵呵哒", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            if (d== DialogResult.OK)
            {
                Application.Exit();
            }
        }
    }
}
