using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TimoControl;
namespace TestDemo
{
    public partial class Form1 : Form
    {
        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool InternetSetCookie(string lpszUrlName, string lbszCookieName, string lpszCookieData);

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //pictureBox1.Image = platAlipay.getValidateCode();
            bool b = platGPK.loginGPK();
            if (!b)
            {
                return;
            }

            //WebBrowser wb = new WebBrowser();
            //Uri url = new Uri("http://bh.bs004.gpk135.com/BetRecord?search=true&account=xiaosan778&wagersTimeBegin=2019%2F02%2F01%2000:00:00&gameCategories=BBINbbsport,BBINlottery,BBINvideo,SabaSport,SabaNumber,SabaVirtualSport,AgBr,Mg2Real,Pt2Real,GpiReal,SingSport,AllBetReal,IgLottery,IgLotto,Rg2Real,Rg2Board,Rg2Lottery,Rg2Lottery2,JdbBoard,EvoReal,BgReal,GdReal,Pt3Real,SunbetReal,CmdSport,Sunbet2Real,Mg3Real,KgBoard,LxLottery,EBetReal,ImEsport,OgReal,VrLottery,City761Board,FsBoard,SaReal,ImsSport,IboSport,NwBoard,JsBoard,ThBoard&_=1549438899788");
            //url = new Uri("http://bh.bs004.gpk135.com/BetRecord?search=true&account=xiaosan778&wagersTimeBegin=2019%2F02%2F01%2000:00:00&_=1549437086185");
            //foreach (string c in platGPK.cookie.ToString().Split(';'))
            //{

            //    string[] item = c.Split('=');
            //    if (item.Length == 2)
            //    {
            //        string name = item[0];
            //        string value = item[1];
            //        InternetSetCookie(url.ToString(), name, value);
            //        textBox1.Text = wb.Document.Cookie;
            //    }

            //}
            //wb.Navigate(url);


            //ws

            //textBox1.Text = ExecuteScript();

            JavascriptUtility objJavascriptUtility = new JavascriptUtility();
            objJavascriptUtility.RunJavaScript("javascript:DoubleClickCopyToClipBoard('" + this.LabelResult.Text + "');");

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
    }
}
