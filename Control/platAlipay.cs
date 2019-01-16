using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TimoControl
{
    public static class platAlipay
    {
        static CookieContainer cookie;
        public static Image getValidateCode()
        {
            string url = "https://authet15.alipay.com/login/imgcode.htm?sessionID="+Guid.NewGuid().ToString("N")+"&t="+ (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000 ;
            HttpWebRequest request = null;
            HttpWebResponse response = null;
            StreamReader reader = null;
            try
            {
                 request = WebRequest.Create("https://www.alipay.com/") as HttpWebRequest;

                request.ProtocolVersion = HttpVersion.Version11;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11;
                request.Method = "GET";
                request.UserAgent = "Mozilla/4.0";
                request.ContentType = "application/x-www-form-urlencoded; charset=utf-8";

                cookie = new CookieContainer();
                request.CookieContainer = cookie;               
                response = (HttpWebResponse)request.GetResponse();
                cookie.Add(response.Cookies);


                //验证码
                request = WebRequest.Create(url) as HttpWebRequest;

                request.ProtocolVersion = HttpVersion.Version11;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11;
                request.Method = "POST";
                request.UserAgent = "Mozilla/4.0";
                request.ContentType = "application/x-www-form-urlencoded; charset=utf-8";

                //cookie = new CookieContainer();
                request.CookieContainer = cookie;
                response = (HttpWebResponse)request.GetResponse();

                cookie.Add(response.Cookies);
                response = (HttpWebResponse)request.GetResponse();
                //reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                //Bitmap.FromStream(response.GetResponseStream());
                Image img = Image.FromStream(response.GetResponseStream());
                return img;
            }
            catch (WebException ex)
            {
                appSittingSet.txtLog(string.Format("获取验证码失败：{0}   ", ex.Message));
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
    }
}
