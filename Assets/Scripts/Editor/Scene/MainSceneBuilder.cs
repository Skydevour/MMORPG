#if UNITY_EDITOR
using MMORPG.Game.Core;
using MMORPG.Game.Level;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MMORPG.EditorTools.SceneSetup
{
    public static class MainSceneBuilder
    {
        private const string MainScenePath = "Assets/Scenes/MainScene.unity";

        [MenuItem("MMORPG/Build Prototype MainScene")]
        public static void BuildPrototypeMainScene()
        {
            Scene scene = EditorSceneManager.OpenScene(MainScenePath);

            GameObject root = GameObject.Find("GameRoot");
            if (root == null)
            {
                root = new GameObject("GameRoot");
            }

            if (root.GetComponent<GameManager>() == null)
            {
                root.AddComponent<GameManager>();
            }

            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                mainCamera.orthographic = true;
                mainCamera.orthographicSize = LevelMapLoader.LevelHeight * 0.5f;
                mainCamera.transform.position = new Vector3(0f, 0f, -10f);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("Prototype MainScene is ready. Press Play to load the garden map and player.");
        }
    }
}
#endif
