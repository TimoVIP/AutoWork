using BaseFun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimoControl;
using NPOIHelper;
namespace 批量导入红包数据
{
    class Program
    {
        static Hashtable myConfig;
        static void Main(string[] args)
        {
            //读取配置
            myConfig = appSittingSet.readConfig();
            //string s = appSittingSet.readAppsettings("test");




            //读取excel
            NPOIExcel npoi = new NPOIExcel();
            DataTable dt = new DataTable();

            //遍历文件
            DirectoryInfo di = new DirectoryInfo(Environment.CurrentDirectory+"\\data");
            FileInfo[] fs = di.GetFiles("*.xlsx", SearchOption.TopDirectoryOnly);

            Console.Title = "自动导入红包信息 处理程序";
            if (fs.Length == 0)
            {
                Console.WriteLine("没有找到对应的.xlsx文件，按任意键退出");
                Console.ReadKey(true);
            }

            foreach (var file in fs)
            {
                Console.WriteLine("开始处理文件" + file.FullName);
                //读取EXCEL文件
                dt = npoi.ExcelToDataTable_hb(file.FullName);
                //调用接口处理数据
                if (dt == null)
                {
                    continue;
                }
                dt.Columns.Add("红包金额");
                dt.Columns.Add("稽核金额");
                dt.Columns.Add("前台备注");
                foreach (DataRow dr in dt.Rows)
                {
                    //处理计算 第三列的金额
                    foreach (DictionaryEntry item in myConfig)
                    {
                        if (item.Key.ToString() == dr[1].ToString())
                        {
                            double min = double.Parse(item.Value.ToString().Split('@')[0]);
                            double max = double.Parse(item.Value.ToString().Split('@')[1]);
                            double c = GetRandomNumber(min, max, 2);
                            dr[2] = c;
                            dr[3] = 0;
                            break;
                        }
                    }

                    //Console.WriteLine("处理" + dr[0]);
                }

                //生成execl
                dt.Columns.RemoveAt(1);
                dt.Columns[0].ColumnName = "帐号";
                string filepath = $"{Environment.CurrentDirectory}\\datatemp\\{file.Name}";
                bool b =npoi.ToExcel(dt,"","sheet1",filepath);
                //移动之前的文件
                string sf = file.FullName.Replace("data", "bak");
                if (File.Exists(sf))
                {
                    File.Delete(sf);
                }
                file.MoveTo(sf);
                if (b)
                {

                    //先登陆一遍
                    b = platGPK.loginGPK();
                    if (!b)
                    {
                        Console.WriteLine("GPK登陆失败");
                        return;
                    }
                    else
                        Console.WriteLine("GPK登陆成功");

                    //上传文件到gpk
                    Dictionary<string, string> postData = new Dictionary<string, string>();
                        postData.Add("Description", "心想事成、多多盈利");
                        postData.Add("Password", platGPK.pwd);
                    if (file.Name.Contains("1"))
                    {
                        postData.Add("Name", "当日红包1");
                        postData.Add("StartTime", DateTime.Now.Date.ToString("yyyy/MM/dd")+ " 14:00:00");
                        postData.Add("EndTime", DateTime.Now.Date.ToString("yyyy/MM/dd") + " 19:59:59");
                    }
                    else if (file.Name.Contains("2"))
                    {
                        postData.Add("Name", "当日红包2");
                        postData.Add("StartTime", DateTime.Now.Date.ToString("yyyy/MM/dd")+ " 20:00:00");
                        postData.Add("EndTime", DateTime.Now.Date.ToString("yyyy/MM/dd") + " 01:59:59");
                    }
                    else if (file.Name.Contains("3"))
                    {
                        postData.Add("Name", "当日红包3");
                        postData.Add("StartTime", DateTime.Now.Date.ToString("yyyy/MM/dd")+ " 02:00:00");
                        postData.Add("EndTime", DateTime.Now.Date.ToString("yyyy/MM/dd") + " 07:59:59");
                    }
                    else if (file.Name.Contains("4"))
                    {
                        postData.Add("Name", "当日红包4");
                        postData.Add("StartTime", DateTime.Now.Date.ToString("yyyy/MM/dd")+ " 08:00:00");
                        postData.Add("EndTime", DateTime.Now.Date.ToString("yyyy/MM/dd") + " 13:59:59");
                    }


                    b = platGPK.RedEnvelopeManagement_GetExcelSum(filepath, postData);

                }
                else
                {
                    Console.WriteLine("处理文件失败,按任意键退出...");
                }

                Console.WriteLine("处理完毕,按任意键退出...");

            }
            //获取用户& 需要的钱

        }

        public static double GetRandomNumber(double minimum, double maximum, int Len)   //Len小数点保留位数
        {
            Random random = new Random();
            return Math.Round(random.NextDouble() * (maximum - minimum) + minimum, Len);
        }
    }
}
