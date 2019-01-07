using System;
using System.Drawing;
using System.Windows.Forms;

namespace TestDemo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.Icon =new System.Drawing.Icon(Properties.Resources.favicon__2_,256,256);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void button2_MouseMove(object sender, MouseEventArgs e)
        {
            Button b = (Button)sender;
            Random re = new Random();
            int rw = re.Next(0, this.Width - b.Width - 20);
            int rh = re.Next(0, this.Height - b.Height - 60);
            b.Location = new Point(rw, rh);
        }
        bool agree = false;
        private void button1_Click(object sender, EventArgs e)
        {
            //DialogResult d = MessageBox.Show("就知道你会同意的", "呵呵哒", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //if (d== DialogResult.OK)
            //{
            //    Application.Exit();
            //}
            agree = true;
            //Application.Exit();
            new FrmFullScreen().Show();
            this.Hide();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!agree)
            {
                DialogResult d = MessageBox.Show("这么狠心吗？┭┮﹏┭┮\n不过亲，没有这个选择的额", "错误的操作", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //d = MessageBox.Show("没有这个选择额，亲", "错误的操作", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //if (d == DialogResult.OK)
                //{

                //}
                e.Cancel = true;
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult d = MessageBox.Show("这样都可以😮\n果然是凭本事单身的，练就了举世无双的手速啊\n不过没有这种操作呢，亲", "错误的操作", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

    }
}
