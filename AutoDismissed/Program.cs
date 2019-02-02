using System;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using TimoControl;
namespace AutoDismissed
{
    class Program
    {
        static void Main(string[] args)
        {
            NPOIExcel npoi = new NPOIExcel();
            DataTable dt = new DataTable();
            //遍历文件
            DirectoryInfo di = new DirectoryInfo(Environment.CurrentDirectory);
            FileInfo[] fs = di.GetFiles("*.xlsx", SearchOption.AllDirectories);

            //先登陆一遍
            bool b= platGPK.loginGPK();
            if (!b)
            {
                Console.WriteLine("GPK登陆失败");
                return;
            }
            else
            {
                Console.WriteLine("GPK登陆成功");                    
            }

            foreach (var file in fs)
            {
                Console.WriteLine("开始处理文件"+file.FullName);       
                //读取EXCEL文件
                dt = npoi.ExcelToDataTable(true, "", file.FullName);
                //调用接口处理数据

                /*
                Parallel.ForEach(dt.AsEnumerable(), (dr) =>
                {
                    Gpk_UserDetail user = platGPK.GetUserDetail(dr[0].ToString());
                    if (user != null)
                    {
                        decimal d = 0;
                        decimal.TryParse(dr[1].ToString(), out d);
                        user.Wallet = d;
                        //全部取回
                        platGPK.AllWalletBackMember(user);
                        //重置密码
                        platGPK.ResetPassword(user);
                        //提现
                        platGPK.WithdrawSubmit(user);
                    }

                    Console.WriteLine("处理" + dr[0]);
                });
                */
 

                
                foreach (DataRow dr in dt.Rows)
                {
                    Gpk_UserDetail user = platGPK.GetUserDetail(dr[0].ToString());
                    if (user==null)
                    {
                        continue;
                    }
                    if (dr.ItemArray.Length == 2 )
                    {
                        decimal d = 0;
                        decimal.TryParse(dr[1].ToString(), out d);
                        user.Wallet = d;
                        //全部取回
                        //platGPK.AllWalletBackMember(user);
                        //提现
                        platGPK.WithdrawSubmit(user);
                    }
                    else
                    {
                        //重置密码
                        platGPK.ResetPassword(user);
                    }
                    Console.WriteLine("处理" + dr[0]);
                }

                file.Delete();
                Console.WriteLine("处理完毕,按任意键退出...");        
                
            }


            Console.ReadKey();
        }
    }
}
