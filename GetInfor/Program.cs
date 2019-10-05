using BaseFun;
using MySQLHelper;
using SQLiteHelper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TimoControl;

namespace GetInfor
{
    class Program
    {
       static string[] connectionString = appSittingSet.readAppsettings("MySqlConnect").Split('|');
        static Gpk_UserDetail user=null;
        static string sql = "";
        static bool b=true ;
        static void Main(string[] args)
        {

            b = ToDB();

            platGPK.loginGPK();
            Console.WriteLine("开始处理详细数据");

            ToDB2();

            Console.WriteLine("处理完毕,按任意键退出");
            Console.ReadKey();

        }

        private static bool ToDB()
        {
            //sql = "select id from infor order by id desc limit 1;";
            sql = "select id from infor order by rowid desc limit 1;";
            string maxid = SQLiteHelper.SQLiteHelper.execScalarSql(sql);
            maxid = maxid == "" ? "0" : maxid;
            //sql= "select  DISTINCT(username),id from e_submissions where `status`=1 and addtime>  '"+ 1555384291 + "' ORDER BY id desc;";
            //sql = "select  DISTINCT(username),id from e_submissions where `status`=1 and id> "+maxid+" ORDER BY id desc;";
            sql = $"select  DISTINCT(account),id from give where `status`=1 and id> {maxid} ORDER BY id desc;";
            //获取用户名 
            MySQLHelper.MySQLHelper.connectionString = connectionString[0];
            DataTable dt = MySQLHelper.MySQLHelper.Query(sql).Tables[0];
            if (dt.Rows.Count==0)
            {
                b = false;
                return b;
            }
            int count = 1;
            List<string> sql_list = new List<string>();
            foreach (DataRow item in dt.Rows)
            {
                //如果存在就跳过 数据库已经配置好了唯一约束
                sql_list.Add(string.Format("insert  or ignore  into infor (account,id)  values('{0}',{1});", item[0],item[1]));
                //if (count % 1000 == 0 || count==dt.Rows.Count-1)
                //{
                //    SQLiteHelper.SQLiteHelper.execSql(sql_list);
                //    sql_list.Clear();
                //}

                if (sql_list.Count == 500 || sql_list.Count == dt.Rows.Count % 500)
                {
                    SQLiteHelper.SQLiteHelper.execSql(sql_list);
                    sql_list.Clear();
                }

                Console.WriteLine("正在获取 "+count+"/" + dt.Rows.Count);
                count++;
            }

            return true;
        }

        private static bool ToDB2()
        {
            try
            {
                sql = "select account,id from infor where status=0 order by jointime ;";
                DataTable dt = SQLiteHelper.SQLiteHelper.getDataTableBySql(sql);
                int count = 1;
                List<string> sql_list = new List<string>();
                //普通循环

                foreach (DataRow item in dt.Rows)
                {
                    user = platGPK.GetUserDetail(item[0].ToString());
                    if (user == null)
                    {
                        //删掉就行了 更改状态为3
                        sql = "update infor set status=3 where id='" + item[1].ToString() + "';";
                        SQLiteHelper.SQLiteHelper.execSql(sql);
                        continue;
                    }
                    sql_list.Add(" INSERT or ignore   INTO detail(Account,Birthday,Email,Id,Mobile,Sex,Wallet,LatestLogin_IP,LatestLogin_time,LatestLogin_Id,BankAccount,BankName,City,Province,BankMemo,RegisterDevice,RegisterUrl ,Name,QQ ) VALUES('" + user.Account + "','" + user.Birthday + "','" + user.Email + "','" + user.Id + "','" + user.Mobile + "','" + user.SexString + "'," + user.Wallet + ",'" + user.LatestLogin_IP + "','" + user.LatestLogin_time + "','" + user.LatestLogin_Id + "','" + user.BankAccount + "','" + user.BankName + "','" + user.City + "','" + user.Province + "','" + user.BankMemo + "','" + user.RegisterDevice + "', '" + user.RegisterUrl + "' ,'" + user.Name + "','" + user.QQ + "' );update infor set status=1 where id=" + item[1].ToString() + ";");

                    if (sql_list.Count == 100 || sql_list.Count==dt.Rows.Count % 100)
                    {
                        SQLiteHelper.SQLiteHelper.execSql(sql_list);
                        sql_list.Clear();
                    }

                    Console.WriteLine(string.Format("正在存储第{0}条数据，用户名{1} ",  count + "/" + dt.Rows.Count, item[0]));
                    count++;
                }


                //线程循环
                //Parallel.ForEach(dt.Rows.Cast<DataRow>(), (item) =>
                //{


                //    user = platGPK.GetUserDetail(item[0].ToString());
                //    Thread.Sleep(100);
                //    if (user == null)
                //    {
                //        //删掉就行了 更改状态为3
                //        sql = "update infor set status=3 where id='" + item[1].ToString() + "';";
                //        SQLiteHelper.SQLiteHelper.execSql(sql);
                //        //count++;
                //        return;
                //    }

                //    sql_list.Add(" INSERT or ignore   INTO detail(Account,Birthday,Email,Id,Mobile,Sex,Wallet,LatestLogin_IP,LatestLogin_time,LatestLogin_Id,BankAccount,BankName,City,Province,BankMemo,RegisterDevice,RegisterUrl ,Name,QQ ) VALUES('" + user.Account + "','" + user.Birthday + "','" + user.Email + "','" + user.Id + "','" + user.Mobile + "','" + user.SexString + "'," + user.Wallet + ",'" + user.LatestLogin_IP + "','" + user.LatestLogin_time + "','" + user.LatestLogin_Id + "','" + user.BankAccount + "','" + user.BankName + "','" + user.City + "','" + user.Province + "','" + user.BankMemo + "','" + user.RegisterDevice + "', '" + user.RegisterUrl + "' ,'" + user.Name + "','" + user.QQ + "' );update infor set status=1 where id='" + item[1].ToString() + "';");
                //    if (sql_list.Count == 100)
                //    {
                //        SQLiteHelper.SQLiteHelper.execSql(sql_list);
                //        sql_list.Clear();
                //    }
                //    Console.WriteLine(string.Format("正在获取第{0}条数据，用户名{1} ", count + "/" + dt.Rows.Count, item[0]));
                //    count++;
                //});

                return true;
            }
            catch (Exception ex)
            {
                appSittingSet.Log(ex.Message);
                return false;
            }




        }

    }
}
