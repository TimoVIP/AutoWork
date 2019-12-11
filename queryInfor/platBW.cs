using BaseFun;
//using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using HtmlAgilityPack;
using System.Data;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;

namespace queryInfor
{
    public static class platBW
    {
        public static string urlbase { get; set; }
        public static string acc1 { get; set; }
        public static string pwd1 { get; set; }
        public static string acc2 { get; set; }
        public static string pwd2 { get; set; }
        public static string otp { get; set; }

        public static string PHPSESSID { get; set; }
        private static CookieContainer cookie { get; set; } = new CookieContainer();
        private static string token { get; set; }
        //private static IWebDriver selenium { get; set; }
        private static string Authorization { get; set; }

        private static HttpWebRequest myReq = null;
        private static HttpWebResponse wr = null;
        private static CredentialCache mycache = new CredentialCache();

        /// <summary>
        /// 游戏编号
        /// </summary>
        public static Dictionary<string, string> dic_gameids = new Dictionary<string, string>();

        //public static Dictionary<string, string> dic_channel = new Dictionary<string, string>();

        /// <summary>
        /// 最近一笔兑换成功记录的时间
        /// </summary>
        private static string latest_dhsj { get; set; } = "";

        /// <summary>
        /// 最近一笔充值成功记录的时间
        /// </summary>
        private static string latest_czsj { get; set; } = "";


        #region 不用
        /*
        public static bool login_()
        {
            string s1, s2;
            try
            {

                s1 = appSittingSet.readAppsettings("BW1");
                s2 = appSittingSet.readAppsettings("BW2");

                //if(acc=="")
                //    acc = s1.Split('|')[0];
                //if(pwd=="")
                //    pwd = s1.Split('|')[1];

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
                //opt.AddArgument("--headless");
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
                //不起作用
                //opt.AddArguments("--proxy-server=" + s1.Split('|')[2]);

                //加了这个报错
                //opt.AddExtension("proxy.zip");

                //opt.AddUserProfilePreference("", "");
                opt.AddArgument($"--proxy={s2.Split('|')[2]}");
                opt.AddArgument($"--proxy-auth={s2.Split('|')[0]}:{s2.Split('|')[1]}");
                var byteArray = Encoding.ASCII.GetBytes($"{s2.Split('|')[0]}:{s2.Split('|')[1]}");
                opt.AddArgument($"--Authorization: Basic {Convert.ToBase64String(byteArray)}");

                //opt.AddAdditionalCapability("phantomjs.page.settings.userAgent", "myagent/blah.blah");
                //opt.AddAdditionalCapability("phantomjs.page.customHeaders.Referer", "https://www.google.com/");

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
                //不能通过这样访问
                //http://admin:password@192.168.16.1/

                urlbase = s2.Split('|')[2];
                //selenium.Url = urlbase;
                selenium.Navigate().GoToUrl(urlbase);

                //WebDriverWait wait = new WebDriverWait(selenium, new TimeSpan(0,0,10));
                //selenium.Navigate().GoToUrl(s2.Split('|')[2]);
                //IAlert prompt = selenium.SwitchTo().Alert();
                //prompt.SetAuthenticationCredentials(acc,pwd);

                Thread.Sleep(200);
                //处理弹层
                //IAlert prompt = selenium.SwitchTo().Alert();
                //prompt.SendKeys(s1.Split('|')[0]);
                //prompt.SendKeys(Keys.Tab);
                //prompt.SendKeys(s1.Split('|')[1]);
                //prompt.Accept();
                //Thread.Sleep(200);
                //输入账号
                selenium.FindElement(By.Id("username")).SendKeys(s2.Split('|')[0]);
                selenium.FindElement(By.Id("password")).SendKeys(s2.Split('|')[0]);
                selenium.FindElement(By.Id("code")).SendKeys(otp);
                selenium.FindElement(By.XPath("/html/body/div[2]/div/div/div/div/form/div[6]/div/button")).Click();
                Thread.Sleep(200);
                //判断是否登陆 
                if (selenium.Title.Contains("管理员") || selenium.Title.Contains("后台") || selenium.Url.Contains("index/admin"))
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

        */
        #endregion

