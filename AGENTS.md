# AI 项目开发规范（AGENTS.md）

## 一、项目简介

- 名称：FishBreak（摸鱼一下），工程名 `BreakFishApp`
- 描述：Windows 托盘工作休息提醒。按工作日、上下班和午休自动生成当天计划，轮换提醒起来活动、护眼、肩颈、喝水。
- 形态：.NET Framework 4.7.2 WinForms 托盘应用

## 二、技术栈

- C# / .NET Framework 4.7.2
- WinForms（`NotifyIcon`、`System.Windows.Forms.Timer`）
- 配置：`%AppData%\FishBreak\settings.json`（`JavaScriptSerializer`）
- 开机启动：当前用户 `Run` 注册表，不需要管理员权限

不要引入数据库、Web、较新 C# 语法（file-scoped namespace、target-typed `new` 等）。

## 三、项目目录说明

```text
BreakFishApp/
├── Program.cs                 入口、单实例、--self-test
├── SelfTests.cs               调度逻辑自测
├── UiTheme.cs                 颜色与字体
├── Models/                    设置、状态、提醒、计划事件
├── Services/                  调度、配置、通知、开机启动
├── Managers/TrayManager.cs    托盘图标与菜单
├── Controls/                  首页 / 计划 / 统计 / 设置
├── Forms/                     MainForm、ReminderDialog
├── Utils/                     时间与 JSON
└── agent.md                   产品规格（勿当代码改）
```

`bin/`、`obj/`、用户 `%AppData%\FishBreak` 不要当源码提交。

## 四、架构说明

```text
MainForm（1 秒 Timer）
  → WorkScheduler.Tick() 用 DateTime 判断状态
  → ReminderDue / StatusNotice
  → NotificationService + ReminderDialog
  → SettingsService / DailyStateService
```

计划由 `ScheduleBuilder` 按「工作分钟」间隔生成（不把休息分钟叠进时间轴，与规格示例 09:50 / 10:40 一致）。跳过、休息结束后按下一次 = 现在 + 工作分钟，并避开午休窗口。休眠唤醒后按当前时间重算，不补过去的提醒。

## 五、开发命令

在安装了 VS / Build Tools 的机器上：

```text
msbuild BreakFishApp.sln /p:Configuration=Debug
bin\Debug\BreakFishApp.exe --self-test
bin\Debug\BreakFishApp.exe
```

## 六、开发原则

- 先改调度逻辑，再改 UI；UI 不直接读写 JSON
- 时间判断用 `IClock` / `DateTime.Now`，禁止用 Tick 次数当经过时间
- 全局只保留一个 UI Timer
- 兼容性以本项目契约为准：设置字段保持 `AppSettings` 现有名称，不要并行双字段

## 七、模块开发规范

| 需求 | 放哪 |
|------|------|
| 新提醒文案 | `ReminderCatalog` |
| 计划生成规则 | `ScheduleBuilder` |
| 状态机 / 暂停 / 延后 | `WorkScheduler` |
| 持久化 | `SettingsService` / `DailyStateService` |
| 托盘菜单 | `TrayManager` |
| 页面 | `Controls/*Page.cs` |

## 八、禁止事项

- 不要每个提醒一个 Timer
- 不要在 Form 里直接写 `settings.json`
- 不要把错过的提醒全部补弹
- 不要提交密钥、用户本机 AppData 数据
- 不要改 `bin/`、`obj/`

## 九、修改流程

1. 读 `agent.md` 对应章节和现有调用链
2. 先补 `SelfTests` 能表达的行为
3. 改 Service，再改 UI
4. 编译 + `--self-test`
5. 手动看托盘、首页倒计时、设置保存
6. 说明影响范围

## 十、代码质量要求

- 命名与规格一致：`WorkScheduler`、`Pause`、`Resume`
- 跟现有 WinForms 手写控件风格，不强制 Designer
- 失败要有中文原因（设置校验），不要空 catch

## 十一、验证要求

- `BreakFishApp.exe --self-test` 退出码 0
- 工作日 09:50 起按类型轮换提醒
- 关闭主窗口进托盘；托盘「退出」才结束进程
- 改下班时间「仅今天」不改默认设置

## 十二、安全规范

- 配置只写 `%AppData%\FishBreak`
- 开机启动只写 `HKCU\...\Run`
- 不要把路径或设置拼进进程命令做多余执行

## 十三、性能要求

- Timer 间隔 1000ms，Tick 保持轻量
- 不要每秒写盘；休息/暂停/提醒/退出时保存当日状态即可

## 十四、输出规范

改代码时说明：改了什么、为什么、影响哪条用户路径、风险（休眠、跨天、午休边界）。

## 十五、开发检查清单

- [ ] 自测通过
- [ ] 只有一个 Timer
- [ ] 设置经 `SettingsService`
- [ ] 午休 / 下班 / 暂停 / 延后仍按 `DateTime` 重算
- [ ] 未把规格里的示例时刻写死在 UI
