using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimoControl;

namespace GetInfor
{
    class Program
    {
       static string[] connectionString = appSittingSet.readAppsettings("MySqlConnect").Split('|');
        static Gpk_UserDetail user=null;
        static string sql = "";
        static bool b ;
        static void Main(string[] args)
        {
            //List<string> sql_list = new List<string>();
            //for (int i = 0; i < 10; i++)
            //{
            //    sql_list.Add(string.Format("insert into infor (account)  values('aa{0}');", i));
            //}
            //appSittingSet.execSql(sql_list);

            //return;

            //b = ToDB();
            b = true;
            if (b)
            {
                platGPK.loginGPK();
                Console.WriteLine("开始处理详细数据");
                ToDB2();
            }
            Console.ReadLine();


            //存入mysql库

        }

        private static bool ToDB()
        {
            sql = "select  DISTINCT username from e_submissions where `status`=1 ORDER BY id desc;";
            //获取用户名 
            MySQLHelper.connectionString = connectionString[0];
            DataTable dt = MySQLHelper.Query(sql).Tables[0];
            int count = 1;
            List<string> sql_list = new List<string>();
            foreach (DataRow item in dt.Rows)
            {
                sql_list.Add(string.Format("insert  or ignore  into infor (account)  values('{0}');", item[0]));
                if (count % 1000 == 0 || count==dt.Rows.Count-1)
                {
                    appSittingSet.execSql(sql_list);
                    sql_list.Clear();
                }
                count++;
                Console.WriteLine("正在获取 "+count+"/" + dt.Rows.Count);
            }
            /*
            Parallel.ForEach(dt.Rows.Cast<DataRow>(), (item) => {
                sql_list.Add(string.Format("insert  or ignore  into infor (account)  values('{0}');", item[0]));
                if (count % 1000 == 0 || count==dt.Rows.Count-1)
                {
                    appSittingSet.execSql(sql_list);
                    sql_list.Clear();
                }
                count++;
            });
            */

            return true;
        }

        private static bool ToDB2()
        {
            sql = "select account,id from infor where status=0 order by jointime ;";
            DataTable dt = appSittingSet.getDataTableBySql(sql);
            int count = 1;
            List<string> sql_list = new List<string>();

            foreach (DataRow item in dt.Rows)
            {
                user = new Gpk_UserDetail();
                user = platGPK.GetUserDetail(item[0].ToString());
                //sql = " INSERT INTO detail(Account,Birthday,Email,Id,Mobile,Sex,Wallet,LatestLogin_IP,LatestLogin_time,LatestLogin_Id,BankAccount,BankName,City,Province,BankMemo,RegisterDevice,RegisterUrl  ) VALUES('" + user.Account + "','" + user.Birthday + "','" + user.Email + "','" + user.Id + "','" + user.Mobile + "','" + user.SexString + "'," + user.Wallet + ",'" + user.LatestLogin_IP + "','" + user.LatestLogin_time + "','" + user.LatestLogin_Id + "','" + user.BankAccount + "','" + user.BankName + "','" + user.City + "','" + user.Province + "','" + user.BankMemo + "','" + user.RegisterDevice + "', '" + user.RegisterUrl + "'  );update infor set status=1 where id='" + item[1].ToString() + "';";
                //b = appSittingSet.execSql(sql);
                Console.WriteLine("处理详细数据 " + item[1]);
                if (user==null)
                {
                    //删掉就行了 更改状态为3
                    sql = "update infor set status=3 where id='" + item[1].ToString() + "';";
                    appSittingSet.execSql(sql);
                    continue;
                }

                sql_list.Add(" INSERT INTO detail(Account,Birthday,Email,Id,Mobile,Sex,Wallet,LatestLogin_IP,LatestLogin_time,LatestLogin_Id,BankAccount,BankName,City,Province,BankMemo,RegisterDevice,RegisterUrl  ) VALUES('" + user.Account + "','" + user.Birthday + "','" + user.Email + "','" + user.Id + "','" + user.Mobile + "','" + user.SexString + "'," + user.Wallet + ",'" + user.LatestLogin_IP + "','" + user.LatestLogin_time + "','" + user.LatestLogin_Id + "','" + user.BankAccount + "','" + user.BankName + "','" + user.City + "','" + user.Province + "','" + user.BankMemo + "','" + user.RegisterDevice + "', '" + user.RegisterUrl + "'  );update infor set status=1 where id='" + item[1].ToString() + "';");
                if (count % 100 == 0 || count == dt.Rows.Count - 1)
                {
                    appSittingSet.execSql(sql_list);
                    sql_list.Clear();
                }
                count++;
                Console.WriteLine("正在获取 " + count + "/" + dt.Rows.Count);
            }
            /*
            //线程循环
            Parallel.ForEach(dt.Rows.Cast<DataRow>(), (item) =>
            {
                user = platGPK.GetUserDetail(item[0].ToString());
                //sql = " INSERT INTO detail(Account,Birthday,Email,Id,Mobile,Sex,Wallet,LatestLogin_IP,LatestLogin_time,LatestLogin_Id,BankAccount,BankName,City,Province,BankMemo,RegisterDevice,RegisterUrl  ) VALUES('" + user.Account + "','" + user.Birthday + "','" + user.Email + "','" + user.Id + "','" + user.Mobile + "','" + user.SexString + "'," + user.Wallet + ",'" + user.LatestLogin_IP + "','" + user.LatestLogin_time + "','" + user.LatestLogin_Id + "','" + user.BankAccount + "','" + user.BankName + "','" + user.City + "','" + user.Province + "','" + user.BankMemo + "','" + user.RegisterDevice + "', '" + user.RegisterUrl + "'  );update infor set status=1 where id='" + item[1].ToString() + "';";
                //b = appSittingSet.execSql(sql);
                Console.WriteLine("处理详细数据 " + item[1]);


                sql_list.Add(" INSERT INTO detail(Account,Birthday,Email,Id,Mobile,Sex,Wallet,LatestLogin_IP,LatestLogin_time,LatestLogin_Id,BankAccount,BankName,City,Province,BankMemo,RegisterDevice,RegisterUrl  ) VALUES('" + user.Account + "','" + user.Birthday + "','" + user.Email + "','" + user.Id + "','" + user.Mobile + "','" + user.SexString + "'," + user.Wallet + ",'" + user.LatestLogin_IP + "','" + user.LatestLogin_time + "','" + user.LatestLogin_Id + "','" + user.BankAccount + "','" + user.BankName + "','" + user.City + "','" + user.Province + "','" + user.BankMemo + "','" + user.RegisterDevice + "', '" + user.RegisterUrl + "'  );");
                if (count % 500 == 0 || count == dt.Rows.Count - 1)
                {
                    appSittingSet.execSql(sql_list.ToString(), true);
                    sql_list.Clear();
                }
                count++;
            });
            */
            return true;
        }
    }
}
