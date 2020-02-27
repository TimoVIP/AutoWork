using BaseFun;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TimoControl
{
    public static class platBET02
    {
        private static string urlbase { get; set; }
        private static string acc { get; set; }
        private static string pwd { get; set; }
        private static string uid { get; set; }
        private static CookieContainer cookie { get; set; }
        private static string otp { get; set; }
        private static string Authorization { get; set; }
        /// <summary>
        /// 登陆
        /// </summary>
        /// <returns></returns>
        public static bool login()
        {
            try
            {
                string s1 = appSittingSet.readAppsettings("BET");
                acc = s1.Split('|')[0];
                pwd = s1.Split('|')[1];
                urlbase = s1.Split('|')[2];
            }
            catch (Exception ex)
            {
                appSittingSet.Log("BET取配置文件失败" + ex.Message);
                return false;
            }


            try
            {
                Console.WriteLine($"请输入账号{acc}对应的Google验证码,按Enter 2次");
                otp = Console.ReadLine();
                pwd = appSittingSet.md5(pwd);//密码加密

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                string url = $"{urlbase}adminsystem/server/adminuser/login";
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "POST";
                //request.Host = urlbase.Replace("http://", "");
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:72.0) Gecko/20100101 Firefox/72.0";
                request.Accept = "application/json, text/plain, */*";
                request.ContentType = " application/x-www-form-urlencoded";
                request.Referer = urlbase;
                request.Headers.Add("Authorization", "ff23e0e6-c80b-470b-bef8-2bac8316515d");//每次发送一个GUID
                request.Headers.Add("Origin", urlbase);
                //request.Headers.Add("", "");
                request.KeepAlive = true;
                //request.Connection = "keep-alive";

                //request.ProtocolVersion = HttpVersion.Version11;
                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11;
                ////证书错误
                //ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                //request.CookieContainer = cookie;


                //发送数据1
                //var obj = new { userAccount = acc, userPassword = pwd, validCode = otp };
                //string postdata = JsonConvert.SerializeObject(obj);
                //byte[] bytes = Encoding.UTF8.GetBytes(postdata);
                //request.ContentLength = bytes.Length;
                //Stream newStream = request.GetRequestStream();
                //newStream.Write(bytes, 0, bytes.Length);
                //newStream.Close();

                //发送数据2
                StreamWriter requestWriter = new StreamWriter(request.GetRequestStream());
                requestWriter.Write($"userAccount={acc}&userPassword={pwd}&validCode={otp}");
                requestWriter.Close();
                requestWriter = null;


                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string ret_html = reader.ReadToEnd();
                /*
                 {"status":1,"msg":"SUCCESS","data":{"permissionCode":["accountchange$export","accountchange$query","accountchange$queryShowUser","accountchange$queryShowUserSum","inspection$findInspectionInfoByUserIdOrOrder","inspection$findInspectionInfoShowUser","inspection$query","memberAnalysisReport$export","memberAnalysisReport$query","memberAnalysisReport$queryShowUser","memberlevel$findByCompanyCardList","memberlevel$findByMemberLevelList","memberlevel$query","memberlevel$update","memberlevel$updateBind","memberLoginLog$query","memberLoginLog$queryShowUser","memberManager$batchUpdateFrozenStatus","memberManager$find","memberManager$findBalanceByUserAccount","memberManager$findBxxList","memberManager$findByMemberPageResult","memberManager$findByUserCard","memberManager$findMemberAccountDetailInfo","memberManager$findMemberAccountDetailInfoShowUser","memberManager$findMemberPlatformRegistered","memberManager$findNameByUserAccount","memberManager$findPlatformBalanceByMember","memberManager$findPlatformList","memberManager$implByMemberField","memberManager$platformWithdraw","memberManager$query","memberManager$queryShowUser","memberManager$updateBxxPwd","memberManager$updateDeposit","memberManager$updateHandsel","memberManager$updateLoginPassword","memberManager$updateMemberLevel","memberManager$updateMemberRemark","memberManager$updateNameByUserAccount","memberManager$updateRankCardById","memberManager$updateWithdraw","memberManager$updateWithdrawPassword","memberplaylog$findGamePlatformList","memberplaylog$kickAUser","memberplaylog$kickUser","memberplaylog$query","memberplaylog$queryShowUser","memberRechargeOrder$exportOfflineRechargeOrder","memberRechargeOrder$exportOnlineRechargeOrder","memberRechargeOrder$findOfflineRechargeOrderPageResult","memberRechargeOrder$findOfflineRechargeOrderTotal","memberRechargeOrder$findOnlineRechargeOrderPageResult","memberRechargeOrder$findOnlineRechargeOrderTotal","memberRechargeOrder$onlineRechargeOrderForceDeposit","memberRechargeOrder$showMemberAccount","memberRechargeOrder$updateOfflineRechargeOrder","memberWithdraw$queryOrderStatus","memberWithdraw$queryProcessingFinishOrderPage","memberWithdraw$queryProcessingOrderPage","memberWithdraw$showMemberAccount","memberWithdraw$showMemberZfbNo","memberWithdraw$update","newActivityList$newActivity","onLineMemberManager$deleteUser","onLineMemberManager$queryOnLine","onLineMemberManager$queryShowUser","PlatformRtRecord$queryReturn","PlatformRtRecord$queryReturnDetailed","PlatformRtRecord$queryReturnShowUser","platfromBetManage$collectPlatformOrder","platfromBetManage$findByGamePlatfromList","platfromBetManage$findMemberGamingType","platfromBetManage$query","platfromBetManage$queryBuyu","platfromBetManage$queryBuyuShowUser","platfromBetManage$queryCaipiao","platfromBetManage$queryCaipiaoShowUser","platfromBetManage$queryDianzi","platfromBetManage$queryDianziShowUser","platfromBetManage$queryQipai","platfromBetManage$queryQipaiShowUser","platfromBetManage$queryTiyu","platfromBetManage$queryTiyuShowUser","platfromBetManage$queryZhenren","platfromBetManage$queryZhenrenShowUser","platfromBetManage$resetRedisCacheValue"],"resourceTree":[{"creationTime":1528439525000,"creationBy":"admin","lastUpdatedTime":1528439525000,"lastUpdatedBy":"admin","dataSourceKey":null,"currentUser":null,"resourceId":14,"resourceCode":null,"resourceUrl":null,"resourceName":"会员管理","iconUrl":"team","orderNum":"20","parentId":-1,"isLeafNode":null,"checkd":null,"children":[{"creationTime":null,"creationBy":null,"lastUpdatedTime":null,"lastUpdatedBy":null,"dataSourceKey":null,"currentUser":null,"resourceId":154,"resourceCode":"memberplaylog","resourceUrl":"/manager/memberplaylog/findByMemberplaylogList","resourceName":"最后登录平台","iconUrl":null,"orderNum":null,"parentId":14,"isLeafNode":null,"checkd":null,"children":null,"userId":null},{"creationTime":1528439525000,"creationBy":"admin","lastUpdatedTime":1528439525000,"lastUpdatedBy":"admin","dataSourceKey":null,"currentUser":null,"resourceId":27,"resourceCode":"onLineMemberManager","resourceUrl":"/manager/onlinemember/onlineMemberList","resourceName":"在线会员","iconUrl":null,"orderNum":"10","parentId":14,"isLeafNode":null,"checkd":null,"children":null,"userId":null},{"creationTime":1528439525000,"creationBy":"admin","lastUpdatedTime":1528439525000,"lastUpdatedBy":"admin","dataSourceKey":null,"currentUser":null,"resourceId":28,"resourceCode":"memberManager","resourceUrl":"/manager/member/memberList","resourceName":"会员列表","iconUrl":null,"orderNum":"20","parentId":14,"isLeafNode":null,"checkd":null,"children":null,"userId":null},{"creationTime":1528963894000,"creationBy":"admin","lastUpdatedTime":1528963894000,"lastUpdatedBy":"admin","dataSourceKey":null,"currentUser":null,"resourceId":72,"resourceCode":"memberLoginLog","resourceUrl":"/manager/memberloginlog/memberLoginLogList","resourceName":"登录日志","iconUrl":null,"orderNum":"30","parentId":14,"isLeafNode":null,"checkd":null,"children":null,"userId":null},{"creationTime":1528439525000,"creationBy":"admin","lastUpdatedTime":1528439525000,"lastUpdatedBy":"admin","dataSourceKey":null,"currentUser":null,"resourceId":29,"resourceCode":"accountchange","resourceUrl":"/manager/membercapitaldetail/memberCapitalDetailList","resourceName":"会员现金系统","iconUrl":null,"orderNum":"40","parentId":14,"isLeafNode":null,"checkd":null,"children":null,"userId":null}],"userId":49},{"creationTime":1528439525000,"creationBy":"admin","lastUpdatedTime":1528439525000,"lastUpdatedBy":"admin","dataSourceKey":null,"currentUser":null,"resourceId":15,"resourceCode":null,"resourceUrl":null,"resourceName":"资金管理","iconUrl":"dollar","orderNum":"30","parentId":-1,"isLeafNode":null,"checkd":null,"children":[{"creationTime":1528439525000,"creationBy":"admin","lastUpdatedTime":1528439525000,"lastUpdatedBy":"admin","dataSourceKey":null,"currentUser":null,"resourceId":32,"resourceCode":"memberRechargeOrder","resourceUrl":"/manager/memberorder/memberorderList","resourceName":"审核存款","iconUrl":null,"orderNum":"10","parentId":15,"isLeafNode":null,"checkd":null,"children":null,"userId":null},{"creationTime":1528439525000,"creationBy":"admin","lastUpdatedTime":1528439525000,"lastUpdatedBy":"admin","dataSourceKey":null,"currentUser":null,"resourceId":33,"resourceCode":"memberWithdraw","resourceUrl":"/manager/auditdraw/auditDrawList","resourceName":"审核出款","iconUrl":null,"orderNum":"20","parentId":15,"isLeafNode":null,"checkd":null,"children":null,"userId":null}],"userId":49},{"creationTime":null,"creationBy":null,"lastUpdatedTime":null,"lastUpdatedBy":null,"dataSourceKey":null,"currentUser":null,"resourceId":134,"resourceCode":null,"resourceUrl":null,"resourceName":"运营管理","iconUrl":"cluster","orderNum":"35","parentId":-1,"isLeafNode":null,"checkd":null,"children":[{"creationTime":null,"creationBy":null,"lastUpdatedTime":null,"lastUpdatedBy":null,"dataSourceKey":null,"currentUser":null,"resourceId":143,"resourceCode":"memberlevel","resourceUrl":"/manager/memberlayermanage/memberLayerManageList","resourceName":"层级管理","iconUrl":null,"orderNum":"40","parentId":134,"isLeafNode":null,"checkd":null,"children":null,"userId":null},{"creationTime":null,"creationBy":null,"lastUpdatedTime":null,"lastUpdatedBy":null,"dataSourceKey":null,"currentUser":null,"resourceId":142,"resourceCode":"inspection","resourceUrl":"/manager/addMosaicAudit/addMosaicAuditList","resourceName":"会员稽查","iconUrl":null,"orderNum":"50","parentId":134,"isLeafNode":null,"checkd":null,"children":null,"userId":null},{"creationTime":1551771647000,"creationBy":"admin","lastUpdatedTime":1552547510000,"lastUpdatedBy":"admin","dataSourceKey":null,"currentUser":null,"resourceId":161,"resourceCode":"memberAnalysisReport","resourceUrl":"/manager/operationsManagement/MembershipAnalysisSystemList","resourceName":"会员分析系统","iconUrl":null,"orderNum":"70","parentId":134,"isLeafNode":null,"checkd":null,"children":null,"userId":null},{"creationTime":1553316596000,"creationBy":"admin","lastUpdatedTime":1553316601000,"lastUpdatedBy":"admin","dataSourceKey":null,"currentUser":null,"resourceId":164,"resourceCode":"PlatformRtRecord","resourceUrl":"/manager/WashCodeRecord/WashCodeRecord","resourceName":"洗码记录","iconUrl":null,"orderNum":"80","parentId":134,"isLeafNode":null,"checkd":null,"children":null,"userId":null}],"userId":49},{"creationTime":1528439525000,"creationBy":"admin","lastUpdatedTime":1528439525000,"lastUpdatedBy":"admin","dataSourceKey":null,"currentUser":null,"resourceId":16,"resourceCode":null,"resourceUrl":null,"resourceName":"注单管理","iconUrl":"file-search","orderNum":"40","parentId":-1,"isLeafNode":null,"checkd":null,"children":[{"creationTime":null,"creationBy":null,"lastUpdatedTime":null,"lastUpdatedBy":null,"dataSourceKey":null,"currentUser":null,"resourceId":132,"resourceCode":"platfromBetManage","resourceUrl":"/manager/bet/findByBetPageResult","resourceName":"平台注单","iconUrl":null,"orderNum":null,"parentId":16,"isLeafNode":null,"checkd":null,"children":null,"userId":null}],"userId":49},{"creationTime":null,"creationBy":null,"lastUpdatedTime":null,"lastUpdatedBy":null,"dataSourceKey":null,"currentUser":null,"resourceId":137,"resourceCode":null,"resourceUrl":null,"resourceName":"活动管理","iconUrl":"gift","orderNum":"47","parentId":-1,"isLeafNode":null,"checkd":null,"children":[{"creationTime":1564031387000,"creationBy":"admin","lastUpdatedTime":1564031400000,"lastUpdatedBy":"admin","dataSourceKey":null,"currentUser":null,"resourceId":173,"resourceCode":"newActivityList","resourceUrl":"/manager/PromotionManage/NewActivity/NewActivityList","resourceName":"新活动列表","iconUrl":null,"orderNum":"20","parentId":137,"isLeafNode":null,"checkd":null,"children":null,"userId":null}],"userId":49}],"adminUserVO":{"creationTime":1581061727000,"creationBy":"xiongmao","lastUpdatedTime":1581061730000,"lastUpdatedBy":"xiongmao","dataSourceKey":null,"currentUser":"robot2","userId":49,"userSystemId":"robot2","userAccount":"robot2","companyId":null,"userPassword":null,"userType":"1","telphone":"robot2","email":"robot2@gmail.com","isEnable":1,"validCode":null,"oldUserPassword":null,"confirmUserPassword":null,"adminUserPermissionVOList":null,"adminResourceVOList":null,"adminKey":null,"startDate":null,"endDate":null,"userSecret":"ART6FA4WWFIBGV4F"}}}
                 */
                //获取响应头 Authorization
                Authorization = response.Headers.Get("Authorization");

                cookie = new CookieContainer();
                cookie.Add(response.Cookies);
                reader.Close();
                reader.Dispose();
                response.Dispose();
                request.Abort();

                JObject jo = (JObject)JsonConvert.DeserializeObject(ret_html.ToString());
                if (jo["msg"].ToString() == "SUCCESS" || jo["status"].ToString() == "1")
                {
                    return true;
                }
                else
                    return false;
            }
            catch (WebException ex)
            {
                appSittingSet.Log(string.Format("BET登录失败：{0}   ", ex.Message));
                return false;
            }
        }

        /// <summary>
        /// 查询会员 余额
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool findBalanceByUserAccount(betData b)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                string url = $"{urlbase}/adminsystem/server/memberManager/findBalanceByUserAccount?userAccount={b.username}";
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "GET";
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:72.0) Gecko/20100101 Firefox/72.0";
                request.Accept = "application/json, text/plain, */*";
                request.ContentType = " application/x-www-form-urlencoded";
                request.Referer = urlbase;
                request.Headers.Add("Authorization", Authorization);
                request.Headers.Add("Origin", urlbase);
                request.KeepAlive = true;
                request.CookieContainer = cookie;

                //request.ProtocolVersion = HttpVersion.Version11;
                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11;
                ////证书错误
                //ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                //request.CookieContainer = cookie;

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string ret_html = reader.ReadToEnd();
                /*
                 {"status":0,"msg":"该会员不存在！","data":null}
                 {"status":1,"msg":"SUCCESS","data":{"creationTime":null,"creationBy":null,"lastUpdatedTime":null,"lastUpdatedBy":null,"dataSourceKey":null,"currentUser":null,"walletId":null,"userId":null,"balance":55.90,"isFrist":0,"withdrawPassword":null,"safeDepositBalance":0.00}}
                 */
                cookie = new CookieContainer();
                cookie.Add(response.Cookies);
                reader.Close();
                reader.Dispose();
                response.Dispose();
                request.Abort();

                JObject jo = (JObject)JsonConvert.DeserializeObject(ret_html.ToString());

                if (jo["msg"].ToString().Contains("该会员不存在") || jo["status"].ToString() == "0")
                {
                    return false;
                }
                else
                    return true;
            }
            catch (WebException ex)
            {
                appSittingSet.Log(string.Format("BET查询会员余额失败：{0}   ", ex.Message));
                if (ex.Message.Contains("操作超时"))
                {
                    Console.WriteLine("请重新登陆BET");
                    login();
                }

                return false;
            }
        }

        /// <summary>
        /// 加钱
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool updateDeposit(betData b)
        {
            try
            {

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                string url = $"{urlbase}adminsystem/server/memberManager/updateDeposit";
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "POST";
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:72.0) Gecko/20100101 Firefox/72.0";
                request.Accept = "application/json, text/plain, */*";
                request.ContentType = " application/json;charset=utf-8";
                request.Referer = urlbase;
                request.Headers.Add("Authorization", Authorization);
                request.Headers.Add("Origin", urlbase);
                request.KeepAlive = true;


                //request.ProtocolVersion = HttpVersion.Version11;
                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11;
                ////证书错误
                //ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                //request.CookieContainer = cookie;


                //发送数据1                  request.ContentType = " application/json;charset=utf-8";
                var obj = new { userAccount = b.username, balance = b.betMoney, damaMultiple = b.Audit, note =b.Memo };
                string postdata = JsonConvert.SerializeObject(obj);
                byte[] bytes = Encoding.UTF8.GetBytes(postdata);
                request.ContentLength = bytes.Length;
                Stream newStream = request.GetRequestStream();
                newStream.Write(bytes, 0, bytes.Length);
                newStream.Close();

                //发送数据2                  request.ContentType = " application/x-www-form-urlencoded";
                //StreamWriter requestWriter = new StreamWriter(request.GetRequestStream());
                //requestWriter.Write($"userAccount={b.username}&balance={b.betMoney}&note={b.bbid}&damaMultiple=1.00");
                //requestWriter.Close();
                //requestWriter = null;


                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string ret_html = reader.ReadToEnd();
                /*
                 {"status":1,"msg":"SUCCESS","data":1}*/
                cookie = new CookieContainer();
                cookie.Add(response.Cookies);
                reader.Close();
                reader.Dispose();
                response.Dispose();
                request.Abort();

                JObject jo = (JObject)JsonConvert.DeserializeObject(ret_html.ToString());
                if (jo["msg"].ToString() == "SUCCESS" || jo["status"].ToString() == "1")
                {
                    return true;
                }
                else
                    return false;
            }
            catch (WebException ex)
            {
                appSittingSet.Log(string.Format("BET充值失败：{0}   ", ex.Message));
                return false;
            }
        }

        /// <summary>
        /// 加优惠
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool updateHandsel(betData b)
        {
            try
            {

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                string url = $"{urlbase}adminsystem/server/memberManager/updateHandsel";
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "POST";
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:72.0) Gecko/20100101 Firefox/72.0";
                request.Accept = "application/json, text/plain, */*";
                request.ContentType = " application/json;charset=utf-8";
                request.Referer = urlbase;
                request.Headers.Add("Authorization", Authorization);
                request.Headers.Add("Origin", urlbase);
                request.KeepAlive = true;


                //request.ProtocolVersion = HttpVersion.Version11;
                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11;
                ////证书错误
                //ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                //request.CookieContainer = cookie;


                //发送数据1                  request.ContentType = " application/json;charset=utf-8";
                var obj = new { userAccount = b.username, balance = b.betMoney, damaMultiple = b.Audit, note =b.Memo, isCapital = b.AuditType };
                string postdata = JsonConvert.SerializeObject(obj);
                byte[] bytes = Encoding.UTF8.GetBytes(postdata);
                request.ContentLength = bytes.Length;
                Stream newStream = request.GetRequestStream();
                newStream.Write(bytes, 0, bytes.Length);
                newStream.Close();

                //发送数据2                  request.ContentType = " application/x-www-form-urlencoded";
                //StreamWriter requestWriter = new StreamWriter(request.GetRequestStream());
                //requestWriter.Write($"userAccount={b.username}&balance={b.betMoney}&note={b.bbid}&damaMultiple=1.00");
                //requestWriter.Close();
                //requestWriter = null;


                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string ret_html = reader.ReadToEnd();
                /*
                 {"status":1,"msg":"SUCCESS","data":1}*/
                cookie = new CookieContainer();
                cookie.Add(response.Cookies);
                reader.Close();
                reader.Dispose();
                response.Dispose();
                request.Abort();

                JObject jo = (JObject)JsonConvert.DeserializeObject(ret_html.ToString());
                if (jo["msg"].ToString() == "SUCCESS" || jo["status"].ToString() == "1")
                {
                    return true;
                }
                else
                    return false;
            }
            catch (WebException ex)
            {
                appSittingSet.Log(string.Format("BET充值失败：{0}   ", ex.Message));
                return false;
            }
        }

        /// <summary>
        /// 修改层级
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool updateMemberLevel(betData b)
        {
            try
            {

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                string url = $"{urlbase}/adminsystem/server/memberManager/updateMemberLevel";
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "POST";
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:72.0) Gecko/20100101 Firefox/72.0";
                request.Accept = "application/json, text/plain, */*";
                request.ContentType = " application/json;charset=utf-8";
                request.Referer = urlbase;
                request.Headers.Add("Authorization", Authorization);
                request.Headers.Add("Origin", urlbase);
                request.KeepAlive = true;


                //request.ProtocolVersion = HttpVersion.Version11;
                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11;
                ////证书错误
                //ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                //request.CookieContainer = cookie;


                //发送数据1                  request.ContentType = " application/json;charset=utf-8";
                //string ids = "[" + b.memberId + "]";
                //var obj = new { userIdArr =ids, userLevelId = b.level };
                //string postdata = JsonConvert.SerializeObject(obj);

                string postdata = "{\"userIdArr\":["+b.memberId+"],\"userLevelId\":"+b.level+"}";
                byte[] bytes = Encoding.UTF8.GetBytes(postdata);
                request.ContentLength = bytes.Length;
                Stream newStream = request.GetRequestStream();
                newStream.Write(bytes, 0, bytes.Length);
                newStream.Close();

                //发送数据2                  request.ContentType = " application/x-www-form-urlencoded";
                //StreamWriter requestWriter = new StreamWriter(request.GetRequestStream());
                //requestWriter.Write($"userAccount={b.username}&balance={b.betMoney}&note={b.bbid}&damaMultiple=1.00");
                //requestWriter.Close();
                //requestWriter = null;


                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string ret_html = reader.ReadToEnd();
                /*
                 {"status":1,"msg":"SUCCESS","data":1}*/
                cookie = new CookieContainer();
                cookie.Add(response.Cookies);
                reader.Close();
                reader.Dispose();
                response.Dispose();
                request.Abort();

                JObject jo = (JObject)JsonConvert.DeserializeObject(ret_html.ToString());
                if (jo["msg"].ToString() == "SUCCESS" || jo["status"].ToString() == "1")
                {
                    return true;
                }
                else
                    return false;
            }
            catch (WebException ex)
            {
                appSittingSet.Log(string.Format("BET修改层级失败：{0}   ", ex.Message));
                return false;
            }
        }
        /// <summary>
        /// 查询是否绑定银行卡
        /// Memo 账户姓名
        /// PortalMemo 卡号
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public static betData findByUserCard(betData b)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                //string url = $"{urlbase}/adminsystem/server/memberManager/findByMemberUserPageResult?pageNo=1&pageSize=20&userAccount={b.username}";
                string url = $"{urlbase}adminsystem/server/memberManager/findByUserCard?accountType=1&userAccount={b.username}";
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "GET";
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:72.0) Gecko/20100101 Firefox/72.0";
                request.Accept = "application/json, text/plain, */*";
                request.ContentType = " application/x-www-form-urlencoded";
                request.Referer = urlbase;
                request.Headers.Add("Authorization", Authorization);
                request.Headers.Add("Origin", urlbase);
                request.KeepAlive = true;
                request.CookieContainer = cookie;

                //request.ProtocolVersion = HttpVersion.Version11;
                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11;
                ////证书错误
                //ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                //request.CookieContainer = cookie;

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string ret_html = reader.ReadToEnd();

                cookie = new CookieContainer();
                cookie.Add(response.Cookies);
                reader.Close();
                reader.Dispose();
                response.Dispose();
                request.Abort();

                JObject jo = (JObject)JsonConvert.DeserializeObject(ret_html.ToString());
                b.passed = false;
                if (jo["data"] !=null &&  jo["data"].ToString().Length>10)
                {
                    JArray ja = JArray.FromObject(jo["data"]);
                    if (ja.Count>=1)
                    {
                        if (ja[0]["userBankName"].ToString().Length >= 2 && ja[0]["bankNo"].ToString().Length >= 16)
                        {
                            b.Memo = ja[0]["userBankName"].ToString();
                            b.PortalMemo = ja[0]["bankNo"].ToString();
                            b.passed = true;
                        }

                    }
                }
                return b;
            }
            catch (WebException ex)
            {
                appSittingSet.Log(string.Format("BET查询会员银行卡失败：{0}   ", ex.Message));
                if (ex.Message.Contains("操作超时"))
                {
                    Console.WriteLine("请重新登陆BET");
                    login();
                }
                return null;
            }
        }


        /// <summary>
        /// 查询会员信息
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public static betData findByMemberUserPageResult (betData b)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                string url = $"{urlbase}adminsystem/server/memberManager/findByMemberUserPageResult?pageNo=1&pageSize=20&userAccount={b.username}";
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "GET";
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:72.0) Gecko/20100101 Firefox/72.0";
                request.Accept = "application/json, text/plain, */*";
                //request.ContentType = " application/x-www-form-urlencoded";
                request.Referer = urlbase;
                request.Headers.Add("Authorization", Authorization);
                request.Headers.Add("Origin", urlbase);
                request.KeepAlive = true;
                request.CookieContainer = cookie;

                //request.ProtocolVersion = HttpVersion.Version11;
                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11;
                ////证书错误
                //ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                //request.CookieContainer = cookie;

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string ret_html = reader.ReadToEnd();

                cookie = new CookieContainer();
                cookie.Add(response.Cookies);
                reader.Close();
                reader.Dispose();
                response.Dispose();
                request.Abort();

                JObject jo = (JObject)JsonConvert.DeserializeObject(ret_html.ToString());
                b.passed = false;
                if (jo["data"]["totalRecord"].ToString() == "1")
                {
                    JArray ja = JArray.FromObject(jo["data"]["result"]);
                    if (ja.Count>=1)
                    {
                        decimal d = 0;
                        decimal.TryParse(ja[0]["rechargeValue"].ToString(), out d);
                        b.DepositTotal = d;//总存款
                        b.Payoff = 0-d;
                        decimal.TryParse(ja[0]["damaValue"].ToString(), out d);
                        b.hisBetMoney = (int)d;//打码量
                        b.links = ja[0]["loginIp"].ToString();//最后登陆IP
                        b.memberId =  ja[0]["userId"].ToString();//编号
                        b.passed = true;
                        b.level =  ja[0]["leverName"].ToString();//级别
                        b.Memo=  ja[0]["telephone"].ToString();//手机
                        b.PortalMemo=  ja[0]["bundleVersionId"].ToString();//来源
                        b.betTime=  ja[0]["registerTime"].ToString();//注册时间 
                        b.Type = int.Parse(ja[0]["loginSource"].ToString());//登陆设备  1pc 2ios 3安卓 4h5 其他5
                    }

                }
                return b;
            }
            catch (WebException ex)
            {
                appSittingSet.Log(string.Format("BET查询会员信息失败：{0}   ", ex.Message));
                if (ex.Message.Contains("操作超时"))
                {
                    Console.WriteLine("请重新登陆BET");
                    login();
                }

                return null;
            }
        }


        /// <summary>
        /// 查询同IP 登陆信息 是否通过 
        /// </summary>
        /// <param name="b"></param>
        /// <param name="max">最大数</param>
        /// <returns></returns>
        public static int findMemberUserLoginLog(betData b)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                string url = $"{urlbase}/adminsystem/server/memberLoginLog/findMemberUserLoginLog?pageNo=1&pageSize=50&loginIp={b.links}";
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "GET";
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:72.0) Gecko/20100101 Firefox/72.0";
                request.Accept = "application/json, text/plain, */*";
                //request.ContentType = " application/x-www-form-urlencoded";
                request.Referer = urlbase;
                request.Headers.Add("Authorization", Authorization);
                request.Headers.Add("Origin", urlbase);
                request.KeepAlive = true;
                request.CookieContainer = cookie;

                //request.ProtocolVersion = HttpVersion.Version11;
                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11;
                ////证书错误
                //ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                //request.CookieContainer = cookie;

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string ret_html = reader.ReadToEnd();

                cookie = new CookieContainer();
                cookie.Add(response.Cookies);
                reader.Close();
                reader.Dispose();
                response.Dispose();
                request.Abort();

                JObject jo = (JObject)JsonConvert.DeserializeObject(ret_html.ToString());


                if ((int)jo["data"]["totalRecord"]>=1)
                {
                    JArray ja = JArray.FromObject(jo["data"]["result"]);
                    List<string> list = new List<string>();
                    foreach (var item in ja)
                    {
                        if (!list.Contains(item["userId"].ToString()))
                        {
                            list.Add(item["userId"].ToString());
                        }
                    }
                    return list.Count;
                }
                else
                {
                    return 0;
                }
            }
            catch (WebException ex)
            {
                appSittingSet.Log(string.Format("BET查询IP信息失败：{0}   ", ex.Message));
                if (ex.Message.Contains("操作超时"))
                {
                    Console.WriteLine("请重新登陆BET");
                    login();
                }
                return -1;
            }
        }




    }
}
