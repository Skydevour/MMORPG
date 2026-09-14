using System.Collections;
using MMORPG.Game.Audio;
using MMORPG.Game.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
namespace MMORPG.Game.UI
{
    public sealed class BattleTitleView : MonoBehaviour
    {
        private static BattleTitleView active;
        public static bool IsOpen => active != null;
        private GameManager manager;
        private GardenUiScreen title, settings;
        private bool settingsOnly, closing, starting;
        private Selectable returnFocus;
        private CanvasGroup underneath;
        private bool underneathInteractive;
        private float underneathAlpha;
        public static void Create(GameManager manager, bool settingsOnly = false)
        {
            if (active != null) return;
            var previous = EventSystem.current == null ? null : EventSystem.current.currentSelectedGameObject;
            var root = GardenUiRoot.Create("GardenTitle", 200);
            var view = root.gameObject.AddComponent<BattleTitleView>(); active = view;
            view.manager = manager; view.settingsOnly = settingsOnly;
            view.returnFocus = previous == null ? null : previous.GetComponent<Selectable>();
            var art = GardenUiCatalog.Load();
            if (!settingsOnly)
            {
                view.SetWorldActorsVisible(false);
                view.title = GardenUiScreen.Spawn(art.title, root);
                view.title.Get<Button>("start").onClick.AddListener(view.StartChallenge);
                view.title.Get<Button>("settings").onClick.AddListener(view.OpenSettings);
                view.title.Get<Button>("quit").onClick.AddListener(Application.Quit);
                view.title.group.alpha = 0f;
                view.title.gameObject.AddComponent<GardenUiMotion>().Fade(view.title.group, 1f, GardenUiCatalog.Rules.titleEnter);
                view.title.Get<Button>("start").Select();
            }
            view.settings = GardenUiScreen.Spawn(art.settings, root);
            view.settings.Get<Slider>("master").SetValueWithoutNotify(BattleAudio.Master);
            view.settings.Get<Slider>("music").SetValueWithoutNotify(BattleAudio.Music);
            view.settings.Get<Slider>("effects").SetValueWithoutNotify(BattleAudio.Effects);
            view.settings.Get<Slider>("master").onValueChanged.AddListener(v => BattleAudio.SetVolumes(v, BattleAudio.Music, BattleAudio.Effects));
            view.settings.Get<Slider>("music").onValueChanged.AddListener(v => BattleAudio.SetVolumes(BattleAudio.Master, v, BattleAudio.Effects));
            view.settings.Get<Slider>("effects").onValueChanged.AddListener(v => BattleAudio.SetVolumes(BattleAudio.Master, BattleAudio.Music, v));
            view.settings.Get<Button>("motion").onClick.AddListener(view.ChangeMotion);
            view.settings.Get<Button>("back").onClick.AddListener(view.CloseSettings);
            view.RefreshMotion(); view.settings.gameObject.SetActive(false);
            if (settingsOnly) view.OpenSettings();
        }
        private void StartChallenge()
        {
            if (starting || closing) return;
            starting = true; title.SetInteractive(false);
            BattleAudio.Play("ui_start");
            title.GetComponent<GardenUiMotion>().Fade(title.group, 0f, GardenUiCatalog.Rules.titleExit, () =>
            {
                manager.Encounter.Begin(); Destroy(gameObject);
            });
        }
        private void OpenSettings()
        {
            if (closing || starting || settings.gameObject.activeSelf) return;
            if (!settingsOnly)
            {
                var current = EventSystem.current.currentSelectedGameObject;
                returnFocus = current != null ? current.GetComponent<Selectable>() : title.Get<Button>("settings");
            }
            underneath = settingsOnly ? manager.BattleHud.PauseGroup : title.group;
            underneath.GetComponent<GardenUiMotion>()?.Cancel();
            underneathInteractive = underneath.interactable; underneathAlpha = 1f;
            underneath.interactable = false; underneath.alpha = 0f;
            settings.gameObject.SetActive(true); settings.group.alpha = 0f; settings.SetInteractive(true);
            var motion = settings.GetComponent<GardenUiMotion>() ?? settings.gameObject.AddComponent<GardenUiMotion>();
            motion.Fade(settings.group, 1f, GardenUiCatalog.Rules.menuEnter);
            settings.Get<Slider>("master").Select();
        }
        private void CloseSettings()
        {
            if (closing || !settings.gameObject.activeSelf) return;
            closing = true; settings.SetInteractive(false); BattleAudio.Play("ui_cancel");
            settings.GetComponent<GardenUiMotion>().Fade(settings.group, 0f, GardenUiCatalog.Rules.menuExit, () =>
            {
                RestoreFocus(); settings.gameObject.SetActive(false);
                if (settingsOnly) Destroy(gameObject); else closing = false;
            });
        }
        private void RestoreFocus()
        {
            if (underneath != null) { underneath.interactable = underneathInteractive; underneath.alpha = underneathAlpha; }
            if (returnFocus != null && returnFocus.gameObject.activeInHierarchy && returnFocus.IsInteractable()) returnFocus.Select();
            else if (settingsOnly && manager != null) manager.BattleHud.FocusResume();
            else if (title != null) title.Get<Button>("settings").Select();
        }
        private void ChangeMotion()
        {
            PlayerPrefs.SetInt("Camera.Shake", (PlayerPrefs.GetInt("Camera.Shake", 0) + 1) % 3);
            PlayerPrefs.Save(); RefreshMotion();
        }
        private void RefreshMotion() => settings.Get<Text>("motionLabel").text = new[] { "动态与震动：标准", "动态与震动：减弱", "动态与震动：关闭" }[Mathf.Clamp(PlayerPrefs.GetInt("Camera.Shake", 0), 0, 2)];
        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame && settings.gameObject.activeSelf) CloseSettings();
#endif
        }
        private void OnDestroy()
        {
            if (!settingsOnly) SetWorldActorsVisible(true);
            if (underneath != null) { underneath.interactable = underneathInteractive; underneath.alpha = underneathAlpha; }
            if (active == this) active = null;
        }
        private void SetWorldActorsVisible(bool visible)
        {
            if (manager == null) return;
            if (manager.Player != null)
                foreach (var renderer in manager.Player.GetComponentsInChildren<SpriteRenderer>(true)) renderer.enabled = visible;
            if (manager.Boss != null)
            {
                var entrance = manager.Boss.GetComponent<MMORPG.Game.Bosses.BossEntrancePlayer>();
                foreach (var renderer in manager.Boss.GetComponentsInChildren<SpriteRenderer>(true))
                    renderer.enabled = visible && !(renderer.transform == manager.Boss.transform && entrance != null && entrance.OwnsBodyRenderer);
            }
        }
    }
}
