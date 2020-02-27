using BaseFun;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Web;
using WebSocketSharp;

namespace TimoControl
{
    public static class platGPK
    {

        private static string acc { get; set; }
        public static string pwd { get; set; }
        public static string url_gpk_base { get; set; }
        private static string td_cookie { get; set; }
        public static string connectionId { get; set; }
        public static string connectionToken { get; set; }
        public static CookieContainer cookie { get; set; }
        private static string connectionToken_old { get; set; }
        /// <summary>
        /// 获取所有游戏类别
        /// 0 Video 视讯
        /// 1  Sport 体育
        /// 2 Lottery 彩票
        /// 3 Slot 机率
        /// 4 Board 棋牌
        /// 5 Fish 捕鱼
        /// </summary>
        public static List<string> KindCategories { get; set; }
        /// <summary>
        /// WebSocket
        /// </summary>
        public static WebSocketSharp.WebSocket wsk  { get; set; }
        /// <summary>
        /// sokectclient 
        /// </summary>
        public static ClientWebSocket cws { get; set; }
        /// <summary>
        /// websoket_id
        /// </summary>
        public static string socket_id { get; set; }

        public static bool needsocket { get; set; } = false;
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
                appSittingSet.Log("获取配置文件失败" + ex.Message);
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
                            KindCategories = GetKindCategories();

