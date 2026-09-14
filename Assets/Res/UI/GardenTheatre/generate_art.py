"""Original deterministic UI raster artwork, drawn with Pillow. No AI image model or third-party artwork."""
from pathlib import Path
from PIL import Image, ImageDraw
import math

ROOT = Path(__file__).resolve().parent
INK = '#202525'
PAPER = '#F5F4EA'
GREEN = '#34785B'
RED = '#BA443E'
PINK = '#E96BAD'
YELLOW = '#F3D36A'

def save(image, group, name):
    folder = ROOT / group / 'Textures'
    folder.mkdir(parents=True, exist_ok=True)
    image.save(folder / (name + '.png'))

def ticket(w, h, torn=False):
    im = Image.new('RGBA', (w, h))
    d = ImageDraw.Draw(im)
    if torn:
        points = [(12 + (i % 3)*3, i) for i in range(12, h-12, 16)]
        points += [(i, h-12-(i % 5)) for i in range(12, w-12, 16)]
        points += [(w-12-(i % 3)*3, i) for i in range(h-12, 12, -16)]
        points += [(i, 12+(i % 5)) for i in range(w-12, 12, -16)]
        d.polygon(points, fill=PAPER, outline=INK)
        d.line(points+[points[0]], fill=INK, width=5)
    else:
        d.rectangle((4, 4, w-5, h-5), fill=PAPER, outline=INK, width=5)
        for x in (0,w):
            d.ellipse((x-14,h//2-14,x+14,h//2+14), fill=(0,0,0,0), outline=INK,width=4)
    d.rectangle((24,24,w-25,h-25), outline=GREEN,width=3)
    for x in range(38,w-30,18):
        d.line((x,15,x+6,15), fill=RED,width=2)
    return im

save(ticket(720,128),'Common','ticket')
save(ticket(960,992),'Pause','programme')
save(ticket(1440,780,True),'Defeat','torn_ticket')
for full in (True,False):
    im=ticket(88,128); d=ImageDraw.Draw(im)
    color=RED if full else '#989D99'
    d.ellipse((21,37,45,62),fill=color); d.ellipse((43,37,67,62),fill=color)
    d.polygon([(21,51),(67,51),(44,85)],fill=color)
    if not full: d.line((21,94,67,32),fill=INK,width=5)
    save(im,'HUD','health_full' if full else 'health_empty')
    im=ticket(72,96); d=ImageDraw.Draw(im)
    d.polygon([(36,22),(54,48),(36,74),(18,48)],fill=PINK if full else PAPER,outline=INK,width=3)
    if full: d.line((36,31,36,63),fill=PAPER,width=3)
    save(im,'HUD','energy_full' if full else 'energy_empty')
im=Image.new('RGBA',(128,128));d=ImageDraw.Draw(im)
pts=[]
for i in range(48):
    a=i*math.pi/24; r=59 if i%2==0 else 54
    pts.append((64+math.cos(a)*r,64+math.sin(a)*r))
d.polygon(pts,fill=YELLOW,outline=INK,width=4);d.ellipse((17,17,111,111),outline=GREEN,width=4)
pending = im.copy()
pd = ImageDraw.Draw(pending)
pd.ellipse((17,17,111,111), fill=PAPER, outline=INK, width=4)
save(pending, 'Victory', 'stamp_pending')
d.line((36,65,55,83,91,43),fill=GREEN,width=9)
save(im,'Victory','stamp')
im=Image.new('RGBA',(400,240));d=ImageDraw.Draw(im)
for flip in (-1,1):
    d.arc((50,10,350,230),10 if flip==1 else 170,170 if flip==1 else 350,fill=GREEN,width=8)
    for i in range(6):
        x=60+i*48;y=160-abs(i-2.5)*27
        d.ellipse((x,y-20,x+30,y+4),fill=GREEN,outline=INK,width=2)
save(im,'Common','sprig')
for actor in ('Potato', 'Onion', 'Carrot'):
    source = ROOT.parents[1] / 'Bosses' / actor / 'Frames' / 'idle'
    candidates = sorted(p for p in source.glob('*.png') if '_tween' not in p.name)
    portrait = Image.open(candidates[0]).convert('RGBA')
    save(portrait.crop(portrait.getchannel('A').getbbox()), 'Title', actor.lower() + '_portrait')
print('原创 UI 票券、生命/能量卡与印章已生成。')
