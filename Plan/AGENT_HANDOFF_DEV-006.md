# DEV-006 开发交接与验证报告

日期：2026-09-13。依据：PM-006、GAME_DESIGNER_SPEC-004。状态：四阶段代码已接入，技术验证通过，体验交付待产品试玩复核；未提交或推送。

## 交付内容

### V0.1.1 操作与坐标

- 移动按下和松开立即更新速度。真实按键测试发现默认地面摩擦把 5.5 速度降到约 5.25，已增加零摩擦 `Assets/Res/Hero/Player.physicsMaterial2D` 并接入角色。
- 跳跃缓冲仅在成功跳跃时消费；落地检测过滤向上运动和不合适的法线；二段跳有独立触发序号，重新播放跳跃动作。
- 闪避锁定进入方向，期间无敌、隐藏人物并在起止位置播放粒子烟雾。
- 射击前先更新朝向，跑射/空中射击不再强制覆盖为站立射击；枪口使用脚底参考坐标。
- 主角根节点为脚底参考，视觉节点围绕身体中心旋转，物理碰撞体不参与旋转。导入器显式设置 Custom Pivot，移除运行时逐帧居中补偿。
- 原始 PNG 保留，播放目录排除 39 张旧 `_tween` 叠加补间；按动作总时长注册动画。当前目录播放 75 张角色帧和 1 张背景。
- Boss scale 调整为 2.2、X=4.7、埋土深度 0.12，解除过宽图框被强行挤向中心的问题。

### V0.1.2 伤害与特效

- 新增 SpriteFlash Shader 与共享材质，通过 MaterialPropertyBlock 实现真实闪白；3 秒受击无敌期间透明闪烁；死亡停止闪白，避免颜色争用。
- 震屏叠加使用独立偏移、结束归零，窗口变化时清理偏移；背景增加 4% 超扫余量，固定 16:9 舞台。
- 粉色弹两种入口均检查活动状态；同一次采集后不能重复奖励。通用对象池增加重复回收防护和失效闲置对象处理。
- 爆烟、命中、采集、死亡粒子共用 PooledBattleEffect 与预制体；不逐次创建材质。只读审核发现并修正了 World 粒子误在世界原点生成的问题，已补专门测试。
- 大招改为有朝向的 14×1.1 水平能量束，持续 0.85 秒；独立 DirectionalDamageVolume 负责实际范围命中、每个目标单次 30 点伤害，VFX 负责显示与生命周期。
- 粒子主要尺寸、数量、时长，主角动作周期，Boss 预备/收招和大招尺寸写入现有 JSON。

### V0.1.3 土豆攻击节奏

- 进入攻击后有 0.3 秒预备，随后按阶段间隔发射，最后保留 0.25 秒收招。
- Boss 动画支持外部时间线采样，使用相同攻击时钟显示预备、吐弹和收招；保留已有三阶段与危险物逻辑。
- 上中下发射偏移配合新的 Boss 尺寸重新校准。增加独立 AI 开关用于测试，修正旧测试“禁止受伤后又期待扣血”的前置矛盾。

### V0.1.4 挑战闭环

- GameManager 统一暂停入口，按钮通过事件请求恢复；暂停禁止战斗输入和大招消耗。
- Boss 血条以 RectTransform 实际宽度裁切，保留延迟血量表现；阶段标签移入右上安全区。
- 左下生命卡保留并增加轻微俯仰，能量布局随生命数量排列。
- 新增离线制作的 6 帧死亡倾倒和 1 张幽灵图，统一 307×167 画布；播放一次死亡后切换幽灵上升，约 2 秒出现结算。
- 结算增加遮罩、标题倾斜、淡入和缩放过渡，保留重新挑战；连续重试 10 次测试通过。
- 构建前自动预处理资源和目录；必需帧或背景缺失时构建失败，不再只输出警告。图片加载不触发运行时导入设置。

## 文件职责

