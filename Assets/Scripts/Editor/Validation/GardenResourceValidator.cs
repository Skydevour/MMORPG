#if UNITY_EDITOR
using MMORPG.Game.Audio;
using MMORPG.Game.Bosses;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace MMORPG.EditorTools.Validation
{
    public sealed class GardenResourceValidator : IPreprocessBuildWithReport
    {
        public int callbackOrder => 20;
        public void OnPreprocessBuild(BuildReport report)
        {
            var art = Resources.Load<GardenBossAssets>("Config/GardenBossAssets");
            if (art == null || art.lineMaterial == null || art.impactMaterial == null) throw new BuildFailedException("三形态图像或特效目录缺失。");
            if (art.developmentArt && (report.summary.options & BuildOptions.Development) == 0)
                throw new BuildFailedException("当前 Boss 仍使用开发占位原画，不能打包为正式发布版。");
            var geometry = new[] { art.potatoGeometry, art.onionGeometry, art.carrotGeometry };
            var forms = MMORPG.Game.Config.GameConfigService.Current.encounter.forms;
            for (int i = 0; i < geometry.Length; i++)
            {
                var g = geometry[i];
                if (g == null || g.idleBounds.height <= 0f || g.bodyHitBounds.width <= 0f || g.actionBounds.width <= 0f)
                    throw new BuildFailedException($"Boss {forms[i].id} 离线几何元数据缺失。");
                float width = g.actionBounds.width * forms[i].visibleHeight / g.idleBounds.height;
                if (width > 4.8f) Debug.LogWarning($"美术验收缺口：{forms[i].id} 全动作含飞土宽度 {width:F2} 超过4.8，需要独立特效资源；本次仅提供开发测试包。");
            }
            MMORPG.EditorTools.Import.GardenUiPreparation.Validate();
            CheckFrames(art.onionIdle, "洋葱待机", null); CheckFrames(art.onionAttack, "洋葱攻击", art.onionIdle[0]);
            CheckFrames(art.carrotIdle, "胡萝卜待机", null); CheckFrames(art.carrotAttack, "胡萝卜攻击", art.carrotIdle[0]);
            var audio = Resources.Load<BattleAudioCatalog>("Config/BattleAudioCatalog");
            if (audio == null || audio.mixer == null || audio.groups == null || audio.groups.Length != 5 || audio.music == null || audio.music.Length != 3)
                throw new BuildFailedException("三形态音频或混音器目录缺失。");
            foreach (var clip in audio.music) if (clip == null) throw new BuildFailedException("三形态音乐引用失效。");
            foreach (var group in audio.groups) if (group == null) throw new BuildFailedException("混音器分组引用失效。");
            string[] required = { "shot", "jump", "double_jump", "dash_start", "dash_end", "hurt", "death", "parry_success", "pickup", "energy_full", "charge", "release", "super_end", "super_loop", "emerge", "inhale", "spit", "boss_hit", "boss_defeat", "sob_tell", "tear_fall", "tear_splash", "wipe", "psychic_charge", "lock", "beam", "seeker", "stun", "ui_focus", "ui_confirm", "ui_cancel", "ui_start", "ui_defeat", "ui_victory" };
            if (audio.entries == null) throw new BuildFailedException("音频事件目录缺失。");
            foreach (string id in required)
                if (!System.Array.Exists(audio.entries, entry => entry != null && entry.id == id))
                    throw new BuildFailedException($"必需音频事件 {id} 未登记。");
            foreach (var entry in audio.entries)
            {
                if (entry == null) throw new BuildFailedException("音频事件存在空配置。");
                if (entry.clips == null || entry.clips.Length == 0) throw new BuildFailedException($"音频事件 {entry.id} 没有片段。");
                foreach (var clip in entry.clips) if (clip == null) throw new BuildFailedException($"音频事件 {entry.id} 引用失效。");
            }
        }
        private static void CheckFrames(Sprite[] frames, string action, Sprite reference)
        {
            if (frames == null || frames.Length == 0) throw new BuildFailedException($"动作 {action} 缺少序列帧。");
            if (reference == null) reference = frames[0];
            if (reference == null) throw new BuildFailedException($"动作 {action} 首帧缺失。");
            foreach (var sprite in frames)
                if (sprite == null || sprite.rect.size != reference.rect.size || sprite.pivot != reference.pivot ||
                    Mathf.Abs(sprite.pivot.x - sprite.rect.width * 0.5f) > 0.01f || sprite.pivot.y < 0f || sprite.pivot.y > sprite.rect.height * 0.01f)
                    throw new BuildFailedException($"动作 {action} 帧尺寸或锚点不一致。");
        }
    }
}
#endif
