using BaseFun;
using SQLHelper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TimoControl;

namespace ShowData
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void Button1_Click(object sender, EventArgs e)
        {

            string username = TextBox1.Text.Trim();
            string password = TextBox2.Text.Trim();
            string verifycode = TextBox3.Text.Trim();
            if (Session["verifycode"]==null)
            {
                Label1.Text = "检查session设置";
                return;
            }
            if (Session["verifycode"].ToString() !=appSittingSet.md5(verifycode,16))
            {
                Label1.Text = "验证码错误，重新登陆";
                return;
            }
            if (username != "" && password != "")
            {
                string sql = "select count(id) from t_user where username=@username and password=@password";
                password= System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(password, "MD5").ToLower();
                int i =SQLHelper.SQLHelper.ExecuteScalar(sql, new SqlParameter[] { new SqlParameter("@username", username), new SqlParameter("@password", password) });
                if (i==1)
                {
                    Session["user"] = Guid.NewGuid();
                    Response.Redirect("default.aspx");
                }
                else
                {
                    Label1.Text = "用户名、密码错误，重新登陆";  
                }
            }
        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            TextBox1.Text = "";
            TextBox2.Text = "";
            TextBox3.Text = "";
        }
    }
}