| 分类 | 主要文件/目录 |
| --- | --- |
| 框架 | Framework/Animation/FrameAnimator.cs、Framework/Pooling/ComponentObjectPool.cs |
| 角色 | Game/Player/PlayerController2D.cs、PlayerSpawner.cs、PlayerStateDriver.cs |
| 战斗 | Game/Combat/HitFlashEffect.cs、ScreenShakeEffect.cs、DirectionalDamageVolume.cs |
| Boss | Game/Bosses/Potato/PotatoBossController.cs、PotatoBossStateDriver.cs |
| 特效 | Game/VFX/PooledBattleEffect.cs、SuperAttackEffect.cs 及烟雾/命中/采集入口 |
| UI/场景 | Game/UI、Game/Core/GameManager.cs、Game/Level/LevelMapLoader.cs |
| 资源 | Assets/Res/Hero/Frames/dead、ghost；Assets/Res/Hero/Player.physicsMaterial2D；Assets/Res/VFX |
| 导入 | Editor/Import/BattleAssetPreparation.cs、PrototypeTexturePostprocessor.cs、PrototypeSpriteCatalogBuilder.cs |
| 验证 | Tests/PlayMode/BattleFoundationTests.cs、Editor/Validation/BattleBuildValidator.cs、Game/Core/BattlePlayerValidation.cs |

## 验证证据

Unity 版本：6000.0.23f1c1。使用本机 `D:/Unity/Editor/6000.0.23f1c1/Editor/Unity.exe`。未停止其他项目的 Unity。

1. 资源准备：`-batchmode -executeMethod MMORPG.EditorTools.Import.BattleAssetPreparation.Prepare -quit`，退出码 0。
2. PlayMode：`-batchmode -runTests -testPlatform PlayMode`，最终 14/14 通过，约 42.11 秒；见 `playmode_results_DEV-006.xml` 和 `unity_DEV-006.log`。
3. Windows 构建：`-batchmode -executeMethod MMORPG.EditorTools.Validation.BattleBuildValidator.Build -quit`，退出码 0，Runtime/Editor/Player 编译无 C# 错误或警告；见 `unity_build_DEV-006.log`。
4. 独立播放器：1280×720、1920×1080、1024×768 三种尺寸均运行结束，退出码 0；未发现运行时异常或缺资源日志。
5. `git diff --check` 通过；Git 仅提示原有换行规范 LF/CRLF 转换，不是代码错误。

新增测试覆盖真实键盘移动、两段跳、闪避方向、跑射、暂停按钮/Escape、暂停大招保护、粉色弹重复采集、真实投射物碰撞、大招方向与单次伤害、实际导入 pivot、3 秒无敌、100 次震屏归位、100 次粒子复用、死亡幽灵与 10 次重试。测试环境临时启用无焦点输入并在结束恢复，没有修改玩家输入偏好。

### 发布版渲染采样

设备：NVIDIA GeForce RTX 4070 Ti SUPER。为了在后台验证时确实绘制画面，专用 `-battleValidation` 入口逐帧离屏渲染，临时提高 Boss 生命并自动射击；正常启动不进入此入口。该数据包含验证场景的模拟与离屏绘制，不代表前台输入到显示的延迟、垂直同步表现或低配置机器指标。

| 尺寸 | 采样时间 | 平均帧时间 | P95 |
| --- | --- | --- | --- |
| 1280×720 | 60 秒 | 0.80 毫秒 | 1.05 毫秒 |
| 1920×1080 | 3 秒尺寸冒烟验证 | 0.85 毫秒 | 1.17 毫秒 |
| 1024×768 | 3 秒尺寸冒烟验证 | 0.85 毫秒 | 1.19 毫秒 |

截图：`Validation/DEV-006/player_1280x720.png`、`player_1920x1080.png`、`player_1024x768.png`。均已打开检查，非空白，4:3 窗口使用上下黑边，背景与 HUD 保持在舞台内。PlayMode 的 `ghost.png`、`defeat.png` 记录死亡与结算表现。

## 试玩入口与边界

- 直接运行 `Builds/DEV-006/BossBattle.exe`，或在 Unity 打开 MainScene 后 Play。
- A/D 或左右键移动，空格跳跃/二段跳，J/Z 射击，L/Shift 闪避，K 满能量大招，Escape 暂停。
- 当前资源是原型资产修整：死亡帧由已有姿态离线加工，幽灵为新制简化图；没有宣称完成商业级逐帧原画，也没有用透明叠加伪造补间。
- 自动测试和截图不能代替长时间人工难度试玩。全部原帧逐帧美术评审、20 次连续翻转录像、完整三阶段无伤可解性与前台输入延迟尚无完整证据，不能把 PM-006 所有体验验收项勾成完成。
- 本轮功能交付已完成，建议产品下一步集中试玩调参和原画质量，不继续叠加新机制。当前工作区包含用户和此前开发的改动，全部保留，没有提交或推送。