                            //连接socket
                            if (needsocket)
                            {
                                WebSocketConnect(null);
                            }
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
                appSittingSet.Log(string.Format("GPK站登录失败：{0}   ", ex.Message));
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
                //appSittingSet.Log(string.Format("获取到的connectionToken:{0} ; connectionId:{1};", list[0], list[1]));
                return list;

            }
            catch (WebException ex)
            {
                if (ex.HResult == -2146233079)
                {
                    list.Add("False");
                }
                appSittingSet.Log("获取connectionId、connectionToken失败");
                return list;
            }

        }
        /// <summary>
        /// 检查账号、钱包
        /// </summary>
        /// <returns></returns>
        public static betData checkInGPK(betData bb)
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

        /// <summary>
        /// 提交充值到GPK 不适合 改用公共方法 报错 必须同一个请求对象
        /// </summary>
        /// <param name="bb"></param>
        public static bool MemberDepositSubmit(betData bb)
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
                appSittingSet.Log(msg);
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
                appSittingSet.Log(msg);
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
                appSittingSet.Log(ex.Message);
                if (ex.HResult == -2146233079)
                {
                    return true;
                }
                return false;
            }
        }


        /// <summary>
        /// 检查账号 获取转账记录 
        /// 减去12小时 到美东时间 终止时间再加2分钟  13点08分 2019年4月5日 增加
        /// </summary>
        /// <returns></returns>
        public static betData MemberTransactionSearch(betData bb)
        {
            return MemberTransactionSearch(bb,true);
        }

        public static betData MemberTransactionSearch(betData bb,bool calcTime=true)
        {
            string postUrl = "MemberTransaction/Search";
            string postData = "{\"Account\":\"" + bb.username + "\",\"IsReal\":\"" + bb.isReal + "\",\"Types\":" + JsonConvert.SerializeObject(bb.Types);
            //string ss = JsonConvert.SerializeObject(bb.Types);
            if (bb.lastOprTime != null && bb.lastOprTime != "")
            {
                if (calcTime)
                {
                    DateTime d1;
                    DateTime.TryParse(bb.lastOprTime, out d1);
                    bb.lastOprTime = d1.AddMinutes(2).AddHours(-12).ToString("yyyy/MM/dd HH:mm:ss");
                }
                postData = postData + ",\"TimeBegin\":\"" + bb.lastOprTime + "\"";
            }

            if (bb.betTime != null && bb.betTime != "")
            {
                //时间-12 变为美东时间
                if (calcTime)
                {
                    DateTime d2;
                    DateTime.TryParse(bb.betTime, out d2);
                    bb.betTime = d2.AddMinutes(2).AddHours(-12).ToString("yyyy/MM/dd HH:mm:ss");
                }
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

                //第一条记录的数据
                JArray ja = JArray.FromObject(jo["PageData"]);
                decimal amount = 0;
                decimal.TryParse(ja[0]["Amount"].ToString(), out amount);
                if (ja[0]["IsIncome"].ToString().ToLower() == "false")
                {
                    amount = 0 - amount;
                    bb.passed = false;
                }
                else
                {
                    bb.passed = true;
                }
                bb.betMoney = amount;

                decimal subtotal = 0;
                decimal.TryParse(ja[0]["Subtotal"].ToString(), out subtotal);
                bb.subtotal = subtotal;

                bb.level = ja[0]["MemberLevelId"].ToString();//记录层级
                bb.Id = ja[0]["Id"].ToString();//记录ID
                bb.memberId = ja[0]["MemberId"].ToString();//记录MemberId
                bb.lastCashTime = ja[0]["Time"].ToString();
                //bb.PortalMemo = ja[0]["Memo"].ToString();
                //bb.lastCashTime = ja[0]["Time"].ToString().Replace("\\", "").Replace("/Date(", "").Replace(")", "").Replace("/","");//计算的时间不对 需要自己来算
                //bb.lastCashTime = appSittingSet.unixTimeToTime(bb.lastCashTime);//上次存款时间
                //bb.passed = true;
                bb.total_money = totalMoney;
                bb.betTimes = total;
                return bb;
            }
            catch (Exception ex)
            {
                string msg = string.Format("用户 {0} GPK查询转账记录失败 {1}", bb.username, ex.Message);
                appSittingSet.Log(msg);
                return null;
            }
        }

        /// <summary>
        /// 更新会员级别 
        /// 如果开关0 临时存取 Province  或者级别是一样的就不改变了 用RegisterDevice 临时存取新的级别
        /// </summary>
        /// <param name="memberId">会员号</param>
        /// <param name="levelId">级别号</param>
        /// <returns></returns>
        public static bool UpadateMemberLevel(Gpk_UserDetail userinfo)
        {
            try
            {
                //如果开关0 或者级别是一样的就不改变了 用RegisterDevice 临时存取新的级别
                if (userinfo.Province=="0" || userinfo.MemberLevelSettingId==userinfo.RegisterDevice)
                {
                    return true;
                }
                string postUrl = "Member/UpdateMemberLevel";
                string postData = "{\"memberId\":" + userinfo.Id + ",\"levelId\":" + userinfo.RegisterDevice + "}";
                string postRefere = "MemberDeposit";
                HttpStatusCode r = GetResponse<HttpStatusCode>(postUrl, postData, "POST", postRefere);
                //if (r==HttpStatusCode.OK)
                //    return true;
                //else
                //    appSittingSet.txtLog("用户" + userinfo.Account + "更新等级失败，需手动更新");
                //    return false;

                bool b = r == HttpStatusCode.OK ? true : false;
                if (!b)
                    appSittingSet.Log("用户" + userinfo.Account + "更新等级失败，需手动更新");
                return b;

            }
            catch (Exception ex)
            {
                appSittingSet.Log("用户"+userinfo.Account+"更新等级失败 " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 查询账户详细信息
        /// </summary>
        /// <param name="Account"></param>
        /// <returns></returns>
        //public static betData MemberGetDetail(betData bb)
        //{
        //    try
        //    {
        //        string postUrl = "Member/GetDetail";
        //        string postData = "{\"account\":\"" + bb.username + "\"}";
        //        string postRefere = "MemberDeposit";
        //        JObject jo = GetResponse<JObject>(postUrl, postData, "POST", postRefere);

        //        decimal d = 0;
        //        decimal.TryParse(jo["Member"]["Wallet"].ToString(), out d);
        //        bb.subtotal = d;
        //        return bb;
        //    }
        //    catch (Exception ex)
        //    {
        //        //如果 操作超时 重新登录一下GPK
        //        string msg = "查询账户信息(钱包)失败 " + ex.Message;
        //        appSittingSet.Log(msg);
        //        return null;
        //    }
        //}

        /// <summary>
        /// 查询是否有注单记录
        /// true 没有记录 false 曾在记录
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
                if (bb.GameTypeName != null)
                {
                    postData = postData + "\"GameTypeName\":\"" + bb.GameTypeName + "\",\"GameTypeNameIsLike\":false,";
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
                appSittingSet.Log(msg);
                return null;
            }
        }


        /// <summary>
        /// 获取 投注记录查询 没有开放这个接口 (gpk 关闭)
        /// </summary>
        /// <param name="bb"></param>
        /// <returns></returns>
        public static SoketObjetRecordQuery BetRecordGetInfo(betData bb)
        {
            try
            {
                string postUrl = "BetRecord/GetInfo";
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
                SoketObjetRecordQuery obj = new SoketObjetRecordQuery() { };
                if (jo != null)
                {
                    obj.Count = (int)jo["Count"];
                    obj.TotalBetAmount = (decimal)jo["TotalBetAmount"];
                    obj.TotalCommissionable = (decimal)jo["TotalCommissionable"];
                    obj.TotalPayoff = (decimal)jo["TotalPayoff"];
                }
                return obj;
            }
            catch (Exception ex)
            {
                //如果 操作超时 重新登录一下GPK
                string msg = "查询投注记录总数错误 " + ex.Message;
                appSittingSet.Log(msg);
                return null;
            }
        }





        /// <summary>
        /// 获取账户列表 到数据库 无用到
        /// "2019/03/01"
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public static List<Gpk_UserInfo> getUserListByPage(int pageIndex ,bool TDB,string JoinMemberBegin)
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
                     postdata = JsonConvert.SerializeObject(new { JoinMemberBegin=JoinMemberBegin, connectionId= connectionId });
                    //postdata = "{\"BalanceBegin\":\"1\",\"State\":\"1\",\"connectionId\":\"" + connectionId + "\"}";
                }
                else
                {
                    postdata = JsonConvert.SerializeObject(new { JoinMemberBegin = JoinMemberBegin, connectionId = connectionId, pageIndex = pageIndex - 1 });
                    //postdata = "{\"BalanceBegin\":\"1\",\"State\":\"1\",\"pageIndex\":"+(pageIndex-1).ToString()+",\"connectionId\":\""+connectionId+"\"}";
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
                    appSittingSet.Log(postdata + " 没有交易记录");
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
                    info1.MemberLevelSettingName = item["MemberLevelSettingId"].ToString();
                    info1.Name = item["Name"].ToString();

                    list.Add(info1);
                    if (TDB)
                    {
                        //生成sql语句
                        sb.Append("INSERT INTO infor ( Account, Balance, JoinTime, MemberLevelSettingName, Name ) VALUES( '" + info1.Account + "', " + info1.Balance + ", '" + info1.JoinTime + "', '" + info1.MemberLevelSettingName + "',  '" + info1.Name + "' ); ");
                    }
                }
                if (TDB)
                {
                    try
                    {
                        //插入到数据库
                        bool f = SQLiteHelper.SQLiteHelper.execSql(sb.ToString());
                        appSittingSet.Log("已经操作页数" + pageIndex);
                    }
                    catch (Exception ex)
                    {
                        appSittingSet.Log(ex.Message);
                    }
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

                appSittingSet.Log(msg);
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
                //string postdata = string.Format("?account={0}&begin={1}&end={2}&isMember=true&orderBy=Commissionable&reverse=true&skip=0&take=100&types=BBINprobability&types=BBINFish30&types=BBINFish38&types=AgEbr&types=AgHsr&types=AgYoPlay&types=Mg2Slot&types=Mg2Html5&types=Pt2Slot&types=GpiSlot3D&types=GpiSlotR&types=GnsSlot&types=PrgSlot&types=SgSlot&types=Rg2Fish&types=Rg2Slot&types=JdbSlot&types=JdbFish&types=HabaSlot&types=Cq9Slot&types=Cq9Fish&types=NetEntSlot&types=GdSlot&types=Pt3Slot&types=RedTigerSlot&types=GameArtSlot&types=Mw2Slot&types=PgSlot&types=RedTiger2Slot&types=LgVirtualSport&types=Mg3Slot&types=IsbSlot&types=PtsSlot&types=PngSlot&types=City761Fish&types=FsSlot&types=FsFish&types=FsArcade&types=KaSlot&types=JsSlot&types=JsFish&types=GtiSlot&types=PlsSlot&types=AeSlo", bb.username, DateTime.Now.AddDays(-DateTime.Now.Day + 1).ToString("yyyy/MM/dd"), DateTime.Now.Date.ToString("yyyy/MM/dd"));
                string postdata =string.Format("?_={4}&account={0}&begin={1}&end={2}&isMember=true&orderBy=Commissionable&reverse=true&skip=0&take=100{3}", bb.username, bb.lastCashTime,bb.lastOprTime, bb.gamename,appSittingSet.GetTimeStamp(13));
                request = WebRequest.Create(url_gpk_base + "Statistics/GetDetailInfo" + postdata) as HttpWebRequest;
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
                if (ret_html=="")
                {
                    return null;
                }
                JObject jo = JObject.Parse(ret_html);
                //如果为空 
                if ((bool)jo["IsSuccess"] == false)
                {
                    appSittingSet.Log(postdata + " 没有记录"+ jo["ErrorMessage"]);
                    bb.passed = false;
                    return bb;
                }
                JArray ja = JArray.FromObject(jo["ReturnObject"]);
                decimal amount = 0;
                decimal.TryParse(ja[0]["Commissionable"].ToString(), out amount);
                bb.total_money = amount;
                bb.Commissionable = amount;
                bb.BetRecordCount = Convert.ToInt32(ja[0]["BetRecordCount"]);
                decimal.TryParse(ja[0]["BetAmount"].ToString(), out amount);
                bb.BetAmount = amount;
                decimal.TryParse(ja[0]["Payoff"].ToString(), out amount);
                bb.Payoff = amount;
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
                appSittingSet.Log(msg);
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
        /// 公共方法 没有改
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

                //request.ServicePoint.Expect100Continue = false;
                request.ProtocolVersion = HttpVersion.Version11;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

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
                    appSittingSet.Log(postUrl + "-" + postData + " 没有记录");
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
                string msg = "失败-" + postUrl + "-" +ex.HResult +"-" +ex.Status +"-"+ ex.Message;
                //if (ex.HResult == -2146233079 || ex.Message == "操作超时") || ex.Status==WebExceptionStatus.ConnectFailure
                if ((ex.Status == WebExceptionStatus.ProtocolError || ex.Status == WebExceptionStatus.Timeout ) && !postUrl.Contains("UpdateMemberLevel"))
                {
                    if (request != null)
                    {
                        request.Abort();
                    }
                    //需要重新登录 更新级别导致错误几率很大 不需要重新登陆
                    loginGPK();
                    //WebSocketConnect();
                    msg = msg + "-已经重新登录账号";
                }
                appSittingSet.Log(msg);
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
        /// 查询操作历史记录 关键字查询 注册和开户时间间隔
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string GetUserLoadHistory(Gpk_UserDetail userinfo,string keywords, int seed)
        {
            try
            {
                DateTime dt1 = DateTime.Parse("2018/01/01 00:00:00");
                DateTime dt2 = DateTime.Parse("2018/01/01 00:00:00");

                List<HisToryInfor> list = MemberLoadHistory(userinfo, keywords);
                if (list.Count<2)
                {
                    return "未绑定银行卡 R";
                }
                else
                {
                    dt1 = DateTime.Parse(list[1].Time);
                    dt2 = DateTime.Parse(list[0].Time);
                }



                //foreach (var item in list)
                //{
                //    if (item["Content"].ToString() == "申请加入会员")
                //    {
                //        dt1 = DateTime.Parse(item["Time"].ToString());
                //    }
                //    if (item["Content"].ToString() == "建立银行帐户资讯")
                //    {
                //        dt2 = DateTime.Parse(item["Time"].ToString());
                //    }
                //}
                //if (dt2 == DateTime.Parse("2018/01/01 00:00:00"))
                //{
                //    return "未绑定银行卡 R";
                //}

                bool b = (dt2 - dt1).Duration().TotalSeconds > seed;
                return b ? "OK" : "同IP其他会员已申请过！R";

            }
            catch (Exception ex)
            {
                //如果 操作超时 重新登录一下GPK
                string msg = "查询操作历史记录失败 " + ex.Message;
                appSittingSet.Log(msg);
                return "ERR错误没有资料";
            }
        }

        /// <summary>
        /// 查询操作历史记录 关键字查询 
        /// </summary>
        /// <param name="userinfo"></param>
        /// <param name="keywords"></param>
        /// <returns></returns>
        public static List<HisToryInfor> MemberLoadHistory(Gpk_UserDetail userinfo, string keywords)
        {
            try
            {
                string postUrl = "Member/LoadHistory";
                //string postData = "{\"id\":" + id + ",\"take\":100,\"skip\":0,\"query\":{}}";
                string postData = JsonConvert.SerializeObject(new { id = userinfo.Id, take = 100, skip = 0, query = "{}" });
                if (keywords != "")
                {
                    postData = JsonConvert.SerializeObject(new { id = userinfo.Id, take = 100, skip = 0, query = new { Include = keywords } });
                }

                string postRefere = "Member/" + userinfo.Account + "/History";
                JArray ja = GetResponse<JArray>(postUrl, postData, "POST", postRefere);
                List<HisToryInfor> list = new List<HisToryInfor>();
                HisToryInfor his = null;
                foreach (var item in ja)
                {
                    his = new HisToryInfor() { Content = item["Content"].ToString(), Display = item["Display"].ToString(), IP = item["IP"].ToString(), Time = item["Time"].ToString() };
                    list.Add(his);
                }
                return list;

            }
            catch (Exception ex)
            {
                //如果 操作超时 重新登录一下GPK
                string msg = "查询操作历史记录失败 " + ex.Message;
                appSittingSet.Log(msg);
                return null;
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

                string postUrl = "Member/GetDetail";
                string postData = "{\"account\":\"" + username + "\"}";
                string postRefere = "MemberDeposit";
                JObject jo = GetResponse<JObject>(postUrl, postData, "POST", postRefere);

                if (jo != null && jo["Member"] != null)
                {
                    dd = new Gpk_UserDetail() { };
                    dd.Account = jo["Member"]["Account"].ToString();
                    dd.Birthday= jo["Member"]["Birthday"].ToString();
                    dd.Email = jo["Member"]["Email"].ToString();
                    dd.Id = jo["Member"]["Id"].ToString();
                    dd.Mobile = jo["Member"]["Mobile"].ToString();
                    dd.SexString = jo["Member"]["SexString"].ToString();
                    dd.Wallet = decimal.Parse(jo["Member"]["Wallet"].ToString());
                    dd.MemberLevelSettingId = jo["Member"]["MemberLevelSettingId"].ToString();
                    dd.Name = jo["Member"]["Name"].ToString();
                    dd.QQ = jo["Member"]["QQ"].ToString();
                    dd.JoinTime = DateTime.Parse(jo["Member"]["JoinTime"].ToString());//注册时间 在反序列化已经转换了
                    //dd.JoinTime = appSittingSet.unixTimeToTime( jo["Member"]["JoinTime"].ToString());//注册时间

                    //17点00分 2019年7月6日 增加 timo
                    dd.Balance = decimal.Parse(jo["Member"]["Balance"].ToString());
                    //dd.YuebaoPrincipal = decimal.Parse(jo["Member"]["YuebaoPrincipal"].ToString());//15点49分取消功能

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

        /// <summary>
        /// 重置用户密码
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
                    appSittingSet.Log(string.Format("{0}密码重置失败", user.Account));
                }
                else
                {
                    appSittingSet.Log(string.Format("{0}密码重置为{1}", user.Account, jo["Password"]));               
                }

            }
            catch (Exception ex)
            {
                string msg = user.Account +"重置密码失败" +   " " + ex.Message;
                appSittingSet.Log(msg);
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
                    appSittingSet.Log(string.Format("{0}提出失败,错误{1}", user.Account,jo["ErrorMessage"]));
                }
                else
                {
                    appSittingSet.Log(string.Format("{0}提出成功ID:{1}", user.Account, jo["Id"]));               
                }

            }
            catch (Exception ex)
            {
                string msg = user.Account +"提出失败" +   " " + ex.Message;
                appSittingSet.Log(msg);
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
                    appSittingSet.Log(string.Format("{0}全取回失败,错误{1}", user.Account,jo["ErrorMessage"]));
                }
                else
                {
                    appSittingSet.Log(string.Format("{0}全取回成功ID:{1}", user.Account, jo["Member"]["Wallet"]));               
                }

            }
            catch (Exception ex)
            {
                string msg = user.Account +"提出失败" +   " " + ex.Message;
                appSittingSet.Log(msg);
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
                    appSittingSet.Log(string.Format("会员编号{0}不受区域验证限制操作成功", user.Id));
                }
                else
                {
                    appSittingSet.Log(string.Format("会员编号{0}不受区域验证限制操作失败", user.Id));
                }

            }
            catch (Exception ex)
            {
                string msg = user.Account + "会员不受区域验证限制 " + " " + ex.Message;
                appSittingSet.Log(msg);
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

                //if (jo== "true")
                //    appSittingSet.Log(string.Format("会员{0}可跨区登入操作成功", user.Account));
                //else
                //    appSittingSet.Log(string.Format("会员{0}可跨区登入操作失败", user.Account));

                if (jo!="true")
                    appSittingSet.Log(string.Format("会员{0}可跨区登入操作失败", user.Account));
            }
            catch (Exception ex)
            {
                string msg = user.Account + "可跨区登入 " + " " + ex.Message;
                appSittingSet.Log(msg);
            }
        }
        /*
        /// <summary>
        /// 保存websocket 数据到数据库 
        /// 异步方法 2019年2月27日
        /// </summary>
        public static void SaveSocket2DB()
        {
            if (wsk==null || connectionToken != connectionToken_old ||wsk.IsAlive== false)
            {
                wsk= WebSocketConnect();
            }

            if (connectionToken != connectionToken_old)
            {
                wsk = WebSocketConnect();
            }

            if (wsk==null)
            {
                wsk = WebSocketConnect();
            }
            else
            {
                if (wsk.IsAlive == false)
                {
                    wsk = WebSocketConnect();
                }
            }
        }
        */

        /// <summary>
        /// 连接websocket
        /// </summary>
        /// <returns></returns>
        public static WebSocketSharp.WebSocket WebSocketConnect( betData bb)
        {
            //先断开已经有得
            //if (wsk!=null)
            //{
            //    wsk.Close(CloseStatusCode.Normal, "主动关闭");
            //    wsk = null;
            //}
            if (bb == null)
                bb = new betData() { aid = "0" };
            //else
            //    b.aid.Replace("1002", "");

            string url = "ws://" + url_gpk_base.Replace("http://", "").Replace("/", "") + "/signalr/connect?transport=webSockets&clientProtocol=1.5&connectionToken=" + System.Web.HttpUtility.UrlEncode(connectionToken) + "&connectionData=%5B%7B%22name%22%3A%22mainhub%22%7D%5D&tid=8";
            WebSocketSharp.WebSocket ws = new WebSocketSharp.WebSocket(url);
            //ws.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls11 | System.Security.Authentication.SslProtocols.Tls; //加了会报错 This instance does not use a secure connection.
            ws.Origin = url_gpk_base;
            foreach (Cookie item in cookie.GetCookies(new Uri(url_gpk_base)))
            {
                ws.SetCookie(new WebSocketSharp.Net.Cookie(item.Name, item.Value, item.Path, item.Domain));
            }
            ws.OnOpen += (sender, e) =>
            {
                appSittingSet.Log("websocket is open");
                connectionToken_old = connectionToken;
            };
            ws.OnMessage += (sender, e) =>
            {
                if (e.Data.Contains("MainHub") && e.Data.Contains("BetRecordQueryCtrl_searchComplete"))
                {
                    JObject jo = JObject.Parse(e.Data);

                    if (jo["M"][0]["M"].ToString() == "BetRecordQueryCtrl_searchComplete")
                    {
                        //保存到数据库
                        betData b = new betData() {
                            username = "__socket",
                            gamename = socket_id,
                            betno = jo["M"][0]["A"][0]["Count"].ToString(),
                            betMoney = (decimal)jo["M"][0]["A"][0]["TotalCommissionable"],
                            passed = false,
                            msg = jo["M"][0]["A"][0]["TotalPayoff"].ToString(),
                            aid = "1002"+bb.aid,
                            bbid = "-"+DateTime.Now.Millisecond.ToString() + new Random().Next(100, 999)//随机值
                        };
                        string sql = $"insert  or ignore into record (username, gamename,betno,chargeMoney,pass,msg,subtime,aid,bbid) values ('{ b.username}', '{ b.gamename}','{b.betno }',{ b.betMoney },{(b.passed == true ? 1 : 0) },'{ b.msg }','{DateTime.Now.AddHours(-12).ToString("yyyy-MM-dd HH:mm:ss") }' , {b.aid},{b.bbid})";
                        SQLiteHelper.SQLiteHelper.execSql(sql);
                    }
                }
            };

            ws.OnError += (sender, e) =>
            {
                appSittingSet.Log("websocket error " + e.Message);
            };
            ws.OnClose += (sender, e) =>
            {

                //loginGPK();
                //ws.Connect();
                appSittingSet.Log("websocket is close " + e.Code + " " + e.Reason + "状态" + ws.IsAlive);
                //wsk = null;
                //SaveSocket2DB();
                //WebSocketConnect();
            };

            //if (!ws.IsAlive)
            //{
            //    ws.Connect();
            //}
            //ws.EmitOnPing = true;
            //ws.Ping();
            ws.Connect();
            ws.Log.Level = LogLevel.Debug;
            ws.Log.File = appSittingSet.logPath+"\\socket"+ DateTime.Now.Date.ToString("yyyyMMdd") + ".txt";
            //if (wsk!=null && wsk.ReadyState!= WebSocketSharp.WebSocketState.Closed)
            //{
            //    wsk.Close();
            //    wsk = null;
            //}
            wsk = ws;
            return ws;
        }
        /*不用代码
        /// <summary>
        /// websocketclient server 2008 不支持 
        /// </summary>
        public static async void WebSocketConnect2()
        {
            try
            {
                string url = "ws://" + platGPK.url_gpk_base.Replace("http://", "").Replace("/", "") + "/signalr/connect?transport=webSockets&clientProtocol=1.5&connectionToken=" + System.Web.HttpUtility.UrlEncode(platGPK.connectionToken) + "&connectionData=%5B%7B%22name%22%3A%22mainhub%22%7D%5D&tid=8";
                ClientWebSocket client = new ClientWebSocket();
                client.Options.Cookies = platGPK.cookie;
                //client.Options.SetRequestHeader("Connection", "keep-alive");
                client.Options.KeepAliveInterval = TimeSpan.FromHours(1);
                client.ConnectAsync(new Uri(url), CancellationToken.None).Wait();
                cws = client;
                //client.State== WebSocketState.
                //string line="Ok";
                //var array = new ArraySegment<byte>(Encoding.UTF8.GetBytes(line));
                //client.SendAsync(array, WebSocketMessageType.Text, true, CancellationToken.None);
                if (client.State == System.Net.WebSockets.WebSocketState.Open)
                {
                        appSittingSet.Log("websocket open");
                }

                while (true)
                {
                    appSittingSet.Log("websocket state"+ client.State);
                    //if (client.State == System.Net.WebSockets.WebSocketState.CloseReceived)
                    //{
                    //    appSittingSet.Log("websocket close");
                    //}
                    var array = new byte[4096];
                    var result = await client.ReceiveAsync(new ArraySegment<byte>(array), CancellationToken.None);
                    //Console.WriteLine(result.CloseStatus);
                    if (result.MessageType == WebSocketMessageType.Text)
                    {

                        string msg = Encoding.UTF8.GetString(array, 0, result.Count);
                        //Console.WriteLine("--> {0}", msg);

                        if (msg.Contains("MainHub") && msg.Contains("BetRecordQueryCtrl_searchComplete"))
                        {
                            JObject jo = JObject.Parse(msg);

                            if (jo["M"][0]["M"].ToString() == "BetRecordQueryCtrl_searchComplete")
                            {
                                //保存到数据库
                                betData b = new betData()
                                {
                                    username = "__socket",
                                    gamename = socket_id,
                                    betno = jo["M"][0]["A"][0]["Count"].ToString(),
                                    betMoney = (decimal)jo["M"][0]["A"][0]["TotalCommissionable"],
                                    passed = false,
                                    msg = jo["M"][0]["A"][0]["TotalPayoff"].ToString(),
                                    aid = "1002",
                                    bbid = DateTime.Now.Millisecond.ToString() + new Random().Next(100, 999)//随机值
                                };
                                string sql = $"insert  or ignore into record (username, gamename,betno,chargeMoney,pass,msg,subtime,aid,bbid) values ('{ b.username}', '{ b.gamename}','{b.betno }',{ b.betMoney },{(b.passed == true ? 1 : 0) },'{ b.msg }','{DateTime.Now.AddHours(-12).ToString("yyyy-MM-dd HH:mm:ss") }' , {b.aid},{b.bbid})";
                                SQLiteHelper.SQLiteHelper.execSql(sql);
                                //appSittingSet.recorderDb(b);
                                //string sql = string.Format("INSERT INTO record (username,gamename,subtime,betno,chargeMoney,pass,msg,aid) VALUES ( '__socket', '{3}', datetime(CURRENT_TIMESTAMP,'localtime'), '{0}', {1}, 0, '{2}', 1002 );", jo["M"][0]["A"][0]["Count"], jo["M"][0]["A"][0]["TotalCommissionable"], jo["M"][0]["A"][0]["TotalPayoff"], socket_id);
                                //bool b = appSittingSet.execSql(sql);
                            }
                        }
                    }
                    //else if (result.MessageType == WebSocketMessageType.Close)
                    //{
                    //    await client.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                    //}
                }
            }
            catch (System.Net.WebSockets.WebSocketException ex)
            {
                appSittingSet.Log(ex.WebSocketErrorCode + ex.Message);
            }

        }
        */
        /// <summary>
        /// 从数据库获取websoket数据 2019年2月27日
        /// </summary>
        /// <returns></returns>
        public static SoketObjetRecordQuery getSoketDataFromDb(string  aid)
        {
            SoketObjetRecordQuery so = null;
            string sql = $"select  * from record where gamename = '{socket_id}' and aid={"1002" + aid} order by rowid desc limit 1;";
            //string sql= $"select  * from record where username='__socket'  and aid={"1002" + aid} order by rowid desc limit 1;";
            DataTable dt =SQLiteHelper.SQLiteHelper.getDataTableBySql(sql);
            if (dt.Rows.Count>0)
            {
                 so = new SoketObjetRecordQuery()
                {
                    Count =int.Parse(dt.Rows[0]["betno"].ToString()),
                     TotalCommissionable =decimal.Parse(  dt.Rows[0]["chargeMoney"].ToString()),
                      TotalPayoff = decimal.Parse(dt.Rows[0]["msg"].ToString()),
                };
            }
            return so;
        }

        /// <summary>
        /// 对比最后两笔是否一样 第一次全部 第二次 几率和捕鱼
        /// 是否需要增加一个范围
        /// </summary>
        /// <returns></returns>
        public static object getSoketDataFromDbCompare( out decimal chargeMoney,string aid)
        {
            int count1,count2 = 0;
            decimal TotalBetAmount1, TotalBetAmount2 = 0;
            decimal TotalPayoff1, TotalPayoff2 = 0;
            string sql = $"select  * from record where gamename = '{socket_id}' and aid={"1002"+aid} order by rowid desc limit 2;";
            DataTable dt = SQLiteHelper.SQLiteHelper.getDataTableBySql(sql);
            //2条记录 并且为同一组
            if (dt.Rows.Count == 2 )
            {
                count1 =Convert.ToInt16(dt.Rows[0]["betno"]);
                count2 = Convert.ToInt16(dt.Rows[1]["betno"]);
                TotalBetAmount1 = Convert.ToDecimal(dt.Rows[0]["chargeMoney"]);
                TotalBetAmount2 = Convert.ToDecimal(dt.Rows[1]["chargeMoney"]);
                TotalPayoff1 = Convert.ToDecimal(dt.Rows[0]["msg"]);
                TotalPayoff2 = Convert.ToDecimal(dt.Rows[1]["msg"]);
                if ((count1 > count2 - 10 && count1 < count2 + 10) && (TotalBetAmount1 > TotalBetAmount2 - 10 && TotalBetAmount1 < TotalBetAmount2 + 10) && (TotalPayoff1 > TotalPayoff2 - 10 && TotalPayoff1 < TotalPayoff2 + 10))
                {
                    chargeMoney =Math.Abs( TotalPayoff1);
                    return true;
                }
                else
                {
                    chargeMoney = Math.Abs(TotalPayoff1);
                    return false;
                }
            }
            else
            {
                chargeMoney = 0;
                return null;
            }
        }

        /// <summary>
        /// 获取所有游戏类别
        /// 0 Video 视讯
        /// 1  Sport 体育
        /// 2 Lottery 彩票
        /// 3 Slot 机率
        /// 4 Board 棋牌
        /// 5 Fish 捕鱼
        /// </summary>
        /// <returns></returns>
        public static List<string> GetKindCategories()
        {
            //StringBuilder GameCategories_all = new StringBuilder();
            //StringBuilder GameCategories_ele = new StringBuilder();
            List<string> list = new List<string>();
            StringBuilder sb;
            JArray ja = GetResponse<JArray>("BetRecord/GetKindCategories", "", "POST", "BetRecord");
            if (ja == null)
            {
                return list;
            }

            foreach (var c in ja)
            {
                JArray jab = JArray.FromObject(c["Categories"]);
                sb = new StringBuilder();
                foreach (var item in jab)
                {
                    //if (c["Value"].ToString() == "Slot" || c["Value"].ToString() == "Fish")
                    //{
                    //    GameCategories_ele.AppendFormat("\"{0}\",", item["PropertyName"].ToString());
                    //}
                    //GameCategories_all.AppendFormat("\"{0}\",", item["PropertyName"].ToString());
                    //list_all.Add(item["PropertyName"].ToString());
                    sb.AppendFormat("\"{0}\",", item["PropertyName"].ToString());
                }

                list.Add(sb.Remove(sb.Length - 1, 1).ToString());
            }
            //list.Add(GameCategories_all.Remove(GameCategories_all.Length - 1, 1).ToString());
            //list.Add(GameCategories_ele.Remove(GameCategories_ele.Length - 1, 1).ToString());
            return list;
        }

        public static List<List<string>> GetKindCategories2()
        {
            List<List<string>> list = new List<List<string>>();
            
            StringBuilder sb;
            JArray ja = GetResponse<JArray>("BetRecord/GetKindCategories", "", "POST", "BetRecord");
            if (ja == null)
            {
                return list;
            }

            foreach (var c in ja)
            {
                JArray jab = JArray.FromObject(c["Categories"]);
                sb = new StringBuilder();
                List<string> list_in = new List<string>();
                foreach (var item in jab)
                {
                    sb.AppendFormat("\"{0}\",", item["PropertyName"].ToString());
                    list_in.Add(item["PropertyName"].ToString());
                }
                list.Add(list_in);
            }
            return list;
        }

        /// <summary>
        /// 修改会员状态 
        ///user.SexString设置为 false 打开 true 关闭
        /// </summary>
        /// <param name="user"></param>
        public static void MemberUpdateMemberState(Gpk_UserDetail userinfo)
        {
            try
            {
                string postUrl = "Member/UpdateMemberState";
                string postData = JsonConvert.SerializeObject(new { id = userinfo.Id, state = userinfo.SexString });
                string postRefere = "Member/"+userinfo.Account;
                HttpStatusCode r = GetResponse<HttpStatusCode>(postUrl, postData, "POST", postRefere);

                bool b = r == HttpStatusCode.OK ? true : false;
                if (!b)
                    appSittingSet.Log("用户" + userinfo.Account + "编号" + userinfo.Id + "更新状态失败，需手动更新");
                //return b;

            }
            catch (Exception ex)
            {
                string msg = "用户" + userinfo.Account + "编号" + userinfo.Id + "更新状态失败 " +  ex.Message;
                appSittingSet.Log(msg);
            }
        }

        /// <summary>
        /// 发送站内信
        /// </summary>
        /// <param name="mail"></param>
        public static void SiteMailSendMail(SendMailBody mail)
        {
            try
            {
                string postUrl = "SiteMail/SendMail";
                string postData = JsonConvert.SerializeObject(new { BatchParam = mail.BatchParam, ExcelFilePath = mail.ExcelFilePath, MailBody = mail.MailBody, MailRecievers = mail.MailRecievers, ResendMailID = mail.ResendMailID, SearchParam = mail.SearchParam, SendMailType = mail.SendMailType, Subject = mail.Subject, SuperSearchRequest = mail.SuperSearchRequest });
                string postRefere = "SiteMail/Send";
                HttpStatusCode r = GetResponse<HttpStatusCode>(postUrl, postData, "POST", postRefere);

                bool b = r == HttpStatusCode.OK ? true : false;
                if (!b)
                    appSittingSet.Log("站内信发送成功" + mail.MailRecievers);
                //return b;

            }
            catch (Exception ex)
            {
                string msg = "站内信发送失败" + mail.MailRecievers +"-"+ ex.Message;
                appSittingSet.Log(msg);
            }
        }

        public static bool RedEnvelopeManagement_GetExcelSum(string filename,Dictionary<string,string> postData)
        {
            bool flag = false;
            HttpWebRequest request = null;
            HttpWebResponse response = null;
            StreamReader reader = null;
            try
            {
                //提交数据
                request = WebRequest.Create(url_gpk_base + "RedEnvelopeManagement/AddRedEnvelope") as HttpWebRequest;
                request.Method = "POST";
                request.UserAgent = "Mozilla/4.0";
                request.KeepAlive = true;
                request.Accept = "application/json, text/plain, */*";
                //request.ContentType = "multipart/form-data; boundary=----WebKitFormBoundaryX4DPbIHLA2CmUH0w";//发送的是json数据 注意
                request.Host = url_gpk_base.Replace("http://", "").Replace("/", "");
                request.Referer = url_gpk_base + "/RedEnvelopeManagement/Create";
                request.Headers.Add("Origin", url_gpk_base);
                request.Headers.Add("X-Requested-With", "XMLHttpRequest");

                //设置请求头、cookie
                request.CookieContainer = cookie;


                //2

                string boundary = DateTime.Now.Ticks.ToString("X"); // 随机分隔线
                boundary = "----WebKitFormBoundarybDKJa9ZpJWmQg1xV";
                request.ContentType = "multipart/form-data; boundary=" + boundary;
                byte[] itemBoundaryBytes = Encoding.UTF8.GetBytes("\r\n--" + boundary + "\r\n");
                byte[] endBoundaryBytes = Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n");
                Stream postStream = request.GetRequestStream();
                postStream.Write(itemBoundaryBytes, 0, itemBoundaryBytes.Length);

                //写入文本
                if (postData != null && postData.Count > 0)
                {

                    var keys = postData.Keys;
                    foreach (var key in keys)
                    {
                        string strHeader = $"Content-Disposition: form-data; name=\"{key}\"\r\n\r\n";
                        byte[] strByte = Encoding.UTF8.GetBytes(strHeader);
                        postStream.Write(strByte, 0, strByte.Length);

                        byte[] value = Encoding.UTF8.GetBytes(string.Format("{0}", postData[key]));
                        postStream.Write(value, 0, value.Length);

                        postStream.Write(itemBoundaryBytes, 0, itemBoundaryBytes.Length);

                    }
                }
                postStream.Write(itemBoundaryBytes, 0, itemBoundaryBytes.Length);

                string sbHeader = $"Content-Disposition:form-data;name=\"FileBase\";filename=\"{filename.Substring(filename.LastIndexOf("\\") + 1)}\"\r\nContent-Type:application/vnd.openxmlformats-officedocument.spreadsheetml.sheet\r\n\r\n\r\n";
                byte[] postHeaderBytes = Encoding.UTF8.GetBytes(sbHeader);

                FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
                byte[] bArr = new byte[fs.Length];
                fs.Read(bArr, 0, bArr.Length);
                fs.Close();
                postStream.Write(postHeaderBytes, 0, postHeaderBytes.Length);
                postStream.Write(bArr, 0, bArr.Length);

                postStream.Write(itemBoundaryBytes, 0, itemBoundaryBytes.Length);
                postStream.Write(endBoundaryBytes, 0, endBoundaryBytes.Length); //结束标志
                postStream.Close();


                //2

                response = (HttpWebResponse)request.GetResponse();
                reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string ret_html = reader.ReadToEnd();
                // {"ReturnObject":null,"IsSuccess":false,"ErrorMessage":"DEPOSITIMPORT_FileErrorPattern"}
                //{"TotalMemberCount":5,"TotalAmount":103.44,"TotalAudit":103.44,"FailCount":0}

                //{"ReturnObject":3,"IsSuccess":true,"ErrorMessage":null}
                JObject jo = (JObject)JsonConvert.DeserializeObject(ret_html.ToString());
                if (jo["IsSuccess"]!=null && jo["IsSuccess"].ToString().ToLower()=="true")
                {
                    flag = true;
                }

                //if (ret_html.Contains("TotalMemberCount") && ret_html.Contains("TotalAmount"))
                //{
                //    flag = true;
                //}

                reader.Close();
                reader.Dispose();
          
                response.Close();
                response.Dispose();

                if (request != null)
                {
                    request.Abort();
                }
                return true;
            }
            catch (WebException ex)
            {
                appSittingSet.Log($"导入数据失败：文件名 {filename}");
                flag = false;
            }

            return flag;
        }

        /// <summary>
        /// 根据订单号查询 用户名 金额
        /// </summary>
        /// <param name="bb"></param>
        /// <returns></returns>
        public static betData ThirdPartyPaymentDTPPGetDetail(betData bb)
        {
            try
            {
                string postUrl = "ThirdPartyPayment/DTPPGetDetail";
                string postData = "{\"id\":\"" + bb.bbid + "\"}";
                string postRefere = "ThirdPartyPayment/"+bb.bbid;
                JObject jo = GetResponse<JObject>(postUrl, postData, "POST", postRefere);

                decimal d = 0;
                decimal.TryParse(jo["Detail"]["Amount"].ToString(), out d);

                bb.betMoney = d;
                bb.username = jo["Detail"]["Account"].ToString();
                return bb;
            }
            catch (Exception ex)
            {
                //如果 操作超时 重新登录一下GPK
                string msg = "查询账户信息(钱包)失败 " + ex.Message;
                appSittingSet.Log(msg);
                return null;
            }
        }

        /// <summary>
        /// 取款次数 信息
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        public static betData MemberGetDepositWithdrawInfo(Gpk_UserDetail u)
        {
            try
            {
                string postUrl = "Member/GetDepositWithdrawInfo";
                string postData = "{\"id\":\"" + u.Id+ "\"}";
                string postRefere = "Member/" + u.Account;
                JObject jo = GetResponse<JObject>(postUrl, postData, "POST", postRefere);

                decimal d = 0;
                int e = 0;
                decimal.TryParse(jo["DepositTotal"].ToString(), out d);
                int.TryParse(jo["DepositTimes"].ToString(), out e);
                betData bb = new betData();
                bb.DepositTimes = e;
                bb.DepositTotal = d;
                decimal.TryParse(jo["WithdrawTotal"].ToString(), out d);
                int.TryParse(jo["WithdrawTimes"].ToString(), out e);
                bb.WithdrawTimes = e;
                bb.WithdrawTotal = d;
                return bb;
            }
            catch (Exception ex)
            {
                //如果 操作超时 重新登录一下GPK
                string msg = "查询账户信息(钱包)失败 " + ex.Message;
                appSittingSet.Log(msg);
                return null;
            }
        }


        /// <summary>
        /// 获取活动列表
        /// </summary>
        /// <param name="url_part"></param>
        /// <returns></returns>
        public static List<int> ActGetList(string url_part)
        {
            List<int> list = new List<int>();
            try
            {
                string postUrl = url_part+ "/GetList";
                string postData = "{\"search\":{\"AllState\":false,\"Status\":[1]},\"skip\":0,\"take\":100}";

                string postRefere = url_part + "?AllState=false&Status=1";
                JObject jo = GetResponse<JObject>(postUrl, postData, "POST", postRefere);

                if ((bool)jo["IsSuccess"] == true)
                {
                    JArray ja = JArray.FromObject(jo["ReturnObject"]);
                    foreach (var item in ja)
                    {
                            list.Add((int)item["Id"]);
                    }

                }
                else
                {
                    appSittingSet.Log(postData + " 没有记录" + jo["ErrorMessage"]);                        
                }

                return list;

            }
            catch (Exception ex)
            {
                //如果 操作超时 重新登录一下GPK
                string msg = "查询活动信息 列表失败 " + ex.Message;
                appSittingSet.Log(msg);
                return null;
            }
        }

        /// <summary>
        /// 获取活动列表用户
        /// </summary>
        /// <param name="url_part"></param>
        /// <returns></returns>
        public static List<int> ActGetRewardRecords(betData b)
        {
            List<int> list = new List<int>();
            try
            {
                string postUrl = b.aid+ "/GetRewardRecords";
                //string postData = "{\"search\":{\"AllState\":false,\"Status\":[1]},\"skip\":0,\"take\":100}";
                string postData = JsonConvert.SerializeObject(new { search = new { RewardStatus = new int[1] { 0 }, RewardTypes = new string[4] { "0", "1", "2", "3" }, IsCheckStatus = true }, luckyWheelId = b.bbid, skip = 0, take = 100 });

                string postRefere =$"{ b.aid }/LotteryRecord/{ b.bbid}?RewardStatus=0&IsCheckStatus";


                JObject jo = GetResponse<JObject>(postUrl, postData, "POST", postRefere);

                if ((bool)jo["IsSuccess"] == true)
                {
                    JArray ja = JArray.FromObject(jo["ReturnObject"]);
                    foreach (var item in ja)
                    {
                            list.Add((int)item["Id"]);
                    }

                }
                else
                {
                    appSittingSet.Log(postData + " 没有记录" + jo["ErrorMessage"]);                        
                }

                return list;

            }
            catch (Exception ex)
            {
                //如果 操作超时 重新登录一下GPK
                string msg = "查询活动信息 用户列表失败 " + ex.Message;
                appSittingSet.Log(msg);
                return null;
            }
        }

        public static bool ActSendRewards(List<int> list,betData b)
        {
            try
            {
                string postUrl = b.aid+ "/SendRewards";
                //string postData = "{\"search\":{\"AllState\":false,\"Status\":[1]},\"skip\":0,\"take\":100}";
                string postData = JsonConvert.SerializeObject( new { luckyWheelId = b.bbid, recordIds = list.ToArray() });

                string postRefere =$"{ b.aid }/LotteryRecord/{ b.bbid}?RewardStatus=0&IsCheckStatus";

                JObject jo = GetResponse<JObject>(postUrl, postData, "POST", postRefere);

                return (bool)jo["IsSuccess"];

            }
            catch (Exception ex)
            {
                string msg = "查询活动信息 用户列表失败 " + ex.Message;
                appSittingSet.Log(msg);
                return false;
            }
        }
    }
}
