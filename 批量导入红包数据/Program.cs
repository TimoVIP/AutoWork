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
using System.Threading;

namespace 批量导入红包数据
{
    class Program
    {
        static Hashtable myConfig;
        static NPOIExcel npoi = new NPOIExcel();
        static DataTable dt = new DataTable();


        static void Main(string[] args)
        {



            //读取配置
            myConfig = appSittingSet.readConfig();
            Console.Title = myConfig["platname"] + "自动导入红包活动";
            bool b = false;

            if (myConfig==null || myConfig.Count==0)
            {
                Console.WriteLine("请检查配置文件，按任意键退出");
                Console.ReadKey(true);
            }

            //遍历文件
            DirectoryInfo di = new DirectoryInfo(Environment.CurrentDirectory);//根目录
            FileInfo[] fs = di.GetFiles("*.xlsx", SearchOption.TopDirectoryOnly);
            if (fs.Length == 0)
            {
                Console.WriteLine("没有找到对应的.xlsx文件，按任意键退出");
                Console.ReadKey(true);
            }

            //先登陆一遍
            b = platGPK.loginGPK();
            if (!b)
            {
                Console.WriteLine("GPK登陆失败");
                return;
            }
            else
                Console.WriteLine("GPK登陆成功");


            try
            {

                //读取excel
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
                    //dt.Columns.RemoveAt(1);
                    dt.Columns[0].ColumnName = "帐号";
                    dt.Columns[1].ColumnName = "级别";
                    DataSet ds = new DataSet();

                    //复制表格

                    //生成4份dt
                    for (int i = 0; i < 4; i++)
                    {
                        ds.Tables.Add(dt.Copy());
                        //插入4tb 数据
                        foreach (DataRow dr in ds.Tables[i].Rows)
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
                        }

                        //移除级别
                        ds.Tables[i].Columns.RemoveAt(1);

                        //生成4份表格
                        string filepath = $"{Environment.CurrentDirectory}\\data\\tmp{DateTime.Now.Date.ToString("yyyyMMdd") + i}.xlsx";
                        b = npoi.ToExcel(ds.Tables[i], "", "sheet1", filepath);
                        if (b)
                        {

                            //上传文件到gpk
                            string StartTime = "";
                            string EndTime = "";
                            string docName = $"{DateTime.Now.Date.ToString("yyyy-MM-dd") }红包雨第{ (i + 1)}波";
                            Dictionary<string, string> postData = new Dictionary<string, string>();
                            postData.Add("Description", "心想事成、多多盈利");
                            postData.Add("Password", platGPK.pwd);
                            postData.Add("Name", docName);

                            StartTime = DateTime.Now.Date.ToString("yyyy/MM/dd") + myConfig["StartTime" + (i + 1)].ToString();
                            if (i == 3)
                                EndTime = DateTime.Now.Date.AddDays(1).ToString("yyyy/MM/dd") + myConfig["EndTime"+(i + 1)].ToString();
                            else
                                EndTime = DateTime.Now.Date.ToString("yyyy/MM/dd") + myConfig["EndTime"+(i + 1)].ToString();

                            postData.Add("StartTime", StartTime);
                            postData.Add("EndTime", EndTime);
                            b = platGPK.RedEnvelopeManagement_GetExcelSum(filepath, postData);
                            Thread.Sleep(500);
                        }
                        else
                        {
                            Console.WriteLine($"处理文件{filepath}失败");
                            appSittingSet.Log($"处理文件{filepath}失败");
                        }
                        Console.WriteLine($"处理文件{filepath}成功");
                            appSittingSet.Log($"处理文件{filepath}成功");             
                            Thread.Sleep(500);                           
                    }

                    //移动之前的文件
                    string sf= $"{Environment.CurrentDirectory}\\data\\bak{DateTime.Now.Date.ToString("yyyyMMdd")}.xlsx";

                    if (File.Exists(sf))
                    {
                        File.Delete(sf);
                    }
                    file.MoveTo(sf);
 
                    Console.WriteLine("本次处理完毕，等待下一次处理");
                    Console.ReadKey();
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                appSittingSet.Log(ex.Message);

            }

        }

        public static double GetRandomNumber(double minimum, double maximum, int Len)   //Len小数点保留位数
        {
            Random random = new Random(Guid.NewGuid().GetHashCode());
            return Math.Round(random.NextDouble() * (maximum - minimum) + minimum, Len);
        }


    }
}
