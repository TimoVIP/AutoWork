using BaseFun;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace MySQLHelper
{

    public static class MySQLHelper
    {
        public static string connectionString { get; set; }

        public static MySqlConnection conn()
        {
            try
            {
                if (connectionString == null)
                {
                    connectionString = appSittingSet.readAppsettings("MySqlConnect").Split('|')[0];
                }

                MySqlConnection connection = new MySqlConnection(connectionString);
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }
                return connection;
            }
            catch (MySqlException ex)
            {
                appSittingSet.Log(ex.Message+ex.ToString());
                throw;
            }

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
            catch (SqlException ex)
            {
                //throw new Exception(ex.Message);
                appSittingSet.Log(ex.Message + ex.ToString());
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
            return ExecuteSql(SQLString, null);
        }
        public static int ExecuteSql(string SQLString, List<MySqlParameter> para)
        {
            MySqlConnection connection = conn();
            using (MySqlCommand cmd = new MySqlCommand(SQLString, connection))
            {
                try
                {
                    if (para != null && para.Count > 0)
                    {
                        cmd.Parameters.AddRange(para.ToArray());
                    }
                    int rows = cmd.ExecuteNonQuery();
                    cmd.Parameters.Clear();
                    return rows;
                }
                catch (SqlException ex)
                {
                    connection.Close();
                    appSittingSet.Log(ex.Message + ex.ToString());
                    throw;
                }
                finally
                {
                    cmd.Dispose();
                    connection.Close();
                }
            }
        }

        //执行多条SQL语句，实现数据库事务。 
        /// <summary> 
        /// 执行多条SQL语句，实现数据库事务。 
        /// </summary> 
        /// <param name="SQLStringList">多条SQL语句</param> 
        public static bool ExecuteNoQueryTran(List<String> SQLStringList)
        {
            MySqlConnection connection = conn();
            MySqlCommand cmd = new MySqlCommand();
            MySqlTransaction tx = connection.BeginTransaction();
            cmd.Connection = connection;
            cmd.Transaction = tx;
            try
            {
                foreach (var item in SQLStringList)
                {
                    cmd.CommandText = item;
                    cmd.ExecuteNonQuery();
                }
                tx.Commit();
                return true;
            }
            catch(Exception ex)
            {
                tx.Rollback();
                appSittingSet.Log(ex.Message + ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// 执行SQL语句数组，返回影响的记录数
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
            catch (SqlException ex)
            {
                connection.Close();
                appSittingSet.Log(ex.Message + ex.ToString());
                throw;
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
                catch (SqlException ex)
                {
                    connection.Close();
                    appSittingSet.Log(ex.Message + ex.ToString());
                    throw;
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
            object o = GetScalar(SQLString);
            return o == null ? 0 : (int)o;
        }

        /// <summary>
        /// 获取符合条件的首行首列
        /// </summary>
        /// <param name="SQLString"></param>
        /// <returns></returns>
        public static object GetScalar(string SQLString)
        {
            try
            {
                MySqlConnection connection = conn();
                using (MySqlCommand cmd = new MySqlCommand(SQLString, connection))
                {
                    object o = cmd.ExecuteScalar();
                    return o;
                }
            }
            catch (SqlException ex)
            {
                appSittingSet.Log(ex.Message + ex.ToString());
                return null;
            }
        }

        public static T GetScalar<T>(string SQLString)
        {
            object o = GetScalar(SQLString);
            return o != null ? (T)Convert.ChangeType(o, typeof(T)) : default(T);
        }
    }
}
