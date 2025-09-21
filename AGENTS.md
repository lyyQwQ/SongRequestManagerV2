# SongRequestManagerV2（点歌姬）Agent 指南

- 范围：适用于 `SongRequestManagerV2_139/` 子树；与根 `AGENTS.md` 冲突时，本文件优先。
- 语言：永远使用中文回复。
- 术语：本模块 = “点歌姬”。

## 构建
- 构建：`dotnet build SongRequestManagerV2_139/SongRequestManagerV2.sln -c Debug`

说明：当前以 `.csproj.user` 中的 `BeatSaberDir/ReferencePath` 为准，已指向 `C:\\Users\\m5689\\BSManager\\BSInstances\\1.39.1`。如需切换实例，请直接修改 `SongRequestManagerV2_139/SongRequestManagerV2/SongRequestManagerV2.csproj.user` 中的路径。

## 代码风格
- 遵循仓库 `.editorconfig`（4 空格缩进，UTF-8）。
- 启用 nullable；按需使用依赖注入（例如 Zenject 或等价方案）。

## 依赖
- 依赖共享库 `ChatCore-v2`；请确保在本地 Beat Saber 环境中正确解析引用。

## 测试
- 如需添加测试，建议创建独立测试项目并使用 `dotnet test` 运行。

## Archive 约定
- `SongRequestManagerV2_139/archive/` 为参考代码，仅供查阅。
- 不要修改、构建或将其导入活动代码，除非明确要求。

更多通用规范、提交与安全约定，请参见仓库根目录的 `AGENTS.md`。

---

## 调研与结论（UI/网络/线程）

以下为与 archive 版本对比后的关键差异与修复建议（2025-09）：

- 下载通道与线程模型
  - 现状：核心请求改为 UnityWebRequest，并通过 `CoroutineRunner.Instance.StartCoroutine(...)` 驱动。
  - 风险：`System.Timers.Timer` 回调在线程池上执行，当前在非主线程路径里创建/使用 Unity 对象（包含 `CoroutineRunner` 与 `UnityWebRequest`），在 Unity/BS 环境下易导致未定义行为（崩溃/卡死/无响应）。
  - 建议：保留 UnityWebRequest 的高吞吐，但统一在主线程初始化与调度（预先在主线程创建 Runner，所有协程通过 `Dispatcher.RunCoroutine`/`MainThreadInvoker` 入队到主线程执行）。
  - 状态：已完成（2025-09-21）。实现：主线程协程与统一调度（`Networks/DownloadService.cs`）、`WebClient` 全量委托服务（`WebClient.cs`）、Zenject 绑定（`Installer/SRMAppInstaller.cs`）。

- 事件解绑 Bug
  - `Utils/ChatManager.Dispose` 对 `OnTextMessageReceived` 使用了 `+=` 而非 `-=`，Dispose 时会“重复订阅”。应改为 `-=` 以避免消息重复与资源泄漏。


- 依赖/manifest 对齐
  - `manifest.json` 标注的 `gameVersion` 与依赖区间较旧（如 BSML 1.6.10）。构建应以实际实例 DLL 为准（通过 `BSPath`），发布前对齐目标实例版本（BSML/SiraUtil/SongCore/ChatCore）。

- Bouyomi 管线生命周期
  - `Plugin.OnDisabled()` 直接 `BouyomiPipeline.instance.Stop()`，可能在未初始化时触发单例创建。建议先判断 `PersistentSingleton.IsSingletonAvailable` 再 Stop。

- UI 悬浮提示（优先修复）
  - 症状：列表项悬浮不再显示“点歌人 + 时间”。
  - 研判：BSML 版本/绑定差异导致 `hover-hint` 挂在 `background tags='hovered'` 上时不生效；或 `Hint` 赋值时机与解析时机存在竞态。
  - 处理：在行模板的可见容器（`horizontal`）也添加 `hover-hint='~hover-hint'`，保证旧/新 BSML 皆可解析；同时确认 `SongRequest.Setup()` 在 `#post-parse` 阶段为 `Hint` 赋值（代码已满足）。
  - 状态：已完成（2025-09-21）。实现：`Views/RequestBotListView.bsml` 为行容器增加 `hover-hint`；`Bots/SongRequest.cs` 在 `#post-parse` 阶段赋值，时间格式 `yyyy/MM/dd HH:mm`；字符串模板见 `Statics/StringFormat.cs`。

- 其他小项
  - `DownloadImage` 可能返回 null，`LoadImage(b)` 需要判空早退，避免 NRE。
  - `FeedbackText` 关闭后，用户可能误判为“机器人未响应”，建议提升 UI/日志提示。

以上内容仅在本模块树内生效；与根 AGENTS.md 冲突时，以本文件为准。

## 新需求（UI）— 搜索结果区分与“添加到队列”按钮

- 背景与目标
  - 当前右侧搜索后显示的“搜索结果”和“已入队请求”在列表里样式几乎一致，难以区分。
  - 期望：
    1) 列表项明确标记“搜索结果”。
    2) 在“跳过”按钮下增加“添加到队列”按钮，手动将选中的“搜索结果”加入队列。

