using BaseFun;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TimoControl;

namespace GetHttpData2GPK
{
    class Program
    {

        static decimal rate = 0;
        static string pkey = "";
        static string ip = "";
        static string tmp = "";
        static string sign_new = "";
        static string msg = "";
        static bool r = false;

        private static HttpListener listener = new HttpListener();

        static void Main(string[] args)
        {
            try
            {
                pkey = appSittingSet.readAppsettings("pkey");
                ip = appSittingSet.readAppsettings("ip");
                rate = Convert.ToDecimal(appSittingSet.readAppsettings("rate"));
                Console.Title=appSittingSet.readAppsettings("platname");
            }
            catch (Exception ex)
            {
                msg = $"按任意键结束\r\n读取配置文件失败{ex.Message}";
                Console.WriteLine(msg);
                appSittingSet.Log(msg);
                Console.ReadKey();
                return;
            }

            try
            {
                listener.Prefixes.Add(ip); //添加需要监听的url范围
                listener.Start(); //开始监听端口，接收客户端请求
                msg = $"开启监听{ip}成功";
                Console.WriteLine(msg);
                appSittingSet.Log(msg);
            }
            catch (Exception ex)
            {
                msg = $"按任意键结束\r\n开启监听{ip}失败：{ex.Message}";
                Console.WriteLine(msg);
                appSittingSet.Log(msg);
                Console.ReadKey();
                return;
            }

            try
            {
                //登陆gpk
                bool islogin = false;
                if (!islogin)
                {
                    islogin = platGPK.loginGPK();
                }
                if (islogin)
                {
                    Console.WriteLine("GPK登陆成功"+DateTime.Now.ToString());
                    appSittingSet.Log("GPK登陆成功");
                }
                else
                {
                msg = $"按任意键结束\r\n登陆GPK失败";
                Console.WriteLine(msg);
                //appSittingSet.Log(msg);
                }
            }
            catch (Exception ex)
            {
                msg = $"按任意键结束\r\n登陆GPK出错{ex.Message}";
                Console.WriteLine(msg);
                appSittingSet.Log(msg);
                Console.ReadKey();
                return;
            }



            


            string recive = "";
            string orderid = "";
            string sign = "";
            string userid = "";
            while (true)
            {
                //阻塞主函数至接收到一个客户端请求为止
                HttpListenerContext context = listener.GetContext();
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;
                //提交方式约定为 post + application/x-www-form-urlencoded
                Stream stream = context.Request.InputStream;
                StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                recive = reader.ReadToEnd();
                if (request.HttpMethod!="POST")
                {
                    writemsg(response, new { success = "true", msg = "仅限POST /application/x-www-form-urlencoded方式提交数据" });
                    continue;
                }
                else
                {
                    if (!request.ContentType.Contains("application/x-www"))
                    {
                        writemsg(response, new { success = "true", msg = "仅限POST /application/x-www-form-urlencoded方式提交数据" });
                        continue;
                    }
                    else
                    {
                            //orderid=8524927&userid=17251&sign=9B88EDD8A3855FB44A0B2C948A333FE6
                            var a = recive.Split(new char[] { '&', '=' }, StringSplitOptions.RemoveEmptyEntries);
                            if (a.Length!=6)
                            {
                                writemsg(response, new { success = "true", msg = "请检查参数是否有误" });
                            }
                            else
                            {
                                orderid = a[1];
                                userid = a[3];
                                sign = a[5];
                            }

                    }
                }


                if (orderid == "" || sign == "" || userid == "")
                {
                    writemsg(response, new { success = "true", msg = "请检查参数是否有误" });
                }
                else
                {
                    tmp = $"orderid={orderid}&userid={userid}&key={pkey}";
                    sign_new = appSittingSet.md5(tmp, 32, true);

                    //调试
                    //appSittingSet.Log(tmp);
                    //appSittingSet.Log(sign_new);


                    if (sign != sign_new)
                    {
                        writemsg(response, new { success = "true", msg = "验参错误，请检查参数是否有误" });

                    }
                    else
                    {
                        //处理数据
                        betData bb = new betData();
                        bb.bbid = orderid;
                        //查询gpk订单信息
                        bb = platGPK.ThirdPartyPaymentDTPPGetDetail(bb);
                        if (bb == null)
                        {
                            msg = $"订单号{orderid}处理失败，请人工处理";
                        }
                        else
                        {
                            //组织信息

                            bb.betMoney = bb.betMoney * rate;
                            bb.Audit = bb.betMoney;
                            bb.AuditType = "Discount";
                            bb.Memo = "笔笔存送" + bb.bbid;
                            bb.Type = 5;
                            bb.passed = true;
                            //上分gpk
                            r = platGPK.MemberDepositSubmit(bb);
                            if (!r)
                            {
                                msg = $"订单号{orderid}用户{bb.username}金额{bb.betMoney}失败，请人工处理";
                            }
                            else
                            {
                                msg = $"订单号{orderid}用户{bb.username}金额{bb.betMoney}处理成功";
                            }
                        }
                        Console.WriteLine(msg + DateTime.Now.ToString());
                        appSittingSet.Log(msg);
                        writemsg(response, new { success = "true", msg = msg });
                    }
                }
                //Console.WriteLine("收到数据：" + recive);
            }


            //listener.Stop(); //关闭HttpListener


    

            //开启一个监听端口
            /*
            httpPostRequest.Prefixes.Add(ip);
            httpPostRequest.Start();
            Console.WriteLine($"开启监听{ip}");

            Thread ThrednHttpPostRequest = new Thread(new ThreadStart(httpPostRequestHandle));
            ThrednHttpPostRequest.Start();

            //登陆gpk
            bool islogin = false;
            if (!islogin)
            {
                islogin =platGPK.loginGPK();
            }
            if (islogin)
            {
                Console.WriteLine("GPK登陆成功");
                appSittingSet.Log("GPK登陆成功");
            }
            */

            /*
             3
             */

            //_listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
            //_listener.Prefixes.Add(ip);
            //_listener.Start();
            //appSittingSet.Log("启用数据监听！");
            //Console.WriteLine("启用数据监听！");
            //_listener.BeginGetContext(ListenerHandle, _listener);
            //Console.ReadLine();
        }

