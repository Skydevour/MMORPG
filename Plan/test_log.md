# 测试日志

## 2026-07-05

### 测试范围

- 计划文档创建检查。
- 生成资源文件检查。
- 透明背景处理检查。
- 主角帧与 Tile 拆分数量检查。
- C# 运行时代码编译检查。
- C# Editor 代码编译检查。
- MainScene 根节点和脚本引用检查。

### 测试结果

- `Plan` 文件夹和计划文档已创建。
- `Assets/Art/Generated` 原型资源已创建。
- `Assets/Res` 原型资源已创建。
- `Assets/Scenes/MainScene.unity` 已包含 `GameRoot` 和 `GameManager` 脚本引用。
- 运行时代码已创建，但尚未在 Unity Editor 内进入 Play Mode 实测。

### 资源规格修正测试

#### 测试范围

- 主角动作帧重新拆分结果。
- 背景、Tile、主角帧尺寸一致性。
- 主角帧内本体根点稳定性。
- C# 运行时代码编译。
- C# Editor 代码编译。

#### 测试步骤

1. 使用 Python/Pillow 读取 `Assets/Res` 下 PNG 实际尺寸与帧数量。
2. 查看 `Assets/Res/Hero/cup_hero_action_atlas.png` 预览，确认没有半截角色帧。
3. 反查生成后主角帧的脚底基线和下半身中心点。
4. 顺序运行 `dotnet build Assembly-CSharp.csproj -v:minimal`。
5. 顺序运行 `dotnet build Assembly-CSharp-Editor.csproj -v:minimal`。

#### 实际结果

- 主角帧数：
  - `idle`：10 帧
  - `run`：10 帧
  - `jump`：9 帧
  - `dash`：8 帧
  - `shoot`：7 帧
- 主角每帧尺寸均为 `307x167`。
- 主角资源生成阶段固定根点为图内 `x=99, y=166`。
- Unity 底部 pivot 对应 `x=99, y=1`。
- 脚底基线校验通过，生成后主角脚底统一在图内 `y=166`。
- 已从原始动作图集按连通组件重新拆分，红框示例中的相邻帧人物残片已被过滤。
- 已按所有帧非透明像素的最大包围范围进行离线 trim，加载时可直接使用处理后的 PNG。
- 背景尺寸为 `2048x1152`。
- Tile 尺寸均为 `128x128`。
- `dotnet build Assembly-CSharp.csproj -v:minimal` 通过，0 错误。
- `dotnet build Assembly-CSharp-Editor.csproj -v:minimal` 通过，0 错误。
- 编译中出现 Unity 包和 Unity API 过时警告，不影响当前功能。

#### 回归结论

- [x] 通过
- [ ] 未通过

### 未完成验证

- 尚未进入 Unity Editor Play Mode 实测脚底贴地。
- 尚未实测 Tilemap 碰撞。
- 尚未实测相机 16:9 视口限制。
- 尚未实测动画切换手感。

## 2026-07-06

### 测试范围

- 框架级序列帧播放器编译检查。
- 通用对象池编译检查。
- 玩家移动、落地判定、射击点和冲刺无敌标识编译检查。
- 第一关隐藏地面碰撞与非碰撞 Tilemap 装饰编译检查。
- 土豆 Boss 序列帧资源尺寸、动作帧数量和透明背景检查。
- C# 运行时代码编译检查。
- C# Editor 导入代码编译检查。

### 测试步骤

1. 使用 Python/Pillow 读取生成的土豆 Boss 图集，按 4 行 6 列拆分，去除绿幕并输出统一尺寸 PNG。
2. 查看 `Assets/Res/Bosses/Potato/potato_boss_atlas.png` 预览图，确认背景透明、没有文字水印、动作帧完整。
3. 运行 `dotnet build Assembly-CSharp.csproj -v:minimal`。
4. 运行 `dotnet build Assembly-CSharp-Editor.csproj -v:minimal`。

### 实际结果

- 土豆 Boss 每个动作 6 帧，共 24 帧。
- 土豆 Boss 每帧统一为 `268x255`。
- 土豆 Boss 根点为底部中心，Unity pivot 规则为 `x=0.5, y=1/255`。
- `dotnet build Assembly-CSharp.csproj -v:minimal` 通过，0 警告，0 错误。
- `dotnet build Assembly-CSharp-Editor.csproj -v:minimal` 通过，0 警告，0 错误。

