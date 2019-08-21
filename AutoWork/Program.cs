using System;
using System.Diagnostics;
using System.Windows.Forms;
using BaseFun;
namespace AutoWork_Plat1
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
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
            Application.Run(new frmMain());
            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            appSittingSet.Log(e.ExceptionObject.ToString());
            //throw new NotImplementedException();
        }

        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            appSittingSet.Log(e.Exception.Message);
            //throw new NotImplementedException();
        }
    }
}
