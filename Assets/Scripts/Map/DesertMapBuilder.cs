using System;
using UnityEngine;

[ExecuteAlways]
public sealed class DesertMapBuilder : MonoBehaviour
{
    [Header("Map")]
    [SerializeField] private int gridSize = 28;
    [SerializeField] private float tileSize = 8f;
    [SerializeField] private float worldScale = 4f;
    [SerializeField] private float wallHeight = 5f;
    [SerializeField] private float wallThickness = 2.2f;

    [Header("Materials")]
    [SerializeField] private Material sandMaterial;
    [SerializeField] private Material stoneMaterial;
    [SerializeField] private Material earthMaterial;
    [SerializeField] private Material woodMaterial;
    [SerializeField] private Material clothMaterial;
    [SerializeField] private Material cityMaterial;
    [SerializeField] private Material cliffMaterial;
    [SerializeField] private Material neonPrimaryMaterial;
    [SerializeField] private Material neonAccentMaterial;

    [Header("Generation")]
    [SerializeField] private bool rebuildOnEnable = true;

    private const string GeneratedRootName = "Generated";

    private void OnEnable()
    {
        if (rebuildOnEnable)
        {
            Rebuild();
        }
    }

    [ContextMenu("Rebuild Desert Map")]
    public void Rebuild()
    {
        ClearGenerated();

        Transform generated = new GameObject(GeneratedRootName).transform;
        generated.SetParent(transform, false);
        generated.localScale = Vector3.one * worldScale;

        BuildSandField(generated);
        BuildOuterWalls(generated);
        BuildFortress(generated);
        BuildMounds(generated);
        BuildRockFields(generated);
        BuildCliffs(generated);
        BuildCityDistrict(generated);
        BuildNeonInfrastructure(generated);
        BuildCamp(generated);
        BuildRouteMarkers(generated);
    }

    private void ClearGenerated()
    {
        Transform child = transform.Find(GeneratedRootName);
        if (child == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(child.gameObject);
        }
        else
        {
            DestroyImmediate(child.gameObject);
        }
    }

    private void BuildSandField(Transform parent)
    {
        Transform tilesRoot = CreateGroup(parent, "Sand_Tile_Field");
        float half = gridSize * tileSize * 0.5f;

        for (int x = 0; x < gridSize; x++)
        {
            for (int z = 0; z < gridSize; z++)
            {
                float px = x * tileSize - half + tileSize * 0.5f;
                float pz = z * tileSize - half + tileSize * 0.5f;
                float height = SandHeight(px, pz) * 0.45f;

                GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tile.name = $"Sand_Tile_{x:00}_{z:00}";
                tile.transform.SetParent(tilesRoot, false);
                tile.transform.localPosition = new Vector3(px, height - 0.15f, pz);
                tile.transform.localScale = new Vector3(tileSize, 0.3f, tileSize);
                ApplyMaterial(tile, sandMaterial);
            }
        }
    }

    private void BuildOuterWalls(Transform parent)
    {
        Transform wallsRoot = CreateGroup(parent, "Outer_City_Walls");
        float width = gridSize * tileSize;
        float offset = width * 0.5f + wallThickness * 0.5f;

        CreateBlock(wallsRoot, "North_Wall", new Vector3(0, wallHeight * 0.5f, offset), new Vector3(width + wallThickness * 2f, wallHeight, wallThickness), stoneMaterial);
        CreateBlock(wallsRoot, "South_Wall", new Vector3(0, wallHeight * 0.5f, -offset), new Vector3(width + wallThickness * 2f, wallHeight, wallThickness), stoneMaterial);
        CreateBlock(wallsRoot, "East_Wall", new Vector3(offset, wallHeight * 0.5f, 0), new Vector3(wallThickness, wallHeight, width), stoneMaterial);
        CreateBlock(wallsRoot, "West_Wall", new Vector3(-offset, wallHeight * 0.5f, 0), new Vector3(wallThickness, wallHeight, width), stoneMaterial);

        float towerY = wallHeight * 0.65f;
        Vector3 towerScale = new Vector3(7f, wallHeight * 1.3f, 7f);
        CreateBlock(wallsRoot, "Tower_NE", new Vector3(offset, towerY, offset), towerScale, stoneMaterial);
        CreateBlock(wallsRoot, "Tower_NW", new Vector3(-offset, towerY, offset), towerScale, stoneMaterial);
        CreateBlock(wallsRoot, "Tower_SE", new Vector3(offset, towerY, -offset), towerScale, stoneMaterial);
        CreateBlock(wallsRoot, "Tower_SW", new Vector3(-offset, towerY, -offset), towerScale, stoneMaterial);

        CreateGateOpening(wallsRoot, "South_Gate_Arch", new Vector3(0, 3.2f, -offset - 0.35f));
    }

