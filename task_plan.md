# 土豆 Boss 序列帧统一计划

## 目标

将 `Assets/Res/Bosses/Potato/Frames` 下所有动作帧统一为相同画布尺寸，角色脚底接触区域对齐到画布底部中心，Unity Sprite Pivot 固定为 `(0.5, 0)`，避免同动作和跨动作切换时产生位置偏移。

## 阶段

- [x] 阶段 1：统计所有动作帧、透明边界和现有导入规则。
- [x] 阶段 2：编写并运行离线规范化工具。
- [x] 阶段 3：验证尺寸、底线、脚底中心和 Unity Pivot 配置。
- [x] 阶段 4：更新中文开发日志与测试记录。
- [x] 阶段 5：定位并清理 `attack_spit` 帧中的误裁切碎片，重新计算脚底锚点。
- [x] 阶段 6：将 Hero 冲刺烟雾替换为运行时粒子 Burst 特效。
- [x] 阶段 7：执行资源、Editor 编译与日志回归。

## 固定决策

- 所有动作共用一个全局画布，不按动作分别裁切。
- 使用图片底部 12 像素内的不透明像素重心估算脚底锚点。
- 所有内容离线写入 PNG，运行时不再做 trim 或位置补偿。
- 保留原文件名、目录和 `.meta` 文件，避免 Unity GUID 变化。
- Sprite Pivot 使用严格的底部中心 `(0.5, 0)`。

## 错误记录

| 错误 | 处理 |
|---|---|
| Windows 沙箱并行读取触发 `apply deny-read ACLs` | 改为经批准的顺序只读检查 |
| 本地图片查看工具触发同一 ACL 错误 | 使用透明边界和脚底像素统计完成确定性验证 |
| Windows PowerShell 5 无法正确解析无 BOM UTF-8 脚本中的中文 | 将规范化工具转换为 UTF-8 BOM 后重新运行 |
| Editor --no-dependencies 构建缺少 Temp/Bin/Debug 依赖 DLL | 改用完整 Editor 构建生成依赖并验证项目脚本 |
| 碎片清理器在源 PNG 仍被 Bitmap 占用时尝试覆盖文件 | 释放图片句柄后再写回文件 |
| 首次失败遗留固定名称临时 PNG，导致 GDI+ 二次保存报参数无效 | 临时文件改为 GUID 唯一名称 |
| PowerShell 双引号解析 Markdown 反引号，导致计划行包含控制字符 | 用单引号和整行正则替换修正文本 |
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
## 2026-08-03 Boss 右侧三连射

### 已完成

- [x] Boss 右侧安全出生和同水平出生线。
- [x] attack_spit 短间隔上中下三连射。
- [x] 子弹高速水平飞行。
- [x] 全局 JSON 配置和 PlayMode 断言。
- [x] 三套程序集 0 警告、0 错误编译。

### 待完成

- [ ] 关闭交互式 Unity Editor 后执行 MainScene PlayMode 回归。
- [ ] 进行画面级确认并记录截图或观察结果。
