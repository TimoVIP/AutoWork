using System;
using System.Drawing;
using System.Windows.Forms;
using TimoControl;

namespace AutoWork_Plat_5hao
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //this.Text = "X=" + MousePosition.X.ToString() + " " + "Y=" + MousePosition.Y.ToString();
            //textBox1.Text = MousePosition.X.ToString();
            //textBox2.Text = MousePosition.Y.ToString();
            Text = "X=" + textBox1.Location.X.ToString() + " " + "Y=" + textBox1.Location.Y.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int X = Convert.ToInt32(textBox1.Text);
            int Y = Convert.ToInt32(textBox2.Text);
            KeyBoard_Mouse.MouseLeftClickEvent(X, Y, 0);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // 双击
            int X = Convert.ToInt32(textBox1.Text);
            int Y = Convert.ToInt32(textBox2.Text);
            KeyBoard_Mouse.MouseLeftClickEvent(X, Y, 0);
            KeyBoard_Mouse.MouseLeftClickEvent(X, Y, 0);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            timer1.Start();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //KeyBoard_Mouse.SetCursorPos(textBox1.Location.X + 10, textBox1.Location.Y + 10);
            //KeyBoard_Mouse.SetCursorPos(100,100);
            Point p1 = LocationOnClient(textBox1);
            //KeyBoard_Mouse.MouseLeftClickEvent(p1.X+5, p1.Y+10, 0);
            //KeyBoard_Mouse.MouseLeftClickEvent(p1.X, p1.Y, 0);
            KeyBoard_Mouse.MouseClick(p1, KeyBoard_Mouse.MouseButton.Left);
            KeyBoard_Mouse.keyPress(KeyBoard_Mouse.vKeyX);
            KeyBoard_Mouse.keyPress(KeyBoard_Mouse.vKeyH);
            KeyBoard_Mouse.keyPress(KeyBoard_Mouse.vKeyY);
            KeyBoard_Mouse.keyPress(KeyBoard_Mouse.vKey8);
            KeyBoard_Mouse.keyPress(KeyBoard_Mouse.vKey3);
            KeyBoard_Mouse.keyPress(KeyBoard_Mouse.vKey1);

        }
        private Point LocationOnClient(Control c)
        {
            //Point retval = new Point(0, 0);
            //for (; c.Parent != null; c = c.Parent)
            //{
            //    retval.Offset(c.Location);
            //}
            //return retval;
            Point p = PointToScreen(c.Location);
            p.X += c.Width / 3;
            p.Y += c.Height / 3;
            //Point p2 = PointToScreen(new Point(c.Location.X + c.Width / 2, c.Location.Y + c.Height / 2));
            return p;
        }
    }
}
