using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Text;

namespace TimoControl
{
    public static class platAppSign
    {
        private static string acc { get; set; }
        private static string pwd { get; set; }
        private static string url_base { get; set; }
        private static CookieContainer cookie { get; set; }

        /// <summary>
        /// 登录
        /// </summary>
        /// <returns></returns>
        public static bool login()
        {

            try
            {
                string s = appSittingSet.readAppsettings("RedEnvelope");
                acc = s.Split('|')[0];
                pwd = s.Split('|')[1];
                url_base = s.Split('|')[2];
            }
            catch (Exception ex)
            {
                appSittingSet.txtLog("获取配置文件失败" + ex.Message);
                return false;
            }


            HttpWebRequest request = null;
            HttpWebResponse response = null;
            StreamReader reader = null;
            try
            {
                //ServicePointManager.DefaultConnectionLimit = 50;
                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;

                request = WebRequest.Create(url_base + "Public/login") as HttpWebRequest;

                request.ProtocolVersion = HttpVersion.Version11;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                request.Method = "POST";
                request.UserAgent = "Mozilla/4.0";
                request.ContentType = "application/x-www-form-urlencoded; charset=utf-8";
                string postdata = string.Format("user={0}&password={1}", acc, pwd);
                byte[] bytes = Encoding.UTF8.GetBytes(postdata);
                request.ContentLength = bytes.Length;
                Stream newStream = request.GetRequestStream();
                newStream.Write(bytes, 0, bytes.Length);
                newStream.Close();

                cookie = new CookieContainer();
                request.CookieContainer = cookie;

                response = (HttpWebResponse)request.GetResponse();
                reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);

                string ret_html = reader.ReadToEnd();
                cookie.Add(response.Cookies);
                reader.Close();
                reader.Dispose();
                response.Dispose();
                if (ret_html.Contains("登录成功") || ret_html.Contains("Index/index") || ret_html.Contains("管理员后台"))
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (WebException ex)
            {
                appSittingSet.txtLog(string.Format("红包站登录失败：{0}   ", ex.Message));
                return false;
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

        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        public static List<betData> getActData()
        {

            HttpWebRequest request = null;
            HttpWebResponse response = null;
            StreamReader reader = null;
            List<betData> list = new List<betData>();

            try
            {
                request = WebRequest.Create(url_base + "Records/index") as HttpWebRequest;

                request.ProtocolVersion = HttpVersion.Version11;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                request.Method = "POST";
                request.UserAgent = "Mozilla/5.0";
                request.ContentType = "application/x-www-form-urlencoded;";
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
                //request.Headers.Add("upgrade-insecure-requests", "1");
                //request.Headers.Add("X-Requested-With", "XMLHttpRequest");
                //request.Headers.Add("Origin", url_base.Replace("/index.php/admin/", ""));
                request.Referer = url_base + "records/index";
                request.CookieContainer = cookie;


                string postdata = "username=&level=&is_send=0&starttime=&endtime=";
                byte[] bytes = Encoding.UTF8.GetBytes(postdata);
                request.ContentLength = bytes.Length;
                Stream newStream = request.GetRequestStream();
                newStream.Write(bytes, 0, bytes.Length);
                newStream.Close();



                response = (HttpWebResponse)request.GetResponse();
                reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);

                string ret_html = reader.ReadToEnd();

                if (ret_html.Contains("130102031") || ret_html.Contains("后台登录"))
                {
                    //需要重新登录
                    login();
                    return null;
                }
                //HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
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
                    string bbid = node1.SelectSingleNode("/html/body/div[3]/div/table/tbody/tr[" + i + "]/td[1]/input").Attributes["value"].Value;
                    string username = node1.SelectSingleNode("/html/body/div[3]/div/table/tbody/tr[" + i + "]/td[3]").InnerText;
                    decimal betmony = 0;
                    decimal.TryParse(node1.SelectSingleNode("/html/body/div[3]/div/table/tbody/tr[" + i + "]/td[4]").InnerText, out betmony);
                    list.Add(new betData() { username = username, betMoney = betmony, bbid = bbid });
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

        /// <summary>
        /// 确认-单个
        /// </summary>
        /// <param name="bb"></param>
        /// <returns></returns>
        public static bool confirm(betData bb)
        {

            HttpWebRequest request = null;
            HttpWebResponse response = null;
            StreamReader reader = null;
            //List<betData> list = new List<betData>();

            try
            {
                request = WebRequest.Create(url_base + "Records/changeStatus") as HttpWebRequest;

                request.ProtocolVersion = HttpVersion.Version11;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                request.Method = "POST";
                request.UserAgent = "Mozilla/5.0";
                request.ContentType = "application/x-www-form-urlencoded;";
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
                //request.Headers.Add("upgrade-insecure-requests", "1");
                //request.Headers.Add("X-Requested-With", "XMLHttpRequest");
                //request.Headers.Add("Origin", url_base.Replace("/index.php/admin/", ""));
                request.Referer = url_base + "records/index";
                request.CookieContainer = cookie;

                string postdata = "id=" + bb.bbid;
                byte[] bytes = Encoding.UTF8.GetBytes(postdata);
                request.ContentLength = bytes.Length;
                Stream newStream = request.GetRequestStream();
                newStream.Write(bytes, 0, bytes.Length);
                newStream.Close();

                response = (HttpWebResponse)request.GetResponse();
                reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string ret_html = reader.ReadToEnd();

                JObject jo = JObject.Parse(ret_html);
                //if (jo["code"].ToString() == "130102031")
                //{
                //    login();
                //}

                if (jo["status"].ToString() == "1")
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (WebException ex)
            {
                string msg = string.Format("回填活动处理结果(异常)：用户 {0} 金额{1} {2} ", bb.username, bb.betMoney, ex.Message);
                appSittingSet.txtLog(msg);
                return false;
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
