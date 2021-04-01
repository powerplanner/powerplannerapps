using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;

namespace ToolsUniversal
{
    public class WebTimeOffset
    {
        private static int? _storedAnswer;
        private static AsyncLock _lock = new AsyncLock();
        private static DateTime _lastGot;

        public static async Task<int> Get()
        {
            using (await _lock.LockAsync())
            {
                if (_storedAnswer == null || Math.Abs((_lastGot - DateTime.UtcNow).TotalMinutes) >= 30)
                {
                    HttpClient client = new HttpClient();

                    DateTime start = DateTime.UtcNow;
                    string result = await client.GetStringAsync(new Uri("http://powerplanner.cloudapp.net/api/gettime"));
                    DateTime end = DateTime.UtcNow;

                    if (result == null)
                        throw new Exception("Failed to download current time from server.");

                    //comes in like "2013-12-30T02:23:18.1047001Z" with those quotes
                    //parser automatically consideres it UTC time and automatically converts to local, so that's why we go back to UTC
                    DateTime time = DateTime.Parse(result.Trim('"')).ToUniversalTime();

                    _storedAnswer = (int)((time - start).TotalMilliseconds - (end - start).TotalMilliseconds / 1.2); //we know pinging the server will take longer than response, so we set it to 1.2

                    //this ensures that if they switch timezones, it'll have to redownload correct time offset
                    _lastGot = DateTime.UtcNow;
                }

                return _storedAnswer.Value;
            }
        }
    }
}
