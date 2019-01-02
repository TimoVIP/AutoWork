using MySql.Data.MySqlClient;
using System;
using System.Data;

namespace TimoControl
{
    public static class MySQLHelper
    {
        private static string connectionString;

        public static MySqlConnection conn()
        {
            connectionString = appSittingSet.readAppsettings("MySqlConnect");
            MySqlConnection connection = new MySqlConnection(connectionString);
            if (connection.State== ConnectionState.Closed)
            {
                connection.Open();
            }
            return connection;
        }

        /// <summary>
        /// 执行查询语句，返回DataSet
        /// </summary>
        /// <param name="SQLString">查询语句</param>
        /// <returns>DataSet</returns>
        public static DataSet Query(string SQLString)
        {
            MySqlConnection connection = conn();
            DataSet ds = new DataSet();
            try
            {
                MySqlDataAdapter command = new MySqlDataAdapter(SQLString, connection);
                command.Fill(ds);
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                connection.Close();
            }
            return ds;
        }
        /// <summary>
        /// 执行SQL语句，返回影响的记录数
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <returns>影响的记录数</returns>
        public static int ExecuteSql(string SQLString)
        {
            MySqlConnection connection = conn();
            using (MySqlCommand cmd = new MySqlCommand(SQLString, connection))
            {
                try
                {
                    int rows = cmd.ExecuteNonQuery();
                    return rows;
                }
                catch (System.Data.SqlClient.SqlException e)
                {
                    connection.Close();
                    throw e;
                }
                finally
                {
                    cmd.Dispose();
                    connection.Close();
                }
            }
        }
        /// <summary>
        /// 执行SQL语句，返回影响的记录数
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <returns>影响的记录数</returns>
        public static int ExecuteSql(string[] arrSql)
        {
            MySqlConnection connection = conn();
            try
            {
                //MySqlCommand cmdEncoding = new MySqlCommand(SET_ENCODING, connection);
                //cmdEncoding.ExecuteNonQuery();
                int rows = 0;
                foreach (string strN in arrSql)
                {
                    using (MySqlCommand cmd = new MySqlCommand(strN, connection))
                    {
                        rows += cmd.ExecuteNonQuery();
                    }
                }
                return rows;
            }
            catch (System.Data.SqlClient.SqlException e)
            {
                connection.Close();
                throw e;
            }
            finally
            {
                connection.Close();
            }
        }

        /// <summary>
        /// 是否存在记录
        /// </summary>
        /// <param name="SQLString"></param>
        /// <returns></returns>
        public static bool Exsist(string SQLString)
        {
            MySqlConnection connection = conn();
            using (MySqlCommand cmd = new MySqlCommand(SQLString, connection))
            {
                try
                {
                    MySqlDataReader sdr = cmd.ExecuteReader();
                    return sdr.HasRows;
                }
                catch (System.Data.SqlClient.SqlException e)
                {
                    connection.Close();
                    throw e;
                }
                finally
                {
                    cmd.Dispose();
                    connection.Close();
                }
            }

        }

        /// <summary>
        /// 获取符合条件的记录行数
        /// </summary>
        /// <param name="SQLString"></param>
        /// <returns></returns>
        public static int GetCount(string SQLString)
        {
            object o =GetScalar(SQLString);
            return o == null ? 0 : (int)o;
        }

        /// <summary>
        /// 获取符合条件的首行首列
        /// </summary>
        /// <param name="SQLString"></param>
        /// <returns></returns>
        public static object GetScalar(string SQLString)
        {
            MySqlConnection connection = conn();
            using (MySqlCommand cmd = new MySqlCommand(SQLString, connection))
            {
                try
                {
                    object o = cmd.ExecuteScalar();
                    return o;
                }
                catch (System.Data.SqlClient.SqlException e)
                {
                    connection.Close();
                    throw e;
                }
                finally
                {
                    cmd.Dispose();
                    connection.Close();
                }
            }
        }
    }
}
