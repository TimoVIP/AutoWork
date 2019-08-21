using BaseFun;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using TimoControl;

namespace GAC_load
{
    public static class platTGaming
    {
        private static string urlbase { get; set; }
        private static string acc { get; set; }
        private static string pwd { get; set; }
        private static string uid { get; set; }
        private static CookieContainer cookie { get; set; }
        private static string token { get; set; }
        private static IWebDriver selenium { get; set; }
        #region request 方法,密码加密方式没有找到，不能使用
        /// <summary>
        /// 登录
        /// </summary>
        /// <returns></returns>
        public static bool _login()
        {
            try
            {
                string s1 = appSittingSet.readAppsettings("TG");
                acc = s1.Split('|')[0];
                pwd = s1.Split('|')[1];
                urlbase = s1.Split('|')[2];
                //uid = s1.Split('|')[3];
            }
            catch (Exception ex)
            {
                appSittingSet.Log("活动站获取配置文件失败" + ex.Message);
                return false;
            }


            try
            {
                //pwd = MD5Encrypt(pwd);//加密
                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                string url = urlbase;
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "POST";
                request.Accept = "application/json, text/javascript, */*; q=0.01";
                request.KeepAlive = true;
                request.Host = urlbase.Replace("http://", "").Replace("/", "");
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/75.0.3770.80 Safari/537.36";

                string postData = JsonConvert.SerializeObject(new { username = acc, password = pwd });
                byte[] bytes = Encoding.UTF8.GetBytes(postData);
                request.ContentLength = bytes.Length;
                Stream newStream = request.GetRequestStream();
                newStream.Write(bytes, 0, bytes.Length);
                newStream.Close();

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string ret_html = reader.ReadToEnd();
                // {"code":200,"message":"操作成功","transNo":"898f964a9bc561e2","data":{"token":"eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJ0b2tlblNhbHQiOiI1YmM4NjFlZGQ1YWMzNmRiZmY2NDdlMDFlMmY0N2VhZTc0ZTJhYWE0IiwiaWZTdWJBY2NvdW50TG9naW4iOnRydWUsInVzZXJOYW1lIjoiSlFSQHhwajExMSIsInVzZXJSb2xlIjoibWVyY2hhbnQiLCJ1c2VySWQiOiIiLCJzaWduVGltZSI6MTU2MDQxMzE4MTIzOSwiZXhwaXJlcyI6ODY0MDAwMDAsImV4cCI6MTU2MDQ5OTU4MSwibmJmIjoxNTYwNDEzMTgxfQ.Ui1RJL3jcMdBiroUZEef_ewGAw6-3tncnw8HiwLyF2c","roleList":["1","1_1","2","2_0","2_0_0","2_0_1","2_1","2_1_0","2_1_1"],"role":"SUB_ACCOUNT","backendGrantListForSubAcct":[],"mainAccount":"xpj111","subAccount":"JQR@xpj111"}}
                cookie = new CookieContainer();
                cookie.Add(response.Cookies);
                reader.Close();
                reader.Dispose();
                response.Dispose();
                request.Abort();

                return ret_html.Contains("操作成功");
                //JObject jo = (JObject)JsonConvert.DeserializeObject(ret_html.ToString());
                //if (jo["message"].ToString() == "操作成功")
                //{
                //    subAccount = jo["data"]["subAccount"].ToString();
                //    token = jo["data"]["token"].ToString();
                //    transNo = jo["transNo"].ToString();
                //    return true;
                //}
                //else
                //    return false;

                /*
                //request.ProtocolVersion = HttpVersion.Version10;
                //ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11;
                //ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                //request.Method = "POST";
                //request.UserAgent = "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; Trident/4.0; QQWubi 133; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; CIBA; InfoPath.2)";
                

                request.ContentType = "application/json;charset=utf-8";
                request.Accept = "application/json";

                //string postdata = string.Format("user={0}&password={1}", acc, pwd);
                //byte[] bytes = Encoding.UTF8.GetBytes(postdata);
                //request.ContentLength = bytes.Length;
                //Stream newStream = request.GetRequestStream();
                //newStream.Write(bytes, 0, bytes.Length);
                //newStream.Close();

                cookie = new CookieContainer();
                cookie.Add(new Cookie("sidebarStatus", "0", "/", "api.qapple.io"));
                request.CookieContainer = cookie;


                //X509Certificate certificate = new X509Certificate(Properties.Resources.cert);
                //request.ClientCertificates.Add(certificate);

                //X509Certificate2 certificate2 = new X509Certificate2(Properties.Resources.cert);
                //request.ClientCertificates.Add(certificate);


                //X509Store certStore = new X509Store(StoreName.My, StoreLocation.LocalMachine);
                //certStore.Open(OpenFlags.ReadOnly);
                //X509Certificate2Collection certCollection = certStore.Certificates.Find(X509FindType.FindBySubjectName, "api.qapple.io", false);
                //request.ClientCertificates.Add(certCollection[0]);

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);

                string ret_html = reader.ReadToEnd();

                cookie.Add(response.Cookies);
                reader.Close();
                reader.Dispose();
                response.Dispose();
                request.Abort();
                */
            }
            catch (WebException ex)
            {
                appSittingSet.Log(string.Format("登录失败：{0}   ", ex.Message));
                return false;
            }
        }
        /// <summary>
        /// 查询用户
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        private static bool searchUser(betData b)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                string url = urlbase + $"member/index/?searchKey=username&searchVal={b.username}&timeType=&startTime=&endTime=";

                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "GET";
                //request.Headers.Add("logntoken", token);
                //request.Headers.Add("username", subAccount);
                //request.Headers.Add("userrole", "merchant");
                //request.Headers.Add("busitype", "merchant");
                //request.Headers.Add("clitype", "PC");
                //request.Headers.Add("cliv", "1.0.0");

