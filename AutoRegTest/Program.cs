using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TimoControl;
using System.Drawing;
using System.Drawing.Imaging;

namespace AutoRegTest
{
    class Program
    {
        static CookieContainer cookie;
        static string c02b5;
        static string cap_value;
        static string cap_base64;
        static void Main(string[] args)
        {
            //加载页面

            //获取c02b5
            c02b5 = getc02b5();
            //获取注册码
            string path = GetCaptchaForRegister();
            //验证码识别
            //var cracker = new Cracker();
            //var result = cracker.Read((Bitmap)Bitmap.FromFile(path));

            Bitmap bitmap = (Bitmap)Bitmap.FromFile(path);
            UnCodebase ud = new UnCodebase(bitmap);
            ud.GrayByPixels();
            ud.ClearNoise(128, 2);
            ud.GetPicValidByValue(128, 4);
            Bitmap[] pics = ud.GetSplitPics(4, 1);     //分割
            string co =  ud.GetSingleBmpCode(pics[0],128);
            pics[0].Save(Environment.CurrentDirectory + "\\captchacode\\1.bmp", ImageFormat.Bmp);

            //tessnet2.Tesseract ocr = new tessnet2.Tesseract();//声明一个OCR类
            //ocr.SetVariable("tessedit_char_whitelist", "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz");
            //ocr.Init(Environment.CurrentDirectory+ "\tessdata", "eng", false);
            //List<tessnet2.Word> result = ocr.DoOCR(bitmap, Rectangle.Empty);//执行识别操作
            //string code = result[0].Text;

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
            request.Headers.Add("Origin", " https://www.3730-33.com/");
            request.Host = "www.3730-33.com";
            request.Referer = "https://www.3730-33.com/Register";
            //request.ContentLength = 0;
            //6018     I2GoA28vJhEbiV84vDsEvg==
            //1140  sSY5yCwgyY6rKXfiajNYug==
            //string postData = JsonConvert.SerializeObject(new { GroupBank = "", Accoun = username, Password = pwd, MoneyPassword = pwd, Name = realname, checkCode = "1140", checkCodeEncrypt = "sSY5yCwgyY6rKXfiajNYug==", BankName = "" });
            string postData = JsonConvert.SerializeObject(new { GroupBank = "", Accoun = username, Password = pwd, MoneyPassword = pwd, Name = realname, checkCode = "1140", checkCodeEncrypt = cap_value, BankName = "" });
            //发送注册数据
            byte[] bytes = Encoding.UTF8.GetBytes(postData);
            request.ContentLength = bytes.Length;
            Stream newStream = request.GetRequestStream();
            newStream.Write(bytes, 0, bytes.Length);
            newStream.Close();

            response = (HttpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
            string ret_html = reader.ReadToEnd();
            JObject jo = JObject.Parse(ret_html);
            if (jo["IsSuccess"].ToString() =="true")
            {
                appSittingSet.Log(string.Format("注册成功：用户名{0}密码{1}支付密码{2}姓名{3}",username,pwd,pwd,realname));
            }

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
                cap_value = jo["value"].ToString();
                cap_base64 =  jo["image"].ToString();
                //return jo["value"].ToString();
                //保存到本地，返回验证码路径
                //转图片
                byte[] bit = Convert.FromBase64String(cap_base64);
                MemoryStream ms = new MemoryStream(bit);
                Bitmap bmp = new Bitmap(ms);
                string path = Environment.CurrentDirectory + "\\captchacode\\" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".bmp";
                bmp.Save(path, ImageFormat.Bmp);
                return path;
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
