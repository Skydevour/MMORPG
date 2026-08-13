# Development Roadmap

## Phase 0: 项目整理

目标：让项目适合 2D 横版动作开发。

任务：
- 确认 URP 2D Renderer 可用。
- 保留 `Assets/Scenes/MainScene.unity` 作为原型场景。
- 建立目录结构：
  - `Assets/Scripts/Core`
  - `Assets/Scripts/Player`
  - `Assets/Scripts/Combat`
  - `Assets/Scripts/Boss`
  - `Assets/Scripts/UI`
  - `Assets/Prefabs`
  - `Assets/Art/Placeholder`
  - `Assets/Audio`

验收：
- Unity 打开无编译错误。
- MainScene 可以进入 Play Mode。

## Phase 1: 玩家控制

目标：做出可以反复调手感的玩家原型。

任务：
- `PlayerInputReader`：读取移动、跳跃、冲刺、射击、锁定、暂停输入。
- `PlayerMotor2D`：移动、跳跃、重力、落地检测。
- `PlayerDash`：地面/空中冲刺。
- `PlayerHealth`：生命、受伤、无敌帧、死亡事件。
- `PlayerAim`：方向锁定和射击方向。

验收：
- 移动 5 分钟不丢输入、不穿地。
- 跳跃高度和移动速度能在 Inspector 调整。
- 冲刺有明确冷却或空中次数限制。

## Phase 2: 战斗基础

目标：建立可复用的射击、伤害和子弹框架。

任务：
- `IDamageable`：统一受伤接口。
- `Projectile`：速度、方向、伤害、生命周期。
- `ProjectilePool`：对象池。
- `Shooter`：玩家射击发射器。
- `Hitbox/Hurtbox`：命中与受击边界。

验收：
- 玩家能持续射击。
- 子弹命中测试目标后造成伤害。
- 子弹生命周期结束能回收。

## Phase 3: 弹反与能量

目标：加入 Cuphead-like 的关键风险回报机制。

任务：
- `ParryTarget`：标记可弹反对象。
- `PlayerParry`：空中触发弹反、反弹高度、成功反馈。
- `EnergyMeter`：能量条、EX 消耗、能量获取。
- `EXShot`：第一版 EX 技能。

验收：
- 普通子弹不可弹反，可弹反目标可弹反。
- 弹反成功增加能量。
- EX 技能消耗能量并造成更高伤害。

## Phase 4: 第一个 Boss

目标：完成一个原创三阶段 Boss。

任务：
- `BossHealth`：Boss 血量和阶段阈值。
- `BossStateMachine`：Intro、Phase1、Phase2、Phase3、Defeated。
- `BossAttackPattern`：攻击模式基类。
- 攻击 1：直线弹。
- 攻击 2：扇形弹。
- 攻击 3：地面冲击波。
- 攻击 4：可弹反召唤物。

验收：
- Boss 能按血量切换 3 个阶段。
- 每个攻击有清晰前摇、判定、后摇。
- 玩家能通过学习规律稳定通关。

## Phase 5: UI 与流程

目标：形成完整一局体验。

任务：
- 玩家生命 UI。
- Boss 阶段或血量 UI。
- 能量条 UI。
- 暂停菜单。
- 死亡重开。
- 胜利结算。
- 评分系统。

验收：
- 从 Play 到死亡或胜利都有闭环。
- 重开无需重启 Unity Play Mode。
- 评分显示通关时间、剩余生命、弹反次数、评级。

## Phase 6: 内容扩展

目标：在核心闭环稳定后扩大内容。

任务：
- 第二种武器。
- 护符系统。
- 第二个 Boss。
- 关卡选择界面。
- 跑枪关卡原型。
- 原创手绘资产替换占位图。

验收：
- 新内容不破坏 Phase 1-5 的核心闭环。
- 每个新增 Boss 都有独立主题和阶段设计文档。


## 当前阶段：Boss 单阶段可通关原型

### 已完成的核心闭环
- 玩家和 Boss 均有生命值、受击事件和死亡事件。
- 玩家拥有受伤无敌帧；冲刺和大招保持无敌。
- Boss 血条、玩家生命条和三格能量 HUD 已接入固定参考分辨率 Canvas。
- Boss 受击闪白、命中粒子、死亡爆炸和屏幕震动已接入。
- 胜利、失败、暂停和重新开始流程已接入 GameManager。
- 当前 Boss 为单阶段攻击：右侧水平三连射，上中下弹道轮流生成。

### 下一阶段实现顺序
1. Boss 三阶段状态机：入场、阶段一、阶段二、阶段三、死亡。
2. 阶段切换条件和演出：血量阈值、短暂无敌、画面震动、阶段提示和攻击间隔变化。
3. 攻击模式扩展：扇形弹道、地面冲击波、预警区域，并确保每种攻击都有前摇和后摇。
4. 紫色子弹弹反：空中窗口、成功判定、能量增加、弹反音效和视觉反馈。
5. 玩家死亡/复活的正式动画与音效，Boss 入场和死亡完整演出。
6. 结算评分：通关时间、剩余生命、弹反次数、受伤次数和评分等级。
7. PlayMode 与画面级回归：所有阶段、暂停、重试、对象池回收和固定画布比例。

### 当前验收口径
- 编译必须保持 0 警告、0 错误。
- 所有运行日志使用中文。
- 所有静态配置继续放入 Assets/Resources/Config/GameConfig.json。
- 新增角色、Boss、小怪和投射物逻辑按 Framework、Game/Player、Game/Bosses、Game/Combat、Game/UI、Game/VFX 分类。
- PlayMode 需要在 Unity Editor 空闲且脚本刷新完成后执行。