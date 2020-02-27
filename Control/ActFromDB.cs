using BaseFun;
using MySQLHelper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
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
            try
            {
                return MySQLHelper.MySQLHelper.conn().State == ConnectionState.Open;
            }
            catch (Exception ex)
            {
                appSittingSet.Log(ex.Message);
                return false;
            }

        }
        /// <summary>
        /// 获取优惠提交列表
        /// </summary>
        /// <param name="aid"></param>
        /// <returns></returns>
        public static List<betData> getActData(string aid)
        {
            try
            {
                string sql = $"select id,account,addtime,applyfrom,ActivityId from Give where ActivityId={aid} and `Status`= 0 order by id limit 10;";
                if (aid.Contains(","))
                {
                    sql= $"select id,account,addtime,applyfrom,ActivityId from Give where ActivityId in ({aid}) and `Status`= 0 order by id limit 10;";
                }



                DataTable dt = MySQLHelper.MySQLHelper.Query(sql).Tables[0];

                List<betData> list = new List<betData>();

                foreach (DataRow dr in dt.Rows)
                {
                    //解析 applyfrom 内容
                    //if (dr["applyfrom"].ToString() == "" || dr["applyfrom"].ToString() == "[]")
                    //{

                    //}
                    object jo = JsonConvert.DeserializeObject(dr["applyfrom"].ToString());

                    JArray ja = JArray.FromObject(jo);
                    betData b = new betData();
                    b.betno = "";
                    foreach (var item in ja)
                    {
                        if (item["Label"].ToString().Contains("注单") || item["Label"].ToString().Contains("手机"))
                        {
                            b.betno = item["Value"].ToString().Trim();
                            b.PortalMemo = item["Value"].ToString().Trim();
                        }
                        if (item["Label"].ToString().Contains("游戏名称"))
                        {
                            b.gamename = item["Value"].ToString().Trim();
                        }
                        if (item["Label"].ToString().Contains("签到天数"))
                        {
                            string t = item["Value"].ToString().Trim();
                            int ts = 0;
                            if (!int.TryParse(t, out ts))
                            {
                                if (t.Contains("一"))
                                {
                                    ts = 1;
                                }
                                else if (t.Contains("二"))
                                {
                                    ts = 2;
                                }
                                else if (t.Contains("三"))
                                {
                                    ts = 3;
                                }
                                else if (t.Contains("四"))
                                {
                                    ts = 4;
                                }
                                else if (t.Contains("五"))
                                {
                                    ts = 5;
                                }
                                else if (t.Contains("六"))
                                {
                                    ts = 6;
                                }
                                else if (t.Contains("七"))
                                {
                                    ts = 7;
                                }
                                else if (t.Contains("八"))
                                {
                                    ts = 8;
                                }
                                else if (t.Contains("九"))
                                {
                                    ts = 9;
                                }
                                else if (t.Contains("十"))
                                {
                                    ts = 10;
                                }
                            }
                            //if (ts>7)
                            //{
                            //    ts = 7;
                            //}
                            b.PortalMemo = ts.ToString();
                        }
                        if (item["Label"].ToString().Contains("活动方案"))
                        {
                            string t = item["Value"].ToString().Trim();
                            int ts = 1;
                            if (!int.TryParse(t, out ts))
                            {
                                if (t.Contains("一"))
                                {
                                    ts = 1;
                                }
                                else if (t.Contains("二"))
                                {
                                    ts = 2;
                                }
                                else
                                {
                                    ts = 1;
                                }
                            }
                            //if (ts>2)
                            //{
                            //    ts = 1;
                            //}
                            b.Memo = ts.ToString().Trim();
                        }
                    }

                    b.bbid = dr["id"].ToString();
                    b.username = dr["account"].ToString().Trim();
                    b.betTime = dr["addtime"].ToString();
                    b.aid = dr["ActivityId"].ToString();
                    b.passed = true;

                    if (!list.Exists(x => x.bbid == b.bbid))
                    {
                        list.Add(b);
                    }
                }

                return list;
            }
            catch (Exception ex)
            {
                appSittingSet.Log(ex.Message);
                return null;
            }
        }
        /// <summary>
        /// 最后一次-上次成功的记录的时间
        /// </summary>
        /// <param name="bb"></param>
        /// <returns></returns>
        public static betData getActData2_time(betData bb)
        {
            string sql = $"select  id,account,addtime,applyfrom,ActivityId  from Give where Account='{bb.username}' AND ActivityId={bb.aid} AND  `Status`=1 ";
            if (bb.betTime!="")
            {
                //bb.betTime = DateTime.Now.Date.AddDays(1).ToString("yyyy-MM-dd");
                sql += $" and AddTime<'{bb.betTime}'  ";
            }
            if (bb.lastOprTime!="")
            {
                sql += $" and AddTime >'{bb.lastOprTime}'  ";
            }

            sql += "  ORDER BY id desc LIMIT 1 ;";
            bb.Memo = "";
            bb.PortalMemo = "";

            DataTable dt = MySQLHelper.MySQLHelper.Query(sql).Tables[0];
            if (dt.Rows.Count>0)
            {
                bb.lastOprTime = dt.Rows[0]["addtime"].ToString();
                object jo = JsonConvert.DeserializeObject(dt.Rows[0]["applyfrom"].ToString());
                JArray ja = JArray.FromObject(jo);
                foreach (var item in ja)
                {
                    if (item["Label"].ToString().Contains("签到天数"))
                    {
                        string t = item["Value"].ToString().Trim();
                        int ts = 0;
                        if (!int.TryParse(t,out ts))
                        {
                            if (t.Contains("一"))
                            {
                                ts = 1;
                            }
                            else if (t.Contains("二"))
                            {
                                ts = 2;
                            }
                            else if (t.Contains("三"))
                            {
                                ts = 3;
                            }
                            else if (t.Contains("四"))
                            {
                                ts = 4;
                            }
                            else if (t.Contains("五"))
                            {
                                ts = 5;
                            }
                            else if (t.Contains("六"))
                            {
                                ts = 6;
                            }
                            else if (t.Contains("七"))
                            {
                                ts = 7;
                            }
                            else if (t.Contains("八"))
                            {
                                ts = 8;
                            }
                            else if (t.Contains("九"))
                            {
                                ts = 9;
                            }
                            else if (t.Contains("十"))
                            {
                                ts = 10;
                            }
                        }
                        bb.PortalMemo = ts.ToString();
                    }
                    if (item["Label"].ToString().Contains("活动方案"))
                    {
                        string t = item["Value"].ToString().Trim();
                        int ts = 0;
                        if (!int.TryParse(t, out ts))
                        {
                            if (t.Contains("一"))
                            {
                                ts = 1;
                            }
                            else if (t.Contains("二"))
                            {
                                ts = 2;
                            }
                            else
                            {
                                ts =1;
                            }
                        }
                        bb.Memo =ts.ToString().Trim();
                    }
                }
            }
            return bb;
        }

        /// <summary>
        /// 最后一次-上次成功的记录
        /// </summary>
        /// <param name="bb"></param>
        /// <returns></returns>
        //public static betData getActData2_time(betData bb)
        //{
        //    string sql = $"select AddTime from Give where Account='{bb.username}' AND ActivityId={bb.aid} AND  `Status`=1 ORDER BY id desc LIMIT 1 ;";
        //    object o= MySQLHelper.MySQLHelper.GetScalar(sql);

        //    bb.lastOprTime = o != null ? o.ToString() : "";
        //    return bb;
        //}

        /// <summary>
        /// 更改回数据库
        /// </summary>
        /// <param name="bb"></param>
        /// <returns></returns>
        public static bool confirmAct(betData bb)
        {
            try
            {
                //更改数据库状态
                //string sql = $"update Give set Content='{bb.msg}',`Status`={(bb.passed ? 1 : -1)},passtime=now(),HandleMan='robot' where id={bb.bbid};";
                //int i = MySQLHelper.ExecuteSql(sql);

                List<string> list = new List<string>();
                //3号台子
                //list.Add($"update Give set Content='{bb.msg}',`Status`={(bb.passed ? 1 : -1)} where id={bb.bbid};");

                string sql= string.Format(appSittingSet.readConfig()["sql_give_upadte"].ToString(), bb.msg, (bb.passed ? 1 : -1), bb.bbid);


                //其他平台
                //list.Add($"update Give set Content='{bb.msg}',`Status`={(bb.passed ? 1 : -1)},passtime=now(),HandleMan='robot' where id={bb.bbid};");
                //不需要减少
                //list.Add($"update activity set NoApplyNum = NoApplyNum-1, AlreadyApplyNum= AlreadyApplyNum+1 where Id={bb.aid};");
                //bool b =  MySQLHelper.MySQLHelper.ExecuteNoQueryTran(list);
                 int e = MySQLHelper.MySQLHelper.ExecuteSql(sql);
                bool b = e > 0;
                //通过的 记录到sqlite数据库
                if (bb.passed)
                {
                    //bb.msg = "";
                    sql = $"insert  or ignore into record (username, gamename,betno,chargeMoney,pass,msg,subtime,aid,bbid) values ('{ bb.username}', '{ bb.gamename}','{bb.betno }',{ bb.betMoney },{(bb.passed == true ? 1 : 0) },'{ bb.msg }','{DateTime.Now.AddHours(-12).ToString("yyyy-MM-dd HH:mm:ss") }' , {bb.aid},{bb.bbid})";
                    SQLiteHelper.SQLiteHelper.execSql(sql);
                }


                string msg = $"活动{bb.aname}用户{bb.username}订单{bb.betno}优惠金额{bb.betMoney}处理完毕，处理为 {(bb.passed ? "通过" : "不通过")}，回复消息 {bb.msg}";
                appSittingSet.Log(msg);
                //return i > 0;
                return b;
            }
            catch (Exception ex)
            {
                appSittingSet.Log(ex.Message+ ex.ToString());
                return false;
            }

        }



        #region 旧数据库
        public static List<betData> getActData_old(string aid)
        {
            string sql = $"select id,username,FROM_UNIXTIME(addtime) addtime,value from e_submissions where aid={aid} and `Status`= 0 order by id limit 10;";
            DataTable dt = MySQLHelper.MySQLHelper.Query(sql).Tables[0];

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
            object o= MySQLHelper.MySQLHelper.GetScalar(sql);

            bb.lastOprTime = o != null ? o.ToString() : "";
            return bb;
        }

        public static bool confirmAct_old(betData bb)
        {
            //更改数据库状态
            string sql = $"update e_submissions set message= '{bb.msg}',`Status`={(bb.passed ? 1 : 2)} , handletime=unix_timestamp(now()) where id={bb.bbid};";
            int i = MySQLHelper.MySQLHelper.ExecuteSql(sql);
            //记录到sqlite数据库
            //appSittingSet.recorderDb(bb);
            sql = $"insert  or ignore into record (username, gamename,betno,chargeMoney,pass,msg,subtime,aid,bbid) values ('{ bb.username}', '{ bb.gamename}','{bb.betno }',{ bb.betMoney },{(bb.passed == true ? 1 : 0) },'{ bb.msg }','{DateTime.Now.AddHours(-12).ToString("yyyy-MM-dd HH:mm:ss") }' , {bb.aid},{bb.bbid})";
            SQLiteHelper.SQLiteHelper.execSql(sql);
            string msg = $"<{bb.aname}>用户{bb.username}处理完毕，处理为 {(bb.passed ? "通过" : "不通过")}，回复消息 {bb.msg}";
            appSittingSet.Log(msg);
            return i > 0;
        }

        #endregion



        
    }
}
