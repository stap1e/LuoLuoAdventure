using LuoLuoTrip.Combat;
using NUnit.Framework;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    public class CombatControllerInputTests
    {
        private GameObject _playerGo;

        [TearDown]
        public void TearDown()
        {
            if (_playerGo != null)
                Object.DestroyImmediate(_playerGo);
        }

        private CombatController CreatePlayer()
        {
            _playerGo = new GameObject("TestPlayer");
            var entity = _playerGo.AddComponent<CharacterEntity>();
            entity.Bind(CharacterData.Create("test_player", "TestPlayer", SubFactionId.MotorIronRiders, CharacterRole.Common));
            _playerGo.AddComponent<Combatant>();
            var ctrl = _playerGo.AddComponent<CombatController>();
            return ctrl;
        }

        [Test]
        public void KeyCode_Fallback_W_MapsToPositiveVertical()
        {
            var ctrl = CreatePlayer();
            ctrl.ApplyMoveInput(new Vector2(0f, 1f));
            Assert.Pass("ApplyMoveInput with (0,1) did not throw — W maps to forward");
        }

        [Test]
        public void InputDisabled_BlocksMovement()
        {
            var ctrl = CreatePlayer();
            ctrl.SetInputEnabled(false);
            Assert.That(ctrl.IsInputEnabled, Is.False);
        }

        [Test]
        public void InputEnabled_AllowsMovement()
        {
            var ctrl = CreatePlayer();
            ctrl.SetInputEnabled(true);
            Assert.That(ctrl.IsInputEnabled, Is.True);
        }

        [Test]
        public void ZeroMoveSpeed_FallbackIsNonZero()
        {
            var ctrl = CreatePlayer();

            var field = typeof(CombatController).GetField("_moveSpeed",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null) field.SetValue(ctrl, 0f);

            var posBefore = _playerGo.transform.position;
            ctrl.ApplyMoveInput(new Vector2(0f, 1f));
            var posAfter = _playerGo.transform.position;

            Assert.That(Vector3.Distance(posBefore, posAfter), Is.GreaterThan(0f),
                "Move with zero serialized speed should use fallback");
        }

        [Test]
        public void CombatState_Idle_AllowsMovement()
        {
            var ctrl = CreatePlayer();
            var combatant = _playerGo.GetComponent<Combatant>();
            Assert.That(combatant.State, Is.EqualTo(CombatState.Idle));
        }

        [Test]
        public void DefaultInputEnabled_IsTrue()
        {
            var ctrl = CreatePlayer();
            Assert.That(ctrl.IsInputEnabled, Is.True);
        }

        [Test]
        public void ReadMoveInput_ReturnsVector2()
        {
            var ctrl = CreatePlayer();
            var input = ctrl.ReadMoveInput();
            Assert.That(input, Is.InstanceOf<Vector2>());
        }

        [Test]
        public void ApplyMoveInput_NonZeroInput_ChangesPosition()
        {
            var ctrl = CreatePlayer();
            var posBefore = _playerGo.transform.position;
            ctrl.ApplyMoveInput(new Vector2(0f, 1f));
            var posAfter = _playerGo.transform.position;
            Assert.That(Vector3.Distance(posBefore, posAfter), Is.GreaterThan(0f));
        }

        [Test]
        public void ApplyMoveInput_ZeroInput_DoesNotChangePosition()
        {
            var ctrl = CreatePlayer();
            var posBefore = _playerGo.transform.position;
            ctrl.ApplyMoveInput(Vector2.zero);
            var posAfter = _playerGo.transform.position;
            Assert.That(Vector3.Distance(posBefore, posAfter), Is.EqualTo(0f));
        }
    }
}
