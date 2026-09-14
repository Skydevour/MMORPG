using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif
namespace MMORPG.Game.UI
{
    public static class GardenUiRoot
    {
        public static Transform Create(string name, int order)
        {
            EnsureEvents();
            var obj = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = obj.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = Camera.main;
            canvas.planeDistance = 0.5f; canvas.sortingOrder = order;
            var scaler = obj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1280, 720); scaler.matchWidthOrHeight = 0.5f;
            return obj.transform;
        }
        public static void EnsureEvents()
        {
            var system = EventSystem.current;
            if (system == null) system = new GameObject("EventSystem").AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
            var module = system.GetComponent<InputSystemUIInputModule>();
            if (module == null && system.GetComponent<BaseInputModule>() == null)
            {
                module = system.gameObject.AddComponent<InputSystemUIInputModule>(); module.AssignDefaultActions();
                module.actionsAsset.bindingMask = UnityEngine.InputSystem.InputBinding.MaskByGroups("Keyboard&Mouse");
            }
            if (module != null) module.deselectOnBackgroundClick = false;
#else
            if (system.GetComponent<BaseInputModule>() == null) system.gameObject.AddComponent<StandaloneInputModule>();
#endif
        }
    }
}
