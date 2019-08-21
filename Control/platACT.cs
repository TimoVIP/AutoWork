using BaseFun;
using System.Collections.Generic;

namespace TimoControl
{
    /// <summary>
    /// 优惠大厅
    /// </summary>
    public static class platACT
    {

        /// <summary>
        /// 登录优惠大厅并跳转
        /// </summary>
        /// <returns></returns>
        public static bool loginActivity()
        {
            bool r = false;
            switch (appSittingSet.readAppsettings("GetDataType"))
            {
                case "1":
                    r = ActDataFromWeb.login();
                    break;
                case "2":
                    r = ActFromDB.loginActivity();
                    break;
                case "3":
                    r = ActFromDB.loginActivity();
                    break;
            }
            return r;

        }

        /// <summary>
        /// 获取注单数据
        /// </summary>
        /// <returns></returns>
        public static List<betData> getActData(string aid)
        {
            List<betData> list = new List<betData>();
            switch (appSittingSet.readAppsettings("GetDataType"))
            {
                case "1":
                    list = ActDataFromWeb.getData(aid);
                    break;
                case "2":
                    list = ActFromDB.getActData_old(aid);
                    break;
                case "3":
                    list = ActFromDB.getActData(aid);
                    break;
            }
            return list;

        }

        /// <summary>
        /// 回填活动结果
        /// </summary>
        /// <param name="bb"></param>
        /// <returns></returns>
        public static bool confirmAct(betData bb)
        {
            bool r = false;
            switch (appSittingSet.readAppsettings("GetDataType"))
            {
                case "1":
                    r = ActDataFromWeb.confirm(bb);
                    break;
                case "2":
                    r = ActFromDB.confirmAct_old(bb);
                    break;
                case "3":
                    r = ActFromDB.confirmAct(bb);
                    break;
            }
            return r;
        }


        /// <summary>
        /// 以小博大 获取上一次设置的时间
        /// </summary>
        /// <param name="bb"></param>
        /// <returns></returns>
        public static betData getActData2_time(betData bb)
        {

            switch (appSittingSet.readAppsettings("GetDataType"))
            {
                case "1":
                    bb = ActDataFromWeb.getData2_time(bb);
                    break;
                case "2":
                    bb = ActFromDB.getActData2_time_old(bb);
                    break;
                case "3":
                    bb = ActFromDB.getActData2_time(bb);
                    break;
            }
            return bb;

        }

    }
}
