using MeetingApp.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace MeetingApp
{
    class RestService
    {
        HttpClient _client;

        public RestService()
        {
            _client = new HttpClient();
        }

        //会議情報を単一で取得するAPIコール
        public async Task<MeetingData> GetMeetingDataAsync(string uri)
        {
            MeetingData meetingData = null;
            try
            {
                HttpResponseMessage response = await _client.GetAsync(uri);
                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(content);
                    meetingData = JsonConvert.DeserializeObject<MeetingData>(content);
                    Console.WriteLine(meetingData);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("\tERROR {0}", ex.Message);
            }

            return meetingData;
        }

    }
}
