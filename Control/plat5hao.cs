using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Text;

namespace TimoControl
{
    public static class plat5hao
    {
        private static string acc { get; set; }
        private static string pwd { get; set; }
        private static string url_base { get; set; }
        private static CookieContainer cookie { get; set; }
        private static string[] BankTransInfo{ get; set; }
        private static string[] Prob { get; set; }
        /// <summary>
        /// 登录
        /// </summary>
        /// <returns></returns>
        public static bool login()
        {

            try
            {
                string s = appSittingSet.readAppsettings("5hao");
                acc = s.Split('|')[0];
                pwd = s.Split('|')[1];
                url_base = s.Split('|')[2];

                //获取银行关键字 配置
                BankTransInfo = appSittingSet.readAppsettings("BankTransInfo").Split('|');
                Prob = appSittingSet.readAppsettings("Prob").Split('|');
            }
            catch (Exception ex)
            {
                appSittingSet.Log("获取配置文件失败" + ex.Message);
                return false;
            }


            HttpWebRequest request = null;
            HttpWebResponse response = null;
            StreamReader reader = null;
            try
            {
                string headurl = url_base.Replace("http://", "").Replace("/", "");
                string posturl = url_base + "Base/ProcessRequest?A=Login";
                string postdata = "Action=Login&UserName="+acc+"&Pwd="+pwd+"&PwdType=hash";

                request = WebRequest.Create(posturl) as HttpWebRequest;
                request.Method = "POST";
                request.Host = headurl;
                request.KeepAlive = true;

                request.Headers.Add("Origin", url_base.TrimEnd('/'));
                request.UserAgent = "Mozilla/5.0";
                request.Accept = "*/*";
                request.Referer = url_base + "Login";
                //request.Headers.Add("Accept-Language", "zh-CN,zh;q=0.9");
                //request.Headers["Accept-Encoding"] = "gzip, deflate";
                //request.AutomaticDecompression = DecompressionMethods.GZip;
                //request.ServicePoint.Expect100Continue = false;
                request.ContentType = "application/x-www-form-urlencoded";//发送的数据 注意


                //Init();//证书错误

                //设置请求头、cookie
                CookieContainer _cookie= new CookieContainer();
                request.CookieContainer = _cookie;

                //发送数据
                byte[] bytes = Encoding.UTF8.GetBytes(postdata);
                request.ContentLength = bytes.Length;
                Stream newStream = request.GetRequestStream();
                newStream.Write(bytes, 0, bytes.Length);
                newStream.Flush();
                newStream.Close();

                response = (HttpWebResponse)request.GetResponse();
                reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string ret_html = reader.ReadToEnd();
                /*
                 {
                  "Code": 0,
                  "StrCode": "未绑定域名",
                  "DataCount": 0,
                  "BackUrl": null,
                  "BackData": null
                }
                 */
                _cookie.Add(response.Cookies);
                cookie = _cookie;
                reader.Close();
                reader.Dispose();
                response.Close();
                response.Dispose();
                if (request!=null)
                {
                    request.Abort();
                }
                if (ret_html.Contains("登录成功"))
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
                appSittingSet.Log(string.Format("5hao站登录失败：{0}   ", ex.Message));
                return false;
            }
            finally
            {
                if (request!=null)
                {
                    request.Abort();
                }
                if (response!=null)
                {
                    response.Close();
                    response.Dispose();
                }
                if (reader!=null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }

        /// <summary>
        /// 获取列表 判定 1分钟内两笔 同金额 同用户 为重复提交
        /// </summary>
        /// <returns></returns>
        public static List<Recharge> getList_apply()
        {
            string posturl = url_base + "Base/ProcessRequest?A=GetListRecharge&U="+acc;
            string postdata = "UserName=&EndTime=" + DateTime.Now.Date.ToString("yyyy-MM-dd") + "&StartTime=" + DateTime.Now.AddDays(-1).Date.ToString("yyyy-MM-dd") + "&PageSize=20&PageNum=0&Action=GetListRecharge&AccountName=&OperateState=0";

            List<Recharge> list = new List<Recharge>();


            HttpWebRequest request = null;
            HttpWebResponse response = null;
            StreamReader reader = null;
            try
            {
                string headurl = url_base.Replace("http://", "").Replace("/", "");

                request = WebRequest.Create(posturl) as HttpWebRequest;
                request.Method = "POST";
                request.Host = headurl;
                request.KeepAlive = true;

                request.Headers.Add("Origin", url_base.TrimEnd('/'));
                request.UserAgent = "Mozilla/5.0";
                request.Accept = "*/*";
                request.Referer = url_base + "Recharge";
                request.ContentType = "application/x-www-form-urlencoded";//发送的数据 注意
                request.CookieContainer = cookie;

                //Init();//证书错误

                //发送数据
                byte[] bytes = Encoding.UTF8.GetBytes(postdata);
                request.ContentLength = bytes.Length;
                Stream newStream = request.GetRequestStream();
                newStream.Write(bytes, 0, bytes.Length);
                newStream.Flush();
                newStream.Close();

                response = (HttpWebResponse)request.GetResponse();
                reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string ret_html = reader.ReadToEnd();

                reader.Close();
                reader.Dispose();
                response.Close();
                response.Dispose();
                if (request != null)
                {
                    request.Abort();
                }

                //解析json
                JObject jo = JObject.Parse(ret_html);
                if (jo["Code"].ToString()=="-9")
                {
                    login();
                }
                if ((int)jo["DataCount"] < 1)
                {
                    return list;
                }

                JArray ja = JArray.FromObject(jo["BackData"]);
                foreach (var item in ja)
                {
                    Recharge rc = new Recharge();
                    rc.Id = item["Id"].ToString();
                    rc.AccountName = item["AccountName"].ToString();
                    rc.AddTime =DateTime.Parse( item["AddTime"].ToString());
                    rc.RechargeMoney = Convert.ToDecimal(item["RechargeMoney"]);
                    rc.RechargeType =  item["RechargeType"].ToString();
                    rc.SerialNumber = item["SerialNumber"].ToString();
                    rc.UserName = item["UserName"].ToString();

                    //测试 只处理测试账号
                    if (BankTransInfo[2].Length>0 && rc.UserName != BankTransInfo[2])
                    {
                        continue;
                    }
                    //是否开启概率随机拒绝
                    if (Prob[1]=="1")
                    {
                        foreach (var s in Prob[0].Split('@'))
                        {
                            if (rc.SerialNumber.EndsWith(s))
                            {
                                rc.IsRepeat = true;
                                break;
                            }
                            else
                            {
                                continue;
                            }
                        }

                        //if (Prob[0].Contains(rc.SerialNumber.Substring(rc.SerialNumber.Length-1)))
                        //{
                        //    rc.IsRepeat = true;
                        //    continue;
                        //}
                    }
                    //if (item["AccountName"].ToString().Contains("云闪付") && !list.Exists(o => o.UserName == rc.UserName && o.RechargeMoney == rc.RechargeMoney && Math.Abs( o.AddTime.Minute - rc.AddTime.Minute)< 2 ))
                    //{
                    //    list.Add(rc);
                    //}

                    //匹配 云闪付 字样
                    if (item["AccountName"].ToString().Contains(BankTransInfo[1]))
                    {
                        if (list.Exists(o => o.UserName == rc.UserName && o.RechargeMoney == rc.RechargeMoney && Math.Abs(o.AddTime.Minute - rc.AddTime.Minute) < 2))
                        {
                            //2分钟内同样的记录 认定为重复提交，取消掉
                            rc.IsRepeat = true;
                        }
                        else
                        {
                            rc.IsRepeat = false;
                        }
                        list.Add(rc);
                    }
                }

                return list;
            }
            catch (WebException ex)
            {
                appSittingSet.Log(string.Format("5hao站获取资料失败：{0}   ", ex.Message));
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

        /*
        /// <summary>
        /// 获取用户银行信息 列表
        /// </summary>
        /// <param name="rc"></param>
        /// <returns></returns>
        public static List<BankInfo> getList_bank(Recharge rc)
        {
            string posturl = url_base + "Base/ProcessRequest?A=GetListUserBankcard&U=" + acc;
            string postdata = "UserName=" + rc.UserName + "&CardNum=&RealName=&PageSize=20&PageNum=0&Action=GetListUserBankcard";

            List<BankInfo> list = new List<BankInfo>();


            HttpWebRequest request = null;
            HttpWebResponse response = null;
            StreamReader reader = null;
            try
            {
                string headurl = url_base.Replace("http://", "").Replace("/", "");

                request = WebRequest.Create(posturl) as HttpWebRequest;
                request.Method = "POST";
                request.Host = headurl;
                request.KeepAlive = true;

                request.Headers.Add("Origin", url_base.TrimEnd('/'));
                request.UserAgent = "Mozilla/5.0";
                request.Accept = "* /*";//注意空格
                request.Referer = url_base + "Recharge";
                request.ContentType = "application/x-www-form-urlencoded";//发送的数据 注意
                request.CookieContainer = cookie;

                //Init();//证书错误

                //发送数据
                byte[] bytes = Encoding.UTF8.GetBytes(postdata);
                request.ContentLength = bytes.Length;
                Stream newStream = request.GetRequestStream();
                newStream.Write(bytes, 0, bytes.Length);
                newStream.Flush();
                newStream.Close();

                response = (HttpWebResponse)request.GetResponse();
                reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string ret_html = reader.ReadToEnd();

                reader.Close();
                reader.Dispose();
                response.Close();
                response.Dispose();
                if (request != null)
                {
                    request.Abort();
                }

                //解析json
                JObject jo = JObject.Parse(ret_html);

                if ((int)jo["DataCount"]< 1)
                {
                    return list;
                }

                JArray ja = JArray.FromObject(jo["BackData"]);
                foreach (var item in ja)
                {
                    BankInfo bi = new BankInfo();
                    bi.Id = item["Id"].ToString();
                    bi.RealName = item["RealName"].ToString();
                    bi.BankName =  item["BankName"].ToString();
                    bi.CardNum = item["CardNum"].ToString();
                    bi.UserName = item["UserName"].ToString();
                    list.Add(bi);
                }

                return list;
            }
            catch (WebException ex)
            {
                appSittingSet.txtLog(string.Format("5hao站获取银行账户资料失败：{0}   ", ex.Message));
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

        */

        /// <summary>
        /// 获取银行对应的户名 中文名
        /// </summary>
        /// <param name="rc"></param>
        /// <returns></returns>
        public static Recharge getBankAccName(Recharge rc)
        {
            string posturl = url_base + "Base/ProcessRequest?A=GetListUserBankcard&U=" + acc;
            string postdata = "UserName=" + rc.UserName + "&CardNum=&RealName=&PageSize=20&PageNum=0&Action=GetListUserBankcard";
            List<string> names = new List<string>();
            HttpWebRequest request = null;
            HttpWebResponse response = null;
            StreamReader reader = null;
            try
            {
                string headurl = url_base.Replace("http://", "").Replace("/", "");

                request = WebRequest.Create(posturl) as HttpWebRequest;
                request.Method = "POST";
                request.Host = headurl;
                request.KeepAlive = true;

                request.Headers.Add("Origin", url_base.TrimEnd('/'));
                request.UserAgent = "Mozilla/5.0";
                request.Accept = "*/*";
                request.Referer = url_base + "Recharge";
                request.ContentType = "application/x-www-form-urlencoded";//发送的数据 注意
                request.CookieContainer = cookie;

                //Init();//证书错误

                //发送数据
                byte[] bytes = Encoding.UTF8.GetBytes(postdata);
                request.ContentLength = bytes.Length;
                Stream newStream = request.GetRequestStream();
                newStream.Write(bytes, 0, bytes.Length);
                newStream.Flush();
                newStream.Close();

                response = (HttpWebResponse)request.GetResponse();
                reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string ret_html = reader.ReadToEnd();

                reader.Close();
                reader.Dispose();
                response.Close();
                response.Dispose();
                if (request != null)
                {
                    request.Abort();
                }

                //解析json
                JObject jo = JObject.Parse(ret_html);

                if ((int)jo["DataCount"] >= 1)
                {
                    JArray ja = JArray.FromObject(jo["BackData"]);
                    foreach (var item in ja)
                    {
                        names.Add(item["RealName"].ToString());
                    }
                }

                rc.RealName = names;
                return rc;

            }
            catch (WebException ex)
            {
                appSittingSet.Log(string.Format("5hao站获取银行账户资料失败：{0}   ", ex.Message));
                return rc;
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
        /// 回填操作 确认 取消 恢复取消 
        /// </summary>
        /// <param name="rc">需要操作的行对象</param>
        /// <returns></returns>
        public static bool confirm(Recharge rc)
        {
            string posturl = url_base + "Base/ProcessRequest?A=SetRecharge&U=" + acc;
            //确认1取消3恢复取消2
            string postdata = "Action=SetRecharge&Id="+rc.Id+"&RechargeType="+rc.RechargeType+"&OperateType="+rc.OperateType;
            //Action=SetRecharge&Id=53488&RechargeType=291&OperateType=1
            HttpWebRequest request = null;
            HttpWebResponse response = null;
            StreamReader reader = null;
            try
            {
                string headurl = url_base.Replace("http://", "").Replace("/", "");

                request = WebRequest.Create(posturl) as HttpWebRequest;
                request.Method = "POST";
                request.Host = headurl;
                request.KeepAlive = true;

                request.Headers.Add("Origin", url_base.TrimEnd('/'));
                request.UserAgent = "Mozilla/5.0";
                request.Accept = "*/*";
                request.Referer = url_base + "Recharge";
                request.ContentType = "application/x-www-form-urlencoded";//发送的数据 注意
                request.CookieContainer = cookie;

                //Init();//证书错误

                //发送数据
                byte[] bytes = Encoding.UTF8.GetBytes(postdata);
                request.ContentLength = bytes.Length;
                Stream newStream = request.GetRequestStream();
                newStream.Write(bytes, 0, bytes.Length);
                newStream.Flush();
                newStream.Close();

                response = (HttpWebResponse)request.GetResponse();
                reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string ret_html = reader.ReadToEnd();

                reader.Close();
                reader.Dispose();
                response.Close();
                response.Dispose();
                if (request != null)
                {
                    request.Abort();
                }

                //解析json
                JObject jo = JObject.Parse(ret_html);

                if (jo["Code"].ToString() == "1")
                {
                    return true;
                }
                else if(jo["Code"].ToString() == "-1")
                {
                    return false;
                }
                else
                {
                    return false;
                }
            }
            catch (WebException ex)
            {
                appSittingSet.Log(string.Format("5hao站确认失败：{0}   ", ex.Message));
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
        /// 读取本地下载后本地的银行交易记录，并入数据库
        /// </summary>
        /// <param name="FolderPath">本地文件夹路径</param>
        /// <returns></returns>
        public static bool dbFromFile(string FolderPath)
        {
            List<string> sqllist = new List<string>();
            bool b = true;
            string sql = "";
            //文件夹下所有文本文件
            try
            {
                DirectoryInfo di = new DirectoryInfo(FolderPath);
                FileInfo[] fs = di.GetFiles("*.txt", SearchOption.AllDirectories);
                //是否第一次运行
                bool isFirstRun = false;
                if (File.Exists(FolderPath + "\\lock.lock"))
                {
                    isFirstRun = true;
                    File.Delete(FolderPath + "\\lock.lock");
                }
                
                //foreach (var item in fs)
                //{
                //    if (item.Name.Contains("lock"))
                //    {
                //        isFirstRun = true;
                //        item.Delete();
                //        break;
                //    }
                //}

                foreach (var file in fs)
                {
                    //判断文件是否处理过
                    sql = "select * from filehistory where filename='" + file.Name.Replace(".txt", "") + "' and status=1;";
                    bool flag = appSittingSet.recorderDbCheck(sql);
                    if (!flag)
                    {
                        //读取所有的行
                        string[] lines = File.ReadAllLines(file.FullName, Encoding.UTF8);
                        foreach (var line in lines)
                        {
                            string[] content = line.Trim().Split(new char[] { '^', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            //if (content.Length != 13 || content[1] != BankTransInfo[0])//银联入账字样 不需要
                            //{
                            //    continue;
                            //}
                            if (content.Length != 13 || content[1].Contains("日期") )
                            {
                                continue;
                            }

                            decimal d1 = 0;
                            decimal.TryParse(content[8], out d1);
                            decimal d2 = 0;
                            decimal.TryParse(content[10], out d2);
                            DateTime time;
                            DateTime.TryParse(content[0], out time);
                            //不是当日的记录 跳过
                            if (time!=DateTime.Now.Date)
                            {
                                continue;
                            }
                            //取款的跳过
                            if (content[1]== "跨行汇款" || d1<0)
                            {
                                continue;
                            }
                            DepositInfo dep = new DepositInfo()
                            {
                                //                                                                  "BI004"
                                Account = content[11].Split(new string[] { BankTransInfo[0] }, StringSplitOptions.RemoveEmptyEntries)[0],
                                Deposit = d1,
                                Time = time,
                                Balance = d2,
                                FileName = file.Name.Replace(".txt", "")
                            };

                            sqllist.Add("INSERT or ignore INTO DepositInfo ( Account,  Deposit, Balance,Time,FileName,Status ) VALUES ( '" + dep.Account + "', " + dep.Deposit + ", " + dep.Balance + ",'" + dep.Time.ToString("yyyy-MM-dd") + "','" + dep.FileName + "'," + (isFirstRun ? '1' : '0') + " );");
                        }

                        sqllist.Add("INSERT INTO FileHistory ( FileName, Status ) VALUES ( '" + file.Name.Replace(".txt", "") + "', '1' );");
                        b = appSittingSet.execSql(sqllist);
                    }
                    else
                    {
                        //删除数据库文件记录
                        //sql = "DELETE FROM FileHistory WHERE FileName = '" + file.Name.Replace(".txt", "") + "' AND  Status = '1';";
                        //appSittingSet.execScalarSql(sql);
                        //file.Delete();
                    }
                }
                return b;
            }
            catch (Exception ex)
            {
                appSittingSet.Log("处理文件失败" + ex.Message);
                return false;
            }

        }

        public static List<DepositInfo> listFromFile(string FolderPath)
        {
            List<DepositInfo> list = new List<DepositInfo>();
            //文件夹下所有文本文件
            try
            {
                DirectoryInfo di = new DirectoryInfo(FolderPath);
                FileInfo[] fs = di.GetFiles("*.txt", SearchOption.AllDirectories);
                foreach (var file in fs)
                {
                    //读取所有的行
                    string[] lines = File.ReadAllLines(file.FullName, Encoding.UTF8);
                    foreach (var line in lines)
                    {
                        string[] content = line.Trim().Split(new char[] { '^', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (content.Length != 13 || content[1] != "银联入账")
                        {
                            continue;
                        }

                        decimal d1 = 0;
                        decimal.TryParse(content[8], out d1);
                        decimal d2 = 0;
                        decimal.TryParse(content[10], out d2);
                        DateTime time;
                        DateTime.TryParse(content[0], out time);
                        DepositInfo dep = new DepositInfo()
                        {
                            Account = content[11].Split(new string[] { "BI004" }, StringSplitOptions.RemoveEmptyEntries)[0],
                            Deposit = d1,
                            Time = time,
                            Balance = d2,
                            FileName = file.Name.Replace(".txt", "")
                        };
                        //不同的文件去重复

                        //if (!list.Contains(dep))
                        //{
                        //    list.Add(dep);
                        //}
                        if (!list.Exists(o => o.Account ==dep.Account && o.Deposit==dep.Deposit && o.Balance == dep.Balance))
                        {
                            list.Add(dep);
                        }
                    }

                    file.Delete();
                }
                return list;
            }
            catch (Exception ex)
            {
                appSittingSet.Log("处理文件失败" + ex.Message);
                return null;
            }

        }

        /// <summary>
        /// 查询数据库未处理的数据， 前一天12:00:00点以后的数据 同网站后台一样
        /// </summary>
        /// <returns></returns>
        public static List<DepositInfo> getLits_db(int days)
        {
            string sql = "SELECT * FROM DepositInfo  WHERE  Status = '0' and Time >'"+DateTime.Now.Date.AddDays(-1).ToString("yyyy-MM-dd")+" 12:00:00';";
            List<DepositInfo> list = new List<DepositInfo>();
            DataTable dt = appSittingSet.getDataTableBySql(sql);
            DateTime time;

            if (dt.Rows.Count>0)
            {
                foreach (DataRow item in dt.Rows)
                {
                    DateTime.TryParse(item["Time"].ToString(), out time);
                    DepositInfo d = new DepositInfo()
                    {
                        Account = item["Account"].ToString(),
                        Balance = Convert.ToDecimal(item["Balance"]),
                        Deposit = Convert.ToDecimal(item["Deposit"]),
                        FileName = item["FileName"].ToString(),
                        Time = time
                    };
                    list.Add(d);
                }
            }
            return list;
        }


        //工商银行二维码
        /*
         * 获取二维码
         https://epass.icbc.com.cn/login/login_qrcode_img.jsp?encStrSingleInfo=RPnp_SWxkbGWWBVM8DDMjZZSA69WkIYdGEDCeFHHHocNqskKf6AXMdwHFRUbekdF7tjMxgAt8HRCRLHrsE6uwUZuQd9Lw.qPLPygIWrXJmrKTvpvs5UroD6nYBrbQpxkpE0HWMdo_Hae6nhF1TgIj9RHvfe_EA4YQyEks1Hv5dxzxQQ6qlU70sQXTMszJPX.rpYVAbsbzKkefdRU8yhqTiAbC6In21eVRLPCdN1b_w.c8qYwk26CqU90JEHsGwS8oj.m5y6MqLX042HYsuh2XQ==
         https://epass.icbc.com.cn/login/login_qrcode_img.jsp?encStrSingleInfo=RPnp_SWxkbGWWBVM8DDMjZZSA69WkIYdGEDCeFHHHocNqskKf6AXMdwHFRUbekdF7tjMxgAt8HRCRLHrsE6uwUZuQd9Lw.qP4vbRIjX6o16G14HbORrExT6nYBrbQpxkpE0HWMdo_Hae6nhF1TgIj9RHvfe_EA4YQyEks1Hv5dxzxQQ6qlU70sQXTMszJPX.rpYVAbsbzKkefdRU8yhqTmVDwZQPIM55rUA0.GrKKO.c8qYwk26CqU90JEHsGwS8oj.m5y6MqLX042HYsuh2XQ==
         Accept: image/webp,image/apng,image/*,* /*;q=0.8
        Accept-Encoding: gzip, deflate, br
        Accept-Language: zh-CN,zh;q=0.9
        Connection: keep-alive
        Cookie: ar_stat_uv=20840365056994191096|9999; BIGipServerDianZiYinHangKeHuQuDao_80_pool=874704906.20480.0000
        Host: epass.icbc.com.cn
        Referer: https://epass.icbc.com.cn/servlet/ICBCBaseReqServletNoSession?dse_operationName=epass_CreateQRCodeOp&serviceId=&serviceIdInto=&transData=&StructCode=1&clientIP=&orgurl=0&APPNO=44&betaFlag=1
        User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36
         
        刷新 监控是否扫码
        https://epass.icbc.com.cn/servlet/socketGetDataServlet?ebdp_jsonpCallback=jQuery1112003945613948614057_1544851957547&tranCode=WS00000&data=&userID=la9Qmuek_InxSyimiBIyxyys93y3nkcG.5a48xat2fejcbDwGUpVQw%3D%3D&_=1544851957548
        https://epass.icbc.com.cn/servlet/socketGetDataServlet?ebdp_jsonpCallback=jQuery111202770186511726844_1544852320063&tranCode=WS00000&data=&userID=la9Qmuek_InxSyimiBIyx.c4SY6Qqn4ZYxwP13kFpuqjcbDwGUpVQw%3D%3D&_=1544852320103

         */


    }
}
