using BaseFun;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using SQLiteHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TimoControl;

namespace 自动确认744371
{
    public static class plat744371B
    {
        private static string urlbase { get; set; }
        private static string acc { get; set; }
        private static string pwd { get; set; }
        private static string uid { get; set; }

        private static string token { get; set; }
        private static IWebDriver selenium { get; set; }
        private static ChromeOptions opt { get; set; }

        private static CookieContainer cookie { get; set; }
        private static string otp { get; set; }
        private static string Authorization { get; set; }

        /// <summary>
        /// 获取验证码
        /// </summary>
        /// <returns>验证码本地路径</returns>
        public static string getCaptcha()
        {

            try
            {
                string s1 = appSittingSet.readAppsettings("744371");
                acc = s1.Split('|')[0];
                pwd = s1.Split('|')[1];
                urlbase = s1.Split('|')[2];
            }
            catch (Exception ex)
            {
                appSittingSet.Log("744371取配置文件失败" + ex.Message);
                return "err";
            }

            try
            {

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                string url = appSittingSet.readAppsettings("captchaurl");
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "POST";
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:72.0) Gecko/20100101 Firefox/72.0";
                request.Accept = "application/json, text/plain, */*";
                request.ContentType = "application/json;charset=utf-8";
                request.Referer = urlbase;
                request.Headers.Add("sign", "D8B446EF98E13AD475029033A415B40B");//每次发送一个GUID
                request.Headers.Add("Origin", urlbase);
                request.Headers.Add("TE", "Trailers");
                request.Headers.Add("x-platform-domain", urlbase.Split('/')[2]);
                request.Host = url.Split('/')[2];
                //request.Headers.Add("", "");
                request.KeepAlive = true;
                //request.Connection = "keep-alive";

                //request.ProtocolVersion = HttpVersion.Version11;
                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11;
                ////证书错误
                //ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                //request.CookieContainer = cookie;


                //发送数据1
                var obj = new { };
                string postdata = JsonConvert.SerializeObject(obj);
                byte[] bytes = Encoding.UTF8.GetBytes(postdata);
                request.ContentLength = bytes.Length;
                Stream newStream = request.GetRequestStream();
                newStream.Write(bytes, 0, bytes.Length);
                newStream.Close();


                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string ret_html = reader.ReadToEnd();

                //获取响应头 Authorization
                Authorization = response.Headers.Get("authorization");

                cookie = new CookieContainer();
                //设置cookie
                //Set - Cookie 设置
                string s = response.Headers.Get("Set-Cookie").Replace(" path=/", "");
                string name = s.Split('=')[0];
                string value = s.Split('=')[1];
                System.Net.Cookie c = new System.Net.Cookie(name, value, "/", urlbase.Split('/')[2]);
                //cookie.Add(c);


                cookie.Add(response.Cookies);
                reader.Close();
                reader.Dispose();
                response.Dispose();
                request.Abort();

                JObject jo = (JObject)JsonConvert.DeserializeObject(ret_html.ToString());
                if (jo["success"].ToString() == "true" || jo["code"].ToString() == "200")
                {
                    //写入文件
                    string fileName = Environment.CurrentDirectory + "\\capcha\\" + appSittingSet.GetTimeStamp() + ".jpg";// + "\\1.jpg";
                    File.WriteAllBytes(fileName, (byte[])jo["data"]["captcha"]);
                    return fileName;
                }
                else
                    return "err";
            }
            catch (WebException ex)
            {
                appSittingSet.Log(string.Format("获取验证码失败：{0}   ", ex.Message));
                return "err";
            }
        }






