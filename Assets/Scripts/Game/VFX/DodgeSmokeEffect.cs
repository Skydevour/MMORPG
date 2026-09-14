using UnityEngine;
namespace MMORPG.Game.VFX
{
    public sealed class DodgeSmokeEffect : MonoBehaviour
    {
        public static void Spawn(Vector3 position, int facingDirection)
        {
            var config = MMORPG.Game.Config.GameConfigService.Current.vfx;
            PooledBattleEffect.Spawn(position + Vector3.up * config.smokeCenterYOffset, Color.white,
                config.smokeSize, config.smokeCount, config.smokeDuration, true,
                new Vector2(-Mathf.Sign(facingDirection) * config.smokeDriftSpeed, config.smokeRiseSpeed));
        }
    }
}
