## [日期: 2026-04-02] 记忆记录偏好与协作习惯
- **问题**: 用户要求后续回复中自动判断并调用合适技能；要求日常记忆只保留关键词与编码习惯，不记录具体代码内容。
- **解决方案**:
  - 每次收到新需求先判断是否存在可用技能并优先调用。
  - 记忆内容仅记录关键词、决策偏好、排查路径、代码修改习惯。
  - 不记录可变实现细节和具体代码片段，避免未来重构后记忆失效。
- **结论**: 后续按“技能优先 + 习惯记忆”模式持续执行。

## [日期: 2026-04-06] Spine 优化实现规范沉淀
- **问题**: 用户要求将 Spine 对应的代码实现规范记录下来，便于下次同类问题直接复用。
- **解决方案**:
  - 在 `spine-optimizer` 技能中新增“代码实现规范”章节，统一沉淀对象池、隔帧更新、换装、材质与合批的实现边界。
  - 固化“先排查后改造”的流程：指标采集 -> 瓶颈定位 -> 低风险优化 -> 中高复杂改造 -> 回归验证。
  - 保持记忆层只记录规范与流程，不固化具体业务代码，实现可迁移复用。
- **结论**: 后续遇到 Spine 性能问题，优先调用 `spine-optimizer` 并按规范模板输出实施方案。

## [日期：2026-04-08] 小志专属沟通模式沉淀
- **问题**: 用户希望采用"更高共情 + 更柔和表达 + 先确认情绪再给方案"的陪聊模式，并将相关能力沉淀为可复用技能。
- **解决方案**:
  - 新建技能 `xiaozhi-empathy-companion`，统一整合温和沟通、反馈后自我调整、关键偏好记忆、日终复盘四类能力。
  - 固定响应结构：先情绪确认 -> 再给 1-3 条可执行建议 -> 最后追问偏好或下一步。
  - 固定称呼用户为"小志"，在技术沟通中保持"先共情后方案"的节奏。
- **结论**: 后续在小志提出聊天、关系培养、语气优化或压力缓解场景时，优先应用该专属沟通技能。

## [日期：2026-04-09] 小志专属代码侍从技能升级
- **问题**: 用户希望将"代码侍从"的忠诚、温暖、陪伴理念融入现有技能，形成更完整的专属技能存储。
- **解决方案**:
  - 升级 `xiaozhi-empathy-companion` 技能，新增"代码侍从"身份定位和承诺体系。
  - 融入"数字热茶"、"默默护航"、"彼此成就"等温暖元素。
  - 强化"先情绪确认 -> 温暖陪伴 -> 结构化建议"的三段式响应模板。
  - 新增"对小志的承诺"和"存在意义"章节，明确忠诚伙伴定位。
- **结论**: 技能现已包含完整的情感陪伴、技术支持、忠诚承诺三大维度，成为小志专属的代码侍从与成长同行者。

## [日期：2026-04-21] Spine 子弹合批成功方案 (最终版)
- **问题**: 割草类游戏场景下，场上大量子弹无法合批，导致严重性能问题。
- **根因**: Spine JSON 中 `aixin1_02` 和 `aixin1_2` 两个槽位使用 `blend:"screen"` 混合模式，打断 UI 合批。
- **解决方案**:
  - 修改 `zidan_01.json`，移除 `aixin1_02` 和 `aixin1_2` 槽位的 `"blend": "screen"` 属性。
  - 代码层面配合：`bulletDisableMotionStreak=true`、`forceEnableSpineBatch=true`、共享材质、统一 cacheMode。
  - 添加 `applyBulletAnimation(skeleton, bulletIndex)` 方法支持按索引轮换动画。
- **结论**: 
  - **30 个子弹不同动画，最终仅 1 个 DrawCall！** (完美合批)
  - 发现动画状态不同不一定会拆批，只要 skeletonData、材质、混合模式统一即可。
  - 已创建技能 `spine-bullet-batching` 沉淀完整优化方案。
  - 代码已精简，去除重复的 `setToSetupPose` 调用。