    private void BuildFortress(Transform parent)
    {
        Transform fortress = CreateGroup(parent, "Central_Fortress");
        CreateBlock(fortress, "Fort_Base", new Vector3(0, 1.4f, 18f), new Vector3(28f, 2.8f, 24f), stoneMaterial);
        CreateBlock(fortress, "Fort_Keep", new Vector3(0, 7f, 18f), new Vector3(14f, 11f, 12f), stoneMaterial);
        CreateBlock(fortress, "Fort_Left_Wing", new Vector3(-17f, 3f, 18f), new Vector3(8f, 6f, 18f), stoneMaterial);
        CreateBlock(fortress, "Fort_Right_Wing", new Vector3(17f, 3f, 18f), new Vector3(8f, 6f, 18f), stoneMaterial);
        CreateBlock(fortress, "Fort_Ramp", new Vector3(0, 0.45f, 2f), new Vector3(18f, 0.9f, 18f), earthMaterial);
    }

    private void BuildMounds(Transform parent)
    {
        Transform mounds = CreateGroup(parent, "Sand_Mounds");
        Vector3[] positions =
        {
            new(-52f, 1.2f, -28f),
            new(46f, 1.5f, -44f),
            new(-34f, 1.0f, 46f),
            new(62f, 1.8f, 36f),
            new(8f, 1.1f, -58f),
            new(-78f, 1.4f, 8f),
            new(88f, 1.6f, -8f),
            new(-92f, 1.9f, -72f),
            new(74f, 1.1f, 84f),
            new(-12f, 1.7f, 92f),
            new(104f, 2.2f, 10f),
            new(-108f, 1.6f, 38f)
        };

        for (int i = 0; i < positions.Length; i++)
        {
            GameObject mound = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            mound.name = $"Sand_Mound_{i + 1:00}";
            mound.transform.SetParent(mounds, false);
            mound.transform.localPosition = positions[i];
            float scale = 8f + i * 1.5f;
            mound.transform.localScale = new Vector3(scale, 2.4f + i * 0.18f, scale * 0.72f);
            mound.transform.localRotation = Quaternion.Euler(0f, i * 37f, 0f);
            ApplyMaterial(mound, earthMaterial);
        }
    }

    private void BuildRockFields(Transform parent)
    {
        Transform rocks = CreateGroup(parent, "Rock_Fields");
        for (int i = 0; i < 34; i++)
        {
            float angle = i * 31.7f;
            float radius = 38f + (i % 7) * 12f;
            Vector3 pos = Quaternion.Euler(0f, angle, 0f) * new Vector3(radius, 0f, 0f);
            pos += new Vector3(Mathf.Sin(i * 1.9f) * 14f, 1.2f + (i % 5) * 0.35f, Mathf.Cos(i * 1.3f) * 16f);

            GameObject rock = GameObject.CreatePrimitive(i % 3 == 0 ? PrimitiveType.Capsule : PrimitiveType.Sphere);
            rock.name = $"Large_Rock_{i + 1:00}";
            rock.transform.SetParent(rocks, false);
            rock.transform.localPosition = pos;
            rock.transform.localRotation = Quaternion.Euler(i * 11f, angle, i * 17f);
            float width = 4f + (i % 6) * 1.4f;
            rock.transform.localScale = new Vector3(width, 3f + (i % 4), width * (0.65f + (i % 3) * 0.18f));
            ApplyMaterial(rock, cliffMaterial != null ? cliffMaterial : earthMaterial);
        }
    }

    private void BuildCliffs(Transform parent)
    {
        Transform cliffs = CreateGroup(parent, "Cliff_Borders");
        float half = gridSize * tileSize * 0.5f;
        float cliffOffset = half + 24f;

        for (int i = -7; i <= 7; i++)
        {
            float segment = i * 18f;
            float height = 9f + Mathf.Abs(i % 4) * 1.6f;
            CreateBlock(cliffs, $"North_Cliff_{i + 7:00}", new Vector3(segment, height * 0.5f - 2f, cliffOffset + Mathf.Sin(i) * 5f), new Vector3(18f, height, 8f), cliffMaterial);
            CreateBlock(cliffs, $"West_Cliff_{i + 7:00}", new Vector3(-cliffOffset + Mathf.Cos(i) * 4f, height * 0.5f - 2f, segment), new Vector3(8f, height, 18f), cliffMaterial);
        }

        CreateBlock(cliffs, "South_Escarpment_Long", new Vector3(34f, 4.5f, -cliffOffset - 6f), new Vector3(96f, 13f, 10f), cliffMaterial);
        CreateBlock(cliffs, "East_Escarpment_Long", new Vector3(cliffOffset + 5f, 5.5f, -28f), new Vector3(10f, 15f, 104f), cliffMaterial);
    }

