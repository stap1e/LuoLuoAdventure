using System.Collections;
using LuoLuoTrip.Combat;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace LuoLuoTrip.Tests.PlayMode
{
    public class CityGateDebugTeleportSmokeTests
    {
        [UnityTest]
        public IEnumerator TeleportPlayerToCityGateDisputeArea_MovesPlayerNearTrigger()
        {
            var player = new GameObject("Player");
            var trigger = new GameObject("CityGateDisputeTrigger");
            var debugGo = new GameObject("CommanderPrototypeDebug");
            try
            {
                trigger.transform.position = new Vector3(50f, 0f, 0f);

                var data = CharacterData.Create("player", "Player", SubFactionId.MotorIronRiders, CharacterRole.Common);
                var entity = player.AddComponent<CharacterEntity>();
                entity.Bind(data);
                player.AddComponent<Combatant>();
                var controller = player.AddComponent<CombatController>();
                controller.SetInputEnabled(true);
                player.AddComponent<CharacterMovementMotor>();

                var debug = debugGo.AddComponent<PrototypeDebugController>();
                yield return null;

                debug.TeleportPlayerToCityGateDisputeArea();

                Assert.That(Vector3.Distance(player.transform.position, new Vector3(50f, 0.5f, -4f)), Is.LessThan(0.25f));
            }
            finally
            {
                Object.DestroyImmediate(debugGo);
                Object.DestroyImmediate(trigger);
                Object.DestroyImmediate(player);
            }
        }
    }
}