### 问题与处理

- 第一次运行时代码构建失败，原因是新建框架脚本尚未加入当前 `Assembly-CSharp.csproj` 编译列表；已同步 csproj 后重新构建通过。
- 当前环境未直接进入 Unity Editor Play Mode，移动手感和碰撞体表现仍需在编辑器内实测。

### 回归结论

- [x] 通过
- [ ] 未通过

## 2026-07-06 代码模块整理验证

### 测试范围

- 脚本目录模块化后的运行时代码编译。
- Player 项目文件编译。
- Editor 脚本自身编译。
- csproj 中旧脚本路径残留检查。

### 测试步骤

1. 移动 `.cs` 与对应 `.meta` 文件到新的模块目录。
2. 同步命名空间和 `using` 引用。
3. 更新 `Assembly-CSharp.csproj`、`Assembly-CSharp.Player.csproj`、`Assembly-CSharp-Editor.csproj` 的编译路径。
4. 运行 `dotnet build Assembly-CSharp.csproj -v:minimal`。
5. 运行 `dotnet build Assembly-CSharp.Player.csproj -v:minimal`。
6. 运行 `dotnet build Assembly-CSharp-Editor.csproj -v:minimal --no-dependencies`。

### 实际结果

- `dotnet build Assembly-CSharp.csproj -v:minimal` 通过，0 项目脚本错误。
- `dotnet build Assembly-CSharp.Player.csproj -v:minimal` 通过，0 项目脚本错误。
- `dotnet build Assembly-CSharp-Editor.csproj -v:minimal --no-dependencies` 通过，0 错误。
- 完整 Editor 构建仍被 Unity 包 `UnityEditor.UI.csproj` 阻断，错误位置为 `Library/PackageCache/com.unity.ugui/Editor/UGUI/UI/MenuOptions.cs`，不是本次项目脚本整理造成的错误。

### 回归结论

- [x] 通过
- [ ] 未通过

## 2026-07-06 玩法修复与 Boss 接入验证

### 测试范围

- 静态背景替代可见 Tilemap。
- 玩家和 Boss 共用角色状态机基类。
- 土豆 Boss idle 渲染接入。
- 主角脚底根点、碰撞体、枪口点、跳跃高度和二段跳逻辑。
- 运行时代码、Player 构建和 Editor 自身脚本编译。

### 测试步骤

1. 检查 `LevelMapLoader` 是否不再创建可见 Tilemap。
2. 检查 `GameManager` 是否同时初始化玩家和土豆 Boss。
3. 检查玩家和 Boss 是否都通过 `CharacterStateDriverBase` 驱动状态。
4. 运行 `dotnet build Assembly-CSharp.csproj -v:minimal`。
5. 运行 `dotnet build Assembly-CSharp.Player.csproj -v:minimal`。
6. 运行 `dotnet build Assembly-CSharp-Editor.csproj -v:minimal --no-dependencies`。

### 实际结果

- 运行时代码构建通过，0 项目脚本错误。
- Player 构建通过，0 项目脚本错误。
- Editor 自身脚本构建通过，0 错误。
- Player 构建仍有 Unity/TextMeshPro 包内未使用字段警告，不影响当前项目脚本。

### 未完成验证

- 尚未在 Unity Editor Play Mode 中实测画面和手感。
- 尚未确认 Boss 在实际画面中的视觉尺寸是否合适。
- 尚未确认主角碰撞框在 Scene 视图中是否完全贴合预期。

### 回归结论

- [x] 编译通过
- [ ] Play Mode 未验证

## 2026-07-06 闪避与动作补帧验证

### 测试范围

- 主角碰撞体偏移配置。
- 土豆 Boss 缩放配置。
- 主角补间帧数量。
- 闪避烟雾资源生成。
- `L` 键闪避逻辑编译。
- 运行时代码、Player 构建和 Editor 自身脚本编译。

### 测试步骤

1. 使用 Python/Pillow 为主角每个动作相邻帧插入一张补间帧。
2. 使用 Python/Pillow 生成 8 帧闪避烟雾 PNG。
3. 查看 `Assets/Res/VFX/DodgeSmoke/dodge_smoke_atlas_preview.png`。
4. 运行 `dotnet build Assembly-CSharp.csproj -v:minimal`。
5. 运行 `dotnet build Assembly-CSharp.Player.csproj -v:minimal`。
6. 运行 `dotnet build Assembly-CSharp-Editor.csproj -v:minimal --no-dependencies`。

