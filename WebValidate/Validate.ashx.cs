
using System.Web;
using System.Web.Script.Serialization;
using System.Data;
using System.Data.SQLite;
namespace WebValidate
{
    /// <summary>
    /// Validate 的摘要说明
    /// </summary>
    public class Validate : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            if (context.Request.RequestType != "POST")
                return;
            //登陆验证 SQLite 数据库

            string Type = context.Request.Headers["Type"];
            string UserName = context.Request.Headers["UserName"];
            string UserPwd = context.Request.Headers["UserPwd"];
            switch (Type)
            {
                case "Validate":
                    string dbpath = System.Web.Configuration.WebConfigurationManager.AppSettings["dbpath"];
                    SQLiteConnection m_dbConnection = new SQLiteConnection("Data Source=" + System.AppDomain.CurrentDomain.BaseDirectory + dbpath + ";Version=3;Journal Mode=Off;");
                    if (m_dbConnection.State != ConnectionState.Open)
                    {
                        m_dbConnection.Open();
                    }
                    string sql = string.Format("SELECT ExpireDate FROM User  WHERE  UserName = '{0}' AND  UserPwd = '{1}' AND  Status = '0' and date(ExpireDate) >  date('now');", UserName, UserPwd);
                    SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                    object o = command.ExecuteScalar();
                    command.Dispose();
                    m_dbConnection.Close();
                    if (o == null)
                        HttpContext.Current.Response.Write(new JavaScriptSerializer().Serialize(new { Result = false, Msg = "验证失败", Data = "null" }));
                    else
                        HttpContext.Current.Response.Write(new JavaScriptSerializer().Serialize(new { Result = true, Msg = "验证通过", Data = new { ExpireDate = o.ToString() } }));
                    break;
                case "":
                    HttpContext.Current.Response.Write(new JavaScriptSerializer().Serialize(new { Result = false, Msg = "错误的请求类型", Data = "null" }));
                    break;
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}