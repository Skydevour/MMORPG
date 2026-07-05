# 每日开发日志

## 2026-07-05

### 今日待开发

- [x] 确认项目方向：制作原创 2D Boss Rush 原型，参考《茶杯头》的操作节奏与关卡结构，但不直接复制商业素材。
- [x] 建立 `Plan` 文件夹，用于记录开发计划、每日进度、测试结果和待办事项。
- [x] 梳理第一版 MVP 范围：玩家移动、跳跃、冲刺、射击、动画状态切换、第一关测试地面与背景。

### 今日已开发

- 已创建项目计划文件：
  - `Plan/README.md`
  - `Plan/cuphead_like_design.md`
  - `Plan/roadmap.md`
  - `Plan/daily_dev_log.md`
  - `Plan/test_log.md`
  - `Plan/backlog.md`
  - `Plan/level01_resource_plan.md`
  - `Plan/input_policy.md`
- 已生成并接入第一关原型资源：
  - 主角动作帧。
  - 第一关菜园背景图。
  - 第一关 Tile 与 TileSet。
- 已在 `Assets/Scenes/MainScene.unity` 创建 `GameRoot` 根节点并挂载 `GameManager`。
- 已按运行时分层接入第一版玩法代码：
  - `Assets/Scripts/Framework`：通用状态机接口和状态机。
  - `Assets/Scripts/Game`：地图加载、主角生成、主角控制、动画播放、开枪弹丸。
  - `Assets/Scripts/Editor`：MainScene 构建菜单和 `Assets/Res` 纹理导入处理器。
- 游戏开始时会自动创建 `Level01_Garden`、Tilemap 地面、背景、出生点和玩家。
- 主角已支持基础键盘操作：移动、跳跃、冲刺、持续开枪和动作状态切换。
- 已明确当前阶段只支持键盘，手柄只做后续预留，不提前接入。

### 资源规格修正

- 已重新从 `Assets/Art/Generated/Hero/cup_hero_action_atlas_raw.png` 按原图中的完整角色组件拆分主角动作帧，不再强制每个动作固定 9 帧。
- 当前主角动作帧数：
  - `idle`：10 帧
  - `run`：10 帧
  - `jump`：9 帧
  - `dash`：8 帧
  - `shoot`：7 帧
- 所有主角帧先按人物脚底根点统一对齐，再按所有帧非透明像素的最大包围范围进行离线 trim，当前尺寸为 `307x167` PNG。
- 资源生成阶段已把主角根点固定到图内 `x=99, y=166`，对应 Unity 底部 pivot 为 `x=99, y=1`，让每张序列帧里的主角身体处在同一个固定区域，避免播放时产生位置偏移。
- 已改为从原始动作图集按连通组件重新拆分，避免相邻帧的人物残片被矩形裁剪带入。
- 开枪动作中过滤掉落在主角左侧的独立子弹，保留主角右侧或手部附近的枪口火花与子弹表现。
- trim 使用统一最大包围框，不做逐帧不同尺寸裁剪，避免每张 PNG 尺寸和角色相对位置变化。
- 加载时直接使用处理后的 PNG，不再进行额外 trim 或位置修正。
- 已清理原始图切分带来的小型边缘碎片。
- 背景统一为 `2048x1152`，对应 16:9 画布。
- Tile 统一为 `128x128`，并重建 `garden_tileset.png`。
- Unity 导入默认 PPU 调整为 `128`。
- 主角运行时结构调整为 `Player` 物理根节点加 `Visual` 显示子节点，让动画显示和碰撞体中心解耦。
- 运行时相机增加 16:9 视口限制，避免画布比例变化时穿帮。

### 修改文件

- `Assets/Res/Hero/Frames/**`
- `Assets/Res/Hero/cup_hero_action_atlas.png`
- `Assets/Res/Level01_Garden/Background/garden_background_wide.png`
- `Assets/Res/Level01_Garden/Tiles/**`
- `Assets/Res/Level01_Garden/garden_tileset.png`
- `Assets/Scripts/Game/GameManager.cs`
- `Assets/Scripts/Game/LevelMapLoader.cs`
- `Assets/Scripts/Game/PlayerController2D.cs`
- `Assets/Scripts/Game/PlayerSpawner.cs`
- `Assets/Scripts/Game/PrototypeAssetLoader.cs`
- `Assets/Scripts/Editor/MainSceneBuilder.cs`
- `Assets/Scripts/Editor/PrototypeTexturePostprocessor.cs`

