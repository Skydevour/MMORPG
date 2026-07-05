from pathlib import Path

from PIL import Image


PROJECT_ROOT = Path(__file__).resolve().parents[1]
FRAMES_ROOT = PROJECT_ROOT / "Assets" / "Art" / "Generated" / "Hero" / "Frames"
MIN_VISIBLE_PIXELS = 2000


def visible_pixel_count(path: Path) -> int:
    image = Image.open(path).convert("RGBA")
    alpha = image.getchannel("A")
    return sum(1 for value in alpha.getdata() if value > 8)


def repair_action(action_dir: Path) -> list[str]:
    report: list[str] = []
    last_good: Path | None = None

    for frame_path in sorted(action_dir.glob("*.png")):
        visible_pixels = visible_pixel_count(frame_path)
        if visible_pixels >= MIN_VISIBLE_PIXELS:
            last_good = frame_path
            report.append(f"{frame_path.name}: ok ({visible_pixels})")
            continue

        if last_good is None:
            report.append(f"{frame_path.name}: blank ({visible_pixels}), no previous frame")
            continue

        Image.open(last_good).save(frame_path)
        repaired_pixels = visible_pixel_count(frame_path)
        report.append(
            f"{frame_path.name}: repaired from {last_good.name} "
            f"({visible_pixels} -> {repaired_pixels})"
        )

    return report


def main() -> None:
    for action_dir in sorted(path for path in FRAMES_ROOT.iterdir() if path.is_dir()):
        print(action_dir.name)
        for line in repair_action(action_dir):
            print(f"  {line}")


if __name__ == "__main__":
    main()
