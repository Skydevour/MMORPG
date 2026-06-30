---
name: "spine-bullet-batching"
description: "Cocos Creator Spine 合批优化指南。提炼通用合批条件和操作步骤，适用于大量 Spine 特效合批场景。当需要优化 Spine DrawCall 时调用。"
---

# Spine 合批优化指南

## 🎯 核心思想

**UI 合批被打断的本质原因**：渲染状态不一致导致无法共享批次。

只要保证以下 5 个条件，即使大量 Spine 播放不同动画，也能完美合批！

## 🔑 合批 5 要素

| 要素 | 影响 | 处理方式 |
|------|------|---------|
| **混合模式** | screen/add 等混合模式会强制拆批 | 改为 normal |
| **enableBatch** | 组件未开启批处理 | 强制设为 true |
| **材质** | 不同材质实例导致拆批 | 共享同一材质 |
| **渲染状态** | useTint/premultipliedAlpha 不一致 | 统一设置 |
| **附加渲染组件** | MotionStreak 等增加额外 DC | 按需禁用 |

## 🚀 快速操作流程

### 步骤 1：检查并修改 Spine JSON

```bash
# 搜索所有 screen blend 槽位
grep '"blend": "screen"' *.json

# 移除这些属性，改为默认 normal 混合模式
```

### 步骤 2：代码统一渲染状态

```typescript
// 核心配置（必须全部启用）
forceEnableSpineBatch = true          // 强制开启组件批处理
forceUnifySpineRenderState = true     // 统一 PMA/Tint 状态
useSharedMaterial = true              // 共享材质
enableDrawCallOptimization = true     // 启用优化策略

// 按需配置
bulletDisableMotionStreak = true      // 禁用拖尾效果
```

### 步骤 3：初始化时应用设置

```typescript
// 伪代码流程
for (const spine of spines) {
    // 1. 强制开启合批
    spine.enableBatch = true;
    
    // 2. 共享材质
    spine.customMaterial = sharedMaterial;
    
    // 3. 统一渲染状态
    spine.useTint = false;
    spine.premultipliedAlpha = true;
    
    // 4. 禁用附加渲染组件（如有）
    motionStreak.enabled = false;
    
    // 5. 设置动画
    spine.setAnimation(0, animName, true);
}
```

## 📊 验证方法

**合批签名日志**：
```typescript
signatureCount=1  → 完美合批（所有共享同一批次）
signatureCount>1  → 有拆批（检查上述 5 要素）
```

**DrawCall 参考**：
- 30 个 Spine 特效，合批后应只有 **1-2 个 DC**
- 如果 DC 数量接近 Spine 数量，说明未合批

## ⚡ 关键发现

1. **不同动画也能合批** - 动画状态/相位不同不一定会拆批
2. **screen blend 是最大杀手** - 1 个 screen 槽位就能打断整个批次
3. **MotionStreak 增加额外 DC** - 每个实例独立渲染

## � 调试技巧

```typescript
// 打印每个 Spine 的渲染状态，找出不一致的项
console.log(`enableBatch=${sk.enableBatch}, useTint=${sk.useTint}, pma=${sk.premultipliedAlpha}`);
```
