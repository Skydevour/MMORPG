using UnityEngine;

namespace MMORPG.Game.Bosses
{
    public sealed class GardenBossAssets : ScriptableObject
    {
        public Sprite[] onionIdle, onionAttack, carrotIdle, carrotAttack;
        public BossEntranceClip potatoEntrance = new BossEntranceClip();
        public BossEntranceClip onionEntrance = new BossEntranceClip();
        public BossEntranceClip carrotEntrance = new BossEntranceClip();
        public Material lineMaterial;
        public Material impactMaterial;
        public Sprite resultPaper;
        public bool developmentArt = true;
        public BossSpriteGeometry potatoGeometry, onionGeometry, carrotGeometry;
    }
}
