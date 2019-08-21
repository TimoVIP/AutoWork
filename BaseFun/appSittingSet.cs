using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;

namespace BaseFun
{
    public static class appSittingSet
    {
        /// <summary>
        /// 日志文件夹路劲
        /// </summary>
        public static string logPath = AppDomain.CurrentDomain.BaseDirectory + "log";
        /// <summary>
        /// 日志文件路径 不会换日期 取消
        /// </summary>
        //private static string logFilePath;
        /// <summary>
        /// 读取AppSet节点
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string readAppsettings(string key)
        {
            if (HttpContext.Current != null)
            {
                if (WebConfigurationManager.AppSettings[key] == null)
                    return "";
                else
                    return WebConfigurationManager.AppSettings[key];
            }
            else
            {
                if (ConfigurationManager.AppSettings[key] == null)
                {
                    if (readConfig() == null)
                    {
                        return "";
                    }
                    object a = readConfig()[key];
                    if (a == null)
                        return "";
                    else
                        return readConfig()[key].ToString();
                    //return readConfig()[key]==null? "": readConfig()[key].ToString();
                    //return "";
                }
                else
                    return ConfigurationManager.AppSettings[key].ToString();
            }
        }

        /// <summary>
        /// 写AppSet节点
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void writeAppsettings(string key, string value)
        {
            Configuration config = null;
            if (HttpContext.Current != null)
            {
                config = WebConfigurationManager.OpenWebConfiguration(null);
            }
            else
            {
                config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            }

            //AppSettingsSection app = config.AppSettings;
            //app.Settings[key].Value = value;
            //config.Save(ConfigurationSaveMode.Modified);

            bool exist = false;
            foreach (string _key in config.AppSettings.Settings.AllKeys)
            {
                if (_key == key)
                {
                    exist = true;
                    break;
                }
            }
            if (exist)
            {
                config.AppSettings.Settings.Remove(key);
            }
            config.AppSettings.Settings.Add(key, value);
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        //public static Hashtable readConfig()
        //{
        //    return readConfig(null);
        //}
        public static Hashtable readConfig(string SectionName = "appconfig")
        {
            try
            {
                Hashtable myConfig = (Hashtable)ConfigurationManager.GetSection(SectionName);
                return myConfig;
            }
            catch (Exception ex)
            {
                Log(ex.Message);
                return null;
            }
        }

        private static Dictionary<long, long> lockDic = new Dictionary<long, long>();
        /// <summary>
        /// 写文件日志 2018年12月21日 解决多线程写入的问题 编码改为ansi
        /// </summary>
        /// <param name="log"></param>
        public static void Log(string log)
        {
            Log(log, "");
        }
        /// <summary>
        /// 写文件日志
        /// </summary>
        /// <param name="log">日志内容</param>
        /// <param name="path">文件名 不含有文件夹路径</param>
        public static void Log(string log, string path)
        {
            if (!Directory.Exists(logPath))
                Directory.CreateDirectory(logPath);
            if (path != "")
                path = logPath + "\\" + path;
            else
                path = logPath + "\\" + DateTime.Now.Date.ToString("yyyyMMdd") + ".txt";

            //path = logPath +"\\"+ path != "" ?  path : DateTime.Now.Date.ToString("yyyyMMdd") + ".txt";

            //logFilePath = path;
            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite, 8, FileOptions.Asynchronous))
            {
                Byte[] dataArray = System.Text.Encoding.Default.GetBytes(log + " " + DateTime.Now.ToString("G") + System.Environment.NewLine);
                bool flag = true;
                long slen = dataArray.Length;
                long len = 0;
                while (flag)
                {
                    try
                    {
                        if (len >= fs.Length)
                        {
                            fs.Lock(len, slen);
                            lockDic[len] = slen;
                            flag = false;
                        }
                        else
                        {
                            len = fs.Length;
                        }
                    }
                    catch (Exception ex)
                    {
                        while (!lockDic.ContainsKey(len))
                        {
                            len += lockDic[len];
                        }
                    }
                }
                fs.Seek(len, SeekOrigin.Begin);
                fs.Write(dataArray, 0, dataArray.Length);
                fs.Close();
            }
        }

