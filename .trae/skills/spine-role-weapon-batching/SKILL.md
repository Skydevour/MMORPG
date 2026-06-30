---
name: "spine-role-weapon-batching"
description: "总结角色本体与武器 Spine 共 atlas 合批方案。Invoke when 需要验证或实现角色+武器 Spine 合批、精简测试脚本或排查 DrawCall。"
---

# Spine Role Weapon Batching

## 目标

在 Cocos Creator 中验证“角色本体 Spine + 武器 Spine”成功合批。

这个 skill 适合以下场景：

- 用户已经把角色和武器的贴图合并到同一张 atlas
- 需要把测试脚本收敛成最小验证闭环
- 需要排查角色和武器为什么没有合批
- 需要把测试逻辑提炼成可复用 checklist

## 核心思路

不要强行把角色和武器拼成一个 Skeleton。

更稳妥的方案是保留：

- 角色一个 `sp.SkeletonData`
- 武器一个 `sp.SkeletonData`

只要它们满足以下条件，依然可以合批：

1. 角色和武器最终都引用同一个 atlas
2. 角色和武器共享同一个材质
3. `enableBatch` 一致
4. `useTint` 一致
5. `premultipliedAlpha` 一致
6. 没有额外 UI/拖尾/特效打断批次

## 资源准备

推荐在同一个目录下放置以下资源：

- `role.png`
- `role.atlas`
- `sanguo_juese_merge.json`
- `langrensha_wuqi_01_merge.json`

两个 merged json 的 `.meta` 都要绑定到同一个 `role.atlas.meta` 的 uuid。

## 最小测试脚本结构

测试脚本只保留四步：

1. 批量生成角色 prefab
2. 给 `spine`、`weapon1`、`weapon2` 绑定 merged skeletonData
3. 统一渲染状态并共享材质
4. 打印状态签名，确认是否只剩 1 组渲染状态

## 关键代码

### 1. 绑定 merged 资源

```typescript
const rolePath = "game/Battle/skeleton/hero/sanguo/role/sanguo_juese_merge";
const weaponPath = "game/Battle/skeleton/hero/sanguo/role/langrensha_wuqi_01_merge";

const roleData = await SpineLoader.loadSkeletonData(rolePath);
const weaponData = await SpineLoader.loadSkeletonData(weaponPath);

SpineLoader.bindSkeletonData(roleSkeleton, roleData);
SpineLoader.bindSkeletonData(weapon1, weaponData);
SpineLoader.bindSkeletonData(weapon2, weaponData);
```

### 2. 只保留 Spine 渲染

```typescript
const renderers = node.getComponentsInChildren(UIRenderer);
for (const renderer of renderers) {
    if (renderer instanceof sp.Skeleton) continue;
    renderer.enabled = false;
}
```

### 3. 统一渲染状态

注意：如果 atlas 里写的是 `pma: false`，测试代码里也要统一成 `false`，不要误设成 `true`。

```typescript
for (const sk of skeletons) {
    sk.enableBatch = true;
    sk.useTint = false;
    sk.premultipliedAlpha = false;
}
```

### 4. 共享同一材质

```typescript
let sharedMaterial = captureMaterial(skeletons[0]);
for (const sk of skeletons) {
    sk.customMaterial = sharedMaterial;
}
```

### 5. 选择皮肤并播放一个稳定动画

```typescript
function applySkin(sk: sp.Skeleton, skinName: string) {
    sk.setSkin(skinName);
    sk.setToSetupPose();
}

function playFirstAnimation(sk: sp.Skeleton) {
    const animations = sk.skeletonData?.getRuntimeData?.()?.animations;
    const animName = animations?.[0]?.name;
    if (animName) sk.setAnimation(0, animName, true);
}
```

## 验证标准

不要把 `skin` 或 `skeletonData` 也算进合批签名里。

对于“角色和武器共 atlas”这个问题，真正关键的是渲染状态是否一致，而不是它们是不是同一个 SkeletonData。

推荐只统计：

- material
- enableBatch
- useTint
- premultipliedAlpha
- blendFactor

示例：

```typescript
const sig = [
    `mat=${matId}`,
    `batch=${sk.enableBatch ? 1 : 0}`,
    `tint=${sk.useTint ? 1 : 0}`,
    `pma=${sk.premultipliedAlpha ? 1 : 0}`,
    `blend=${sk.srcBlendFactor}/${sk.dstBlendFactor}`,
].join("|");
```

如果最终：

- `stateSignatureCount = 1`

说明测试场景下角色和武器已经具备同批次渲染条件。

## 常见误区

### 误区 1：把 `skin` 放进签名

不同皮肤不等于不能合批。

如果把 `skin` 算进签名，日志会误判成“多批次”。

### 误区 2：把 `skeletonData` 放进签名

角色和武器通常就是两个不同的 SkeletonData。

只要它们落到同一张 atlas 并共享材质，仍然可能合批。

### 误区 3：PMA 设置和 atlas 不一致

atlas 写了 `pma: false`，脚本却强制 `premultipliedAlpha = true`，很容易导致材质或混合状态不一致。

### 误区 4：保留血条和其他 UI

角色 prefab 里如果还保留血条、名字、品质框等渲染组件，就会额外增加 DrawCall，干扰观察。

## 推荐工作流

1. 先生成 merged json，并绑定同一个 atlas
2. 再用极简测试脚本验证角色 + 双武器显示
3. 确认 `stateSignatureCount = 1`
4. 最后再把同样的资源和渲染策略迁回正式业务代码
