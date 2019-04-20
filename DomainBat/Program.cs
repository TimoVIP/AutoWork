using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using TimoControl;

namespace DomainBat
{
    class Program
    {
        static void Main(string[] args)
        {
            string s = appSittingSet.readAppsettings("MySqlConnect").Split('|')[0];
            //string group = appSittingSet.readAppsettings("group");
            MySQLHelper.connectionString = s;
            if (s=="" )
            {
                Console.WriteLine("数据库配置有误");
                return;
            }
            else
            {
                if (MySQLHelper.conn().State!= ConnectionState.Open)
                {
                    Console.WriteLine("数据库配置有误");
                    return;
                }
            }
            NPOIExcel npoi = new NPOIExcel();
            DataTable dt = new DataTable();
            //遍历文件
            DirectoryInfo di = new DirectoryInfo(Environment.CurrentDirectory);
            FileInfo[] fs = di.GetFiles("*.xls", SearchOption.TopDirectoryOnly);

            Console.Title = "自动添加域名CDN程序";
            if (fs.Length > 0)
            {
                //先登陆一遍
                //bool b = platGPK.loginGPK();
                //if (!b)
                //{
                //    Console.WriteLine("GPK登陆失败");
                //    return;
                //}
                //else
                //{
                //    Console.WriteLine("GPK登陆成功");
                //}
                foreach (var file in fs)
                {
                    Console.WriteLine("发现待处理文件 " + file.FullName);
                }
            }
            else
            {
                Console.WriteLine("没有找到对应的.xlsx文件，按任意键退出");
                Console.ReadKey(true);
            }

            foreach (var file in fs)
            {
                Console.WriteLine("开始处理文件" + file.FullName);
                //读取EXCEL文件
                dt = npoi.ExcelToDataTable(true, "", file.FullName);
                StringBuilder sb = new StringBuilder();
                List<MySqlParameter> para = new List<MySqlParameter>() { };
                int index_ = 0;
                string old_SSLCrtContent_path = "";
                string old_SSLKeyContent_path = "";

                string SSLCrtContent = "";
                string SSLKeyContent = "";

                //调用接口处理数据
                if (dt == null)
                {
                    continue;
                }

                int i = 0;
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr[0].ToString()=="" || dr[1].ToString()=="" || dr[2].ToString()=="" || dr[3].ToString()=="" || dr[4].ToString()=="" || dr[5].ToString()=="")
                    {
                        continue;
                    }
                    sb.Clear();
                    //组织sql语句
                    sb.Append("INSERT INTO fikcdn_domain (hostname,username,add_time,upstream,icp,DNSName,SSLCrtContent,SSLKeyContent,SSLExtraParams,SSLOpt,buy_id,group_id) VALUES  (@hostname_"+index_+", 'client', unix_timestamp(now()), @upstream_"+index_+", '', '', @SSLCrtContent_"+index_+", @SSLKeyContent_"+index_+ ", 'SessionSize=5000&amp;Password=',2,@buy_id_"+index_+",@group_id_"+index_+");");
                    para.Add(new MySqlParameter("@hostname_"+index_, dr[0]));
                    para.Add(new MySqlParameter("@upstream_"+index_, dr[1]));
                    para.Add(new MySqlParameter("@buy_id_" + index_, dr[4]));
                    para.Add(new MySqlParameter("@group_id_" + index_, dr[5]));
                    string msg = string.Format("第{0}条数据{1}读处理完毕", index_, dr[0]);

                    try
                    {
                        if (old_SSLCrtContent_path!=dr[2].ToString())
                        {
                            //读取SSLCrtContent 内容
                            SSLCrtContent = File.ReadAllText(Environment.CurrentDirectory + "\\SSL\\" + dr[2].ToString());
                            //appSittingSet.Log("111");
                        }
                        if (old_SSLKeyContent_path!=dr[3].ToString())
                        {
                            //读取SSLCrtContent 内容
                            SSLKeyContent = File.ReadAllText(Environment.CurrentDirectory + "\\SSL\\" + dr[3].ToString());
                        }

                    }
                    catch (IOException ex)
                    {
                         msg = string.Format("第{0}条数据{1}读取证书错误,请操作完毕后手动更改-{2}",index_,dr[0],ex.Message);
                        appSittingSet.Log (msg);
                        //continue;             
                    }

                    para.Add(new MySqlParameter("@SSLCrtContent_"+index_,SSLCrtContent ));
                    para.Add(new MySqlParameter("@SSLKeyContent_"+index_,SSLKeyContent ));

                    Console.WriteLine(msg);

                    old_SSLCrtContent_path = dr[2].ToString();
                    old_SSLKeyContent_path= dr[3].ToString();
                    index_++;

                    i+= MySQLHelper.ExecuteSql(sb.ToString(), para);
                    //if (index_ % 200==0)
                    //{
                    //    i+= MySQLHelper.ExecuteSql(sb.ToString(), para);
                    //    sb.Clear();
                    //}
                }

                //for (int i = 0; i < dt.Rows.Count; i++)
                //{
                //    if (true)
                //    {

                //    }
                //}

                //int i = MySQLHelper.ExecuteSql(sb.ToString(), para);
                //if (i==index_)
                //    Console.WriteLine("处理完毕");
                //else
                //    Console.WriteLine("处理完毕,但是有不等数据");
                file.CopyTo(Environment.CurrentDirectory + "\\log\\" + file.Name+DateTime.Now.Date.ToString("yyyyMMddHHmmssfff"),true);
                file.Delete();

            }

            Console.WriteLine("处理完毕,按任意键退出...");
            Console.ReadKey(true);
        }
    }
}
