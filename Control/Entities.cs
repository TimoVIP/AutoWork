using System;
using System.Collections.Generic;

namespace TimoControl
{
    /// <summary>
    /// 注单对象
    /// </summary>
    public class betData
    {
        public string GameTypeName;

        public string username { get; set; }
        public string betno { get; set; }
        public string gamename { get; set; }
        public decimal betMoney { get; set; }
        public int betTimes { get; set; }
        public string links { get; set; }
        public bool passed { get; set; }
        public string msg { get; set; }
        public string bbid { get; set; }
        public string wallet { get; set; }
        public string betTime { get; set; }
        public string level { get; set; }
        /// <summary>
        /// 最后操作时间，亦用作起始时间的结束
        /// </summary>
        public string lastOprTime { get; set; }
        public string Id { get; set; }
        public string memberId { get; set; }
        public decimal total_money { get; set; }
        public decimal subtotal { get; set; }
        /// <summary>
        /// 最后存款时间，亦用作起始时间的开始
        /// </summary>
        public string lastCashTime { get; set; }
        /// <summary>
        /// 所属活动编号
        /// </summary>
        public string aid { get; set; }
        /// <summary>
        /// 所属活动名称
        /// </summary>
        public string aname { get; set; }
        //public int betAudit { get; set; }//优惠稽核倍数
        //人工存入需要的信息
        //public bool needAudit { get; set; }//是否需要优惠稽核
        /// <summary>
        /// 游戏种类
        /// </summary>
        public string GameCategories { get; set; }
        /// <summary>
        /// 稽核方式
        /// </summary>
        public string AuditType { get; set; }
        /// <summary>
        /// 稽核金额
        /// </summary>
        public decimal Audit { get; set; }
        /// <summary>
        /// 稽核类型
        /// </summary>
        public int Type { get; set; }
        /// <summary>
        /// 实际存提
        /// </summary>
        public bool isReal { get; set; } = true;
        /// <summary>
        /// 前台备注
        /// </summary>
        public string PortalMemo { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Memo { get; set; }
        /// <summary>
        /// 存款类型
        /// </summary>
        public string[] Types { get; set; } = { "Account", "Manual", "ThirdPartyPayment" };
        /// <summary>
        /// 投注笔数
        /// </summary>
        public int hisBetTimes { get; set; }
        /// <summary>
        /// 投注金额
        /// </summary>
        public int hisBetMoney { get; set; }
    }

    /// <summary>
    /// GPK 用户
    /// </summary>
    public class Gpk_UserInfo
    {
        public string Account { get; set; }
        public decimal Balance { get; set; }
        public string JoinTime { get; set; }
        public string MemberLevelSettingName { get; set; }
        public string Name { get; set; }
    }

    /// <summary>
    /// GPK 用户信息
    /// </summary>
    public class Gpk_UserDetail
    {
        public string Account { get; set; }
        public string Birthday { get; set; }
        public string Email { get; set; }
        public string Id { get; set; }
        public string Mobile { get; set; }
        public string SexString { get; set; }
        public decimal Wallet { get; set; }
        public string LatestLogin_IP { get; set; }
        public string LatestLogin_time { get; set; }
        public string LatestLogin_Id { get; set; }
        public string BankAccount { get; set; }
        public string BankName { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string BankMemo { get; set; }
        public string RegisterDevice { get; set; }
        public string RegisterUrl { get; set; }
        public string MemberLevelSettingId { get; set; }
        public string Name { get; set; }
        public string QQ { get; set; }
        public DateTime JoinTime { get; set; }

        public decimal YuebaoPrincipal { get; set; }
        public decimal Balance { get; set; }
    }

    /// <summary>
    /// 历史记录对象
    /// </summary>
    public class HisToryInfor
    {
        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// 操作人
        /// </summary>
        public string Display { get; set; }
        public string IP { get; set; }
        public string Time { get; set; }
    }

    public class SoketObjetRecordQuery
    {
        /// <summary>
        /// 笔数
        /// </summary>
        public int Count { get; set; }
        /// <summary>
        /// 有效投注
        /// </summary>
        public decimal TotalBetAmount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal TotalCommissionable { get; set; }
        /// <summary>
        /// 派彩
        /// </summary>
        public decimal TotalPayoff { get; set; }
    }
    /// <summary>
    /// 5hao站 用户存款信息--银行
    /// </summary>
    public class DepositInfo
    {
        /// <summary>
        /// 中文名
        /// </summary>
        public string Account { get; set; }
        /// <summary>
        /// 存款金额
        /// </summary>
        public decimal Deposit { get; set; }
        /// <summary>
        /// 变动余额
        /// </summary>
        public decimal Balance { get; set; }
        /// <summary>
        /// 记录日期
        /// </summary>
        public DateTime Time { get; set; }
        /// <summary>
        /// 所在文件名
        /// </summary>
        public string FileName { get; set; }
    }

    /// <summary>
    /// 5hao站 用户提交存款申请
    /// </summary>
    public class Recharge
    {
        public string Id { get; set; }
        public string SerialNumber { get; set; }
        /// <summary>
        /// 用户名 英文
        /// </summary>
        public string UserName { get; set; }
        public decimal RechargeMoney { get; set; }
        /// <summary>
        /// 存款渠道
        /// </summary>
        public string AccountName { get; set; }
        public DateTime AddTime { get; set; }
        public string RechargeType { get; set; }
        /// <summary>
        /// 1确认 3取消 2恢复取消
        /// </summary>
        public int OperateType { get; set; }
        public bool IsRepeat { get; set; }
        /// <summary>
        /// 银行卡 户名 中文
        /// </summary>
        public List<string> RealName { get; set; }
        /// <summary>
        /// 此次提交申请用的 姓名 中文
        /// </summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// 5号站 用户银行信息
    /// </summary>
    public class BankInfo
    {
        public string UserName { get; set; }
        public string RealName { get; set; }
        public string BankName { get; set; }
        public string CardNum { get; set; }
        public string Id { get; set; }
    }

    /// <summary>
    /// 站内信主体
    /// </summary>
    public class SendMailBody
    {
        public string BatchParam { get; set; }
        public string ExcelFilePath { get; set; }
        public string MailBody { get; set; }
        /// <summary>
        /// 国歌用,隔开 如ys5164 或者 wgd4416,zyy64002,ys5164
        /// </summary>
        public string MailRecievers { get; set; }
        public string ResendMailID { get; set; }
        public string SearchParam { get; set; }
        /// <summary>
        /// 默认1
        /// </summary>
        public string SendMailType { get; set; }
        public string Subject { get; set; }
        public string SuperSearchRequest { get; set; }
    }
}
