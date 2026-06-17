using LuoLuoTrip;
using LuoLuoTrip.Combat;
using LuoLuoTrip.Combat.Feedback;
using LuoLuoTrip.Feedback;
using NUnit.Framework;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    public class PlayerHitFeedbackTests
    {
        private Combatant CreatePlayer(GameObject go, string id, SubFactionId faction)
        {
            var entity = go.AddComponent<CharacterEntity>();
            entity.Bind(new CharacterData(id, id, faction, CharacterRole.Common, 5));
            go.AddComponent<CombatController>();
            return entity.Combatant;
        }

        [Test]
        public void Player_TakeDamage_ReducesHP()
        {
            var go = new GameObject("Player");
            try
            {
                var player = CreatePlayer(go, "p", SubFactionId.MotorIronRiders);
                player.AutoTickEnabled = false;
                var hpBefore = player.CurrentHealth;

                player.ApplyHealthDamage(20f);

                Assert.Less(player.CurrentHealth, hpBefore);
                Assert.AreEqual(hpBefore - 20f, player.CurrentHealth, 0.1f);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void Player_OnHitReceived_Fires()
        {
            var playerGo = new GameObject("Player");
            var enemyGo = new GameObject("Enemy");
            try
            {
                var player = CreatePlayer(playerGo, "p", SubFactionId.MotorIronRiders);
                var enemyEntity = enemyGo.AddComponent<CharacterEntity>();
                enemyEntity.Bind(new CharacterData("e", "e", SubFactionId.BeastIronClaw, CharacterRole.Common, 5));
                var enemy = enemyEntity.Combatant;
                player.AutoTickEnabled = false;
                enemy.AutoTickEnabled = false;

                int received = 0;
                player.OnHitReceived += _ => received++;

                enemy.TryLightAttack(player);
                enemy.Tick(enemy.AttackWindup + 0.001f);

                Assert.AreEqual(1, received);
            }
            finally { Object.DestroyImmediate(playerGo); Object.DestroyImmediate(enemyGo); }
        }

        [Test]
        public void Player_IsPlayerRole_True()
        {
            var go = new GameObject("Player");
            try
            {
                var player = CreatePlayer(go, "p", SubFactionId.MotorIronRiders);
                Assert.IsTrue(player.IsPlayerRole);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void Enemy_IsPlayerRole_False()
        {
            var go = new GameObject("Enemy");
            try
            {
                var entity = go.AddComponent<CharacterEntity>();
                entity.Bind(new CharacterData("e", "e", SubFactionId.BeastIronClaw, CharacterRole.Common, 5));
                Assert.IsFalse(entity.Combatant.IsPlayerRole);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void Player_Death_FiresOnDeath_AndMarksDataDead()
        {
            var go = new GameObject("Player");
            try
            {
                var player = CreatePlayer(go, "p", SubFactionId.MotorIronRiders);
                player.AutoTickEnabled = false;

                int deathCount = 0;
                player.OnDeath += _ => deathCount++;

                player.ApplyHealthDamage(99999f);
                Assert.AreEqual(1, deathCount);
                Assert.IsFalse(player.IsAlive);
                Assert.IsFalse(player.CharacterEntity.Data.IsAlive);
            }
            finally { Object.DestroyImmediate(go); }
        }
    }
}
