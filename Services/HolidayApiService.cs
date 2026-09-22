using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace BreakFishApp.Services
{
    /// <summary>
    /// 调用第三方节假日接口（默认 timor.tech）判定某天工作/休息/补班。
    /// - 同步 GetInfo 永不阻塞、永不抛异常：优先内存/本地缓存，未命中回退内置表。
    /// - 后台异步拉取并缓存，下次即可用接口结果。
    /// - 离线或接口失败时自动用内置 HolidayCalendar 兜底。
    /// </summary>
    public sealed class HolidayApiService : IHolidayProvider
    {
        private const string ApiBase = "https://timor.tech/api/holiday/info/";

        private static readonly object Sync = new object();
        private static readonly Dictionary<string, HolidayInfo> Cache = new Dictionary<string, HolidayInfo>();
        private static readonly HashSet<string> Fetching = new HashSet<string>();
        private static bool _prefetching;
        private static readonly string CachePath = Path.Combine(AppPaths.Root, "holiday-cache.json");

        public HolidayApiService()
        {
            LoadCache();
        }

        public HolidayInfo GetInfo(DateTime date)
        {
            var key = date.Date.ToString("yyyy-MM-dd");
            HolidayInfo info;
            lock (Sync)
            {
                if (Cache.TryGetValue(key, out info))
                {
                    return info;
                }
            }

            // 未命中缓存：先用内置表立即给出正确结果，同时后台拉接口刷新。
            EnsureFetching(date);
            return HolidayCalendar.GetInfo(date);
        }

        public DateTime? GetNextHoliday(DateTime from, out string name)
        {
            name = null;
            var today = from.Date;
            var currentName = LookupName(today);
            var cursor = today.AddDays(1);
            var limit = today.AddYears(1);
            while (cursor <= limit)
            {
                var n = LookupName(cursor);
                if (n != null && n != currentName)
                {
                    name = n;
                    return cursor;
                }
                cursor = cursor.AddDays(1);
            }

            // 缓存里找不到：立刻用内置表；同时后台预取，方便以后用接口结果。
            PrefetchNextHoliday(from);
            return HolidayCalendar.GetNextHoliday(from, out name);
        }

        private static string LookupName(DateTime date)
        {
            HolidayInfo info;
            lock (Sync)
            {
                if (Cache.TryGetValue(date.Date.ToString("yyyy-MM-dd"), out info) && info != null && info.Kind == 1)
                {
                    return string.IsNullOrEmpty(info.Name) ? null : info.Name;
                }
            }

            info = HolidayCalendar.GetInfo(date);
            if (info != null && info.Kind == 1 && !string.IsNullOrEmpty(info.Name))
            {
                return info.Name;
            }

            return null;
        }

        private static void EnsureFetching(DateTime date)
        {
            var key = date.Date.ToString("yyyy-MM-dd");
            lock (Sync)
            {
                if (Cache.ContainsKey(key) || Fetching.Contains(key))
                {
                    return;
                }
                Fetching.Add(key);
            }

            Task.Run(() =>
            {
                try
                {
                    var fetched = Fetch(date);
                    if (fetched != null)
                    {
                        lock (Sync)
                        {
                            Cache[key] = fetched;
                        }
                        SaveCache();
                    }
                }
                catch
                {
                    // 静默失败，下次再试
                }
                finally
                {
                    lock (Sync)
                    {
                        Fetching.Remove(key);
                    }
                }
            });
        }

        /// <summary>后台逐日拉取，直到找到下一个节假日或查满 40 天，结果入缓存。</summary>
        private static void PrefetchNextHoliday(DateTime from)
        {
            lock (Sync)
            {
                if (_prefetching)
                {
                    return;
                }
                _prefetching = true;
            }

            Task.Run(() =>
            {
                try
                {
                    var cursor = from.Date.AddDays(1);
                    var limit = from.Date.AddDays(40);
                    while (cursor < limit)
                    {
                        var key = cursor.ToString("yyyy-MM-dd");
                        HolidayInfo info;
                        lock (Sync)
                        {
                            if (Cache.TryGetValue(key, out info) && info != null && info.Kind == 1)
                            {
                                break;
                            }
                        }

                        info = Fetch(cursor);
                        if (info != null)
                        {
                            lock (Sync)
                            {
                                Cache[key] = info;
                            }
                            if (info.Kind == 1)
                            {
                                SaveCache();
                                break;
                            }
                        }
                        cursor = cursor.AddDays(1);
                    }
                    SaveCache();
                }
                catch
                {
                    // 静默失败
                }
                finally
                {
                    lock (Sync)
                    {
                        _prefetching = false;
                    }
                }
            });
        }

        private static HolidayInfo Fetch(DateTime date)
        {
            var url = ApiBase + date.Date.ToString("yyyy-MM-dd");
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.Timeout = 3000;
            request.ReadWriteTimeout = 3000;
            request.UserAgent = "FishBreak/1.0";

            using (var response = request.GetResponse())
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream))
            {
                var json = reader.ReadToEnd();
                return Parse(json);
            }
        }

        private static HolidayInfo Parse(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            try
            {
                var serializer = new JavaScriptSerializer();
                var root = serializer.Deserialize<Dictionary<string, object>>(json);
                if (root == null)
                {
                    return null;
                }

                // timor.tech: {"code":0,"type":{"type":1,"name":"国庆节",...},"holiday":{...}}
                object codeObj;
                if (root.TryGetValue("code", out codeObj) && codeObj != null && codeObj.ToString() != "0")
                {
                    return null;
                }

                object typeObj;
                if (!root.TryGetValue("type", out typeObj) || typeObj == null)
                {
                    return null;
                }

                var type = typeObj as Dictionary<string, object>;
                if (type == null)
                {
                    return null;
                }

                object kindObj;
                if (!type.TryGetValue("type", out kindObj) || kindObj == null)
                {
                    return null;
                }

                int kind;
                if (!int.TryParse(kindObj.ToString(), out kind))
                {
                    return null;
                }

                object nameObj;
                string name = null;
                if (type.TryGetValue("name", out nameObj) && nameObj != null)
                {
                    name = nameObj.ToString();
                    if (kind == 0)
                    {
                        name = null;
                    }
                }

                return new HolidayInfo { Kind = kind, Name = name };
            }
            catch
            {
                return null;
            }
        }

        private static void LoadCache()
        {
            try
            {
                if (!File.Exists(CachePath))
                {
                    return;
                }

                var json = File.ReadAllText(CachePath);
                if (string.IsNullOrEmpty(json))
                {
                    return;
                }

                var serializer = new JavaScriptSerializer();
                var dict = serializer.Deserialize<Dictionary<string, Dictionary<string, object>>>(json);
                if (dict == null)
                {
                    return;
                }

                lock (Sync)
                {
                    foreach (var kv in dict)
                    {
                        int kind = 0;
                        string name = null;
                        if (kv.Value != null)
                        {
                            object k, n;
                            if (kv.Value.TryGetValue("Kind", out k) && k != null)
                            {
                                int.TryParse(k.ToString(), out kind);
                            }
                            if (kv.Value.TryGetValue("Name", out n) && n != null)
                            {
                                name = n.ToString();
                            }
                        }
                        Cache[kv.Key] = new HolidayInfo { Kind = kind, Name = name };
                    }
                }
            }
            catch
            {
                // 缓存读取失败不影响运行
            }
        }

        private static void SaveCache()
        {
            try
            {
                Directory.CreateDirectory(AppPaths.Root);
                var serializer = new JavaScriptSerializer();
                Dictionary<string, HolidayInfo> snapshot;
                lock (Sync)
                {
                    snapshot = new Dictionary<string, HolidayInfo>(Cache);
                }
                File.WriteAllText(CachePath, serializer.Serialize(snapshot));
            }
            catch
            {
                // 缓存写入失败不影响运行
            }
        }
    }
}