- 方案设计（BSML + 现有模型）
  - 视觉标识（行模板内）：
    - 在自定义行 `<horizontal>` 增加一个小标签 `<text text='[搜索]' color='yellow' font-size='2' active='~is-search'>`。
    - 在 `SongRequest` 增加 `[UIValue("is-search")]`，返回 `Status == RequestStatus.SongSearch`，用于行内逐项判定。
- 新按钮（右侧按钮区）：
  - 位置：在“开始（Play）”按钮的上方（不是“跳过”下面）。
  - 在按钮列表中插入 `<button text='添加到队列' interactable='~add-to-queue-enable' on-click='add-to-queue-click'>`，位于 Play 前一行。
    - `add-to-queue-enable`：由 `RequestBotListView` 暴露 `[UIValue]`，当选中项存在且其 `Status == SongSearch` 且非历史页时为 `true`。
    - `add-to-queue-click`：`RequestBotListView` `[UIAction]` 调用 `RequestBot.AddSearchResultToQueue(SongRequest)`（新增方法）。
- 业务逻辑（新增 `RequestBot.AddSearchResultToQueue`）：
  - 将选中项 `Status` 从 `SongSearch` 置为 `Queued`（沿用 `SetRequestStatus`）。
  - 入队位置：若队列非空，则追加到队列末尾（`RequestManager.RequestSongs.Add(req)`）；不做 MoveToTop 处理。
  - 更新去重表（`duplicatelist`）、写队列文件（`_requestManager.WriteRequest()`），并通过 `DynamicText` 输出“已添加到队列”提示（沿用 `StringFormat.AddSongToQueueText`）。
  - 触发 `UpdateRequestUI()/RefreshSongQuere()`，保持与现有流程一致。

- DeepWiki 可行性确认（BSML 绑定范围与行为）
  - 自定义列表 `<custom-list>` 每行以“行数据对象”为 host 解析，因此行内 `active='~is-search'` 能逐行取 `SongRequest` 的 `[UIValue]`（已确认）。
  - 行外按钮以 `ViewController` 为 host 解析，`interactable='~add-to-queue-enable'` 与 `on-click='add-to-queue-click'` 可直接绑定到 `RequestBotListView` 的 `[UIValue]/[UIAction]`（已确认）。
  - 绑定作用域注意点：行模板无法直接访问 `ViewController` 的 `[UIValue]`，反之亦然；需分别在 `SongRequest` 与 `RequestBotListView` 暴露对应属性/动作（已确认）。

- 影响面与风险
  - UI 改动小、只读绑定与按钮回调，对性能影响可忽略。
  - 需要为“添加到队列”按钮增加本地化文本键，或直接内嵌中文文本（短期）。
  - 需确保队列关闭时是否允许“手动入队”（当前建议允许，属本地操作；如需受控，可复用 `RequestQueueOpen` 状态）。

- 实施优先级与粒度
  - P1：行模板标记 + 新按钮 + 简单入队逻辑。
  - P2：可选：为搜索结果增加不同底色/边框；为入队按钮加入二次确认（与 Skip 类似）。

（以上为方案记录，尚未落地实现）

## 下载服务设计（UnityWebRequest）

实现状态：已落地（2025-09-21）。核心组件：`Networks/IDownloadService.cs`、`Networks/DownloadService.cs`、`Installer/SRMAppInstaller.cs`，并已在 `WebClient.cs` 侧统一委托调用。

为保留 UnityWebRequest 的高吞吐并确保稳定性，下载逻辑将集中到由 Zenject 管理的 DownloadService（主线程 MonoBehaviour 单例）中，向外提供 Task 风格 API。

- 生成与调度
  - 在 AppInstaller 中注册：FromNewComponentOn(new GameObject("DownloadService")).AsSingle().NonLazy()
  - 仅在主线程创建/发送 UWR；协程与回调也在主线程执行。外部通过 await 使用，无需关心线程。

- API 形态（示例）
  - GetAsync(string url, CancellationToken, IProgress<double>?) → WebResponse
  - DownloadImage(string url, CancellationToken, IProgress<double>?) → byte[]
  - DownloadZip(string url/hash, CancellationToken, IProgress<double>?) → byte[]（内部可采用 DownloadHandlerFile 临时落盘，再读入字节）

- 超时与重试
  - 超时：5 分钟（uwr.timeout = 300）
  - 重试：指数退避（例如 1s → 2s → 4s），总次数按资源类型控制（图片 3 次，ZIP 4 次）
  - 进度：每帧通过 IProgress<double> 报告 uwr.downloadProgress

- 并发与去重
  - 并发：ZIP 2、图片 4–6（后续可配置）
  - 去重：同一 URL 维持单个进行中的 Task，复用其结果

- CDN 与中转策略
  - 常规：按配置在主 CDN 与备用地区 CDN（AS/NA/EU/R2）间回退
  - 中转服务器已启用（如 EstrellaTest）时：不检测速度门限、不切换 CDN，仅使用中转通道（允许长时间下载）
  - Header：保留 User-Agent，按服务器类型添加 X-API-Key

- 失败回退
  - 可选在多次失败后回退 HttpClient（小心跨平台证书/代理差异），优先保障成功率

- 资源与内存
  - ZIP 优先 DownloadHandlerFile，避免一次性内存峰值；图片使用 DownloadHandlerBuffer

- 日志
  - 失败/切换/重试打印摘要；Release 降噪，Debug 详尽（包含状态码、CDN 名称、重试次数）
