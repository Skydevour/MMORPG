# MMORPG Agent 协作入口

本仓库使用三个职责独立的 Agent 共同开发 Unity 2D Boss Rush 项目。所有 Agent 共用当前 Git 分支和工作目录，但必须遵守 [Agents/SHARED_PROTOCOL.md](Agents/SHARED_PROTOCOL.md)。

## 调用路由

根据用户请求选择一个主 Agent；跨职责需求先由产品经理拆分和定优先级，再调用策划或开发：

| 用户需求 | 主 Agent | 必须加载 |
| --- | --- | --- |
| 产品方向、版本范围、竞品差异、优先级、验收和发布 | 产品经理 | `Agents/PRODUCT_MANAGER.md` |
| 玩法、关卡、地图、UI、角色资源、规则、数值和配表 | 策划专家 | `Agents/GAME_DESIGNER.md` |
| Unity C#、场景、Prefab、状态机、对象池、特效、测试和性能 | 开发专家 | `Agents/UNITY_DEVELOPER.md` |
| 同时涉及方向、设计和实现 | 产品经理 | 依次调用产品经理 -> 策划专家 -> 开发专家 |

## 角色调用方式

Codex 处理项目任务时，读取对应的角色文件作为当前 Agent 的职责边界。不要让一个 Agent 越权替代另外两个角色：

1. 产品经理定义目标、范围、优先级和验收标准。
2. 策划专家把目标转成设计文档、资源清单、规则和数值。
3. 开发专家依据已确认的设计实现代码、资源接入和测试。
4. 产品经理复核结果，更新任务状态，决定是否提交和推送。

## 共享仓库规则

- 三个 Agent 共用当前分支，不创建职责分支，不执行强制推送。
- 只有产品经理执行 `git commit` 和 `git push`。
- 策划和开发提交工作结果、测试证据和阻塞项，但不得擅自推送远程。
- 开始修改前必须登记 `Plan/AGENT_TASK_BOARD.md`，结束后必须补充 `Plan/AGENT_DAILY_LOG.md`。
- 详细流程、冲突处理和验收门禁见 [Agents/SHARED_PROTOCOL.md](Agents/SHARED_PROTOCOL.md)。
