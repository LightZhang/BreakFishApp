# FishBreak「摸鱼一下」

> 英文名：FishBreak  
> 中文名：摸鱼一下  
> 技术：C# + .NET Framework + WinForms  
> 平台：Windows  
> 类型：桌面托盘应用

---

# 一、产品定位

FishBreak 是一个运行在 Windows 托盘中的轻量级工作休息提醒程序。

核心目标：

> 用户设置好上班、下班、午休时间后，程序在每个工作日自动运行，并根据工作周期提醒用户起来走走、保护眼睛、放松肩颈、喝水等。

产品不是单纯的倒计时工具。

核心体验：

```text
上班
 ↓
工作
 ↓
提醒：起来走走
 ↓
继续工作
 ↓
提醒：休息眼睛
 ↓
继续工作
 ↓
提醒：放松肩颈
 ↓
午休
 ↓
下午继续工作
 ↓
下班
```

---

# 二、技术方案

## 2.1 技术栈

```text
C#
.NET Framework
WinForms
Visual Studio
```

建议目标：

```text
.NET Framework 4.7.2
```

保持兼容性，不使用较新的 C# 特性。

---

# 三、程序形态

FishBreak 应该是一个：

> **Windows Tray Application**

程序启动后：

```text
启动程序
 ↓
创建 TrayIcon
 ↓
隐藏主窗口
 ↓
开始调度
```

正常情况下用户不需要一直看到主窗口。

Windows 右下角显示：

```text
🐟
```

---

# 四、程序窗口

主窗口主要用于：

- 查看今天状态
- 查看下一次提醒
- 修改设置
- 查看今日计划
- 查看统计

默认启动：

```text
最小化到托盘
```

---

# 五、核心页面

第一版只需要 4 个页面。

```text
MainForm
│
├── 首页
├── 今日计划
├── 统计
└── 设置
```

不需要复杂导航。

可以使用：

```text
Panel
UserControl
```

实现页面切换。

---

# 六、首页

首页是程序最重要的页面。

建议：

```text
┌──────────────────────────────────┐
│ 🐟 FishBreak                     │
│                                  │
│          正在工作                │
│                                  │
│            36:42                 │
│                                  │
│       距离下一次提醒             │
│                                  │
│ ──────────────────────────────── │
│                                  │
│ 🚶 下一步                        │
│                                  │
│ 起来走走                         │
│ 站起来活动 3～5 分钟             │
│                                  │
│ [立即休息]                       │
│                                  │
│ 今日：工作 4h20m · 休息 25m      │
└──────────────────────────────────┘
```

重点：

> **让用户一眼看到“下一次我要做什么”。**

---

# 七、工作时间设置

设置：

```text
工作日

☑ 周一
☑ 周二
☑ 周三
☑ 周四
☑ 周五
☐ 周六
☐ 周日

上班时间
09:00

下班时间
18:00

午休
☑ 启用

开始
12:00

结束
13:30
```

默认：

```text
周一～周五
09:00～18:00
12:00～13:30
```

---

# 八、摸鱼计划

用户可以直接设置：

```text
工作时间
[50] 分钟

休息时间
[5] 分钟
```

提供预设：

### 轻量

```text
工作 60 分钟
休息 3 分钟
```

### 正常

```text
工作 50 分钟
休息 5 分钟
```

### 摸鱼

```text
工作 40 分钟
休息 5 分钟
```

### 佛系

```text
工作 30 分钟
休息 5 分钟
```

用户也可以自定义。

---

# 九、工作日自动生成计划

例如：

```text
上班 09:00
午休 12:00～13:30
下班 18:00

工作 50 分钟
休息 5 分钟
```

自动计算：

```text
09:00  🟢 开始工作

09:50  🚶 起来走走
10:40  👀 休息眼睛
11:30  🧘 放松肩颈

12:00  🍚 午休

13:30  🟢 开始工作

14:20  💧 喝点水
15:10  🚶 起身活动
16:00  👀 看远处
16:50  🧘 活动肩颈
17:40  🐟 摸鱼5分钟

18:00  🏠 下班
```

不要把这些时间写死。

程序应该根据：

```text
上班时间
下班时间
午休时间
工作时长
休息时长
```

动态计算。

---

# 十、提醒内容

建立统一的提醒模型。

```csharp
public class ReminderItem
{
    public string Id { get; set; }

    public string Type { get; set; }

    public string Title { get; set; }

    public string Message { get; set; }

    public int DurationMinutes { get; set; }

    public bool Enabled { get; set; }
}
```

例如：

```text
Id: walk
Type: activity
Title: 起来走走
Message: 已经工作50分钟了，站起来走3～5分钟吧。
```

---

# 十一、提醒类型

建议第一版：

```text
activity       活动
eye            眼睛
neck           颈肩
water          喝水
break          摸鱼
```

例如：

### 活动

```text
起来走走
站起来伸伸腿
离开工位走两步
活动一下腰背
```

### 眼睛

```text
看看远处
离开屏幕一会儿
闭眼休息一下
眨眨眼
```

### 颈肩

```text
放松肩膀
活动一下颈部
肩颈放松一下
不要一直低头
```