### 实际结果

- 主角动作帧数量：
  - `idle`：19 帧
  - `run`：19 帧
  - `jump`：17 帧
  - `dash`：15 帧
  - `shoot`：13 帧
- 闪避烟雾 8 帧已生成。
- `dotnet build Assembly-CSharp.csproj -v:minimal` 通过，0 项目脚本错误。
- `dotnet build Assembly-CSharp.Player.csproj -v:minimal` 通过，0 项目脚本错误。
- `dotnet build Assembly-CSharp-Editor.csproj -v:minimal --no-dependencies` 通过，0 错误。

### 未完成验证

- 尚未进入 Unity Editor Play Mode 实测闪避烟雾位置和翻滚中心。
- 尚未在 Scene 视图中确认 `CapsuleCollider2D.offset=(-0.45,-0.04)` 是否完全符合角色轮廓。

### 回归结论

- [x] 编译通过
- [ ] Play Mode 未验证

## 测试记录模板

### 测试范围

- 

### 测试步骤

1. 
2. 
3. 

### 预期结果

- 

### 实际结果

- 

### 问题与处理

- 

### 回归结论

- [ ] 通过
- [ ] 未通过

## 2026-07-06 闪避烟雾、子弹枪口与土豆站位回归

### 测试范围

- 闪避烟雾序列帧资源更新。
- 玩家子弹枪口偏移配置。
- 土豆 Boss 出生点与缩放配置。
- 运行时代码、Player 构建和 Editor 脚本构建。

### 测试步骤

1. 查看 `Assets/Res/VFX/DodgeSmoke/dodge_smoke_atlas_preview.png`，确认烟雾帧已经改为更大的白灰云团。
2. 检查 `LevelMapLoader.BossSpawnPosition` 是否为画面中心点。
3. 检查 `PlayerController2D` 中的枪口偏移是否已下压到手部附近。
4. 运行 `dotnet build Assembly-CSharp.csproj -v:minimal`。
5. 运行 `dotnet build Assembly-CSharp.Player.csproj -v:minimal`。
6. 运行 `dotnet build Assembly-CSharp-Editor.csproj -v:minimal --no-dependencies`。

### 预期结果

- 烟雾帧播放时呈现脚底爆开的云团效果。
- 子弹初始生成点靠近角色手/枪口。
- 土豆 Boss 站在画面横向中心，尺寸保持放大后的表现。
- 三项构建均通过。

### 实际结果

- 烟雾预览图已更新为 9 帧白灰云团。
- Boss 出生点已调整为 `(0, StageFloorY, 0)`。
- 枪口偏移已调整为 `muzzleOffsetX=0.86`、`muzzleOffsetY=0.28`。
- `dotnet build Assembly-CSharp.csproj -v:minimal` 通过，0 警告，0 错误。
- `dotnet build Assembly-CSharp.Player.csproj -v:minimal` 通过，0 警告，0 错误。
- `dotnet build Assembly-CSharp-Editor.csproj -v:minimal --no-dependencies` 通过，0 警告，0 错误。

### 问题与处理

- 尚未在 Unity Play Mode 中确认最终画面表现，本次仅完成资源预览和编译验证。
- 构建过程产生 `obj/Debug` 临时产物，未自动删除，避免在未确认前执行破坏性清理。

### 回归结论

- [x] 编译通过
- [ ] Play Mode 未验证

## 2026-07-30 土豆 Boss 序列帧与锚点回归

### 测试范围

- 24 张 Boss PNG 的统一画布尺寸。
- 所有帧的垂直脚底线和水平脚底中心。
- 24 个 `.meta` 的 Sprite Pivot。
- Unity 自动导入器的土豆 Boss Pivot 配置。
- Editor 脚本编译。

### 测试步骤

1. 运行 `Tools/Art/NormalizePotatoBossFrames.ps1 -ValidateOnly`。
2. 使用 `System.Drawing` 独立统计 24 张 PNG 尺寸。
3. 扫描所有 `.png.meta` 的 `spritePivot`。
4. 检查 `PrototypeTexturePostprocessor.PotatoBossFramePivot`。
5. 检查是否残留 `*.normalize.tmp.png` 临时文件。
6. 完整构建 `Assembly-CSharp-Editor.csproj`。