    private void BuildCityDistrict(Transform parent)
    {
        Transform city = CreateGroup(parent, "Cyber_City_District");
        city.localPosition = new Vector3(96f, 0f, 86f);

        int index = 0;
        const int cityBlocks = 5;
        const float buildingSpacing = 32f;
        for (int x = 0; x < cityBlocks; x++)
        {
            for (int z = 0; z < cityBlocks; z++)
            {
                if ((x == 2 && z == 2) || (x == 1 && z == 3))
                {
                    continue;
                }

                float height = 32f + ((x * 5 + z * 7) % 11) * 5.2f;
                Vector3 pos = new Vector3((x - 2f) * buildingSpacing, height * 0.5f, (z - 2f) * buildingSpacing);
                Vector3 scale = new Vector3(9f + (z % 2) * 3f, height, 9f + (x % 2) * 2.5f);
                GameObject tower = CreateBlock(city, $"High_Rise_{++index:00}", pos, scale, cityMaterial);

                CreateBuildingNeonBands(tower.transform, height, scale);

                if ((x + z) % 3 == 0)
                {
                    CreateBlock(tower.transform, "Roof_Antenna", new Vector3(0f, height * 0.5f + 3.5f, 0f), new Vector3(1.2f, 7f, 1.2f), neonAccentMaterial);
                }
            }
        }

        CreateBlock(city, "Central_Neon_Plaza", new Vector3(0f, 0.25f, 0f), new Vector3(44f, 0.5f, 44f), neonPrimaryMaterial);
        CreateBlock(city, "East_Wide_Avenue", new Vector3(72f, 0.12f, 0f), new Vector3(12f, 0.24f, 138f), stoneMaterial);
        CreateBlock(city, "South_Wide_Avenue", new Vector3(0f, 0.12f, -72f), new Vector3(138f, 0.24f, 12f), stoneMaterial);
        CreateBlock(city, "Collapsed_Plaza_Block", new Vector3(-58f, 2f, 12f), new Vector3(28f, 4f, 24f), cityMaterial);
        CreateBlock(city, "Avenue_Rubble_Line", new Vector3(0f, 0.7f, -104f), new Vector3(126f, 1.4f, 6f), cliffMaterial);
    }

    private void BuildNeonInfrastructure(Transform parent)
    {
        Transform tech = CreateGroup(parent, "Cyberpunk_Infrastructure");

        Vector3[] pylons =
        {
            new(-118f, 0f, 92f),
            new(132f, 0f, -96f),
            new(-138f, 0f, -122f),
            new(116f, 0f, 128f),
            new(0f, 0f, -142f),
            new(-34f, 0f, 132f)
        };

        for (int i = 0; i < pylons.Length; i++)
        {
            float height = 18f + (i % 3) * 6f;
            Transform pylon = CreateGroup(tech, $"Energy_Pylon_{i + 1:00}");
            pylon.localPosition = pylons[i];
            CreateBlock(pylon, "Dark_Core", new Vector3(0f, height * 0.5f, 0f), new Vector3(3f, height, 3f), cityMaterial);
            CreateBlock(pylon, "Neon_Cap", new Vector3(0f, height + 1.2f, 0f), new Vector3(8f, 1.2f, 8f), neonPrimaryMaterial);
            CreateBlock(pylon, "Vertical_Light", new Vector3(0f, height * 0.5f, -1.65f), new Vector3(0.35f, height * 0.9f, 0.18f), neonAccentMaterial);
        }

        for (int i = -9; i <= 9; i++)
        {
            float x = i * 18f;
            CreateBlock(tech, $"Neon_Road_Strip_North_{i + 9:00}", new Vector3(x, 0.12f, 42f), new Vector3(9f, 0.16f, 0.8f), neonPrimaryMaterial);
            CreateBlock(tech, $"Neon_Road_Strip_South_{i + 9:00}", new Vector3(x, 0.12f, -42f), new Vector3(9f, 0.16f, 0.8f), neonAccentMaterial);
        }
    }

    private void BuildCamp(Transform parent)
    {
        Transform camp = CreateGroup(parent, "Raider_Camp");
        camp.localPosition = new Vector3(-48f, 0f, -54f);

        for (int i = 0; i < 5; i++)
        {
            float angle = i * 72f;
            Vector3 pos = Quaternion.Euler(0f, angle, 0f) * new Vector3(12f, 0f, 0f);
            CreateTent(camp, $"Tent_{i + 1:00}", pos, Quaternion.Euler(0f, -angle + 25f, 0f));
        }

        CreateBlock(camp, "Supply_Crate_A", new Vector3(-5f, 0.8f, 3f), new Vector3(2.5f, 1.6f, 2.5f), woodMaterial);
        CreateBlock(camp, "Supply_Crate_B", new Vector3(-1.8f, 0.65f, 5.5f), new Vector3(2f, 1.3f, 2f), woodMaterial);
        CreateBlock(camp, "Watch_Post", new Vector3(17f, 3f, 4f), new Vector3(4f, 6f, 4f), woodMaterial);
    }

