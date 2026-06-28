using LuoLuoTrip.Combat;
using NUnit.Framework;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    public class CombatTuningBalanceTests
    {
        [Test]
        public void DefaultConfig_Validates()
        {
            var config = CombatTuningConfigSO.Default;
            Assert.IsTrue(config.Validate(out var err), err);
        }

        [Test]
        public void PlayerAttackDamage_DefaultOrOverride_NonNegative()
        {
            var config = CombatTuningConfigSO.Default;
            Assert.GreaterOrEqual(config.playerAttackDamage, 0f);
        }

        [Test]
        public void EnemyAttackDamage_DefaultOrOverride_NonNegative()
        {
            var config = CombatTuningConfigSO.Default;
            Assert.GreaterOrEqual(config.enemyAttackDamage, 0f);
        }

        [Test]
        public void PlayerAttackRange_NonNegative()
        {
            var config = CombatTuningConfigSO.Default;
            Assert.GreaterOrEqual(config.playerAttackRange, 0f);
        }

        [Test]
        public void EnemyAttackRange_NonNegative()
        {
            var config = CombatTuningConfigSO.Default;
            Assert.GreaterOrEqual(config.enemyAttackRange, 0f);
        }

        [Test]
        public void AllDurations_Positive()
        {
            var config = CombatTuningConfigSO.Default;
            Assert.Greater(config.playerAttackWindup, 0f);
            Assert.Greater(config.playerAttackActive, 0f);
            Assert.Greater(config.playerAttackRecovery, 0f);
            Assert.Greater(config.enemyAttackWindup, 0f);
            Assert.Greater(config.enemyAttackActiveDuration, 0f);
            Assert.Greater(config.enemyAttackRecovery, 0f);
            Assert.Greater(config.dodgeDuration, 0f);
            Assert.Greater(config.staggerDuration, 0f);
            Assert.Greater(config.hitFlashDuration, 0f);
            Assert.Greater(config.damageNumberDuration, 0f);
        }

        [Test]
        public void AIParams_Positive()
        {
            var config = CombatTuningConfigSO.Default;
            Assert.Greater(config.aiChaseSpeed, 0f);
            Assert.Greater(config.aiAttackCooldown, 0f);
            Assert.Greater(config.aiStopDistance, 0f);
        }

        [Test]
        public void Validate_RejectsZeroWindup()
        {
            var config = ScriptableObject.CreateInstance<CombatTuningConfigSO>();
            config.playerAttackWindup = 0f;
            Assert.IsFalse(config.Validate(out var err));
            Assert.IsNotNull(err);
        }
    }
}
