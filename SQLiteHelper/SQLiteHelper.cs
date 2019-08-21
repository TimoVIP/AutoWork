using BaseFun;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;

namespace SQLiteHelper
{
    public static class SQLiteHelper
    {
        private static SQLiteConnection get_dbConnection()
        {
            string dbpath = appSittingSet.readAppsettings("dbpath");
            if (dbpath == "")
            {
                dbpath = "db.sqlite";
            }
            SQLiteConnection m_dbConnection = null;
            try
            {
                m_dbConnection = new SQLiteConnection("Data Source=" + AppDomain.CurrentDomain.BaseDirectory + dbpath + ";Version=3;Journal Mode=Off;");
                if (m_dbConnection.State != ConnectionState.Open)
                {
                    m_dbConnection.Open();
                }
            }
            catch (SQLiteException ex)
            {
                appSittingSet.Log("数据库打开失败" + ex.Message);
            }
            return m_dbConnection;
        }



        /// <summary>
        /// 查询是否存在记录
        /// </summary>
        /// <param name="sql"></param>
        /// <returns> true 存在记录</returns>
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
        public static bool recorderDbCheck(string sql, out DataTable dt)
        {
            SQLiteConnection m_dbConnection = get_dbConnection();

            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();

            dt = new DataTable();
            dt.Load(reader);

            bool b = reader.HasRows;
            reader.Close();
            m_dbConnection.Close();
            return b;
        }

        public static bool execSql(string sql)
        {
            //return execSql(sql, false);
            List<string> list = new List<string>();
            list.Add(sql);
            return execSql(list);
        }

        /// <summary>
        /// 批量执行SQL 使用了事务
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static bool execSql(List<string> sql_list)
        {
            SQLiteConnection m_dbConnection = get_dbConnection();
            SQLiteTransaction trans = m_dbConnection.BeginTransaction();
            SQLiteCommand command = new SQLiteCommand();
            try
            {
                foreach (var item in sql_list)
                {
                    command.CommandText = item;
                    command.Connection = m_dbConnection;
                    command.ExecuteNonQuery();
                }

                trans.Commit();
                command.Dispose();
                m_dbConnection.Close();
                return true;
            }
            catch (SQLiteException ex)
            {
                trans.Rollback();
                command.Dispose();
                m_dbConnection.Close();
                appSittingSet.Log("数据库执行SQL错误" + ex.Message);
                return false;
            }
        }

        public static string execScalarSql(string sql)
        {
            SQLiteConnection m_dbConnection = get_dbConnection();

            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            object o = command.ExecuteScalar();
            command.Dispose();
            m_dbConnection.Close();
            return o == null ? "" : o.ToString();
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
    }
}
