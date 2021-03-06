using MeetingApp.Models.Data;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;

namespace MeetingApp.Data
{
    public class MeetingData
    {
        /// <summary>
        /// 会議ID
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        /// 会議タイトル
        /// </summary>

        [JsonProperty("title")]
        public String Title { get; set; }

        /// <summary>
        /// 会議開始時刻
        /// </summary>

        [JsonProperty("startDatetime")]
        public DateTime StartDatetime { get; set; }


        /// <summary>
        /// 会議終了時刻
        /// </summary>
        [JsonProperty("endDatetime")]
        public DateTime EndDatetime { get; set; }


        /// <summary>
        /// 会議が定期true 不定期false
        /// </summary>
        [JsonProperty("regular")]
        public Boolean Regular { get; set; }

        /// <summary>
        /// 会議の管理者（作成者）のuid
        /// </summary>

        [JsonProperty("owner")]
        public int Owner { get; set; }


        /// <summary>
        /// 会議実施場所
        /// </summary>

        [JsonProperty("location")]
        public String Location { get; set; }


        /// <summary>
        /// 会議情報が有効かどうか（終了しているかどうか）
        /// </summary>
        [JsonProperty("isvisible")]
        public Boolean IsVisible { get; set; }



        /// <summary>
        /// 会議で管理者であるか否か
        /// </summary>
        public Boolean IsOwner { get; set; }


        /// <summary>
        /// 会議の参加者であるか否か
        /// </summary>
        public Boolean IsGeneral { get; set; }


        /// <summary>
        /// 会議開始時刻文字列
        /// </summary>
        public string StartTime { get; set; }

        /// <summary>
        /// 会議終了時刻文字列
        /// </summary>
        /// 
        public string EndTime { get; set; }

        /// <summary>
        /// 会議実施日時文字列
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        ///会議に紐づくラベル群
        /// </summary>
        public ObservableCollection<MeetingLabelData> MeetingLabelDatas { get; set; }

        public MeetingData() { }

    }
}
