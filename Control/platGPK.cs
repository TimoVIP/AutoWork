using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Text;

namespace TimoControl
{
    public static class platGPK
    {

        private static string acc { get; set; }
        private static string pwd { get; set; }
        private static string url_gpk_base { get; set; }
        private static string td_cookie { get; set; }
        public static string connectionId { get; set; }
        public static string connectionToken { get; set; }
        public static CookieContainer cookie { get; set; }

        /// <summary>
        /// 登录GPK
        /// </summary>
        /// <returns></returns>
        public static bool loginGPK()
        {

            try
            {
                string s3 = appSittingSet.readAppsettings("GPK");

               acc = s3.Split('|')[0];
               pwd = s3.Split('|')[1];
               url_gpk_base = s3.Split('|')[2];
               //不从appconfig 里面取得了 第一次直接生产随机数 后面从gpk获取真实数据
               connectionId = Guid.NewGuid().ToString();
                td_cookie = "18446744070" + new Random().Next(100000000, 999999999).ToString();
                //td_cookie = ((DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000).ToString();//时间戳不对
            }
            catch (Exception ex)
            {
                appSittingSet.txtLog("获取配置文件失败" + ex.Message);
                return false;
            }

            try
            {
                HttpWebRequest request = WebRequest.Create(url_gpk_base + "Account/ValidateAccount") as HttpWebRequest;
                request.Method = "POST";
                request.UserAgent = "Mozilla/4.0";
                request.KeepAlive = true;
                request.Accept = "application/json, text/plain, */*";
                request.ContentType = "application/json; charset=utf-8";
                string postdata = "{\"account\":\"" + acc + "\",\"password\":\"" + pwd + "\"}";

                string headurl = url_gpk_base.Replace("http://", "").Replace("/", "");
                //Init();//证书错误

                //设置请求头、cookie
                cookie = new CookieContainer();
                request.CookieContainer = cookie;
                //request.Headers.Add("Origin", "bh.bs004.gpk135.com");

                //发送数据
                byte[] bytes = Encoding.UTF8.GetBytes(postdata);
                request.ContentLength = bytes.Length;
                Stream newStream = request.GetRequestStream();
                newStream.Write(bytes, 0, bytes.Length);
                newStream.Flush();
                newStream.Close();

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                    {
                        //Set - Cookie 设置
                        string s = response.Headers.Get("Set-Cookie").Replace(" path=/", "");

                        string[] sl = response.Headers.GetValues("Set-Cookie");
                        foreach (string item in sl)
                        {
                            if (item.Contains("master"))
                            {
                                cookie.Add(new Cookie("master", item.Substring(item.IndexOf('=') + 1, 32), "/", headurl));
                            }
                            if (item.Contains(".ASPXAUTHFORMASTER"))
                            {
                                cookie.Add(new Cookie(".ASPXAUTHFORMASTER", item.Substring(item.IndexOf('=') + 1, item.IndexOf(';') - item.IndexOf('=') - 1), "/", headurl));
                            }
                        }

                        cookie.Add(new Cookie("language", "zh-CN", "/", headurl));
                        cookie.Add(new Cookie("td_cookie", td_cookie, "/", headurl));

                        string ret_html = reader.ReadToEnd();
                        cookie.Add(response.Cookies);
                        //platGPK.cookie = cookie;

                        if (ret_html == "{\"IsSuccess\":true,\"Methods\":null}")
                        {
                            //获取 GPKconnectionId 从appconfig 获取固定的即可 
                            //改为动态获取 
                            List<string> list = getNegotiate();
                            if (list.Count == 2)
                            {
                                connectionToken = list[0];
                                connectionId = list[1];
                            }
                            //获取游戏列表 登陆获取一次 2019年3月1日 12点55分

                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                appSittingSet.txtLog(string.Format("GPK站登录失败：{0}   ", ex.Message));
                return false;
            }
            
        }
        /// <summary>
        /// 获取 ConnectionId ConnectionToken等信息 GET
        /// </summary>
        /// <returns></returns>
        public static List<string> getNegotiate()
        {
            List<string> list = new List<string>();
            try
            {
                //获取 json {"Url":"/signalr","ConnectionToken":"T44v9Tu/1IStPOnbdE7+HdM42DTHWbTdLepvqqAKH35l4Mn0MEW55bMZBhHqxwrPMZhwMNr3bhdgcVwR0xdKjJTn/sik78JhwpCgw9Jc18HvJw6I","ConnectionId":"60d5d888-6f43-46a7-860f-98f382dd4edc","KeepAliveTimeout":20.0,"DisconnectTimeout":30.0,"ConnectionTimeout":110.0,"TryWebSockets":true,"ProtocolVersion":"1.5","TransportConnectTimeout":5.0,"LongPollDelay":0.0}

                HttpWebRequest request = WebRequest.Create(url_gpk_base + "signalr/negotiate?clientProtocol=1.5&connectionData=%5B%7B%22name%22%3A%22mainhub%22%7D%5D&_=1543135791156") as HttpWebRequest;
                request.Method = "GET";
                request.UserAgent = "Mozilla/4.0";
                request.KeepAlive = true;
                request.Accept = "text/plain, */*; q=0.01";
                request.ContentType = "application/json; charset=utf-8";//发送的是json数据 注意
                request.Host = url_gpk_base.Replace("http://", "").Replace("/", "");
                request.Referer = url_gpk_base + "MemberDeposit";
                request.Headers.Add("X-Requested-With", "XMLHttpRequest");
                //设置请求头、cookie
                request.CookieContainer = cookie;

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string ret_html = reader.ReadToEnd();
                reader.Close();
                reader.Dispose();
                response.Close();
                response.Dispose();

                JObject jo = (JObject)JsonConvert.DeserializeObject(ret_html);

                list.Add(jo["ConnectionToken"] == null ? "" : jo["ConnectionToken"].ToString());
                list.Add(jo["ConnectionId"] == null ? "" : jo["ConnectionId"].ToString());

                //string value = jo[item][index].ToString(); 
                appSittingSet.txtLog(string.Format("获取到的connectionToken:{0} ; connectionId:{1};", list[0], list[1]));
                return list;

            }
            catch (WebException ex)
            {
                if (ex.HResult == -2146233079)
                {
                    list.Add("False");
                }
                appSittingSet.txtLog("获取connectionId、connectionToken失败");
                return list;
            }

        }
        /// <summary>
        /// 检查账号、钱包
        /// </summary>
        /// <returns></returns>
        public static betData checkInGPK(betData bb)
        {

            try
            {
                string postUrl = "Member/Search";
                string postData = JsonConvert.SerializeObject(new { Account = bb.username, BbAccount = bb.wallet, connectionId = connectionId });
                string postRefere = url_gpk_base;
                JObject jo = GetResponse<JObject>(postUrl, postData, "POST", postRefere);
                if ((bool)jo["IsSuccess"])
                {
                    JArray ja = JArray.FromObject(jo["PageData"]);
                    if (ja.Count>0)
                    {
                        bb.level = jo["PageData"][0]["MemberLevelSettingId"].ToString();
                        bb.passed = true;
                    }
                    else
                    {
                        bb.passed = false;
                        bb.msg = "请提供正确的账号 R";
                    }
                }
                else
                {
                    bb.passed = false;
                    bb.msg = "请提供正确的账号 R";
                }

                return bb;

            }
            catch (Exception ex)
            {
                string msg = string.Format("用户 {0} 注单{1}  GPK查询账号、钱包失败 {2}", bb.username, bb.betno, ex.Message);
                //如果 操作超时 重新登录一下GPK
                //if (ex.HResult == -2146233079 || ex.Message == "操作超时")
                //{
                //    //需要重新登录
                //    loginGPK();
                //    msg = string.Format("用户 {0} 注单{1}  GPK查询账号、钱包失败 {2} 已经重新登录账号", bb.username, bb.betno, ex.Message);
                //}
                appSittingSet.txtLog(msg);
                return null;
            }

        }

        /// <summary>
        /// 提交充值到GPK 不适合 改用公共方法 报错 必须同一个请求对象
        /// </summary>
        /// <param name="bb"></param>
        public static bool submitToGPK(betData bb)
        {
            //查询
            HttpWebRequest request = null;
            HttpWebResponse response = null;
            StreamReader reader = null;
            string ret_html = "";
            bool flag = true;

            try
            {
                //获取deposit_token
                request = WebRequest.Create(url_gpk_base + "Member/DepositToken") as HttpWebRequest;

                request.ProtocolVersion = HttpVersion.Version11;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                //这个在Post的时候，一定要加上，如果服务器返回错误，他还会继续再去请求，不会使用之前的错误数据，做返回数据
                request.ServicePoint.Expect100Continue = false;

                request.Method = "POST";
                request.UserAgent = "Mozilla/4.0";
                request.KeepAlive = true;
                request.Accept = "application/json, text/plain, */*";
                request.ContentType = "application/json; charset=utf-8";//发送的是json数据 注意
                request.Host = url_gpk_base.Replace("http://", "").Replace("/", "");
                request.Referer = url_gpk_base + "MemberDeposit";
                request.Headers.Add("Origin", url_gpk_base);
                request.ContentLength = 0;
                request.Headers.Add("X-Requested-With", "XMLHttpRequest");
                //设置请求头、cookie
                request.CookieContainer = cookie;

                response = (HttpWebResponse)request.GetResponse();
                reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                ret_html = reader.ReadToEnd().Replace("\"", "");


                //如果为空 
                reader.Close();
                reader.Dispose();

                response.Close();
                response.Dispose();
            }
            catch (WebException ex)
            {
                string msg = string.Format("GPK获取充值Token失败：用户 {0} 活动{1} {2} ", bb.username, bb.aname, ex.Message);
                appSittingSet.txtLog(msg);
                flag = false;
            }


            try
            {
                //提交数据
                request = WebRequest.Create(url_gpk_base + "Member/DepositSubmit") as HttpWebRequest;
                request.Method = "POST";
                request.UserAgent = "Mozilla/4.0";
                request.KeepAlive = true;
                request.Accept = "application/json, text/plain, */*";
                request.ContentType = "application/json; charset=utf-8";//发送的是json数据 注意
                request.Host = url_gpk_base.Replace("http://", "").Replace("/", "");
                request.Referer = url_gpk_base + "MemberDeposit";
                request.Headers.Add("Origin", url_gpk_base);
                request.Headers.Add("X-Requested-With", "XMLHttpRequest");
                string postdata = "";
                var obj1 = new { AccountsString = bb.username, Amount = bb.betMoney, AmountString=bb.betMoney, Audit =bb.Audit, AuditType = bb.AuditType, DepositToken = ret_html, IsReal= bb.isReal, Memo = bb.Memo, Password = pwd, PortalMemo = bb.PortalMemo, TimeStamp = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000, Type = bb.Type };


                //var obj1 = new { AccountsString = bb.username, Amount = bb.betMoney, AuditType = "None", DepositToken = ret_html, Memo = bb.aname, Password = pwd, PortalMemo = bb.aname, TimeStamp = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000, Type = 5 };
                //postdata = "{\"AccountsString\":\"" + bb.username + "\",\"Type\":5,\"DepositToken\":\"" + ret_html + "\",\"AuditType\":\"None\",\"Amount\":" + bb.betMoney + ",\"IsReal\":false,\"PortalMemo\":\"" + aname + "-" + bb.gamename + "-" + bb.betno + "\",\"Memo\":\"" + aname + "-" + bb.gamename + "-" + bb.betno + "\",\"Password\":\"" + pwd + "\",\"TimeStamp\":" + (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000 + "}";
                postdata = JsonConvert.SerializeObject(obj1);
                //需要稽核
                //if (aname.Contains("消除") || aname.Contains("幸运")  || aname.Contains("APP")|| aname.Contains("救援") || bb.needAudit)
                //if(bb.needAudit)
                //{
                //    if (bb.betAudit==0)
                //    {
                //        bb.betAudit = 1;
                //    }
                //    var obj2 = new { AccountsString = bb.username, Amount = bb.betMoney, Audit = bb.betMoney * bb.betAudit , AuditType = "Discount", DepositToken = ret_html, Memo = bb.aname, Password = pwd, PortalMemo = bb.aname, TimeStamp = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000, Type = 5 };
                //    postdata = JsonConvert.SerializeObject(obj2);
                //}

                //设置请求头、cookie
                request.CookieContainer = cookie;

                //发送数据
                byte[] bytes = Encoding.UTF8.GetBytes(postdata);
                request.ContentLength = bytes.Length;
                Stream newStream = request.GetRequestStream();
                newStream.Write(bytes, 0, bytes.Length);
                newStream.Flush();
                newStream.Close();

                response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode== HttpStatusCode.OK)
                {
                    flag = true;
                }

                response.Close();
                response.Dispose();
            }
            catch (WebException ex)
            {
                string msg = string.Format("GPK提交充值数据失败：用户 {0} 活动{1} {2} ", bb.username, bb.aname, ex.Message);
                appSittingSet.txtLog(msg);
                flag = false;
            }

            return flag;

        }


        /// <summary>
        /// 刷新会员账户余额
        /// </summary>
        /// <returns></returns>
        public static bool autoUpadateMemberAcc(string id)
        {
            try
            {
                string postUrl = "Member/AllWalletUpdateMember";
                string postData = "{\"id\":" + id + "}";
                string postRefere = "MemberDeposit";
                JObject jo = GetResponse<JObject>(postUrl, postData, "POST", postRefere);
                HttpStatusCode r = GetResponse<HttpStatusCode>(postUrl, postData, "POST", postRefere);
                return r == HttpStatusCode.OK ? true : false;
            }
            catch (WebException ex)
            {
                appSittingSet.txtLog(ex.Message);
                if (ex.HResult == -2146233079)
                {
                    return true;
                }
                return false;
            }
        }


        /// <summary>
        /// 检查账号 获取转账记录
        /// </summary>
        /// <returns></returns>
        public static betData MemberTransactionSearch(betData bb)
        {
            string postUrl = "MemberTransaction/Search";
            string postData = "{\"Account\":\"" + bb.username + "\",\"IsReal\":\"true\",\"Types\":[\"Account\",\"Manual\",\"ThirdPartyPayment\"]";
            if (bb.lastOprTime != null && bb.lastOprTime != "")
            {
                DateTime d1;
                DateTime.TryParse(bb.lastOprTime, out d1);
                bb.lastOprTime = d1.AddHours(-12).ToString("yyyy/MM/dd HH:mm:ss");
                postData = postData + ",\"TimeBegin\":\"" + bb.lastOprTime + "\"";
            }
            if (bb.betTime != null && bb.betTime != "")
            {
                //时间-12 变为美东时间
                DateTime d2;
                DateTime.TryParse(bb.betTime, out d2);
                bb.betTime = d2.AddHours(-12).ToString("yyyy/MM/dd HH:mm:ss");
                postData = postData + ",\"TimeEnd\":\"" + bb.betTime + "\"";
            }
            postData = postData + "}";

            JObject jo = GetResponse<JObject>(postUrl, postData, "POST", "");

            try
            {
                if ((bool)jo["IsSuccess"] == false)
                {
                    //appSittingSet.txtLog(postdata + " 没有交易记录");
                    bb.passed = false;
                    return bb;
                }

                int total = 0;
                int.TryParse(jo["Total"].ToString(), out total);
                //int total =(int) jo["Total"];
                //decimal totalMoney =(decimal) jo["TotalMoney"];
                decimal totalMoney = 0;
                decimal.TryParse(jo["TotalMoney"].ToString(), out totalMoney);
                //bb.betMoney = (decimal) ja[0]["Amount"];//记录总金额 后面会改回来
                JArray ja = JArray.FromObject(jo["PageData"]);
                decimal amount = 0;
                decimal.TryParse(ja[0]["Amount"].ToString(), out amount);
                bb.betMoney = amount;

                decimal subtotal = 0;
                decimal.TryParse(ja[0]["Subtotal"].ToString(), out subtotal);
                bb.subtotal = subtotal;

                bb.level = ja[0]["MemberLevelId"].ToString();//记录层级
                bb.Id = ja[0]["Id"].ToString();//记录ID
                bb.memberId = ja[0]["MemberId"].ToString();//记录MemberId
                bb.lastCashTime = ja[0]["Time"].ToString();
                //bb.lastCashTime = ja[0]["Time"].ToString().Replace("\\", "").Replace("/Date(", "").Replace(")", "").Replace("/","");//计算的时间不对 需要自己来算
                //bb.lastCashTime = appSittingSet.unixTimeToTime(bb.lastCashTime);
                bb.passed = true;
                bb.total_money = totalMoney;
                bb.betTimes = total;
                return bb;
            }
            catch (Exception ex)
            {
                string msg = string.Format("用户 {0} GPK查询转账记录失败 {1}", bb.username, ex.Message);
                appSittingSet.txtLog(msg);
                return null;
            }
        }

        /// <summary>
        /// 更新会员级别 
        /// </summary>
        /// <param name="memberId">会员号</param>
        /// <param name="levelId">级别号</param>
        /// <returns></returns>
        public static bool UpadateMemberLevel(string memberId, string levelId)
        {
            try
            {
                string postUrl = "Member/UpdateMemberLevel";
                string postData = "{\"memberId\":" + memberId + ",\"levelId\":" + levelId + "}";
                string postRefere = "MemberDeposit";
                HttpStatusCode r = GetResponse<HttpStatusCode>(postUrl, postData, "POST", postRefere);
                return r == HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                appSittingSet.txtLog(memberId + " 更新等级失败 " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 查询账户详细信息
        /// </summary>
        /// <param name="Account"></param>
        /// <returns></returns>
        public static betData MemberGetDetail(betData bb)
        {
            try
            {
                string postUrl = "Member/GetDetail";
                string postData = "{\"account\":\"" + bb.username + "\"}";
                string postRefere = "MemberDeposit";
                JObject jo = GetResponse<JObject>(postUrl, postData, "POST", postRefere);

                decimal d = 0;
                decimal.TryParse(jo["Member"]["Wallet"].ToString(), out d);
                bb.subtotal = d;
                return bb;
            }
            catch (Exception ex)
            {
                //如果 操作超时 重新登录一下GPK
                string msg = "查询账户信息(钱包)失败 " + ex.Message;
                appSittingSet.txtLog(msg);
                return null;
            }
        }

        /// <summary>
        /// 查询是否有注单记录
        /// </summary>
        /// <param name="bb"></param>
        /// <returns></returns>
        public static object BetRecordSearch(betData bb)
        {
            try
            {
                string postUrl = "BetRecord/Search";
                string postData = "{\"Account\":\"" + bb.username + "\",";
                if (bb.lastCashTime!=null)
                {
                    postData = postData + "\"WagersTimeBegin\":\"" + bb.lastCashTime + "\",";
                }
                if (bb.lastOprTime!=null)
                {
                    postData = postData + "\"WagersTimeEnd\":\"" + bb.lastOprTime + "\",";
                    postData = postData + "\"PayoffTimeEnd\":\"" + bb.lastOprTime + "\",";
                }

                if (bb.GameCategories != null)
                {
                    postData = postData + "\"GameCategories\":" + bb.GameCategories + ",";
                }
                postData = postData + "\"connectionId\":\"" + connectionId + "\"}";

                string postRefere = "MemberDeposit";
                JObject jo = GetResponse<JObject>(postUrl, postData, "POST", postRefere);
                JArray ja = JArray.FromObject(jo["PageData"]);
                return ja.Count == 0 ? true : false;
            }
            catch (Exception ex)
            {
                //如果 操作超时 重新登录一下GPK
                string msg = "查询是否有注单记录错误 " + ex.Message;
                appSittingSet.txtLog(msg);
                return null;
            }
        }

        /// <summary>
        /// 获取账户列表 到数据库 无用到
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public static List<Gpk_UserInfo> getInfor(int pageIndex)
        {
                StringBuilder sb = new StringBuilder();
            try
            {

                HttpWebRequest request = WebRequest.Create(url_gpk_base + "Member/Search") as HttpWebRequest;
                request.Method = "POST";
                request.UserAgent = "Mozilla/4.0";
                request.KeepAlive = true;
                request.Accept = "application/json, text/plain, */*";
                request.ContentType = "application/json; charset=utf-8";//发送的是json数据 注意
                request.Host = url_gpk_base.Replace("http://", "").Replace("/", "");
                request.Referer = url_gpk_base + "MemberDeposit";
                request.Headers.Add("Origin", url_gpk_base);

                request.Headers.Add("X-Requested-With", "XMLHttpRequest");
                //设置请求头、cookie
                request.CookieContainer = cookie;
                string postdata = "";
                if (pageIndex==1)
                {
                    postdata = "{\"BalanceBegin\":\"1\",\"State\":\"1\",\"connectionId\":\"" + connectionId + "\"}";
                }
                else
                {
                    postdata = "{\"BalanceBegin\":\"1\",\"State\":\"1\",\"pageIndex\":"+(pageIndex-1).ToString()+",\"connectionId\":\""+connectionId+"\"}";
                }

                //发送数据
                byte[] bytes = Encoding.UTF8.GetBytes(postdata);
                request.ContentLength = bytes.Length;
                Stream newStream = request.GetRequestStream();
                newStream.Write(bytes, 0, bytes.Length);
                newStream.Flush();
                newStream.Close();

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string ret_html = reader.ReadToEnd();

                //如果为空 
                reader.Close();
                reader.Dispose();
                response.Close();
                response.Dispose();

                JsonSerializerSettings js = new JsonSerializerSettings();
                //js.DateParseHandling = DateParseHandling.None;
                js.DateParseHandling = DateParseHandling.DateTime;
                js.DateTimeZoneHandling = DateTimeZoneHandling.Local;
                //js.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                JObject jo = (JObject)JsonConvert.DeserializeObject(ret_html, js);
                //JObject jo = JObject.Parse(ret_html,js);
                //string s = appSittingSet.unixTimeToTime("1544116883183");
                if ((bool)jo["IsSuccess"] == false)
                {
                    appSittingSet.txtLog(postdata + " 没有交易记录");
                    return null;
                }

                JArray ja = JArray.FromObject(jo["PageData"]);
                List<Gpk_UserInfo> list = new List<Gpk_UserInfo>();

                foreach (var item in ja)
                {
                    Gpk_UserInfo info1 = new Gpk_UserInfo() { };
                    info1.Account = item["Account"].ToString();
                    info1.Balance = decimal.Parse(item["Balance"].ToString());
                    info1.JoinTime = item["JoinTime"].ToString();
                    info1.MemberLevelSettingName = item["MemberLevelSettingName"].ToString();
                    info1.Name = item["Name"].ToString();

                    list.Add(info1);
                    //生成sql语句
                    sb.Append("INSERT INTO infor ( Account, Balance, JoinTime, MemberLevelSettingName, Name ) VALUES( '"+ info1.Account +"', "+info1.Balance+", '"+info1.JoinTime+"', '"+info1.MemberLevelSettingName+"',  '"+info1.Name+"' ); ");

                }
                try
                {
                    //插入到数据库
                    bool f = appSittingSet.execSql(sb.ToString());
                    appSittingSet.txtLog("已经操作页数" + pageIndex);
                }
                catch (Exception ex)
                {
                    appSittingSet.txtLog(ex.Message);
                }

                return list;
            }
            catch (WebException ex)
            {
                //如果 操作超时 重新登录一下GPK
                string msg = "获取账户列表失败，编号为： "+pageIndex+" " + ex.Message;
                if (ex.HResult == -2146233079 || ex.Message == "操作超时")
                {
                    //需要重新登录
                    loginGPK();
                    msg += " 已经重新登录账号 ";
                }

                appSittingSet.txtLog(msg);
                return null;
            }


        }



        /// <summary>
        /// 查询用户 统计报表
        /// GET 方法
        /// </summary>
        /// <param name="bb"></param>
        /// <returns></returns>
        public static betData GetDetailInfo(betData bb)
        {
            HttpWebRequest request = null;
            HttpWebResponse response = null;
            StreamReader reader = null;

            try
            {
                //                string postdata =string.Format( "?account={0}&begin={1}&end={2}&isMember=true&orderBy=Commissionable&reverse=true&skip=0&take=100&types=BBINprobability&types=BBINFish30&types=BBINFish38&types=AgEbr&types=AgHsr&types=AgYoPlay&types=Mg2Slot&types=Mg2Html5&types=Pt2Slot&types=GpiSlot3D&types=GpiSlotR&types=GnsSlot&types=PrgSlot&types=SgSlot&types=Rg2Fish&types=Rg2Slot&types=JdbSlot&types=JdbFish&types=HabaSlot&types=Cq9Slot&types=Cq9Fish&types=NetEntSlot&types=GdSlot&types=Pt3Slot&types=RedTigerSlot&types=GameArtSlot&types=Mw2Slot&types=PgSlot&types=RedTiger2Slot&types=LgVirtualSport&types=Mg3Slot&types=IsbSlot&types=PtsSlot&types=PngSlot&types=City761Fish&types=FsSlot&types=FsFish&types=FsArcade&types=KaSlot&types=JsSlot&types=JsFish&types=GtiSlot&types=PlsSlot&types=AeSlot",bb.username, DateTime.Now.AddDays(-DateTime.Now.Day + 1).ToString("yyyy/MM/dd"),DateTime.Now.Date.ToString("yyyy/MM/dd"));                                                                                                     
                string postdata =string.Format("?account={0}&begin={1}&end={2}&isMember=true&orderBy=Commissionable&reverse=true&skip=0&take=100&types=BBINprobability&types=BBINFish30&types=BBINFish38&types=AgEbr&types=AgHsr&types=AgYoPlay&types=Mg2Slot&types=Mg2Html5&types=Pt2Slot&types=GpiSlot3D&types=GpiSlotR&types=GnsSlot&types=PrgSlot&types=SgSlot&types=Rg2Fish&types=Rg2Slot&types=JdbSlot&types=JdbFish&types=HabaSlot&types=Cq9Slot&types=Cq9Fish&types=NetEntSlot&types=GdSlot&types=Pt3Slot&types=RedTigerSlot&types=GameArtSlot&types=Mw2Slot&types=PgSlot&types=RedTiger2Slot&types=LgVirtualSport&types=Mg3Slot&types=IsbSlot&types=PtsSlot&types=PngSlot&types=City761Fish&types=FsSlot&types=FsFish&types=FsArcade&types=KaSlot&types=JsSlot&types=JsFish&types=GtiSlot&types=PlsSlot&types=AeSlo", bb.username, DateTime.Now.AddDays(-DateTime.Now.Day + 1).ToString("yyyy/MM/dd"),DateTime.Now.Date.ToString("yyyy/MM/dd"));
                request = WebRequest.Create(url_gpk_base + "Statistics/GetDetailInfo" +postdata) as HttpWebRequest;
                request.Method = "GET";
                request.UserAgent = "Mozilla/4.0";
                request.KeepAlive = true;
                request.Accept = "application/json, text/plain, */*";
                //request.ContentType = "application/json; charset=utf-8";//发送的是json数据 注意
                request.Host = url_gpk_base.Replace("http://", "").Replace("/", "");
                request.Referer = url_gpk_base + "MemberDeposit";
                //request.Headers.Add("Origin", url_gpk_base);

                request.Headers.Add("X-Requested-With", "XMLHttpRequest");
                //设置请求头、cookie
                request.CookieContainer = cookie;

                response = (HttpWebResponse)request.GetResponse();
                reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string ret_html = reader.ReadToEnd();
                JObject jo = JObject.Parse(ret_html);
                //如果为空 
                if ((bool)jo["IsSuccess"] == false)
                {
                    appSittingSet.txtLog(postdata + " 没有记录"+ jo["ErrorMessage"]);
                    bb.passed = false;
                    return bb;
                }
                JArray ja = JArray.FromObject(jo["ReturnObject"]);
                decimal amount = 0;
                decimal.TryParse(ja[0]["Commissionable"].ToString(), out amount);
                bb.total_money = amount;
                
                return bb;
            }
            catch (WebException ex)
            {
                //如果 操作超时 重新登录一下GPK
                string msg = "查询报表失败 " + ex.Message;
                if (ex.HResult == -2146233079 || ex.Message == "操作超时")
                {
                    //需要重新登录
                    loginGPK();
                    msg = string.Format("用户 {0}-{2}-{3} 查询报表失败 {1} 已经重新登录账号 ", bb.username, ex.Message, bb.lastCashTime, bb.betTime);
                }
                appSittingSet.txtLog(msg);
                return null;
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
        /// 查询非电子类的注单报表
        /// </summary>
        /// <param name="bb"></param>
        /// <returns></returns>
        public static betData GetDetailInfo_withoutELE(betData bb)
        {
            HttpWebRequest request = null;
            HttpWebResponse response = null;
            StreamReader reader = null;

            try
            {
                //if (bb.lastOprTime != null && bb.lastOprTime != "")
                //{
                //    DateTime d1;
                //    DateTime.TryParse(bb.lastOprTime, out d1);
                //    bb.lastOprTime = d1.ToString("yyyy/MM/dd");
                //}

                
                //                string postdata =string.Format( "?account={0}&begin={1}&end={2}&isMember=true&orderBy=Commissionable&reverse=true&skip=0&take=100&types=BBINprobability&types=BBINFish30&types=BBINFish38&types=AgEbr&types=AgHsr&types=AgYoPlay&types=Mg2Slot&types=Mg2Html5&types=Pt2Slot&types=GpiSlot3D&types=GpiSlotR&types=GnsSlot&types=PrgSlot&types=SgSlot&types=Rg2Fish&types=Rg2Slot&types=JdbSlot&types=JdbFish&types=HabaSlot&types=Cq9Slot&types=Cq9Fish&types=NetEntSlot&types=GdSlot&types=Pt3Slot&types=RedTigerSlot&types=GameArtSlot&types=Mw2Slot&types=PgSlot&types=RedTiger2Slot&types=LgVirtualSport&types=Mg3Slot&types=IsbSlot&types=PtsSlot&types=PngSlot&types=City761Fish&types=FsSlot&types=FsFish&types=FsArcade&types=KaSlot&types=JsSlot&types=JsFish&types=GtiSlot&types=PlsSlot&types=AeSlot",bb.username, DateTime.Now.AddDays(-DateTime.Now.Day + 1).ToString("yyyy/MM/dd"),DateTime.Now.Date.ToString("yyyy/MM/dd"));                                                                                                     
                //string postdata =string.Format("?account={0}&begin={1}&end={2}&isMember=true&orderBy=Commissionable&reverse=true&skip=0&take=100&types=BBINbbsport&types=BBINlottery&types=BBINvideo&types=SabaSport&types=SabaNumber&types=SabaVirtualSport&types=AgBr&types=Mg2Real&types=Pt2Real&types=GpiReal&types=SingSport&types=AllBetReal&types=IgLottery&types=IgLotto&types=Rg2Real&types=Rg2Board&types=Rg2Lottery&types=Rg2Lottery2&types=JdbBoard&types=EvoReal&types=BgReal&types=GdReal&types=Pt3Real&types=SunbetReal&types=CmdSport&types=Sunbet2Real&types=Mg3Real&types=KgBoard&types=LxLottery&types=EBetReal&types=ImEsport&types=OgReal&types=VrLottery&types=City761Board&types=FsBoard&types=SaReal&types=ImsSport&types=IboSport&types=NwBoard&types=JsBoard&types=ThBoard", bb.username, bb.lastCashTime, bb.lastOprTime);
                string postdata = string.Format("?account={0}&begin={1}&end={2}&isMember=true&orderBy=Commissionable&reverse=true&skip=0&take=100",bb.username, bb.lastCashTime, bb.lastOprTime);
                request = WebRequest.Create(url_gpk_base + "Statistics/GetDetailInfo" +postdata) as HttpWebRequest;
                request.Method = "GET";
                request.UserAgent = "Mozilla/4.0";
                request.KeepAlive = true;
                request.Accept = "application/json, text/plain, */*";
                //request.ContentType = "application/json; charset=utf-8";//发送的是json数据 注意
                request.Host = url_gpk_base.Replace("http://", "").Replace("/", "");
                request.Referer = url_gpk_base + "MemberDeposit";
                //request.Headers.Add("Origin", url_gpk_base);

                request.Headers.Add("X-Requested-With", "XMLHttpRequest");
                //设置请求头、cookie
                request.CookieContainer = cookie;

                response = (HttpWebResponse)request.GetResponse();
                reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string ret_html = reader.ReadToEnd();

                appSittingSet.txtLog(ret_html);

                JObject jo = JObject.Parse(ret_html);
                //如果为空 
                if ((bool)jo["IsSuccess"] == false)
                {
                    appSittingSet.txtLog(postdata + " 没有记录"+ jo["ErrorMessage"]);
                    bb.passed = false;
                    return bb;
                }
                JArray ja = JArray.FromObject(jo["ReturnObject"]);
                decimal amount = 0;
                decimal.TryParse(ja[0]["Commissionable"].ToString(), out amount);
                bb.total_money = amount;
                
                return bb;
            }
            catch (WebException ex)
            {
                //如果 操作超时 重新登录一下GPK
                string msg = "查询报表失败 " + ex.Message;
                if (ex.HResult == -2146233079 || ex.Message == "操作超时")
                {
                    //需要重新登录
                    loginGPK();
                    msg = string.Format("用户 {0}-{2}-{3} 查询报表失败 {1} 已经重新登录账号 ", bb.username, ex.Message, bb.lastCashTime, bb.betTime);
                }
                appSittingSet.txtLog(msg);
                return null;
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
        /// 公共方法
        /// JARRY 必须顶层 是数组才行
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="postUrl"></param>
        /// <param name="postData"></param>
        /// <param name="postMethod"></param>
        /// <returns></returns>
        public static T GetResponse<T>(string postUrl, string postData, string postMethod, string postRefere)
        {
            HttpWebRequest request = null;
            HttpWebResponse response = null;
            StreamReader reader = null;
            try
            {
                request = WebRequest.Create(url_gpk_base + postUrl) as HttpWebRequest;
                request.Method = "POST";
                request.UserAgent = "Mozilla/4.0";
                request.KeepAlive = true;
                request.Accept = "application/json, text/plain, */*";
                request.ContentType = "application/json; charset=utf-8";//发送的是json数据 注意
                request.Host = url_gpk_base.Replace("http://", "").Replace("/", "");
                if (postRefere != null || postRefere != "")
                {
                    request.Referer = url_gpk_base + postRefere;
                }
                request.Headers.Add("Origin", url_gpk_base);
                request.Headers.Add("X-Requested-With", "XMLHttpRequest");
                //设置请求头、cookie
                request.CookieContainer = cookie;
                if (postMethod == "POST" && postData != null)
                {
                    if (postData.Length > 0)
                    {
                        //发送数据
                        byte[] bytes = Encoding.UTF8.GetBytes(postData);

                        Stream newStream = request.GetRequestStream();
                        newStream.Write(bytes, 0, bytes.Length);
                        newStream.Flush();
                        newStream.Close();
                    }
                    else
                    {
                        request.ContentLength = 0;
                    }
                }

                response = (HttpWebResponse)request.GetResponse();
                //只需要状态码
                if (typeof(T).Name == "HttpStatusCode")
                {
                    HttpStatusCode statusCode = response.StatusCode;
                    return (T)(object)statusCode;
                }

                reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                object ret_html = reader.ReadToEnd();

                if (ret_html == null)
                {
                    appSittingSet.txtLog(postUrl + "-" + postData + " 没有记录");
                    return default(T);
                }

                if (typeof(T).Name == "JObject")
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
                //如果 操作超时 重新登录一下GPK
                string msg = "失败-" + postUrl + "-" + ex.Message;
                if (ex.HResult == -2146233079 || ex.Message == "操作超时")
                {
                    //需要重新登录
                    loginGPK();
                    msg = msg + "-已经重新登录账号";
                }
                appSittingSet.txtLog(msg);
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
        /// 同ip用户数
        /// </summary>
        /// <param name="IP"></param>
        /// <returns></returns>
        public static int GetUserCountSameIP(string IP)
        {
            string postUrl = "MemberLogin/SearchV2";
            string postData = "{\"search\":{\"IP\":\"" + IP + "\",\"IpIsLike\":false},\"pageIndex\":\"\",\"pageSize\":30}";
            string postRefere = "MemberLogin?search=true&ip=" + IP;
            JObject jo = GetResponse<JObject>(postUrl, postData, "POST", postRefere);
            //    if (jo != null)
            //    return (int)jo["Total"];
            //else
            //    return 0;
            JArray ja = JArray.FromObject(jo["PageData"]);
            List<string> list_name = new List<string>();
            foreach (var item in ja)
            {
                if (!list_name.Contains(item["Account"].ToString()))
                {
                    list_name.Add(item["Account"].ToString());
                }
            }
            return list_name.Count;
        }

        /// <summary>
        /// 查询操作历史记录 注册和开户时间间隔
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string GetUserLoadHistory(string id, string username, int timeSec_min, int timeSec_max)
        {
            try
            {
                string postUrl = "Member/LoadHistory";
                string postData = "{\"id\":" + id + ",\"take\":100,\"skip\":0,\"query\":{}}";
                string postRefere = "Member/" + username + "/History";
                JArray ja = GetResponse<JArray>(postUrl, postData, "POST", postRefere);

                DateTime dt1 = DateTime.Parse("2018/01/01 00:00:00");
                DateTime dt2 = DateTime.Parse("2018/01/01 00:00:00");

                foreach (var item in ja)
                {
                    if (item["Content"].ToString() == "申请加入会员")
                    {
                        dt1 = DateTime.Parse(item["Time"].ToString());
                    }
                    if (item["Content"].ToString() == "建立银行帐户资讯")
                    {
                        dt2 = DateTime.Parse(item["Time"].ToString());
                    }
                }
                if (dt2 == DateTime.Parse("2018/01/01 00:00:00"))
                {
                    return "未绑定银行卡 R";
                }

                bool b = (dt2 - dt1).Duration().TotalSeconds > new Random().Next(timeSec_min, timeSec_max);
                return b ? "OK" : "同IP其他会员已申请过！R";

            }
            catch (Exception ex)
            {
                //如果 操作超时 重新登录一下GPK
                string msg = "查询操作历史记录失败 " + ex.Message;
                appSittingSet.txtLog(msg);
                return "错误没有资料";
            }
        }

        /// <summary>
        /// 查询用户详细信息
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public static Gpk_UserDetail GetUserDetail(string username)
        {
            Gpk_UserDetail dd = null;

            try
            {

                string postUrl = "Member/GetDetail";
                string postData = "{\"account\":\"" + username + "\"}";
                string postRefere = "MemberDeposit";
                JObject jo = GetResponse<JObject>(postUrl, postData, "POST", postRefere);

                if (jo["Member"] == null)
                {
                    appSittingSet.txtLog(postData + " 没有记录");
                }
                else
                {
                    dd = new Gpk_UserDetail() { };
                    dd.Account = jo["Member"]["Account"].ToString();
                    dd.Birthday= jo["Member"]["Birthday"].ToString();
                    dd.Email = jo["Member"]["Email"].ToString();
                    dd.Id = jo["Member"]["Id"].ToString();
                    dd.Mobile = jo["Member"]["Mobile"].ToString();
                    dd.SexString = jo["Member"]["SexString"].ToString();
                    dd.Wallet = decimal.Parse(jo["Member"]["Wallet"].ToString());
                    if (jo["Member"]["LatestLogin"]!=null && jo["Member"]["LatestLogin"].ToString().Length>0)
                    {
                        dd.LatestLogin_IP = jo["Member"]["LatestLogin"]["IP"].ToString();
                        dd.LatestLogin_time = jo["Member"]["LatestLogin"]["Time"].ToString();
                        dd.LatestLogin_Id = jo["Member"]["LatestLogin"]["Id"].ToString();
                    }
                    if (jo["Member"]["BankAccount"]!=null && jo["Member"]["BankAccount"].ToString().Length>3)
                    {
                        dd.BankAccount = jo["Member"]["BankAccount"]["Account"].ToString().Replace("'", ""); ;
                        dd.BankName = jo["Member"]["BankAccount"]["BankName"].ToString().Replace("'", ""); ;
                        dd.City = jo["Member"]["BankAccount"]["City"].ToString().Replace("'","");
                        dd.Province = jo["Member"]["BankAccount"]["Province"].ToString().Replace("'", ""); ;
                        dd.BankMemo = jo["Member"]["BankAccount"]["Memo"].ToString().Replace("'", ""); ;
                    }
                    if (jo["Member"]["RegisterInfo"]!=null && jo["Member"]["RegisterInfo"].ToString().Length>3)
                    {
                        dd.RegisterDevice = jo["Member"]["RegisterInfo"]["RegisterDevice"].ToString();
                        dd.RegisterUrl = jo["Member"]["RegisterInfo"]["RegisterUrl"].ToString();
                    }
                }

                return dd;
            }
            catch (Exception ex)
            {
                //如果 操作超时 重新登录一下GPK
                string msg = "查询账户信息详细信息失败，用户为： " + username + " " + ex.Message;
                appSittingSet.txtLog(msg);
                return dd;
            }
            //try
            //{
            //    string sql = " INSERT INTO detail(Account,Birthday,Email,Id,Mobile,Sex,Wallet,LatestLogin_IP,LatestLogin_time,LatestLogin_Id,BankAccount,BankName,City,Province,BankMemo,RegisterDevice,RegisterUrl  ) VALUES('"+dd.Account+"','"+dd.Birthday + "','"+dd.Email + "','"+dd.Id + "','"+dd.Mobile + "','"+dd.SexString + "',"+dd.Wallet + ",'"+dd.LatestLogin_IP + "','"+dd.LatestLogin_time + "','"+dd.LatestLogin_Id + "','"+dd.BankAccount + "','"+dd.BankName + "','"+ dd.City + "','"+dd.Province + "','"+dd.BankMemo + "','"+dd.RegisterDevice + "', '"+dd.RegisterUrl + "'  );";
            //    //appSittingSet.txtLog(sql);
            //    bool f = appSittingSet.execSql(sql);
            //    return f;
            //}
            //catch (Exception ex)
            //{
            //    appSittingSet.txtLog(ex.Message);
            //    return false;
            //}
        }

        /// <summary>
        /// 充值密码
        /// </summary>
        /// <param name="user"></param>
        public static void ResetPassword(Gpk_UserDetail user)
        {
            try
            {
                string postUrl = "Member/ResetPassword";
                string postData = "{\"id\":\"" + user.Id + "\"}";
                string postRefere = "MemberDeposit";
                JObject jo = GetResponse<JObject>(postUrl, postData, "POST", postRefere);

                if (jo["Password"] == null)
                {
                    appSittingSet.txtLog(string.Format("{0}密码重置失败", user.Account));
                }
                else
                {
                    appSittingSet.txtLog(string.Format("{0}密码重置为{1}", user.Account, jo["Password"]));               
                }

            }
            catch (Exception ex)
            {
                string msg = user.Account +"重置密码失败" +   " " + ex.Message;
                appSittingSet.txtLog(msg);
            }
        }

        /// <summary>
        /// 人工提出
        /// </summary>
        /// <param name="user"></param>
        public static void WithdrawSubmit(Gpk_UserDetail user)
        {
            try
            {
                string postUrl = "Member/WithdrawSubmit";
                string postData = JsonConvert.SerializeObject(new { id = user.Id, isReal = false, memo = "", money = user.Wallet, password = pwd, type = 7 });
                string postRefere = "MemberDeposit";
                JObject jo = GetResponse<JObject>(postUrl, postData, "POST", postRefere);

                if (jo["Id"] == null)
                {
                    appSittingSet.txtLog(string.Format("{0}提出失败,错误{1}", user.Account,jo["ErrorMessage"]));
                }
                else
                {
                    appSittingSet.txtLog(string.Format("{0}提出成功ID:{1}", user.Account, jo["Id"]));               
                }

            }
            catch (Exception ex)
            {
                string msg = user.Account +"提出失败" +   " " + ex.Message;
                appSittingSet.txtLog(msg);
            }
        }

        /// <summary>
        /// 批量取回
        /// </summary>
        /// <param name="user"></param>
        public static void AllWalletBackMember(Gpk_UserDetail user)
        {
            try
            {
                string postUrl = "Member/AllWalletBackMember";
                string postData = JsonConvert.SerializeObject(new { id = user.Id });
                string postRefere = "MemberDeposit";
                JObject jo = GetResponse<JObject>(postUrl, postData, "POST", postRefere);

                if (jo["Member"] == null)
                {
                    appSittingSet.txtLog(string.Format("{0}全取回失败,错误{1}", user.Account,jo["ErrorMessage"]));
                }
                else
                {
                    appSittingSet.txtLog(string.Format("{0}全取回成功ID:{1}", user.Account, jo["Member"]["Wallet"]));               
                }

            }
            catch (Exception ex)
            {
                string msg = user.Account +"提出失败" +   " " + ex.Message;
                appSittingSet.txtLog(msg);
            }
        }

        /// <summary>
        /// 会员不受区域验证限制
        /// user.SexString设置为 false 打开 true 关闭
        /// </summary>
        /// <param name="user"></param>
        public static void UpdateMemberLoginEveryWhere(Gpk_UserDetail user)
        {
            try
            {
                string postUrl = "Member/UpdateMemberLoginEveryWhere";
                //string postData = "{\"memberId\":" + user.Id + ",\"allow\":" + user.SexString + "}";
                string postData = JsonConvert.SerializeObject(new { memberId = user.Id, allow = user.SexString });
                string postRefere = "MemberDeposit";
                string jo = GetResponse<string>(postUrl, postData, "POST", postRefere);

                if (jo== "true")
                {
                    appSittingSet.txtLog(string.Format("会员编号{0}不受区域验证限制操作成功", user.Id));
                }
                else
                {
                    appSittingSet.txtLog(string.Format("会员编号{0}不受区域验证限制操作失败", user.Id));
                }

            }
            catch (Exception ex)
            {
                string msg = user.Account + "会员不受区域验证限制 " + " " + ex.Message;
                appSittingSet.txtLog(msg);
            }
        }

        /// <summary>
        /// 可跨区登入 
        ///user.SexString设置为 false 打开 true 关闭
        /// </summary>
        /// <param name="user"></param>
        public static void UpdateCrossRegionLogin(Gpk_UserDetail user)
        {
            try
            {
                string postUrl = "Member/UpdateCrossRegionLogin";
                string postData = JsonConvert.SerializeObject(new { memberId = user.Id, allow = user.SexString });
                string postRefere = "MemberDeposit";
                string jo = GetResponse<string>(postUrl, postData, "POST", postRefere);

                if (jo== "true")
                {
                    appSittingSet.txtLog(string.Format("会员编号{0}可跨区登入操作成功", user.Id));
                }
                else
                {
                    appSittingSet.txtLog(string.Format("会员编号{0}可跨区登入操作失败", user.Id));
                }

            }
            catch (Exception ex)
            {
                string msg = user.Account + "可跨区登入 " + " " + ex.Message;
                appSittingSet.txtLog(msg);
            }
        }

        /// <summary>
        /// 保存websocket 数据到数据库 2019年2月27日
        /// </summary>
        public static void SaveSocket2DB()
        {
            string url = "ws://" + url_gpk_base.Replace("http://", "").Replace("/", "") + "/signalr/connect?transport=webSockets&clientProtocol=1.5&connectionToken=" + System.Web.HttpUtility.UrlEncode(connectionToken) + "&connectionData=%5B%7B%22name%22%3A%22mainhub%22%7D%5D&tid=8";
            WebSocketSharp.WebSocket ws = new WebSocketSharp.WebSocket(url);
            ws.Origin = url_gpk_base;
            foreach (System.Net.Cookie item in platGPK.cookie.GetCookies(new Uri(url_gpk_base)))
            {
                ws.SetCookie(new WebSocketSharp.Net.Cookie(item.Name, item.Value, item.Path, item.Domain));
            }
            ws.OnOpen += (sender, e) =>
            {
                //Console.WriteLine("Open");
                appSittingSet.txtLog("websocket is open");
            };
            ws.OnMessage += (sender, e) =>
            {
                //appSittingSet.txtLog(e.Data);
                if (e.Data.Contains("MainHub") && e.Data.Contains("BetRecordQueryCtrl_searchComplete"))
                {
                    JObject jo = JObject.Parse(e.Data);

                    if (jo["M"][0]["M"].ToString() == "BetRecordQueryCtrl_searchComplete")
                    {
                        //保存到数据库
                        string sql = string.Format("INSERT INTO record (username,gamename,subminttime,betno,chargeMoney,pass,msg,aid) VALUES ( '__socket', '__socket', datetime(CURRENT_TIMESTAMP,'localtime'), '{0}', {1}, 0, '{2}', 1002 );", jo["M"][0]["A"][0]["Count"], jo["M"][0]["A"][0]["TotalCommissionable"], jo["M"][0]["A"][0]["TotalPayoff"]);
                        bool b= appSittingSet.execSql(sql);
                        appSittingSet.txtLog(b.ToString());
                        //停止
                        //ws.Close();
                    }
                }
            };

            ws.OnError += (sender, e) =>
            {
                //Console.WriteLine(e.Message);
                appSittingSet.txtLog("websocket error " + e.Message);
            };
            ws.OnClose += (sender, e) =>
            {
                //Console.WriteLine(e.Code);
                appSittingSet.txtLog("websocket is close");
            };

            ws.Connect();
        }

        /// <summary>
        /// 从数据库获取websoket数据 2019年2月27日
        /// </summary>
        /// <returns></returns>
        public static SoketObj_etRecordQuery getSoketDataFromDb(string  aid)
        {
            SoketObj_etRecordQuery so = null;
            DataTable dt = appSittingSet.getDataTableBySql("select  * from record where aid= " + aid + " order by rowid desc limit 1;");
            if (dt.Rows.Count>0)
            {
                 so = new SoketObj_etRecordQuery()
                {
                    Count = (int)dt.Rows[0]["betno"],
                     TotalCommissionable = (decimal)dt.Rows[0]["TotalBetAmount"],
                      TotalPayoff = (decimal)dt.Rows[0]["TotalPayoff"],
                };
            }
            return so;
        }

        /// <summary>
        /// 对比最后两笔是否一样
        /// 是否需要增加一个范围
        /// </summary>
        /// <returns></returns>
        public static object getSoketDataFromDbCompare()
        {
            
            DataTable dt = appSittingSet.getDataTableBySql("select  * from record where aid = 1002 order by rowid desc limit 2;");
            if (dt.Rows.Count==2)
            {
                if (dt.Rows[0]["betno"] == dt.Rows[1]["betno"] && dt.Rows[0]["TotalBetAmount"]== dt.Rows[1]["TotalBetAmount"] && dt.Rows[0]["TotalPayoff"]== dt.Rows[1]["TotalBetAmount"])
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return null;
            }
        }

        public static SoketObj_etRecordQuery getInfoByWebsocket(betData bb)
        {
            SoketObj_etRecordQuery so = null;
            //string token = platGPK.getNegotiate()[0];
            //if (token=="False")
            //{
            //    return null;
            //}

            string url = "ws://" + url_gpk_base.Replace("http://", "").Replace("/","") + "/signalr/connect?transport=webSockets&clientProtocol=1.5&connectionToken=" + System.Web.HttpUtility.UrlEncode(connectionToken)+ "&connectionData=%5B%7B%22name%22%3A%22mainhub%22%7D%5D&tid=8";

            WebSocketSharp.WebSocket ws = new WebSocketSharp.WebSocket(url);
            ws.Origin = url_gpk_base; 
            foreach (System.Net.Cookie item in platGPK.cookie.GetCookies(new Uri(url_gpk_base)))
            {
                ws.SetCookie(new WebSocketSharp.Net.Cookie(item.Name, item.Value, item.Path, item.Domain));
            }
            ws.OnOpen += (sender, e) =>
            {
                //Console.WriteLine("Open");
            };
            ws.OnMessage += (sender, e) =>
            {
                //appSittingSet.txtLog(e.Data);
                if (e.Data.Contains("MainHub") && e.Data.Contains("BetRecordQueryCtrl_searchComplete"))
                {
                    JObject jo = JObject.Parse(e.Data);

                    if (jo["M"][0]["M"].ToString() == "BetRecordQueryCtrl_searchComplete")
                    {
                        so = new SoketObj_etRecordQuery()
                        {
                            Count = (int)jo["M"][0]["A"][0]["Count"],
                            TotalBetAmount = (decimal)jo["M"][0]["A"][0]["TotalBetAmount"],
                            TotalCommissionable = (decimal)jo["M"][0]["A"][0]["TotalCommissionable"],
                            TotalPayoff = (decimal)jo["M"][0]["A"][0]["TotalPayoff"],
                        };
                        //停止
                        ws.Close();
                    }
                }

            };

            ws.OnError += (sender, e) =>
            {
                //Console.WriteLine(e.Message);
            };
            ws.OnClose += (sender, e) =>
            {
                //Console.WriteLine(e.Code);
            };

            ws.Connect();

            object o = BetRecordSearch(bb);

            //if (so!=null)
            //{
            //    return so;
            //}
            //else
            //{

            //}
            return so;
        }


    }
}
