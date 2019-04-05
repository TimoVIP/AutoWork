using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace AutoRegTest
{
    class Program
    {
        static CookieContainer cookie;
        static string c02b5;
        static void Main(string[] args)
        {
            //加载页面

            //获取c02b5
            c02b5 = getc02b5();
            //获取注册码
            string Captcha = GetCaptchaForRegister();
            //提交注册
            string username = GenerateRandomNumber(6, constant_en);
            string pwd= GenerateRandomNumber(8, constant_en); 
            string realname = GenerateRandomNumber(2, constant_ch);

            HttpWebRequest request;
            HttpWebResponse response;

            request = WebRequest.Create("https://www.3730-33.com/Register/Submit") as HttpWebRequest;
            request.Method = "POST";
            request.UserAgent = "Mozilla/4.0";
            request.KeepAlive = true;
            request.Accept = "application/json, text/plain, */*";
            request.ContentType = "application/json;charset=UTF-8";
            request.CookieContainer = cookie;

            request.Headers.Add("c02b5", c02b5);
            request.Headers.Add("Origin", " https://28835533.com");
            request.Host = "28835533.com";
            request.Referer = "https://28835533.com/Register";
            //request.ContentLength = 0;
            //6018     I2GoA28vJhEbiV84vDsEvg==
            string postData = JsonConvert.SerializeObject(new { GroupBank = "", Accoun = username, Password = pwd, MoneyPassword = pwd, Name = realname, checkCode = "4817", checkCodeEncrypt = Captcha, BankName = "" });
        //发送注册数据
        byte[] bytes = Encoding.UTF8.GetBytes(postData);
            request.ContentLength = bytes.Length;
            Stream newStream = request.GetRequestStream();
            newStream.Write(bytes, 0, bytes.Length);
            newStream.Close();

        }

        private static string getc02b5()
        {
            try
            {
                HttpWebRequest request = WebRequest.Create("https://www.3730-33.com/Register") as HttpWebRequest;
                request.ProtocolVersion = HttpVersion.Version11;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                request.Method = "GET";
                request.UserAgent = "Mozilla/4.0";
                request.ContentType = "application/x-www-form-urlencoded; charset=utf-8";

                cookie = new CookieContainer();
                request.CookieContainer = cookie;

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);

                string html = reader.ReadToEnd();
                string c02b5 = html.Substring(html.IndexOf("__RequestVerificationToken") + 49, 92);
                cookie.Add(response.Cookies);
                reader.Close();
                reader.Dispose();
                response.Dispose();
                request.Abort();
                return c02b5;

            }
            catch (WebException)
            {

                throw;
            }
        }

        private static string GetCaptchaForRegister()
        {
            try
            {
                HttpWebRequest request = WebRequest.Create("https://www.3730-33.com/Home/GetCaptchaForRegister") as HttpWebRequest;
                request.ProtocolVersion = HttpVersion.Version11;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                request.Method = "POST";
                request.UserAgent = "Mozilla/4.0";
                request.ContentType = "application/x-www-form-urlencoded; charset=utf-8";
                request.Headers.Add("c02b5", c02b5);
                request.Headers.Add("Origin", "https://www.3730-33.com");
                request.Host = "www.3730-33.com";
                request.CookieContainer = cookie;
                request.ContentLength = 0;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);

                string ret_html = reader.ReadToEnd();
                JObject jo = JObject.Parse(ret_html);
                cookie.Add(response.Cookies);
                reader.Close();
                reader.Dispose();
                response.Dispose();
                request.Abort();

                return jo["value"].ToString();
            }
            catch (WebException ex)
            {

                throw;
            }
        }

        //随机字母字符串(数字字母混和)
        private static char[] constant_en =
        {
    '0','1','2','3','4','5','6','7','8','9',
    'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z',
    'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z'
    };

        private static char[] constant_ch =
        {
    '赵','钱','孙','李','周','吴','郑','王','冯','陈',
    '东','西','南','北','春','夏','秋','东','梅','兰','竹','菊','雨','雪','冰','霜'
    };


        /// <summary>
        /// 随机字符串
        /// </summary>
        /// <param name="Length">长度</param>
        /// <returns></returns>
        public static string GenerateRandomNumber(int Length, char[] constant)
        {
            System.Text.StringBuilder newRandom = new System.Text.StringBuilder(62);
            Random rd = new Random();
            for (int i = 0; i < Length - 1; i++)
            {
                newRandom.Append(constant[rd.Next(constant.Length)]);
            }
            newRandom.Append(constant[rd.Next(10)]);//前十位必须出现一个 （密码必须要包含数字） 姓也出现最好
            return newRandom.ToString();
        }
    }
}
