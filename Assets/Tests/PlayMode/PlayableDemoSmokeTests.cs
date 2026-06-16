using System.Collections;
using NUnit.Framework;
using LuoLuoTrip.Combat;
using UnityEngine;
using UnityEngine.TestTools;

namespace LuoLuoTrip.Tests.PlayMode
{
    public class PlayableDemoSmokeTests
    {
        [TearDown]
        public void TearDown()
        {
            CharacterEntity.HostilityResolver = null;
        }

        [UnityTest]
        public IEnumerator Combatant_CanCompleteFullAttackCycle()
        {
            var go = new GameObject("Attacker");
            var defenderGo = new GameObject("Defender");
            try
            {
                var entity = go.AddComponent<CharacterEntity>();
                entity.Bind(new CharacterData("test_attacker", "Attacker", SubFactionId.MotorIronRiders, CharacterRole.Common, 5));
                var attacker = go.AddComponent<Combatant>();

                var defEntity = defenderGo.AddComponent<CharacterEntity>();
                defEntity.Bind(new CharacterData("test_defender", "Defender", SubFactionId.BeastIronClaw, CharacterRole.Common, 5));
                defenderGo.AddComponent<Combatant>();

                attacker.AutoTickEnabled = false;

                Assert.That(attacker.TryLightAttack(defEntity.Combatant), Is.True);

                attacker.Tick(0.25f);
                Assert.That(attacker.State, Is.EqualTo(CombatState.Attacking));

                attacker.Tick(0.2f);
                Assert.That(attacker.State, Is.EqualTo(CombatState.AttackRecovery));

                attacker.Tick(0.3f);
                Assert.That(attacker.State, Is.EqualTo(CombatState.Idle));

                yield return null;
            }
            finally
            {
                Object.DestroyImmediate(go);
                Object.DestroyImmediate(defenderGo);
            }
        }

        [UnityTest]
        public IEnumerator Combatant_DodgeProvidesInvulnerability()
        {
            var go = new GameObject("Dodger");
            try
            {
                var entity = go.AddComponent<CharacterEntity>();
                entity.Bind(new CharacterData("dodger", "Dodger", SubFactionId.MotorIronRiders, CharacterRole.Common, 5));
                var dodger = go.AddComponent<Combatant>();
                dodger.AutoTickEnabled = false;

                dodger.TryDodge(Vector3.forward);
                Assert.That(dodger.IsInvulnerable, Is.True);

                yield return null;
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [UnityTest]
        public IEnumerator CommanderControlRuntimeState_ReleaseControl_ResetsState()
        {
            var state = new CommanderControlRuntimeState();

            state.ReleaseControl();

            Assert.That(state.IsDirectControllingOther, Is.False);
            Assert.That(state.HasActiveCommand, Is.False);
            Assert.That(state.IsSyncAssistActive, Is.False);

            yield return null;
        }

        [UnityTest]
        public IEnumerator MissionChainService_DefaultState_HasConvoyUnlocked()
        {
            var chainService = new MissionChainService();

            Assert.That(chainService.IsUnlocked("convoy_energy_conflict"), Is.True);
            Assert.That(chainService.IsUnlocked("border_retaliation"), Is.False);

            yield return null;
        }

        [UnityTest]
        public IEnumerator CombatTuningConfig_DefaultExists()
        {
            var config = CombatTuningConfigSO.LoadOrDefault();
            Assert.That(config, Is.Not.Null);
            Assert.That(config.playerAttackWindup, Is.GreaterThan(0f));

            yield return null;
        }
    }
}