        /// <summary>
        /// 登录
        /// </summary>
        /// <returns></returns>
        public static bool login()
        {
            try
            {
                string s1 = appSittingSet.readAppsettings("744371");
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
                if (opt == null)
                {
                    opt = new ChromeOptions();
                }

                //隐藏
                //opt.AddArgument("--headless");
                //opt.AddArgument("--disable-gpu");
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
                if (selenium.Url.Contains("activityInspect") || selenium.Url.Contains("FlowFindPage"))
                {
                    return true;
                }

                selenium.Navigate().GoToUrl(urlbase);
                Thread.Sleep(200);
                //selenium.FindElement(By.Id("username")).SendKeys(acc);
                //selenium.FindElement(By.Id("txtPasword")).SendKeys(pwd);

                //获取并 输入otp 验证码 

                Console.Write("请输入验证码：");
                otp = Console.ReadLine();
                //Console.WriteLine(otp);
                /*
                 //*[@id="username"]
                 //*[@id="password"]
                 //*[@id="captcha"]

                 /html/body/div/div/div/div/div[2]/form/div[4]/div/div/span/div/div/button

                 */
                 /*
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
                */
                selenium.FindElement(By.Id("username")).SendKeys(acc);
                selenium.FindElement(By.Id("password")).SendKeys(pwd);
                selenium.FindElement(By.Id("captcha")).SendKeys(otp);


                //selenium.FindElement(By.Name("submit")).Click();
                Thread.Sleep(2000);
                selenium.FindElement(By.XPath("/html/body/div/div/div/div/div[2]/form/div[4]/div/div/span/div/div/button")).Click();

                //判断是否登陆 
                if (selenium.Title.Contains("首页") || selenium.Title.Contains("Ant Design Pro") || selenium.PageSource.Contains(acc) || selenium.PageSource.Contains("后台管理") )
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

        public static List<betData> getRecorder()
        {
            List<betData> list = new List<betData>();

            if (!selenium.Url.Contains("activityInspect"))
            {
                selenium.Navigate().GoToUrl($"{urlbase}activityInspect");
            }
            //*[@id="app"]/div/div[2]/div[2]/div[3]/div/div/div[2]/div/div/div/div/div[2]
            ///html/body/div[1]/div/div[2]/div[2]/div[3]/div/div/div[2]/div/div/div/div/div[2]

            //为了测试选昨天
            selenium.FindElements(By.TagName("button"))[2].Click();//昨天
            Thread.Sleep(500);
            selenium.FindElements(By.TagName("button"))[5].Click();//查询
            Thread.Sleep(200);

            
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(selenium.PageSource);
            HtmlNode node1 = htmlDocument.DocumentNode.SelectSingleNode("//table//tbody");
            ////*[@id="app"]/div/div[2]/div[2]/div[3]/div/div/div[1]/form/div[1]/div[1]/div/div[2]/div/span/button[2]



            if (node1.InnerLength>0 || !selenium.PageSource.Contains("暂无数据"))
            {
                //获取数据 /html/body/div[1]/div/div[2]/div[2]/div[3]/div/div/div[1]/form/div[2]/div[1]/div/div[2]/div/span/div/div/div/div
                //需要操作下拉框 不要操作 直接不操作 获取数据 /html/body/div[1]/div/div[2]/div[2]/div[3]/div/div/div[2]/div/div/div/div/div/table/tbody

                HtmlNodeCollection collection = node1.SelectNodes("//tr");//跟Xpath一样，轻松的定位到相应节点下
                for (int i = 0; i < collection.Count; i++)
                {
                    string id = collection[i].ChildNodes[0].InnerText;
                    string proj = collection[i].ChildNodes[1].InnerText;
                    string money = collection[i].ChildNodes[2].InnerText;
                    if (proj== "首充赠送")
                    {
                        list.Add(new betData() { bbid = id, Id = id, gamename = proj, betMoney = decimal.Parse(money) });
                    }
                }
                /*
             var nodes = doc.DocumentNode.SelectNodes("//table/tr");
var table = new DataTable("MyTable");

var headers = nodes[0]
    .Elements("th")
    .Select(th => th.InnerText.Trim());
foreach (var header in headers)
{
    table.Columns.Add(header);
}

var rows = nodes.Skip(1).Select(tr => tr
    .Elements("td")
    .Select(td => td.InnerText.Trim())
    .ToArray());
foreach (var row in rows)
{
    table.Rows.Add(row);
}    
             */
            }
            return list;
        }


        //activityInspect

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


                if (selenium.Title.Contains("后台管理系统") || selenium.PageSource.Contains("会员列表") || selenium.PageSource.Contains("手工存款"))
                {

                }
                else
                {
                    //找不到这个页面
                    return;
                }


                //模拟按钮 /html/body/div[1]/div/section/section/div[2]/div[1]/div[3]/div[2]/div[2]/div/div/div/div/div[2]/button[4]
                var btn = selenium.FindElement(By.XPath("/html/body/div[1]/div/section/section/div[2]/div[1]/div[3]/div[2]/div[2]/div/div/div/div/div[2]/button[4]"));

                try
                {
                    //判断弹框是否打开
                    if (btn != null)
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
                btn = selenium.FindElement(By.XPath("//*[@id='userAccount']/div/div"));

                if (btn != null)
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
                btn = selenium.FindElement(By.XPath("//*[@id='userAccount']/input"));
                if (btn != null)
                {
                    btn.SendKeys(b.username);
                }
                Thread.Sleep(200);
                //查询
                btn = selenium.FindElement(By.XPath("//*[@id='userAccount']/button"));
                //btn =  selenium.FindElement(By.XPath("/html/body/div[3]/div/div[2]/div/div[2]/div[3]/button[2]"));
                if (btn != null)
                {
                    btn.Click();
                }
                Thread.Sleep(200);
                //查看返回信息 判断余额的值 /html/body/div[3]/div/div[2]/div/div[2]/div[2]/form/div[2]/div[2]/div/span
                var aaa = selenium.FindElement(By.XPath("/html/body/div[3]/div/div[2]/div/div[2]/div[2]/form/div[2]/div[2]/div/span/span"));
                var balance = aaa.Text.Replace("元", "");
                decimal balance_ = 0;
                bool isOk = decimal.TryParse(balance, out balance_);
                if (!isOk)
                {
                    //没有 用户记录
                    b.passed = false;
                    b.msg = "账户不存在";
                    platQPGV2.confirmAct(b);
                    //关闭弹框 /html/body/div[3]/div/div[2]/div/div[2]/button
                    btn = selenium.FindElement(By.XPath("/html/body/div[3]/div/div[2]/div/div[2]/button"));
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
