using UnityEngine;
namespace MMORPG.Game.VFX
{
    public sealed class CombatImpactEffect : MonoBehaviour
    {
        public static void SpawnHit(Vector3 position, Color color, bool strong = false, Vector2 direction = default)
        {
            var config = MMORPG.Game.Config.GameConfigService.Current.vfx;
            PooledBattleEffect.Spawn(position, color, config.hitSize * (strong ? 1.8f : 1f), config.hitCount * (strong ? 2 : 1), config.hitDuration,
                drift: direction.normalized * config.hitDriftSpeed);
        }
        public static void SpawnDeath(Vector3 position, Color color)
        {
            var config = MMORPG.Game.Config.GameConfigService.Current.vfx;
            PooledBattleEffect.Spawn(position, color, config.deathSize, config.deathCount, config.deathDuration);
        }
    }
}
