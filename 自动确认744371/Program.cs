using BaseFun;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimoControl;

namespace 自动确认744371
{
    class Program
    {
        static void Main(string[] args)
        {
            //string ss = plat744371.getCaptcha();
            //string s = appSittingSet.sha512("jiqiren66");
            bool b = plat744371B.login();
            if (b)
            {
                List<betData> list = plat744371B.getRecorder();
            }

            Console.ReadLine();
        }
    }
}
