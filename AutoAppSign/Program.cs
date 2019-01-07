using System;
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
            string msg = "";
            if (!AppCheck.isExpired(out msg))
            {
                appSittingSet.txtLog(msg);
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
