using System;
using System.Collections.Generic;
using UnityEngine;

namespace MMORPG.Game.Core
{
    [CreateAssetMenu(fileName = "PrototypeSpriteCatalog", menuName = "MMORPG/Prototype Sprite Catalog")]
    public sealed class PrototypeSpriteCatalog : ScriptableObject
    {
        public Material flashMaterial;
        public Material particleMaterial;
        public PhysicsMaterial2D playerPhysicsMaterial;
        public GameObject battleParticlePrefab;
        [Serializable]
        private sealed class FolderEntry
        {
            [SerializeField] private string folderPath;
            [SerializeField] private Sprite[] sprites;

            public string FolderPath => folderPath;
            public Sprite[] Sprites => sprites ?? Array.Empty<Sprite>();

            public FolderEntry(string path, Sprite[] values)
            {
                folderPath = path;
                sprites = values ?? Array.Empty<Sprite>();
            }
        }

        [Serializable]
        private sealed class FileEntry
        {
            [SerializeField] private string assetPath;
            [SerializeField] private Sprite sprite;

            public string AssetPath => assetPath;
            public Sprite Sprite => sprite;

            public FileEntry(string path, Sprite value)
            {
                assetPath = path;
                sprite = value;
            }
        }

        [SerializeField] private FolderEntry[] folders = Array.Empty<FolderEntry>();
        [SerializeField] private FileEntry[] files = Array.Empty<FileEntry>();

        [NonSerialized] private Dictionary<string, Sprite[]> folderLookup;
        [NonSerialized] private Dictionary<string, Sprite> fileLookup;

        public bool TryGetSpritesInFolder(string folderPath, out Sprite[] sprites)
        {
            EnsureLookup();
            if (folderLookup.TryGetValue(NormalizePath(folderPath), out sprites))
            {
                return sprites != null && sprites.Length > 0;
            }

            sprites = Array.Empty<Sprite>();
            return false;
        }

        public bool TryGetSprite(string assetPath, out Sprite sprite)
        {
            EnsureLookup();
            if (fileLookup.TryGetValue(NormalizePath(assetPath), out sprite))
            {
                return sprite != null;
            }

            sprite = null;
            return false;
        }

        private void OnEnable()
        {
            folderLookup = null;
            fileLookup = null;
        }

        private void EnsureLookup()
        {
            if (folderLookup != null && fileLookup != null)
            {
                return;
            }

            folderLookup = new Dictionary<string, Sprite[]>(StringComparer.OrdinalIgnoreCase);
            fileLookup = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);

            if (folders != null)
            {
                foreach (FolderEntry entry in folders)
                {
                    if (entry == null || string.IsNullOrWhiteSpace(entry.FolderPath))
                    {
                        continue;
                    }

                    folderLookup[NormalizePath(entry.FolderPath)] = entry.Sprites;
                }
            }

            if (files != null)
            {
                foreach (FileEntry entry in files)
                {
                    if (entry == null || string.IsNullOrWhiteSpace(entry.AssetPath) || entry.Sprite == null)
                    {
                        continue;
                    }

                    fileLookup[NormalizePath(entry.AssetPath)] = entry.Sprite;
                }
            }
        }

#if UNITY_EDITOR
        public void SetEditorEntries(
            string[] folderPaths,
            Sprite[][] folderSprites,
            string[] filePaths,
            Sprite[] fileSprites)
        {
            int folderCount = folderPaths == null ? 0 : folderPaths.Length;
            folders = new FolderEntry[folderCount];
            for (int index = 0; index < folderCount; index++)
            {
                Sprite[] sprites = folderSprites != null && index < folderSprites.Length
                    ? folderSprites[index]
                    : Array.Empty<Sprite>();
                folders[index] = new FolderEntry(folderPaths[index], sprites);
            }

            int fileCount = filePaths == null ? 0 : filePaths.Length;
            files = new FileEntry[fileCount];
            for (int index = 0; index < fileCount; index++)
            {
                Sprite sprite = fileSprites != null && index < fileSprites.Length
                    ? fileSprites[index]
                    : null;
                files[index] = new FileEntry(filePaths[index], sprite);
            }

            folderLookup = null;
            fileLookup = null;
        }
#endif

        private static string NormalizePath(string path)
        {
            return string.IsNullOrEmpty(path) ? string.Empty : path.Replace('\\', '/');
        }
    }
}