### 喝水

```text
喝点水
起来接杯水
喝口水顺便走走
```

### 摸鱼

```text
摸鱼5分钟
离开电脑一会儿
起来走走，别一直坐着
```

---

# 十二、提醒轮换

不能每次随机到同一种类型。

建议维护一个：

```text
ReminderSelector
```

负责选择下一条提醒。

基本策略：

```text
上一次：activity
↓
下一次：eye
↓
下一次：neck
↓
下一次：water
↓
下一次：activity
```

同一种提醒尽量不要连续出现。

---

# 十三、通知

使用 Windows 原生通知。

第一版优先：

```text
NotifyIcon
```

配合托盘气泡提示。

例如：

```text
🐟 FishBreak

🚶 起来走走

已经工作50分钟了。
站起来活动3～5分钟吧。
```

如果后续需要更漂亮的 Windows Toast，再单独升级。

---

# 十四、提醒交互

提醒出现：

```text
🚶 起来走走

已经工作50分钟了。

站起来走3～5分钟吧。

[开始休息]
[5分钟后提醒]
[跳过]
```

按钮行为：

### 开始休息

进入：

```text
break
```

开始休息倒计时。

### 延后

例如：

```text
当前 10:50
↓
延后5分钟
↓
10:55 再提醒
```

### 跳过

直接进入下一工作周期。

---

# 十五、暂停功能

托盘菜单：

```text
暂停提醒

15分钟
30分钟
1小时
今天
```

暂停期间：

```text
不提醒
不创建新的提醒
```

恢复后：

```text
根据当前时间重新计算下一次提醒
```

不要简单地把原来的 Timer 继续跑。

---

# 十六、临时下班时间

非常实用。

例如正常：

```text
18:00 下班
```

今天想：

```text
17:00 下班
```

设置：

```text
今天下班时间

[17:00]

☑ 仅今天
```

当天计划重新计算。

默认配置不修改。

---

# 十七、状态模型

建立：

```csharp
public enum WorkStatus
{
    Idle,
    Working,
    Breaking,
    Lunch,
    Paused,
    Finished
}
```

状态：

```text
Idle
 ↓
Working
 ↓
Breaking
 ↓
Working
 ↓
Lunch
 ↓
Working
 ↓
Finished
```

---

# 十八、调度器

整个程序的核心类：

```text
WorkScheduler
```

负责：

```text
判断今天是不是工作日
判断当前是不是工作时间
判断当前是不是午休
计算下一次提醒
触发提醒
处理休息
处理暂停
处理下班
```

建议接口：

```csharp
public class WorkScheduler
{
    public void Start();

    public void Stop();

    public void Pause(TimeSpan duration);

    public void Resume();

    public ScheduleState GetCurrentState();

    public ReminderItem GetNextReminder();
}
```

---

# 十九、不要大量使用 Timer

不要：

```text
Timer1
Timer2
Timer3
Timer4
Timer5
```

每个提醒一个 Timer。

建议：

> **一个主调度 Timer + 根据当前时间重新计算下一事件。**

例如：

```text
每秒
 ↓
获取当前时间
 ↓
判断状态
 ↓
判断 nextReminderAt
 ↓
到达时间
 ↓
触发提醒
 ↓
计算下一次
```

这样程序重启、电脑休眠、时间变化之后也容易恢复。

---

# 二十、推荐 Timer

WinForms 使用：

```csharp
System.Windows.Forms.Timer
```

主要用于 UI 倒计时。

例如：

```text
Interval = 1000
```

每秒刷新：

```text
36:42
36:41
36:40
```

真正的时间判断使用：

```csharp
DateTime.Now
```

不要依赖：

```text
Timer Tick 次数
```

来计算实际经过时间。

---

# 二十一、电脑休眠处理

这是桌面软件必须考虑的。

例如：

```text
10:00 工作
10:30 电脑休眠
11:30 唤醒
```

不能认为：

```text
Timer 一直运行
```

应该重新根据：

```text
DateTime.Now
```

判断当前状态。

如果已经超过多个提醒：

> 不需要把过去的提醒全部补出来。

直接：

```text
重新计算下一次提醒
```

---

# 二十二、系统托盘

建立：

```text
TrayManager
```

负责：

```text
NotifyIcon
ContextMenuStrip
```

菜单：

```text
🐟 FishBreak

🟢 正在工作
下一次：10:40 休息眼睛

────────────────

立即休息

暂停提醒
├── 15分钟
├── 30分钟
├── 1小时
└── 今天

打开主窗口

设置

退出
```

---

# 二十三、开机启动

第一版提供：

```text
☑ Windows 开机自动启动
```

建议使用 Windows 用户启动项/注册表方式。

不需要管理员权限。

---

# 二十四、配置存储

第一版：

> **不使用数据库。**

建议：

```text
AppData
└── FishBreak
    └── settings.json
```

例如：

```json
{
  "workDays": [1, 2, 3, 4, 5],
  "startTime": "09:00",
  "endTime": "18:00",
  "lunchStart": "12:00",
  "lunchEnd": "13:30",
  "workMinutes": 50,
  "breakMinutes": 5,
  "autoStart": true,
  "notificationEnabled": true,
  "soundEnabled": true
}
```

