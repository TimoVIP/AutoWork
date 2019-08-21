using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Threading;
using OpenQA.Selenium.Interactions;
using BaseFun;
namespace AlipayGetMobile
{
    class Program
    {
        static IWebDriver selenium;
        static void Main(string[] args)
        {
            //登陆支付宝
            //打开网址，获取验证码
            ChromeOptions opt = new ChromeOptions();
            //隐藏
            //opt.AddArgument("--headless");
            //opt.AddArgument("--disable-gpu");
            opt.AddArgument("disable-infobars");
            //opt.AddAdditionalCapability("excludeSwitches", "['enable-automation']");

            opt.AddUserProfilePreference("excludeSwitches", "['enable-automation']");
            if (selenium == null)
            {
                selenium = new ChromeDriver(opt);
            }
            #region 密码登陆不行 按钮不起作用  换扫二维码登陆
            //string username = "18108629313";
            //string userpwd = "";

            //selenium.Navigate().GoToUrl("https://auth.alipay.com/login/index.htm");
            //selenium.Manage().Timeouts().PageLoad = TimeSpan.FromMilliseconds(100);
            //selenium.FindElement(By.Id("J-input-user")).SendKeys(username);
            //selenium.FindElement(By.Id("password_rsainput")).SendKeys(userpwd);
            //string vCode=  Console.ReadLine();
            //if(vCode.Length==4)
            //    selenium.FindElement(By.Id("J-input-checkcode")).SendKeys(vCode);

            //Thread.Sleep(300);
            //selenium.FindElement(By.Id("J-login-btn")).Click();

            #endregion
            //二维码登陆
            loginByOrcode();

            //查询转账 获取手机号
            getMobileNum();

        }

        private static void getMobileNum()
        {
            int count = 0;
            Thread.Sleep(1000);
            string url = "https://shenghuo.alipay.com/send/payment/fill.htm";
            if(!selenium.Url.Equals(url))
                selenium.Navigate().GoToUrl(url);
            while (count<1000)
            {
                string mobile = getRandomTel();
                try
                {
                    IWebElement el = selenium.FindElement(By.Id("ipt-search-key"));
                    el.Clear();
                    el.SendKeys(mobile);
                    Thread.Sleep(new Random().Next(100,1000));
                    selenium.FindElement(By.Id("amount")).Click();
                    Thread.Sleep(100);
                    //判断手机号是否正确
                    string msg = selenium.FindElement(By.Id("accountStatusMsg")).Text;
                    if (msg == "你的操作过于频繁，请稍后再试")
                    {
                        //检测到机器人
                        Thread.Sleep(new Random().Next(100,1000));
                        getMobileNum();
                    }
                    else if (msg == "")
                    {
                        count++;
                        appSittingSet.Log(mobile);
                        Console.WriteLine($"第{count}个号码：{mobile}");
                    }
                }
                catch (NoSuchElementException ex)
                {
                    //重新登陆
                    loginByOrcode();
                    getMobileNum();
                }

            }
        }

        private static void loginByOrcode()
        {
            string url = "https://auth.alipay.com/login/index.htm";
            if(!selenium.Url.Equals(url))
                selenium.Navigate().GoToUrl(url);
            //selenium.Manage().Timeouts().PageLoad = TimeSpan.FromMilliseconds(200);

            while (true)
            {
                if (selenium.Title.Contains("我的支付宝"))
                {
                    //获取Cookie
                    ICookieJar listCookie = selenium.Manage().Cookies;
                    // IList<Cookie> listCookie = selenuim.Manage( ).Cookies.AllCookies;//只是显示 可以用Ilist对象
                    break;
                }
            }
        }

        static Random ran = new Random();
        /// <summary>
        /// 随机生成电话号码
        /// </summary>
        /// <returns></returns>
        public static string getRandomTel()
        {
            string[] telStarts = "134,135,136,137,138,139,150,151,152,157,158,159,130,131,132,155,156,133,153,180,181,182,183,185,186,176,187,188,189,177,178".Split(',');
            int n = ran.Next(10, 1000);
            int index = ran.Next(0, telStarts.Length - 1);
            string first = telStarts[index];
            string second = (ran.Next(100, 888) + 10000).ToString().Substring(1);
            string thrid = (ran.Next(1, 9100) + 10000).ToString().Substring(1);
            return first + second + thrid;
        }
    }
}