        public static void showLogFile()
        {
            showLogFile("");
        }
        public static void showLogFile(string path)
        {
            if (path != "")
                path = logPath + "\\" + path;
            else
                path = logPath + "\\" + DateTime.Now.Date.ToString("yyyyMMdd") + ".txt";
            System.Diagnostics.Process.Start("notepad.exe", path);
        }
        /// <summary>
        /// 删除几天前的log
        /// </summary>
        /// <param name="days"></param>
        public static void clsLogFiles(int days)
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(logPath);
                FileInfo[] fs = di.GetFiles("*.txt", SearchOption.AllDirectories);
                foreach (var item in fs)
                {
                    if (item.CreationTime < DateTime.Now.AddDays(0 - days).Date)
                    {
                        item.Delete();
                    }
                }
            }
            catch (Exception ex)
            {
                Log("清除日志失败" + ex.Message);
            }
        }

        /*
        /// <summary>
        /// 记录到SQLite数据库 记录的是提交时间 需要考虑是否正确 MSG
        /// alter table record add bbid INTEGER;
        /// MSG 记录提交到活动站的时间
        /// </summary>
        /// <param name="bb"></param>
        public static void recorderDb(betData bb)
        {
            SQLiteConnection m_dbConnection = get_dbConnection();
            //string Str_time="";
            //if (bb.betTime!=null)
            //{
            //    Str_time = bb.betTime;
            //}
            //if (bb.lastCashTime!=null)
            //{
            //    Str_time = bb.lastCashTime;
            //}


            string sql = $"insert  or ignore into record (username, gamename,betno,chargeMoney,pass,msg,subtime,aid,bbid) values ('{ bb.username}', '{ bb.gamename}','{bb.betno }',{ bb.betMoney },{(bb.passed == true ? 1 : 0) },'{ bb.msg }','{DateTime.Now.AddHours(-12).ToString("yyyy-MM-dd HH:mm:ss") }' , {bb.aid},{bb.bbid})";


            SQLiteCommand command1 = new SQLiteCommand(sql, m_dbConnection);
            command1.ExecuteNonQuery();
            m_dbConnection.Close();
        }

        */
        /// <summary>
        /// 发邮件
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="content"></param>
        public static void sendEmail(string subject, string content)
        {

            try
            {
                //实例化一个发送邮件类。
                MailMessage mailMessage = new MailMessage();
                //发件人邮箱地址，方法重载不同，可以根据需求自行选择。
                mailMessage.From = new MailAddress("120173721@qq.com", "机器人提醒", Encoding.UTF8);
                //收件人邮箱地址。
                mailMessage.To.Add(new MailAddress("29857287@qq.com"));
                //邮件标题。
                mailMessage.Subject = subject;
                //邮件内容。
                mailMessage.Body = content + "\r\n 发生时间：" + DateTime.Now.ToString("G");

                //实例化一个SmtpClient类。
                SmtpClient client = new SmtpClient();
                //在这里我使用的是qq邮箱，所以是smtp.qq.com，如果你使用的是126邮箱，那么就是smtp.126.com。
                client.Host = "smtp.qq.com";
                client.Port = 25;
                //使用安全加密连接。
                //client.EnableSsl = true;
                //不和请求一块发送。
                //client.UseDefaultCredentials = false;
                client.UseDefaultCredentials = true;
                //验证发件人身份(发件人的邮箱，邮箱里的生成授权码);
                client.Credentials = new NetworkCredential("120173721@qq.com", "Swq1w2e3");
                //异步发送 
                client.SendAsync(mailMessage, mailMessage);
                client.SendCompleted += Client_SendCompleted;
                //发送 关闭的时候会等待 程序假死
                //client.Send(mailMessage);
                //txtLog(string.Format("发送邮件成功 主题 {0}-{1} ", mailMessage.Subject, mailMessage.Body));
            }
            catch (Exception ex)
            {
                Log("发送邮件失败" + ex.Message);
            }

        }

        /// <summary>
        /// 发邮件 异步
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Client_SendCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            //throw new NotImplementedException();
            MailMessage mailMessage = (MailMessage)e.UserState;
            Log(string.Format("发送邮件成功 主题 {0}-{1} ", mailMessage.Subject, mailMessage.Body));
        }

        /// <summary>
        /// 时间戳转换时间字符串
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        public static string unixTimeToTime(string timeStamp)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime;
            if (timeStamp.Length.Equals(10))//判断是10位
            {
                lTime = long.Parse(timeStamp + "0000000");
            }
            else
            {
                lTime = long.Parse(timeStamp + "0000");//13位
            }
            TimeSpan toNow = new TimeSpan(lTime);
            DateTime daTime = dtStart.Add(toNow);
            string time = daTime.ToString("yyyy/MM/dd HH:mm:ss");//转为了string格式
            return time;

        }

        public static string md5(string code, int len)
        {
            if (len == 32)
                return System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(code, "MD5").ToLower();
            else
                return System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(code, "MD5").ToLower().Substring(8, 16);
        }

        /// <summary>
        /// 获取SHA256加密
        /// </summary>
        /// <param name="s">待价密字符</param>
        /// <param name="upper">是否大写</param>
        /// <returns></returns>
        public static string getSHA512(string s,bool upper)
        {
            byte[] result = new SHA512Managed().ComputeHash(Encoding.Default.GetBytes(s));

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < result.Length; i++)
            {
                if (upper)
                    sb.Append(result[i].ToString("X2"));               
                else
                    sb.Append(result[i].ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
