using UnityEngine;
using MMORPG.Game.Config;
namespace MMORPG.Game.UI
{
    public sealed class GardenUiCatalog : ScriptableObject
    {
        public GardenUiScreen hud, energy, title, pause, settings, defeat, victory;
        public Sprite ticket, programme, defeatPaper, healthFull, healthEmpty, energyFull, energyEmpty, stamp, sprig;
        public Font font;
        public Sprite stampPending;
        public bool developmentArt = true;
        public static GardenUiConfig Rules => GameConfigService.Current.presentation.ui ?? new GardenUiConfig();
        public static bool ReducedMotion => PlayerPrefs.GetInt("Camera.Shake", 0) != 0;
        public static Color Color(string hex) { ColorUtility.TryParseHtmlString(hex, out var c); return c; }
        public static GardenUiCatalog Load()
        {
            var value = Resources.Load<GardenUiCatalog>("Config/GardenUiCatalog");
            if (value == null) throw new System.InvalidOperationException("缺少 UI 资源目录 Config/GardenUiCatalog，请执行 Prepare Garden UI。");
            return value;
        }
    }
}
