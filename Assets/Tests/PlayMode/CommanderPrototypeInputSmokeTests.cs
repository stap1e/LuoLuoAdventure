using System.Collections;
using NUnit.Framework;
using LuoLuoTrip.Combat;
using UnityEngine;
using UnityEngine.TestTools;

namespace LuoLuoTrip.Tests.PlayMode
{
    public class CommanderPrototypeInputSmokeTests
    {
        [TearDown]
        public void TearDown()
        {
            CharacterEntity.HostilityResolver = null;
        }

        [UnityTest]
        public IEnumerator CommanderControlController_HasHasSelectedTarget()
        {
            var go = new GameObject("Player");
            try
            {
                var entity = go.AddComponent<CharacterEntity>();
                entity.Bind(new CharacterData("player", "Player", SubFactionId.MotorIronRiders, CharacterRole.Common, 5));
                go.AddComponent<Combatant>();
                go.AddComponent<CombatController>();
                var ctrl = go.AddComponent<CommanderControlController>();

                Assert.That(ctrl.HasSelectedTarget(), Is.False);
                Assert.That(ctrl.State, Is.Not.Null);

                yield return null;
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [UnityTest]
        public IEnumerator CommanderControlRuntimeState_SyncAssistAppliesBuffs()
        {
            var targetGo = new GameObject("Target");
            try
            {
                var entity = targetGo.AddComponent<CharacterEntity>();
                entity.Bind(new CharacterData("target", "Target", SubFactionId.MotorIronRiders, CharacterRole.Minion, 5));
                var combatant = targetGo.AddComponent<Combatant>();

                var state = new CommanderControlRuntimeState();
                state.ActivateSyncAssist(3f);
                state.ApplySyncAssistBuff(entity);

                Assert.That(combatant.SyncAssistAttackBonus, Is.GreaterThan(0f));
                Assert.That(combatant.SyncAssistDefenseBonus, Is.GreaterThan(0f));

                state.ReleaseControl();

                yield return null;
            }
            finally
            {
                Object.DestroyImmediate(targetGo);
            }
        }

        [UnityTest]
        public IEnumerator CombatController_BlockMovementDuringAttack()
        {
            var go = new GameObject("Player");
            var enemyGo = new GameObject("Enemy");
            try
            {
                var entity = go.AddComponent<CharacterEntity>();
                entity.Bind(new CharacterData("player", "Player", SubFactionId.MotorIronRiders, CharacterRole.Common, 5));
                var combatant = go.AddComponent<Combatant>();
                go.AddComponent<CombatController>();
                combatant.AutoTickEnabled = false;

                var enemyEntity = enemyGo.AddComponent<CharacterEntity>();
                enemyEntity.Bind(new CharacterData("enemy", "Enemy", SubFactionId.BeastIronClaw, CharacterRole.Minion, 5));
                enemyGo.AddComponent<Combatant>();

                combatant.TryLightAttack(enemyEntity.Combatant);
                Assert.That(combatant.State, Is.Not.EqualTo(CombatState.Idle));

                yield return null;
            }
            finally
            {
                Object.DestroyImmediate(go);
                Object.DestroyImmediate(enemyGo);
            }
        }
    }
}
