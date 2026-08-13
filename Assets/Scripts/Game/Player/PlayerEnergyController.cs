using System;
using MMORPG.Game.Config;
using UnityEngine;

namespace MMORPG.Game.Player
{
    public sealed class PlayerEnergyController : MonoBehaviour
    {
        public static PlayerEnergyController Active { get; private set; }

        public event Action<int, int> EnergyChanged;

        public int CurrentEnergy { get; private set; }

        public int MaxEnergy { get; private set; }

        public bool IsFull => CurrentEnergy >= MaxEnergy;

        private void Awake()
        {
            Active = this;
            MaxEnergy = GameConfigService.Current.player.maxEnergy;
            CurrentEnergy = 0;
        }

        private void OnDestroy()
        {
            if (Active == this)
            {
                Active = null;
            }
        }

        public void AddEnergy(int amount)
        {
            if (amount <= 0 || IsFull)
            {
                return;
            }

            int previous = CurrentEnergy;
            CurrentEnergy = Mathf.Clamp(CurrentEnergy + amount, 0, MaxEnergy);
            if (previous != CurrentEnergy)
            {
                EnergyChanged?.Invoke(CurrentEnergy, MaxEnergy);
            }
        }

        public bool TryConsumeAll()
        {
            if (!IsFull)
            {
                return false;
            }

            CurrentEnergy = 0;
            EnergyChanged?.Invoke(CurrentEnergy, MaxEnergy);
            return true;
        }
    }
}
