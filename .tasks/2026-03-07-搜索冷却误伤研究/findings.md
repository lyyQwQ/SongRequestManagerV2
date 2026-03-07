# Findings

## Status Snapshot
- Stage: Research
- Status Bar: [>R -P -A -I -V]
- Task: 搜索冷却误伤研究
- Date: 2026-03-07

## Task Snapshot
- User goal: 只读确认 SRM refactor 中“搜索被队列冷却误伤”是否仍存在，并梳理搜索按钮、搜索结果入队、正式点歌命令三条路径。
- Constraints: 不改文件；重点检查 `SongRequestManagerV2/Bots/RequestBot.cs`、`SongRequestManagerV2/Models/CommandManager.cs`，以及任何 `ParseState`、`CmdFlags`、`LookupSongs`、`AddSearchResultToQueue`、`QueueSong`、`ProcessSongRequest` 相关代码。
- Out of scope: 不做修复、不做 Git 写操作、不对运行时行为做未经代码支撑的猜测。

## Open Questions
- 无阻塞性开放问题；代码路径已足够判断本次 bug 是否仍存在。

## Exploration Checklist
- [x] Clarify the real problem
- [x] Identify constraints from repo/docs/user input
- [x] Capture raw evidence before context is lost
- [x] Surface material unknowns that require a user decision

## Evidence Log
- 已确认仓库根目录：`C:/tools/GameRelated/beatsaber/workspace/mods/SongRequestManagerV2_139_refactor`。
- 首轮全文检索命中关键位置：
  - `SongRequestManagerV2/Bots/RequestBot.cs`
  - `SongRequestManagerV2/Models/CommandManager.cs`
  - `SongRequestManagerV2/Models/ParseState.cs`
  - `SongRequestManagerV2/Views/RequestBotListView.cs`
  - `SongRequestManagerV2/Statics/Enums.cs`
- UI 命中显示 `RequestBotListView.cs:569` 调用 `_bot.AddSearchResultToQueue(_bot.CurrentSong)`，说明搜索结果入队存在独立入口，未必经聊天命令解析。
- `RequestBot.cs` 已命中：
  - 本地搜索触发：`!addnew/top`、`!addpp/top/mod/pp`、`!addsongs/top ...`、`!makesearchdeck ...`、`!addsongs/top/mod ...`
  - 关键流程方法：`ProcessSongRequest`、`AddSearchResultToQueue`
- `ParseState.cs`、`Enums.cs` 已命中 `CmdFlags` 权限/静默/禁用等逻辑，待确认是否也承载队列冷却或请求限额。
- 已确认搜索按钮路径：
  - `RequestBot.Search()` 调用 `Parse(GetLoginUser(), \"!addsongs/top <keyword>\", CmdFlags.Local)`。
  - `ParseState.ParseCommand()` 解析 `!addsongs`；`/top` 子命令将 `state.Flags |= CmdFlags.MoveToTop`。
  - `CommandManager` 将 `!addsongs` 绑定到 `RequestBot.Addsongs(ParseState)`。
  - `Addsongs()` 调 `GetSongListFromResults(...)`，然后对结果逐个执行 `QueueSong(state, entry)`。
  - `QueueSong()` 仅创建 `RequestStatus.SongSearch` 条目并塞入 `RequestManager.RequestSongs`，不进入 `ProcessSongRequest()`。
- 已确认搜索结果入队路径：
  - `RequestBotListView.AddToQueueClick()` 仅在当前条目 `Status == SongSearch` 时调用 `_bot.AddSearchResultToQueue(_bot.CurrentSong)`。
  - `AddSearchResultToQueue()` 直接把该条目的状态改成 `Queued`，重排列表，写重复列表与请求文件，刷新 UI。
  - `AddSearchResultToQueue()` 内没有调用 `ProcessSongRequest()`、`CheckRequest()`、`QueueSong()`，也没有触发 `RequestTracker` 限额检查。
- 已确认正式点歌路径：
  - `CommandManager` 将 `!bsr / !request / !add / !srm / !sr / 点歌` 绑定到 `RequestBot.ProcessSongRequest(ParseState)`。
  - `ProcessSongRequest()` 先检查 `RequestQueueOpen`，再检查 `RequestTracker[user].numRequests >= Utility.GetRequestLimit(...)`。
  - 通过后把 `RequestInfo` 放入 `ChatManager.RequestInfos`。
  - `Timer_Elapsed()` 消费 `RequestInfos` 并调用 `CheckRequest()`。
  - `CheckRequest()` 才会做重复请求、BeatSaver 搜索、多结果处理、`SongSearchFilter()` 过滤、最终 `RequestTracker++` 和真正入队。
- 已确认本仓库没有“已实现的命令冷却计时器”：
  - `CmdFlags.Timeout` / `TimeoutSub` 仅存在于枚举和注释中，未在 `ParseState`、`CommandManager`、`RequestBot` 的执行路径中被消费。
- 已确认搜索路径的真实拦截项主要是歌曲过滤，而不是请求限额：
  - `GetSongListFromResults()` 通过 `SongSearchFilter(..., fast: true, filter)` 过滤结果。
  - `SongSearchFilter()` 涵盖队列重复、黑名单、mapper 白名单、会话重复、时长、NJS、remap、评分等。
  - 搜索按钮不会检查 `RequestQueueOpen`，也不会检查 `RequestTracker` 用户请求上限。

## Decision Log
| Decision | Why | Source |
|----------|-----|--------|
| 使用 `planning-workflow` 并落盘到 `.tasks/**` | 顶层 AGENTS 明确要求复杂研究任务优先走该流程 | `AGENTS.md`, `planning-workflow/SKILL.md` |
| 当前阶段保持在 Research | 用户要求是只读研究，尚未到 Plan/Implement | 用户请求 |
| 将“冷却/限额”拆成三类看待：命令权限、请求限额、歌曲过滤 | 代码中不存在真正启用的 timeout 逻辑，避免误把过滤当冷却 | `Enums.cs`, `ParseState.cs`, `RequestBot.cs` |

## Risks and Assumptions
- 如果用户记忆中的“冷却”实际指“队列关闭”或“每用户请求上限”，则本次结论是搜索结果点击入队已不经过这些检查。
- 如果用户记忆中的“误伤”指歌曲过滤本身，例如重复/黑名单/NJS/评分，那么这些过滤在搜索按钮构建结果时依然存在，但那不是正式点歌限额路径。

## Resources
- `SongRequestManagerV2/Bots/RequestBot.cs`
- `SongRequestManagerV2/Models/CommandManager.cs`
- `SongRequestManagerV2/Models/ParseState.cs`
- `SongRequestManagerV2/Views/RequestBotListView.cs`
- `SongRequestManagerV2/Statics/Enums.cs`
