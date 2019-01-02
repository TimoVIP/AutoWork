using System;

namespace TimoControl
{
    /// <summary>
    /// AMESTime 的摘要说明:
    /// 美东时间的转换
    /// 
    /// 美东时间在UTC-5时区，美国有夏时制，即在夏令时启用之后美东时间比起所在时区早一个小时，即UTC-4 
    /// 在未使用夏令时时美东时间比北京时间（UTC+8时区）晚13个小时，在启用夏令时时美东时间比北京时间晚12个小时
    /// 
    /// 
    /// 美国国会2005年通过的能源法案，夏令时时段：从2007年开始每年3月的第二个星期日开始夏令时，结束日期为11月的第一个星期日。
    /// </summary>
    public class AMESTime
    {
        private static DateTime _thisYearDaylightSavingTimeStart,
            _thisYearDaylightSavingTimeEnd;

        private const int TIMEZONE_OFFSET_DAY_SAVING_LIGHT = -12;
        private const int TIMEZONE_OFFSET = -13;

        public static DateTime BeijingTimeToAMESTime(DateTime beijingTime)
        {
            int offsetHours = (IsNowAMESDayLightSavingTime ? TIMEZONE_OFFSET_DAY_SAVING_LIGHT : TIMEZONE_OFFSET);

            return beijingTime.AddHours(offsetHours);
        }

        public static DateTime AMESNow
        {
            get
            {
                return BeijingTimeToAMESTime(DateTime.Now);
            }
        }

        /// <summary>
        /// 判断当前日期是否是美国夏令时
        /// 从2007年开始每年3月的第二个星期日开始夏令时，结束日期为11月的第一个星期日。
        /// </summary>
        /// <returns>是，返回true，否则为false</returns>
        public static bool IsNowAMESDayLightSavingTime
        {
            get
            {
                return DateTime.UtcNow > DayLightSavingStartTimeUtc
                    && DateTime.UtcNow < DayLightSavingEndTimeUtc;
            }
        }

        /// <summary>
        /// 夏令时开始时间
        /// </summary>
        static DateTime DayLightSavingStartTimeUtc
        {
            get
            {
                if (_thisYearDaylightSavingTimeStart.Year != DateTime.Now.Year)
                {
                    DateTime temp = new DateTime(DateTime.Now.Year, 3, 8, 0, 0, 0);
                    while (temp.DayOfWeek != DayOfWeek.Sunday)
                    {
                        temp = temp.AddDays(1);
                    }
                    _thisYearDaylightSavingTimeStart = temp.AddHours(TIMEZONE_OFFSET);
                }

                return _thisYearDaylightSavingTimeStart;
            }
        }

        /// <summary>
        /// 夏令时结束时间
        /// </summary>
        static DateTime DayLightSavingEndTimeUtc
        {
            get
            {
                if (_thisYearDaylightSavingTimeEnd.Year != DateTime.Now.Year)
                {
                    DateTime temp = new DateTime(DateTime.Now.Year, 11, 1, 0, 0, 0);
                    while (temp.DayOfWeek != DayOfWeek.Sunday)
                    {
                        temp = temp.AddDays(1);
                    }
                    _thisYearDaylightSavingTimeEnd = temp.AddHours(TIMEZONE_OFFSET_DAY_SAVING_LIGHT);
                }
                return _thisYearDaylightSavingTimeEnd;
            }
        }
    }
}
