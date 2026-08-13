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

## 2026-07-06 玩法修复与 Boss 接入

### 今日待开发
- [x] 移除第一关可见 Tilemap 色块，只保留静态背景图和隐藏地面碰撞。
- [x] 渲染土豆 Boss，并让 Boss 默认处于 `idle` 状态。
- [x] 抽出玩家和 Boss 共用的角色状态机基类。
- [x] 修正主角动画播放过快的问题，先降低当前序列帧播放速度。
- [x] 提高主角跳跃高度并加入二段跳。
- [x] 修正主角枪口发射点和碰撞体位置。

### 今日已开发
- `LevelMapLoader` 不再创建 `Grid`、`ForegroundTilemap`、`DecorationTilemap`，第一关现在只使用 `GardenBackground` 作为静态背景。
- 保留 `StageCollision` 作为隐藏地面碰撞，避免玩家掉出场景；该碰撞不负责可见地块表现。
- 新增 `Game/Characters` 通用角色模块：
  - `CharacterStateDriverBase`
  - `CharacterAnimationState`
- `PlayerStateDriver` 改为继承通用角色状态机基类，玩家仍独立解析 `idle/run/jump/dash/shoot`。
- 新增 `Game/Bosses/Potato` 土豆 Boss 模块：
  - `PotatoBossController`
  - `PotatoBossStateDriver`
  - `PotatoBossSpawner`
- `GameManager` 游戏开始时会初始化玩家和土豆 Boss。
- 土豆 Boss 当前注册 `idle` 和 `dead` 两个状态，默认播放 `idle`，`dead` 暂时复用 `hurt` 帧作为占位。
- 主角 `Player` 根节点改为脚底根点，`Visual` 归零放置，`CapsuleCollider2D` 从脚底向上包围主角。
- 主角枪口偏移调整为基于脚底根点的身体中部位置，避免子弹从头顶发出。
- 主角第一段跳跃速度提高，新增一次空中二段跳；二段跳当前使用 `Visual` 旋转实现翻跟头表现。
- 主角动画播放帧率下调：`idle 7fps`、`run 10fps`、`jump 8fps`、`dash 12fps`、`shoot 10fps`。

### 修改文件
- `Assets/Scripts/Game/Level/LevelMapLoader.cs`
- `Assets/Scripts/Game/Core/GameManager.cs`
- `Assets/Scripts/Game/Player/PlayerController2D.cs`
- `Assets/Scripts/Game/Player/PlayerSpawner.cs`
- `Assets/Scripts/Game/Player/PlayerSpriteAnimator.cs`
- `Assets/Scripts/Game/Player/PlayerStateDriver.cs`
- `Assets/Scripts/Game/Characters/CharacterAnimationState.cs`
- `Assets/Scripts/Game/Characters/CharacterStateDriverBase.cs`
- `Assets/Scripts/Game/Bosses/Potato/PotatoBossController.cs`
- `Assets/Scripts/Game/Bosses/Potato/PotatoBossStateDriver.cs`
- `Assets/Scripts/Game/Bosses/Potato/PotatoBossSpawner.cs`
- `Assembly-CSharp.csproj`
- `Assembly-CSharp.Player.csproj`
- `Plan/code_module_structure.md`

### 今日阻塞
- 尚未进入 Unity Editor Play Mode 实测主角碰撞框、枪口发射点、二段跳手感和 Boss 尺寸。
- 二段跳翻跟头目前是运行时旋转占位，后续如果需要更接近正式动画，应单独生成二段跳序列帧。

### 下次建议
- 在 Play Mode 中优先检查：背景是否还有色块、Boss 是否出现在右侧并 idle 播放、主角碰撞框是否贴合、子弹是否从手部附近发出、二段跳翻跟头是否自然。
- 如果 Boss 尺寸或站位不合适，下一步只调整 `PotatoBossSpawner` 的缩放和 `LevelMapLoader.BossSpawnPosition`。

## 2026-07-06 闪避与动作补帧修复

### 今日待开发
- [x] 按反馈设置主角碰撞体偏移为 `x=-0.45, y=-0.04`。
- [x] 放大土豆 Boss，使其更接近参考图中的压迫感。
- [x] 调整二段跳翻滚为围绕人物中心旋转。
- [x] 给主角各动作插入补间帧，提高序列帧播放顺滑度。
- [x] 增加 `L` 键闪避，闪避起点和终点生成烟雾爆开效果。

### 今日已开发
- `PlayerSpawner` 改为 `Player -> VisualPivot -> Sprite` 层级：
  - `VisualPivot` 作为人物中心旋转点。
  - `Sprite` 继续承载 `SpriteRenderer` 和 `PlayerSpriteAnimator`。
