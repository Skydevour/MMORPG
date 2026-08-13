# 土豆 Boss 战斗流程设计

## 目标

在现有第一关测试场景中完成一条可游玩的 Boss 战斗闭环：土豆随机发射普通子弹或紫色子弹；玩家开枪命中紫色子弹后获得能量；能量最多三格；满能量后按 `K` 释放大招并对 Boss 造成伤害。

## 架构

- `GameConfigService` 从 `Assets/Resources/Config/GameConfig.json` 读取全局静态配置。GameManager 在初始化玩家和 Boss 前加载配置。
- `PotatoBossController` 只负责攻击节奏、目标方向、生命和状态请求；`BossProjectile` 负责投射物移动、碰撞和对象池回收。
- `PlayerEnergyController` 管理三格能量；`PlayerEnergyMeter` 只订阅能量变化并显示三个独立格子；玩家大招输入仍留在键盘输入层。
- 普通玩家子弹命中 Boss 会造成伤害，命中紫色子弹会将其转化为能量并播放采集特效；大招使用运行时粒子表现，代码归属 `Game/VFX`。

## 行为约定

- 普通子弹为棕色土块，命中玩家会造成伤害。
- 紫色子弹为紫色可反击投射物，仍会造成伤害；玩家子弹命中后销毁并增加一格能量。
- 玩家冲刺期间保持无敌；大招演出期间也暂时保护玩家，避免释放瞬间被 Boss 子弹打断。
- Boss 攻击动作播放 `attack_spit`，发射时随机选择子弹类型，攻击结束回到 `idle`。
- 视觉资源优先使用现有序列帧和运行时程序化特效，不复制参考图片中的商业素材。

## 配置与容错

配置 JSON 缺失、解析失败或字段为非法值时，`GameConfigService` 使用内置安全默认值并输出中文警告。所有新增的速度、冷却、能量、伤害、尺寸和输入说明都从配置读取，避免继续把玩法参数散落在组件代码中。

## 验证

- 运行配置 JSON 结构和资源路径检查脚本。
- 编译 `Assembly-CSharp`、`Assembly-CSharp.Player` 与 Editor 程序集。
- Unity Play Mode 中按“开始游戏 → Boss idle → Boss 发射两种子弹 → 玩家开枪收集紫色子弹 → 三格满 → 按 K 放大招 → Boss 进入 dead”顺序检查，并记录到中文测试日志。
