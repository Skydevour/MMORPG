using System;
using UnityEngine;

namespace MMORPG.Game.Config
{
    [Serializable]
    public sealed class EncounterConfig
    {
        public bool enabled = true;
        public float introDuration = 1.2f;
        public float transitionDuration = 1.3f;
        public float exitDuration = 0.45f;
        public float safetyDuration = 0.6f;
        public float bossSeparation = 0.1f;
        public BossFormConfig[] forms = {
            new BossFormConfig { id = "potato", displayName = "土豆", maxHealth = 100, visibleHeight = 4.2f },
            new BossFormConfig { id = "onion", displayName = "洋葱", maxHealth = 120, visibleHeight = 4.4f },
            new BossFormConfig { id = "carrot", displayName = "胡萝卜", maxHealth = 150, visibleHeight = 4.6f }
        };
        public float potatoTell = 0.35f, potatoInterval = 0.18f, potatoRecovery = 0.45f, potatoSpeed = 8f;
        public float potatoCooldownMin = 0.65f, potatoCooldownMax = 0.95f;
        public float[] potatoHeights = { 1.75f, 0.95f, 0.35f };
        public float onionTell = 0.65f, rainInterval = 0.42f, rainWarning = 0.55f, rainSpeed = 6.5f, onionRecovery = 0.85f;
        public int rainGroups = 4, rainColumns = 8;
        public float beamTell = 0.55f, beamLock = 0.2f, beamActive = 0.18f, beamWidth = 0.45f, carrotRecovery = 0.75f;
        public float seekerSpeed = 3.8f, seekerTurnRate = 100f, seekerLifetime = 4f, seekerStraightTime = 0.25f, seekerInterval = 0.45f;

        public void Validate()
        {
            string[] ids = { "potato", "onion", "carrot" };
            if (forms == null || forms.Length != 3) throw new InvalidOperationException("遭遇配置必须包含土豆、洋葱、胡萝卜三个形态。");
            for (int i = 0; i < 3; i++)
                if (forms[i] == null || forms[i].id != ids[i] || forms[i].maxHealth < 1 || forms[i].visibleHeight <= 0f)
                    throw new InvalidOperationException($"遭遇配置第 {i + 1} 个形态无效。");
            if (introDuration < 0 || transitionDuration < 0.7f || exitDuration < 0f || exitDuration > transitionDuration || safetyDuration < 0.6f ||
                potatoInterval <= 0 || rainInterval <= 0 || rainWarning < 0.55f || rainColumns != 8 || rainGroups < 1 ||
                beamTell <= beamLock || beamLock < 0.2f || beamActive <= 0 || seekerLifetime <= 0 || seekerInterval <= 0)
                throw new InvalidOperationException("遭遇攻击时序或安全窗口配置无效。");
            if (potatoHeights == null || potatoHeights.Length != 3 || potatoTell < 0f || potatoRecovery < 0f || potatoSpeed <= 0f ||
                potatoCooldownMin < 0f || potatoCooldownMax < potatoCooldownMin || onionTell < 0f || onionRecovery < 0f || rainSpeed <= 0f ||
                beamWidth <= 0f || carrotRecovery < 0f || seekerSpeed <= 0f || seekerTurnRate < 0f || seekerStraightTime < 0f)
                throw new InvalidOperationException("遭遇弹速、恢复时间或弹道配置无效。");
        }
    }

    [Serializable] public sealed class BossFormConfig
    {
        public string id, displayName;
        public int maxHealth;
        public float spawnX = 4f, embedDepth = 0.12f, visibleHeight;
    }

    [Serializable] public sealed class FeedbackConfig
    {
        public float parryStop = 0.045f, parryBounce = 7.5f, parryProtection = 0.18f, parryPose = 0.1f;
        public float hurtStop = 0.055f, superStop = 0.07f, deathStop = 0.09f;
        public float superWindup = 0.12f, energyTravelDuration = 0.3f;
    }

    [Serializable] public sealed class PresentationConfig
    {
        public GardenUiConfig ui = new GardenUiConfig();
        public string title = "菜园恶战", start = "开始挑战", settings = "设置", quit = "退出";
        public string victory = "YOU WIN", defeat = "YOU LOSE", retry = "RETRY", returnToTitle = "TITLE";
        public float resultFadeDuration = 0.3f;
        public float bossHealthFillDuration = 0.3f;
        public float bossHealthTrailSpeed = 2.4f;
    }

    [Serializable] public sealed class BattleAudioConfig
    {
        public int maxVoices = 24;
        public float master = 1f, music = 0.316f, effects = 0.5f;
        public float musicCrossfadeDuration = 0.5f;
        public float shotDuckDuration = 0.18f, shotDuckGain = 0.631f;
        public AudioEventConfig[] events = Array.Empty<AudioEventConfig>();
    }

    [Serializable] public sealed class AudioEventConfig
    {
        public string eventId;
        public int priority = 50, maxVoices = 3;
        public float gain = 0.6f, minInterval = 0.06f;
    }
}
