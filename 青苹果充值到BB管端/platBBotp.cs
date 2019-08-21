using BaseFun;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace 青苹果充值到BB管端
{
    public static class platBBotp
    {
        private static string urlbase { get; set; }
        private static string acc { get; set; }
        private static string pwd { get; set; }
        private static string otp { get; set; }
        private static CookieContainer cookie { get; set; }
        private static string token { get; set; }
        private static IWebDriver selenium { get; set; }

        /// <summary>
        /// 登录
        /// </summary>
        /// <returns></returns>
        public static bool login()
        {
            try
            {
                string s1 = appSittingSet.readAppsettings("BBOPT");
                acc = s1.Split('|')[0];
                pwd = s1.Split('|')[1];
                urlbase = s1.Split('|')[2];
                //uid = s1.Split('|')[3];
                Console.WriteLine("请输入otp：");
                otp = Console.ReadLine();

            }
            catch (Exception ex)
            {
                appSittingSet.Log("TG取配置文件失败" + ex.Message);
                return false;
            }


            try
            {
                ChromeOptions opt = new ChromeOptions();
                //隐藏
                //opt.AddArgument("--headless");
                //opt.AddArgument("--disable-gpu");
                //禁用扩展之类
                opt.AddArgument("--audio-output-channels=0");
                opt.AddArgument("--disable-default-apps");
                opt.AddArgument("--disable-extensions");
                opt.AddArgument("--disable-translate");
                opt.AddArgument("--disable-sync");
                opt.AddArgument("--hide-scrollbars");
                opt.AddArgument("--mute-audio");

                if (selenium == null)
                {
                    selenium = new ChromeDriver(opt);

                }
                //selenium.Manage().Timeouts().PageLoad = TimeSpan.FromMilliseconds(100);
                //selenium = new PhantomJSDriver();
                selenium.Url = urlbase;
                selenium.FindElement(By.XPath("/html/body/div[1]/div/form/div[2]/div/div[1]/input")).SendKeys(acc);
                selenium.FindElement(By.XPath("/html/body/div[1]/div/form/div[3]/div/div/input")).SendKeys(pwd);
                selenium.FindElement(By.XPath("/html/body/div[1]/div/form/div[4]/div/div/input")).SendKeys(otp);
                selenium.FindElement(By.XPath("/html/body/div[1]/div/form/div[5]/div/button")).Click();
                selenium.Manage().Timeouts().PageLoad = TimeSpan.FromMilliseconds(100);
                //Thread.Sleep(200);
                //判断是否登陆 
                if (selenium.Title.Contains("管理员") || selenium.Title.Contains("后台"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                appSittingSet.Log(string.Format("登录失败：{0}   ", ex.Message));
                return false;
            }
        }


    }
}
