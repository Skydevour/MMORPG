# 代码模块结构约定

## 目标

项目代码按职责分层，避免后续角色、怪物、子弹、Boss、关卡和通用框架逻辑混在同一个目录中。新增脚本时优先放到最具体的模块目录，只有真正跨玩法复用的能力才放入 `Framework`。

## Framework

`Assets/Scripts/Framework` 只存放不依赖具体玩法对象的通用能力。

- `Framework/Animation`
  - 序列帧动画数据和播放器。
  - 例如：`FrameAnimationClip`、`FrameAnimator`。
- `Framework/StateMachine`
  - 通用状态接口和状态机。
  - 例如：`IState`、`StateMachine`。
- `Framework/Pooling`
  - 通用对象池接口和对象池实现。
  - 例如：`IPoolable`、`ComponentObjectPool<T>`。

约定：`Framework` 不直接引用 `Game.Player`、`Game.Projectiles`、`Game.Level` 或 Boss 逻辑。

## Game

`Assets/Scripts/Game` 存放具体游戏运行逻辑。

- `Game/Core`
  - 游戏启动、全局运行时根节点、资源加载辅助。
  - 例如：`GameManager`、`PrototypeAssetLoader`。
- `Game/Level`
  - 关卡加载、地图、地形碰撞、出生点。
  - 例如：`LevelMapLoader`。
- `Game/Player`
  - 主角控制、主角生成、主角动画状态。
  - 例如：`PlayerController2D`、`PlayerSpawner`、`PlayerSpriteAnimator`、`PlayerStateDriver`。
- `Game/Projectiles`
  - 子弹、飞行物、弹幕基础逻辑。
  - 例如：`PlayerProjectile`。
- `Game/Bosses`
  - Boss 生成、Boss 状态机、Boss 攻击逻辑。
  - 后续土豆 Boss 建议放到 `Game/Bosses/Potato`。

约定：具体玩法模块可以引用 `Framework`，但同级模块之间要尽量通过清晰接口协作，避免互相硬耦合。

## Editor

`Assets/Scripts/Editor` 只存放编辑器工具。

- `Editor/Scene`
  - 场景构建、场景修复、测试场景生成工具。
- `Editor/Import`
  - 资源导入、贴图 pivot、PPU、压缩和透明设置。

约定：Editor 脚本不得被运行时代码引用。

## 命名空间

命名空间与目录保持一致。

- `MMORPG.Framework.Animation`
- `MMORPG.Framework.StateMachine`
- `MMORPG.Framework.Pooling`
- `MMORPG.Game.Core`
- `MMORPG.Game.Level`
- `MMORPG.Game.Player`
- `MMORPG.Game.Projectiles`
- `MMORPG.EditorTools.SceneSetup`
- `MMORPG.EditorTools.Import`

## 新增代码放置规则

- 新增主角相关逻辑放入 `Game/Player`。
- 新增怪物或 Boss 逻辑放入 `Game/Bosses`，如果是具体 Boss，继续按 Boss 名称建子目录。
- 新增子弹或弹幕逻辑放入 `Game/Projectiles`。
- 新增关卡、地图、地形、出生点逻辑放入 `Game/Level`。
- 新增通用状态机、对象池、序列帧播放、计时器等跨模块能力放入 `Framework` 对应子目录。
- 新增编辑器菜单、导入器、批处理工具放入 `Editor` 对应子目录。
