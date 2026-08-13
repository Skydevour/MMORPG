using System;

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
    }
}