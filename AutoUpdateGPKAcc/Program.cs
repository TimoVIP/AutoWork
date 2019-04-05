using TimoControl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutoUpdateGPKAcc
{
    class Program
    {

        static void Main(string[] args)
        {

            bool f = platGPK.loginGPK();
            if (f)
            {
                Console.WriteLine("登录成功");

                DisableUser2();
                //DisableUser3();

                /*
                for (int i = 1; i <= 100; i++)
                {

                    Thread th = new Thread(dowork);
                    th.IsBackground = true;
                    th.Name = "线程" + i;
                    th.Start(i);
                }
                */

                Console.WriteLine("执行完毕");
                Console.Read();
            }
        }


        private static void dowork(object index)
        {
            string msg = "";
            for (int i = 149000; i < 150000; i++)
            {

                if (i % (int)index == 0)
                {
                    bool b = platGPK.autoUpadateMemberAcc(i.ToString());
                    string msg1 = string.Format(" 线程{2} 更新id {0}-{1}-{3}", i, b ? "完毕" : "失败", index, DateTime.Now.ToString());
                    Console.WriteLine(msg1);
                    appSittingSet.Log(msg1);
                }
            }
        }

        private static void DisableUser()
        {
            List<Gpk_UserInfo> user_list = new List<Gpk_UserInfo>();
            Gpk_UserDetail userinfo = null;

            //遍历所有会员
            for (int i = 1; i < 170000; i++)
            //Parallel.For(147, 170000, (i, loopState) =>
            {
                Console.WriteLine("处理页数:" + i);
                user_list = platGPK.getUserListByPage(i, false, DateTime.Now.Date.ToShortDateString());
                Parallel.ForEach(user_list, (item) =>
                //foreach (var item in user_list)
                {
                    string msg = "处理账号" + item.Account;
                    bool b = false;
                    userinfo = platGPK.GetUserDetail(item.Account);
                    if (userinfo.BankAccount == null)
                    {
                        Console.WriteLine(msg);
                        //continue;
                        return;
                    }
                    //userinfo = new Gpk_UserDetail() { Account = item.Account, Id = i.ToString(), MemberLevelSettingId = item.MemberLevelSettingName };
                    //历史记录 注册 和绑定银行卡的时间间隔 小于10m 为机器注册 id
                    string sr = platGPK.GetUserLoadHistory(userinfo, "申请加入会员,建立会员,建立银行帐户资讯", 10);
                    if (sr.Contains("同IP其他"))
                    {
                        //充值0.01 很多信息
                        betData bb = new betData() { username = userinfo.Account, betMoney = 0.01M, AuditType = "None", Memo = "账号停用", isReal = false, Type = 99, aname = "停用会员", aid = "1003" };
                        b = platGPK.MemberDepositSubmit(bb);
                        msg += string.Format("存款0.01{0} ", b);
                        //停用账号 id 
                        userinfo.SexString = "0";
                        platGPK.MemberUpdateMemberState(userinfo);
                        msg += "停用账号操作 ";
                        //更新层级=》黑名单 id levelid
                        if (userinfo.MemberLevelSettingId != "14")
                        {
                            userinfo.MemberLevelSettingId = "14";
                            b = platGPK.UpadateMemberLevel("1", userinfo);
                            msg += "层级操作 " + b;
                        }
                        appSittingSet.Log(msg);
                    }
                    Console.WriteLine(msg);
                }
                );

            };
        }

        private static void DisableUser2()
        {
            List<Gpk_UserInfo> user_list = new List<Gpk_UserInfo>();
            Gpk_UserDetail userinfo = null;
            string msg;
            bool b;

            //遍历所有会员ID号
            for (int i = 2188116; i > 1000000; i--)
            {
                msg = "处理用户编号 " + i;
                userinfo = new Gpk_UserDetail() { Id = i.ToString(), Account = "tanxi", SexString = "0", MemberLevelSettingId = "14" };//tanxi 测试数据
                //历史记录 注册 和绑定银行卡的时间间隔 小于10m 为机器注册 id
                string sr = platGPK.GetUserLoadHistory(userinfo, "申请加入会员,建立会员,建立银行帐户资讯", 10);
                if (sr.Contains("同IP其他"))
                {
                    //停用账号 id 
                    platGPK.MemberUpdateMemberState(userinfo);
                    msg += "停用账号 ";
                    //更新层级=》黑名单 id levelid
                    b = platGPK.UpadateMemberLevel("1", userinfo);
                    msg += "层级操作 " + b;
                    appSittingSet.Log(msg);
                }
                Console.WriteLine(msg);
                //appSittingSet.Log(i.ToString(), "note.txt");
            }
            
        }

        private static void DisableUser3()
        {
            List<Gpk_UserInfo> user_list = new List<Gpk_UserInfo>();
            Gpk_UserDetail userinfo = null;
            string msg;
            bool b;

            //遍历所有会员ID号
            //不能从大到小
            Parallel.For(1000000, 2288116, (i, loopState) => {
                msg = "处理用户编号 " + i;
                userinfo = new Gpk_UserDetail() { Id = i.ToString(), Account = "tanxi", SexString = "0", MemberLevelSettingId = "14" };//tanxi 测试数据
                //历史记录 注册 和绑定银行卡的时间间隔 小于10m 为机器注册 id
                string sr = platGPK.GetUserLoadHistory(userinfo, "申请加入会员,建立会员,建立银行帐户资讯", 10);
                if (sr.Contains("同IP其他"))
                {
                    //停用账号 id 
                    platGPK.MemberUpdateMemberState(userinfo);
                    msg += "停用账号 ";
                    //更新层级=》黑名单 id levelid
                    userinfo.MemberLevelSettingId = "14";
                    b = platGPK.UpadateMemberLevel("1", userinfo);
                    msg += "层级操作 " + b;
                    appSittingSet.Log(msg);
                }
                Console.WriteLine(msg);
            });
        }
    }
}
