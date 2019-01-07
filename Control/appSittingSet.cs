using System;
using System.Configuration;
using System.Data.SQLite;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Mail;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Web.Configuration;

namespace TimoControl
{
    public static class  appSittingSet
    {
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
                    return "";
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


        private static Dictionary<long, long> lockDic = new Dictionary<long, long>();
        /// <summary>
        /// 写文件日志 2018年12月21日 解决多线程写入的问题 编码改为ansi
        /// </summary>
        /// <param name="log"></param>
        public static void txtLog(string log)
        {
            if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "log"))
            {
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "log");
            }
            string filePath = AppDomain.CurrentDomain.BaseDirectory + "log\\" + DateTime.Now.Date.ToString("yyyyMMdd") + ".txt";

            using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite, 8, FileOptions.Asynchronous))
            {
                Byte[] dataArray = System.Text.Encoding.Default.GetBytes(log + DateTime.Now.ToString("G") + System.Environment.NewLine);
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
            string filePath = AppDomain.CurrentDomain.BaseDirectory + "log\\" + DateTime.Now.Date.ToString("yyyyMMdd") + ".txt";
            System.Diagnostics.Process.Start("notepad.exe", filePath);
            
        }

        /// <summary>
        /// 删除几天前的log
        /// </summary>
        /// <param name="days"></param>
        public static void clsLogFiles(int days)
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "log");
                FileInfo[] fs = di.GetFiles("*.txt", SearchOption.AllDirectories);
                foreach (var item in fs)
                {
                    if (item.CreationTime<DateTime.Now.AddDays(0-days).Date)
                    {
                        item.Delete();
                    }
                }
            }
            catch (Exception ex)
            {
                txtLog("清除日志失败" + ex.Message);
            }
        }


        private static SQLiteConnection get_dbConnection()
        {
            string dbpath = readAppsettings("dbpath");
            if (dbpath=="")
            {
                dbpath = "db.sqlite";
            }
                SQLiteConnection m_dbConnection = null;
            try
            {
                m_dbConnection = new SQLiteConnection("Data Source=" + AppDomain.CurrentDomain.BaseDirectory + dbpath +";Version=3;Journal Mode=Off;");
                if (m_dbConnection.State != ConnectionState.Open)
                {
                    m_dbConnection.Open();
                }
            }
            catch (SQLiteException ex)
            {
                txtLog("数据库打开失败" + ex.Message);
            }
            return m_dbConnection;
        }

        /// <summary>
        /// 记录到SQLite数据库 记录的是提交时间 需要考虑是否正确
        /// </summary>
        /// <param name="bb"></param>
        public static void recorderDb(betData bb,string aid)
        {
            SQLiteConnection m_dbConnection = get_dbConnection();
            string sql1 = "insert into record (username, gamename,betno,chargeMoney,pass,msg,subminttime,aid) values ('" + bb.username + "', '" + bb.gamename + "','" + bb.betno + "'," + bb.betMoney + "," + (bb.passed == true ? 1 : 0) + ",'" + bb.msg + "','"+ DateTime.Now.AddHours(-12).ToString("yyyy-MM-dd HH:mm:ss") + "' , "+aid+")";
            SQLiteCommand command1 = new SQLiteCommand(sql1, m_dbConnection);
            command1.ExecuteNonQuery();
            m_dbConnection.Close();
        }

        /// <summary>
        /// 查询是否存在记录
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static bool recorderDbCheck(string sql)
        {
            SQLiteConnection m_dbConnection = get_dbConnection();

            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            bool b = reader.HasRows;
            reader.Close();
            m_dbConnection.Close();
            return b;

        }

        public static bool execSql(string sql)
        {
            return execSql(sql, false);
        }

        /// <summary>
        /// 批量执行SQL
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="use_trans"></param>
        /// <returns></returns>
        public static bool execSql(string sql,bool use_trans)
        {
            try
            {
                int i = 0;
                SQLiteConnection m_dbConnection = get_dbConnection();
                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                if (use_trans)
                {
                    SQLiteTransaction trans = m_dbConnection.BeginTransaction();
                    try
                    {
                        command.Transaction = trans;
                        i= command.ExecuteNonQuery();
                        trans.Commit();
                    }
                    catch (Exception)
                    {
                        trans.Rollback();
                        command.Dispose();
                        m_dbConnection.Close();
                        return false;
                    }
                }
                else
                {
                    i= command.ExecuteNonQuery();
                }

                command.Dispose();
                m_dbConnection.Close();
                return Convert.ToBoolean( i);
            }
            catch (Exception ex)
            {
                txtLog("数据库执行SQL错误" + ex.Message);
                return false;
            }

        }


        public static object execScalarSql(string sql)
        {
            SQLiteConnection m_dbConnection = get_dbConnection();

            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            object o = command.ExecuteScalar();
            command.Dispose();
            m_dbConnection.Close();
            return o;
        }

        public static DataTable getDataTableBySql(string sql)
        {
            SQLiteConnection m_dbConnection = get_dbConnection();

            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataAdapter sda = new SQLiteDataAdapter(command);
            DataTable dt = new DataTable();
            sda.Fill(dt);
            sda.Dispose();
            command.Dispose();
            m_dbConnection.Close();
            return dt;
        }

        /// <summary>
        /// 发邮件
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="content"></param>
        public static void sendEmail(string subject,string content)
        {

            try
            {
                //实例化一个发送邮件类。
                MailMessage mailMessage = new MailMessage();
                //发件人邮箱地址，方法重载不同，可以根据需求自行选择。
                mailMessage.From = new MailAddress("120173721@qq.com","机器人提醒",Encoding.UTF8);
                //收件人邮箱地址。
                mailMessage.To.Add(new MailAddress("29857287@qq.com"));
                //邮件标题。
                mailMessage.Subject = subject;
                //邮件内容。
                mailMessage.Body = content + "\r\n 发生时间："+ DateTime.Now.ToString("G");

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

                //client.SendAsync(mailMessage, mailMessage);
                //client.SendCompleted += Client_SendCompleted;
                //发送
                client.Send(mailMessage);
                txtLog(string.Format("发送邮件成功 主题 {0}-{1} ", mailMessage.Subject, mailMessage.Body));
            }
            catch (Exception ex)
            {
                txtLog("发送邮件失败"+ex.Message);
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
            txtLog(string.Format("发送邮件成功 主题 {0}-{1} ", mailMessage.Subject, mailMessage.Body));
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


    }
}
