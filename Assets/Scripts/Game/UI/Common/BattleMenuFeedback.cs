using MMORPG.Game.Audio;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MMORPG.Game.UI
{
    public sealed class BattleMenuFeedback : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, ISubmitHandler
    {
        public UnityEngine.UI.Graphic focusMark;
        private bool selected, hovered, pressed;
        private float submitRemaining;
        public void OnSelect(BaseEventData e) { selected = true; BattleAudio.Play("ui_focus"); }
        public void OnDeselect(BaseEventData e) => selected = false;
        public void OnPointerEnter(PointerEventData e) => hovered = true;
        public void OnPointerExit(PointerEventData e) { hovered = false; pressed = false; }
        public void OnPointerDown(PointerEventData e) => pressed = true;
        public void OnPointerUp(PointerEventData e) => pressed = false;
        public void OnSubmit(BaseEventData e) => submitRemaining = GardenUiCatalog.Rules.pressDuration;
        private void Update()
        {
            submitRemaining = Mathf.Max(0f, submitRemaining - Time.unscaledDeltaTime);
            var rules = GardenUiCatalog.Rules;
            if (focusMark != null)
            {
                var c = focusMark.color; c.a = Mathf.MoveTowards(c.a, selected || hovered ? 1f : 0f, Time.unscaledDeltaTime / Mathf.Max(0.01f, rules.focusDuration)); focusMark.color = c;
            }
            float target = !GardenUiCatalog.ReducedMotion && (pressed || submitRemaining > 0f) ? 0.97f : 1f;
            transform.localScale = Vector3.one * Mathf.MoveTowards(transform.localScale.x, target, Time.unscaledDeltaTime * 0.03f / Mathf.Max(0.01f, target < 1f ? rules.pressDuration : rules.releaseDuration));
        }
        private void OnDisable() { selected = hovered = pressed = false; submitRemaining = 0f; transform.localScale = Vector3.one; }
    }
}
