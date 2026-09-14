using System.Collections.Generic;
using MMORPG.Framework.Pooling;
using MMORPG.Game.Combat;
using MMORPG.Game.Config;
using MMORPG.Game.Core;
using MMORPG.Game.Bosses.Potato;
using UnityEngine;

namespace MMORPG.Game.VFX
{
    public sealed class SuperAttackEffect : MonoBehaviour, IPoolable
    {
        private static ComponentObjectPool<SuperAttackEffect> pool;
        private static Transform poolRoot;
        private DirectionalDamageVolume damageVolume;
        private LineRenderer beam;
        private LineRenderer core;
        private float remaining;
        private int direction;
        private SpecialConfig config;
        private float windup;
        private bool confirmedHit, released;
        private static readonly HashSet<SuperAttackEffect> active = new HashSet<SuperAttackEffect>();
        public static void ClearAll()
        {
            foreach (var effect in new List<SuperAttackEffect>(active)) if (effect != null) pool.Release(effect);
            active.Clear();
            MMORPG.Game.Audio.BattleAudio.StopLoop("super_loop");
        }

        public static void Spawn(Vector3 position, int facingDirection)
        {
            if (poolRoot == null)
            {
                poolRoot = new GameObject("SuperAttackPool").transform;
                pool = new ComponentObjectPool<SuperAttackEffect>(Create, poolRoot, 2);
            }
            var effect = pool.Get(position, Quaternion.identity, poolRoot);
            effect.direction = facingDirection;
            effect.config = GameConfigService.Current.special;
            effect.remaining = effect.config.duration;
            effect.windup = GameConfigService.Current.feedback.superWindup; effect.confirmedHit = effect.released = false; active.Add(effect);
            effect.damageVolume.Begin(facingDirection, new Vector2(effect.config.range, effect.config.height), effect.config.damage);
            effect.ConfigureLine(effect.beam, effect.config.height, new Color(0.25f, 0.9f, 1f));
            effect.ConfigureLine(effect.core, effect.config.height * 0.55f, Color.white);
            effect.beam.enabled = effect.core.enabled = false;
            MMORPG.Game.Audio.BattleAudio.Play("charge", position);
            ScreenShakeEffect.Shake(0.15f, 0.08f);
        }

        private static SuperAttackEffect Create()
        {
            GameObject go = new GameObject("PlayerSuperBeam");
            go.SetActive(false);
            var effect = go.AddComponent<SuperAttackEffect>();
            effect.damageVolume = go.AddComponent<DirectionalDamageVolume>();
            effect.beam = go.AddComponent<LineRenderer>();
            var child = new GameObject("Core");
            child.transform.SetParent(go.transform, false);
            effect.core = child.AddComponent<LineRenderer>();
            return effect;
        }

        private void ConfigureLine(LineRenderer line, float width, Color color)
        {
            line.sharedMaterial = Resources.Load<PrototypeSpriteCatalog>("Config/PrototypeSpriteCatalog").flashMaterial;
            line.useWorldSpace = false;
            line.positionCount = 2;
            line.SetPosition(0, Vector3.zero);
            line.SetPosition(1, Vector3.right * direction * config.range);
            line.startWidth = line.endWidth = width;
            line.startColor = line.endColor = color;
            line.numCapVertices = 6;
            line.sortingOrder = line == core ? 87 : 86;
        }

        private void Update()
        {
            if (Time.timeScale <= 0f) return;
            remaining -= Time.deltaTime;
            if (remaining <= 0f) { active.Remove(this); pool.Release(this); return; }
            windup -= Time.deltaTime;
            if (windup > 0f) return;
            if (!released)
            {
                released = true; beam.enabled = core.enabled = true;
                MMORPG.Game.Audio.BattleAudio.Play("release", transform.position);
                MMORPG.Game.Audio.BattleAudio.Play("super_loop", transform.position);
            }
            if (damageVolume.ResolveHits() && !confirmedHit)
            {
                confirmedHit = true;
                MMORPG.Framework.Timing.BattleClock.Stop(GameConfigService.Current.feedback.superStop);
            }
        }
        public void OnSpawnedFromPool() => damageVolume.Clear();
        public void OnDespawnedToPool()
        {
            damageVolume.Clear();
            MMORPG.Game.Audio.BattleAudio.StopLoop("super_loop");
            if (released) MMORPG.Game.Audio.BattleAudio.Play("super_end", transform.position);
        }
    }
}
