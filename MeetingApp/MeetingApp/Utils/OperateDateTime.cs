using System;

namespace MeetingApp.Utils
{
    public class OperateDateTime
    {
        /// <summary>
        /// 現在時刻
        /// </summary>
        public DateTime CurrentDateTime { get; set; }

        /// <summary>
        /// Utcから計算して現在の日本時間を算出する
        /// </summary>
        public OperateDateTime()
        {
            CurrentDateTime = DateTime.UtcNow.AddHours(9);
        }
    }


}
