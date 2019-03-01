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
            string type = "0";

            Console.Title = "自动处理程序";
            if (fs.Length>0)
            {
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
                    Console.WriteLine("发现待处理文件 " + file.FullName);
                }
                Console.WriteLine("请选择：1全部更新；2全部取回；3人工提出；4重置密码；0退出");
               type = Console.ReadLine();
                if (type == "0")
                {
                    Environment.Exit(0);
                }

            }
            else
            {
                Console.WriteLine("没有找到对应的.xlsx文件，按任意键退出");
                Console.ReadKey(true);
            }
            
            foreach (var file in fs)
            {
                Console.WriteLine("开始处理文件"+file.FullName);       
                //读取EXCEL文件
                dt = npoi.ExcelToDataTable(true, "", file.FullName);
                //调用接口处理数据
                if (dt==null)
                {
                    continue;
                }
                foreach (DataRow dr in dt.Rows)
                {
                    Gpk_UserDetail user = platGPK.GetUserDetail(dr[0].ToString());
                    if (user==null)
                    {
                        continue;
                    }

                    if (type == "1")
                    {
                        //全部更新
                        platGPK.autoUpadateMemberAcc(user.Id);
                    }
                    else if (type == "2")
                    {
                        //全部取回
                        platGPK.AllWalletBackMember(user);
                    }
                    else if (type == "3")
                    {
                        //提现 人工提出
                        decimal d = 0;
                        decimal.TryParse(dr[1].ToString(), out d);
                        user.Wallet = d;
                        platGPK.WithdrawSubmit(user);
                    }
                    else if (type == "4")
                    {
                        //重置密码
                        platGPK.ResetPassword(user);
                    }

                    //if (dr.ItemArray.Length == 2 )
                    //{
                    //    decimal d = 0;
                    //    decimal.TryParse(dr[1].ToString(), out d);
                    //    user.Wallet = d;
                    //    //全部取回
                    //    //platGPK.AllWalletBackMember(user);
                    //    //提现
                    //    platGPK.WithdrawSubmit(user);
                    //}
                    //else
                    //{
                    //    //重置密码
                    //    platGPK.ResetPassword(user);
                    //}

                    Console.WriteLine("处理" + dr[0]);
                }

                file.Delete();
                Console.WriteLine("处理完毕,按任意键退出...");        
                
            }


            //Console.ReadKey();
        }
    }
}
