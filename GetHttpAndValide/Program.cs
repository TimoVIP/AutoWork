using BaseFun;
using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace GetHttpAndValide
{
    class Program
    {

        static string pkey = "";
        static string ip = "";
        static string tmp = "";
        static string sign_new = "";
        static string msg = "";
        static bool r = false;
        static string constr = "";
        static Hashtable config;
        private static HttpListener listener = new HttpListener();
        static void Main(string[] args)
        {
            try
            {
                config = appSittingSet.readConfig("appconfig");
                Console.Title = config["platname"].ToString();

                pkey = config["pkey"].ToString();
                pkey = File.ReadAllText($"{ Environment.CurrentDirectory}/{pkey}");//内容

                ip = config["ip"].ToString();
                constr = config["MySqlConnect"].ToString();

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
                //链接数据库
                //MySQLHelper.MySQLHelper.connectionString = constr;
                //MySQLHelper.MySQLHelper.Exsist("");
                if (MySQLHelper.MySQLHelper.IsOpen())
                {
                    msg = "数据库连接成功";
                    Console.WriteLine(msg);
                    appSittingSet.Log(msg);
                }
            }
            catch (Exception ex)
            {
                msg = $"按任意键结束\r\n数据库连接失败：{ex.Message}";
                Console.WriteLine(msg);
                appSittingSet.Log(msg);
                Console.ReadKey();
                return;
            }



            //try
            //{
            //    //登陆gpk
            //    bool islogin = false;
            //    if (!islogin)
            //    {
            //        islogin = platGPK.loginGPK();
            //    }
            //    if (islogin)
            //    {
            //        Console.WriteLine("GPK登陆成功" + DateTime.Now.ToString());
            //        appSittingSet.Log("GPK登陆成功");
            //    }
            //    else
            //    {
            //        msg = $"按任意键结束\r\n登陆GPK失败";
            //        Console.WriteLine(msg);
            //        //appSittingSet.Log(msg);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    msg = $"按任意键结束\r\n登陆GPK出错{ex.Message}";
            //    Console.WriteLine(msg);
            //    appSittingSet.Log(msg);
            //    Console.ReadKey();
            //    return;
            //}






            string recive = "";
            string orderid = "";
            string operate = "";
            string money = "";
            while (true)
            {
                //阻塞主函数至接收到一个客户端请求为止
                HttpListenerContext context = listener.GetContext();
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;
                //验证IP
                bool r = false;
                string cur_ip = request.RemoteEndPoint.Address.ToString();
                foreach (string item in config["whitelist"].ToString().Split('|'))
                {
                    if (cur_ip.Contains(item))
                    {
                        r = true;
                        break;
                    }
                }
                if (!r)
                {
                    writemsg(response, new { success = "false", msg = "IP不合法" });
                    msg = $"IP不合法({cur_ip})-{DateTime.Now}:\r\n{recive}";
                    Console.WriteLine(msg);
                    appSittingSet.Log(msg);
                    continue;
                }

                Console.WriteLine($"{cur_ip}已经连接{DateTime.Now}");

                //提交方式约定为 post + application/x-www-form-urlencoded
                Stream stream = context.Request.InputStream;
                StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                recive = reader.ReadToEnd();
                reader.Close();
                if (request.HttpMethod != "POST")
                {
                    writemsg(response, new { success = "false", msg = "仅限POST /application/x-www-form-urlencoded方式提交数据" });
                    continue;
                }
                else
                {
                    if (!request.ContentType.Contains("application/x-www"))
                    {
                        writemsg(response, new { success = "false", msg = "仅限POST /application/x-www-form-urlencoded方式提交数据" });
                        continue;
                    }
                }

                //解密参数
                try
                {
                    recive = Decrypt(pkey, recive);
                    var a = recive.Split('|');
                    if (a.Length != 3)
                    {
                        writemsg(response, new { success = "true", msg = "请检查参数是否有误" });
                        continue;
                    }
                    else
                    {
                        orderid = a[0];
                        money = a[1];
                        operate = a[2];
                    }

                    if (orderid == "" || money == "" || (operate != "operation" && operate!= "query"))
                    {
                        writemsg(response, new { success = "true", msg = "请检查参数是否有误" });
                        //continue;
                    }
                    else
                    {
                        //判断参数
                        if (operate== "operation")
                        {
                            MySQLHelper.MySQLHelper.ExecuteSql(string.Format(config["sql_insert"].ToString(), orderid, money));
                            writemsg(response, new { success = "true", msg = "记录成功" });
                            //continue;
                        }
                        else if (operate== "query")
                        {
                            bool b = MySQLHelper.MySQLHelper.Exsist(string.Format(config["sql_exsist"].ToString(), orderid));
                            writemsg(response, new { success = b, msg = "" });
                            //continue;                            
                        }
                    }
                }
                catch (Exception ex)
                {
                    writemsg(response, new { success = "false", msg = "请检查参数是否有误" });
                    //continue;                 
                }

                //Console.WriteLine("收到数据：" + recive);
                msg = $"收到数据{DateTime.Now}:\r\n{recive}";
                Console.WriteLine(msg);
                appSittingSet.Log(msg);
            }
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

        /// <summary>
        /// 解密字符串
        /// </summary>
        /// <param name="privateKey"></param>
        /// <param name="encryptedContent"></param>
        /// <returns></returns>
        private static string Decrypt(string privateKey, string encryptedContent)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(privateKey);

            var decryptString = Encoding.UTF8.GetString(rsa.Decrypt(Convert.FromBase64String(encryptedContent), false));

            return decryptString;
        }

    }
}
