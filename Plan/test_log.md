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
