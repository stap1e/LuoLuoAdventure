using LuoLuoTrip;
using LuoLuoTrip.Combat;
using NUnit.Framework;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    public class CombatRootMovementTests
    {
        private GameObject _go;

        [TearDown]
        public void TearDown()
        {
            if (_go != null)
                Object.DestroyImmediate(_go);
        }

        private (CombatController ctrl, Combatant combatant, CharacterMovementMotor motor) CreatePlayer(Vector3 startPos)
        {
            _go = new GameObject("PlayerRoot");
            _go.transform.position = startPos;
            var entity = _go.AddComponent<CharacterEntity>();
            entity.Bind(CharacterData.Create("test_player", "TestPlayer", SubFactionId.MotorIronRiders, CharacterRole.Common));
            var motor = _go.AddComponent<CharacterMovementMotor>();
            var combatant = _go.GetComponent<Combatant>();
            if (combatant == null) combatant = _go.AddComponent<Combatant>();
            var ctrl = _go.AddComponent<CombatController>();
            return (ctrl, combatant, motor);
        }

        [Test]
        public void ApplyMoveInput_MovesRootOnXZ_KeepsY()
        {
            var (ctrl, _, _) = CreatePlayer(new Vector3(0f, 0.5f, 0f));
            var beforeY = _go.transform.position.y;
            ctrl.ApplyMoveInput(new Vector2(0f, 1f));
            // After ApplyMoveInput, root must not lose its Y.
            Assert.AreEqual(beforeY, _go.transform.position.y, 1e-3);
        }

        [Test]
        public void Player_HasMotor_Component()
        {
            var (_, _, motor) = CreatePlayer(Vector3.zero);
            Assert.IsNotNull(motor);
            Assert.IsNotNull(_go.GetComponent<CharacterMovementMotor>());
        }

        [Test]
        public void Combatant_DodgeUsesMotor_KeepsY()
        {
            var (_, combatant, _) = CreatePlayer(new Vector3(0f, 0.5f, 0f));
            combatant.AutoTickEnabled = false;
            combatant.TryDodge(Vector3.forward);
            for (int i = 0; i < 10; i++)
                combatant.Tick(0.05f);
            Assert.AreEqual(0.5f, _go.transform.position.y, 1e-3, "Dodge must not change root Y");
        }

        [Test]
        public void Combatant_AttackDoesNotLowerRootY()
        {
            var (_, combatant, _) = CreatePlayer(new Vector3(0f, 0.5f, 0f));
            combatant.AutoTickEnabled = false;

            var enemyGo = new GameObject("Enemy");
            enemyGo.transform.position = new Vector3(0.5f, 0.5f, 0f);
            var enemyEntity = enemyGo.AddComponent<CharacterEntity>();
            enemyEntity.Bind(CharacterData.Create("test_enemy", "Enemy", SubFactionId.BeastIronClaw, CharacterRole.Minion));
            var enemyCombatant = enemyGo.GetComponent<Combatant>();

            try
            {
                combatant.TryLightAttack(enemyCombatant);
                for (int i = 0; i < 30; i++)
                    combatant.Tick(0.05f);
                Assert.AreEqual(0.5f, _go.transform.position.y, 1e-3, "Attack sequence must not lower root Y");
            }
            finally
            {
                Object.DestroyImmediate(enemyGo);
            }
        }
    }
}
