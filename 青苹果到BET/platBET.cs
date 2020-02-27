using BaseFun;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using SQLiteHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TimoControl;

namespace 青苹果到BET
{
    public static class platBET
    {
        private static string urlbase { get; set; }
        private static string acc { get; set; }
        private static string pwd { get; set; }
        private static string uid { get; set; }
        //private static CookieContainer cookie { get; set; }
        private static string token { get; set; }
        private static IWebDriver selenium { get; set; }
        private static ChromeOptions opt { get; set; }
        public static string otp { get; set; }
        /// <summary>
        /// 登录
        /// </summary>
        /// <returns></returns>
        public static bool login()
        {
            try
            {
                string s1 = appSittingSet.readAppsettings("BET");
                acc = s1.Split('|')[0];
                pwd = s1.Split('|')[1];
                urlbase = s1.Split('|')[2];
                //uid = s1.Split('|')[3];
            }
            catch (Exception ex)
            {
                appSittingSet.Log("BET取配置文件失败" + ex.Message);
                return false;
            }


            try
            {
                if (opt==null)
                {
                    opt = new ChromeOptions();
                }

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
                if (selenium.Url.Contains("manager"))
                {
                    return true;
                }

                selenium.Navigate().GoToUrl(urlbase);
                Thread.Sleep(200);
                //selenium.FindElement(By.Id("username")).SendKeys(acc);
                //selenium.FindElement(By.Id("txtPasword")).SendKeys(pwd);
                //输入otp
                Console.Write("请输入OTP：");
                otp = Console.ReadLine();
                //Console.WriteLine(otp);

                var inputs = selenium.FindElements(By.TagName("input"));
                if (inputs.Count == 4)
                {
                    inputs[0].SendKeys(acc);
                    inputs[1].SendKeys(pwd);
                    inputs[2].SendKeys(otp);
                    Thread.Sleep(200);
                }
                var btn = selenium.FindElements(By.TagName("img"));
                if (btn.Count == 5)
                {
                    btn[4].Click();
                }
                //selenium.FindElement(By.Name("submit")).Click();
                Thread.Sleep(2000);
                //判断是否登陆 
                if (selenium.Title.Contains("公告") || selenium.Title.Contains("后台") || selenium.PageSource.Contains("管理系统") || selenium.PageSource.Contains("手工存款")|| selenium.PageSource.Contains("会员列表"))
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
                //是否充值过了                     本地存在，直接确认掉
                string sql = $"select * from record where status=1 and order_no ='{b.bbid}'";
                if (SQLiteHelper.SQLiteHelper.recorderDbCheck(sql))
                {
                    //操作成功 确认
                    b.passed = true;
                    platQPGV2.confirmAct(b);
                    return;
                }

                //导航到充值页面 模拟充值
                selenium.Navigate().GoToUrl($"{urlbase}#/manager/member/memberList");

                Thread.Sleep(200);
                //if (selenium.PageSource.Contains("登陆") || selenium.PageSource.Contains("请输入帐号") )
                //{
                //    //重新登陆
                //    login();
                //}


                if (selenium.Title.Contains("后台管理系统") || selenium.PageSource.Contains("会员列表")  || selenium.PageSource.Contains("手工存款") )
                {

                }
                else
                {
                    //找不到这个页面
                    return;
                }


                //模拟按钮 /html/body/div[1]/div/section/section/div[2]/div[1]/div[3]/div[2]/div[2]/div/div/div/div/div[2]/button[4]
                var btn =  selenium.FindElement(By.XPath("/html/body/div[1]/div/section/section/div[2]/div[1]/div[3]/div[2]/div[2]/div/div/div/div/div[2]/button[4]"));

                try
                {
                    //判断弹框是否打开
                    if (btn!=null)
                    {
                        btn.Click();
                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("element click intercepted"))
                    {

                    } 
                }



                Thread.Sleep(200);



                //模拟选择ID下拉框
                Actions actions = new Actions(selenium);
                btn =  selenium.FindElement(By.XPath("//*[@id='userAccount']/div/div"));
                
                if (btn!=null)
                {
                    btn.Click();

                }
                Thread.Sleep(200); //*[@id="66c7592c-0c1e-4789-97f8-16ffe2184611"]/ul/li[2]

                //WebDriverWait wait = new WebDriverWait(selenium, TimeSpan.FromSeconds(5));
                //bool f1 = wait.Until(selenium => selenium.PageSource.Contains("会员账号"));

                //var dp1 = selenium.FindElements(By.TagName("ul"))[3].FindElements(By.TagName("li"))[1];
                var dp1 = selenium.FindElement(By.XPath("/html/body/div[4]/div/div/div/ul/li[2]"));

                if (dp1 != null)
                {
                    if (!dp1.Displayed)
                    {
                        btn.Click();

                    }
                    //actions.MoveToElement(dp1).Perform();
                    //bool f1= selenium.FindElement(By.XPath("/html/body/div[4]/div/div/div/ul/li[2]")).Displayed;
                    ////*[@id="19e9a03f-9708-49ac-9ae8-3f8decabcaae"]/ul/li[2]
                    ///html/body/div[4]/div/div/div/ul/li[2]
                    dp1.Click();
                }
                Thread.Sleep(200);

                // 模拟 选择用户名
                //btn =  selenium.FindElement(By.XPath("//*ul[@role='listbox']/li[1]"));
                //if (btn!=null)
                //{
                //    btn.Click();
                //}

                //会员名
                btn =  selenium.FindElement(By.XPath("//*[@id='userAccount']/input"));
                if (btn!=null)
                {
                    btn.SendKeys(b.username);
                }
                Thread.Sleep(200);
                //查询
                btn =  selenium.FindElement(By.XPath("//*[@id='userAccount']/button"));
                //btn =  selenium.FindElement(By.XPath("/html/body/div[3]/div/div[2]/div/div[2]/div[3]/button[2]"));
                if (btn!=null)
                {
                    btn.Click();
                }
                Thread.Sleep(200);
                //查看返回信息 判断余额的值 /html/body/div[3]/div/div[2]/div/div[2]/div[2]/form/div[2]/div[2]/div/span
                var aaa = selenium.FindElement(By.XPath("/html/body/div[3]/div/div[2]/div/div[2]/div[2]/form/div[2]/div[2]/div/span/span"));
                var balance = aaa.Text.Replace("元","");
                decimal balance_ = 0;
                bool isOk= decimal.TryParse(balance, out balance_);
                if (!isOk)
                {
                    //没有 用户记录
                    b.passed = false;
                    b.msg = "账户不存在";
                    platQPGV2.confirmAct(b);
                    //关闭弹框 /html/body/div[3]/div/div[2]/div/div[2]/button
                    btn= selenium.FindElement(By.XPath("/html/body/div[3]/div/div[2]/div/div[2]/button"));
                    if (btn != null)
                    {
                        btn.Click();
                    }
                    return;
                }




                //金额
                selenium.FindElement(By.Id("balance")).SendKeys(b.betMoney.ToString());


                //备注
                selenium.FindElement(By.Id("note")).SendKeys($"青苹果 {b.bbid}");
                Thread.Sleep(200);
                //提交按钮
                btn = selenium.FindElements(By.ClassName("ant-modal-footer"))[0].FindElements(By.TagName("button"))[1];
                if (btn != null)
                {
                    btn.Click();
                }
                Thread.Sleep(200);
                //selenium.FindElement(By.Id("search_button")).Click();
                //var btn = selenium.FindElements(By.TagName("button"));
                //if (btn.Count == 1)
                //{
                //    btn[0].Click();
                //}

                //Thread.Sleep(200);

                //WebDriverWait wait = new WebDriverWait(selenium, TimeSpan.FromSeconds(5));
                //bool f1 = wait.Until(selenium => selenium.PageSource.Contains("RMB.gif"));




                //if (isAlertExist() && selenium.SwitchTo().Alert().Text.Contains("该会员不存在"))
                //{
                //    selenium.SwitchTo().Alert().Accept();
                //    //没有 用户记录
                //    b.passed = false;
                //    b.msg = "账户不存在";
                //    platQPGV2.confirmAct(b);
                //    return;
                //}

                //提交
                //btn = selenium.FindElements(By.ClassName("ant-modal-footer"))[1];
                //btn =  selenium.FindElement(By.XPath("/html/body/div[6]/div/div[2]/div/div[2]/div[3]/button[2]"));


                if (true)
                {
                    //selenium.SwitchTo().Alert().Accept();
                    //已经成功送出
                    //插入本地数据库
                    //SQLiteHelper.SQLiteHelper.execSql($"insert into record values({0},'{b.bbid}','{b.username}',datetime(CURRENT_TIMESTAMP,'localtime'),0,1);");
                    //操作成功 确认
                    b.passed = true;
                    bool r = platQPGV2.confirmAct(b);
                    //关闭弹框 /html/body/div[3]/div/div[2]/div/div[2]/button
                    btn = selenium.FindElement(By.XPath("/html/body/div[3]/div/div[2]/div/div[2]/button"));
                }

            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("no such element"))
                {
                    Console.WriteLine("请重新登陆");
                    login();
                }
                string msg = $"确认失败：用户 {b.username} 单{b.bbid} 错误{ex.Message}";
                appSittingSet.Log(msg);
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