        public static async void HTTP_GET()
        {
            var TARGETURL = "http://en.wikipedia.org/";

            HttpClientHandler handler = new HttpClientHandler()
            {
                Proxy = new WebProxy("http://127.0.0.1:8888"),
                UseProxy = true,
            };

            Console.WriteLine("GET: + " + TARGETURL);

            // ... Use HttpClient.            
            HttpClient client = new HttpClient(handler);

            var byteArray = Encoding.ASCII.GetBytes("username:password1234");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            HttpResponseMessage response = await client.GetAsync(TARGETURL);
            HttpContent content = response.Content;

            // ... Check Status Code                                
            Console.WriteLine("Response StatusCode: " + (int)response.StatusCode);

            // ... Read the string.
            string result = await content.ReadAsStringAsync();

            // ... Display the result.
            if (result != null &&
            result.Length >= 50)
            {
                Console.WriteLine(result.Substring(0, 50) + "...");
            }
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <returns></returns>
        public static bool login(out string resualt)
        {
            //获取用户名、密码 等资料
            Hashtable config = appSittingSet.readConfig("appconfig");
            //一层账号
            acc1 = config["bw1"].ToString().Split('|')[0];
            pwd1 = config["bw1"].ToString().Split('|')[1];
            urlbase = config["bw1"].ToString().Split('|')[2];
            PHPSESSID = config["PHPSESSID"].ToString();
            try
            {
                myReq = (HttpWebRequest)WebRequest.Create($"{urlbase}admin.php/user/publics/signin.html");
                if (mycache.GetCredential(new Uri(urlbase), "Basic") == null)
                {
                    mycache.Add(new Uri(urlbase), "Basic", new NetworkCredential(acc1, pwd1));
                }

                myReq.Credentials = mycache;
                Authorization = "Basic " + Convert.ToBase64String(new ASCIIEncoding().GetBytes(acc1 + ":" + pwd1));
                myReq.Headers.Add("Authorization", Authorization);

                cookie.Add(new System.Net.Cookie("PHPSESSID", PHPSESSID, "/", urlbase.Split(':')[1].Replace("/","")));
                myReq.CookieContainer = cookie;

                wr = (HttpWebResponse)myReq.GetResponse();
                cookie.Add(wr.Cookies);
                Stream receiveStream = wr.GetResponseStream();
                cookie.Add(wr.Cookies);
                StreamReader reader = new StreamReader(receiveStream, Encoding.UTF8);
                string content = reader.ReadToEnd();
                //myReq.Abort();
                reader.Close();
                reader.Dispose();
                receiveStream.Close();
                receiveStream.Dispose();
                wr.Close();
                wr.Dispose();

                if ((content.Contains("index.html")) || (content.Contains("包网官网后台") && content.Contains("谷歌二步验证")))
                {
                    //google 验证码
                    /*
                    myReq = (HttpWebRequest)WebRequest.Create($"{urlbase}admin.php/user/publics/getgooglemsg.html");
                    myReq.Method = "POST";
                    myReq.ContentType = "application/x-www-form-urlencoded";
                    myReq.Credentials = mycache;
                    myReq.Headers.Add("Authorization", "Basic" + Convert.ToBase64String(new ASCIIEncoding().GetBytes(acc1 + ":" + pwd1)));
                    myReq.Headers.Add("X-Requested-With", "XMLHttpRequest");
                    myReq.Referer = $"{urlbase}admin.php/user/publics/signin.html";

                    byte[] data = Encoding.UTF8.GetBytes($"provideraccount={acc2}&passwd={pwd2}");
                    myReq.ContentLength = data.Length;
                    Stream reqStream = myReq.GetRequestStream();
                    reqStream.Write(data, 0, data.Length);
                    reqStream.Close();
                    wr = myReq.GetResponse();
                    receiveStream = wr.GetResponseStream();
                    reader = new StreamReader(receiveStream, Encoding.UTF8);
                    content = reader.ReadToEnd();
                    reader.Close();
                    reader.Dispose();
                    reqStream.Close();
                    reqStream.Dispose();
                    wr.Close();
                    wr.Dispose();
                    */

                    //提交数据
                    myReq = (HttpWebRequest)WebRequest.Create($"{urlbase}admin.php/user/publics/signin.html");
                    myReq.Method = "POST";
                    myReq.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";

                    //mycache.Add(new Uri(urlbase), "Basic", new NetworkCredential(acc, pwd));
                    myReq.Credentials = mycache;
                    myReq.Headers.Add("Authorization", Authorization);

                    myReq.Headers.Add("X-Requested-With", "XMLHttpRequest");
                    myReq.Referer = $"{urlbase}admin.php/user/publics/signin.html";


                    byte[] data = Encoding.UTF8.GetBytes($"provideraccount={acc2}&passwd={pwd2}&validate_type=2&code={otp}");
                    myReq.ContentLength = data.Length;
                    Stream reqStream = myReq.GetRequestStream();
                    reqStream.Write(data, 0, data.Length);
                    reqStream.Close();
                    wr = (HttpWebResponse)myReq.GetResponse();
                    cookie.Add(wr.Cookies);
                    receiveStream = wr.GetResponseStream();
                    reader = new StreamReader(receiveStream, Encoding.UTF8);
                    content = reader.ReadToEnd();

                    reader.Close();
                    reader.Dispose();
                    receiveStream.Close();
                    receiveStream.Dispose();
                    wr.Close();
                    wr.Dispose();
                    //"{\"code\":1,\"msg\":\"登录成功\",\"data\":\"\",\"url\":\"\\/admin.php\\/admin\\/index\\/index.html\",\"wait\":3}"



                    if (!content.Contains("登录成功"))
                    {
                        appSittingSet.Log("登陆失败" + content);
                        resualt = "google 验证码不对";
                        return false;
                    }


                    //跳转到主页 必须步骤
                    myReq = (HttpWebRequest)WebRequest.Create($"{urlbase}admin.php/admin/index/index.html");
                    myReq.Method = "GET";
                    myReq.Credentials = mycache;
                    myReq.Headers.Add("Authorization", Authorization);
                    myReq.Headers.Add("Upgrade-Insecure-Requests", "1");
                    //myReq.Headers.Add("Sec-Fetch-Mode", "navigate");
                    //myReq.Headers.Add("Sec-Fetch-Site", "same-origin");
                    //myReq.Headers.Add("Sec-Fetch-User", "?1");
                    //myReq.Host = urlbase.Replace("https://", "").Replace("/", "");

                    myReq.CookieContainer = cookie;
                    myReq.KeepAlive = true;
                    myReq.Referer = $"{urlbase}admin.php/user/publics/signin.html";
                    myReq.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/76.0.3809.100 Safari/537.36";

                    wr = (HttpWebResponse)myReq.GetResponse();
                    receiveStream = wr.GetResponseStream();
                    reader = new StreamReader(receiveStream, Encoding.UTF8);
                    content = reader.ReadToEnd();

                    if (content.Contains("欢迎登陆后台") && content.Contains("退出帐号"))
                    {
                        resualt = "登陆成功";
                        return true;
                    }
                    else
                        resualt = "请先通过浏览器登陆一遍，复制浏览器中 Cookie: PHPSESSID=53b551625f315ea2e16e3716a0a70e47 这个部分替换config中的内容";
                        return false;

                }
                else
                {
                        resualt = "登陆成功";
                        return false;
                }
            }
            catch (Exception ex)
            {
                resualt = "基本验证失败";
                appSittingSet.Log(ex.Message);
                return false;
            }

        }


        /// <summary>
        /// 获取基本信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string getUserinfor(string id)
        {
            string url = $"{urlbase}admin.php/gameplayer/index/info/user_id/{id}.html";
            myReq = (HttpWebRequest)WebRequest.Create(url);
            //mycache = new CredentialCache();
            //mycache.Add(new Uri(urlbase), "Basic", new NetworkCredential(acc1, pwd1));
            myReq.Credentials = mycache;
            myReq.Headers.Add("Authorization", Authorization);
            myReq.CookieContainer = cookie;

            wr = (HttpWebResponse)myReq.GetResponse();
            Stream receiveStream = wr.GetResponseStream();
            StreamReader reader = new StreamReader(receiveStream, Encoding.UTF8);
            string content = reader.ReadToEnd();
            //myReq.Abort();
            reader.Close();
            reader.Dispose();
            receiveStream.Close();
            receiveStream.Dispose();
            wr.Close();
            wr.Dispose();

            HtmlDocument htmlDocument = new HtmlDocument();
            //string ss= htmlDocument.GetElementbyId("builder-table-main").OuterHtml;

            htmlDocument.LoadHtml(content);

            //HtmlNode node1 = htmlDocument.DocumentNode.SelectSingleNode("//*[@id='builder-table-main']");
            HtmlNodeCollection node2 = htmlDocument.DocumentNode.SelectNodes("//*[@id='builder-table-main']//tr");
            string display = "";

            if (node2==null)
            {
                display = "用户不存在\r\n"+htmlDocument.DocumentNode.SelectSingleNode("/html/body/div[1]/div/div/h1").InnerText;
                return display;
            }

            foreach (HtmlNode item in node2)
            {
                foreach (HtmlNode c in item.ChildNodes)
                {
                    if (c.Name=="td")
                    {
                        display += c.InnerText.Replace("\r\n", "").Replace(" ", "").Trim() + ":";
                    }
                }
                display = display.TrimEnd(':');
                display += "\r\n";
            }

            return display;
        }
        public static Hashtable getUserinfor(string id,out string s)
        {

            string url = $"{urlbase}admin.php/gameplayer/index/index.html?_s=type=user_id|keyword={id}|channel=|reg_time=|login_time=|category=|usertype=|min_times=|max_times=|wincount=|lostcount=|min_gold=|max_gold=|min_recharge=|max_recharge=|min_exchange=|max_exchange=|min_waste=|max_waste=|special_status=&_o=type=eq|keyword=eq|channel=eq|reg_time=between%20time|login_time=between%20time|category=eq|usertype=eq|min_times=eq|max_times=eq|wincount=eq|lostcount=eq|min_gold=eq|max_gold=eq|min_recharge=eq|max_recharge=eq|min_exchange=eq|max_exchange=eq|min_waste=eq|max_waste=eq|special_status=eq";
            myReq = (HttpWebRequest)WebRequest.Create(url);
            //mycache = new CredentialCache();
            //mycache.Add(new Uri(urlbase), "Basic", new NetworkCredential(acc1, pwd1));
            myReq.Credentials = mycache;
            myReq.Headers.Add("Authorization", Authorization);
            myReq.CookieContainer = cookie;

            wr = (HttpWebResponse)myReq.GetResponse();
            Stream receiveStream = wr.GetResponseStream();
            StreamReader reader = new StreamReader(receiveStream, Encoding.UTF8);
            string content = reader.ReadToEnd();
            //myReq.Abort();
            reader.Close();
            reader.Dispose();
            receiveStream.Close();
            receiveStream.Dispose();
            wr.Close();
            wr.Dispose();

            HtmlDocument htmlDocument = new HtmlDocument();

            htmlDocument.LoadHtml(content);

            Hashtable ht = new Hashtable();
            s = "";
            //表头
            HtmlNodeCollection thead = htmlDocument.DocumentNode.SelectNodes("//*[@id='builder-table-head']/table/thead/tr/th");
            //string[] arr_thead = new string[thead.Count];
            

            //内容
            HtmlNodeCollection tbody = htmlDocument.DocumentNode.SelectNodes("//*[@id='builder-table-main']/tbody/tr/td");


            //if (node2==null)
            //{
            //    display = "用户不存在\r\n"+htmlDocument.DocumentNode.SelectSingleNode("/html/body/div[1]/div/div/h1").InnerText;
            //    return null;
            //}
            if (tbody.Count<=2)
            {
                s = "没有数据";
                return ht;
            }

            for (int i = 1; i < tbody.Count; i++)
            {

                string key = thead[i].InnerText.Replace("\r\n", "").Replace(" ", "").Trim();
                string value = tbody[i].InnerText.Replace("\r\n", "").Replace(" ", "").Trim() ;
                s += key + ":" + value+"\r\n";
                ht.Add(key, value);
            }

            //PropertyInfo[] PropertyList = u.GetType().GetProperties();


            //ht.Add();


            //foreach (HtmlNode item in node2)
            //{
            //    //foreach (HtmlNode c in item.ChildNodes)
            //    //{
            //    //    if (c.Name=="td")
            //    //    {
            //    //        display += c.InnerText.Replace("\r\n", "").Replace(" ", "").Trim() + ":";
            //    //    }
            //    //}

            //    display = display.TrimEnd(':');
            //    display += "\r\n";
            //}

            return ht;
        }

        /// <summary>
        /// 查询赠送金币记录
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <param name="page">第几页</param>
        /// <param name="pagesize">页大小</param>
        /// <returns></returns>
        public static DataTable getJBZS(string id,DateTime d1, DateTime d2, out string s ,int page=1,int pagesize=100)
        {
            string url = $"{urlbase}admin.php/user/log/index.html?_s=type%3D4%7Coper_id%3D%7Cuser_id%3D{id}%7Ccreate_time%3D{d1.ToString("yyyy-MM-dd HH:mm:ss")}+-+{d2.ToString("yyyy-MM-dd HH:mm:ss")}%7Cis_export%3D&_o=type%3Deq%7Coper_id%3Deq%7Cuser_id%3Deq%7Ccreate_time%3Dbetween+time%7Cis_export%3Deq&page={page}&list_rows={pagesize}";
            //string url = $"{urlbase}admin.php/gameplayer/index/cunk/group/tab3/user_id/{id}.html?_s=roomid=|logtype=8|create_time=|is_history=&_o=roomid=eq|logtype=eq|create_time=between%20time|is_history=eq&page={page}&list_rows={pagesize}";
            //string url = $"{urlbase}admin.php/user/log/index.html?_s=type=4|oper_id=|user_id={id}|create_time={d1.ToString("yyyy-MM-dd HH:mm:ss")} - {d2.ToString("yyyy-MM-dd HH:mm:ss")}|is_export=_o: type =eq|oper_id=eq|user_id=eq|create_time=between time|is_export=eq&page={page}&list_rows={pagesize}";
            myReq = (HttpWebRequest)WebRequest.Create(url);
            //mycache = new CredentialCache();
            //mycache.Add(new Uri(urlbase), "Basic", new NetworkCredential(acc1, pwd1));
            myReq.Credentials = mycache;
            myReq.Headers.Add("Authorization", Authorization);
            myReq.CookieContainer = cookie;

            wr = (HttpWebResponse)myReq.GetResponse();
            Stream receiveStream = wr.GetResponseStream();
            StreamReader reader = new StreamReader(receiveStream, Encoding.UTF8);
            string content = reader.ReadToEnd();
            //myReq.Abort();
            reader.Close();
            reader.Dispose();
            receiveStream.Close();
            receiveStream.Dispose();
            wr.Close();
            wr.Dispose();

            DataTable dt = new DataTable();

            HtmlDocument htmlDocument = new HtmlDocument();

            htmlDocument.LoadHtml(content);

            //获取表头
            HtmlNodeCollection node_thead = htmlDocument.DocumentNode.SelectNodes("//*[@id='builder-table-head']/table/thead/tr/th");

            foreach (HtmlNode item in node_thead)
            {
                dt.Columns.Add(item.InnerText.Replace("\r\n", "").Replace(" ", "").Trim());
            }
            s = "";
            int num = 0;
            double d = 0;
           
            //获取内容
            HtmlNodeCollection node_trow = htmlDocument.DocumentNode.SelectNodes("//*[@id='builder-table-main']/tbody/tr");
            if (node_trow.Count==1 && node_trow[0].InnerHtml.Contains("暂无数据"))
            {
                //没有数据
                return dt;
            }
            foreach (HtmlNode item in node_trow)
            {
                HtmlNodeCollection hnc = item.SelectNodes("td");
                DataRow dataRow = dt.NewRow();
                for (int e = 0; e <hnc.Count; e++)
                {
                    dataRow[e] = hnc[e].InnerText.Replace("\r\n", "").Trim();
                }
                dt.Rows.Add(dataRow);
                num += 1;
                d += double.Parse(dataRow[4].ToString());
            }
            s = $"订单数:{num}，金额:{d}";
            return dt;
        }

        /// <summary>
        /// 查询用户兑换记录
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <param name="page">第几页</param>
        /// <param name="pagesize">页大小</param>
        /// <returns></returns>
        public static DataTable getYHDH(string id,DateTime d1, DateTime d2,out string s,int page=1,int pagesize=100)
        {
            string url = $"{urlbase}admin.php/gameplayer/gameuser/exchangerecord.html?_s=search_month=|date_at={d1.ToString("yyyy-MM-dd HH:mm:ss")}%20-%20{d2.ToString("yyyy-MM-dd HH:mm:ss")}|check_time=|status=2|ex_type_id=|is_agent_pay=|ex_channel=|user_type=|auto_handle=|special_status=|UserID={id}|OrderID=|OrderNo=|receive_id=|is_export=&_o=search_month=eq|date_at=between%20time|check_time=between%20time|status=eq|ex_type_id=eq|is_agent_pay=eq|ex_channel=eq|user_type=eq|auto_handle=eq|special_status=eq|UserID=eq|OrderID=eq|OrderNo=eq|receive_id=eq|is_export=eq&page={page}&list_rows={pagesize}";

            myReq = (HttpWebRequest)WebRequest.Create(url);
            myReq.Credentials = mycache;
            myReq.Headers.Add("Authorization", Authorization);
            myReq.CookieContainer = cookie;

            wr = (HttpWebResponse)myReq.GetResponse();
            Stream receiveStream = wr.GetResponseStream();
            StreamReader reader = new StreamReader(receiveStream, Encoding.UTF8);
            string content = reader.ReadToEnd();
            //myReq.Abort();
            reader.Close();
            reader.Dispose();
            receiveStream.Close();
            receiveStream.Dispose();
            wr.Close();
            wr.Dispose();

            DataTable dt = new DataTable();

            HtmlDocument htmlDocument = new HtmlDocument();

            htmlDocument.LoadHtml(content);

            //获取表头
            HtmlNodeCollection node_thead = htmlDocument.DocumentNode.SelectNodes("//*[@id='builder-table-head']/table/thead/tr/th");

            foreach (HtmlNode item in node_thead)
            {
                dt.Columns.Add(item.InnerText.Replace("\r\n", "").Replace(" ", "").Trim());
            }

            s = htmlDocument.DocumentNode.SelectSingleNode("//*[@id='main-container']/div[2]/div[1]/p").InnerText.Replace("\r\n", "").Trim().Replace("&nbsp;", "").Replace("当前查询", "").Replace("兑换", "").Replace("    ", " ");
            //获取内容
            HtmlNodeCollection node_trow = htmlDocument.DocumentNode.SelectNodes("//*[@id='builder-table-main']/tbody/tr");
            if (node_trow.Count==1 && node_trow[0].InnerHtml.Contains("暂无数据"))
            {
                //没有数据
                return dt;
            }
            foreach (HtmlNode item in node_trow)
            {
                HtmlNodeCollection hnc = item.SelectNodes("td");
                DataRow dataRow = dt.NewRow();
                for (int e = 0; e <hnc.Count; e++)
                {
                    dataRow[e] = hnc[e].InnerText.Replace("\r\n", "").Trim();
                }
                dt.Rows.Add(dataRow);

            }
            //时间
            latest_dhsj = dt.Rows[0][7].ToString();
            return dt;
        }


        /// <summary>
        /// 查询用户充值记录
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <param name="page">第几页</param>
        /// <param name="pagesize">页大小</param>
        /// <returns></returns>
        public static DataTable getYHCZ(string id,DateTime d1, DateTime d2,out string s ,int page=1,int pagesize=100)
        {
            //if (latest_dhsj!="")
            //{
            //    d1 = DateTime.Parse(latest_dhsj);
            //}
            //string url = $"{urlbase}admin.php/gameplayer/gameuser/rechargerecord.html?_s=date_at%3D2019-10-22+-+2019-10-23%7Ccreate_ymd%3D%7Cchannel_id%3D%7Corder_status%3D1%7Cuser_status%3D%7Cplat_form%3D%7Cmerchant_id%3D%7Crecharge_channel%3D%7Cpay_time%3D%7CUserID%3D2989719%7COrderID%3D%7COrderNo%3D%7Cis_export%3D&_o=date_at%3Dbetween+time%7Ccreate_ymd%3Deq%7Cchannel_id%3Deq%7Corder_status%3Deq%7Cuser_status%3Deq%7Cplat_form%3Deq%7Cmerchant_id%3Deq%7Crecharge_channel%3Deq%7Cpay_time%3Dbetween+time%7CUserID%3Deq%7COrderID%3Deq%7COrderNo%3Deq%7Cis_export%3Deq&page=1&list_rows=100";
            string url = $"{urlbase}admin.php/gameplayer/gameuser/rechargerecord.html?_s=date_at={d1.ToString("yyyy-MM-dd HH:mm:ss")}%20-%20{d2.ToString("yyyy-MM-dd HH:mm:ss")}|create_ymd=|channel_id=|order_status=|user_status=|plat_form=|merchant_id=|recharge_channel=|pay_time=|UserID={id}|OrderID=|OrderNo=|is_export=&_o=date_at=between%20time|create_ymd=eq|channel_id=eq|order_status=eq|user_status=eq|plat_form=eq|merchant_id=eq|recharge_channel=eq|pay_time=between%20time|UserID=eq|OrderID=eq|OrderNo=eq|is_export=eq&page={page}&list_rows={pagesize}";
            //string url = $"{urlbase}admin.php/gameplayer/index/recharge/group/tab4/user_id/{id}.html?_s=create_time={d1.ToString("yyyy-MM-dd HH:mm:ss")}%20-%20{d2.ToString("yyyy-MM-dd HH:mm:ss")}|recharge_table=|order_status=1&_o=create_time=between%20time|recharge_table=eq|order_status=eq&page={page}&list_rows={pagesize}";
            myReq = (HttpWebRequest)WebRequest.Create(url);
            myReq.Credentials = mycache;
            myReq.Headers.Add("Authorization", Authorization);
            myReq.CookieContainer = cookie;

            wr = (HttpWebResponse)myReq.GetResponse();
            Stream receiveStream = wr.GetResponseStream();
            StreamReader reader = new StreamReader(receiveStream, Encoding.UTF8);
            string content = reader.ReadToEnd();
            //myReq.Abort();
            reader.Close();
            reader.Dispose();
            receiveStream.Close();
            receiveStream.Dispose();
            wr.Close();
            wr.Dispose();

            DataTable dt = new DataTable();

            HtmlDocument htmlDocument = new HtmlDocument();

            htmlDocument.LoadHtml(content);

            //获取表头
            HtmlNodeCollection node_thead = htmlDocument.DocumentNode.SelectNodes("//*[@id='builder-table-head']/table/thead/tr/th");

            foreach (HtmlNode item in node_thead)
            {
                dt.Columns.Add(item.InnerText.Replace("\r\n", "").Replace(" ", "").Trim());
            }
             s = htmlDocument.DocumentNode.SelectSingleNode("//*[@id='main-container']/div[2]/div[1]/p").InnerText.Replace("\r\n", "").Trim().Replace("&nbsp;", "").Replace("当前查询","").Replace("充值", "").Replace("    ", " ");
            //获取内容
            HtmlNodeCollection node_trow = htmlDocument.DocumentNode.SelectNodes("//*[@id='builder-table-main']/tbody/tr");
            if (node_trow.Count==1 && node_trow[0].InnerHtml.Contains("暂无数据"))
            {
                //没有数据
                return dt;
            }
            foreach (HtmlNode item in node_trow)
            {
                HtmlNodeCollection hnc = item.SelectNodes("td");
                DataRow dataRow = dt.NewRow();
                for (int e = 0; e <hnc.Count; e++)
                {
                    dataRow[e] = hnc[e].InnerText.Replace("\r\n", "").Trim();
                }
                dt.Rows.Add(dataRow);
            }


            //获取渠道列表数据字典
            //if (dic_channel.Count == 0)
            //{

            //    HtmlNodeCollection node_channel = htmlDocument.DocumentNode.SelectNodes("//*[@id='search_channel']/option");
            //    foreach (HtmlNode item in node_channel)
            //    {

            //        string nid = item.GetAttributeValue("value", "");
            //        string nvalue = item.InnerText.Replace(" ", "");
            //        if (nid != "" && nvalue != "")
            //        {
            //            dic_channel.Add(nvalue, nid);
            //        }
            //    }
            //}
            //充值时间
            latest_czsj = dt.Rows[0][20].ToString();
            return dt;
        }

        /// <summary>
        /// 查询用户总游戏记录
        /// </summary>
        /// <param name="id"></param>
        /// <param name="d1"></param>
        /// <param name="d2"></param>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        public static DataTable getYHZYXJL(string id,DateTime d1, DateTime d2,int page=1,int pagesize=100)
        {
            //if (latest_czsj != "")
            //{
            //    d1 = DateTime.Parse(latest_czsj);
            //}
            string url = $"{urlbase}admin.php/gameplayer/index/game_statics/group/tab8/user_id/{id}.html?_s=create_time%3D{d1.ToString("yyyy-MM-dd HH:mm:ss")}+-+{d2.ToString("yyyy-MM-dd HH:mm:ss")}%7Cgame_id%3D&_o=create_time%3Dbetween+time%7Cgame_id%3Deq&group=tab8&user_id={id}&page={page}&list_rows={pagesize}";
            myReq = (HttpWebRequest)WebRequest.Create(url);
            myReq.Credentials = mycache;
            myReq.Headers.Add("Authorization", Authorization);
            myReq.CookieContainer = cookie;

            wr = (HttpWebResponse)myReq.GetResponse();
            Stream receiveStream = wr.GetResponseStream();
            StreamReader reader = new StreamReader(receiveStream, Encoding.UTF8);
            string content = reader.ReadToEnd();
            //myReq.Abort();
            reader.Close();
            reader.Dispose();
            receiveStream.Close();
            receiveStream.Dispose();
            wr.Close();
            wr.Dispose();

            DataTable dt = new DataTable();

            HtmlDocument htmlDocument = new HtmlDocument();

            htmlDocument.LoadHtml(content);

            
            //获取表头
            HtmlNodeCollection node_thead = htmlDocument.DocumentNode.SelectNodes("//*[@id='builder-table-head']/table/thead/tr/th");

            foreach (HtmlNode item in node_thead)
            {
                dt.Columns.Add(item.InnerText.Replace("\r\n", "").Replace(" ", "").Trim());
            }
            // s = htmlDocument.DocumentNode.SelectSingleNode("//*[@id='main-container']/div[2]/div[1]/p").InnerText.Replace("\r\n", "").Trim().Replace("&nbsp;", "").Replace("当前查询","").Replace("兑换", "").Replace("    ", " ");
            //获取内容
            HtmlNodeCollection node_trow = htmlDocument.DocumentNode.SelectNodes("//*[@id='builder-table-main']/tbody/tr");
            if (node_trow.Count==1 && node_trow[0].InnerHtml.Contains("暂无数据"))
            {
                //没有数据
                return dt;
            }
            foreach (HtmlNode item in node_trow)
            {
                HtmlNodeCollection hnc = item.SelectNodes("td");
                DataRow dataRow = dt.NewRow();
                for (int e = 0; e <hnc.Count; e++)
                {
                    dataRow[e] = hnc[e].InnerText.Replace("\r\n", "").Trim();
                }
                dt.Rows.Add(dataRow);
            }

            //获取游戏列表数据字典
            //HtmlNodeCollection node_gameids = htmlDocument.DocumentNode.SelectSingleNode("//*[@id='search_game_id']").ChildNodes;
            if (dic_gameids.Count == 0)
            {

                HtmlNodeCollection node_gameids = htmlDocument.DocumentNode.SelectNodes("//*[@id='search_game_id']/option");
                foreach (HtmlNode item in node_gameids)
                {

                    string nid = item.GetAttributeValue("value", "");
                    string nvalue = item.InnerText.Replace(" ", "");
                    if (nid != "" && nvalue != "")
                    {
                        dic_gameids.Add(nvalue, nid);
                    }


                    //HtmlNodeCollection hnc = item.SelectNodes("td");
                    //DataRow dataRow = dt.NewRow();
                    //for (int e = 0; e <hnc.Count; e++)
                    //{
                    //    dataRow[e] = hnc[e].InnerText.Replace("\r\n", "").Trim();
                    //}
                    //dt.Rows.Add(dataRow);
                }
            }
            return dt;
        }

        /// <summary>
        /// 用户游戏记录
        /// </summary>
        /// <param name="id"></param>
        /// <param name="d1"></param>
        /// <param name="d2"></param>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        public static DataTable getYHYXJL(string id,DateTime d1, DateTime d2,string gameid,int page=1,int pagesize=100)
        {

            string url = $"{urlbase}admin.php/gameplayer/index/gamerecord/group/tab5/user_id/{id}.html?_s=create_time%3D{d1.ToString("yyyy-MM-dd HH:mm:ss")}+-+{d2.ToString("yyyy-MM-dd HH:mm:ss")}%7Cgame_id%3D{gameid}%7Cis_history%3D&_o=create_time%3Dbetween+time%7Cgame_id%3Deq%7Cis_history%3Deq&group=tab5&user_id={id}&page={page}&list_rows={pagesize}";
            //string url = $"{urlbase}admin.php/gameplayer/index/game_statics/group/tab8/user_id/{id}.html?_s=create_time%3D{d1.ToString("yyyy-MM-dd HH:mm:ss")}+-+{d2.ToString("yyyy-MM-dd HH:mm:ss")}%7Cgame_id%3D&_o=create_time%3Dbetween+time%7Cgame_id%3Deq&group=tab8&user_id={id}&page={page}&list_rows={pagesize}";
            myReq = (HttpWebRequest)WebRequest.Create(url);
            myReq.Credentials = mycache;
            myReq.Headers.Add("Authorization", Authorization);
            myReq.CookieContainer = cookie;

            wr = (HttpWebResponse)myReq.GetResponse();
            Stream receiveStream = wr.GetResponseStream();
            StreamReader reader = new StreamReader(receiveStream, Encoding.UTF8);
            string content = reader.ReadToEnd();
            //myReq.Abort();
            reader.Close();
            reader.Dispose();
            receiveStream.Close();
            receiveStream.Dispose();
            wr.Close();
            wr.Dispose();

            DataTable dt = new DataTable();

            HtmlDocument htmlDocument = new HtmlDocument();

            htmlDocument.LoadHtml(content);

            //获取表头
            HtmlNodeCollection node_thead = htmlDocument.DocumentNode.SelectNodes("//*[@id='builder-table-head']/table/thead/tr/th");

            foreach (HtmlNode item in node_thead)
            {
                dt.Columns.Add(item.InnerText.Replace("\r\n", "").Replace(" ", "").Trim());
            }
            // s = htmlDocument.DocumentNode.SelectSingleNode("//*[@id='main-container']/div[2]/div[1]/p").InnerText.Replace("\r\n", "").Trim().Replace("&nbsp;", "").Replace("当前查询","").Replace("兑换", "").Replace("    ", " ");
            //获取内容
            HtmlNodeCollection node_trow = htmlDocument.DocumentNode.SelectNodes("//*[@id='builder-table-main']/tbody/tr");
            if (node_trow.Count==1 && node_trow[0].InnerHtml.Contains("暂无数据"))
            {
                //没有数据
                return dt;
            }
            foreach (HtmlNode item in node_trow)
            {
                HtmlNodeCollection hnc = item.SelectNodes("td");
                DataRow dataRow = dt.NewRow();
                for (int e = 0; e <hnc.Count; e++)
                {
                    dataRow[e] = hnc[e].InnerText.Replace("\r\n", "").Trim();
                }
                dt.Rows.Add(dataRow);
            }
            return dt;
        }

        /// <summary>
        /// 获取用户渠道 汉字
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public static string getChannel(string uid)
        {
            string url = $"{urlbase}admin.php/gameplayer/index/index.html?_s=type=user_id|keyword={uid}|channel=|reg_time=|login_time=|category=|usertype=|min_times=|max_times=|wincount=|lostcount=|min_gold=|max_gold=|min_recharge=|max_recharge=|min_exchange=|max_exchange=|min_waste=|max_waste=&_o=type=eq|keyword=eq|channel=eq|reg_time=between%20time|login_time=between%20time|category=eq|usertype=eq|min_times=eq|max_times=eq|wincount=eq|lostcount=eq|min_gold=eq|max_gold=eq|min_recharge=eq|max_recharge=eq|min_exchange=eq|max_exchange=eq|min_waste=eq|max_waste=eq";
            myReq = (HttpWebRequest)WebRequest.Create(url);
            myReq.Credentials = mycache;
            myReq.Headers.Add("Authorization", Authorization);
            myReq.CookieContainer = cookie;

            wr = (HttpWebResponse)myReq.GetResponse();
            Stream receiveStream = wr.GetResponseStream();
            StreamReader reader = new StreamReader(receiveStream, Encoding.UTF8);
            string content = reader.ReadToEnd();
            //myReq.Abort();
            reader.Close();
            reader.Dispose();
            receiveStream.Close();
            receiveStream.Dispose();
            wr.Close();
            wr.Dispose();

            HtmlDocument htmlDocument = new HtmlDocument();

            htmlDocument.LoadHtml(content);
            HtmlNode node = htmlDocument.DocumentNode.SelectSingleNode("//*[@id='builder-table-main']/tbody/tr/td[6]/div/a");
            //if (node != null)
            //    return node.InnerText;
            //else
            //    return "";
            return node == null ? "" : node.InnerText;
        }


        public static string getProperties<T>(T t)
        {
            string tStr = string.Empty;
            if (t == null)
            {
                return tStr;
            }
            System.Reflection.PropertyInfo[] properties = t.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

            if (properties.Length <= 0)
            {
                return tStr;
            }
            foreach (System.Reflection.PropertyInfo item in properties)
            {
                string name = item.Name;
                object value = item.GetValue(t, null);
                if (item.PropertyType.IsValueType || item.PropertyType.Name.StartsWith("String"))
                {
                    tStr += string.Format("{0}:{1},", name, value);
                }
                else
                {
                    getProperties(value);
                }
            }
            return tStr;
        }

        /// <summary>
        /// C#反射遍历对象属性
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="model">对象</param>
        public static void ForeachClassProperties<T>(T model)
        {
            Type t = model.GetType();
            PropertyInfo[] PropertyList = t.GetProperties();
            foreach (PropertyInfo item in PropertyList)
            {
                string name = item.Name;
                object value = item.GetValue(model, null);
            }
        }
    }

    public  class userInfo {
        /*
         用户ID 

账号状态 

渠道ID 

用户账号 

用户昵称 

用户备注 

用户支付宝 

用户银行卡 

注册时间 

最后登录时间 

今日总输赢 

历史总输赢 

总税收 

今日充值金额 

历史充值金额 

今日充值次数 

历史充值次数 

今日提现金额 

历史提现金额 

今日提现次数 

历史提现次数 

身上金币 

银行金币 

总金币 

玩家今日流水 

玩家今日游戏局数 

玩家总流水 

注册IP 

注册机器码 

最后登录IP 

最后机器码 

【赠送金币】总赠送次数 

【赠送金币】总赠送金额 

【赠送金币】总领取次数 

【赠送金币】总领取金额 

             */

        /// <summary>
        /// 用户ID 
        /// </summary>
        string id { get; set; }
        /// <summary>
        /// 账号状态 
        /// </summary>
        string status { get; set; }
        /// <summary>
        /// 渠道ID
        /// </summary>
        string channelid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        string channel { get; set; }
        /// <summary>
        /// 用户账号
        /// </summary>
        string username { get; set; }
        /// <summary>
        /// 用户昵称
        /// </summary>
        string nike { get; set; }
        /// <summary>
        /// 用户备注
        /// </summary>
        string memo { get; set; }
        /// <summary>
        /// 用户支付宝
        /// </summary>
        string alipay { get; set; }
        /// <summary>
        /// 用户银行卡 
        /// </summary>
        string bank { get; set; }
        /// <summary>
        /// 注册时间 
        /// </summary>
        string regtime { get; set; }
        /// <summary>
        /// 最后登录时间
        /// </summary>
        string lastlogintime { get; set; }
        /// <summary>
        /// 今日总输赢
        /// </summary>
        string daylibal { get; set; }
        /// <summary>
        /// 历史总输赢 
        /// </summary>
        string hisbal { get; set; }
        /// <summary>
        /// 总税收 
        /// </summary>
        string tax { get; set; }
        /// <summary>
        /// 今日充值金额 
        /// </summary>
        string daylicharge { get; set; }
        /// <summary>
        /// 历史充值金额 
        /// </summary>
        string hischarge { get; set; }
        /// <summary>
        /// 今日充值次数
        /// </summary>
        string daylichargetimes { get; set; }
        /// <summary>
        /// 历史充值次数
        /// </summary>
        string hischargetimes { get; set; }
        /// <summary>
        /// 今日提现金额 
        /// </summary>
        string daylicash { get; set; }
        /// <summary>
        /// 历史提现金额 
        /// </summary>
        string hiscash { get; set; }
        /// <summary>
        /// 今日提现次数
        /// </summary>
        string daylicashtimes { get; set; }
        /// <summary>
        /// 历史提现次数
        /// </summary>
        string hiscashtimes { get; set; }
        /// <summary>
        /// 身上金币 
        /// </summary>
        string money { get; set; }
        /// <summary>
        /// 银行金币 
        /// </summary>
        string bankmoney { get; set; }
        /// <summary>
        /// 总金币 
        /// </summary>
        string totalmoney { get; set; }
        /// <summary>
        /// 玩家今日流水 
        /// </summary>
        string daylirecorder { get; set; }
        /// <summary>
        /// 玩家今日游戏局数 
        /// </summary>
        string dayligameround { get; set; }
        /// <summary>
        /// 玩家总流水
        /// </summary>
        string totalrecorder { get; set; }
        /// <summary>
        /// 注册IP 
        /// </summary>
        string regip { get; set; }
        /// <summary>
        /// 注册机器码 
        /// </summary>
        string regmatinecode { get; set; }
        /// <summary>
        /// 最后登录IP 
        /// </summary>
        string latestip { get; set; }
        /// <summary>
        /// 最后机器码 
        /// </summary>
        string latestmachinecode { get; set; }
        /// <summary>
        /// 总赠送次数
        /// </summary>
        string zzscs { get; set; }
        /// <summary>
        /// 总赠送金额
        /// </summary>
        string zzsje { get; set; }
        /// <summary>
        /// 总领取次数
        /// </summary>
        string zlqcs { get; set; }
        /// <summary>
        /// 总领取金额
        /// </summary>
        string zlqje { get; set; }

    }
}