        /// <summary>
        /// 向页面输出信息
        /// </summary>
        /// <param name="response"></param>
        /// <param name="json"></param>
        private static void writemsg(HttpListenerResponse response, object json)
        {
            response.StatusCode = 200;
            response.Headers.Add("Access-Control-Allow-Origin", "*");
            response.ContentType = "application/json";
            response.ContentEncoding = Encoding.UTF8;
            byte[] buffer = Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(json));
            //对客户端输出相应信息.
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            //关闭输出流，释放相应资源
            response.OutputStream.Close();
        }


        #region 不用代码

        private static HttpListener httpPostRequest = new HttpListener();
        private static HttpListener _listener = new HttpListener();

        private static void httpPostRequestHandle()
        {
            while (true)
            {
                HttpListenerContext requestContext = httpPostRequest.GetContext();
                Thread threadsub = new Thread(new ParameterizedThreadStart((requestcontext) =>
                {
                    HttpListenerContext request = (HttpListenerContext)requestcontext;

                    //获取Post请求中的参数和值帮助类
                    HttpListenerPostParaHelper httppost = new HttpListenerPostParaHelper(request);
                    //获取Post过来的参数和数据
                    List<HttpListenerPostValue> lst = httppost.GetHttpListenerPostValue();

                    string orderid = "";
                    string sign = "";
                    string userid = "";
                    //string adType = "";

                    //使用方法
                    foreach (var key in lst)
                    {
                        if (key.type == 0)
                        {
                            string value = Encoding.UTF8.GetString(key.datas).Replace("\r\n", "");
                            if (key.name == "orderid")
                            {
                                orderid = value;
                                //Console.WriteLine(value);
                            }
                            if (key.name == "sign")
                            {
                                sign = value;
                                //Console.WriteLine(value);
                            }
                            if (key.name == "userid")
                            {
                                userid = value;
                                //Console.WriteLine(value);
                            }

                        }
                        #region 不用
                        //如果是文件 好像不起作用
                        //if (key.type == 1)
                        //{
                        //    string fileName = request.Request.QueryString["FileName"];
                        //    if (!string.IsNullOrEmpty(fileName))
                        //    {
                        //        string filePath = AppDomain.CurrentDomain.BaseDirectory + DateTime.Now.ToString("yyMMdd_HHmmss_ffff") + Path.GetExtension(fileName).ToLower();
                        //        if (key.name == "File")
                        //        {
                        //            FileStream fs = new FileStream(filePath, FileMode.Create);
                        //            fs.Write(key.datas, 0, key.datas.Length);
                        //            fs.Close();
                        //            fs.Dispose();
                        //        }
                        //    }
                        //}
                        #endregion

                    }

                    if (orderid == null || sign == null || userid == null)
                    {
                        writemsg(request, requestContext, new { success = "true", msg = "请检查参数是否有误" });
                    }
                    else
                    {
                        tmp = $"orderid={orderid}&userid={userid}&key={pkey}";
                        sign_new = appSittingSet.md5(tmp, 32, true);

                        //调试
                        appSittingSet.Log(tmp);
                        appSittingSet.Log(sign_new);


                        if (sign!=sign_new)
                        {
                            writemsg(request, requestContext, new { success = "true", msg = "验参错误，请检查参数是否有误" });

                        }
                        else
                        {
                            //处理数据
                            betData bb = new betData();
                            bb.bbid = orderid;
                            //查询gpk订单信息
                            bb = platGPK.ThirdPartyPaymentDTPPGetDetail(bb);
                            if (bb==null)
                            {
                                msg = $"订单号{orderid}处理失败，请人工处理";
                            }
                            else
                            {
                                //组织信息
                                bb.Audit = bb.betMoney;
                                bb.betMoney = bb.betMoney * rate;
                                bb.AuditType = "Discount";
                                bb.Memo = "笔笔存送"+bb.bbid;
                                bb.Type = 5;
                                bb.passed = true;
                                //上分gpk
                                r= platGPK.MemberDepositSubmit(bb);
                                if (!r)
                                {
                                    msg= $"订单号{orderid}上分失败，请人工处理";
                                }
                                else
                                {
                                    msg= $"订单号{orderid}处理成功";
                                }
                            }
                            appSittingSet.Log(msg);
                            writemsg(request, requestContext, new { success = "true", msg = msg });
                        }
                    }



                    //Response
                    //request.Response.StatusCode = 200;
                    //request.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                    //request.Response.ContentType = "application/json";
                    //requestContext.Response.ContentEncoding = Encoding.UTF8;
                    //byte[] buffer = System.Text.Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(new { success = "true", msg = "提交成功" }));
                    //request.Response.ContentLength64 = buffer.Length;
                    //var output = request.Response.OutputStream;
                    //output.Write(buffer, 0, buffer.Length);
                    //output.Close();

                }));
                threadsub.Start(requestContext);
            }
        }

