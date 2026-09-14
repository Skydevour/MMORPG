# 音频来源与状态

DEV-008 新增 `Arranged`：由 `Tools/Audio/compose_garden_audio.py` 确定性合成的原创十六小节摇摆编曲，三阶段分别使用簧管、键盘、铜管主声部，配合低音、和声及鼓组，48kHz 立体声。短音按射击、反制、受伤、液体、念力和 UI 分开合成。目录工具优先绑定新版，旧文件保留。波形峰值/RMS/削波报告在 `Plan/Validation/DEV-008/audio_metrics.json`；这不是听感或 LUFS 验收结论。

DEV-007 音效和音乐由本项目 `GardenAssetPreparation` 使用确定性波形合成离线生成，无商业游戏录音、第三方素材或外部许可依赖。

短音为 48kHz 单声道 WAV，运行时目录为 `Config/BattleAudioCatalog`，原文件位于 `Assets/Res/Audio`。当前合成音乐为开发配乐，正式混音与听感待验收；尚不等于 DESIGN-005 要求的最终编曲。