- 主角 `CapsuleCollider2D.offset` 已设置为 `(-0.45, -0.04)`。
- 主角二段跳翻滚改为旋转 `VisualPivot`，不再围绕脚底根点旋转。
- 土豆 Boss 缩放设置为 `2.25` 倍。
- 新增 `Assets/Scripts/Game/VFX/DodgeSmokeEffect.cs`，用于闪避烟雾播放和自动销毁。
- 新增闪避烟雾资源：
  - `Assets/Res/VFX/DodgeSmoke/Frames/dodge_smoke_01.png` 至 `dodge_smoke_08.png`
  - `Assets/Res/VFX/DodgeSmoke/dodge_smoke_atlas_preview.png`
- 主角动作帧已插入补间帧：
  - `idle`：19 帧
  - `run`：19 帧
  - `jump`：17 帧
  - `dash`：15 帧
  - `shoot`：13 帧
- `PlayerController2D` 新增 `L` 键闪避输入，闪避开始和结束都会调用 `DodgeSmokeEffect.Spawn`。

### 修改文件
- `Assets/Scripts/Game/Player/PlayerSpawner.cs`
- `Assets/Scripts/Game/Player/PlayerController2D.cs`
- `Assets/Scripts/Game/Bosses/Potato/PotatoBossSpawner.cs`
- `Assets/Scripts/Game/VFX/DodgeSmokeEffect.cs`
- `Assets/Res/Hero/Frames/**`
- `Assets/Res/VFX/DodgeSmoke/**`
- `Assembly-CSharp.csproj`
- `Assembly-CSharp.Player.csproj`
- `Plan/code_module_structure.md`

### 今日阻塞
- 主角碰撞体偏移已按反馈设置，但实际贴合效果仍需在 Unity Scene/Play Mode 中确认。
- 补间帧当前使用离线图像混合生成，属于原型过渡方案；如果后续追求正式动画品质，仍建议重新绘制关键帧和中间帧。

### 下次建议
- 在 Play Mode 中重点检查：`L` 闪避距离、烟雾出现位置、翻滚中心、Boss 尺寸、主角碰撞体和地面接触。
- 如果补间帧出现轻微重影，可改为保留补帧文件但降低播放帧率，或重新生成正式动作帧。

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

## 2026-07-06 闪避烟雾、子弹枪口与土豆站位修正

### 今日待开发
- [x] 重绘闪避烟雾序列帧，使效果从细碎颗粒改为脚底爆开的圆润白灰烟团。
- [x] 修正玩家子弹生成点，使子弹更贴近手部/枪口位置，不再明显从头顶附近发出。
- [x] 将土豆 Boss 出生点调整到画面横向中心，尺寸保持上一轮翻倍后的压迫感。
- [x] 重新执行运行时代码、Player 构建和 Editor 脚本构建验证。

### 今日已开发
- `LevelMapLoader.BossSpawnPosition` 从右侧点位改为 `(0, StageFloorY, 0)`，让土豆 Boss 站在画面中央。
- `PotatoBossSpawner.BossScale` 当前为 `4.5`，这是从上一版 `2.25` 翻倍后的尺寸，本轮保留该放大结果。
- `PlayerController2D` 的枪口偏移调整为 `muzzleOffsetX=0.86`、`muzzleOffsetY=0.28`，让子弹生成点更靠近角色手部。
- 重新生成 `Assets/Res/VFX/DodgeSmoke/Frames` 下 9 张烟雾 PNG，并同步更新 `dodge_smoke_atlas_preview.png`。
- 新烟雾资源继续保留原目录和 `.meta` 文件，避免 Unity 资源 GUID 丢失。

### 修改文件
- `Assets/Scripts/Game/Level/LevelMapLoader.cs`
- `Assets/Scripts/Game/Player/PlayerController2D.cs`
- `Assets/Res/VFX/DodgeSmoke/Frames/dodge_smoke_01.png` 至 `dodge_smoke_09.png`
- `Assets/Res/VFX/DodgeSmoke/dodge_smoke_atlas_preview.png`
- `Plan/daily_dev_log.md`
- `Plan/test_log.md`

### 今日阻塞
- 尚未进入 Unity Editor Play Mode 进行画面级实测，Boss 实际屏幕占比、子弹首帧枪口贴合度、烟雾爆开位置仍需要运行确认。
- `dotnet build` 会生成 `obj/Debug` 临时构建产物，后续如需保持工作区干净，需要清理这些构建缓存。

