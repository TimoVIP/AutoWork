using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using TimoControl;

namespace QappleLoad
{
    public static class webHelper
    {

        private static string urlbase { get; set; }
        private static string acc { get; set; }
        private static string pwd { get; set; }
        private static string uid { get; set; }
        private static CookieContainer cookie { get; set; }

        /// <summary>
        /// 登录优惠大厅
        /// </summary>
        /// <returns></returns>
        public static bool login()
        {
            try
            {
                string s1 = appSittingSet.readAppsettings("ACT");
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
                string url = urlbase + $"subAccount/loginOfSubAccount?subAccount={acc}&password={pwd}";
                url = "https://apod.nasa.gov/apod/";
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.ProtocolVersion = HttpVersion.Version10;
                //ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                request.Method = "GET";
                request.UserAgent = "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; Trident/4.0; QQWubi 133; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; CIBA; InfoPath.2)";
                request.ContentType = "text/html; charset=ISO-8859-1";
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";

                //request.ContentType = "application/json;charset=utf-8";
                //request.Accept = "application/json";

                //string postdata = string.Format("user={0}&password={1}", acc, pwd);
                //byte[] bytes = Encoding.UTF8.GetBytes(postdata);
                //request.ContentLength = bytes.Length;
                //Stream newStream = request.GetRequestStream();
                //newStream.Write(bytes, 0, bytes.Length);
                //newStream.Close();

                cookie = new CookieContainer();
                cookie.Add(new Cookie("sidebarStatus", "0", "/", "api.qapple.io"));
                request.CookieContainer = cookie;


                //X509Certificate2 certificate = new X509Certificate2(Properties.Resources.client, Properties.Resources.httpsKey8843);
                //request.ClientCertificates.Add(certificate);

                //X509Certificate2 certificate = new X509Certificate2(Properties.Resources.cert_ie); 
                //request.ClientCertificates.Add(certificate);


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
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        public static List<betData> getActData()
        {
            string url = urlbase + "submissions/index.html?status=0&p=1&start=&end=&username=&actual=&ip="; //未处理0 分页20条
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
                HtmlNodeCollection collection = node1.SelectNodes("//tr[@class='gradeA']");//跟Xpath一样，轻松的定位到相应节点下

                for (int i = 1; i <= node1.SelectNodes("//tbody//tr").Count; i++)
                {

                    //*[@id="rightSide"]/div[3]/div[2]/table/tbody/tr[1]/td[1]
                    //*[@id="rightSide"]/div[3]/div[2]/table/tbody/tr[1]/td[7]/input
                    //*[@id="rightSide"]/div[3]/div[2]/table/tbody/tr[1]/td[2]
                    string bbid = node1.SelectSingleNode("//tbody//tr[" + i + "]/td[7]/input").Attributes["sid"].Value;
                    string username = node1.SelectSingleNode("//tbody//tr[" + i + "]/td[1]").InnerText;
                    string mobile = node1.SelectSingleNode("//tbody//tr[" + i + "]/td[2]").InnerText;
                    list.Add(new betData() { username = username, bbid = bbid, PortalMemo = mobile });
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
        public static bool confirmAct(betData bb)
        {
            string url = urlbase + "Submissions/save.html";
            try
            {
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.ProtocolVersion = HttpVersion.Version11;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                request.Method = "POST";
                request.UserAgent = "Mozilla/4.0";
                request.Accept = "*/*";
                request.Headers.Add("X-Requested-With", "XMLHttpRequest");
                request.CookieContainer = cookie;

                request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                //username 可以不要
                string postdata = string.Format("id={0}&uid={1}&username={2}&status={3}&message={4}&handletime={5}", bb.bbid, uid, bb.username, bb.passed == true ? 1 : 2, HttpUtility.UrlEncode(bb.msg, Encoding.UTF8), (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000);
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
                appSittingSet.Log(msg);
                return false;
            }
        }
    }
}
