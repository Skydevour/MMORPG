# DEV-006 开发执行计划

目标：实现 PM-006 四阶段修复，复用 Unity 6 现有状态机、帧动画与对象池。

1. PlayerController2D、PlayerSpawner、FrameAnimator、导入器和配置：修复输入顺序、跳跃消费、中心旋转、帧选择及 Boss 构图。
2. HitFlashEffect、ScreenShakeEffect、VFX、BossProjectile：建立共享材质、池化粒子、单次采集、真实大招命中。
3. PotatoBossController/StateDriver：攻击预备、关键点和收招统一时钟，现有阶段保留。
4. GameManager、BossBattleHud、HealthBarView：暂停统一、实际血条裁切、死亡与重试闭环。
5. PlayMode 增加真实碰撞、重复采集、暂停、伤害、动画和震屏回归；Unity 批处理测试、构建、截图验证。

验证命令：Unity.exe -batchmode -projectPath E:/UnityProject/MMORPG/MMORPG -runTests -testPlatform PlayMode -testResults Plan/playmode_results_DEV-006.xml -logFile Plan/unity_DEV-006.log。构建另用编辑器验证入口。不退出或影响其他项目正在运行的 Unity。

状态：四阶段实现已交付，技术验证通过。14/14 PlayMode、Windows 构建和三种尺寸的发布版渲染已完成，证据见 `AGENT_HANDOFF_DEV-006.md`。人工难度与原画质量验收保留为待复核；不创建职责分支，不自动提交推送。
