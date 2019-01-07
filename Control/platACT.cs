using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace TimoControl
{
    public static class platACT
    {

        private static string url_act_base { get; set; }
        private static string act_acc { get; set; }
        private static string act_pwd { get; set; }
        private static string act_uid { get; set; }
        //private static int aid { get; set; }
        private static CookieContainer ct_yh { get; set; }
        //public static betData bb { get; set; }

        /// <summary>
        /// 登录优惠大厅并跳转
        /// </summary>
        /// <returns></returns>
        public static bool loginActivity()
        {
            try
            {
                string s1 = appSittingSet.readAppsettings("ACT");

                act_acc = s1.Split('|')[0];
                act_pwd = s1.Split('|')[1];
                url_act_base = s1.Split('|')[2];
                act_uid = s1.Split('|')[3];

                //aid = int.Parse(appSittingSet.readAppsettings("aid"));
            }
            catch (Exception ex)
            {
                appSittingSet.txtLog("活动站获取配置文件失败" + ex.Message);
                return false;
            }


            try
            {
                //ServicePointManager.DefaultConnectionLimit = 50;
                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;



                HttpWebRequest request = WebRequest.Create(url_act_base + "Public/login.html") as HttpWebRequest;

                request.ProtocolVersion = HttpVersion.Version11;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                request.Method = "POST";
                request.UserAgent = "Mozilla/4.0";
                request.ContentType = "application/x-www-form-urlencoded; charset=utf-8";
                string postdata = string.Format("user={0}&password={1}", act_acc, act_pwd);
                byte[] bytes = Encoding.UTF8.GetBytes(postdata);
                request.ContentLength = bytes.Length;
                Stream newStream = request.GetRequestStream();
                newStream.Write(bytes, 0, bytes.Length);
                newStream.Close();

                CookieContainer cookie = new CookieContainer();
                request.CookieContainer = cookie;

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);

                string ret_html = reader.ReadToEnd();
                cookie.Add(response.Cookies);
                ct_yh = cookie;
                reader.Close();
                reader.Dispose();
                response.Dispose();

                return ret_html.Contains("index.html");

                //<p class="jump">页面自动 < a id = "href" href = "/8943h4812iun4i32.php/Index/index.html" > 跳转 </ a > 等待时间： < b id = "wait" > 1 </ b ></ p >
            }
            catch (WebException ex)
            {
                appSittingSet. txtLog(string.Format("活动站登录失败：{0}   ", ex.Message));
                return false;
            }
        }

        /// <summary>
        /// 获取注单数据列表 尾数 消除奖
        /// </summary>
        /// <returns></returns>
        public static List<betData> getActData(string aid)
        {
            string url_act_list = url_act_base + "Submissions/index/aid/" + aid + ".html?status=0&p=1&start=&end=&username=&psize=20"; //未处理0 分页20条
            HttpWebRequest request = WebRequest.Create(url_act_list) as HttpWebRequest;
            request.ProtocolVersion = HttpVersion.Version11;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;//SecurityProtocolType.Tls1.2;
            //request.Proxy = new WebProxy("url的ip地址", 80);
            request.Method = "GET";
            request.UserAgent = "Mozilla/4.0";
            request.KeepAlive = true;
            request.CookieContainer = ct_yh;

            List<betData> list = new List<betData>();
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);

                string ret_html = reader.ReadToEnd();
                reader.Close();
                reader.Dispose();
                response.Close();
                response.Dispose();
                if (request != null)
                {
                    request.Abort();
                }
                if (ret_html.Contains("130102031"))
                {
                    //需要重新登录
                    loginActivity();
                    return null;
                }
                //HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                var htmlDocument = new HtmlAgilityPack.HtmlDocument();
                htmlDocument.LoadHtml(ret_html);
                HtmlNode node1 = htmlDocument.DocumentNode.SelectSingleNode("//table//tbody");
                if (node1 == null)
                {
                    //appSittingSet.txtLog("没有获取到活动列表信息");
                    return list;
                }

                if (node1.ChildNodes.Count < 2)
                {
                    //appSittingSet.txtLog("没有获取到活动列表信息");
                    return list;
                }
                HtmlNodeCollection collection = node1.SelectNodes("//tr[@class='gradeA']");//跟Xpath一样，轻松的定位到相应节点下

                foreach (HtmlNode node in collection)
                {
                    string bbid = "0";

                    //**************************只能用字符串处理    用其他方法 SelectSingleNode 均会出错  不知道是否 HtmlAgilityPack bug 原因*****************

                    int index1 = node.InnerHtml.IndexOf("sid=") + 5;
                    int index2 = node.InnerHtml.IndexOf("noTongguo") - 9;
                    bbid = node.InnerHtml.Substring(index1, index2 - index1);

                    //去除\r\n以及空格，获取到相应td里面的数据
                    string[] line = node.InnerText.Replace("\t", "").Split(new char[] { '\r', '\n', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    list.Add(new betData() { username = line[0], betno = line[1], bbid = bbid,aid= aid });

                    //if (line.Length == 11)
                    //{
                    //    list.Add(new betData() { betno = line[1], username = line[0], bbid = bbid });
                    //}
                    //else if (line.Length == 12)
                    //{
                    //    list.Add(new betData() { betno = line[2], username = line[0], bbid = bbid });
                    //}


                }

                return list;
            }
            catch (WebException ex)
            {
                if (ex.HResult == -2146233079 || ex.Message == "操作超时")
                {
                    //需要重新登录
                    loginActivity();
                    return null;
                }
                appSittingSet.txtLog("获取活动列表失败：" + ex.Message);
                return null;
            }

        }

        /// <summary>
        /// 回填活动结果
        /// </summary>
        /// <param name="bb"></param>
        /// <returns></returns>
        public static bool confirmAct(betData bb)
        {
            string url = url_act_base + "Submissions/save.html";
            try
            {
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.ProtocolVersion = HttpVersion.Version11;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                request.Method = "POST";
                request.UserAgent = "Mozilla/4.0";
                request.Accept = "*/*";
                request.Headers.Add("X-Requested-With", "XMLHttpRequest");
                request.CookieContainer = ct_yh;

                request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                //username 可以不要
                string postdata = string.Format("id={0}&uid={1}&username={2}&status={3}&message={4}", bb.bbid,act_uid,bb.username,bb.passed==true?1:2, HttpUtility.UrlEncode(bb.msg, Encoding.UTF8));
                byte[] bytes = Encoding.UTF8.GetBytes(postdata);
                request.ContentLength = bytes.Length;
                Stream newStream = request.GetRequestStream();
                newStream.Write(bytes, 0, bytes.Length);
                newStream.Close();

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                HttpStatusCode r = response.StatusCode;
                //return response.StatusCode == HttpStatusCode.OK ? true : false;
                response.Close();
                response.Dispose();
                if (request != null)
                {
                    request.Abort();
                }
                return r == HttpStatusCode.OK ? true : false;

            }
            catch (WebException ex)
            {
                string msg = string.Format("回填活动处理结果(异常)：用户 {0} 注单{1} {2} ", bb.username, bb.betno, ex.Message);
                //lvRecorder.Items.Insert(0, msg);
                appSittingSet.txtLog(msg);

                return false;
            }
        }

        /// <summary>
        /// 获取活动列表数据 以小博大
        /// </summary>
        /// <param name="aid"></param>
        /// <returns></returns>
        public static List<betData> getActData2(string aid)
        {
            string url_act_list = url_act_base + "Submissions/index/aid/" + aid + ".html?status=0&p=1&start=&end=&username=&psize=20"; //未处理0 分页20条
            HttpWebRequest request = WebRequest.Create(url_act_list) as HttpWebRequest;
            request.ProtocolVersion = HttpVersion.Version11;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            //ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            request.Method = "GET";
            request.UserAgent = "Mozilla/4.0";
            request.KeepAlive = true;
            request.CookieContainer = ct_yh;

            List<betData> list = new List<betData>();
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);

                string ret_html = reader.ReadToEnd();
                reader.Close();
                reader.Dispose();
                response.Close();
                response.Dispose();
                if (request != null)
                {
                    request.Abort();
                }
                if (ret_html.Contains("130102031") ||ret_html.Contains("登录" ))
                {
                    //需要重新登录
                    loginActivity();
                    return null;
                }
                //HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                var htmlDocument = new HtmlAgilityPack.HtmlDocument();
                htmlDocument.LoadHtml(ret_html);
                HtmlNode node1 = htmlDocument.DocumentNode.SelectSingleNode("//table//tbody");
                if (node1 == null)
                {
                    //appSittingSet.txtLog("没有获取到活动列表信息");
                    return list;
                }

                if (node1.ChildNodes.Count < 2)
                {
                    //appSittingSet.txtLog("没有获取到活动列表信息");
                    return list;
                }


                //HtmlNodeCollection node_th = htmlDocument.DocumentNode.SelectNodes("//table//thead//tr//th");
                //int index_username = 0;
                //int index_time = 0;
                //for (int i = 0; i < node_th.Count - 1; i++)
                //{
                //    if (node_th[i].InnerText.Contains("会员名称"))
                //    {
                //        index_username = i;
                //    }
                //    if (node_th[i].InnerText.Contains("申请时间"))
                //    {
                //        index_time = i;
                //    }
                //}

                //HtmlNodeCollection node_td = htmlDocument.DocumentNode.SelectNodes("//table//tbody//tr//td");
                //bb.lastOprTime = node_td[index2].InnerText;


                HtmlNodeCollection collection = node1.SelectNodes("//tr[@class='gradeA']");//跟Xpath一样，轻松的定位到相应节点下



                foreach (HtmlNode node in collection)
                {
                    string bbid = "0";

                    //**************************只能用字符串处理    用其他方法 SelectSingleNode 均会出错  不知道是否 HtmlAgilityPack bug 原因*****************
                    int index1 = node.InnerHtml.IndexOf("sid=") + 5;
                    int index2 = node.InnerHtml.IndexOf("noTongguo") - 9;
                    bbid = node.InnerHtml.Substring(index1, index2 - index1);
                    //string un = node.ChildNodes[1].InnerText.Trim();
                    //去除\r\n以及空格，获取到相应td里面的数据
                    string[] line = node.InnerText.Replace("\t", "").Split(new char[] { '\r', '\n', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    list.Add(new betData() { username = line[0], betTime = line[2].Replace("-", "/") + " " + line[3] + ":59", bbid = bbid, passed = true,aid=aid });//默认等于合格


                    //HtmlNodeCollection node_td = node.SelectNodes("//td");
                    //list.Add(new betData() { username = node_td[index_username].InnerText, betTime =node_td[index_time].InnerText, bbid = node.SelectSingleNode("//td//input").GetAttributeValue("Sid",""), passed=true });//默认等于合格

                }

                return list;
            }
            catch (WebException ex)
            {
                //if (ex.HResult== -2146233079 || ex.Message== "操作超时")
                //{
                //    //需要重新登录
                //    loginActivity();
                //    return null;
                //}
                appSittingSet.txtLog("获取活动列表失败：" + ex.Message);
                return null;
            }

        }

        /// <summary>
        /// 以小博大 获取上一次设置的时间
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="bb"></param>
        /// <returns></returns>
        public static betData getActData2_time(string aid,betData bb)
        {
      //https://3730yh.com/8943h4812iun4i32.php/Submissions/index/aid/38.html?status=1&p=1&start=&end=&username=rr435800&psize=1
            string url_act_list = url_act_base + "Submissions/index/aid/" + aid + ".html?status=1&p=1&start=&end=&username="+bb.username+"&psize=1"; //未处理0 分页1条
            HttpWebRequest request = WebRequest.Create(url_act_list) as HttpWebRequest;
            request.ProtocolVersion = HttpVersion.Version11;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            request.Method = "GET";
            request.UserAgent = "Mozilla/4.0";
            request.KeepAlive = true;
            request.CookieContainer = ct_yh;

            List<betData> list = new List<betData>();
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);

                string ret_html = reader.ReadToEnd();
                reader.Close();
                reader.Dispose();
                response.Close();
                response.Dispose();
                if (request != null)
                {
                    request.Abort();
                }
                if (ret_html.Contains("130102031"))
                {
                    //需要重新登录
                    loginActivity();
                    return null;
                }
                //HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                var htmlDocument = new HtmlAgilityPack.HtmlDocument();
                htmlDocument.LoadHtml(ret_html);
                HtmlNodeCollection node_th =  htmlDocument.DocumentNode.SelectNodes("//table//thead//tr//th");
                HtmlNode node1 = htmlDocument.DocumentNode.SelectSingleNode("//table//tbody");
                if (node1 == null)
                {
                    //bb.lastOprTime = "1900/01/01 00:00:00";
                    bb.lastOprTime = "";
                    return bb;
                }
                if (node1.ChildNodes.Count < 2)
                {
                    //bb.lastOprTime = "1900/01/01 00:00:00";
                    bb.lastOprTime = "";
                    return bb;
                }

                int index1 = 0;
                int index2 = 0;
                for (int i = 0; i <node_th.Count-1; i++)
                {
                    if (node_th[i].InnerText.Contains("会员名称"))
                    {
                        index1 = i;
                    }
                    if (node_th[i].InnerText.Contains("申请时间"))
                    {
                        index2 = i;
                    }
                }

                HtmlNodeCollection node_td =  htmlDocument.DocumentNode.SelectNodes("//table//tbody//tr//td");
                bb.lastOprTime = node_td[index2].InnerText + ":59";//补足59秒

                //去除\r\n以及空格，获取到相应td里面的数据
                //string[] line = node1.SelectSingleNode("//tr[@class='gradeA']").InnerText.Replace("\t", "").Split(new char[] { '\r', '\n', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                //bb.lastOprTime = line[3].Replace("-", "/") +" "+ line[4]+ ":00";
                return bb;

            }
            catch (WebException ex)
            {
                //if (ex.HResult== -2146233079 || ex.Message== "操作超时")
                //{
                //    //需要重新登录
                //    loginActivity();
                //    return null;
                //}
                appSittingSet.txtLog("获取活动列表失败：" + ex.Message);
                return null;
            }

        }

        public static betData getActData2_topone(string aid)
        {
            string url_act_list = url_act_base + "Submissions/index/aid/" + aid + ".html?status=0&p=1&start=&end=&username=&psize=20"; //未处理0 分页20条
            HttpWebRequest request = WebRequest.Create(url_act_list) as HttpWebRequest;
            request.ProtocolVersion = HttpVersion.Version11;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            request.Method = "GET";
            request.UserAgent = "Mozilla/4.0";
            request.KeepAlive = true;
            request.CookieContainer = ct_yh;

            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);

                string ret_html = reader.ReadToEnd();
                reader.Close();
                reader.Dispose();
                response.Close();
                response.Dispose();

                if (ret_html.Contains("130102031") ||ret_html.Contains("登录" ))
                {
                    //需要重新登录
                    loginActivity();
                    return null;
                }
                //HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                var htmlDocument = new HtmlAgilityPack.HtmlDocument();
                htmlDocument.LoadHtml(ret_html);
                HtmlNode node1 = htmlDocument.DocumentNode.SelectSingleNode("//table//tbody");
                if (node1 == null)
                {
                    //appSittingSet.txtLog("没有获取到活动列表信息");
                    return null;
                }

                if (node1.ChildNodes.Count < 2)
                {
                    //appSittingSet.txtLog("没有获取到活动列表信息");
                    return null;
                }
                HtmlNodeCollection collection = node1.SelectNodes("//tr[@class='gradeA']");//跟Xpath一样，轻松的定位到相应节点下

                HtmlNode node_top = collection[collection.Count - 1];
                string bbid = "0";

                //**************************只能用字符串处理    用其他方法 SelectSingleNode 均会出错  不知道是否 HtmlAgilityPack bug 原因*****************
                int index1 = node_top.InnerHtml.IndexOf("sid=") + 5;
                int index2 = node_top.InnerHtml.IndexOf("noTongguo") - 9;
                bbid = node_top.InnerHtml.Substring(index1, index2 - index1);

                //去除\r\n以及空格，获取到相应td里面的数据
                string[] line = node_top.InnerText.Replace("\t", "").Split(new char[] { '\r', '\n', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                return new betData() { username = line[0], betTime = line[3].Replace("-", "/") + " " + line[4] + ":59", bbid = bbid, passed = true };
            }
            catch (WebException ex)
            {
                //if (ex.HResult== -2146233079 || ex.Message== "操作超时")
                //{
                //    //需要重新登录
                //    loginActivity();
                //    return null;
                //}
                appSittingSet.txtLog("获取活动列表失败：" + ex.Message);
                return null;
            }

        }

        /// <summary>
        /// 获取活动列表数据 以小博大
        /// </summary>
        /// <param name="aid"></param>
        /// <returns></returns>
        public static List<betData> getActData_First_Deposit(int aid)
        {
            HttpWebRequest request = null;
            HttpWebResponse response = null;
            StreamReader reader = null;
            List<betData> list = new List<betData>();
            try
            {
                string url_act_list = url_act_base + "Submissions/index/aid/" + aid + ".html?status=0&p=1&start=&end=&username=&psize=20"; //未处理0 分页20条
                request = WebRequest.Create(url_act_list) as HttpWebRequest;
                request.ProtocolVersion = HttpVersion.Version11;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                request.Method = "GET";
                request.UserAgent = "Mozilla/4.0";
                request.KeepAlive = true;
                request.CookieContainer = ct_yh;

                response = (HttpWebResponse)request.GetResponse();
                reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string ret_html = reader.ReadToEnd();
                if (ret_html.Contains("130102031") || ret_html.Contains("登录"))
                {
                    //需要重新登录
                    loginActivity();
                    return null;
                }

                HtmlDocument htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(ret_html);
                HtmlNode node1 = htmlDocument.DocumentNode.SelectSingleNode("//table//tbody");
                if (node1 == null)
                {
                    //appSittingSet.txtLog("没有获取到活动列表信息");
                    return list;
                }

                if (node1.ChildNodes.Count < 2)
                {
                    //appSittingSet.txtLog("没有获取到活动列表信息");
                    return list;
                }

                for (int i = 1; i <= node1.SelectNodes("//tbody//tr").Count; i++)
                {
                    //*[@id="rightSide"]/div[3]/div[2]/table/tbody/tr[1]/td[1]
                    //*[@id="rightSide"]/div[3]/div[2]/table/tbody/tr[2]/td[1]
                    string bbid = node1.SelectSingleNode("/html/body/div[3]/div[2]/table/tbody/tr["+i+"]/td[5]/input").Attributes["sid"].Value;
                    string username = node1.SelectSingleNode("/html/body/div[3]/div[2]/table/tbody/tr["+i+"]/td[1]").InnerText;

                    list.Add(new betData() { username = username,bbid = bbid });
                }

                return list;
            }
            catch (WebException ex)
            {
                //if (ex.HResult== -2146233079 || ex.Message== "操作超时")
                //{
                //    //需要重新登录
                //    loginActivity();
                //    return null;
                //}
                appSittingSet.txtLog("获取活动列表失败：" + ex.Message);
                return null;
            }
            finally
            {
                if (request != null)
                {
                    request.Abort();
                }
                if (response != null)
                {
                    response.Close();
                    response.Dispose();
                }
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }
    }
}
