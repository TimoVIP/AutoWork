using BaseFun;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace TimoControl
{
    public static class ActDataFromWeb
    {

        private static string url_base { get; set; }
        private static string acc { get; set; }
        private static string pwd { get; set; }
        private static string uid { get; set; }
        //private static int aid { get; set; }
        private static CookieContainer cookie { get; set; } = new CookieContainer();
        //public static betData bb { get; set; }

        private static T getHtml<T>(string postUrl, string postData = "")
        {
            HttpWebRequest request = null;
            HttpWebResponse response = null;
            StreamReader reader = null;
            try
            {
                request = WebRequest.Create(postUrl) as HttpWebRequest;

                request.ProtocolVersion = HttpVersion.Version11;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                request.Method = "POST";
                request.UserAgent = "Mozilla/4.0";
                request.ContentType = "application/x-www-form-urlencoded; charset=utf-8";
                request.Accept = "*/*";
                request.Headers.Add("X-Requested-With", "XMLHttpRequest");

                request.CookieContainer = cookie;

                if (postData != "")
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(postData);
                    request.ContentLength = bytes.Length;
                    Stream newStream = request.GetRequestStream();
                    newStream.Write(bytes, 0, bytes.Length);
                    newStream.Close();
                }



                response = (HttpWebResponse)request.GetResponse();
                cookie.Add(response.Cookies);
                reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                object ret_html = reader.ReadToEnd();

                if (ret_html.ToString().Contains("130102031"))
                {
                    //需要重新登录
                    login();
                    return default(T);
                }

                if (typeof(T).Name == "HttpStatusCode")
                {
                    HttpStatusCode statusCode = response.StatusCode;
                    return (T)(object)statusCode;
                }
                else if (typeof(T).Name == "JObject")
                {
                    JsonSerializerSettings js = new JsonSerializerSettings();
                    js.DateParseHandling = DateParseHandling.DateTime;
                    js.DateTimeZoneHandling = DateTimeZoneHandling.Local;
                    JObject jo = (JObject)JsonConvert.DeserializeObject(ret_html.ToString(), js);
                    return (T)(object)jo;
                }
                else if (typeof(T).Name == "JArray")
                {
                    JsonSerializerSettings js = new JsonSerializerSettings();
                    js.DateParseHandling = DateParseHandling.DateTime;
                    js.DateTimeZoneHandling = DateTimeZoneHandling.Local;
                    JArray ja = JArray.FromObject(JsonConvert.DeserializeObject(ret_html.ToString(), js));
                    return (T)(object)ja;
                }
                else
                {
                    return (T)ret_html;
                }
            }
            catch (WebException ex)
            {
                appSittingSet.Log($"活动页错误信息{postUrl}-{ex.Message}");
                return default(T);
            }
            finally
            {
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
                if (request != null)
                {
                    request.Abort();
                }
            }
        }
        /// <summary>
        /// 登录优惠大厅并跳转
        /// </summary>
        /// <returns></returns>
        public static bool login()
        {
            try
            {
                string s1 = appSittingSet.readAppsettings("ACT");

                acc = s1.Split('|')[0];
                pwd = s1.Split('|')[1];
                url_base = s1.Split('|')[2];
                uid = s1.Split('|')[3];

            }
            catch (Exception ex)
            {
                appSittingSet.Log("活动站获取配置文件失败" + ex.Message);
                return false;
            }

            string postUrl = url_base + "Public/login.html";
            string postData = $"user={acc}&password={pwd}";
            string ret_html = getHtml<string>(postUrl, postData);

            return ret_html.Contains("index.html");
        }

        /// <summary>
        /// 获取注单数据列表 尾数 消除奖
        /// 第一列 用户名 第二列 手机号/注单号 第四列时间
        /// </summary>
        /// <returns></returns>
        public static List<betData> getData(string aid)
        {


            string postUrl = url_base + "Submissions/index/aid/" + aid + ".html?status=0&p=1&start=&end=&username=&psize=20"; //未处理0 分页20条
            string ret_html = getHtml<string>(postUrl);
            List<betData> list = new List<betData>();
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
            HtmlNodeCollection collection = node1.SelectNodes("//tr[@class='gradeA']");//跟Xpath一样，轻松的定位到相应节点下

            //方法需要调试看哪里有误 幸运尾数 活动错误
            //for (int i = 1; i <= node1.SelectNodes("//tbody//tr").Count; i++)
            //{
            //    string username = node1.SelectSingleNode("//tbody//tr[" + i + "]/td[1]").InnerText.Trim();
            //    string mobile = node1.SelectSingleNode("//tbody//tr[" + i + "]/td[2]").InnerText.Trim();
            //    string bbid = node1.SelectSingleNode("//tbody//tr[" + i + "]/td[6]/input").Attributes["sid"].Value;
            //    list.Add(new betData() { username = username, bbid = bbid, PortalMemo = mobile, betno = mobile, aid = aid });
            //}


            foreach (HtmlNode node in collection)
            {
                string bbid = "0";
                int index1 = node.InnerHtml.IndexOf("sid=") + 5;
                int index2 = node.InnerHtml.IndexOf("noTongguo") - 9;
                bbid = node.InnerHtml.Substring(index1, index2 - index1);
                string[] line = node.InnerText.Replace("\t", "").Split(new char[] { '\r', '\n', ' ' }, StringSplitOptions.RemoveEmptyEntries);//去除\r\n以及空格，获取到相应td里面的数据

                if (node.ChildNodes.Count == 13)
                {
                    list.Add(new betData() { username = line[0], betno = line[1], bbid = bbid, aid = aid, PortalMemo = line[1] });
                }
                else if (node.ChildNodes.Count == 11)
                {
                    list.Add(new betData() { username = line[0], betTime = line[2].Replace("-", "/") + " " + line[3] + ":00", bbid = bbid, passed = true, aid = aid });//默认等于合格
                }


                //foreach (HtmlNode item in node.ChildNodes)
                //{
                //    if (item.Name== "td")
                //    {

                //    }
                //}

                //**************************只能用字符串处理    用其他方法 SelectSingleNode 均会出错  不知道是否 HtmlAgilityPack bug 原因*****************

            }


            return list;


        }

        /// <summary>
        /// 回填活动结果
        /// </summary>
        /// <param name="bb"></param>
        /// <returns></returns>
        public static bool confirm(betData bb)
        {
            string postUrl = url_base + "Submissions/save.html";
            string postData = $"id={ bb.bbid}&uid={uid}&username={bb.username}&status={(bb.passed == true ? 1 : 2)}&message={HttpUtility.UrlEncode(bb.msg, Encoding.UTF8)}";
            HttpStatusCode r = getHtml< HttpStatusCode>(postUrl, postData);
            bool br = r == HttpStatusCode.OK;
            if (br)
            {
                //记录到sqlite数据库
                //appSittingSet.recorderDb(bb);
                string sql = $"insert  or ignore into record (username, gamename,betno,chargeMoney,pass,msg,subtime,aid,bbid) values ('{ bb.username}', '{ bb.gamename}','{bb.betno }',{ bb.betMoney },{(bb.passed == true ? 1 : 0) },'{ bb.msg }','{DateTime.Now.AddHours(-12).ToString("yyyy-MM-dd HH:mm:ss") }' , {bb.aid},{bb.bbid})";
                SQLiteHelper.SQLiteHelper.execSql(sql);
                string msg = $"用户{bb.username}处理完毕，处理为 {(bb.passed ? "通过" : "不通过")}，回复消息 {bb.msg}";
                appSittingSet.Log(msg);
            }
            else
            {
                string msg = $"用户{bb.username}处理完毕，处理为 {(bb.passed ? "通过" : "不通过")}，回复消息 {bb.msg} 处理失败";
                appSittingSet.Log(msg);
            }
            return br;
        }

        /// <summary>
        /// 以小博大 获取上一次设置的时间
        /// </summary>
        /// <param name="bb"></param>
        /// <returns></returns>
        public static betData getData2_time(betData bb)
        {
            string postUrl = $"{url_base}Submissions/index/aid/{bb.aid}.html?status=1&p=1&start=&end=&username={bb.username}&psize=1";//未处理0 分页1条
            string ret_html = getHtml<string>(postUrl);
            List<betData> list = new List<betData>();


            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(ret_html);
            HtmlNodeCollection node_th = htmlDocument.DocumentNode.SelectNodes("//table//thead//tr//th");
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
            for (int i = 0; i < node_th.Count - 1; i++)
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

            HtmlNodeCollection node_td = htmlDocument.DocumentNode.SelectNodes("//table//tbody//tr//td");
            bb.lastOprTime = node_td[index2].InnerText + ":00";//补足59秒

            //去除\r\n以及空格，获取到相应td里面的数据
            //string[] line = node1.SelectSingleNode("//tr[@class='gradeA']").InnerText.Replace("\t", "").Split(new char[] { '\r', '\n', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            //bb.lastOprTime = line[3].Replace("-", "/") +" "+ line[4]+ ":00";
            return bb;
        }
    }
}
