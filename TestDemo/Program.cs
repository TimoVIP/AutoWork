﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using TimoControl;

namespace TestDemo
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form2());

            //bool b = platQPGV2.login();
            //List<betData> list = platQPGV2.getActData();
            //betData bb = list[0];
            //bb.passed = true;
            //b = platQPGV2.confirmAct(bb);
        }
    }
}