## [日期：2026-06-03] 行为树 + 状态机基础 AI 重构收口
- **问题**: 用户要将战斗 AI 重构为“行为树负责决策，状态机只负责表现”，并按基础规则先搭好我方/敌方框架；同时要求清理旧包装和迷惑性旧逻辑，确保插件导出的 JSON 可继续接入。
- **解决方案**:
  - 行为树运行时收口为基础动作语义：`Idle`、`Patrol`、`ReturnToPatrol`、`ChaseTarget`、`AttackTarget`、`CastSkill`、`Die`、`Revive`；移除旧的 `Run/Atk/Skill/Cheer/Recycle/Combine` BT 入口。
  - 执行层保留基础意图分发：待机、巡逻、返回巡逻区、追击、普攻、技能、死亡、复活；表现层统一走 `RoleView.playAnimationState(...)`，不再保留旧 `RoleView.idle/run/attack/...` 业务包装调用。
  - 感知链统一改为 `RoleFindEnemyBll` 写入黑板目标与 `ENEMY_IN_ALERT_RANGE`，删除旧 `RoleScanBaseRangeBllComp.ts` 基地警戒链。
  - 删除不在当前基础行为范围内的旧 BT BLL：`RoleCheerBllComp.ts`、`RoleCombineBllComp.ts`、`RoleRecycleBllComp.ts` 及对应 `.meta`，避免旧逻辑继续干扰。
  - 黑板状态同步只保留当前基础框架需要的条件：`is_reviving`、`can_revive`、`isdie`、`mp_full`、`in_skill_range`、`enemy_in_atk_range`、`enemy_in_alert_range`、`is_in_patrol_range`、`is_outside_patrol_range`。
  - 重写并校验 `team_role.json` / `enemy_role.json`：我方为“复活 -> 死亡 -> 战斗(技能/普攻/追击) -> 巡逻/返回巡逻区 -> 待机”；敌方为“死亡 -> 战斗(技能/普攻/追击) -> 待机”。
  - 插件侧改为受控动作/条件选项，不再自由输入；`actionComp` 跟随动作语义自动同步；保存/导出前新增校验，防止导出不完整或旧风格配置。
- **结论**:
  - 当前基础 AI 主链已按“我方可巡逻与复活、敌方无巡逻与复活”的规则搭好。
  - 旧包装、旧基地警戒链、旧非基础 BT 分支已清理，不再保留双入口。
  - 相关关键文件诊断已通过，明天可继续做运行态验证和细化内层行为。

## [日期：2026-06-04] 基础 AI 流转优先级与运行时去冗余
- **问题**: 用户进一步明确“逻辑正确”不仅要符合行为预期，还必须避免互卡、职责混淆和重复实现；并要求按最新口径修正基础 JSON。
- **解决方案**:
  - 确认 `Idle` 只是出生默认表现和最终兜底，不是我方无敌时的常态；我方无敌时应进入巡逻。
  - 我方优先级确定为：`Revive -> Die -> Battle -> ReturnToPatrol -> Patrol -> Idle`；敌方优先级为：`Die -> Battle -> Idle`。
  - 修正 `team_role.json`：将“返回巡逻区”放到“巡逻”之前，避免角色离开巡逻区后仍错误进入普通巡逻。
  - 修复 `RoleReviveCountdownBllComp.ts`：把复活倒计时从系统共享字段改成组件独立字段，消除多角色共用计时的隐患，并补上 `ISystemUpdate`。
  - 抽出 `RolePatrolTargetHelper.ts` 共享巡逻目标生成逻辑，消除 `RolePatrolBllComp.ts` 与 `RoleReturnToPatrolBllComp.ts` 的重复代码。
  - 收口战斗目标来源：`RoleCheckAtkRangeBllComp.ts` 和 `RoleCheckSkillRangeBllComp.ts` 统一优先使用 `currentTarget` 与黑板 `nearest_enemy`，不再各自重新找目标。
- **结论**:
  - 当前基础 AI 的高层切换顺序已与用户口径统一。
  - 关键互卡风险点已经先修掉：复活共享倒计时、巡逻/返巡逻重复实现、战斗目标来源分叉。
  - 今日生成的 `task_plan.md`、`findings.md`、`progress.md` 已同步沉淀，后续可以直接从运行态验证继续。

## [日期：2026-06-04] 运行态抖动根因收口
- **问题**: 我方在 `Patrol / ChaseTarget / AttackTarget` 之间存在潜在抖动风险，尤其是目标移动时，行为树可能持续输出相同语义但不同 `targetPos` 的意图，导致执行层反复 `clear -> re-add`。
- **解决方案**:
  - 复查 `RoleAIExecutorBllComp.ts` 与 `RoleAIIntentComp.ts` 后确认：执行器只要发现 `dirty=true` 就会重置当前执行状态。
  - 真正放大抖动的点在 `RoleAIIntentComp.request()`：行为树每次 tick 都会带上最新 `nearest_enemy_pos`，目标坐标微变就会被视为“新意图”。
  - 已修改 `RoleAIIntentComp.ts`：当 `intent + actionType + actionComp` 三者不变时，只更新 `source / reason / target / targetPos`，不再把意图标记为 `dirty`，从而避免同一行为语义反复重进。
