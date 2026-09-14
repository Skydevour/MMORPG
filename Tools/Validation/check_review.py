"""检查播放器真实截图尺寸和非空像素，不替代人工视觉验收。"""
from pathlib import Path
import json
import argparse
from PIL import Image
import numpy as np

parser = argparse.ArgumentParser(description='检查指定开发包的三尺寸截图。')
parser.add_argument('--build', default='DEV-011')
args = parser.parse_args()
root = Path(__file__).resolve().parents[2] / 'Builds' / args.build / 'Review'
results = []
for folder in sorted(root.iterdir()):
    if not folder.is_dir():
        continue
    size = tuple(map(int, folder.name.split('x')))
    pictures = sorted(folder.glob('*.png'))
    if len(pictures) != 8:
        raise ValueError(f'{folder.name} 截图不足八张')
    for path in pictures:
        with Image.open(path) as image:
            if image.size != size:
                raise ValueError(f'{path.name} 尺寸不正确')
            pixels = np.asarray(image.convert('RGB'))
            ratio = float(np.mean(np.max(pixels, axis=2) > 16))
            if ratio < 0.4 or float(pixels.std()) < 12:
                raise ValueError(f'{folder.name}/{path.name} 疑似空白截图')
            results.append({'截图': f'{folder.name}/{path.name}', '有效像素比例': round(ratio, 4)})
if len(results) != 24:
    raise ValueError('三尺寸截图矩阵不完整')
(root / '截图检查.json').write_text(json.dumps(results, ensure_ascii=False, indent=2), encoding='utf-8')
print('三尺寸共24张播放器截图尺寸与非空像素检查通过。')
