using BaseFun;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TimoControl;

namespace GAC_load
{
    public static class platTST03
    {
        private static string urlbase { get; set; }
        private static string acc { get; set; }
        private static string pwd { get; set; }
        private static string uid { get; set; }
        private static CookieContainer cookie { get; set; }
        private static string token { get; set; }
        private static IWebDriver selenium { get; set; }
        public static string otp { get; set; }
        /// <summary>
        /// 登录
        /// </summary>
        /// <returns></returns>
        public static bool login()
        {
            try
            {
                string s1 = appSittingSet.readAppsettings("TST");
                acc = s1.Split('|')[0];
                pwd = s1.Split('|')[1];
                urlbase = s1.Split('|')[2];
                //uid = s1.Split('|')[3];
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
                opt.AddArgument("--headless");
                opt.AddArgument("--disable-gpu");
                //禁用扩展之类
                //opt.AddArgument("--audio-output-channels=0");
                opt.AddArgument("--disable-default-apps");
                opt.AddArgument("--disable-extensions");
                opt.AddArgument("--disable-translate");
                opt.AddArgument("--disable-sync");
                opt.AddArgument("--hide-scrollbars");
                opt.AddArgument("--mute-audio");
                opt.AddArgument("--disable-logging");


                if (selenium == null)
                {
                    selenium = new ChromeDriver(opt);
                    //加上会报错
                    //selenium.Manage().Timeouts().PageLoad = TimeSpan.FromMilliseconds(200);
                    //selenium.Manage().Timeouts().AsynchronousJavaScript = TimeSpan.FromMilliseconds(200);
                }
                if (selenium.Url.Contains("index/admin"))
                {
                    return true;
                }

                selenium.Navigate().GoToUrl(urlbase);
                Thread.Sleep(200);
                //selenium.FindElement(By.Id("username")).SendKeys(acc);
                //selenium.FindElement(By.Id("txtPasword")).SendKeys(pwd);
                //输入otp
                Console.WriteLine("请输入OTP");
                otp = Console.ReadLine();

               var inputs = selenium.FindElements(By.TagName("input"));
                if (inputs.Count==3)
                {
                    inputs[0].SendKeys(acc);
                    inputs[1].SendKeys(pwd);
                    inputs[2].SendKeys(otp);
                    Thread.Sleep(200);
                }
                var btn = selenium.FindElements(By.TagName("button"));
                if (btn.Count == 1)
                {
                    btn[0].Click();
                }
                //selenium.FindElement(By.Name("submit")).Click();
                Thread.Sleep(2000);
                //判断是否登陆 
                if (selenium.Title.Contains("公告") || selenium.Title.Contains("后台") || selenium.PageSource.Contains("管理系统") || selenium.PageSource.Contains("游戏管理"))
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

        /// <summary>
        /// 充值
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public static void recharge(betData b)
        {
            try
            {
                //是否充值过了 直接确认掉
                string sql = $"select * from record where status=1 and order_no ='{b.bbid}'";
                if (SQLiteHelper.SQLiteHelper.recorderDbCheck(sql))
                {
                    //操作成功 确认
                    b.passed = true;
                    platQPGV2.confirmAct(b);
                    return;
                }

                //导航到充值页面 模拟充值
                selenium.Navigate().GoToUrl($"{urlbase}agv3/cl/index.php?module=Deposit&method=Source&langx=gb");

                Thread.Sleep(200);
                //if (selenium.PageSource.Contains("登陆") || selenium.PageSource.Contains("请输入帐号") )
                //{
                //    //重新登陆
                //    login();
                //}
                if (selenium.Title.Contains("人工线上存提") || selenium.PageSource.Contains("人工存入"))
                {

                }
                else
                {
                    //找不到这个页面

                    return;
                }
                selenium.FindElement(By.Name("search_name")).SendKeys(b.username);
                selenium.FindElement(By.Id("search_button")).Click();
                //var btn = selenium.FindElements(By.TagName("button"));
                //if (btn.Count == 1)
                //{
                //    btn[0].Click();
                //}

                Thread.Sleep(200);

                WebDriverWait wait = new WebDriverWait(selenium, TimeSpan.FromSeconds(5));
                bool f1 = wait.Until(selenium => selenium.PageSource.Contains("RMB.gif"));

                //查看返回信息

                if (isAlertExist() && selenium.SwitchTo().Alert().Text.Contains("查无此帐号"))
                    {
                        selenium.SwitchTo().Alert().Accept();
                        //没有 用户记录
                        b.passed = false;
                        b.msg = "账户不存在";
                        platQPGV2.confirmAct(b);
                        return;
                    }
                    //else
                    //{

                    //}


                //selenium.SwitchTo().Alert().Accept();
                var p1 = selenium.FindElement(By.Id("deposit_LoginName"));
                //var p2 = selenium.FindElement(By.Id("deposit_LoginName"));
                if (p1.Text.Trim().ToLower()==b.username.Trim().ToLower())
                {
                    selenium.FindElement(By.Name("amount")).SendKeys(b.betMoney.ToString());
                    selenium.FindElement(By.Name("amount_memo")).SendKeys($"青苹果 {b.bbid}");
                    selenium.FindElement(By.Name("complex")).SendKeys(b.betMoney.ToString());

                    //selenium.FindElement(By.Name("amount")).SendKeys("1.01");
                    //selenium.FindElement(By.Name("amount_memo")).SendKeys($"技术测试 {b.bbid}");
                    //selenium.FindElement(By.Name("complex")).SendKeys("1.01");

                    selenium.FindElement(By.ClassName("Deposit_input")).Click();

                    Thread.Sleep(500);

                    if (isAlertExist() && selenium.SwitchTo().Alert().Text.Contains("请再确认"))
                    {
                        selenium.SwitchTo().Alert().Accept();
                    }

                    if (isAlertExist() && selenium.SwitchTo().Alert().Text.Contains("已送出"))
                    {
                        selenium.SwitchTo().Alert().Accept();
                        //已经成功送出
                        //插入本地数据库
                        //SQLiteHelper.SQLiteHelper.execSql($"insert into record values({0},'{b.bbid}','{b.username}',datetime(CURRENT_TIMESTAMP,'localtime'),0,1);");
                        //操作成功 确认
                        b.passed = true;
                       bool r = platQPGV2.confirmAct(b);
                    }

                    if (isAlertExist() && selenium.SwitchTo().Alert().Text.Contains("操作逾時"))
                    {
                        selenium.SwitchTo().Alert().Accept();

                    }
                    //if (selenium.)
                    //{

                    //}
                    //DesiredCapabilities dc = new DesiredCapabilities();
                    //dc.setCapability(CapabilityType.UNEXPECTED_ALERT_BEHAVIOUR, UnexpectedAlertBehaviour.IGNORE);
                    //d = new FirefoxDriver(dc);
                    //存在 加款
                    //var inputs = selenium.FindElements(By.TagName("input"));
                    //if (inputs.Count == 3)
                    //{
                    //    inputs[0].SendKeys(acc);
                    //    inputs[1].SendKeys(pwd);
                    //    inputs[2].SendKeys(otp);
                    //    Thread.Sleep(200);
                    //}
                    //var btn = selenium.FindElements(By.TagName("button"));
                    //if (btn.Count == 1)
                    //{
                    //    btn[0].Click();
                    //}

                }
                else
                {
                    b.Memo = "用户名正确";
                    b.passed = false;
                    bool r = platQPGV2.confirmAct(b);
                }
            }
            catch (Exception ex)
            {
                /*unexpected alert open: {Alert text : 已送出}
                (Session info: headless chrome = 78.0.3904.70)
  (Driver info: chromedriver = 78.0.3904.70(edb9c9f3de0247fd912a77b7f6cae7447f6d3ad5 - refs / branch - heads / 3904@{#800}),platform=Windows NT 10.0.17763 x86_64)
  */
                if (ex.Message.Contains("操作逾時"))
                {
                    //操作超时
                    selenium.SwitchTo().Alert().Accept();
                }
                else if (ex.Message.Contains("已送出"))
                {
                    //已经充值完毕
                    selenium.SwitchTo().Alert().Accept();
                }
                else if (ex.Message.Contains("查无此帐号"))
                {
                    //已经无账号
                    selenium.SwitchTo().Alert().Accept();
                    b.passed = false;
                    b.msg = "账户不存在";
                    platQPGV2.confirmAct(b);
                }
                string msg = $"确认失败：用户 {b.username} 单{b.bbid} 错误{ex.Message}";
                appSittingSet.Log(msg);

                //if (selenium.PageSource.Contains("退出"))
                //{
                //    login();
                //    appSittingSet.Log("重新登陆");
                //}

            }
        }

        /// <summary>
        /// 判断弹窗是否存在
        /// </summary>
        /// <returns></returns>
        public static bool isAlertExist()
        {
            try
            {
                selenium.SwitchTo().Alert();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
