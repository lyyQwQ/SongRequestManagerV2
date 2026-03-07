# Research

## Status Snapshot
- Stage: Research
- Status Bar: [>R -P -A -I -V]
- Task: 搜索冷却误伤研究
- Date: 2026-03-07

## Current Understanding
- 该问题至少涉及三条潜在入口：
  - UI 搜索按钮触发的本地命令解析路径。
  - UI 搜索结果列表点击后的直接入队路径。
  - 聊天/本地正式点歌命令路径。
- 代码中已经存在一个明显的“搜索结果直接入队”入口：`RequestBotListView` 直接调用 `RequestBot.AddSearchResultToQueue(CurrentSong)`。
- `RequestBot` 同时包含命令入口和实际入队处理方法，说明是否“误伤”应在 `RequestBot` 内部最终判定，而不是只看 `CommandManager` 表面命令定义。
- 搜索按钮不是正式点歌别名，而是本地广播员命令 `!addsongs/top <keyword>`；它会生成 `RequestStatus.SongSearch` 条目。
- 正式点歌才会经过 `ProcessSongRequest() -> RequestInfos -> CheckRequest()` 的异步请求队列。

## Confirmed Constraints
- 本次任务只做只读研究，不改代码。
- 需要覆盖 `ParseState` / `CmdFlags` / `LookupSongs` / `AddSearchResultToQueue` / `QueueSong` / `ProcessSongRequest` 相关实现。
- 结论必须明确回答 bug 是否仍存在，不能停留在“可能”。

## Synthesized Conclusions
- 搜索按钮路径：
  - `RequestBot.Search()` -> `Parse(..., \"!addsongs/top ...\", CmdFlags.Local)` -> `ParseState.ParseCommand()` -> `CommandManager` 的 `!addsongs` -> `RequestBot.Addsongs()` -> `QueueSong()`。
  - 这条路径不会进入 `ProcessSongRequest()`，因此不会命中其“队列是否开启”和“每用户请求上限”检查。
- 搜索结果入队路径：
  - `RequestBotListView.AddToQueueClick()` -> `RequestBot.AddSearchResultToQueue(CurrentSong)`。
  - 这条路径直接把 `SongSearch` 条目改为 `Queued`，也不会命中 `ProcessSongRequest()` 或 `CheckRequest()` 的请求限额入口。
- 正式点歌路径：
  - `!bsr/!request/!add/!srm/!sr/点歌` -> `RequestBot.ProcessSongRequest()` -> `ChatManager.RequestInfos.Enqueue()` -> `RequestBot.Timer_Elapsed()` -> `CheckRequest()` -> 真正入队。
- 真正会拦正式点歌的检查：
  - `ProcessSongRequest()`：`RequestQueueOpen`、`RequestTracker` 用户请求上限。
  - `CheckRequest()`：已在队列、BeatSaver 查找失败、多结果需用户缩小范围、`SongSearchFilter()` 的黑名单/重复/时长/NJS/评分/mapper 等歌曲过滤。
- 搜索相关路径仍会受歌曲过滤影响：
  - `Addsongs()` / `LookupSongs()` 都会通过 `GetSongListFromResults()` 调 `SongSearchFilter()`。
  - 但这与正式请求的用户限额/队列关闭不是同一类拦截。
- 结论：如果用户记忆的 bug 指“搜索结果点击或搜索按钮被正式点歌队列冷却/限额误伤”，那么在当前 refactor 代码里该 bug 已不存在。

## Decisions That Need User Input
- 暂无。

## What Would Make Research Sufficient
- [x] 搜索按钮路径已从 UI/命令定义追到最终处理函数
- [x] 搜索结果入队路径已从 UI 追到最终处理函数
- [x] 正式点歌命令路径已从命令定义追到最终处理函数
- [x] 每条路径命中的冷却/限额拦截点已列清
- [x] 能明确判断“搜索被队列冷却误伤”在 refactor 中是否仍存在

## Research Gate
- Confirmation status: Pending
- Allowed to enter `Plan`: No
- User decision: Waiting for confirmation
- Next step after this file is complete: 暂停，等待用户确认是否需要继续进入 Plan/实现

## Change Log
| Date | Change | Why |
|------|--------|-----|
| 2026-03-07 | Created research draft | Initial synthesis |
| 2026-03-07 | Completed code-path synthesis | 已能判断 bug 是否仍存在 |
