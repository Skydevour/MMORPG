from pathlib import Path
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[3]
OUT = Path(__file__).resolve().parent
FONT = "C:/Windows/Fonts/msyh.ttc"
W, H = 1280, 720
camera_x, camera_y, view_h = 0.2, -0.6, 7.6
view_w = view_h * 16 / 9
left, top = camera_x - view_w / 2, camera_y + view_h / 2
scale = H / view_h


def point(x, y):
    return ((x - left) * scale, (top - y) * scale)


def draw_actor(canvas, path, foot, visible_height=None, pivot=None):
    sprite = Image.open(ROOT / path).convert("RGBA")
    box = sprite.getchannel("A").getbbox()
    factor = visible_height * scale / (box[3] - box[1]) if visible_height else scale / 128
    anchor = pivot if pivot else (sprite.width / 2, sprite.height)
    origin = point(*foot)
    resized = sprite.resize((round(sprite.width * factor), round(sprite.height * factor)), Image.Resampling.LANCZOS)
    canvas.alpha_composite(resized, (round(origin[0] - anchor[0] * factor), round(origin[1] - anchor[1] * factor)))


background = Image.open(ROOT / "Assets/Res/Level01_Garden/Background/garden_background_wide.png").convert("RGBA")
bw, bh = 16 * 1.04, 9 * 1.04
crop = ((left + bw / 2) / bw * background.width,
        (bh / 2 - top) / bh * background.height,
        (left + view_w + bw / 2) / bw * background.width,
        (bh / 2 - (top - view_h)) / bh * background.height)
base = background.crop(crop).resize((W, H), Image.Resampling.LANCZOS)
target = base.copy()
draw_actor(target, "Assets/Res/Bosses/Potato/Frames/idle/idle_01.png", (4.0, -2.62), visible_height=4.2)
soil_y = round(point(0, -bh / 2 + bh * 0.24)[1])
target.alpha_composite(base.crop((0, soil_y, W, H)), (0, soil_y))
draw_actor(target, "Assets/Res/Hero/Frames/idle/idle_01.png", (-4.7, -2.5), pivot=(99, 166))
draw = ImageDraw.Draw(target)
draw.rectangle((32, 24, 1248, 122), outline="#FFFFFF", width=2)
draw.text((42, 32), "HUD 安全带（布局标注，非最终 UI）", font=ImageFont.truetype(FONT, 23), fill="#202525", stroke_width=1, stroke_fill="white")
draw.line((0, point(0, -2.5)[1], W, point(0, -2.5)[1]), fill="#F3D36A", width=2)
target.convert("RGB").save(OUT / "target_composition.png")

sheet = Image.new("RGB", (1680, 630), "#edf1f2")
d = ImageDraw.Draw(sheet)
title_font, body_font = ImageFont.truetype(FONT, 28), ImageFont.truetype(FONT, 22)
current = Image.open(ROOT / "Builds/DEV-008/Validation/1280x720/battle.png").convert("RGB")
for x, picture, title, detail in [
    (24, current, "当前 DEV-008 运行截图", "视野高 9.0；Boss 图框高 3.2；主体较分散"),
    (856, target, "PM-008 构图草案（离线合成）", "视野高 7.6；Boss 可见高 4.2；双方站位内收")
]:
    d.text((x, 18), title, fill="#202525", font=title_font)
    sheet.paste(picture.resize((800, 450), Image.Resampling.LANCZOS), (x, 66))
    d.text((x, 532), detail, fill="#202525", font=body_font)
d.text((24, 585), "草案只验证占屏与构图；不是改动后的游戏截图，未验证跳跃、攻击、碰撞或最终 UI。", fill="#434b50", font=body_font)
sheet.save(OUT / "composition_comparison.png")
print("构图草案已生成；仅供设计评审。")
