using MMORPG.Game.Audio;
using MMORPG.Game.Config;
using MMORPG.Game.Projectiles;
using UnityEngine;

namespace MMORPG.Game.Bosses.Onion
{
    public sealed class OnionBossController : GardenBossActor
    {
        private int group;
        private int cycle;
        private int safeColumn;
        public int SafeColumn => safeColumn;
        public int SafeSpan => CalculateSafeSpan(target.CombatRightEdge);
        public static int CalculateSafeSpan(float rightEdge)
        {
            var config = GameConfigService.Current;
            float cell = (rightEdge - config.level.minStageX) / config.encounter.rainColumns;
            int span = Mathf.Max(2, Mathf.CeilToInt((1.6f + config.player.colliderSizeX + BossProjectile.RainDiameter) / cell));
            if (cell <= 0f || span >= config.encounter.rainColumns)
                throw new System.InvalidOperationException("洋葱泪雨舞台过窄，无法同时提供安全走廊与危险列。");
            return span;
        }

        public static int ChooseSafeColumn(float playerX, int cycleIndex, float rightEdge)
        {
            var config = GameConfigService.Current;
            float cell = (rightEdge - config.level.minStageX) / config.encounter.rainColumns;
            int column = Mathf.FloorToInt((playerX - config.level.minStageX) / cell);
            // 相邻走廊轮换；预备加落点预警足够横移一列，边缘不能永久站桩。
            int span = CalculateSafeSpan(rightEdge);
            return Mathf.Clamp(column + (cycleIndex % 2 == 0 ? -span : 1), 0, config.encounter.rainColumns - span);
        }

        public static float RainPosition(int column, int cycleIndex, float rightEdge)
        {
            var config = GameConfigService.Current;
            float width = (rightEdge - config.level.minStageX) / config.encounter.rainColumns;
            float fraction = 0.2f + ((cycleIndex + column) % 3) * 0.3f;
            return config.level.minStageX + (column + fraction) * width;
        }

        private int ChoosePinkColumn()
        {
            float bossLeft = GetComponent<Collider2D>().bounds.min.x;
            int best = -1;
            float nearest = float.MaxValue;
            for (int column = 0; column < settings.rainColumns; column++)
            {
                float x = RainPosition(column, cycle, target.CombatRightEdge);
                if ((column >= safeColumn && column < safeColumn + SafeSpan) || x >= bossLeft - 0.5f) continue;
                float distance = Mathf.Abs(x - target.FootPosition.x);
                if (distance >= nearest) continue;
                nearest = distance; best = column;
            }
            return best;
        }
        protected override void TickAttack(float deltaTime)
        {
            timer -= deltaTime;
            if (timer > 0f) return;
            if (!attacking)
            {
                attacking = true; group = 0; cycle++;
                Present(BossActionPhase.Tell);
                safeColumn = ChooseSafeColumn(target.FootPosition.x, cycle, target.CombatRightEdge);
                timer = settings.onionTell;
                BattleAudio.Play("sob_tell", transform.position);
                return;
            }
            if (group >= settings.rainGroups)
            {
                attacking = false; timer = settings.onionRecovery + settings.rainWarning + (GameConfigService.Current.camera.Top - GameConfigService.Current.camera.rainTopInset - GameConfigService.Current.level.stageFloorY) / settings.rainSpeed;
                BattleAudio.Play("wipe", transform.position);
                Present(BossActionPhase.Recover);
                return;
            }
            var level = GameConfigService.Current.level;
            Present(BossActionPhase.Release, 0.12f);
            // 整轮保留同一安全走廊，避免相邻组落泪重叠时互相堵死退路。
            int firstSafe = safeColumn;
            int spawned = 0;
            int pinkColumn = group == 1 ? ChoosePinkColumn() : -1;
            if (pinkColumn >= 0)
            {
                float x = RainPosition(pinkColumn, cycle, target.CombatRightEdge);
                GardenHazard.SpawnRain(new Vector3(x, level.stageFloorY, 0f), settings.rainWarning, settings.rainSpeed, true);
                spawned++;
            }
            for (int offset = 0; offset < settings.rainColumns && spawned < 3; offset++)
            {
                int column = (group * 3 + cycle + offset) % settings.rainColumns;
                if ((column >= firstSafe && column < firstSafe + SafeSpan) || column == pinkColumn) continue;
                float x = RainPosition(column, cycle, target.CombatRightEdge);
                GardenHazard.SpawnRain(new Vector3(x, level.stageFloorY, 0f), settings.rainWarning, settings.rainSpeed, false);
                spawned++;
            }
            group++; timer = settings.rainInterval;
        }
    }
}
