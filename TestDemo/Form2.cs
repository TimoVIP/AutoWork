using BaseFun;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestDemo
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            textBox2.Text = appSittingSet.desEncode(textBox1.Text);
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            textBox1.Text = appSittingSet.desDecode(textBox2.Text);
        }
    }
}
