using MMORPG.Game.Audio;
using MMORPG.Game.Config;
using MMORPG.Game.Projectiles;
using UnityEngine;

namespace MMORPG.Game.Bosses.Carrot
{
    public sealed class CarrotBossController : GardenBossActor
    {
        private int cycle, seekerIndex;
        private GardenHazard beam;
        private float elapsed;
        private Vector2 eyeAnchor;
        private void Awake() => eyeAnchor = Resources.Load<GardenBossAssets>("Config/GardenBossAssets").carrotGeometry.eyes;
        public Vector3 EyePosition => transform.TransformPoint(eyeAnchor);
        protected override void TickAttack(float deltaTime)
        {
            timer -= deltaTime;
            if (attacking && cycle % 2 == 1)
            {
                elapsed += deltaTime;
                if (ActionPhase == BossActionPhase.Tell && elapsed >= settings.beamTell)
                    Present(BossActionPhase.Release, settings.beamActive);
                if (beam != null && elapsed < settings.beamTell - settings.beamLock)
                    beam.Aim((Vector2)(target.transform.position + Vector3.up * 0.5f - EyePosition));
            }
            if (timer > 0f) return;
            if (!attacking)
            {
                attacking = true; cycle++; elapsed = 0f; seekerIndex = 0;
                Present(BossActionPhase.Tell);
                if (cycle % 2 == 1)
                {
                    beam = GardenHazard.SpawnBeam(EyePosition, target.transform.position + Vector3.up * 0.5f - EyePosition);
                    timer = settings.beamTell + settings.beamActive;
                    BattleAudio.Play("psychic_charge", EyePosition);
                }
                else timer = settings.seekerInterval;
                return;
            }
            if (cycle % 2 == 0 && seekerIndex < 2)
            {
                BossProjectile.SpawnSeeker(EyePosition, target.transform, seekerIndex == 1);
                Present(BossActionPhase.Release, 0.12f);
                seekerIndex++; timer = seekerIndex < 2 ? settings.seekerInterval : settings.seekerLifetime;
                BattleAudio.Play("seeker", EyePosition);
                return;
            }
            attacking = false; beam = null; timer = settings.carrotRecovery;
            Present(BossActionPhase.Recover);
            BattleAudio.Play("stun", transform.position);
        }
        protected override void CancelAttack() { beam = null; }
    }
}