### 实际结果

- 24/24 张 PNG 均为 `313×253`。
- 24/24 张 PNG 的脚底均贴齐 `y=252`。
- 脚底中心目标为 `x=156`，最大误差为 `0.490` 像素。
- 24/24 个 `.meta` 均为 `spritePivot: {x: 0.5, y: 0}`。
- 自动导入器使用 `PotatoBossFramePivot=(0.5f,0f)`。
- 临时规范化文件数量为 0。
- 完整 Editor 构建通过，0 警告、0 错误。

### 问题与处理

- `--no-dependencies` 构建因当前 `Temp/Bin/Debug` 缺少 Unity 依赖 DLL 而失败；该失败不是项目代码错误。改为完整构建后依赖正确生成并通过。
- 本地图片查看受 ACL 工具错误阻塞，Play Mode 视觉回归仍需在 Unity Editor 内完成。

### 回归结论

- [x] 资源尺寸通过
- [x] 脚底中心对齐通过
- [x] Pivot 配置通过
- [x] Editor 编译通过
- [ ] Unity Play Mode 视觉验证待完成

## 2026-07-31 Boss 攻击帧与冲刺粒子回归

### 测试范围

- `attack_spit` 误裁切碎片检测与清理。
- 24 张 Boss 帧的公共画布、脚底中心和 Sprite Pivot。
- 冲刺粒子特效的代码引用与旧资源清理。
- 运行时代码、Player 和 Editor 脚本构建。

### 测试步骤

1. 运行 `RemovePotatoBossFrameArtifacts.ps1 -ValidateOnly`。
2. 运行 `NormalizePotatoBossFrames.ps1 -ValidateOnly`。
3. 统计所有 Boss PNG 尺寸和 `.meta` Pivot。
4. 搜索旧 DodgeSmoke 资源路径引用并检查目录是否删除。
5. 构建 `Assembly-CSharp.csproj`、`Assembly-CSharp.Player.csproj`、`Assembly-CSharp-Editor.csproj --no-dependencies`。

### 实际结果

- 误裁切碎片验证通过：24 张帧图中残片数量为 0。
- 24/24 张 Boss PNG 均为 `305×192`。
- 24/24 个 Pivot 均为 `spritePivot: {x: 0.5, y: 0}`。
- 脚底中心锚点为 `(152,191)`，最大误差为 `0.466` 像素。
- 旧 VFX 目录不存在，旧序列帧路径引用数量为 0。
- 运行时代码、Player、Editor 构建均通过，0 警告、0 错误。

### 问题与处理

- 清理器初版因 Bitmap 文件锁和固定临时文件冲突失败；已改为释放源文件句柄后写回，并使用 GUID 临时文件名。
- 图片预览工具受 ACL 限制，尚需 Unity Play Mode 视觉确认。

### 回归结论

- [x] Boss 误裁切碎片清理通过
- [x] Boss 帧尺寸与锚点通过
- [x] 旧烟雾序列帧清理通过
- [x] 三项构建通过
- [ ] Unity Play Mode 视觉验证待完成
## 2026-08-02 土豆 Boss 战斗流程回归

### 测试范围

- MainScene 启动后 GameManager、玩家、土豆 Boss 和能量 HUD 初始化。
- Boss 随机生成普通土块子弹和紫色可采集子弹。
- 玩家攻击命中紫色子弹后获得能量。
- 三格能量充满后按 K 释放大招，能量归零并对 Boss 造成伤害。
- 粒子特效创建过程不产生未处理 Unity 日志。

### 测试环境

- Unity 6000.0.23f1c1
- PlayMode 测试：PrototypePlayModeFlowTests.MainSceneCanRunBossEnergyAndSuperFlow
- 结果文件：Plan/playmode_results_2026-08-02.xml

### 实际结果

- 1 个测试通过，0 个失败，0 个跳过。
- Boss 生成普通土块子弹和紫色可采集子弹。
- 玩家命中紫色子弹后获得 1 格能量；补满 3 格后释放大招。
- 大招消耗全部 3 格能量，Boss 受到 30 点伤害，生命从 100 降至 70。
- 未再出现粒子系统 duration 配置警告。
- MMORPG.Runtime、MMORPG.Editor、MMORPG.Tests.PlayMode 均构建通过，0 个警告，0 个错误。

