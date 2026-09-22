using System;

namespace BreakFishApp.Services
{
    /// <summary>节假日类型：0=工作日，1=节假日（休息），2=调休补班（上班）。</summary>
    public class HolidayInfo
    {
        public int Kind { get; set; }
        public string Name { get; set; }
    }

    /// <summary>节假日判定来源：内置表 / 第三方接口缓存。</summary>
    public interface IHolidayProvider
    {
        /// <summary>获取某天的节假日信息。同步、永不抛异常，未命中联网缓存时回退内置表。</summary>
        HolidayInfo GetInfo(DateTime date);

        /// <summary>从 from 之后找下一个节假日，用于首页倒计时显示。</summary>
        DateTime? GetNextHoliday(DateTime from, out string name);
    }
}
