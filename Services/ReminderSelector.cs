using System;
using System.Collections.Generic;
using BreakFishApp.Models;

namespace BreakFishApp.Services
{
    public class ReminderSelector
    {
        private static readonly string[] Cycle = { "activity", "eye", "neck", "water" };

        private readonly ReminderCatalog _catalog;
        private readonly Random _random;

        public ReminderSelector()
            : this(new ReminderCatalog(), null)
        {
        }

        public ReminderSelector(ReminderCatalog catalog, Random random)
        {
            _catalog = catalog ?? new ReminderCatalog();
            _random = random;
        }

        public ReminderItem Next(string lastType, int workMinutes, int breakMinutes, bool preferBreakType)
        {
            string type;
            if (preferBreakType)
            {
                type = "break";
            }
            else
            {
                var index = Array.IndexOf(Cycle, lastType);
                type = index < 0 ? Cycle[0] : Cycle[(index + 1) % Cycle.Length];
            }

            return _catalog.Pick(type, workMinutes, breakMinutes, _random);
        }
    }

    public class ReminderCatalog
    {
        private readonly Dictionary<string, List<ReminderItem>> _items;

        public ReminderCatalog()
        {
            _items = new Dictionary<string, List<ReminderItem>>(StringComparer.OrdinalIgnoreCase);
            Add("activity", "walk", "🚶", "起来走走", "已经工作{0}分钟了，站起来走3～5分钟吧。");
            Add("activity", "stretch-leg", "🚶", "站起来伸伸腿", "离开椅子，活动一下腿和腰。");
            Add("activity", "leave-desk", "🚶", "离开工位走两步", "起来走两步，换个姿势继续工作。");
            Add("activity", "back", "🚶", "活动一下腰背", "站直身体，轻轻活动腰背。");

            Add("eye", "far", "👀", "看看远处", "离开屏幕一会儿，把视线放到远处。");
            Add("eye", "leave-screen", "👀", "离开屏幕一会儿", "已经盯着屏幕很久了，先休息一下眼睛。");
            Add("eye", "close", "👀", "闭眼休息一下", "闭上眼睛，让眼睛放松几分钟。");
            Add("eye", "blink", "👀", "眨眨眼", "多眨几次眼，缓解一下干涩。");

            Add("neck", "shoulder", "🧘", "放松肩膀", "把肩膀打开，不要一直端着。");
            Add("neck", "neck", "🧘", "活动一下颈部", "轻轻转转脖子，放松颈肩。");
            Add("neck", "relax", "🧘", "肩颈放松一下", "不要一直低头，起来活动肩颈。");
            Add("neck", "posture", "🧘", "不要一直低头", "调整一下坐姿，让颈部松一松。");

            Add("water", "drink", "💧", "喝点水", "起来接杯水，顺便活动一下。");
            Add("water", "cup", "💧", "起来接杯水", "喝口水，让身体也休息一下。");
            Add("water", "walk-water", "💧", "喝口水顺便走走", "离开座位，喝水走两步。");

            Add("break", "fish", "🐟", "摸鱼{1}分钟", "离开电脑一会儿，好好休息{1}分钟。");
            Add("break", "leave", "🐟", "离开电脑一会儿", "先离开屏幕，歇一歇再回来。");
            Add("break", "dont-sit", "🐟", "起来走走，别一直坐着", "已经坐很久了，起来活动一下。");
        }

        public ReminderItem Pick(string type, int workMinutes, int breakMinutes, Random random)
        {
            List<ReminderItem> list;
            if (!_items.TryGetValue(type, out list) || list.Count == 0)
            {
                list = _items["activity"];
            }

            var template = random == null ? list[0] : list[random.Next(list.Count)];
            var title = Format(template.Title, workMinutes, breakMinutes);
            var message = Format(template.Message, workMinutes, breakMinutes);
            return new ReminderItem
            {
                Id = template.Id,
                Type = template.Type,
                Title = title,
                Message = message,
                DurationMinutes = breakMinutes,
                Enabled = true,
                Emoji = template.Emoji
            };
        }

        private void Add(string type, string id, string emoji, string title, string message)
        {
            List<ReminderItem> list;
            if (!_items.TryGetValue(type, out list))
            {
                list = new List<ReminderItem>();
                _items[type] = list;
            }

            list.Add(new ReminderItem
            {
                Id = id,
                Type = type,
                Title = title,
                Message = message,
                Enabled = true,
                Emoji = emoji
            });
        }

        private static string Format(string template, int workMinutes, int breakMinutes)
        {
            return string.Format(template, workMinutes, breakMinutes);
        }
    }
}
