using System.Collections.Generic;
using MMORPG.Game.Player;
using UnityEngine;

namespace MMORPG.Game.Combat
{
    public sealed class DirectionalDamageVolume : MonoBehaviour
    {
        private readonly HashSet<IDamageable> damaged = new HashSet<IDamageable>();
        private readonly Collider2D[] hits = new Collider2D[32];
        private Vector2 size;
        private int direction;
        private int damage;

        public void Begin(int facing, Vector2 dimensions, int amount)
        {
            damaged.Clear();
            direction = facing;
            size = dimensions;
            damage = amount;
        }

        public bool ResolveHits()
        {
            bool applied = false;
            Vector2 center = (Vector2)transform.position + Vector2.right * direction * size.x * 0.5f;
            int count = Physics2D.OverlapBox(center, size, 0f, new ContactFilter2D { useTriggers = true }, hits);
            for (int index = 0; index < count; index++)
            {
                var target = hits[index].GetComponentInParent<IDamageable>();
                if (target == null || target is PlayerController2D || target.IsDead || damaged.Contains(target)) continue;
                if (target.ReceiveHit(new HitContext(damage, hits[index].ClosestPoint(transform.position), Vector2.right * direction)).Accepted())
                { damaged.Add(target); applied = true; }
            }
            return applied;
        }

        public void Clear() => damaged.Clear();
    }
}
