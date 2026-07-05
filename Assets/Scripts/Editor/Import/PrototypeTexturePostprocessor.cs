#if UNITY_EDITOR
using UnityEditor;

namespace MMORPG.EditorTools.Import
{
    public sealed class PrototypeTexturePostprocessor : AssetPostprocessor
    {
        private static readonly UnityEngine.Vector2 HeroFramePivot = new UnityEngine.Vector2(99f / 307f, 1f / 167f);
        private static readonly UnityEngine.Vector2 PotatoBossFramePivot = new UnityEngine.Vector2(0.5f, 1f / 255f);

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith("Assets/Res/"))
            {
                return;
            }

            TextureImporter importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 128f;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = UnityEngine.FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 4096;

            if (assetPath.Contains("/Hero/Frames/"))
            {
                importer.spritePivot = HeroFramePivot;
            }
            else if (assetPath.Contains("/Bosses/Potato/Frames/"))
            {
                importer.spritePivot = PotatoBossFramePivot;
            }
        }
    }
}
#endif
