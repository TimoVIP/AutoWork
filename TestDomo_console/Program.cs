using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using TimoControl;
using WebSocketSharp;
using System.Web.UI;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Windows.Forms;
using mshtml;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Collections;
using System.Configuration;
using System.Xml.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace TestDomo_console
{
    class Program
    {
        static void Main(string[] args)
        {
            bool b;
            //FileInfo[] fs = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "log").GetFiles("*.txt", SearchOption.AllDirectories);
            //string filepath= AppDomain.CurrentDomain.BaseDirectory + "log\\等级更新失败的记录.md";
            //foreach (var file in fs)
            //{
            //    File.AppendAllLines(filepath, from line in File.ReadAllLines(file.FullName, Encoding.Default) where (line.Contains("需要手动更新")) select line, Encoding.Default);
            //}
            /*
            StringBuilder sb = new StringBuilder();
            foreach (var file in fs)
            {

                string[] lines = File.ReadAllLines(file.FullName, Encoding.Default);
                foreach (var line in lines)
                {
                    if (line.Contains("需要手动更新"))
                    {
                        sb.AppendLine(line);
                    }
                }
                Console.WriteLine(file.Name);
            }
            File.AppendAllText(filepath, sb.ToString(), Encoding.Default);
            */
            //Console.WriteLine("处理完毕");



            //betData bb = new betData()
            //{
            //    bbid = "915882",
            //    wallet = "100",
            //    username = "tanxi",
            //    lastOprTime = DateTime.Now.AddDays(-DateTime.Now.Day + 1).ToString("yyyy/MM/dd") + " 12:00:00",
            //    lastCashTime = "2019/02/03 03:16:02",//开始时间
            //    betTime = DateTime.Now.AddHours(12).ToString("yyyy/MM/dd HH:mm:ss"),
            //    betMoney = 2,
            //    betno = "356739235433",
            //    //GameCategories = "[\"BBINbbsport\",\"BBINlottery\",\"BBINvideo\",\"SabaSport\",\"SabaNumber\",\"SabaVirtualSport\",\"AgBr\",\"Mg2Real\",\"Pt2Real\",\"GpiReal\",\"SingSport\",\"AllBetReal\",\"IgLottery\",\"IgLotto\",\"Rg2Real\",\"Rg2Board\",\"Rg2Lottery\",\"Rg2Lottery2\",\"JdbBoard\",\"EvoReal\",\"BgReal\",\"GdReal\",\"Pt3Real\",\"SunbetReal\",\"CmdSport\",\"Sunbet2Real\",\"Mg3Real\",\"KgBoard\",\"LxLottery\",\"EBetReal\",\"ImEsport\",\"OgReal\",\"VrLottery\",\"City761Board\",\"FsBoard\",\"SaReal\",\"ImsSport\",\"IboSport\",\"NwBoard\",\"JsBoard\",\"ThBoard\"]",
            //    Audit = 5,
            //    AuditType = "Discount",
            //    Type = 5,
            //};

            betData tanxi1 = new betData()
            {
                username = "bwybwy888",
                //lastCashTime = "2019/02/13 03:16:02",//开始时间
                //lastOprTime = "2019/02/22 12:59:59",//结束时间
            };

            betData tanxi2 = new betData()
            {
                username = "tanxi",
                lastCashTime = "2019/02/13 03:16:02",//开始时间
                lastOprTime = "2019/02/22 12:59:59",//结束时间
                GameCategories = "[\"BBINbbsport\",\"BBINlottery\",\"BBINvideo\",\"SabaSport\",\"SabaNumber\",\"SabaVirtualSport\",\"AgBr\",\"Mg2Real\",\"Pt2Real\",\"GpiReal\",\"SingSport\",\"AllBetReal\",\"IgLottery\",\"IgLotto\",\"Rg2Real\",\"Rg2Board\",\"Rg2Lottery\",\"Rg2Lottery2\",\"JdbBoard\",\"EvoReal\",\"BgReal\",\"GdReal\",\"Pt3Real\",\"SunbetReal\",\"CmdSport\",\"Sunbet2Real\",\"Mg3Real\",\"KgBoard\",\"LxLottery\",\"EBetReal\",\"ImEsport\",\"OgReal\",\"VrLottery\",\"City761Board\",\"FsBoard\",\"SaReal\",\"ImsSport\",\"IboSport\",\"NwBoard\",\"JsBoard\",\"ThBoard\"]",
            };

            betData bb = new betData()
            {
                username = "liuling6600",
                bbid = "1728654",
                passed = false,
                msg = "此活动需存款1元以上，并且电子游艺的有效投注达到100元以上! test",
                lastCashTime = "2019/04/18 22:00:00",//开始时间
                lastOprTime = "2019/04/18 23:59:59",//结束时间
            };



            //b = platGPK.loginGPK();
            //tanxi1.betTime = "";
            //tanxi1.Types = new string[] { "Account", "Manual", "ThirdPartyPayment", "OnlineWithdraw" };
            //tanxi1 = platGPK.MemberTransactionSearch(tanxi1);

            //SoketObjetRecordQuery o = platGPK.BetRecordGetInfo(bb);

            //string[] KindCategories = appSittingSet.readAppsettings("KindCategories").Split('|');//游戏分类
            //foreach (var s in KindCategories)
            //{
            //    bb.GameCategories += platGPK.KindCategories[int.Parse(s)] + ",";
            //}
            //bb.GameCategories = "["+  bb.GameCategories.TrimEnd(',') + "]";



            //b = platWSB.login();
            //List<betData> list = platWSB.getData();
            //foreach (var item in list)
            //{
            //    //来自网页 存入数据库，确认掉
            //    string sql = string.Format("INSERT INTO [t_data] ([oid]  ,[username] ,[deposit]  ,[state]) VALUES ({0} ,{1} ,{2} ,0)", item.betno, item.username, item.betMoney);
            //    int i = sqlHelper.ExecuteNonQuery(sql);
            //}


            //platWSB.confirmAct(list[0]);


            //游戏列表
            //List<string> list = platGPK.GetKindCategories();
            //tanxi1.GameCategories = "[" + platGPK.KindCategories[0] +","+ platGPK.KindCategories[1] + "," + platGPK.KindCategories[2] + "," + platGPK.KindCategories[3] + "," + platGPK.KindCategories[4] + "," + platGPK.KindCategories[5] + "]";

            //tanxi2.GameCategories = "[" + platGPK.KindCategories[3] + "," + platGPK.KindCategories[5] + "]";



            //string sr = platGPK.GetUserLoadHistory(new Gpk_UserDetail() { Account = "weiyb747" ,Id= "2164365" }, "申请加入会员,建立会员,建立银行帐户资讯", 10);
            //string sr = platGPK.GetUserLoadHistory(new Gpk_UserDetail() { Account = "tanxi" ,Id= "2164365" }, "申请加入会员,建立会员,建立银行帐户资讯", 10);
            //if (sr != "OK")
            //{
            //    Console.WriteLine("111");
            //}

            //string sr = platGPK.GetUserLoadHistory(new Gpk_UserDetail() { Account = "feng1o22", Id = "1000472" }, "申请加入会员,建立会员,建立银行帐户资讯", 10);

            //DateTime dt1 = DateTime.Parse("2018/07/05 16:20:54");
            //DateTime dt2 = DateTime.Parse("2018/07/05 16:19:08");
            //double d =  (dt2 - dt1).TotalSeconds ;
            // d =  (dt2 - dt1).Duration().TotalSeconds ;

            //string aa = platGPK.GetUserLoadHistory(new Gpk_UserDetail() { Account = "feng1o22", Id = "1000472" }, "申请加入会员,建立会员,建立银行帐户资讯", 10);


            //platGPK.MemberUpdateMemberState(new Gpk_UserDetail() { Account = "siyue5266521", Id = "2164836", SexString = "0" });
            /*
            //启动sokect
            platGPK.SaveSocket2DB();

            //2次投注记录 有效投注 是否相等 
            //发送一次 全部的查询
            tanxi1.GameCategories = "[" + platGPK.KindCategories[1] + "]";
            object o = platGPK.BetRecordSearch(tanxi1);
            //发送一次 电子的查询
            tanxi1.GameCategories = "[" + platGPK.KindCategories[0] + "]";
            o = platGPK.BetRecordSearch(tanxi1);
            Thread.Sleep(500);
            //查询一次数据库 看是否符合
            o = platGPK.getSoketDataFromDbCompare();

            */


            //开启 会员不经区域验证登入
            //platGPK.UpdateMemberLoginEveryWhere(new Gpk_UserDetail() { Id = "997348", SexString = "true" });

            //platGPK.UpdateCrossRegionLogin(new Gpk_UserDetail() { Id = "997348", SexString = "flase" });

            //platGPK.SaveSocket2DB();
            //platGPK.BetRecordSearch(tanxi1);
            //Thread.Sleep(2000);
            //platGPK.BetRecordSearch(tanxi2);

            //SoketObj_etRecordQuery so =  platGPK.getSoketDataFromDb();


            ////bb = platGPK.checkInGPK_transaction(bb);
            //b = platGPK.submitToGPK(bb, "测试活动 消除");
            ////bb = platGPK.checkInGPK(bb);
            ////bool b = platBB.loginBB();
            //bb = platGPK.GetDetailInfo(bb);
            //bb = platGPK.GetDetailInfo_withoutELE(bb);
            //b = platGPK.submitToGPK(bb);
            //b = platBB.loginBB_old();
            //b = platBB.loginBB();
            //bb = platBB.getBet_Details(bb);
            //bb = platBB.getBetDetail_Bet_Times(bb);

            //b = platACT2.login();
            //List<betData> list = platACT2.getActData();

            //string[] a4 = appSittingSet.readAppsettings("Act4Set").Split(new char[] { '|', '@' }, StringSplitOptions.RemoveEmptyEntries);

            //object o = platGPK.BetRecordSearch(bb);



            //读取EXCEL文件

            //调用接口处理数据

            //重置密码

            //提现
            //platGPK.ResetPassword(new Gpk_UserDetail() { Id = "997348" });


            //string[] aa = { "BBINbbsport", "BBINlottery", "BBINvideo", "SabaSport", "SabaNumber", "SabaVirtualSport", "AgBr", "Mg2Real", "Pt2Real", "GpiReal", "SingSport", "AllBetReal", "IgLottery", "IgLotto", "Rg2Real", "Rg2Board", "Rg2Lottery", "Rg2Lottery2", "JdbBoard", "EvoReal", "BgReal", "GdReal", "Pt3Real", "SunbetReal", "CmdSport", "Sunbet2Real", "Mg3Real", "KgBoard", "LxLottery", "EBetReal", "ImEsport", "OgReal", "VrLottery", "City761Board", "FsBoard", "SaReal", "ImsSport", "IboSport", "NwBoard", "JsBoard", "ThBoard" };
            //Console.WriteLine(aa.ToString());


            //platGPK.GetDetailInfo_withoutELE( new betData() {  username= "YJ7654321", lastCashTime= "2019/02/03", lastOprTime= "2019/02/04" });

            //platGPK.GetDetailInfo_withoutELE(new betData() { username = "YJ7654321", lastCashTime = "2019/02/03 20:00:00", lastOprTime = "2019/02/04" });








            #region websoket 测试
            /*

            //获取ws 连接的数据
            //string token = System.Web.HttpUtility.UrlEncode(platGPK.getNegotiate()[0]);
            //string connectionId =  platGPK.getNegotiate()[1];

            string token = System.Web.HttpUtility.UrlEncode(platGPK.connectionToken);
            string baseUrl = "http://sts.tjuim.com/";
            string url = "ws://" + baseUrl.Replace("http://", "").Replace("/", "") + "/signalr/connect?transport=webSockets&clientProtocol=1.5&connectionToken=" + token + "&connectionData=%5B%7B%22name%22%3A%22mainhub%22%7D%5D&tid=8";
            //ws://sts.tjuim.com/signalr/connect?transport=webSockets&clientProtocol=1.5&connectionToken= &connectionData=%5B%7B%22name%22%3A%22mainhub%22%7D%5D&tid=10
            //url = System.Web.HttpUtility.UrlEncode(url);
            WebSocketSharp.WebSocket ws = new WebSocketSharp.WebSocket(url);
            ws.Origin = baseUrl;
            foreach (System.Net.Cookie item in platGPK.cookie.GetCookies(new Uri(baseUrl)))
            {
                ws.SetCookie(new WebSocketSharp.Net.Cookie(item.Name, item.Value, item.Path, item.Domain));
            }
            
            ws.OnOpen += (sender, e) =>
            {
                Console.WriteLine("Open");
            };
            ws.OnMessage += (sender, e) =>
            {
                //Console.WriteLine("Laputa says: " + e.Data);
                appSittingSet.txtLog(e.Data);
                if (e.Data.Contains("MainHub") && e.Data.Contains("BetRecordQueryCtrl_searchComplete"))
                {
                    JObject jo = JObject.Parse(e.Data);

                    if (jo["M"][0]["M"].ToString() == "BetRecordQueryCtrl_searchComplete")
                    {
                        //SoketObj_etRecordQuery so = new SoketObj_etRecordQuery()
                        //{
                        //    Count = (int)jo["M"][0]["A"][0]["Count"],
                        //    TotalBetAmount = (decimal)jo["M"][0]["A"][0]["TotalBetAmount"],
                        //    TotalCommissionable = (decimal)jo["M"][0]["A"][0]["TotalCommissionable"],
                        //    TotalPayoff = (decimal)jo["M"][0]["A"][0]["TotalPayoff"],
                        //};
                        //Console.WriteLine(jo["M"][0]["A"][0]["Count"]);
                        ////Console.WriteLine(jo["M"][0]["A"][0]["TotalBetAmount"]);
                        //Console.WriteLine(jo["M"][0]["A"][0]["TotalCommissionable"]);
                        //Console.WriteLine(jo["M"][0]["A"][0]["TotalPayoff"]);

                        //保存到数据库
                        string sql = string.Format("INSERT INTO record (username,gamename,subtime,betno,chargeMoney,pass,msg,aid) VALUES ( '{0}', '', datetime(CURRENT_TIMESTAMP,'localtime'), '{1}', {2}, 0, '{3}', 1002 );",tanxi1.username, jo["M"][0]["A"][0]["Count"], jo["M"][0]["A"][0]["TotalCommissionable"], jo["M"][0]["A"][0]["TotalPayoff"]);
                        appSittingSet.execSql(sql);
                        //停止
                        //ws.Close();
                    }
                }

            };

            ws.OnError += (sender, e) =>
            {
                Console.WriteLine(e.Message);
            };
            ws.OnClose += (sender, e) =>
            {
                Console.WriteLine(e.Code);
            };

            ws.Connect();

            //for (int i = 0; i < 4; i++)
            //{
            //    object o = null;
            //    if (1==i)
            //    {
            //        o = platGPK.BetRecordSearch(tanxi1);
            //    }
            //    else 
            //    {
            //        o = platGPK.BetRecordSearch(tanxi2);
            //    }

            //        if (o==null)
            //        {
            //            Console.WriteLine("发送查询失败 " +DateTime.Now );
            //        }
            //        else 
            //        {
            //            Console.WriteLine("发送查询成功,存在记录" + o + " " + DateTime.Now);
            //        }
            //    Thread.Sleep(5000);
            //}

            ws.Close();

            */

            #endregion




            #region
            //Console.WriteLine("请选择：1全部更新；2全部取回；3人工提出");
            //string sw= Console.ReadLine();
            //if (sw=="1")
            //{

            //}
            //else if (sw == "2")
            //{

            //}
            //else if (sw == "2")
            //{

            //}
            //Console.WriteLine(sw);
            #endregion


            //List<string> list = new List<string>();
            //list.Add("ssss");
            //list.Add("aaaa");

            //string test = list.Find(o => o == "ssss");


            //List<string> list = platGPK.GetKindCategories();

            //获取详细信息
            //Gpk_UserDetail userinfo = platGPK.GetUserDetail(tanxi1.username);
            ////发送站内信 14点09分 2019年4月2日
            //SendMailBody mail = new SendMailBody() { MailBody = "<p>您申请的体验金已经送至你的账户</p>", Subject = "您申请的体验金已经送至你的账户", SendMailType = "1", MailRecievers = userinfo.Account };
            //platGPK.SiteMailSendMail(mail);


            //
            //string[]  sLuckNum = appSittingSet.readAppsettings("LuckNum").Split('|');//3@1@88 格式  33333333@888@88888

            //string[] gameNames = appSittingSet.readAppsettings("gameName").Split('|');
            //betData bb = new betData() { betno= "380944340333", betMoney= 2, aname= "连环夺宝" };

            //foreach (string s in sLuckNum)
            //{
            //    if (bb.betno.EndsWith(s.Split('@')[0]))
            //    {
            //        bb.betMoney *= decimal.Parse(s.Split('@')[1]);
            //        if (bb.betMoney > decimal.Parse(s.Split('@')[2]))
            //        {
            //            bb.betMoney = decimal.Parse(s.Split('@')[2]);
            //        }
            //        break;
            //    }
            //}

            //foreach (var g in gameNames[1].Split('@'))
            //{
            //    if (g == bb.gamename)
            //    {
            //        //flag2 = true;
            //        break;
            //    }
            //}






            //List<betData> list = ActFromDB.getActData("30");
            /*

            string[] KindCategories = appSittingSet.readAppsettings("KindCategories").Split('|');//游戏分类

            foreach (var s in KindCategories)
            {
                bb.GameCategories += platGPK.KindCategories[int.Parse(s)] + ",";
            }
            bb.GameCategories = "[" + bb.GameCategories.TrimEnd(',') + "]";

            betData bb1 = new betData();
            betData bb2 = new betData();
            bb.gamename = null;
            bb.lastCashTime = DateTime.Now.Date.AddDays(-4).ToString("yyyy/MM/dd");
            bb.lastOprTime = DateTime.Now.Date.AddDays(-2).ToString("yyyy/MM/dd");


            object ba = platGPK.BetRecordSearch(bb);
            foreach (var s in KindCategories)
            {

                bb.gamename += "&types=" + string.Join("&types=", platGPK.KindCategories[int.Parse(s)].Replace("\"","").Split(',').Skip(1).ToArray());
            }

            bb1 = platGPK.GetDetailInfo(bb);
            bb.gamename = "";
            foreach (var s in platGPK.KindCategories)
            {
                bb.gamename += "&types=" + string.Join("&types=", s.Replace("\"", "").Split(',').Skip(1).ToArray());
            }
            bb2 = platGPK.GetDetailInfo(bb);

            if (Math.Abs( bb1.total_money -bb2.total_money) >10)
            {
                //拒绝

            }

            */




            //Hashtable myEndpointConfig = (Hashtable)ConfigurationManager.GetSection("appconfig");

            /*
            string filePath = Application.ExecutablePath + ".config";
            var xe = XElement.Load(filePath);//改成xml的路径，或者用流，具体参考MSDN
            var aa = xe.Element("appconfig").Attribute("configSource").Value;
            var xns = xe.GetDefaultNamespace();
            var minlevelattr = xe.Descendants(xns + "rules").Elements(xns + "logger").Attributes("minlevel").FirstOrDefault();
            if (minlevelattr != null)
            {
                minlevelattr.Value = "debug"; //改成你想要的值
            }
            xe.Save("t.xml");//保存路径

            */


            //ConfigurationManager.GetSection("appconfig")

            //Console.WriteLine(myEndpointConfig["GPK"]);

            /*
            b =ActDataFromWeb.login();
            //List<betData> list = ActDataFromWeb.getActData("39");
            //b = ActDataFromWeb.confirmAct(bb);
            bb.username = "dtgaiys2017";
            bb.aid = "38";
            bb = ActDataFromWeb.getData2_time(bb);
           b = platACT.loginActivity();


            //platACT.getActData("40");
            //List<betData>  list = platACT.getActData("53");


    */
            //string postUrl = "http://2019.ip138.com/ic.asp";
            //HttpWebRequest request = WebRequest.Create(postUrl) as HttpWebRequest;
            //HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            //StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.Default);
            //var ret_html = reader.ReadToEnd().ToString();
            //var ip = ret_html.Substring(ret_html.IndexOf("[") + 1, (ret_html.LastIndexOf("]") - ret_html.IndexOf("[") - 1));
            //var ip1 = Regex.Match(ret_html, @"(?i)(?<=\[)(.*)(?=\])");

            //b = platBB.loginBB();
            //Console.WriteLine(b);
            //b = platBB.loginBB_old();
            //Console.WriteLine(b);
            //List<betData> list = platACT.getActData("53");

            //bool e = platGPK.loginGPK();
            //Dictionary<string, string> postData = new Dictionary<string, string>();
            //postData.Add("Name", "当日红包");
            //postData.Add("Description", "心想事成、多多盈利");
            //postData.Add("StartTime", "2019/09/05 06:40:00");
            //postData.Add("EndTime", "2019/09/05 23:59:59");
            //postData.Add("Password", "123456");
            //e = platGPK.RedEnvelopeManagement_GetExcelSum($"{Environment.CurrentDirectory}\\data.xlsx",postData);




            //object jo = JsonConvert.DeserializeObject("[]");
            //JArray ja = JArray.FromObject(jo);
            //foreach (var item in ja)
            //{
            //    Console.WriteLine(item["Label"].ToString());
            //}

            b =  platQPGV2.login();
           List<betData> list =  platQPGV2.getActData();

                Console.ReadLine();

        }
    }
}