                request.CookieContainer = cookie;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string ret_html = reader.ReadToEnd();

                cookie.Add(response.Cookies);
                reader.Close();
                reader.Dispose();
                response.Dispose();
                request.Abort();
                return ret_html.Contains("未找到");
            }
            catch (WebException ex)
            {
                string msg = $"查询用户{b.username}失败：错误{ex.Message}";
                appSittingSet.Log(msg);
                return false;
            }
        }
        /// <summary>
        /// 获取token
        /// </summary>
        /// <returns></returns>
        private static string pre_recharge()
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                string url = urlbase + $"money/amount-recharge?";

                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "GET";
                //request.Headers.Add("logntoken", token);
                //request.Headers.Add("username", subAccount);
                //request.Headers.Add("userrole", "merchant");
                //request.Headers.Add("busitype", "merchant");
                //request.Headers.Add("clitype", "PC");
                //request.Headers.Add("cliv", "1.0.0");

                request.CookieContainer = cookie;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string ret_html = reader.ReadToEnd();

                cookie.Add(response.Cookies);
                reader.Close();
                reader.Dispose();
                response.Dispose();
                request.Abort();

                JObject jo = (JObject)JsonConvert.DeserializeObject(ret_html.ToString());
                return "";
                //return ret_html.Contains("操作成功");
            }
            catch (WebException ex)
            {
                string msg = $"获取token失败：错误{ex.Message}";
                appSittingSet.Log(msg);
                return "";
            }
        }

        /// <summary>
        /// 充值
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        private static bool _recharge(betData b)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                string url = urlbase + $"money/amount-recharge?";

                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "POST";
                //request.Headers.Add("logntoken", token);
                //request.Headers.Add("username", subAccount);
                //request.Headers.Add("userrole", "merchant");
                //request.Headers.Add("busitype", "merchant");
                //request.Headers.Add("clitype", "PC");
                //request.Headers.Add("cliv", "1.0.0");

                request.CookieContainer = cookie;
                string postData = JsonConvert.SerializeObject(new { username = acc, amount = b.betMoney, bonus=0, bonusHand=0, gift = 0, dml =1, point=1, token = token , remark  = b.betno});

                byte[] bytes = Encoding.UTF8.GetBytes(postData);
                request.ContentLength = bytes.Length;
                Stream newStream = request.GetRequestStream();
                newStream.Write(bytes, 0, bytes.Length);
                newStream.Close();

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string ret_html = reader.ReadToEnd();

                cookie.Add(response.Cookies);
                reader.Close();
                reader.Dispose();
                response.Dispose();
                request.Abort();

                return ret_html.Contains("操作成功");
            }
            catch (WebException ex)
            {
                string msg = $"添加备注失败：用户 {b.username} 单{b.betno} 错误{ex.Message}";
                appSittingSet.Log(msg);
                return false;
            }
        }

        #endregion


        /// <summary>
        /// 登录
        /// </summary>
        /// <returns></returns>
        public static bool login()
        {
            try
            {
                string s1 = appSittingSet.readAppsettings("TG");
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
                opt.AddArgument("--audio-output-channels=0");
                opt.AddArgument("--disable-default-apps");
                opt.AddArgument("--disable-extensions");
                opt.AddArgument("--disable-translate");
                opt.AddArgument("--disable-sync");
                opt.AddArgument("--hide-scrollbars");
                opt.AddArgument("--mute-audio");

                if (selenium==null)
                {
                    selenium = new ChromeDriver(opt);

                }
                selenium.Manage().Timeouts().PageLoad = TimeSpan.FromMilliseconds(100);
                //selenium = new PhantomJSDriver();
                selenium.Url = urlbase;
                selenium.FindElement(By.Id("username")).SendKeys(acc);
                selenium.FindElement(By.Id("txtPasword")).SendKeys(pwd);
                selenium.FindElement(By.Name("submit")).Click();
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
                //IWebDriver selenium = new ChromeDriver();
                selenium.Manage().Timeouts().PageLoad = TimeSpan.FromMilliseconds(200);
                selenium.Navigate().GoToUrl($"{urlbase}money/amount-recharge?");
                selenium.FindElement(By.Name("username")).SendKeys(b.username);
                selenium.FindElement(By.Name("amount")).SendKeys(b.betMoney.ToString());
                selenium.FindElement(By.Name("remark")).SendKeys(b.bbid);
                selenium.FindElement(By.Name("sub_btn")).Click();
                selenium.Manage().Timeouts().PageLoad = TimeSpan.FromMilliseconds(200);
                //查看返回信息
                if (selenium.PageSource.Contains("没有找到匹配输入的记录"))
                {
                    //没有 用户记录
                    b.passed = false;
                    b.msg = "账户不存在";
                }
                //else if(selenium.PageSource.Contains("输入字符"))
                //{
                //    //输入字符不正确
                //}
                else if (selenium.PageSource.Contains("操作成功"))
                {
                    //插入本地数据库
                    SQLiteHelper.SQLiteHelper.execSql($"insert into record values({0},'{b.bbid}','{b.username}',datetime(CURRENT_TIMESTAMP,'localtime'),0,1);");

                    //操作成功 确认
                    b.passed = true;
                }
                platQPGV2.confirmAct(b);
                selenium.Navigate().GoToUrl($"{urlbase}money/amount-recharge?r={DateTime.Now.ToString("yyyyMMddHHmmss")}");

            }
            catch (Exception ex)
            {

                string msg = $"确认失败：用户 {b.username} 单{b.bbid} 错误{ex.Message}";
                appSittingSet.Log(msg);

                if (selenium.PageSource.Contains("退出"))
                {
                    login();
                    appSittingSet.Log("重新登陆");
                }

            }
        }
    }
}
