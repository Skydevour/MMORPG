# 第一关资源计划

## 目标

使用 Unity 自带 2D Tilemap 工具和原型美术资源，搭建第一关菜园测试场景。当前目标是先让玩家能在固定画布内移动、跳跃、冲刺和射击，Boss 暂不接入。

## 已生成资源

### 主角

- 原始动作图集：`Assets/Art/Generated/Hero/cup_hero_action_atlas_raw.png`
- 当前动作预览图集：`Assets/Res/Hero/cup_hero_action_atlas.png`
- 运行时动作帧目录：
  - `Assets/Res/Hero/Frames/idle`
  - `Assets/Res/Hero/Frames/run`
  - `Assets/Res/Hero/Frames/jump`
  - `Assets/Res/Hero/Frames/dash`
  - `Assets/Res/Hero/Frames/shoot`

### 第一关场景

- 背景：`Assets/Res/Level01_Garden/Background/garden_background_wide.png`
- TileSet：`Assets/Res/Level01_Garden/garden_tileset.png`
- Tile：`Assets/Res/Level01_Garden/Tiles/*.png`

## 当前资源规格

- 主角动作帧不再强制固定 9 帧，而是按原始图中的完整角色组件拆分。
- 当前主角动作帧数：
  - `idle`：10 帧
  - `run`：10 帧
  - `jump`：9 帧
  - `dash`：8 帧
  - `shoot`：7 帧
- 主角每帧统一为 `307x167`。
- 主角根点固定为图内 `x=99, y=166`，对应 Unity 底部 pivot 为 `x=99, y=1`。
- 每张主角序列帧中的角色身体都放在同一个固定区域，不能只依赖 Unity Sprite pivot 修正。
- 主角帧需要按所有帧非透明像素的最大包围范围进行统一 trim，尽量去除共同空白区域；禁止逐帧单独 trim 导致尺寸不一致或角色位置漂移。
- trim 是资源离线预处理步骤，加载时直接使用处理后的 PNG，不再做额外裁剪。
- 拆帧必须按连通组件处理主角本体和附属特效，不能用简单矩形单元格把相邻帧残片带入。
- 子弹、烟尘、冲刺线和枪口火花必须跟随当前主角脚底根点，不允许以这些特效自身作为对齐点。
- 背景尺寸统一为 `2048x1152`，对应 16:9 画布。
- Tile 尺寸统一为 `128x128`。
- Unity 导入默认 PPU 为 `128`。

## 关卡搭建方式

当前采用混合方案：

1. 使用完整背景图作为非碰撞的视觉背景。
2. 使用 Unity Tilemap 绘制可碰撞地面、土块、平台和边缘。
3. 使用独立装饰 Sprite 表现栅栏、花、石头、杂草和植物。
4. 土豆 Boss 后续作为独立动画 GameObject 接入，不放进背景图。

## Tilemap 与整张背景图对比

### 使用 Tilemap 绘制地面

优点：

- 可以直接在 Unity 中绘制、擦除和调整。
- 适合制作可碰撞地面、平台、土块和关卡结构。
- 可以使用 `TilemapCollider2D` 和 `CompositeCollider2D`。
- 便于快速测试关卡设计。
- 同一套 Tile 可以复用到多个测试场景。

缺点：

- Tile 数量较少时容易看出重复感。
- 手绘风格场景可能显得网格化。
- 需要维护 Tile 切分、Tile Palette、排序层和碰撞设置。
- 装饰层次需要额外图层或独立 Sprite。

适合用途：

- 可碰撞地面。
- 平台。
- 土块墙。
- 可复用关卡结构。
- 原型布局。

### 使用整张背景图

优点：

- 画面整体性最好。
- 不容易出现 Tile 重复感。
- 放置最快，只需要一个 SpriteRenderer。
- 适合表现天空、远景、树木、栅栏和整体氛围。

缺点：

- 不适合承载碰撞和关卡迭代。
- 平台位置调整时需要重绘或额外遮罩。
- 复用性弱。
- 后续做长关卡或卷轴关卡会比较麻烦。

适合用途：

- 天空、树、云、远景栅栏、菜园氛围和不可交互装饰。
- 非交互视觉层。

## 最终决定

当前第一关使用：

- 整张背景图负责场景氛围。
- Tilemap 负责玩法地面和碰撞。
- 独立 Sprite 负责可交互对象和前景装饰。

这样能最快得到可玩的原型，同时保留手绘菜园的视觉风格。

## Unity 接入规范

推荐导入设置：

- 主角帧：Sprite Mode 使用 `Single`，Pixels Per Unit 使用 `128`，资源生成阶段固定主角身体区域并进行统一 trim，Unity pivot 仅作为辅助保险。
- 主角图集：当前只作为预览和参考。如果后续直接使用图集，必须按每行动作真实帧数切分，不能按固定列数硬切。
- Tile：Sprite Mode 使用 `Single`，Pixels Per Unit 使用 `128`，Compression 使用 `None`。
- 背景：Sprite Mode 使用 `Single`，Pixels Per Unit 使用 `128`，逻辑世界尺寸为 `16x9`。

推荐场景层级：

```text
Level01_Garden
  Grid
    GroundTilemap
    DecorationTilemap
  Background
    GardenBackground
  Gameplay
    Player
      Visual
    BossSpawnPoint
    PlayerSpawnPoint
```

推荐排序层：

- `Background`
- `Midground`
- `Ground`
- `Characters`
- `Projectiles`
- `Foreground`
- `UI`

## 已知限制

- 当前资源仍是原型质量，不是最终生产资源。
- 跑步烟尘、冲刺线和枪口特效仍混在动作帧中，后续更推荐拆成独立特效。
- 后续正式资源应在绘制阶段就标记脚底接地点和角色根点，确保所有动作帧天然对齐。

## 2026-07-06 地形与 Boss 资源更新

- 第一关地形表现改为更接近参考图 2 的完整舞台：整张背景图负责主要画面，Tilemap 只保留少量非碰撞装饰，不再显示整排方块地面。
- 可走地面改为隐藏的 `StageCollision`，地面顶面固定在 `y=-2.5`，与当前玩家出生脚底位置对齐。
- 当前 Tile 的 `colliderType` 统一为 `None`，避免 Tile 图片轮廓生成不稳定碰撞，玩家落地只依赖隐藏 BoxCollider。
- 土豆 Boss 已生成原创序列帧资源，不直接复制商业素材：
  - 原始图集：`Assets/Art/Generated/Bosses/Potato/potato_boss_sheet_raw.png`
  - 预览图集：`Assets/Res/Bosses/Potato/potato_boss_atlas.png`
  - 运行时帧：`Assets/Res/Bosses/Potato/Frames`
- 土豆 Boss 动作规划：
  - `idle`：待机呼吸。
  - `angry`：愤怒蓄力。
  - `attack_spit`：吐土块攻击。
  - `hurt`：受击后仰。
- 土豆 Boss 帧规格：
  - 每个动作 6 帧。
  - 单帧统一 `268x255`。
  - 资源已离线去绿幕、统一 trim，并以底部中心作为根点。
  - Unity 导入 pivot 使用 `x=0.5, y=1/255`。