    private void BuildRouteMarkers(Transform parent)
    {
        Transform route = CreateGroup(parent, "Battle_Routes");
        for (int i = -8; i <= 8; i++)
        {
            CreateBlock(route, $"Buried_Road_Stone_{i + 8:00}", new Vector3(i * 7f, 0.08f, -8f + Mathf.Sin(i * 0.6f) * 2f), new Vector3(4.8f, 0.16f, 3f), stoneMaterial);
        }
    }

    private void CreateTent(Transform parent, string name, Vector3 position, Quaternion rotation)
    {
        Transform tent = CreateGroup(parent, name);
        tent.localPosition = position;
        tent.localRotation = rotation;

        CreateBlock(tent, "Tent_Floor", new Vector3(0f, 0.15f, 0f), new Vector3(7f, 0.3f, 5f), woodMaterial);

        GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
        roof.name = "Tent_Cloth_Roof";
        roof.transform.SetParent(tent, false);
        roof.transform.localPosition = new Vector3(0f, 2.2f, 0f);
        roof.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
        roof.transform.localScale = new Vector3(5f, 0.35f, 5.6f);
        ApplyMaterial(roof, clothMaterial);

        CreateBlock(tent, "Tent_Pole_Left", new Vector3(-2.4f, 1.2f, 0f), new Vector3(0.35f, 2.4f, 0.35f), woodMaterial);
        CreateBlock(tent, "Tent_Pole_Right", new Vector3(2.4f, 1.2f, 0f), new Vector3(0.35f, 2.4f, 0.35f), woodMaterial);
    }

    private void CreateBuildingNeonBands(Transform tower, float height, Vector3 buildingScale)
    {
        int floorCount = Mathf.FloorToInt(height / 4f);
        float frontZ = -0.5f - 0.015f;
        float rightX = 0.5f + 0.015f;
        float bandHeight = Mathf.Max(0.006f, 0.18f / height);
        float bandDepth = Mathf.Max(0.012f, 0.12f / buildingScale.z);
        float sideDepth = Mathf.Max(0.012f, 0.12f / buildingScale.x);

        for (int floor = 2; floor < floorCount; floor += 2)
        {
            float y = -0.5f + floor * 4f / height;
            Material bandMaterial = floor % 4 == 0 ? neonPrimaryMaterial : neonAccentMaterial;
            CreateBlock(tower, $"Window_Band_Front_{floor:00}", new Vector3(0f, y, frontZ), new Vector3(0.82f, bandHeight, bandDepth), bandMaterial);
            CreateBlock(tower, $"Window_Band_Right_{floor:00}", new Vector3(rightX, y + bandHeight * 2f, 0f), new Vector3(sideDepth, bandHeight, 0.72f), bandMaterial);
        }
    }

    private void CreateGateOpening(Transform parent, string name, Vector3 position)
    {
        Transform gate = CreateGroup(parent, name);
        gate.localPosition = position;
        CreateBlock(gate, "Left_Pillar", new Vector3(-6f, 0f, 0f), new Vector3(3f, 6.4f, 3f), stoneMaterial);
        CreateBlock(gate, "Right_Pillar", new Vector3(6f, 0f, 0f), new Vector3(3f, 6.4f, 3f), stoneMaterial);
        CreateBlock(gate, "Top_Bridge", new Vector3(0f, 4.8f, 0f), new Vector3(15f, 2.4f, 3f), stoneMaterial);
    }

    private static Transform CreateGroup(Transform parent, string name)
    {
        GameObject group = new(name);
        group.transform.SetParent(parent, false);
        return group.transform;
    }

    private static GameObject CreateBlock(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
        block.name = name;
        block.transform.SetParent(parent, false);
        block.transform.localPosition = position;
        block.transform.localScale = scale;
        ApplyMaterial(block, material);
        return block;
    }

    private static void ApplyMaterial(GameObject obj, Material material)
    {
        if (material == null)
        {
            return;
        }

        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = material;
        }
    }

    private static float SandHeight(float x, float z)
    {
        float broad = Mathf.Sin(x * 0.035f) * Mathf.Cos(z * 0.03f);
        float ripple = Mathf.Sin((x + z) * 0.12f) * 0.22f;
        return broad + ripple;
    }

    private void OnValidate()
    {
        gridSize = Mathf.Clamp(gridSize, 8, 64);
        tileSize = Mathf.Max(2f, tileSize);
        wallHeight = Mathf.Max(2f, wallHeight);
        wallThickness = Mathf.Max(0.5f, wallThickness);
        worldScale = Mathf.Clamp(worldScale, 1f, 40f);
    }
}
