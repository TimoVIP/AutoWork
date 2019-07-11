using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;

namespace TimoControl
{
    public static class ActFromDB
    {
        /// <summary>
        /// 需要处理的列表
        /// </summary>
        //public static List<betData> list { get; set; }
        public static bool loginActivity()
        {
            return MySQLHelper.conn().State == ConnectionState.Open;
        }
        /// <summary>
        /// 获取优惠提交列表
        /// </summary>
        /// <param name="aid"></param>
        /// <returns></returns>
        public static List<betData> getActData(string aid)
        {
            string sql = $"select id,account,addtime,applyfrom from Give where ActivityId={aid} and `Status`= 0 order by id limit 10;";
            DataTable dt = MySQLHelper.Query(sql).Tables[0];

            List<betData> list = new List<betData>();

            foreach (DataRow dr in dt.Rows)
            {
                //解析 applyfrom 内容
                object jo = JsonConvert.DeserializeObject(dr["applyfrom"].ToString());

                JArray ja = JArray.FromObject(jo);
                betData b = new betData();
                foreach (var item in ja)
                {
                    if (item["Label"].ToString().Contains("注单") || item["Label"].ToString().Contains("手机"))
                    {
                        b.betno = item["Value"].ToString().Trim();
                        b.PortalMemo = item["Value"].ToString().Trim();
                    }
                }

                b.bbid = dr["id"].ToString();
                b.username = dr["account"].ToString().Trim();
                b.betTime = dr["addtime"].ToString();
                b.aid = aid;
                b.passed = true;

                if (!list.Exists(x => x.bbid == b.bbid))
                {
                    list.Add(b);
                }
            }

            return list;
        }
        /// <summary>
        /// 最后一次-上次成功的记录
        /// </summary>
        /// <param name="bb"></param>
        /// <returns></returns>
        public static betData getActData2_time(betData bb)
        {
            string sql = $"select AddTime from Give where Account='{bb.username}' AND ActivityId={bb.aid} AND  `Status`=1 ORDER BY id desc LIMIT 1 ;";
            object o= MySQLHelper.GetScalar(sql);

            bb.lastOprTime = o != null ? o.ToString() : "";
            return bb;
        }

        /// <summary>
        /// 更改回数据库
        /// </summary>
        /// <param name="bb"></param>
        /// <returns></returns>
        public static bool confirmAct(betData bb)
        {
            //更改数据库状态
            string sql = $"update Give set Content='{bb.msg}',`Status`={(bb.passed ? 1 : -1)} where id={bb.bbid};";
            int i = MySQLHelper.ExecuteSql(sql);
            //记录到sqlite数据库
            appSittingSet.recorderDb(bb);
            string msg = $"用户{bb.username}处理完毕，处理为 {(bb.passed ? "通过" : "不通过")}，回复消息 {bb.msg}";
            appSittingSet.Log(msg);
            return i > 0;
        }



        #region 旧数据库
        public static List<betData> getActData_old(string aid)
        {
            string sql = $"select id,username,FROM_UNIXTIME(addtime) addtime,value from e_submissions where aid={aid} and `Status`= 0 order by id limit 10;";
            DataTable dt = MySQLHelper.Query(sql).Tables[0];

            List<betData> list = new List<betData>();

            foreach (DataRow dr in dt.Rows)
            {
                betData b = new betData();
                //if (aid=="35" )
                //{
                //    //解析 value 内容
                //    JObject jo = (JObject)JsonConvert.DeserializeObject(dr["value"].ToString());
                //    b.betno =jo["34"].ToString();
                //    b.PortalMemo = jo["34"].ToString();
                //}
                //else if (aid=="50")
                //{
                //    JObject jo = (JObject)JsonConvert.DeserializeObject(dr["value"].ToString());
                //    b.betno =jo["54"].ToString();
                //    b.PortalMemo = jo["54"].ToString();
                //}
                //处理单号
                string _tmp = dr["value"].ToString().Replace("\"", ""); ;
                if (_tmp.Contains(":"))
                {
                    if (_tmp.Contains(","))
                        b.betno = _tmp.Split(',')[0].Split(':')[1];
                    else
                        b.betno = _tmp.Split(':')[1].Trim('}');

                    b.PortalMemo = b.betno;
                }

                b.bbid = dr["id"].ToString();
                b.username = dr["username"].ToString();
                b.betTime = dr["addtime"].ToString();
                b.aid = aid;
                b.passed = true;
                list.Add(b);

                //if (!list.Exists(x => x.bbid == b.bbid))
                //{
                //    list.Add(b);
                //}
            }

            return list;
        }

        public static betData getActData2_time_old(betData bb)
        {
            string sql = $"select FROM_UNIXTIME(addtime) addtime from e_submissions where username='{bb.username}' AND aid={bb.aid} AND  `Status`=1 ORDER BY id desc LIMIT 1 ;";
            object o= MySQLHelper.GetScalar(sql);

            bb.lastOprTime = o != null ? o.ToString() : "";
            return bb;
        }

        public static bool confirmAct_old(betData bb)
        {
            //更改数据库状态
            string sql = $"update e_submissions set message= '{bb.msg}',`Status`={(bb.passed ? 1 : 2)} , handletime=unix_timestamp(now()) where id={bb.bbid};";
            int i = MySQLHelper.ExecuteSql(sql);
            //记录到sqlite数据库
            appSittingSet.recorderDb(bb);
            string msg = $"用户{bb.username}处理完毕，处理为 {(bb.passed ? "通过" : "不通过")}，回复消息 {bb.msg}";
            appSittingSet.Log(msg);
            return i > 0;
        }

        #endregion



        
    }
}
