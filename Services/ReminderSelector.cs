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

            Add("activity", "walk", "🚶", "起来走走吧", "已经工作{0}分钟啦，站起来走 3～5 分钟就好。");
            Add("activity", "stretch-leg", "🚶", "伸伸腿吧", "坐久了腿会麻，站起来伸伸腿、换个姿势。");
            Add("activity", "leave-desk", "🚶", "离开工位走两步", "起来走两步，去趟洗手间或倒杯水，顺便活动一下。");
            Add("activity", "back", "🚶", "活动活动腰背", "站直身体，轻轻活动腰背，别一直弯着啦。");
            Add("activity", "window", "🚶", "去窗边站一会儿", "去窗边站一会儿，让身体舒展一下，呼吸点新鲜空气。");
            Add("activity", "久坐", "🚶", "久坐伤腰哦", "坐太久腰会酸的，起来活动活动再回来吧。");
            Add("activity", "give-body", "🚶", "给身体几分钟", "给身体几分钟就好，起来走一走，回来更精神。");
            Add("activity", "change-pose", "🚶", "换个姿势吧", "保持一个姿势太久啦，起来活动活动，让身体松一松。");

            Add("eye", "far", "👀", "看看远处吧", "看屏幕很久啦，把视线放到远处放松一下眼睛。");
            Add("eye", "leave-screen", "👀", "离开屏幕一会儿", "眼睛也需要休息，先离开屏幕一小会儿吧。");
            Add("eye", "close", "👀", "闭眼休息一下", "闭上眼睛，让眼睛歇几分钟，会舒服很多哦。");
            Add("eye", "blink", "👀", "眨眨眼吧", "多眨几次眼，缓解一下干涩，眼睛就不那么累啦。");
            Add("eye", "green", "👀", "看看绿植", "让视线离开屏幕，看看窗外的树或远处，给眼睛放个短假。");
            Add("eye", "酸了", "👀", "眼睛酸了吧", "盯屏幕太久啦，抬头看看远处，闭眼歇一小会儿。");
            Add("eye", "rest-eye", "👀", "让眼睛歇歇", "眼睛也累啦，离开屏幕几分钟，它会感谢你的。");
            Add("eye", "window-view", "👀", "望望窗外", "抬头望望窗外远处，让眼睛的焦距换一换。");

            Add("neck", "shoulder", "🧘", "放松肩膀", "把肩膀打开，别一直端着，放松一下就好。");
            Add("neck", "neck", "🧘", "转转脖子", "轻轻转转脖子，缓解肩颈的紧绷，动作慢一点哦。");
            Add("neck", "relax", "🧘", "肩颈放松一下", "别一直低头啦，起来活动活动肩颈。");
            Add("neck", "posture", "🧘", "调整一下坐姿", "调整一下坐姿，让颈部松一松，会舒服很多。");
            Add("neck", "耸肩", "🧘", "耸耸肩吧", "耸耸肩再放下，重复几次，肩颈会轻松不少。");
            Add("neck", "侧偏", "🧘", "偏偏头拉伸一下", "头慢慢偏向一侧，停几秒再换边，拉伸一下脖子。");
            Add("neck", "酸了", "🧘", "肩颈酸了吧", "肩颈酸啦，起来活动活动，别硬撑着。");
            Add("neck", "抬头", "🧘", "抬头看看天花板", "抬头看看天花板，给颈椎减减压，几秒就够。");

            Add("water", "drink", "💧", "喝点水吧", "起来接杯水吧，顺便活动一下身体。");
            Add("water", "cup", "💧", "起来接杯水", "喝口水，让身体也跟着休息一下。");
            Add("water", "walk-water", "💧", "喝水顺便走走", "离开座位，去喝水走两步，一举两得哦。");
            Add("water", "记得喝", "💧", "记得喝水哦", "水喝够了吗？起来接一杯吧，别等渴了才喝。");
            Add("water", "补充", "💧", "补充点水分", "补充点水分，顺便起身走走，身体会更有活力。");
            Add("water", "温水", "💧", "喝杯温水", "喝杯温水，给身体充个电，再继续也不迟。");
            Add("water", "提醒", "💧", "该喝水啦", "忙起来容易忘喝水，起来倒一杯吧。");
            Add("water", "走动", "💧", "起来倒杯水", "起来倒杯水，活动一下再回来，效率更高。");

            Add("break", "fish", "🐟", "摸鱼{1}分钟", "到时间啦，放下工作，摸鱼{1}分钟好好歇一歇。");
            Add("break", "leave", "🐟", "离开电脑一会儿", "离开电脑一会儿，歇{1}分钟再回来，不急的。");
            Add("break", "dont-sit", "🐟", "别一直坐着啦", "起来走走，别一直坐着，身体会感谢你的。");
            Add("break", "放下", "🐟", "放下工作摸会儿鱼", "工作告一段落，摸鱼{1}分钟，回来状态更好。");
            Add("break", "give-self", "🐟", "给自己{1}分钟", "给自己{1}分钟，离开屏幕放松一下，不差这一会儿。");
            Add("break", "歇会儿", "🐟", "歇一会儿吧", "歇一会儿吧，起来活动活动，别把自己绷太紧。");
            Add("break", "休息", "🐟", "该休息啦", "该休息啦，离开座位走走，{1}分钟后见。");
            Add("break", "喘口气", "🐟", "喘口气吧", "喘口气吧，离开电脑{1}分钟，让大脑也歇歇。");
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
