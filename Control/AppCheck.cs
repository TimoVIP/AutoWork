using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;

namespace TimoControl
{
    public static class AppCheck
    {
        private static string acc { get; set; }
        private static string pwd { get; set; }
        private static string url_base = "http://imtimo.vip/validate/";
        public static bool isExpired()
        {
            try
            {
                string s = appSittingSet.readAppsettings("UserInfo");
                acc = s.Split('|')[0];
                pwd = s.Split('|')[1];
            }
            catch (Exception ex)
            {
                appSittingSet.txtLog("获取配置文件失败" + ex.Message);
                return false;
            }

            string MachineCode = GetCpuInfo() + GetHDid();



            HttpWebRequest request = null;
            HttpWebResponse response = null;
            StreamReader reader = null;
            try
            {
                //ServicePointManager.DefaultConnectionLimit = 50;
                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;

                request = WebRequest.Create(url_base) as HttpWebRequest;

                request.ProtocolVersion = HttpVersion.Version11;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                request.Method = "POST";
                request.UserAgent = "Mozilla/4.0";
                request.ContentType = "application/x-www-form-urlencoded; charset=utf-8";
                string postdata = string.Format("username={0}&password={1}&machinecode={2}", acc, pwd,MachineCode);
                byte[] bytes = Encoding.UTF8.GetBytes(postdata);
                request.ContentLength = bytes.Length;
                Stream newStream = request.GetRequestStream();
                newStream.Write(bytes, 0, bytes.Length);
                newStream.Close();

                request.CookieContainer =  new CookieContainer();

                response = (HttpWebResponse)request.GetResponse();
                reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);

                string ret_html = reader.ReadToEnd();
                JObject jo = JObject.Parse(ret_html);
                return (bool)jo["Result"];
            }
            catch (WebException ex)
            {
                appSittingSet.txtLog(string.Format("验证失败：{0}   ", ex.Message));
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
        /// 获取cpu序列号 
        /// </summary> 
        /// <returns> string </returns> 
        public static string GetCpuInfo()
        {
            string cpuInfo = " ";
            ManagementClass cimobject = new ManagementClass("Win32_Processor ");
            ManagementObjectCollection moc = cimobject.GetInstances();
            foreach (ManagementObject mo in moc)
            {
                cpuInfo = mo.Properties["ProcessorId "].Value.ToString();
            }
            return cpuInfo.ToString();
        }
        /// <summary> 
        /// 获取硬盘ID 
        /// </summary> 
        /// <returns> string </returns> 
        public static string GetHDid()
        {
            string HDid = " ";
            ManagementClass cimobject1 = new ManagementClass("Win32_DiskDrive ");
            ManagementObjectCollection moc1 = cimobject1.GetInstances();
            foreach (ManagementObject mo in moc1)
            {
                HDid = (string)mo.Properties["Model "].Value;
            }
            return HDid.ToString();
        }

    }
}
