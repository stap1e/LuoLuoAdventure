using System.Collections;
using LuoLuoTrip.AI;
using LuoLuoTrip.Combat;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace LuoLuoTrip.Tests.PlayMode
{
    /// <summary>
    /// End-to-end smoke tests covering the full Movement / Animation Regression contract:
    /// - Motor moves root X/Z only
    /// - Animator never drives root (applyRootMotion=false)
    /// - ProceduralAnimator never touches root when Visual exists
    /// - Dodge / Attack / AI fallback all preserve root Y
    /// </summary>
    public class MovementRegressionSmokeTests
    {
        private GameObject _go;
        private GameObject _go2;

        [TearDown]
        public void TearDown()
        {
            if (_go != null) Object.DestroyImmediate(_go);
            if (_go2 != null) Object.DestroyImmediate(_go2);
            CharacterRuntimeComponentGuard.ResetWarnings();
        }

        private GameObject CreateCharacterWithVisual(Vector3 pos, string name, SubFactionId faction)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            var visual = new GameObject("Visual");
            visual.transform.SetParent(go.transform, false);

            var entity = go.AddComponent<CharacterEntity>();
            entity.Bind(CharacterData.Create("test_" + name, name, faction, CharacterRole.Common));
            CharacterRuntimeComponentGuard.Ensure(go);
            return go;
        }

        [UnityTest]
        public IEnumerator Guard_AppliedToDynamicSpawn_ResultsIn_MovableRoot()
        {
            _go = CreateCharacterWithVisual(new Vector3(0f, 0.5f, 0f), "Player", SubFactionId.MotorIronRiders);
            var ctrl = _go.AddComponent<CombatController>();
            ctrl.SetInputEnabled(true);

            yield return null;

            var startY = _go.transform.position.y;
            var startX = _go.transform.position.x;
            ctrl.ApplyMoveInput(new Vector2(1f, 0f));
            yield return null;

            Assert.AreEqual(startY, _go.transform.position.y, 1e-3, "Root Y must remain stable");
            Assert.AreNotEqual(startX, _go.transform.position.x, "Root X must change");
        }

        [UnityTest]
        public IEnumerator AnimatorWithRootMotion_IsDisabledByGuard()
        {
            _go = new GameObject("CharWithAnim");
            var anim = _go.AddComponent<Animator>();
            anim.applyRootMotion = true;

            CharacterRuntimeComponentGuard.Ensure(_go);
            yield return null;

            Assert.IsFalse(anim.applyRootMotion, "Guard must turn off applyRootMotion");
        }

        [UnityTest]
        public IEnumerator FullCombatLoop_KeepsRootYStable()
        {
            _go = CreateCharacterWithVisual(new Vector3(0f, 0.5f, 0f), "Player", SubFactionId.MotorIronRiders);
            _go2 = CreateCharacterWithVisual(new Vector3(0.6f, 0.5f, 0f), "Enemy", SubFactionId.BeastIronClaw);

            var combatant = _go.GetComponent<Combatant>();
            var enemyCombatant = _go2.GetComponent<Combatant>();
            yield return null;

            float startY = _go.transform.position.y;

            // Attack
            combatant.TryLightAttack(enemyCombatant);
            for (int i = 0; i < 30; i++) yield return null;
            Assert.AreEqual(startY, _go.transform.position.y, 1e-3, "Root Y must remain stable during attack sequence");

            // Wait for attack to fully resolve
            for (int i = 0; i < 10; i++) yield return null;

            // Dodge
            combatant.TryDodge(Vector3.right);
            for (int i = 0; i < 20; i++) yield return null;
            Assert.AreEqual(startY, _go.transform.position.y, 1e-3, "Root Y must remain stable during dodge");
        }

        [UnityTest]
        public IEnumerator AIFallback_MovesRoot_KeepsY()
        {
            _go = CreateCharacterWithVisual(new Vector3(0f, 0.5f, 0f), "AI", SubFactionId.BeastIronClaw);
            var bridge = _go.GetComponent<NavigationAgentBridge>();
            if (bridge == null) bridge = _go.AddComponent<NavigationAgentBridge>();

            yield return null;

            bridge.SetDestination(NavigationMoveRequest.To(new Vector3(5f, 0.5f, 0f), 6f, 0.5f));
            float startY = _go.transform.position.y;

            for (int i = 0; i < 30; i++)
            {
                bridge.TickFallback(0.1f);
                yield return null;
            }

            Assert.Greater(_go.transform.position.x, 1f, "AI fallback must move root X");
            Assert.AreEqual(startY, _go.transform.position.y, 1e-3, "AI fallback must not change Y");
        }

        [UnityTest]
        public IEnumerator MultipleSpawns_AllHaveMotor()
        {
            var spawns = new GameObject[5];
            for (int i = 0; i < 5; i++)
            {
                spawns[i] = CreateCharacterWithVisual(new Vector3(i * 2f, 0.5f, 0f), $"Spawn{i}", SubFactionId.MotorIronRiders);
            }

            yield return null;

            try
            {
                foreach (var s in spawns)
                {
                    Assert.IsNotNull(s.GetComponent<CharacterMovementMotor>(), $"{s.name} must have motor");
                    Assert.IsNotNull(s.GetComponent<Rigidbody>(), $"{s.name} must have Rigidbody");
                    Assert.IsNotNull(s.GetComponentInChildren<Collider>(), $"{s.name} must have Collider");
                }
            }
            finally
            {
                foreach (var s in spawns)
                    if (s != null) Object.DestroyImmediate(s);
            }
        }
    }
}
