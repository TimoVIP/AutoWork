using BaseFun;
using MySql.Data.MySqlClient;
using MySQLHelper;
using NPOIHelper;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace imgdata
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string s = appSittingSet.readAppsettings("MySqlConnect").Split('|')[0];
                MySQLHelper.MySQLHelper.connectionString = s;
                if (s == "")
                {
                    Console.WriteLine("数据库配置有误");
                    return;
                }
                else
                {
                    if (MySQLHelper.MySQLHelper.conn().State != ConnectionState.Open)
                    {
                        Console.WriteLine("数据库配置有误");
                        return;
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
            }


                NPOIExcel npoi = new NPOIExcel();
                DataTable dt = new DataTable();
                //遍历文件
                DirectoryInfo di = new DirectoryInfo(Environment.CurrentDirectory+"\\data");
                FileInfo[] fs = di.GetFiles("*.xlsx", SearchOption.TopDirectoryOnly);

                Console.Title = "自动导入移民局签证数据程序";
                //string msg = "";
                if (fs.Length > 0)
                {
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


            try
            {
                foreach (var file in fs)
                {
                    Console.WriteLine($"开始处理文件 {file.Name}\t{DateTime.Now.ToString()}");
                    //读取EXCEL文件
                    dt = npoi.ExcelToDataTable2(file.FullName);

                    //int index_ = 1;

                    //调用接口处理数据
                    if (dt == null)
                    {
                        continue;
                    }

                    DataTable dt1 = null;
                    MySqlDataAdapter da = null;


                    //if (dt.Columns.Contains("HEARING_OFFICER"))
                    //{
                    //    dt1 = dt.DefaultView.ToTable(false, new string[] { "APPLIC_NO", "NAME", "DOB", "NATIONALITY", "PETITIONER", "NO_DEP", "VISA_TYPE", "VALIDITY", "HEARING_OFFICER", "FOLDER_NO", "FILENAME", "SHEETNAME" });
                    //    da = new MySqlDataAdapter("SELECT APPLIC_NO,	NAME,	DOB,	NATIONALITY,	PETITIONER	,NO_DEP	,VISA_TYPE	,VALIDITY	,HEARING_OFFICER,	FOLDER_NO,FILENAME,SHEETNAME FROM 9gdata", MySQLHelper.MySQLHelper.conn());
                    //}
                    //if (dt.Columns.Contains("HEARINGOFFICER"))
                    //{
                    //    dt1 = dt.DefaultView.ToTable(false, new string[] { "APPLIC_NO", "NAME", "DOB", "NATIONALITY", "PETITIONER", "NO_DEP", "VISA_TYPE", "VALIDITY", "HEARINGOFFICER", "FOLDER_NO", "FILENAME", "SHEETNAME" });
                    //    da = new MySqlDataAdapter("SELECT APPLIC_NO,	NAME,	DOB,	NATIONALITY,	PETITIONER	,NO_DEP	,VISA_TYPE	,VALIDITY	,HEARINGOFFICER,	FOLDER_NO,FILENAME,SHEETNAME FROM 9gdata", MySQLHelper.MySQLHelper.conn());
                    //}                    


                    try
                    {
                        //处理datatable
                        dt1 = dt.DefaultView.ToTable(false, new string[] { "APPLICNO", "NAME", "DOB", "NATIONALITY", "PETITIONER", "NODEP", "VISATYPE", "VALIDITY", "HEARINGOFFICER", "FOLDERNO", "FILENAME", "SHEETNAME" });
                        //string filter = $"APPLIC_NO <>'' and NAME <>'' ";
                        DataRow[] drr = dt1.Select("APPLICNO <>'' and NAME <>'' ");
                        int count = drr.Length;
                        //处理dt 删除多余的列
                        Console.WriteLine($"文件 {file.Name}有{count}行数据\t{DateTime.Now.ToString()}");

                        //方法1插入
                        da = new MySqlDataAdapter("SELECT APPLICNO,NAME,	DOB,NATIONALITY,PETITIONER,NODEP,VISATYPE,VALIDITY,HEARINGOFFICER,FOLDERNO,FILENAME,SHEETNAME FROM 9gdata", MySQLHelper.MySQLHelper.conn());
                        MySqlCommandBuilder cb = new MySqlCommandBuilder(da);
                        da.Fill(dt);
                        da.Update(drr);
                    }
                    catch (Exception ex)
                    {
                        appSittingSet.Log($"{file.Name} {ex.Message}");
                        file.MoveTo(file.DirectoryName + "\\Err\\" + file.Name);
                    }





                    //方法2
                    /*
                    List<string> sqlList = new List<string>();
                    bool b = false;
                    foreach (DataRow dr in drr)
                    {
                        msg = $"开始处理{file.Name}文件中的第{index_ }/{count}条数据";
                        Console.WriteLine(msg);
                        sqlList.Add($"insert into 9gdata (APPLIC_NO,NAME,DOB,NATIONALITY,PETITIONER,NO_DEP,VISA_TYPE,VALIDITY,HEARING_OFFICER,FOLDER_NO,FILENAME,SHEETNAME) VALUES('{dr[0]}','{dr[1]}','{dr[2]}','{dr[3]}','{dr[4]}',0,'{dr[6]}','{dr[7]}','{dr[8]}','{dr[9]}','{dr[10]}','{dr[11]}');");
                        //if (index_==dt.Rows.Count-1)
                        //{

                        //}
                        if ((index_ % 200 == 0 || index_ == count ))
                        {
                            b = MySQLHelper.MySQLHelper.ExecuteNoQueryTran(sqlList);
                            sqlList.Clear();
                            msg = $"处理{file.Name}文件中的第{index_ }/{count}条数据成功";
                            Console.WriteLine(msg);
                            appSittingSet.Log(msg);
                            //sb.Clear();
                        }
                        index_++;
                    }
                    */

                    file.MoveTo(file.DirectoryName + "\\bak\\" + file.Name);
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
                appSittingSet.Log(ex.Message);
            }

            Console.WriteLine("处理完毕,按任意键退出...");
            Console.ReadKey(true);
        }
    }
}