### 今日阻塞

- 尚未进入 Unity Editor 的 Play Mode 做实机验证。
- 当前主角和场景资源仍属于原型质量，后续正式制作时需要重新绘制更稳定的生产级动作帧。
- 跑步烟尘、冲刺线和枪口特效目前仍混在动作帧中，后续更推荐拆成独立特效资源。

### 下次建议

- 打开 Unity，进入 `MainScene` 的 Play Mode。
- 实测脚底贴地、Tilemap 碰撞、跳跃高度、冲刺距离、开枪频率和动画状态切换。
- 检查不同窗口比例下相机 letterbox 是否能稳定遮住画布外区域。
- 如果跑步动画仍有轻微视觉漂移，优先把烟尘拆成独立 VFX，不再让烟尘参与主角本体序列帧。

## 2026-07-06

### 今日待开发
- [x] 修正第一关地形表现，避免可见 Tile 方块阵列破坏参考图中的完整舞台感。
- [x] 封装框架级序列帧播放器，解决待机动画反复重启导致的抽搐感。
- [x] 封装可复用对象池，先接入玩家子弹，后续小怪和 Boss 弹幕复用同一套逻辑。
- [x] 排查并修复主角移动异常，重点处理出生点脚底与地面碰撞体错位的问题。
- [x] 为冲刺阶段提供无敌状态标识，后续伤害系统统一读取。
- [x] 生成原创土豆 Boss 序列帧资源，并完成绿幕去底、拆帧、统一尺寸和脚底根点对齐。

### 今日已开发
- 新增 `Assets/Scripts/Framework/FrameAnimationClip.cs` 和 `Assets/Scripts/Framework/FrameAnimator.cs`，序列帧播放现在支持 clip 注册、不同帧率、循环播放、非重复重启和播放完成事件。
- 新增 `Assets/Scripts/Framework/IPoolable.cs` 和 `Assets/Scripts/Framework/ComponentObjectPool.cs`，作为后续子弹、小怪、特效和掉落物的通用池化基础。
- `PlayerSpriteAnimator` 改为包装框架级 `FrameAnimator`，同一动作不会每帧重新从第 1 帧播放。
- `PlayerController2D` 改为使用碰撞体向下 Cast 判定落地，枪口位置改为基于脚底根点偏移，并提供 `IsInvincible` 冲刺无敌标识。
- `PlayerProjectile` 改为对象池生成和回收，不再每颗子弹运行时反复 `new GameObject` 和 `Destroy`。
- `LevelMapLoader` 改成“背景负责完整画面、Tilemap 只做非碰撞装饰、隐藏 `StageCollision` 负责地面碰撞”的结构，避免第一张参考图那种可见方块地形。
- 生成土豆 Boss 原始图集并处理成运行时帧：
  - 原始图集：`Assets/Art/Generated/Bosses/Potato/potato_boss_sheet_raw.png`
  - 预览图集：`Assets/Res/Bosses/Potato/potato_boss_atlas.png`
  - 动作帧目录：`Assets/Res/Bosses/Potato/Frames`
  - 动作分类：`idle`、`angry`、`attack_spit`、`hurt`
  - 每个动作 6 帧，单帧统一 `268x255`，根点为底部中心。
- `PrototypeAssetLoader` 和 `PrototypeTexturePostprocessor` 已补充土豆 Boss 帧的底部 pivot 导入规则。

