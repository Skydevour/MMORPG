using MMORPG.Game.Combat;
using UnityEngine;

namespace MMORPG.Game.Bosses
{
    public interface IBossActor : IDamageable
    {
        Transform Root { get; }
        string DisplayName { get; }
        void SetBattleLocked(bool locked);
    }
}
