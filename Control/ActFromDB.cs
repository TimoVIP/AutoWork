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
        public static List<betData> list { get; set; }
        public static bool loginActivity()
        {
            return MySQLHelper.conn().State == ConnectionState.Open;
        }
        /// <summary>
        /// 获取优惠提交列表
        /// </summary>
        /// <param name="aid"></param>
        /// <returns></returns>
        public static List<betData> getActData_(string aid)
        {
            string sql = $"select id,account,addtime,applyfrom from Give where ActivityId={aid} and `Status`= 0 order by id limit 10;";
            DataTable dt = MySQLHelper.Query(sql).Tables[0];
            if (list==null)
                list = new List<betData>();

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
                        b.betno = item["Value"].ToString();
                        b.PortalMemo = item["Value"].ToString();
                    }
                }

                b.bbid = dr["id"].ToString();
                b.username = dr["account"].ToString();
                b.betTime = dr["addtime"].ToString();
                b.aid = aid;

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
            string sql = string.Format("select AddTime from Give where Account='{0}' AND ActivityId={1} AND  `Status`=1 ORDER BY id desc LIMIT 1 ;",  bb.username,bb.aid);
            object o= MySQLHelper.GetScalar(sql);

            bb.lastOprTime = o != null ? o.ToString() : "";
            return bb;
        }

        public static bool confirmAct(betData bb)
        {
            string sql = string.Format("update Give set Content='{0}',`Status`={1} where id={2};",  bb.msg, bb.passed ? 1 : -1,bb.bbid);
            int i = MySQLHelper.ExecuteSql(sql);
            return i > 0;
        }


    }
}
