# 土豆 Boss 序列帧处理进度

## 2026-07-30

- 已确认 4 个动作、24 张帧图全部为 `268×255`。
- 已确认垂直底线一致，问题集中在内容横向漂移和 Pivot 非严格底部中心。
- 已完成透明边界及底部接触区域统计。
- 已确定采用全动作统一画布、离线脚底对齐方案。
- 已新增 Tools/Art/NormalizePotatoBossFrames.ps1。
- 首次运行因 Windows PowerShell 5 的无 BOM UTF-8 中文解析问题失败，失败发生在编译阶段，PNG 未被修改。
- 已将脚本转换为 UTF-8 BOM，第二次运行成功。
- 已处理 24 张 PNG：统一画布 313×253，脚底锚点 (156,252)，最大中心误差 .490 像素。
- 已将 Unity 土豆 Boss Sprite Pivot 改为 (0.5, 0)。
- 下一步：执行独立资源检查和 Editor 编译验证。

## 完成结果

- 24 张帧图和 24 个 `.meta` 已全部通过一致性检查。
- Unity 导入器与现有 `.meta` 的 Pivot 均为 `(0.5,0)`。
- 规范化工具 `-ValidateOnly` 回归通过，无临时文件残留。
- 首次 Editor `--no-dependencies` 构建因 `Temp/Bin/Debug` 依赖不存在而失败；完整 Editor 构建通过，0 警告、0 错误。
- 本轮四个阶段全部完成，剩余工作是 Unity Play Mode 视觉确认。

## 2026-07-31

- 收到视觉反馈：`attack_spit_01` 含有误裁切的下方碎片，现有锚点算法不能继续使用。
- 已新增阶段 5 至阶段 7：修复攻击帧、改造冲刺粒子、执行回归。
- 当前正在收集连通区域和 Hero 闪避特效调用链证据。

## 2026-07-31 完成结果

- 已修复 `attack_spit_01` 及另外 5 张攻击帧的误裁切碎片问题。
- 已完成 Boss 帧二次规范化、Pivot 验证和临时文件检查。
- 已完成冲刺粒子 Burst 替换并移除旧序列帧资源。
- `Assembly-CSharp`、`Assembly-CSharp.Player` 和 `Assembly-CSharp-Editor --no-dependencies` 均构建通过，0 警告、0 错误。
- 剩余手工验证：在 Unity Play Mode 观察攻击帧切换与 Shift/L 冲刺粒子表现。
## 2026-08-02 土豆 Boss 战斗流程

- 已新增全局战斗配置 Assets/Resources/Config/GameConfig.json，覆盖关卡、玩家、Boss、投射物和大招参数。
- 已完成 Boss 普通土块子弹与紫色可采集子弹的对象池生成和随机攻击。
- 已完成玩家紫色子弹采集、三格能量 HUD、K 键大招及 Boss 伤害。
- 已修复运行时粒子系统默认播放导致的 duration 警告。
- 已修正 PlayMode 测试程序集被误分类为 EditorOnly 的问题。
- 最终 PlayMode 测试 1/1 通过，三套程序集构建均为 0 警告、0 错误。
## 2026-08-03 Boss 右侧三连射

- 已完成 Boss 右侧安全出生位置和同水平出生线。
- 已完成 attack_spit 周期内上、中、下三发短间隔水平连射。
- 已将三条弹道、攻击节奏、动画帧率和子弹速度集中到全局 JSON。
- 三套程序集串行编译通过，0 警告、0 错误。
- PlayMode 断言已补充，但因交互式 Unity Editor 占用项目锁尚未执行最终回归。
