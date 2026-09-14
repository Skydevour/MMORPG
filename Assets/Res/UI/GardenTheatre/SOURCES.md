# Garden Theatre UI source register

本轮补充：标题群像使用原有开发角色待机图离线裁透明边生成独立静态肖像，未修改角色动作帧。未完成印章使用无勾选空心章，避免误报阶段完成。字体及正式原画门禁保持不变。

- Date: 2026-09-14, DEV-009 C/D.
- Ticket, programme, torn ticket, health cards, energy cards, stamp and sprig: original deterministic vector-like raster drawings authored in `generate_art.py` and rendered locally with Pillow. No AI image generator, external stock art or reference-game images used.
- Existing Potato/Onion/Carrot sprites are reused via serialized references for the title cast only. Their original project provenance and development-art restrictions remain unchanged.
- Font: Unity built-in LegacyRuntime.ttf is retained as a technical placeholder. A project-distributed Chinese font with recorded redistribution authorization and complete glyph verification is still required.
- Title and victory/defeat lettering: live text, not delivered original lettering art. Formal title/cast/lettering artwork remains an art acceptance gap.
- This delivery does not clear `developmentArt`. These UI assets establish an interactive implementation, not a claim that all formal art has passed acceptance.
- Source sizes: ticket 720x128, programme 960x992, defeat ticket 1440x780, health 88x128, energy 72x96, stamp 128x128, sprig 400x240. UI import is Sprite, sRGB, alpha transparency, no mipmaps. Ticket/programme borders are set by GardenUiPreparation.