        private static void writemsg(HttpListenerContext request, HttpListenerContext requestContext, object json)
        {
            request.Response.StatusCode = 200;
            request.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            request.Response.ContentType = "application/json";
            requestContext.Response.ContentEncoding = Encoding.UTF8;
            byte[] buffer = Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(json));
            request.Response.ContentLength64 = buffer.Length;
            var output = request.Response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();
        }

        private static void ListenerHandle(IAsyncResult result)
        {
            try
            {
                _listener = result.AsyncState as HttpListener;
                if (_listener.IsListening)
                {
                    _listener.BeginGetContext(ListenerHandle, result);
                    HttpListenerContext context = _listener.EndGetContext(result);
                    //解析Request请求
                    HttpListenerRequest request = context.Request;
                    string content = "";
                    switch (request.HttpMethod)
                    {
                        case "POST":
                            {
                                Stream stream = context.Request.InputStream;
                                StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                                content = reader.ReadToEnd();
                            }
                            break;
                        case "GET":
                            {
                                var data = request.QueryString;
                            }
                            break;
                    }
                    appSittingSet.Log("收到数据：" + content);
                    Console.WriteLine("收到数据：" + content);

                    //构造Response响应
                    HttpListenerResponse response = context.Response;
                    response.StatusCode = 200;
                    response.ContentType = "application/json;charset=UTF-8";
                    response.ContentEncoding = Encoding.UTF8;
                    response.AppendHeader("Content-Type", "application/json;charset=UTF-8");

                    using (StreamWriter writer = new StreamWriter(response.OutputStream, Encoding.UTF8))
                    {
                        writer.Write("success");
                        writer.Close();
                        response.Close();
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                appSittingSet.Log(ex.Message);
            }
        }

        #endregion
    }
}
