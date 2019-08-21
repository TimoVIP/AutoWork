using SQLHelper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
//using TimoControl;
using Wuqi.Webdiyer;

namespace ShowData
{
    public partial class Default : System.Web.UI.Page
    {
        DataSet ds;
        SqlDataAdapter dr;
        string SelectStr = "";
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["user"]==null)
            {
                Response.Redirect("Login.aspx");
            }
            if (!IsPostBack)
            {
                //if (SelectStr.Length==0)
                //{
                //    SelectStr += " and state = " + DropDownList1.SelectedValue;
                //}

                //AspNetPager1.AlwaysShow = true;
                //AspNetPager1.PageSize = 10;
                //AspNetPager1.RecordCount = sqlHelper.ExecuteScalar("select count (id) from t_data where 1=1 " + SelectStr);
                RepeaterDataBind();
            }

        }
        private void RepeaterDataBind()
        {
            //string strconn = System.Configuration.ConfigurationManager.ConnectionStrings["submitionsConnectionString"].ToString();
            //dr = new SqlDataAdapter("select * from (select * from t_data where 1= 1 "+SelectStr+" )AS temp ", strconn);
            //ds = new DataSet();
            //dr.Fill(ds, AspNetPager1.PageSize * (AspNetPager1.CurrentPageIndex - 1), AspNetPager1.PageSize, "Article");
            //this.GridView1.DataSource = ds.Tables["Article"];
            //this.GridView1.DataBind();
            List<SqlParameter> list = new List<SqlParameter>();
            if (tbOid.Text!="")
            {
                SelectStr = " and oid like @oid";
                list.Add(new SqlParameter("@oid", "%"+tbOid.Text+"%"));
            }
            if (tbUserName.Text!="")
            {
                SelectStr = " and username like @username";
                list.Add(new SqlParameter("@username", "%"+tbUserName.Text+"%"));
            }
            if (tbstartTime.Text!="")
            {
                SelectStr = " and subtime > '@subtime'";
                list.Add(new SqlParameter("@subtime", tbstartTime.Text));
            }
            if (tbendTime.Text!="")
            {
                SelectStr = " and subtime < '@subtime'";
                list.Add(new SqlParameter("@subtime", tbendTime.Text));
            }
            if (DropDownList1.SelectedValue == "*")
            {

            }
            else
            {
                SelectStr += " and state = @state";
                list.Add(new SqlParameter("@state", DropDownList1.SelectedValue));
            }

            AspNetPager1.AlwaysShow = true;
            AspNetPager1.PageSize = 100;
            AspNetPager1.RecordCount = SQLHelper.SQLHelper.ExecuteScalar("select count (id) from t_data where 1=1 " + SelectStr,list.ToArray());

            string sql = " SELECT TOP "+ AspNetPager1.PageSize + " * FROM ( SELECT TOP("+ AspNetPager1.CurrentPageIndex + " * "+ AspNetPager1.PageSize + ") ROW_NUMBER() OVER(ORDER BY id desc) AS RowNum, * FROM t_data WHERE 1=1 "+SelectStr+") AS tempTable WHERE RowNum BETWEEN("+ AspNetPager1.CurrentPageIndex + " - 1) * "+ AspNetPager1.PageSize+ " + 1 AND "+ AspNetPager1.CurrentPageIndex + " * "+ AspNetPager1.PageSize+ " ORDER BY RowNum";

            DataTable dt = SQLHelper.SQLHelper.ExecuteSelectDataTable(sql, list.ToArray());

            if (true)
            {

            }
            this.GridView1.DataSource = dt;
            this.GridView1.DataBind();
        }


        protected void GridView1_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            string sid = GridView1.DataKeys[e.RowIndex].Value.ToString();//获取绑定的id
            string sql = "update t_data set  state=4  where id=@id";
            int i = SQLHelper.SQLHelper.ExecuteNonQuery(sql, new SqlParameter[] {  new SqlParameter("@id", sid) });
            if (i>0)
            {
                RepeaterDataBind();
            }
            //else
            //{
            //    Response.Write("<script>alert('删除失败');location.href=WebForm1.aspx;</script>");
            //}
        }

        /// <summary>   
        /// 让当前处于修改状态   
        /// </summary>   
        /// <param name="sender"></param>   
        /// <param name="e"></param>   
        protected void GridView1_RowEditing(object sender, GridViewEditEventArgs e)
        {
            GridView1.EditIndex = e.NewEditIndex;
            RepeaterDataBind();
            TextBox tb =(TextBox) GridView1.Rows[e.NewEditIndex].Cells[2].Controls[0];
            tb.Focus();
            tb.Attributes["onFocus"] = "javascript:this.select();";
        }

        /// <summary>   
        /// 让当前行处于绑定状态   
        /// </summary>   
        /// <param name="sender"></param>   
        /// <param name="e"></param>   
        protected void GridView1_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            GridView1.EditIndex = -1;
            RepeaterDataBind();
        }

        /// <summary>   
        /// 更新至数据库   
        /// </summary>   
        /// <param name="sender"></param>   
        /// <param name="e"></param>   
        protected void GridView1_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            string username = ((TextBox)(GridView1.Rows[e.RowIndex].Cells[2].Controls[0])).Text;
            string sid = GridView1.DataKeys[e.RowIndex].Value.ToString();//获取绑定的id
            string sql = "update t_data set username =@username,state=1 where id=@id";
            int i = SQLHelper.SQLHelper.ExecuteNonQuery(sql, new SqlParameter[] { new SqlParameter("@username", username), new SqlParameter("@id", sid) });
            GridView1.EditIndex = -1;
            RepeaterDataBind();

        }
        protected void AspNetPager1_PageChanging1(object src, PageChangingEventArgs e)
        {
            AspNetPager1.CurrentPageIndex = e.NewPageIndex;
            RepeaterDataBind();
        }

        protected void btnQuery_Click(object sender, EventArgs e)
        {
            RepeaterDataBind();
        }

        protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType== DataControlRowType.DataRow)
            {
                Label lbtext = e.Row.Cells[5].FindControl("Label1") as Label;
                LinkButton ledit = e.Row.Cells[6].FindControl("LinkButton1") as LinkButton;
                LinkButton ldel = e.Row.Cells[6].FindControl("LinkButton2") as LinkButton;
                if (lbtext.Text == "已处理" | lbtext.Text == "已删除")
                {
                    ledit.Visible = false;
                    ldel.Visible = false;
                }
            }
        }

        protected string formatStr(string str)
        {
            switch (str)
            {
                default:
                    break;
                case "0":
                    str= "未处理";
                    break;
                case "1":
                    str="待处理";
                    break;
                case "2":
                    str= "已处理";
                    break;
                case "3":
                    str="处理失败";
                    break;
                case "4":
                    str="已删除";
                    break;
            }
            return str;
        }
    }
}