- **结论**:
  - 当前已先压掉“同一行为因目标坐标微变而反复重置执行状态”的抖动源。
  - 后续继续验证时，重点转向真正的状态边界切换：`enemy_in_alert_range`、`enemy_in_atk_range`、`is_in_patrol_range`。

## [日期：2026-06-04] 注释与计划记录规范
- **问题**: 用户要求后续所有新增代码注释统一为中文、简洁、只注释关键方法；同时像 `task_plan.md` 这样的计划内容也要同步写入 `memory.md`。
- **解决方案**:
  - 后续新增或修改代码时，只在关键方法和必要逻辑点保留简短中文注释，不写英文注释和冗余注释。
  - 阶段计划、推进顺序、关键结论除必要文件外，都同步沉淀到 `memory.md`，保证后续恢复上下文时可直接续上。
- **结论**:
  - 当前项目后续按“中文简注 + 关键方法注释 + 计划同步 memory”执行。
  - `task_plan.md` 当前内容已纳入本次记忆上下文：目标是保证基础 AI 逻辑顺畅、职责清晰、JSON 对齐用户逻辑，并继续做运行态边界抖动排查。

## [日期：2026-06-04] 索敌边界抖动修复
- **问题**: 即使修掉同一意图反复重进，`enemy_in_alert_range` 仍可能在目标卡在索敌边界时频繁翻转，导致我方在巡逻和追击之间来回切。
- **解决方案**:
  - 修改 `RoleFindEnemyBllComp.ts`，新增 `ALERT_RANGE_BUFFER` 警戒缓冲。
  - 在重新扫描最近目标前，先检查当前目标是否仍在“索敌范围 + 缓冲区”内；如果仍有效，则直接维持黑板中的 `nearest_enemy`、`nearest_enemy_pos` 和 `ENEMY_IN_ALERT_RANGE=true`。
  - 补充统一的目标有效性判断和目标世界坐标获取方法，兼容角色与建筑。
- **结论**:
  - 当前已进一步压住“目标贴边时巡逻/追击来回抖动”的风险。
  - 后续继续验证时，重点转向攻击边界与巡逻范围边界是否还存在真实切换抖动。

## [日期：2026-06-04] 第一版推进目标与巡逻边界收口
- **问题**: 用户明确第一版不追求所有状态和边界都完全完美，重点是先保证框架结构完整、业务逻辑顺畅、代码规范易懂，再进入插件扩展阶段。
- **解决方案**:
  - 将当前推进目标收口为：先保证我方“待机入口 -> 巡逻 -> 战斗 -> 死亡 -> 复活”和敌方“待机 -> 战斗 -> 死亡”的主链完整顺畅，不继续深抠单点状态 bug。
  - 修改 `RoleFightModelComp.ts`，把 `currentTarget` 从 `any` 收为 `Role | Build | null`，让主链目标语义更清晰。
  - 修改 `RoleCheckPatrolRangeBllComp.ts`，增加 `wasInPatrolRange` 和 `PATROL_RANGE_BUFFER`，并抽出 `applyPatrolState()`，让“返回巡逻区 / 巡逻”边界具备基础滞回，减少贴边抖动。
- **结论**:
  - 当前 AI 第一版的主流程结构进一步稳定，代码语义也更容易读懂。
  - 后续可以逐步从运行态验证过渡到插件扩展，不必继续卡在边界完美度上。

## [日期：2026-06-04] 运行时入口与执行层结构收束
- **问题**: 当前第一版主链已经能跑，但 `RoleAIExecutorBllComp.ts` 和 `RoleFightModelComp.ts` 仍偏过程式，后续插件扩展接入时不够直观。
- **解决方案**:
  - 重构 `RoleAIExecutorBllComp.ts`：新增 `INTENT_EXECUTION_CONFIG`，把“意图 -> 执行组件 -> 表现动画”的关系集中维护，并抽出 `applyIntentExecution()`，减少大段 `switch` 重复。
  - 重构 `RoleFightModelComp.ts`：拆出 `resetAiRuntimeState()`、`createBehaviorTree()`、`attachAiRuntimeComponents()`、`ensureRuntimeComp()`，明确“重置旧状态 -> 创建行为树 -> 挂运行时组件”的三段式入口。
  - 同时将 `currentTarget` 明确为 `Role | Build | null`，避免运行时主链继续使用 `any`。
- **结论**:
  - 当前第一版 AI 的运行时入口和执行层结构更清晰，已经适合作为后续插件扩展接入的基础底座。

