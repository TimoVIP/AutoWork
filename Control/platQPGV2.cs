using BaseFun;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TimoControl
{
    public static  class platQPGV2
    {
        private static string urlbase { get; set; }
        private static string acc { get; set; }
        private static string pwd { get; set; }
        private static string uid { get; set; }
        private static CookieContainer cookie { get; set; }

        private static string subAccount { get; set; }
        private static string mainAccount { get; set; }
        private static string token { get; set; }
        private static string transNo { get; set; }
        private static int Hours { get; set; }
        private static int deposit_status { get; set; }
        /// <summary>
        /// 登录
        /// </summary>
        /// <returns></returns>
        public static bool login()
        {
            try
            {
                string s1 = appSittingSet.readAppsettings("QPG");
                acc = s1.Split('|')[0];
                pwd = s1.Split('|')[1];
                urlbase = s1.Split('|')[2];
                mainAccount = acc.Split('@')[1];
                string h = appSittingSet.readAppsettings("Hours");
                Hours = h==""? 1: int.Parse(h);
                string dep = appSittingSet.readAppsettings("deposit_status");
                deposit_status = dep == "" ? 0 : int.Parse(dep);
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
                string url = $"{urlbase}v3/api/merchant/merchantcenter/subAccount/loginOfSubAccount?subAccount={acc}&password={pwd}";
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "POST";
                request.ContentLength = 0;
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
                    mainAccount = subAccount.Split('@')[1];
                    token = jo["data"]["token"].ToString();
                    transNo = jo["transNo"].ToString();
                    return true;
                }
                else
                    return false;
            }
            catch (WebException ex)
            {
                appSittingSet.Log(string.Format("登录失败：{0}   ", ex.Message));
                return false;
            }
        }

        public static List<betData> getActData()
        {

            List<betData> list = new List<betData>();

            try
            {

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                string url = $"{urlbase}v1/api/q/funding/backend/merchantPayOrderQueryReq";

                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "POST";
                request.Headers.Add("logntoken", token);
                request.Headers.Add("username", subAccount);
                request.Headers.Add("userrole", "merchant");
                request.Headers.Add("busitype", "merchant");
                request.Headers.Add("clitype", "PC");
                request.Headers.Add("cliv", "1.0.0");
                request.Headers.Add("X-Tk", token);

                request.CookieContainer = cookie;

                //string postdata = JsonConvert.SerializeObject(new { from = mainAccount, merchant = mainAccount, start_time_min = DateTime.Now.Date.ToString("yyyy-MM-dd") + "00:00:00", start_time_max = DateTime.Now.Date.ToString("yyyy-MM-dd") + "23:59:59", order_status = 2, deposit_status = 0, sub_member_no = "", order_no = "", start_page = 1, page_num = 50 });
                string postdata = JsonConvert.SerializeObject(new { from = mainAccount, merchant = mainAccount, start_time_min = DateTime.Now.AddHours(-Hours).ToString("yyyy-MM-dd HH:mm:ss") , start_time_max = DateTime.Now.Date.AddDays(1).AddSeconds(-1).ToString("yyyy-MM-dd HH:mm:ss"), order_status = 2, deposit_status = deposit_status, sub_member_no = "", order_no = "", start_page = 1, page_num = 20 });
                byte[] bytes = Encoding.UTF8.GetBytes(postdata);
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

                JObject jo = (JObject)JsonConvert.DeserializeObject(ret_html.ToString());
                if (jo["message"].ToString() == "OK")
                {
                    if (jo["data"]["orders_array"].ToString().Length > 1)
                    {
                        JArray ja = JArray.FromObject(jo["data"]["orders_array"]);

                        foreach (var item in ja)
                        {
                            betData b = new betData();
                            b.bbid = item["orderNo"].ToString().Trim();
                            b.username = item["subNo"].ToString().Trim();
                            b.betMoney = decimal.Parse(item["quan"].ToString().Trim()) / 10000;
                            b.Memo = item["Remark"].ToString().Trim();

                            //没有备注的加上
                            if (item["Remark"].ToString().Trim().Length == 0 && item["DepositStatus"].ToString() == "3")
                                list.Add(b);
                        }
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
            bool r = false;
            if (!b.passed)
            {
                r = addRemark(b);
            }
            r = changeStatus(b);
            if (r)
            {
                //插入本地数据库
                SQLiteHelper.SQLiteHelper.execSql($"insert  or ignore  into record values({0},'{b.bbid}','{b.username}',datetime(CURRENT_TIMESTAMP,'localtime'),0,{(b.passed ? 1 : 0)});");
                string msg = $"订单{b.bbid}用户{b.username}处理完毕，处理为 {(b.passed ? "通过" : "不通过")}，回复消息 {b.msg} {DateTime.Now.ToString()}";
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
                //"https://b.gac.top/v1/api/q/funding/webMerchant/setOrderRemark";
                string url = $"{urlbase}v1/api/q/funding/webMerchant/setOrderRemark";
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "POST";
                request.Headers.Add("logntoken", token);
                request.Headers.Add("username", subAccount);
                request.Headers.Add("userrole", "merchant");
                request.Headers.Add("busitype", "merchant");
                request.Headers.Add("clitype", "PC");
                request.Headers.Add("cliv", "1.0.0");
                request.Headers.Add("X-Tk", token);

                request.CookieContainer = cookie;
                request.ContentLength = 0;

                string postdata = JsonConvert.SerializeObject(new { from = mainAccount, OrderID = b.bbid, OrderRemark = b.msg });
                byte[] bytes = Encoding.UTF8.GetBytes(postdata);
                request.ContentLength = bytes.Length;
                Stream newStream = request.GetRequestStream();
                newStream.Write(bytes, 0, bytes.Length);
                newStream.Close();

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string ret_html = reader.ReadToEnd();
                JObject jo = (JObject)JsonConvert.DeserializeObject(ret_html);

                cookie.Add(response.Cookies);
                reader.Close();
                reader.Dispose();
                response.Dispose();
                request.Abort();
                if (jo["message"].ToString() == "OK" && jo["msgDebug"].ToString() == "OK")
                {
                    return true;
                }
                else
                    return false;
            }
            catch (WebException ex)
            {
                string msg = $"添加备注失败：用户 {b.username} 单{b.betno} 错误{ex.Message}";
                appSittingSet.Log(msg);
                return false;
            }


        }

        public static bool changeStatus(betData b)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                 string url = $"{urlbase}v3/api/merchant/merchantcenter/oc/manualchantOrder?tradeNo={b.bbid}&synStatus=4";

                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "POST";
                request.Headers.Add("busiType", "merchant");
                request.Headers.Add("cliType", "PC");
                request.Headers.Add("cliV", "1.0.0");
                request.Headers.Add("lognToken", token);
                request.Headers.Add("userName", subAccount);
                request.Headers.Add("userRole", "merchant");

                request.Headers.Add("Sec-Fetch-Mode", "cors");
                request.Headers.Add("Sec-Fetch-Site", "same-origin");
                request.UserAgent = "ozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/76.0.3809.100 Safari/537.36";
                request.KeepAlive = true;
                request.Headers.Add("X-Tk", token);
                request.CookieContainer = cookie;
                request.ContentLength = 0;
                //request.Host = urlbase.Replace("https://", "");


                //request.CookieContainer.Add(new Cookie("sidebarStatus", "0", "", ""));
                //string postdata = JsonConvert.SerializeObject(new { from = mainAccount, OrderID = b.bbid, OrderRemark = b.msg });
                //byte[] bytes = Encoding.UTF8.GetBytes(postdata);
                //request.ContentLength = bytes.Length;
                //Stream newStream = request.GetRequestStream();
                //newStream.Write(bytes, 0, bytes.Length);
                //newStream.Close();

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string ret_html = reader.ReadToEnd();

                cookie.Add(response.Cookies);
                reader.Close();
                reader.Dispose();
                response.Dispose();
                request.Abort();
                if (ret_html.Contains("被强制下线"))
                {
                    bool f = login();
                }

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