### 下次建议
- 在 Play Mode 中优先检查：`L` 闪避烟雾是否位于脚底附近、子弹是否从手/枪口附近出现、土豆 Boss 是否位于画面中心且尺寸足够大。
- 如果子弹仍偏高，下一步只继续下调 `PlayerController2D.muzzleOffsetY`；如果 Boss 仍偏小，再将 `PotatoBossSpawner.BossScale` 从 `4.5` 增至更高值。

## 2026-07-30 土豆 Boss 序列帧统一与脚底锚点修正

### 今日待开发

- [x] 检查土豆 Boss 所有动作帧的尺寸、透明边界和位置漂移。
- [x] 将所有动作帧统一为同一画布尺寸。
- [x] 将角色脚底接触区域对齐到画布底部中心。
- [x] 将 Unity Sprite Pivot 统一为严格的底部中心 `(0.5,0)`。
- [x] 增加可重复执行的离线规范化工具并完成回归验证。

### 今日已开发

- 处理动作：`idle`、`angry`、`attack_spit`、`hurt`，每个动作 6 帧，共 24 张 PNG。
- 原资源虽然全部为 `268×255`，但角色内容横向逐帧漂移；问题根因不是画布尺寸，而是脚底中心没有固定。
- 使用图片底部 12 像素内的不透明像素重心计算脚底中心，所有帧离线对齐到统一锚点。
- 为完整保留 `attack_spit_03` 的右侧动作延伸，公共画布调整为最小安全尺寸 `313×253`。
- 所有帧脚底锚点像素统一为 `(156,252)`，最大中心误差 `0.490` 像素。
- 24 个 `.meta` 和 `PrototypeTexturePostprocessor` 均设置为 `spritePivot=(0.5,0)`。
- 新增 `Tools/Art/NormalizePotatoBossFrames.ps1`，后续增加动作帧后可再次离线规范化。
- 保留原文件名、动作目录和 `.meta` GUID，不在运行时执行 trim 或位置补偿。

### 修改文件

- `Assets/Res/Bosses/Potato/Frames/**/*.png`
- `Assets/Res/Bosses/Potato/Frames/**/*.png.meta`
- `Assets/Scripts/Editor/Import/PrototypeTexturePostprocessor.cs`
- `Tools/Art/NormalizePotatoBossFrames.ps1`
- `task_plan.md`
- `findings.md`
- `progress.md`
- `Plan/daily_dev_log.md`
- `Plan/test_log.md`

### 今日阻塞

- Codex 本地图片查看工具受 Windows ACL 辅助器错误影响，未能直接显示处理前后图集；已使用透明边界、脚底重心、画布尺寸和 Pivot 四项数值完成确定性验证。
- 尚未进入 Unity Play Mode 目测土豆 Boss 的 idle 与跨动作切换效果。

### 下次建议

- 打开 Unity 后等待 24 张帧重新导入，依次播放 `idle -> angry -> attack_spit -> hurt`，确认脚底不滑动且角色不会左右跳变。
- 视觉确认通过后，再继续实现土豆 Boss 的攻击状态、伤害判定和死亡流程。

## 2026-07-31 Boss 攻击帧修复与冲刺粒子替换

### 今日待开发

- [x] 排查 `attack_spit_01` 非底部中心的资源根因。
- [x] 清理所有攻击帧中误裁切的下方残片并重新对齐 Boss 序列帧。
- [x] 将 Hero 冲刺烟雾从图片序列帧替换为运行时粒子特效。
- [x] 删除不再使用的 DodgeSmoke 图片资源并完成编译回归。

### 今日已开发

- `attack_spit` 共 6 张帧图存在下方误裁切碎片，累计为 13 个独立连通区域；这些碎片会错误参与脚底中心计算。
- 新增 `Tools/Art/RemovePotatoBossFrameArtifacts.ps1`，以最大主角色连通区域为基准，移除相距超过 24 像素的下方误裁切碎片。
- 重新运行 Boss 规范化后，24 张帧图统一为 `305×192`，脚底锚点统一为 `(152,191)`，最大误差 `0.466` 像素。
- `DodgeSmokeEffect` 不再加载 `FrameAnimator` 和图片帧，改为运行时构造 18 个软边 `ParticleSystem` 颗粒的 Burst 效果。
- Shift 和 L 两种冲刺都会在起点、终点各生成一次粒子 Burst。
- 已删除 `Assets/Res/VFX/DodgeSmoke` 的旧烟雾序列帧、预览图及元数据。

### 修改文件

