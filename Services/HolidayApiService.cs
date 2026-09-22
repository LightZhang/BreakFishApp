using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace BreakFishApp.Services
{
    /// <summary>
    /// 按年拉取节假日接口，成功后写入本地缓存；之后优先读缓存，不每次联网。
    /// 接口：NateScarlet/holiday-cn（字段 date / name / isOffDay），也兼容按日期为键的同一结构。
    /// </summary>
    public sealed class HolidayApiService : IHolidayProvider
    {
        private static readonly string[] UrlTemplates = new[]
        {
            "https://cdn.jsdelivr.net/gh/NateScarlet/holiday-cn@master/{0}.json",
            "https://raw.githubusercontent.com/NateScarlet/holiday-cn/master/{0}.json"
        };

        private static readonly object Sync = new object();
        private static readonly Dictionary<string, HolidayInfo> Cache = new Dictionary<string, HolidayInfo>();
        private static readonly HashSet<int> FetchingYears = new HashSet<int>();
        private static readonly HashSet<int> LoadedYears = new HashSet<int>();

        public HolidayApiService()
        {
            TryEnableTls12();
            LoadAllYearCaches();
            EnsureYear(DateTime.Today.Year);
            EnsureYear(DateTime.Today.Year + 1);
        }

        public HolidayInfo GetInfo(DateTime date)
        {
            EnsureYear(date.Year);
            var key = date.Date.ToString("yyyy-MM-dd");
            lock (Sync)
            {
                HolidayInfo info;
                if (Cache.TryGetValue(key, out info))
                {
                    return info;
                }
            }

            return HolidayCalendar.GetInfo(date);
        }

        public DateTime? GetNextHoliday(DateTime from, out string name)
        {
            EnsureYear(from.Year);
            EnsureYear(from.Year + 1);

            Dictionary<string, HolidayInfo> snapshot;
            lock (Sync)
            {
                snapshot = new Dictionary<string, HolidayInfo>(Cache);
            }

            var next = HolidayYearParser.GetNextHoliday(snapshot, from, out name);
            if (next.HasValue)
            {
                return next;
            }

            return HolidayCalendar.GetNextHoliday(from, out name);
        }

        private static void EnsureYear(int year)
        {
            lock (Sync)
            {
                if (FetchingYears.Contains(year))
                {
                    return;
                }

                var loaded = LoadedYears.Contains(year) || TryLoadYearFile(year);
                if (loaded && !YearCacheStale(year))
                {
                    return;
                }

                FetchingYears.Add(year);
            }

            Task.Run(() => FetchAndCacheYear(year));
        }

        private static void FetchAndCacheYear(int year)
        {
            try
            {
                string json = null;
                for (var i = 0; i < UrlTemplates.Length; i++)
                {
                    json = Download(string.Format(UrlTemplates[i], year));
                    if (!string.IsNullOrEmpty(json))
                    {
                        break;
                    }
                }

                if (string.IsNullOrEmpty(json))
                {
                    return;
                }

                var parsed = HolidayYearParser.Parse(json);
                if (parsed.Count == 0)
                {
                    return;
                }

                lock (Sync)
                {
                    foreach (var kv in parsed)
                    {
                        Cache[kv.Key] = kv.Value;
                    }

                    LoadedYears.Add(year);
                }

                var path = YearCachePath(year);
                Directory.CreateDirectory(AppPaths.Root);
                File.WriteAllText(path, json);
            }
            catch
            {
                // 静默失败，继续用已有缓存或内置表
            }
            finally
            {
                lock (Sync)
                {
                    FetchingYears.Remove(year);
                }
            }
        }

        private static string Download(string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.Timeout = 8000;
            request.ReadWriteTimeout = 8000;
            request.UserAgent = "FishBreak/1.0";
            using (var response = request.GetResponse())
            using (var stream = response.GetResponseStream())
            {
                if (stream == null)
                {
                    return null;
                }

                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        private static void LoadAllYearCaches()
        {
            try
            {
                if (!Directory.Exists(AppPaths.Root))
                {
                    return;
                }

                var files = Directory.GetFiles(AppPaths.Root, "holiday-*.json");
                for (var i = 0; i < files.Length; i++)
                {
                    var name = Path.GetFileNameWithoutExtension(files[i]);
                    if (name == null || !name.StartsWith("holiday-"))
                    {
                        continue;
                    }

                    int year;
                    if (!int.TryParse(name.Substring("holiday-".Length), out year))
                    {
                        continue;
                    }

                    TryLoadYearFile(year);
                }
            }
            catch
            {
            }
        }

        private static bool TryLoadYearFile(int year)
        {
            try
            {
                var path = YearCachePath(year);
                if (!File.Exists(path))
                {
                    return false;
                }

                var json = File.ReadAllText(path);
                var parsed = HolidayYearParser.Parse(json);
                if (parsed.Count == 0)
                {
                    return false;
                }

                lock (Sync)
                {
                    foreach (var kv in parsed)
                    {
                        Cache[kv.Key] = kv.Value;
                    }

                    LoadedYears.Add(year);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool YearCacheStale(int year)
        {
            try
            {
                var path = YearCachePath(year);
                if (!File.Exists(path))
                {
                    return true;
                }

                return DateTime.Now - File.GetLastWriteTime(path) > TimeSpan.FromDays(7);
            }
            catch
            {
                return true;
            }
        }

        private static string YearCachePath(int year)
        {
            return Path.Combine(AppPaths.Root, "holiday-" + year + ".json");
        }

        private static void TryEnableTls12()
        {
            try
            {
                ServicePointManager.SecurityProtocol |= (SecurityProtocolType)3072;
            }
            catch
            {
            }
        }
    }
}