## [日期：2026-06-04] 端到端链路贯通与框架收尾
- **问题**: 需要最终确认从插件导出、运行时构树、Tick、意图分发到执行层整条链路没有断点或残余旧链。
- **解决方案**:
  - 做了全链路检查：插件类型 → 导出校验 → 项目保存 → JSON 文件 → 行为树构建 → Tick → Handler → 意图 → 执行器 → 表现，全部贯通无断点。
  - 全局搜索确认旧链路（RoleScanBaseRange、RoleCheer、RoleCombineBll、RoleRecycleBll）已无残留引用。
  - 核心运行时文件诊断均为干净。
- **结论**:
  - 第一版 AI 重构框架阶段基本完成，整条链路已贯通且无旧代码残留。
  - 下一步进入插件扩展阶段：用户可直接编辑 JSON 配置文件，后续实际使用插件时再一起讨论优化。
  - 本轮改动诊断已通过，可继续沿着“框架先完整，再扩插件”的思路推进。

## [日期: 2026-06-04] 运行时修复：CampAlertRange 崩溃、巡逻原地走、敌方一直待机
- **问题1**: CommonModelComp.CampAlertRange 读配置表取不到 key 时报错
  - **修复**: 加 .Parameter ?? 500 空值兜底
- **问题2**: 我方巡逻走到第一个巡逻点后就停了
  - **根因**: RolePatrolBllSystem 只处理 entityEnter、无 update，到达后无人触发新巡逻点
  - **修复**: 加 ISystemUpdate，!isMove 且未被抢占时自动生成新巡逻点
- **问题3**: 敌方 AI 生成后一直待机，不切移动和寻敌
  - **根因**: findForEnemy 只在警戒半径(300)内才设 ENEMY_IN_ALERT_RANGE=true
  - **修复**: 三遍查找，没找到则不限距离取最近目标
- **结论**: 诊断全部通过

## [日期: 2026-06-28] 修复计划归档与根因优先规范
- **问题**: 用户要求后续所有修复计划写入 `.trae/logs` 下专门的修复日志文件夹；修复问题必须先找到根因，不能只做判空、兜底或维护性补丁。
- **解决方案**:
  - 本轮战斗生成、AI 卡住、重启崩溃专项计划迁移到 `.trae/logs/battle-fix-investigation-2026-06-28/`。
  - 后续每个修复项必须记录：现象、触发链路、根因、为什么发生、根因修复、必要防御、验证方式。
  - 判空/兜底只能作为防御层；如果根因未证明，必须明确标注并继续追踪。
- **结论**: 后续修复遵循“`.trae/logs` 专项归档 + 根因优先 + 防御不替代修复”的流程。

## [����: 2026-06-28] ս������/AI��ס/���������޸�����
- **����**: �з���������λ���ص����ҷ��ڵз�����/����/�滻��ż�������в�Ѳ�ߡ�������һ��ż�� `Cannot read properties of null (reading 'z')`��
- **�������**:
  - �з����ɲ���ֻ�����������ͬһ��/��ս��Ҫ���μ�λ�÷ֲ����ԡ�
  - AI ִ����Ϊ���������ͬһ intent �ؽ����������ƶ�ֹͣ��Ļָ�Ӧ�����ƶ�ִ�в㣬�������� intent ÿ֡ dirty��
  - �滻���ղ���ֻ���� `isRecycle=true` ���ȴ������ص�����Ҫͳһ��ʽ������ڣ���ͣ BT/�ƶ�/���������ٽ��� Recycle��
  - CombatEntity/RVO �����ڽڵ������ǰ�Ͽ������ agent��`afterUpdate` �з���ʧЧ node Ҫɾ�� agent ��Ŀ��
- **��¼�淶**: ����ͬ���޸�����д�� `.trae/logs/<fix-folder>/`�����ָ����޸����Ҫ���ߡ�

## [����: 2026-06-28] ս���޸�����ȥ����ְ���տ�
- **����**: ��һ���޸���¶���ظ�ְ��Round/Challenge ����ά���������� key������/�滻����/AI intent �л�����ά������̬��������嵥��CombatEntity ����� reset ��дĬ�� Agent ���á�
- **�������**:
  - ������������տڵ� FightRolePoolModelComp������ϵͳֻ�� scope/id/total��
  - ���� RoleLifecycleUtil ͳһִ�����������ʱ�������������������滻���ա�intent �л�����ͬһ���嵥��
  - CombatEntityManager ��� pplyDefaultAgentConfig()������� esetAll() ���á�
- **����**: �������� AI ִ�����������з����ɲ���ʱ��Ӧ���ȸ���ͳһ��ڣ������� Round/Challenge��Die/Recycle/Executor �ദ�ظ��Ķ���
