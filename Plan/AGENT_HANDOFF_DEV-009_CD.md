# DEV-009 C/D UI 开发交接

## 基本信息

- 任务编号：DEV-009 C/D；开发专家；2026-09-14。
- 状态：待主会话集成验收。C/D 修改锁现已释放，主会话接回独占修改。
- 依据：GAME_DESIGNER_SPEC-006、DEV-009_IMPLEMENTATION_AND_ACCEPTANCE 及用户明确授权。
- 本轮未修改 A/B 动作、镜头、Boss 几何及测试源；未提交推送。

## 交付内容

- `Assets/Scripts/Game/UI/BossBattleHud.cs` 保留游戏层外部接口，作为 HUD、暂停、结果视图组装适配层。
- `UI/Common/`：GardenUiCatalog、GardenUiScreen 序列化引用、GardenUiRoot 单 EventSystem、GardenUiMotion 可取消淡入淡出、BattleMenuFeedback 形状加颜色焦点及短按压。
- `UI/Hud/`：HealthBarView、PlayerHealthCardView、PlayerEnergyMeter、EnergyTrailView。旧文件连同 .meta 移动，保留 GUID。
- `UI/Menu/BattleTitleView.cs`：标题、设置模态、音量和动态选择、调用者焦点恢复。旧文件连同 .meta 移动。
- `UI/Results/GardenResultView.cs`：独立胜败 Prefab、结果冻结数据、三节点/HP 权重进度、默认折叠详情、按钮防重入与延迟解锁。
- `Config/GardenUiConfig.cs`、PresentationConfig.ui、GameConfig.json：集中主题、主要布局 Rect、字号和动效时长；保留加载时 A/B 配置值。
- `Editor/Import/GardenUiPreparation.cs`：独立 UI 导入、Prefab 构建、序列化绑定校验、三尺寸静态 Prefab 预览；GardenAssetPreparation 增加调用。
- `Assets/Resources/Config/GardenUiCatalog.asset`：七个屏幕 Prefab、字体和 UI Sprite 引用。
- `Assets/Res/UI/GardenTheatre/`：HUD/Title/Pause/Defeat/Victory Prefab；Common/HUD/Pause/Defeat/Victory 原创程序绘制票签、卡面、印章和植物装饰；`generate_art.py` 可复现，`SOURCES.md` 登记来源和缺口。

## 行为变化

- 顶部姓名/血条/三形态标记按新规格布局，移除常驻百分比。
- 生命卡只对本次损失做 0.18 秒翻灰；能量达到满槽时单次 0.24 秒脉冲，缩放不超过 1.08；能量并入 HUD 子树，飞入仍绑定真实槽位且不加第二次能量。
- 血量目标立即改变；新 Boss 0.30 秒填入；扣血延迟条等待 0.12 秒，再于 0.25 秒追齐。
- 标题使用现有三 Boss Sprite 群像，确认立即锁重复提交，0.25 秒淡出后 Begin；暂停/设置统一节目单与中文操作。
- 设置恢复打开前选中控件，显式内部导航，保留背景点击不丢焦点；模态期间底层禁交互、隐藏，Escape 单层处理。
- 失败使用独立破损票券；胜利用独立题字区域、植物装饰和三枚印章，不复用失败底板。结果出现时 HUD 0.15 秒淡出；胜利摘要仅用时/受伤/粉弹收集。
- 减弱/关闭震动同时关闭卡片翻转、脉冲缩放、按钮压缩和飞入旋转，保留淡入和数据变化。不新增纸屑。

## 验证证据

- 执行：`Unity.exe -batchmode -quit -projectPath E:/UnityProject/MMORPG/MMORPG -executeMethod MMORPG.EditorTools.Import.GardenUiPreparation.Prepare`。
- 首次 `Plan/unity_DEV-009_CD_prepare.log`：程序集编译成功、七 Prefab 绑定校验通过，退出码 0。
- 最后一次 `Plan/unity_DEV-009_CD_prepare_final.log`：文件迁移后启动阶段出现旧缓存路径 CS2001，随后 AssetDatabase 刷新重新编译成功（Tundra build success），执行资源准备/校验/静态预览，最终退出码 0。不要把最早缓存错误行与最终状态混淆，也不声称日志从头完全无错误。
- `Plan/Validation/DEV-009-CD/PrefabPreviews/`：六类视图 x 1280x720、1920x1080、1024x768，共 18 张 Editor 静态资源预览；已人工看过标题1280及设置1024。背景为预览纯色，不是实际花园运行截图。
- 未启动 PlayMode、未修改测试源、未跑全测试、未构建播放器。用户要求后续实际截图、全测试与打包由主会话执行。
- 启动前核对唯一已运行 Unity 为 PID107696，项目 RogueLike/CrazyPeople；未停止或操作它。两次本项目批处理均自行退出。

## 集成定位与时序

- 保留 `ResumeButton`。
- 保留 `BattleHudRoot/PausePanel/ReturnTitleButton`，PausePanel 下按钮均直接子节点。
- `GardenTitle` 上仍有 BattleTitleView；其直接子节点 `Settings` 内含 Slider，标题按钮保留“开始挑战”“设置”“退出”对象名。
- HUD 内容迁至 `BattleHudRoot/HudScreen`；生命卡位于该节点的 `PlayerHealthCards`；能量位于该节点的 `EnergyMeter`。过去直接查 BattleHudRoot 下血条/生命卡的路径需加 HudScreen。
- 旧 ResultPanel 由 `DefeatScreen`/`VictoryScreen` 替代；保留其内部 `RetryButton`、`TitleButton` 和 `ResultText`。
- 标题确认后等待 0.25 秒才进入 Intro；设置关闭等待 0.12 秒后隐藏/销毁并恢复焦点。旧测试若仅 yield 一两帧断言关闭，应按新时序更新（本轮未改测试）。
- 结果按钮失败 0.30 秒、胜利 0.44 秒解锁；暂停显示默认聚焦继续，设置返回原调用按钮。
- 默认中文结果按钮写在 Prefab；旧 presentation.retry/returnToTitle 兼容字段仍在，不驱动新结果视图。

## 缺口与下一步

- 没有已知阻断编译或资源绑定的问题；实际输入/暂停/快速重开及战斗数据回归尚未执行，不等同完成交互验收。
- 主会话接回锁后按 C01、D01、D02 及已有测试覆盖检查；特别检查快开设置时与菜单入场动效的衔接、鼠标背景后键盘操作、返回原焦点、HUD 淡出和能量飞入残留。
- UI 原创图为 Pillow 本地程序绘图，不是 AI 生图。字体仍用 LegacyRuntime.ttf 技术占位；缺明确授权分发的中文字体与正式标题/胜败题字，三 Boss 群像沿用既有开发资产。developmentArt 未解除。
- 标题 Sprite 群像保留源图透明留白，因此土豆在群像中可见尺寸偏小；这仅是 UI 群像占位，不改 Boss 世界几何，可后续在 UI 专用素材中离线裁透明边完善。
- 正式三尺寸游戏截图、键鼠交互及快速重试、最终用户视觉验收由主会话接续；本轮不再启动任何 Unity 检查。