### 修改文件
- `Assembly-CSharp.csproj`
- `Assets/Scripts/Framework/FrameAnimationClip.cs`
- `Assets/Scripts/Framework/FrameAnimator.cs`
- `Assets/Scripts/Framework/IPoolable.cs`
- `Assets/Scripts/Framework/ComponentObjectPool.cs`
- `Assets/Scripts/Game/LevelMapLoader.cs`
- `Assets/Scripts/Game/PlayerController2D.cs`
- `Assets/Scripts/Game/PlayerProjectile.cs`
- `Assets/Scripts/Game/PlayerSpawner.cs`
- `Assets/Scripts/Game/PlayerSpriteAnimator.cs`
- `Assets/Scripts/Game/PrototypeAssetLoader.cs`
- `Assets/Scripts/Editor/PrototypeTexturePostprocessor.cs`
- `Assets/Art/Generated/Bosses/Potato/potato_boss_sheet_raw.png`
- `Assets/Res/Bosses/Potato/potato_boss_atlas.png`
- `Assets/Res/Bosses/Potato/Frames/**`

### 今日阻塞
- 尚未进入 Unity Editor Play Mode 实测地面触感、冲刺无敌窗口和子弹池回收情况。
- 土豆 Boss 目前只完成资源生成与导入规则，尚未生成 Boss GameObject、状态机和攻击逻辑。

### 下次建议
- 打开 `MainScene` 进入 Play Mode，优先检查角色是否能正常左右移动、跳跃、冲刺、射击，以及待机动画是否仍有抽搐。
- 如果移动手感可用，下一步接入 Boss Spawner 和 Boss 的基础待机动画，再实现土豆吐土块攻击。

## 2026-07-06 代码模块整理

### 今日待开发
- [x] 按模块整理代码目录，区分角色、关卡、子弹、Boss 预留、通用框架和编辑器工具。
- [x] 同步命名空间，使目录结构和代码引用语义一致。
- [x] 保留 `.meta` 文件一起移动，避免 Unity 脚本 GUID 丢失。
- [x] 更新 csproj 编译路径，并验证运行时代码和 Player 构建。

### 今日已开发
- 框架层已拆分为：
  - `Assets/Scripts/Framework/Animation`
  - `Assets/Scripts/Framework/StateMachine`
  - `Assets/Scripts/Framework/Pooling`
- 游戏层已拆分为：
  - `Assets/Scripts/Game/Core`
  - `Assets/Scripts/Game/Level`
  - `Assets/Scripts/Game/Player`
  - `Assets/Scripts/Game/Projectiles`
  - `Assets/Scripts/Game/Bosses`
- 编辑器层已拆分为：
  - `Assets/Scripts/Editor/Scene`
  - `Assets/Scripts/Editor/Import`
- 新增模块结构说明：`Plan/code_module_structure.md`。
- 命名空间已同步为 `MMORPG.Framework.Animation`、`MMORPG.Framework.StateMachine`、`MMORPG.Framework.Pooling`、`MMORPG.Game.Core`、`MMORPG.Game.Level`、`MMORPG.Game.Player`、`MMORPG.Game.Projectiles` 等。

### 修改文件
- `Assembly-CSharp.csproj`
- `Assembly-CSharp.Player.csproj`
- `Assembly-CSharp-Editor.csproj`
- `Assets/Scripts/Framework/**`
- `Assets/Scripts/Game/**`
- `Assets/Scripts/Editor/**`
- `Plan/code_module_structure.md`

### 今日阻塞
- `dotnet build Assembly-CSharp-Editor.csproj -v:minimal` 当前被 Unity 自带 `UnityEditor.UI.csproj` 中的 `DefaultControls.factory` 只读属性错误阻断，错误不来自项目脚本。
- 使用 `dotnet build Assembly-CSharp-Editor.csproj -v:minimal --no-dependencies` 验证项目自身 Editor 脚本通过。

### 下次建议
- 后续接入土豆 Boss 时，在 `Assets/Scripts/Game/Bosses/Potato` 下新增 Boss 控制器、动画驱动和攻击状态。
- 如果通用伤害、生命值、受击无敌需要跨角色和怪物复用，可以新增 `Game/Combat` 或 `Framework/Combat`，具体取决于是否依赖玩法对象。

## 日志模板

### 今日待开发

- [ ] 
- [ ] 
- [ ] 

### 今日已开发

- 

### 修改文件

- 

### 今日阻塞

- 

### 下次建议

- 
