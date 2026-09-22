using System;
using System.Collections;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace BreakFishApp.Services
{
    /// <summary>
    /// 解析年度节假日 JSON。
    /// 支持两种形态：
    /// 1) 按日期为键：{ "2026-01-01": { "date", "name", "isOffDay" } }
    /// 2) holiday-cn：{ "days": [ { "date", "name", "isOffDay" } ] }
    /// isOffDay=true → 放假(Kind=1)；false → 调休补班(Kind=2)。
    /// </summary>
    public static class HolidayYearParser
    {
        public static Dictionary<string, HolidayInfo> Parse(string json)
        {
            var result = new Dictionary<string, HolidayInfo>();
            if (string.IsNullOrEmpty(json))
            {
                return result;
            }

            var serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = int.MaxValue;
            var root = serializer.DeserializeObject(json) as Dictionary<string, object>;
            if (root == null)
            {
                return result;
            }

            object daysObj;
            if (root.TryGetValue("days", out daysObj))
            {
                MergeList(result, daysObj as IEnumerable);
            }

            foreach (var kv in root)
            {
                if (kv.Key == "days" || kv.Key == "year" || kv.Key == "papers")
                {
                    continue;
                }

                if (kv.Key.StartsWith("$"))
                {
                    continue;
                }

                MergeItem(result, kv.Key, kv.Value as Dictionary<string, object>);
            }

            return result;
        }

        public static DateTime? GetNextHoliday(IDictionary<string, HolidayInfo> map, DateTime from, out string name)
        {
            name = null;
            if (map == null || map.Count == 0)
            {
                return null;
            }

            var today = from.Date;
            HolidayInfo current;
            var currentName = map.TryGetValue(today.ToString("yyyy-MM-dd"), out current) && current.Kind == 1
                ? current.Name
                : null;

            var cursor = today.AddDays(1);
            var limit = today.AddYears(1);
            while (cursor <= limit)
            {
                HolidayInfo info;
                if (map.TryGetValue(cursor.ToString("yyyy-MM-dd"), out info) &&
                    info.Kind == 1 &&
                    !string.IsNullOrEmpty(info.Name) &&
                    info.Name != currentName)
                {
                    name = info.Name;
                    return cursor;
                }

                cursor = cursor.AddDays(1);
            }

            return null;
        }

        private static void MergeList(Dictionary<string, HolidayInfo> result, IEnumerable list)
        {
            if (list == null)
            {
                return;
            }

            foreach (var item in list)
            {
                MergeItem(result, null, item as Dictionary<string, object>);
            }
        }

        private static void MergeItem(Dictionary<string, HolidayInfo> result, string keyHint, Dictionary<string, object> item)
        {
            if (item == null)
            {
                return;
            }

            object dateObj;
            var date = keyHint;
            if (item.TryGetValue("date", out dateObj) && dateObj != null)
            {
                date = dateObj.ToString();
            }

            if (string.IsNullOrEmpty(date) || date.Length < 10)
            {
                return;
            }

            date = date.Substring(0, 10);
            DateTime parsed;
            if (!DateTime.TryParse(date, out parsed))
            {
                return;
            }

            date = parsed.ToString("yyyy-MM-dd");

            object offObj;
            var isOff = false;
            if (item.TryGetValue("isOffDay", out offObj) && offObj != null)
            {
                if (offObj is bool)
                {
                    isOff = (bool)offObj;
                }
                else
                {
                    var text = offObj.ToString();
                    isOff = text == "true" || text == "True" || text == "1";
                }
            }

            object nameObj;
            string name = null;
            if (item.TryGetValue("name", out nameObj) && nameObj != null)
            {
                name = nameObj.ToString();
            }

            result[date] = new HolidayInfo
            {
                Kind = isOff ? 1 : 2,
                Name = name
            };
        }
    }
}
