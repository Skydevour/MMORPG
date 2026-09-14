using System.Text;
using UnityEngine;

namespace MMORPG.Game.Combat
{
    public sealed class BattleStats
    {
        private static BattleStats active;

        private float duration;
        private bool ended;

        public static BattleStats Active => active;

        public float Duration => duration;

        public int HitsTaken { get; private set; }

        public int PinkCollected { get; private set; }

        public int SuperUses { get; private set; }

        public int PhaseTransitions { get; private set; }

        public bool Victory { get; private set; }

        public bool Defeat { get; private set; }

        public static BattleStats Begin()
        {
            active = new BattleStats();
            return active;
        }

        public static void Reset()
        {
            active = null;
        }

        public void Tick(float deltaTime)
        {
            if (ended)
            {
                return;
            }

            duration += deltaTime;
        }

        public void RecordHitTaken()
        {
            if (!ended)
            {
                HitsTaken++;
            }
        }

        public void RecordPinkCollected()
        {
            if (!ended)
            {
                PinkCollected++;
            }
        }

        public void RecordSuperUse()
        {
            if (!ended)
            {
                SuperUses++;
            }
        }

        public void RecordPhaseTransition()
        {
            if (!ended)
            {
                PhaseTransitions++;
            }
        }

        public void Finish(bool victory)
        {
            if (ended)
            {
                return;
            }

            ended = true;
            Victory = victory;
            Defeat = !victory;
        }

        public string BuildSummaryText()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"战斗时长 {duration:0.0} 秒");
            builder.AppendLine($"玩家受击 {HitsTaken} 次");
            builder.AppendLine($"粉色子弹收集 {PinkCollected} 颗");
            builder.AppendLine($"大招使用 {SuperUses} 次");
            builder.AppendLine($"阶段推进 {PhaseTransitions} 次");
            return builder.ToString();
        }
    }
}
