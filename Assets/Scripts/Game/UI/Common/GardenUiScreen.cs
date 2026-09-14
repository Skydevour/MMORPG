using System;
using UnityEngine;
namespace MMORPG.Game.UI
{
    public sealed class GardenUiScreen : MonoBehaviour
    {
        [Serializable] public struct Entry { public string id; public Component value; }
        public Entry[] bindings;
        public CanvasGroup group;
        private void Awake()
        {
            if (!Application.isPlaying) return;
            foreach (var button in GetComponentsInChildren<UnityEngine.UI.Button>(true))
                button.onClick.AddListener(() => MMORPG.Game.Audio.BattleAudio.Play("ui_confirm"));
        }
        public T Get<T>(string id) where T : Component
        {
            foreach (var entry in bindings) if (entry.id == id) return entry.value as T;
            throw new InvalidOperationException($"UI {name} 缺少序列化绑定 {id}。");
        }
        public static GardenUiScreen Spawn(GardenUiScreen prefab, Transform parent)
        {
            if (prefab == null) throw new InvalidOperationException("UI Prefab 引用缺失，请重新准备 UI 资源。");
            var view = Instantiate(prefab, parent, false); view.name = prefab.name; return view;
        }
        public void SetInteractive(bool enabled) { group.interactable = enabled; group.blocksRaycasts = enabled; }
    }
}
