using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using TimoControl;
namespace GAC_load
{
    public static class platACT2
    {

        private static string urlbase { get; set; }
        private static string acc { get; set; }
        private static string pwd { get; set; }
        private static string uid { get; set; }
        private static CookieContainer cookie { get; set; }

        private static string subAccount { get; set; }
        private static string token { get; set; }
        private static string transNo { get; set; }

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
                pwd = MD5Encrypt(pwd);//加密
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                string url = urlbase + $"subAccount/loginOfSubAccount?subAccount={acc}&password={pwd}";
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "POST";
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

                JObject jo = (JObject)JsonConvert.DeserializeObject(ret_html.ToString());
                if (jo["message"].ToString() == "操作成功")
                {
                    subAccount = jo["data"]["subAccount"].ToString();
                    token = jo["data"]["token"].ToString();
                    transNo = jo["transNo"].ToString();
                    return true;
                }
                else
                    return false;

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
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        public static List<betData> getActData()
        {

            List<betData> list = new List<betData>();

            try
            {

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                string url = urlbase + $"oc/qryMerchantOrderByCondition?tradeId=&timeStart={DateTime.Now.AddDays(-10).Date.ToString("yyyy-MM-dd")}+00:00:00&timeEnd={DateTime.Now.Date.ToString("yyyy-MM-dd")}+23:59:59&payStatus=PAYED&synStatus=NOT_DEAL&fastSubName=&currentPage=1&pageSize=10";
               //测试 状态为 已处理 的数据
                 //url= urlbase + $"oc/qryMerchantOrderByCondition?tradeId=&timeStart={DateTime.Now.AddDays(-10).Date.ToString("yyyy-MM-dd")}+00:00:00&timeEnd={DateTime.Now.Date.ToString("yyyy-MM-dd")}+23:59:59&payStatus=PAYED&synStatus=NORMAL_DEAL&fastSubName=&currentPage=1&pageSize=10";

                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "GET";
                request.Headers.Add("logntoken", token);
                request.Headers.Add("username", subAccount);
                request.Headers.Add("userrole", "merchant");
                request.Headers.Add("busitype", "merchant");
                request.Headers.Add("clitype", "PC");
                request.Headers.Add("cliv", "1.0.0");

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
                if (jo["message"].ToString() == "操作成功")
                {
                    //subAccount = jo["data"]["subAccount"].ToString();
                    //token = jo["data"]["token"].ToString();
                    //transNo = jo["transNo"].ToString();

                    //会员命不存在的 这个里面可能有 "synDesc":"会员名 w83801326"
                    JArray ja = JArray.FromObject(jo["data"]);

                    foreach (var item in ja)
                    {
                        betData b = new betData();
                        b.bbid = item["tradeNo"].ToString();
                        b.username = item["fastSubName"].ToString();
                        b.betMoney = decimal.Parse(item["orderAmountRmb"].ToString()) / 10000;

                        if (item["originTradeUq"].ToString().Length>0)
                            b.Memo = "补单" + item["tradeNo"].ToString();
                        else
                            b.Memo = item["tradeNo"].ToString();

                        if (item["synDesc"].ToString().Length == 0)
                            list.Add(b);

                    }
                }

                return list;
            }
            catch (WebException ex)
            {
                //if (ex.HResult == -2146233079)
                //{
                //    // 远程服务器返回错误: (404) 未找到。
                //}
                //if (ex.Message == "操作超时")
                //{
                //    //需要重新登录
                //    login();
                //    return null;
                //}
                appSittingSet.Log("获取列表失败：" + ex.Message);
                return null;
            }

        }

        /// <summary>
        /// 回填操作结果
        /// </summary>
        /// <param name="bb"></param>
        /// <returns></returns>
        public static bool confirmAct(betData b)
        {
            bool r= false;
            if (!b.passed)
            {
                r =addRemark(b);
            }
            r =changeStatus(b);

            if (r)
            {
                //插入本地数据库
                appSittingSet.execSql($"insert  or ignore  into record values({0},'{b.bbid}','{b.username}',datetime(CURRENT_TIMESTAMP,'localtime'),0,{(b.passed ? 1 : 0)});");
                string msg = $"用户{b.username}处理完毕，处理为 {(b.passed ? "通过" : "不通过")}，回复消息 {b.msg} {DateTime.Now.ToString()}";
                Console.WriteLine(msg);
                appSittingSet.Log(msg);
            }
            return r;
        }

        public static bool addRemark(betData b)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                string url = urlbase + $"oc/remarkMerchantOrder?tradeNo={b.bbid}&desc={HttpUtility.UrlEncode(b.msg,Encoding.UTF8).ToUpper()}";
                //string url = urlbase + $"oc/remarkMerchantOrder?tradeNo={b.bbid}&desc={b.msg}";
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "POST";
                request.Headers.Add("logntoken", token);
                request.Headers.Add("username", subAccount);
                request.Headers.Add("userrole", "merchant");
                request.Headers.Add("busitype", "merchant");
                request.Headers.Add("clitype", "PC");
                request.Headers.Add("cliv", "1.0.0");

                request.CookieContainer = cookie;
                request.ContentLength = 0;

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

        private static bool changeStatus(betData b)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                string url = urlbase + $"oc/manualchantOrder?tradeNo={b.bbid}&synStatus=6";

                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "POST";
                request.Headers.Add("logntoken", token);
                request.Headers.Add("username", subAccount);
                request.Headers.Add("userrole", "merchant");
                request.Headers.Add("busitype", "merchant");
                request.Headers.Add("clitype", "PC");
                request.Headers.Add("cliv", "1.0.0");

                request.CookieContainer = cookie;
                request.ContentLength = 0;

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

        public static string MD5Encrypt(string str)
        {
            MD5 md5 = MD5.Create();
            // 将字符串转换成字节数组
            byte[] byteOld = Encoding.UTF8.GetBytes(str);
            // 调用加密方法
            byte[] byteNew = md5.ComputeHash(byteOld);
            // 将加密结果转换为字符串
            StringBuilder sb = new StringBuilder();
            foreach (byte b in byteNew)
            {
                // 将字节转换成16进制表示的字符串，
                sb.Append(b.ToString("x2"));
            }
            // 返回加密的字符串
            return sb.ToString();
        }
    }
}
