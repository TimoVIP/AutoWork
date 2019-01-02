using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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
        private static string GPKconnectionId { get; set; }
        private static CookieContainer ct_gpk { get; set; }

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
               //GPKconnectionId = appSittingSet.readAppsettings("GPKconnectionId");
               //FiliterGroups = appSittingSet.readAppsettings("FiliterGroups").Split('|');
               //td_cookie = appSittingSet.readAppsettings("td_cookie");

               //不从appconfig 里面取得了 直接生产随机数
               GPKconnectionId = Guid.NewGuid().ToString();
               td_cookie = "18446744070" + new Random().Next(100000000, 999999999).ToString();
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
                request.ContentType = "application/json; charset=utf-8";//发送的是json数据 注意
                string postdata = "{\"account\":\"" + acc + "\",\"password\":\"" + pwd + "\"}";

                string headurl = url_gpk_base.Replace("http://", "").Replace("/", "");
                //Init();//证书错误

                //设置请求头、cookie
                CookieContainer cookie = new CookieContainer();
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
                        ct_gpk = cookie;

                        if (ret_html == "{\"IsSuccess\":true,\"Methods\":null}")
                        {
                            //获取 GPKconnectionId 从appconfig 获取固定的即可
                            //List<string> list =  getNegotiate();
                            //if (list.Count==2)
                            //{
                            //    GPKconnectionId = list[0];
                            //}

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
        /// 获取 ConnectionId 等信息
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
                request.CookieContainer = ct_gpk;

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string ret_html = reader.ReadToEnd();
                reader.Close();
                reader.Dispose();
                response.Close();
                response.Dispose();

                JObject jo = (JObject)JsonConvert.DeserializeObject(ret_html);

                list.Add(jo["ConnectionId"] == null ? "" : jo["ConnectionId"].ToString());
                list.Add(jo["ConnectionToken"] == null ? "" : jo["ConnectionToken"].ToString());

                //string value = jo[item][index].ToString(); 
                appSittingSet.txtLog("获取到的GPKconnectionId:" + list[0]);
                return list;

            }
            catch (WebException ex)
            {
                if (ex.HResult == -2146233079)
                {
                    list.Add("False");
                }
                appSittingSet.txtLog("获取GPKconnectionId失败，用app.config 里面的值" );
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

                //获取deposit_token
                HttpWebRequest request = WebRequest.Create(url_gpk_base + "Member/Search") as HttpWebRequest;

                //request.ProtocolVersion = HttpVersion.Version11;
                //ServicePointManager.SecurityProtocol =SecurityProtocolType.Tls12 ;//SecurityProtocolType.Tls1.2;

                request.Method = "POST";
                request.UserAgent = "Mozilla/4.0";
                request.KeepAlive = true;
                request.Accept = "application/json, text/plain, */*";
                request.ContentType = "application/json; charset=utf-8";//发送的是json数据 注意
                request.Host = url_gpk_base.Replace("http://", "").Replace("/", "");
                //request.Referer = url_gpk_base + string.Format("Member?search=true&account={0}&bbAccount={1}&_=1542780952420", bb.username, bb.wallet);
                request.Referer = url_gpk_base;
                request.Headers.Add("Origin", url_gpk_base);
                request.Headers.Add("X-Requested-With", "XMLHttpRequest");
                //设置请求头、cookie
                request.CookieContainer = ct_gpk;

                request.ServicePoint.Expect100Continue = false;
                request.Timeout = 4000;

                string postdata = "{\"Account\":\"" + bb.username + "\",\"BbAccount\":\"" + bb.wallet + "\",\"connectionId\":\"" + GPKconnectionId + "\"}";
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

                if (ret_html.Contains("Account"))
                {
                    //获取 层级
                    int i1 = ret_html.IndexOf("MemberLevelSettingId") + 22;
                    int i2 = ret_html.IndexOf("MemberLevelSettingName") - 2;
                    string level = ret_html.Substring(i1, i2 - i1);
                    bb.level = level;
                    bb.passed = true;

                }
                else
                {
                    //账号 钱包 对不上
                    bb.passed = false;
                    bb.msg = "请提供正确的账号 R";

                }

                return bb;

            }
            catch (WebException ex)
            {
                string msg = string.Format("用户 {0} 注单{1}  GPK查询账号、钱包失败 {2}", bb.username, bb.betno, ex.Message);
                //如果 操作超时 重新登录一下GPK
                if (ex.HResult == -2146233079 || ex.Message == "操作超时")
                {
                    //需要重新登录
                    loginGPK();
                    msg = string.Format("用户 {0} 注单{1}  GPK查询账号、钱包失败 {2} 已经重新登录账号", bb.username, bb.betno, ex.Message);
                }

                appSittingSet.txtLog(msg);
                return null;
            }

        }

        /// <summary>
        /// 提交充值到GPK 不适合 改用公共方法 报错 必须同一个请求对象
        /// </summary>
        /// <param name="bb"></param>
        public static bool submitToGPK(betData bb,string aname)
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
                request.CookieContainer = ct_gpk;

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
                string msg = string.Format("GPK获取充值Token失败：用户 {0} 注单{1} {2} ", bb.username, bb.betno, ex.Message);
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
                string postdata2 = "{\"AccountsString\":\"" + bb.username + "\",\"Type\":5,\"DepositToken\":\"" + ret_html + "\",\"AuditType\":\"None\",\"Amount\":" + bb.betMoney + ",\"IsReal\":false,\"PortalMemo\":\"" + aname+"-" + bb.gamename + "-" + bb.betno + "\",\"Memo\":\"" + aname+ "-" + bb.gamename + "-" + bb.betno + "\",\"Password\":\"" + pwd + "\",\"TimeStamp\":" + (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000 + "}";

                //设置请求头、cookie
                request.CookieContainer = ct_gpk;

                //Init();//证书错误

                //发送数据
                byte[] bytes = Encoding.UTF8.GetBytes(postdata2);
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
                string msg = string.Format("GPK提交充值数据失败：用户 {0} 注单{1} {2} ", bb.username, bb.betno, ex.Message);
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
        public static betData checkInGPK_transaction(betData bb)
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
            if (bb.betTime != null && bb.lastOprTime != "")
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
                string postData = "{\"Account\":\"" + bb.username + "\",\"WagersTimeBegin\":\"" + bb.lastCashTime + "\",\"connectionId\":\"" + GPKconnectionId + "\"}";
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
                request.CookieContainer = ct_gpk;
                string postdata = "";
                if (pageIndex==1)
                {
                    postdata = "{\"BalanceBegin\":\"1\",\"State\":\"1\",\"connectionId\":\"" + GPKconnectionId + "\"}";
                }
                else
                {
                    postdata = "{\"BalanceBegin\":\"1\",\"State\":\"1\",\"pageIndex\":"+(pageIndex-1).ToString()+",\"connectionId\":\""+GPKconnectionId+"\"}";
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
                string postdata =string.Format( "?account={0}&begin={1}&end={2}&isMember=true&orderBy=Commissionable&reverse=true&skip=0&take=100&types=BBINprobability&types=BBINFish30&types=BBINFish38&types=AgEbr&types=AgHsr&types=AgYoPlay&types=Mg2Slot&types=Mg2Html5&types=Pt2Slot&types=GpiSlot3D&types=GpiSlotR&types=GnsSlot&types=PrgSlot&types=SgSlot&types=Rg2Fish&types=Rg2Slot&types=JdbSlot&types=JdbFish&types=HabaSlot&types=Cq9Slot&types=Cq9Fish&types=NetEntSlot&types=GdSlot&types=Pt3Slot&types=RedTigerSlot&types=GameArtSlot&types=Mw2Slot&types=PgSlot&types=RedTiger2Slot&types=LgVirtualSport&types=Mg3Slot&types=IsbSlot&types=PtsSlot&types=PngSlot&types=City761Fish&types=FsSlot&types=FsFish&types=FsArcade&types=KaSlot&types=JsSlot&types=JsFish&types=GtiSlot&types=PlsSlot&types=AeSlot",bb.username, DateTime.Now.AddDays(-DateTime.Now.Day + 1).ToString("yyyy/MM/dd"),DateTime.Now.Date.ToString("yyyy/MM/dd"));
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
                request.CookieContainer = ct_gpk;

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
        /// 公共方法
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
                request.CookieContainer = ct_gpk;
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
    }
}