- `Assets/Res/Bosses/Potato/Frames/**/*.png`
- `Assets/Scripts/Game/Player/PlayerController2D.cs`
- `Assets/Scripts/Game/VFX/DodgeSmokeEffect.cs`
- `Tools/Art/NormalizePotatoBossFrames.ps1`
- `Tools/Art/RemovePotatoBossFrameArtifacts.ps1`
- `Assets/Res/VFX/**`（已删除）
- `task_plan.md`
- `findings.md`
- `progress.md`
- `Plan/daily_dev_log.md`
- `Plan/test_log.md`

### 今日阻塞

- Codex 本地图片预览受 Windows ACL 辅助器错误影响，无法直接回显处理后的 PNG；已通过连通区域、尺寸、脚底中心和 Pivot 数值验证替代。
- 尚未在 Unity Play Mode 目测 Boss 攻击序列与 Shift/L 冲刺粒子。

### 下次建议

- 在 Play Mode 中连续切换土豆 Boss 的 `idle -> attack_spit -> idle`，确认脚底没有上下或左右跳动。
- 测试 Shift 和 L 冲刺，重点观察粒子是否在起点和终点各爆发一次、是否保持在地面附近。
## 2026-08-02 土豆 Boss 战斗流程开发与验证

### 今日待开发

- [x] 将 Boss 普通土块子弹和紫色可采集子弹接入随机攻击流程。
- [x] 接入玩家三格能量、紫色子弹攻击采集和 K 键大招。
- [x] 将战斗参数集中到 Assets/Resources/Config/GameConfig.json。
- [x] 完成 MainScene 真实 PlayMode 流程验证。

### 今日已开发

- PotatoBossController 按全局配置随机生成普通或紫色投射物，并按玩家方向瞄准。
- PlayerProjectile 命中紫色子弹时调用采集逻辑，PlayerEnergyController 负责能量上限、增加和消耗。
- PlayerController2D 使用键盘 J/Z 攻击，K 释放大招；大招消耗全部能量并对 Boss 造成配置伤害。
- PlayerEnergyMeter 按配置的最大能量动态创建格子，不再把数量写死为 3。
- 粒子特效创建前先停止并清空默认播放状态；拾取特效持续时间和大招持续时间均从 JSON 读取。
- PlayMode 测试程序集从 EditorOnly 边界中拆出，确保 Unity 能正确发现 PlayMode 测试。

### 今日已测试

- PlayMode：1 个测试通过，0 个失败。
- 测试确认普通子弹、紫色子弹均生成；紫色子弹命中后能量增加；满三格能量后大招清零，并使 Boss 生命从 100 降至 70。
- 运行时、编辑器、PlayMode 测试程序集均构建通过，0 个警告，0 个错误。

### 下一步

- 在 Unity Editor Play Mode 中进行画面级检查：子弹颜色和尺寸、紫色子弹命中表现、能量 HUD、大招粒子位置。
- 增加 Boss 生命条、受击反馈和死亡演出。
- 将测试入口的批处理进程退出处理进一步自动化。
## 2026-08-03 Boss 右侧短间隔水平三连射

### 今日待开发

- [x] 将 Boss 出生点调整到主角右侧并保持同一水平出生线。
- [x] 将 Boss 攻击改为短间隔三连射。
- [x] 将三发子弹按上、中、下位置轮流生成。
- [x] 将子弹改为水平高速飞行并与 attack_spit 动画周期同步。
- [ ] 在 Unity Editor PlayMode 中完成最终画面回归。

### 今日已开发

- BossConfig 新增右侧出生标记、连射间隔、连射数量、攻击动画帧率和三条弹道高度。
- PotatoBossSpawner 根据实际 Sprite 世界宽度将 Boss 安全收拢到画面右边界内。
- PotatoBossController 在一次 attack_spit 状态内生成上、中、下三颗子弹，默认间隔 0.18 秒。
- BossProjectile 新增弹道枚举、发射统计和最后一次速度观测，继续复用对象池。
- 普通子弹速度调整为 8.0，紫色子弹速度调整为 7.2。
- 新增 PlayMode 断言：Boss 位于玩家右侧、三条弹道均生成、垂直速度为 0、水平速度向左且大于 7。

### 当前验证状态

- 运行时、PlayMode 测试、Editor 程序集串行编译通过，均为 0 个警告、0 个错误。
- Unity Editor PlayMode 尚未执行：项目当前被交互式 Unity Editor 进程占用，等待保存并关闭后继续。

## 2026-08-03 Boss 关卡生命、受击反馈与胜负闭环

