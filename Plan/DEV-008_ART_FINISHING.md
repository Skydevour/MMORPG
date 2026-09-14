# DEV-008 正式资源制作与接入清单

## 状态

DESIGN-005 的正式动作美术尚未交付。当前洋葱/胡萝卜资源是开发占位；已有土豆嘴部三路姿态和 Hero 反制/受击专用原画不足。不能把图形变形或重复帧记为新原画。

本会话没有可调用的 image_gen，图像 CLI 也缺少 API 密钥。当前不执行未经确认的替代服务调用。功能、输入、碰撞、构建和测试可独立继续。

## 制作规格

| 角色 | 待制作动作 | 存放目录 | 对齐与关键点 |
| --- | --- | --- | --- |
| Hero | parry_success、hurt、专用 dead | Assets/Res/Hero/Frames | 遵循现有脚底参考、所有动作共享 trim 画布；单独登记身体旋转中心与枪口 |
| Potato | emerge、spit_high、spit_mid、spit_low、recover、defeat | Assets/Res/Bosses/Potato/Frames | 每一动作共享底部中心；三路嘴部姿态必须分别对应地面上 1.75/0.95/0.35 的出弹点 |
| Onion | emerge、idle、cry_tell、cry_loop、wipe、hurt、defeat | Assets/Res/Bosses/Onion/Frames | 全动作同尺寸、底部中心，左右眼锚点；眼泪向上形成雨云的来源可见 |
| Carrot | emerge、idle、charge、lock、beam、seeker、stun、hurt、defeat | Assets/Res/Bosses/Carrot/Frames | 全动作同尺寸、底部中心；眼睛与叶片锚点，锁定阶段保持方向清晰 |

使用独立原创绘制、老动画的线条与纸张质感，风格与当前花园背景相容。先制作统一角色转面与动作关键帧，再补中间帧；不强制九帧。透明 PNG，无格线、无相邻动作碎片，连续帧轮廓完整，身体接地点不得随画布变化漂移。

离线处理应先求同角色所有动作 alpha 并集范围，再按固定脚底基准统一裁切；禁止逐帧独立 trim 后让运行时补偿。身体伸展与烟尘应保留在统一画布内，烟尘/子弹优先独立特效资源。

## 接入验收

1. 先以接触表逐帧检查透明边界、脚底、角色中心、嘴/眼关键点；生成资源前后尺寸和偏移报告。
2. 更新 GardenBossAssets 动作目录并沿用 FrameAnimator，不为新角色重复创建播放框架。
3. 发射动作时间绑定清楚的发射关键帧；普通受击反馈不得打断正在播放的攻击。
4. 1280×720、1920×1080、1024×768 检查完整轮廓、HUD 安全带、埋地量和特效遮挡。
5. 原画验收完成后才允许去掉 developmentArt 标志；构建门禁已禁止占位资源进入非 Development 发布包。
6. 本文是待执行清单，不能作为已完成美术或人工视听验收证据。
