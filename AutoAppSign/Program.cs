using System;
using System.Diagnostics;
using System.Windows.Forms;
using TimoControl;
using validation;
namespace AutoAppSign
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            //AppCheck.getMachineCode();
            //验证软件有效期
            //string msg = "";
            //if (!AppCheck.isExpired(out msg))
            //{
            //    appSittingSet.Log(msg);
            //    return;
            //}

            //===== 判断进程法：(修改程序名字后依然能执行) =====
            Process current = Process.GetCurrentProcess();
            Process[] processes = Process.GetProcessesByName(current.ProcessName);
            foreach (Process process in processes)
            {
                if (process.Id != current.Id)
                {
                    if (process.MainModule.FileName == current.MainModule.FileName)
                    {
                        process.Kill();
                        //MessageBox.Show("程序已经运行！", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        //return;
                    }
                }
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