---

# 二十五、配置管理

建立：

```text
SettingsService
```

负责：

```text
Load()
Save()
Reset()
```

不要让 UI 直接操作 JSON。

例如：

```csharp
var settings = settingsService.Load();

settings.WorkMinutes = 50;

settingsService.Save(settings);
```

---

# 二十六、项目目录

推荐：

```text
FishBreak/
│
├── FishBreak.sln
│
├── FishBreak/
│   │
│   ├── Program.cs
│   ├── MainForm.cs
│   ├── MainForm.Designer.cs
│   │
│   ├── Forms/
│   │   ├── MainForm.cs
│   │   ├── SettingsForm.cs
│   │   └── StatisticsForm.cs
│   │
│   ├── Controls/
│   │   ├── TimerControl.cs
│   │   ├── NextReminderControl.cs
│   │   └── ScheduleControl.cs
│   │
│   ├── Models/
│   │   ├── AppSettings.cs
│   │   ├── ReminderItem.cs
│   │   ├── WorkSchedule.cs
│   │   ├── DailyState.cs
│   │   └── WorkStatus.cs
│   │
│   ├── Services/
│   │   ├── WorkScheduler.cs
│   │   ├── ReminderService.cs
│   │   ├── SettingsService.cs
│   │   ├── NotificationService.cs
│   │   └── StatisticsService.cs
│   │
│   ├── Managers/
│   │   └── TrayManager.cs
│   │
│   ├── Utils/
│   │   ├── DateTimeHelper.cs
│   │   └── SystemHelper.cs
│   │
│   └── Resources/
│
└── README.md
```

---

# 二十七、代码组织原则

保持简单。

原则：

```text
UI
 ↓
Service
 ↓
Model
```

例如：

```text
SettingsForm
 ↓
SettingsService
 ↓
AppSettings
```

不要让：

```text
Form
 ↓
直接修改 JSON
```

---

# 二十八、命名规范

类：

```text
WorkScheduler
ReminderService
SettingsService
NotificationService
```

方法：

```text
Start()
Stop()
Pause()
Resume()
Load()
Save()
CalculateNextReminder()
ShowNotification()
```

变量：

```text
currentTime
nextReminderAt
workMinutes
breakMinutes
```

不要：

```text
a
b
tmp
data1
doSomething()
```

---

# 二十九、第一版开发顺序

不要一开始同时开发所有页面。

按照这个顺序：

## 第一步：基础程序

完成：

```text
WinForms
+
NotifyIcon
+
程序启动
+
托盘
```

---

## 第二步：工作时间

实现：

```text
周一～周五
09:00～18:00
12:00～13:30
```

---

## 第三步：调度器

实现：

```text
工作状态
午休状态
下班状态
下一次提醒
```

---

## 第四步：提醒

实现：

```text
起来走走
保护眼睛
放松肩颈
喝水
```

轮换。

---

## 第五步：休息倒计时

实现：

```text
50分钟工作
↓
5分钟休息
↓
继续工作
```

---

## 第六步：设置

实现：

```text
工作日
上班时间
下班时间
午休时间
工作时长
休息时长
```

---

## 第七步：暂停 / 延后

实现：

```text
5分钟后提醒
15分钟暂停
30分钟暂停
今天暂停
```

---

## 第八步：统计

最后再做：

```text
今日工作
今日休息
提醒次数
跳过次数
```

---

# 三十、MVP 完成标准

第一版完成后，用户安装程序：

```text
启动 FishBreak
 ↓
设置
 ↓
09:00 上班
18:00 下班
12:00～13:30 午休
工作50分钟
休息5分钟
 ↓
点击保存
 ↓
程序进入托盘
```

到 09:50：

```text
🐟 起来走走
```

到 10:40：

```text
👀 休息一下眼睛
```

到 11:30：

```text
🧘 放松肩颈
```

12:00：

```text
🍚 午休
```

13:30：

```text
🟢 下午继续工作
```

18:00：

```text
🏠 下班啦
```

然后停止当天提醒。

**做到这里，第一版产品就已经成立。**

---

# 三十一、后续可扩展方向

未来可以增加：

```text
电脑空闲检测
会议自动暂停
节假日
自定义事件
自定义提醒
快捷键
开机启动
统计图表
主题
声音
AI 对话设置
```

AI 可以最后再加入。

例如：

> “我今天5点下班。”

程序自动修改：

```text
今日下班 = 17:00
```

或者：

> “最近颈椎不舒服，多提醒我活动肩膀。”

自动提高：

```text
neck
shoulder
activity
```

类型的提醒频率。

---

# 三十二、最终产品体验

FishBreak 不应该让用户觉得：

> “又一个计时器。”

而应该是：

> **“它帮我安排好了今天什么时候该起来动一下。”**

每天：

```text
启动电脑
    ↓
FishBreak 自动运行
    ↓
工作
    ↓
提醒
    ↓
休息
    ↓
工作
    ↓
休息
    ↓
下班
    ↓
自动结束
```

核心原则：

> **少设置、自动运行、提醒明确、不打扰工作。**