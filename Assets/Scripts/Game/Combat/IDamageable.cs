using System;
using UnityEngine;

namespace MMORPG.Game.Combat
{
    public interface IHealthSource
    {
        int CurrentHealth { get; }

        int MaxHealth { get; }

        bool IsDead { get; }

        event Action<int, int> HealthChanged;

        event Action Died;
    }

    public interface IDamageable : IHealthSource
    {
        void TakeDamage(int damage);
        HitResult ReceiveHit(HitContext hit);
    }

    public enum HitResult { Ignored, Invulnerable, Applied, Killed }

    public readonly struct HitContext
    {
        public readonly int Damage;
        public readonly Vector3 Point;
        public readonly Vector2 Direction;
        public readonly int AttackId;
        public HitContext(int damage, Vector3 point, Vector2 direction = default, int attackId = 0)
        { Damage = damage; Point = point; Direction = direction; AttackId = attackId; }
    }

    public static class HitResultExtensions
    {
        public static bool Accepted(this HitResult result) => result == HitResult.Applied || result == HitResult.Killed;
    }
}
