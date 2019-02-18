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
            //    lastCashTime = "2019/02/03 03:16:02",
            //    betTime = DateTime.Now.AddHours(12).ToString("yyyy/MM/dd HH:mm:ss"),
            //    betMoney = 2,
            //    betno = "356739235433",
            //    GameCategories = "[\"BBINbbsport\",\"BBINlottery\",\"BBINvideo\",\"SabaSport\",\"SabaNumber\",\"SabaVirtualSport\",\"AgBr\",\"Mg2Real\",\"Pt2Real\",\"GpiReal\",\"SingSport\",\"AllBetReal\",\"IgLottery\",\"IgLotto\",\"Rg2Real\",\"Rg2Board\",\"Rg2Lottery\",\"Rg2Lottery2\",\"JdbBoard\",\"EvoReal\",\"BgReal\",\"GdReal\",\"Pt3Real\",\"SunbetReal\",\"CmdSport\",\"Sunbet2Real\",\"Mg3Real\",\"KgBoard\",\"LxLottery\",\"EBetReal\",\"ImEsport\",\"OgReal\",\"VrLottery\",\"City761Board\",\"FsBoard\",\"SaReal\",\"ImsSport\",\"IboSport\",\"NwBoard\",\"JsBoard\",\"ThBoard\"]",
            //    Audit=5,
            //    AuditType= "Discount",
            //    Type=5,
            //};



            //b = platGPK.loginGPK();
            ////bb = platGPK.checkInGPK_transaction(bb);
            //b = platGPK.submitToGPK(bb, "测试活动 消除");
            ////bb = platGPK.checkInGPK(bb);
            ////bool b = platBB.loginBB();
            //bb = platGPK.GetDetailInfo(bb);
            //bb = platGPK.GetDetailInfo_withoutELE(bb);
            //b = platGPK.submitToGPK(bb);

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



            //string[] aa = { "BBINbbsport", "BBINlottery", "BBINvideo", "SabaSport", "SabaNumber", "SabaVirtualSport", "AgBr", "Mg2Real", "Pt2Real", "GpiReal", "SingSport", "AllBetReal", "IgLottery", "IgLotto", "Rg2Real", "Rg2Board", "Rg2Lottery", "Rg2Lottery2", "JdbBoard", "EvoReal", "BgReal", "GdReal", "Pt3Real", "SunbetReal", "CmdSport", "Sunbet2Real", "Mg3Real", "KgBoard", "LxLottery", "EBetReal", "ImEsport", "OgReal", "VrLottery", "City761Board", "FsBoard", "SaReal", "ImsSport", "IboSport", "NwBoard", "JsBoard", "ThBoard" };
            //Console.WriteLine(aa.ToString());


            //platGPK.GetDetailInfo_withoutELE( new betData() {  username= "YJ7654321", lastCashTime= "2019/02/03", lastOprTime= "2019/02/04" });

            //platGPK.GetDetailInfo_withoutELE(new betData() { username = "YJ7654321", lastCashTime = "2019/02/03 20:00:00", lastOprTime = "2019/02/04" });

            //获取ws 连接的数据
            //string url = "ws://sts.tjuim.com/signalr/connect?transport=webSockets&clientProtocol=1.5&connectionToken=S4BsUiRNJIuEWfllUxJunQJwNCum%2BGFFPg78w8z4kp7kRylVvwud5DXjIPFcZf%2BSQh8QbOM7APENYKeUOQTeZINx6zU726XQ65yFP2P9APCiRkF4&connectionData=%5B%7B%22name%22%3A%22mainhub%22%7D%5D&tid=2";
            //Uri uri = new Uri(url);
            /*
             
            WebSocketSharp.WebSocket ws = new WebSocketSharp.WebSocket(url);
            ws.OnOpen += (sender, e) =>
            {
                Console.WriteLine("Open");
            };
            ws.OnMessage += (sender, e) =>
                    Console.WriteLine("Laputa says: " + e.Data);
            ws.OnError += (sender, e) =>
            {
                Console.WriteLine(e.Message);
            };
            ws.OnClose += (sender, e) =>
            {
                Console.WriteLine(e.Code);
            };

            ws.Connect();
            ws.Send("test");

            //ws.Send("BALUS");

            */


            //try
            //{
            //    ClientWebSocket cln = new ClientWebSocket();
            //    cln.ConnectAsync(new Uri(url), new CancellationToken()).Wait();
            //    byte[] result = new byte[1024];
            //    result = Encoding.UTF8.GetBytes("my message");
            //    cln.SendAsync(new ArraySegment<byte>(result), WebSocketMessageType.Text, true, new CancellationToken()).Wait();
            //    cln.ReceiveAsync(new ArraySegment<byte>(result), CancellationToken.None);
            //    Console.WriteLine(Encoding.Default.GetString(result));

            //}
            //catch (Exception ex)
            //{
            //    string ss = ex.ToString();
            //    Console.WriteLine(ss);
            //}



            /*
            ClientWebSocket cws = new ClientWebSocket();
            cws.ConnectAsync(uri, CancellationToken.None).Wait();
            byte[] bt = new byte[1];
            byte[] result = new byte[1024];
            cws.ReceiveAsync(new ArraySegment<byte>(result), CancellationToken.None);
            cws.SendAsync(new ArraySegment<byte>(bt), WebSocketMessageType.Binary, true, CancellationToken.None); //发送数据
            Console.WriteLine(Encoding.Default.GetString(result));

    */



            //WebBrowser wb = new WebBrowser();
            //wb.Navigate(Environment.CurrentDirectory +"/1.html");

            //IHTMLDocument2 document = (IHTMLDocument2)wb.Document.DomDocument;
            //IHTMLTxtRange htmlElem = (IHTMLTxtRange)document.selection.createRange();
            //string s = htmlElem.htmlText;






            //Program p = new Program();
            //p.WebSocket(url);

            //Console.WriteLine(ExecuteScript());

            //JavascriptUtility objJavascriptUtility = new JavascriptUtility();
            //objJavascriptUtility.RunJavaScript("javascript:DoubleClickCopyToClipBoard('" + this.LabelResult.Text + "');");

            //Console.WriteLine(RunByJSCodeProvider().ToString());

            Console.WriteLine("请选择：1全部更新；2全部取回；3人工提出");
            string sw= Console.ReadLine();
            if (sw=="1")
            {

            }
            else if (sw == "2")
            {

            }
            else if (sw == "2")
            {

            }
            Console.WriteLine(sw);

            Console.ReadLine();
        }

        #region ClientWebSocket

        readonly ClientWebSocket _webSocket = new ClientWebSocket();
        readonly CancellationToken _cancellation = new CancellationToken();

        public async void WebSocket(string url)
        {
            try
            {
                //建立连接
                //var url = "ws://121.40.165.18:8800";
                await _webSocket.ConnectAsync(new Uri(url),CancellationToken.None);
                var bsend = new byte[1024];
                await _webSocket.SendAsync(new ArraySegment<byte>(bsend), WebSocketMessageType.Binary, true, CancellationToken.None); //发送数据
                while (true)
                {
                    var result = new byte[1024];
                    await _webSocket.ReceiveAsync(new ArraySegment<byte>(result), CancellationToken.None);//接受数据
                    //var lastbyte = ByteCut(result, 0x00);
                    //var str = Encoding.UTF8.GetString(lastbyte, 0, lastbyte.Length);
                    Console.WriteLine(Encoding.UTF8.GetString(result));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        private static string ExecuteScript()
        {
            string sCode = @"function test() { var wsServer = ""ws://sts.tjuim.com/signalr/connect?transport=webSockets&clientProtocol=1.5&connectionToken=S4BsUiRNJIuEWfllUxJunQJwNCum%2BGFFPg78w8z4kp7kRylVvwud5DXjIPFcZf%2BSQh8QbOM7APENYKeUOQTeZINx6zU726XQ65yFP2P9APCiRkF4&connectionData=%5B%7B%22name%22%3A%22mainhub%22%7D%5D&tid=2"";
            var websocket = new WebSocket(wsServer);
            websocket.onopen = function(evt) {  };
            websocket.onclose = function(evt) {  };
            websocket.onmessage = function(evt) { return evt.data;};
            websocket.onerror = function(evt) {  }; }";
            MSScriptControl.ScriptControl scriptControl = new MSScriptControl.ScriptControl();
            scriptControl.UseSafeSubset = true;
            scriptControl.Language = "JScript";
            scriptControl.AddCode(sCode);
            try
            {
                string str = scriptControl.Eval("test()").ToString();
                return str;
            }
            catch (Exception ex)
            {
                string str = ex.Message;
            }
            return null;
        }

        public static object RunByJSCodeProvider()
        {
            //string md5 = DevCommon.MD5GenerateHashString(scriptCode);
            //if (this.msjsAssemblyTypeList.ContainsKey(md5))
            //{
            //    Type _evaluateType = this.msjsAssemblyTypeList[md5];
            //    object obj = _evaluateType.InvokeMember("JsRun", BindingFlags.InvokeMethod,
            //            null, null, null);
            //    return obj;
            //}
            //else
            //{
                string scriptCode  = @" var wsServer = ""ws://sts.tjuim.com/signalr/connect?transport=webSockets&clientProtocol=1.5&connectionToken=S4BsUiRNJIuEWfllUxJunQJwNCum%2BGFFPg78w8z4kp7kRylVvwud5DXjIPFcZf%2BSQh8QbOM7APENYKeUOQTeZINx6zU726XQ65yFP2P9APCiRkF4&connectionData=%5B%7B%22name%22%3A%22mainhub%22%7D%5D&tid=2"";
            var websocket = new WebSocket(wsServer);
            websocket.onopen = function(evt) {  };
            websocket.onclose = function(evt) {  };
            websocket.onmessage = function(evt) { return evt.data;};
            websocket.onerror = function(evt) {  }; ";
            StringBuilder sb = new StringBuilder();
                sb.Append("package Stdio{");
                sb.Append(" public class JScript {");
                sb.Append("     public static function JsRun() {");
                sb.Append(scriptCode);
                sb.Append("     }");
                sb.Append(" }");
                sb.Append("}");

                CompilerParameters parameters = new CompilerParameters();
                parameters.GenerateInMemory = true;
                CodeDomProvider _provider = new Microsoft.JScript.JScriptCodeProvider();
                CompilerResults results = _provider.CompileAssemblyFromSource(parameters, sb.ToString());
                Assembly assembly = results.CompiledAssembly;
                Type _evaluateType = assembly.GetType("Stdio.JScript");
                object obj = _evaluateType.InvokeMember("JsRun", BindingFlags.InvokeMethod,
                null, null, null);

                return obj;
            }
            #endregion
        }
}
