from pathlib import Path

from PIL import Image


PROJECT_ROOT = Path(__file__).resolve().parents[1]
ART_ROOT = PROJECT_ROOT / "Assets" / "Art" / "Generated"


def split_grid(source: Path, output_root: Path, rows: list[str], columns: int) -> None:
    image = Image.open(source).convert("RGBA")
    cell_width = image.width // columns
    cell_height = image.height // len(rows)

    output_root.mkdir(parents=True, exist_ok=True)

    for row_index, action in enumerate(rows):
        action_dir = output_root / action
        action_dir.mkdir(parents=True, exist_ok=True)

        for column in range(columns):
            frame = image.crop(
                (
                    column * cell_width,
                    row_index * cell_height,
                    (column + 1) * cell_width,
                    (row_index + 1) * cell_height,
                )
            )
            frame.save(action_dir / f"{action}_{column + 1:02d}.png")

    print(f"Split {source.name}: {image.width}x{image.height}, cell {cell_width}x{cell_height}")


def split_tileset(source: Path, output_root: Path) -> None:
    names = [
        "grass_top",
        "soil",
        "edge_left",
        "edge_right",
        "corner_top_left",
        "corner_top_right",
        "corner_bottom_left",
        "corner_bottom_right",
        "mound",
        "cracked_soil",
        "sprout",
        "weeds",
        "stones",
        "root_soil",
        "wood_fence",
        "flower_patch",
    ]

    image = Image.open(source).convert("RGBA")
    columns = 4
    rows = 4
    cell_width = image.width // columns
    cell_height = image.height // rows

    output_root.mkdir(parents=True, exist_ok=True)

    for index, name in enumerate(names):
        column = index % columns
        row = index // columns
        tile = image.crop(
            (
                column * cell_width,
                row * cell_height,
                (column + 1) * cell_width,
                (row + 1) * cell_height,
            )
        )
        tile.save(output_root / f"{index + 1:02d}_{name}.png")

    print(f"Split {source.name}: {image.width}x{image.height}, tile {cell_width}x{cell_height}")


def main() -> None:
    split_grid(
        ART_ROOT / "Hero" / "cup_hero_action_atlas.png",
        ART_ROOT / "Hero" / "Frames",
        ["idle", "run", "jump", "dash", "shoot"],
        9,
    )
    split_tileset(
        ART_ROOT / "Level01_Garden" / "garden_tileset.png",
        ART_ROOT / "Level01_Garden" / "Tiles",
    )


if __name__ == "__main__":
    main()