### 回归结论

- [x] Boss 随机攻击流程通过
- [x] 紫色子弹采集能量通过
- [x] 三格能量和 K 键大招流程通过
- [x] 全局 JSON 配置加载通过
- [x] PlayMode 自动化测试通过
## 2026-08-03 Boss 右侧三连射回归记录

### 已完成的静态验证

- GameConfig.json 可解析，Boss 连射字段和投射物速度字段已写入。
- MMORPG.Runtime.csproj 编译通过，0 个警告，0 个错误。
- MMORPG.Tests.PlayMode.csproj 编译通过，0 个警告，0 个错误。
- MMORPG.Editor.csproj 编译通过，0 个警告，0 个错误。
- PlayMode 测试已增加 Boss 右侧、三条弹道和水平速度断言。

### 待执行验证

- [ ] 启动 MainScene PlayMode。
- [ ] 观察 Boss 是否在画面右侧且脚底与玩家处于同一水平线。
- [ ] 观察 attack_spit 动画期间上、中、下三发子弹的间隔和速度。
- [ ] 确认紫色子弹采集和既有大招流程不受影响。

### 阻塞原因

- 当前已有交互式 Unity Editor 进程打开项目，未强制关闭以保护用户未保存状态。

## 2026-08-03 Boss 关卡闭环测试

### 测试范围
- IHealthSource 生命事件和 IDamageable 兼容性。
- 玩家受击无敌帧、死亡状态和死亡事件。
- 土豆 Boss 受击扣血、死亡状态和死亡事件。
- Boss 血条、玩家生命条同步。
- 胜利面板、失败面板、暂停与重试入口。
- 运行时新增粒子、屏幕震动和闪白组件编译。

### 已执行
- dotnet build MMORPG.Runtime.csproj --no-restore：通过，0 个警告，0 个错误。
- dotnet build MMORPG.Tests.PlayMode.csproj --no-restore：通过，0 个警告，0 个错误。
- GameConfig.json 与 GameConfig.cs 新增玩家无敌帧配置字段检查：通过。
- HUD 锚点静态检查：通过，玩家生命条左上角，Boss 血条顶部居中，能量格下移。

### 尚未执行
- Unity PlayMode：当前项目已被 Unity Editor 进程占用，批处理启动被 Unity 拒绝，未生成 Plan/playmode_results_2026-08-03.xml。
- 画面级验证：尚未确认实际运行时血条填充、粒子层级、屏幕震动和结果面板的最终视觉效果。

### 回归结论
- 代码层面通过，当前没有编译错误。
- 运行时 PlayMode 仍需要在现有 Unity Editor 中手动执行；执行前先等待脚本刷新完成。
## 2026-08-03 玩法优先回归

### 已执行
- MMORPG.Runtime.csproj：通过，0 个警告，0 个错误。
- MMORPG.Tests.PlayMode.csproj：通过，0 个警告，0 个错误。
- 新增无敌帧扣血断言已加入测试源码。

### 待在 Unity Editor 执行
- 键盘移动、跳跃、二段跳和冲刺手感。
- 玩家子弹实际命中土豆 Boss 并扣血。
- Boss 三条弹道实际命中玩家并扣血。
- 冲刺、大招期间受击无效。
- Boss/玩家生命归零后的状态锁定。

### 当前阻塞
- 工程仍被现有 Unity Editor 进程占用，批处理 PlayMode 无法启动。
## 2026-08-04 操作与碰撞回归

### 已执行
- MMORPG.Runtime.csproj：通过，0 个警告，0 个错误。
- MMORPG.Tests.PlayMode.csproj：通过，0 个警告，0 个错误。
- MMORPG.Editor.csproj：通过，0 个警告，0 个错误。
- 静态检查确认玩家移动、跳跃截断、Rigidbody 插值和投射物 FixedUpdate 已接入。

### 尚未执行
- Unity Editor 实际键盘操作验证。
- Unity Editor 实际高速投射物碰撞验证。
- Unity Editor 画面级手感验证。

### 说明
- 当前工程仍被已经打开的 Unity Editor 进程占用，批处理 PlayMode 无法启动。
- 本轮只优化玩法核心，不引入音频和新的外围系统。