### 今日完成
- [x] 新增统一生命契约 IHealthSource，玩家与土豆 Boss 都提供最大生命、当前生命、生命变化事件和死亡事件。
- [x] 玩家接入受击无敌帧，受伤期间不会被连续子弹重复扣血；冲刺和大招继续保持无敌。
- [x] 玩家死亡时停止移动、关闭受击碰撞并触发失败事件。
- [x] 土豆 Boss 接入受击闪白、命中粒子、屏幕震动和死亡爆炸；死亡后关闭碰撞并触发胜利事件。
- [x] 新增通用 HitFlashEffect、ScreenShakeEffect 和 CombatImpactEffect，后续小怪和大招命中可以复用。
- [x] 新增固定参考分辨率的 Boss 战 HUD：Boss 血条、玩家生命条、阶段 1 标识、胜利面板、失败面板、暂停面板。
- [x] GameManager 统一管理战斗开始、胜利锁定、失败锁定、暂停和重新加载当前场景。
- [x] 将玩家受伤无敌时长和闪白时长写入 Assets/Resources/Config/GameConfig.json。
- [x] 新增 PlayMode 测试：血条同步、Boss 死亡胜利、玩家死亡失败。

### 今日静态验证
- MMORPG.Runtime.csproj 编译通过：0 个警告，0 个错误。
- MMORPG.Tests.PlayMode.csproj 编译通过：0 个警告，0 个错误。
- 已检查玩家生命条锚点为左上角，Boss 血条锚点为顶部居中，能量格下移后不再和生命条重叠。

### 今日阻塞
- Unity Editor 进程正在占用当前工程，批处理 Unity 无法打开同一项目，因此新增 PlayMode 测试尚未实际执行，也没有生成新的 XML 结果。
- 未强制关闭现有 Unity Editor，避免破坏用户未保存状态。

### 下一次待开发
- [ ] 在 Unity Editor PlayMode 执行新增的三项战斗闭环测试，并检查血条、结果面板和粒子效果的画面位置。
- [ ] 把 Boss 从单阶段扩展为三阶段：血量阈值、攻击预警、阶段转换演出。
- [ ] 实现紫色子弹弹反窗口，弹反成功才增加能量，并增加弹反反馈。
- [ ] 补充 Boss 入场、受击、死亡音效和胜利音乐。
- [ ] 增加通关时间、剩余生命、弹反次数和评分结算。
## 2026-08-03 玩法优先：移动与扣血修正

### 本轮完成
- [x] 玩家死亡或战斗结束后立即停止读取输入、停止移动和停止射击。
- [x] Boss 死亡后残留子弹不会继续改变玩家生命。
- [x] 玩家受伤无敌帧继续保留，连续碰撞不会在同一段时间内重复扣血。
- [x] 增加跳跃土狼时间 0.1 秒和跳跃输入缓存 0.12 秒，改善边缘起跳和提前按跳。
- [x] 保持键盘操作不变：A/D 或方向键移动，Space 跳跃，Shift/L 冲刺，J/Z 射击，K 大招。
- [x] 增强扣血回归测试，覆盖无敌帧内不重复扣血和无敌帧结束后再次扣血。
- [x] 本轮不新增音效、评分和额外辅助系统。

### 当前玩法验收重点
- 玩家能稳定移动、跳跃、二段跳和冲刺。
- 玩家子弹能命中 Boss 并同步减少 Boss 生命。
- Boss 子弹能命中玩家并同步减少玩家生命。
- 玩家冲刺和大招期间不会扣血。
- 玩家或 Boss 生命归零后战斗不会继续改变状态。
## 2026-08-04 操作手感与扣血可靠性优化

### 本轮完成
- [x] 玩家水平移动由瞬时改速改为可配置加速/减速，减少启停生硬。
- [x] 增加空中控制系数，保留空中修正方向的能力。
- [x] 增加按键时长控制跳跃高度，松开 Space 可以提前收跳。
- [x] 玩家 Rigidbody2D 开启插值和连续碰撞检测。
- [x] 玩家子弹和 Boss 子弹改为 FixedUpdate + Rigidbody2D.MovePosition，避免高速 Transform 移动造成漏判。
- [x] 玩家子弹只对存活 Boss 执行扣血。
- [x] 新增移动参数全部放入 GameConfig.json：groundAcceleration、groundDeceleration、airControl、jumpCutMultiplier。
- [x] 本轮没有新增音效、评分或外围辅助系统。

### 下一步验证
- [ ] Unity Editor 中检查起步、停步、空中转向和短跳/长跳手感。
- [ ] 持续射击确认每发子弹都能稳定扣除 Boss 生命。
- [ ] 站在上、中、下三条弹道中测试玩家扣血和无敌帧。
- [ ] 测试冲刺穿过子弹时不会扣血。