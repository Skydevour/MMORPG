using MMORPG.Game.Config;
using UnityEngine;
namespace MMORPG.Game.VFX
{
    public sealed class EnergyPickupEffect : MonoBehaviour
    {
        public static void Spawn(Vector3 position)
        {
            MMORPG.Game.UI.PlayerEnergyMeter.ShowCollection(position);
            PooledBattleEffect.Spawn(position, new Color(1f, 0.25f, 0.65f), 0.22f, 12,
                GameConfigService.Current.projectile.pinkPickupEffectDuration);
        }
    }
}
