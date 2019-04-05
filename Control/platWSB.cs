using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace TimoControl
{
    public static class platWSB
    {
        private static string urlbase { get; set; }
        private static string acc { get; set; }
        private static string pwd { get; set; }
        private static string uid { get; set; }
        private static CookieContainer cookie { get; set; }

        /// <summary>
        /// 登录
        /// </summary>
        /// <returns></returns>
        public static bool login()
        {
            try
            {
                string s1 = appSittingSet.readAppsettings("WSB");
                acc = s1.Split('|')[0];
                pwd = s1.Split('|')[1];
                urlbase = s1.Split('|')[2];
                uid = s1.Split('|')[3];

            }
            catch (Exception ex)
            {
                appSittingSet.Log("活动站获取配置文件失败" + ex.Message);
                return false;
            }


            try
            {

                HttpWebRequest request = WebRequest.Create(urlbase + "user/login.html") as HttpWebRequest;
                request.ProtocolVersion = HttpVersion.Version11;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                request.Method = "POST";
                request.UserAgent = "Mozilla/4.0";
                request.ContentType = "application/x-www-form-urlencoded; charset=utf-8";
                string postdata = string.Format("__token__={2}&user={0}&password={1}&google=", acc, pwd,uid);
                byte[] bytes = Encoding.UTF8.GetBytes(postdata);
                request.ContentLength = bytes.Length;
                Stream newStream = request.GetRequestStream();
                newStream.Write(bytes, 0, bytes.Length);
                newStream.Close();

                cookie = new CookieContainer();
                request.CookieContainer = cookie;

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);

                string ret_html = reader.ReadToEnd();

                cookie.Add(response.Cookies);
                reader.Close();
                reader.Dispose();
                response.Dispose();
                request.Abort();
                return ret_html.Contains("index.html");

                //<p class="jump">页面自动 < a id = "href" href = "/8943h4812iun4i32.php/Index/index.html" > 跳转 </ a > 等待时间： < b id = "wait" > 1 </ b ></ p >
            }
            catch (WebException ex)
            {
                appSittingSet.Log(string.Format("活动站登录失败：{0}   ", ex.Message));
                return false;
            }
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <returns></returns>
        public static List<betData> getData()
        {
            List<betData> list = new List<betData>();
            List<betData> list1 = getDataFromDB();
            List<betData> list2 = getDataFromWeb();
            if (list1.Count>0)
            {
                list.AddRange(list1);
            }
            if (list2.Count>0)
            {
                list.AddRange(list2);
            }
            return list;
        }

        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        public static List<betData> getDataFromWeb()
        {
            string url = urlbase + "unknoworder/showall.html?uporder=&s_realprice=&state=1&othername_text=&othername=&s_createtime=&s_paytime="; //未处理0 分页20条
            //url = urlbase+ "unknoworder/index.html";
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.ProtocolVersion = HttpVersion.Version11;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11;
            //request.Proxy = new WebProxy("url的ip地址", 80);
            request.Method = "GET";
            request.UserAgent = "Mozilla/4.0";
            request.KeepAlive = true;
            request.CookieContainer = cookie;

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
                    login();
                    return null;
                }
                //HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                var htmlDocument = new HtmlAgilityPack.HtmlDocument();
                htmlDocument.LoadHtml(ret_html);
                HtmlNode node1 = htmlDocument.DocumentNode.SelectSingleNode("//table//tbody");
                if (node1 == null)
                {
                    //appSittingSet.txtLog("没有获取到活动列表信息_____1111 " );
                    return list;
                }

                if (node1.ChildNodes.Count < 2)
                {
                    //appSittingSet.txtLog("没有获取到活动列表信息____2222");
                    return list;
                }
                HtmlNodeCollection collection = node1.SelectNodes("//tr");//跟Xpath一样，轻松的定位到相应节点下
                for (int i = 1; i < collection.Count-2; i++)
                {
                    string betno =collection[i].ChildNodes[1].InnerText;
                    string username =collection[i].ChildNodes[3].InnerText;
                    string money = collection[i].ChildNodes[5].InnerText;
                    decimal d;
                    decimal.TryParse(money, out d);
                    string links = "";
                    if (collection[i].ChildNodes.Count>13)
                    {
                        links = collection[i].ChildNodes[13].ChildNodes[1].Attributes["href"].Value;
                    }
                    list.Add(new betData() { username = username, links = links, betMoney = d , betno =betno,  aname="WSB" });
                }
                return list;
            }
            catch (WebException ex)
            {
                if (ex.HResult == -2146233079)
                {
                    // 远程服务器返回错误: (404) 未找到。
                }
                if (ex.Message == "操作超时")
                {
                    //需要重新登录
                    login();
                    return null;
                }
                appSittingSet.Log("获取活动列表失败：" + ex.Message);
                return null;
            }

        }

        /// <summary>
        /// 回填操作结果
        /// </summary>
        /// <param name="bb"></param>
        /// <returns></returns>
        public static bool confirm(betData bb)
        {
            string url = urlbase + bb.links.Replace("member.php/", "");
            try
            {
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.ProtocolVersion = HttpVersion.Version11;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                request.Method = "GET";
                request.UserAgent = "Mozilla/4.0";
                request.Accept = "*/*";
                request.Headers.Add("X-Requested-With", "XMLHttpRequest");
                request.CookieContainer = cookie;

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
                string msg = string.Format("回填处理结果(异常)：用户 {0}- {1} ", bb.username, ex.Message);
                appSittingSet.Log(msg);
                return false;
            }
        }



        /// <summary>
        /// 从数据库获取信息 
        /// 0代表没有修改过的数据
        /// 1代表客服修改过的数据
        /// </summary>
        /// <returns></returns>
        public static List<betData> getDataFromDB()
        {
            List<betData> list = new List<betData>();
            string sql = "SELECT [oid] ,[username] ,[deposit] ,[subtime] ,[state] FROM  [t_data] where [state]=1";
            DataTable dt = sqlHelper.ExecuteSelectDataTable(sql);

                foreach (DataRow item in dt.Rows)
                {
                    list.Add(new betData() { betno = item["oid"].ToString(), username = item["username"].ToString(), betMoney = (decimal)item["deposit"] , aname="DB"});
                }
            return list;
        }
    }
}
