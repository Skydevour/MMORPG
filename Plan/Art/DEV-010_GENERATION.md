# DEV-010 原画生成准备

用户已明确授权CLI/API生成，使用内置imagegen技能提供的image_gen.py，默认gpt-image-2，不重复请求授权。当前Process/User/Machine均未配置OPENAI_API_KEY，因此未发出生成请求，也未产生新原画。

## 首轮输入

ART-012修订：优先加入References/ART-012/reference-2.png、reference-3.png、reference-4.png作为三Boss造型参考；reference-5.png用于另一次鼓土入场制作。reference-1.png为否决样例，不传作风格正例。具体动作见ART-012_REFERENCE_AND_ACTIONS.md。以下原始输入仍作为项目环境参考。

- 风格参考：Assets/Res/Bosses/Potato/potato_boss_atlas.png，仅参考完好角色，不沿用原图分隔和裁切瑕疵。
- 环境参考：Assets/Res/Level01_Garden/Background/garden_background_wide.png。
- 三Boss设定提示词：DEV-010_boss_style_prompt.txt。
- 失败界面提示词：DEV-010_ui_prompt.txt。
- 输出：output/imagegen/DEV-010/boss-style-v1.png、defeat-style-v1.png。先审核，不自动覆盖Assets。

## 执行示例

在本机配置OPENAI_API_KEY后，通过技能CLI执行，不打印密钥：

```powershell
& 'C:/Users/admin/.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe' 'C:/Users/admin/.codex/skills/.system/imagegen/scripts/image_gen.py' edit --model gpt-image-2 --image 'Assets/Res/Bosses/Potato/potato_boss_atlas.png' --image 'Assets/Res/Level01_Garden/Background/garden_background_wide.png' --prompt-file 'Plan/Art/DEV-010_boss_style_prompt.txt' --size 1536x1024 --quality high --out 'output/imagegen/DEV-010/boss-style-v1.png'
```

样稿通过后再以选定角色图生成独立动作。逐角色全动作联合trim、统一画布/脚底像素位置、嘴眼逐帧标定，禁止逐帧单独裁切导致漂移。当前模型CLI不支持透明背景参数，首轮为设定图；透明生产资产的具体流程需在样稿通过后选择，不能伪称纯色底即透明资产。
