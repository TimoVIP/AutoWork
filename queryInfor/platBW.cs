using BaseFun;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using HtmlAgilityPack;
using System.Data;

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

        private static CookieContainer cookie { get; set; } = new CookieContainer();
        private static string token { get; set; }
        private static IWebDriver selenium { get; set; }
        private static string Authorization { get; set; }

        private static HttpWebRequest myReq = null;
        private static HttpWebResponse wr = null;
        private static CredentialCache mycache = new CredentialCache();
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
        public static bool login()
        {
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

                cookie.Add(new System.Net.Cookie("PHPSESSID", "547df0afd6f63b8baa88358d107b89fb", "/", "bkk.e9xz3mq8.xyz"));
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
                    myReq.ContentType = "application/x-www-form-urlencoded";

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
                        return false;
                    }
                    //myReq = (HttpWebRequest)WebRequest.Create($"{urlbase}admin.php/gameplayer/index/info/user_id/2800776.html");
                    myReq = (HttpWebRequest)WebRequest.Create($"{urlbase}admin.php/admin/index/index.html");
                    myReq.Method = "GET";
                    myReq.Credentials = mycache;
                    myReq.Headers.Add("Authorization", Authorization);
                    myReq.Headers.Add("Upgrade-Insecure-Requests", "1");
                    myReq.CookieContainer = cookie;

                    wr = (HttpWebResponse)myReq.GetResponse();
                    receiveStream = wr.GetResponseStream();
                    reader = new StreamReader(receiveStream, Encoding.UTF8);
                    content = reader.ReadToEnd();

                    if (content.Contains("欢迎登陆后台") && content.Contains("退出帐号"))
                        return true;
                    else
                        return false;

                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
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
            if (node2==null)
            {
                return htmlDocument.DocumentNode.SelectSingleNode("/html/body/div[1]/div/div/h1").InnerText;
            }
            string display = "";
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
            /*
            if (node1 != null)
            {
                content = "";
                node1 = node1.SelectSingleNode("//*[@id='builder-table-main']//tbody");

                foreach (HtmlNode row in node1.SelectNodes("//tr"))
                {
                    if (row.ChildNodes[1].Name.ToLower() == "th")
                    {
                        continue;
                    }
                    content += $"{row.ChildNodes[1].ChildNodes[1].InnerText.Replace("\r\n", "").Replace(" ", "").Trim()}:{row.ChildNodes[3].ChildNodes[1].InnerText.Replace("\r\n", "").Replace(" ", "").Trim()}\r\n";
                    //content += $"{row.InnerText.Replace("\r\n","").Replace(" ","").Trim()}\r\n";
                    //content += row.SelectNodes("//td")[0].InnerText.Replace("\r\n", "").Trim() + ":" + row.SelectNodes("//td")[1].InnerText.Replace("\r\n", "").Trim() + "/r/n";
                }
                //return node1.OuterHtml;
                return content;
            }
            else
                return htmlDocument.DocumentNode.InnerText.Replace(" ","");
                */
            return display;
        }


        /// <summary>
        /// 查询赠送金币记录
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <param name="page">第几页</param>
        /// <param name="pagesize">页大小</param>
        /// <returns></returns>
        public static DataTable getJBZS(string id,DateTime d1, DateTime d2,int page=1,int pagesize=100)
        {
            string url = $"{urlbase}admin.php/user/log/index.html?_s=type%3D4%7Coper_id%3D%7Cuser_id%3D{id}%7Ccreate_time%3D{d1.ToString("yyyy-MM-dd")}+-+{d2.ToString("yyyy-MM-dd")}%7Cis_export%3D&_o=type%3Deq%7Coper_id%3Deq%7Cuser_id%3Deq%7Ccreate_time%3Dbetween+time%7Cis_export%3Deq&page={page}&list_rows={pagesize}";
            //string url = $"{urlbase}admin.php/gameplayer/index/cunk/group/tab3/user_id/{id}.html?_s=roomid=|logtype=8|create_time=|is_history=&_o=roomid=eq|logtype=eq|create_time=between%20time|is_history=eq&page={page}&list_rows={pagesize}";
            //string url = $"{urlbase}admin.php/user/log/index.html?_s=type=4|oper_id=|user_id={id}|create_time={d1.ToString("yyyy-MM-dd")} - {d2.ToString("yyyy-MM-dd")}|is_export=_o: type =eq|oper_id=eq|user_id=eq|create_time=between time|is_export=eq&page={page}&list_rows={pagesize}";
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
        /// 查询用户兑换记录
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <param name="page">第几页</param>
        /// <param name="pagesize">页大小</param>
        /// <returns></returns>
        public static DataTable getYHDH(string id,DateTime d1, DateTime d2,int page=1,int pagesize=100)
        {
            string url = $"{urlbase}/admin.php/gameplayer/gameuser/exchangerecord.html?_s=search_month=|date_at={d1.ToString("yyyy-MM-dd")}%20-%20{d2.ToString("yyyy-MM-dd")}|check_time=|status=|ex_type_id=|is_agent_pay=|ex_channel=|user_type=|auto_handle=|special_status=|UserID={id}|OrderID=|OrderNo=|receive_id=|is_export=&_o=search_month=eq|date_at=between%20time|check_time=between%20time|status=eq|ex_type_id=eq|is_agent_pay=eq|ex_channel=eq|user_type=eq|auto_handle=eq|special_status=eq|UserID=eq|OrderID=eq|OrderNo=eq|receive_id=eq|is_export=eq&page={page}&list_rows={pagesize}";
            //string url = $"{urlbase}admin.php/gameplayer/index/exchange/group/tab9/user_id/{id}.html?_s=create_time%3D%7Ccheck_time%3D%7Cexchange_table%3D%7Cstatus%3D%7Cexchannel%3D%7COrderID%3D%7COrderNo%3D&_o=create_time%3Dbetween+time%7Ccheck_time%3Dbetween+time%7Cexchange_table%3Deq%7Cstatus%3Deq%7Cexchannel%3Deq%7COrderID%3Deq%7COrderNo%3Deq&group=tab9&user_id={id}&page={page}&list_rows={pagesize}";
            //string url = $"{urlbase}admin.php/gameplayer/index/exchange/group/tab9/user_id/2041149.html?_s=create_time=|check_time=|exchange_table=|status=|exchannel=|OrderID=|OrderNo=&_o=create_time=between%20time|check_time=between%20time|exchange_table=eq|status=eq|exchannel=eq|OrderID=eq|OrderNo=eq";
            //string url = $"{urlbase}admin.php/gameplayer/gameuser/exchangerecord.html?_s=search_month=|date_at={d1.ToString("yyyy-MM-dd")}%20-%20{d2.ToString("yyyy-MM-dd")}|check_time=|status=1|ex_type_id=|is_agent_pay=|ex_channel=|user_type=|auto_handle=|special_status=|UserID=2550179|OrderID=|OrderNo=|receive_id=|is_export=&_o=search_month=eq|date_at=between%20time|check_time=between%20time|status=eq|ex_type_id=eq|is_agent_pay=eq|ex_channel=eq|user_type=eq|auto_handle=eq|special_status=eq|UserID=eq|OrderID=eq|OrderNo=eq|receive_id=eq|is_export=eq";
         /*测试*///url = $"{urlbase}admin.php/gameplayer/index/cunk/group/tab3/user_id/{id}.html?_s=roomid=|logtype=1|create_time=|is_history=&_o=roomid=eq|logtype=eq|create_time=between%20time|is_history=eq&page={page}&list_rows={pagesize}";
            //url = $"{urlbase}/admin.php/gameplayer/index/cunk/group/tab3/user_id/{id}.html?_s=roomid%3D%7Clogtype%3D1%7Ccreate_time%3D%7Cis_history%3D&_o=roomid%3Deq%7Clogtype%3Deq%7Ccreate_time%3Dbetween+time%7Cis_history%3Deq&group=tab3&user_id=2464475&page=1&list_rows=1000";
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
        /// 查询用户充值记录
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <param name="page">第几页</param>
        /// <param name="pagesize">页大小</param>
        /// <returns></returns>
        public static DataTable getYHCZ(string id,DateTime d1, DateTime d2,out string s ,int page=1,int pagesize=100)
        {
            string url = $"{urlbase}admin.php/gameplayer/gameuser/rechargerecord.html?_s=date_at={d1.ToString("yyyy-MM-dd")}%20-%20{d2.ToString("yyyy-MM-dd")}|create_ymd=|channel_id=|order_status=|user_status=|plat_form=|merchant_id=|recharge_channel=|pay_time=|UserID={id}|OrderID=|OrderNo=|is_export=&_o=date_at=between%20time|create_ymd=eq|channel_id=eq|order_status=eq|user_status=eq|plat_form=eq|merchant_id=eq|recharge_channel=eq|pay_time=between%20time|UserID=eq|OrderID=eq|OrderNo=eq|is_export=eq&page={page}&list_rows={pagesize}";
            //string url = $"{urlbase}admin.php/gameplayer/index/recharge/group/tab4/user_id/{id}.html?_s=create_time={d1.ToString("yyyy-MM-dd")}%20-%20{d2.ToString("yyyy-MM-dd")}|recharge_table=|order_status=1&_o=create_time=between%20time|recharge_table=eq|order_status=eq&page={page}&list_rows={pagesize}";
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
            return dt;
        }
    }
}
