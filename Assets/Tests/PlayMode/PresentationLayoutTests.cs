#if UNITY_INCLUDE_TESTS
using MMORPG.Game.UI;
using NUnit.Framework;
using UnityEngine;

namespace MMORPG.Tests.PlayMode
{
    public sealed class PresentationLayoutTests
    {
        [Test]
        public void DefeatSummaryDoesNotOverlapProgressOrDetails()
        {
            var screen = GardenUiCatalog.Load().defeat;
            var summary = screen.Get<UnityEngine.UI.Text>("summary").rectTransform;
            var progress = screen.Get<UnityEngine.UI.Text>("progress").rectTransform;
            var details = screen.Get<UnityEngine.UI.Text>("details").rectTransform;
            Assert.IsFalse(Bounds(summary).Overlaps(Bounds(progress)), "失败统计不能遮挡挑战进度。");
            Assert.IsFalse(Bounds(summary).Overlaps(Bounds(details)), "展开详情不能遮挡失败统计。");
        }

        private static Rect Bounds(RectTransform rect)
        {
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            return Rect.MinMaxRect(corners[0].x, corners[0].y, corners[2].x, corners[2].y);
        }
    }
}
#endif
