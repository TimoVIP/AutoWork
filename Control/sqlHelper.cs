using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimoControl
{
    public class sqlHelper
    {
        /// <summary>
        /// 创建一个已打开的连接
        /// </summary>
        /// <returns></returns>
        private static SqlConnection CreateConn()
        {
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["conn"].ConnectionString);
            conn.Open();
            return conn;
        }

        public static bool ConnState()
        {
            SqlConnection conn = CreateConn();
            if (conn.State == ConnectionState.Open)
            {
                conn.Close();
                return true;
            }
            else
                return false;
        }
        /// <summary>
        /// 执行sql语句，返回受影响的行数
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static int ExecuteNonQuery(string sql)
        {
            return ExecuteNonQuery(sql, null);
        }

        /// <summary>
        /// 执行sql语句，返回受影响的行数
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static int ExecuteNonQuery(string sql, SqlParameter[] ps)
        {
            SqlConnection conn = CreateConn();
            SqlCommand cmd = new SqlCommand(sql, conn);
            try
            {
                if (ps != null)
                {
                    cmd.Parameters.AddRange(ps);
                }
                int i =cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();//多了这一句，就解决了问题
                return i;
            }
            catch (Exception ex)
            {
                return 0;
            }
            finally
            {
                conn.Close();
            }
        }
        public static int ExecuteScalar(string sql)
        {
            return ExecuteScalar(sql, null);
        }
        public static int ExecuteScalar(string sql, SqlParameter[] ps)
        {
            SqlConnection conn = CreateConn();
            SqlCommand cmd = new SqlCommand(sql, conn);
            try
            {
                if (ps != null)
                {
                    cmd.Parameters.AddRange(ps);
                }
                object o =  cmd.ExecuteScalar();
                cmd.Parameters.Clear();//多了这一句，就解决了问题
                return o == null ? -1 : (int)o;
            }
            catch (Exception ex)
            {
                return -1;
            }
            finally
            {
                conn.Close();
            }
        }

        /// <summary>
        /// 执行sql语句,返回一个结果表
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static DataTable ExecuteSelectDataTable(string sql)
        {
            return ExecuteSelectDataTable(sql, null);
        }

        /// <summary>
        /// 执行sql语句,返回一个结果表
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="ps"></param>
        /// <returns></returns>
        public static DataTable ExecuteSelectDataTable(string sql, SqlParameter[] ps = null)
        {

            SqlConnection conn = CreateConn();
            SqlCommand cmd = new SqlCommand(sql, conn);
            try
            {
                if (ps != null)
                {
                    cmd.Parameters.AddRange(ps);
                }
                //SqlDataAdapter sda = new SqlDataAdapter(cmd);
                SqlDataReader read = cmd.ExecuteReader();
                DataTable table = new DataTable();
                table.Load(read);
                //DataSet ds = new DataSet();
                //sda.Fill(ds);
                cmd.Parameters.Clear();//多了这一句，就解决了问题
                return table;
            }
            finally
            {
                conn.Close();
            }
        }


        //public static SqlDataReader ExecuteReader(string cmdText, CommandType cmdType, params SqlParameter[] cmdParms)
        //{
        //    SqlCommand cmd = new SqlCommand();
        //    SqlConnection conn = CreateConn();

        //    try
        //    {
        //        ProCommand(cmd, conn, cmdText, cmdType, cmdParms);
        //        SqlDataReader rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
        //        //cmd.Parameters.Clear();//放到这里，返回参数会被清空。
        //        return rdr;
        //    }
        //    catch
        //    {
        //        conn.Close();
        //        throw;
        //    }
        //}


        /// <summary>
        /// 分页查询函数
        /// </summary>
        /// <param name="connStr">数据库连接字符串</param>
        /// <param name="strSql">sql语句</param>
        /// <param name="Params">参数</param>
        /// <param name="pageSize">每页条数</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="order">排序字段</param>
        /// <param name="sort">排序放</param>
        /// <returns>备注：查询效率偏低</returns>
        public static DataTable Pagination(string strSql, List<SqlParameter> Params, int pageSize, int pageIndex, string order, string sort)
        {
            SqlConnection conn = CreateConn();
            SqlCommand cmd = new SqlCommand(strSql, conn);
            SqlDataAdapter ada = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            cmd.Connection = conn;
            string sqlFinal = string.Format(@"WITH tmp2 as( SELECT ROW_NUMBER() OVER(ORDER BY {1} {2}) 
                   AS rownum,* from ({0}) tmp1)
                   select  (select count(*) from tmp2)total,* from tmp2 where tmp2.rownum BETWEEN
                   @startRow and @endRow", strSql, order, sort);
            if (Params==null)
            {
                Params = new List<SqlParameter>();
            }
            Params.Add(new SqlParameter("@startRow", (pageIndex - 1) * pageSize + 1) { SqlDbType = SqlDbType.Int });
            Params.Add(new SqlParameter("@endRow", pageIndex * pageSize) { SqlDbType = SqlDbType.Int });
            cmd.CommandText = sqlFinal;
            for (int i = 0; i < Params.Count; i++)
            {
                cmd.Parameters.Add(Params[i]);
            }
            ada.Fill(dt);
            cmd.Parameters.Clear();//多了这一句，就解决了问题
            return dt;
        }


        /// <summary>
        /// 分页查询函数
        /// </summary>
        /// <param name="connStr">数据库连接字符串</param>
        /// <param name="strSql">sql语句</param>
        /// <param name="Params">参数</param>
        /// <param name="pageSize">每页条数</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="order">排序字段</param>
        /// <param name="sort">排序放</param>
        /// <returns>备注：查询效率高，但是会建临时表</returns>
        public static DataTable Pagination2(string strSql, List<SqlParameter> Params, int pageSize, int pageIndex, string order, string sort)
        {
            SqlConnection conn = CreateConn();
            SqlCommand cmd = new SqlCommand(strSql, conn);
            SqlDataAdapter ada = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            cmd.Connection = conn;
            string sqlFinal = string.Format(@"SELECT ROW_NUMBER() OVER(ORDER BY {1} {2}) 
                   AS rownum,* into #tmp2 from ({0}) tmp1;
                   select  (select count(*) from #tmp2)total,* from #tmp2 where #tmp2.rownum BETWEEN
                   @startRow and @endRow", strSql, order, sort);
            Params.Add(new SqlParameter("@startRow", (pageIndex - 1) * pageSize + 1) { SqlDbType = SqlDbType.Int });
            Params.Add(new SqlParameter("@endRow", pageIndex * pageSize) { SqlDbType = SqlDbType.Int });
            cmd.CommandText = sqlFinal;
            for (int i = 0; i < Params.Count; i++)
            {
                cmd.Parameters.Add(Params[i]);
            }
            ada.Fill(dt);
            cmd.Parameters.Clear();//多了这一句，就解决了问题
            return dt;
        }
    }
}

