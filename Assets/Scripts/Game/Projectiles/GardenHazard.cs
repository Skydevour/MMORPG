using System.Collections.Generic;
using MMORPG.Framework.Pooling;
using MMORPG.Game.Audio;
using MMORPG.Game.Bosses;
using MMORPG.Game.Combat;
using MMORPG.Game.Config;
using MMORPG.Game.Player;
using UnityEngine;

namespace MMORPG.Game.Projectiles
{
    public sealed class GardenHazard : MonoBehaviour, IPoolable
    {
        private static ComponentObjectPool<GardenHazard> pool;
        private static Transform root;
        private static readonly HashSet<GardenHazard> active = new HashSet<GardenHazard>();
        private readonly Collider2D[] contacts = new Collider2D[16];
        private LineRenderer line;
        private float age, tell, duration, speed;
        private bool beam, pink, fired, lockAnnounced;
        private Vector2 direction;
        public Vector2 Direction => direction;
        public bool IsAimLocked => beam && age >= tell - GameConfigService.Current.encounter.beamLock;
        public float TimeUntilDanger => Mathf.Max(0f, tell - age);
        public bool IsDangerous => beam && age >= tell && age < tell + duration;
        public static int ActiveCount => active.Count;

        private static GardenHazard Rent(Vector3 position)
        {
            if (root == null)
            {
                root = new GameObject("GardenHazardPool").transform;
                pool = new ComponentObjectPool<GardenHazard>(Create, root, 12);
            }
            var hazard = pool.Get(position, Quaternion.identity); active.Add(hazard); return hazard;
        }
        private static GardenHazard Create()
        {
            var result = new GameObject("GardenHazard").AddComponent<GardenHazard>();
            result.line = result.gameObject.AddComponent<LineRenderer>();
            result.line.useWorldSpace = false; result.line.positionCount = 2; result.line.sortingOrder = 70;
            result.line.sharedMaterial = Resources.Load<GardenBossAssets>("Config/GardenBossAssets").lineMaterial;
            return result;
        }
        public static GardenHazard SpawnRain(Vector3 ground, float warning, float fallSpeed, bool isPink)
        {
            var h = Rent(ground); h.beam = false; h.pink = isPink; h.tell = warning; h.speed = fallSpeed;
            h.duration = 0.25f; h.line.widthMultiplier = 0.06f;
            h.line.SetPosition(0, Vector3.left * 0.45f); h.line.SetPosition(1, Vector3.right * 0.45f);
            h.line.startColor = h.line.endColor = isPink ? Color.magenta : new Color(0.25f, 0.85f, 1f);
            return h;
        }
        public static GardenHazard SpawnBeam(Vector3 origin, Vector2 aim)
        {
            var h = Rent(origin); var e = GameConfigService.Current.encounter;
            h.beam = true; h.tell = e.beamTell; h.duration = e.beamActive;
            h.Aim(aim); return h;
        }
        public void Aim(Vector2 aim)
        {
            if (IsAimLocked) return;
            direction = aim.normalized;
            line.SetPosition(0, Vector3.zero); line.SetPosition(1, direction * 18f);
        }
        private void Update()
        {
            if (Time.deltaTime <= 0f) return;
            age += Time.deltaTime;
            if (beam)
            {
                if (IsAimLocked && !lockAnnounced)
                {
                    lockAnnounced = true;
                    BattleAudio.Play("lock", transform.position);
                }
                line.widthMultiplier = IsDangerous ? GameConfigService.Current.encounter.beamWidth : 0.035f;
                line.startColor = line.endColor = IsDangerous ? new Color(1f, 0.94f, 0.55f) : new Color(1f, 0.35f, 0.25f, 0.8f);
                if (IsDangerous && !fired) { fired = true; BattleAudio.Play("beam", transform.position); }
                if (age >= tell + duration) Release();
            }
            else if (age >= tell && !fired)
            {
                fired = true;
                var camera = GameConfigService.Current.camera;
                var p = BossProjectile.Spawn(new Vector3(transform.position.x, camera.Top - camera.rainTopInset, 0f), Vector2.down * speed,
                    pink ? BossProjectileType.Pink : BossProjectileType.Normal);
                p.SetRain();
                BattleAudio.Play("tear_fall", transform.position);
                Release();
            }
        }
        private void FixedUpdate()
        {
            if (!IsDangerous) return;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            int count = Physics2D.OverlapBox((Vector2)transform.position + direction * 9f,
                new Vector2(18f, GameConfigService.Current.encounter.beamWidth), angle,
                new ContactFilter2D { useTriggers = true }, contacts);
            for (int i = 0; i < count; i++)
            {
                var player = contacts[i].GetComponentInParent<PlayerController2D>();
                if (player != null) player.ReceiveHit(new HitContext(1, contacts[i].ClosestPoint(transform.position), direction));
            }
        }
        private void Release() { active.Remove(this); pool.Release(this); }
        public static void ClearAll()
        {
            var snapshot = new List<GardenHazard>(active);
            foreach (var h in snapshot) if (h != null) h.Release();
            active.Clear();
        }
        public void OnSpawnedFromPool() { age = 0f; fired = false; lockAnnounced = false; }
        public void OnDespawnedToPool() { }
    }
}
