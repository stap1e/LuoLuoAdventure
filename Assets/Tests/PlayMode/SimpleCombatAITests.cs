using System.Collections;
using NUnit.Framework;
using LuoLuoTrip.Combat;
using UnityEngine;
using UnityEngine.TestTools;

namespace LuoLuoTrip.Tests.PlayMode
{
    public class SimpleCombatAITests
    {
        [UnityTest]
        public IEnumerator AI_IgnoresNonHostileTarget()
        {
            CharacterEntity.HostilityResolver = (source, target) =>
                target == SubFactionId.BeastShadowFang;

            var aiObject = CreateCombatant("ai", SubFactionId.MotorIronRiders, withAi: true, position: Vector3.zero);
            var friendlyObject = CreateCombatant("friendly", SubFactionId.BeastIronClaw, position: new Vector3(2f, 0f, 0f));
            var hostileObject = CreateCombatant("hostile", SubFactionId.BeastShadowFang, position: new Vector3(4f, 0f, 0f));

            try
            {
                var ai = aiObject.GetComponent<SimpleCombatAI>();
                ai.CombatantQuery = () => new[]
                {
                    aiObject.GetComponent<Combatant>(),
                    friendlyObject.GetComponent<Combatant>(),
                    hostileObject.GetComponent<Combatant>()
                };

                yield return null;

                Assert.That(ai.CurrentTarget, Is.Not.Null);
                Assert.That(ai.CurrentTarget, Is.EqualTo(hostileObject.GetComponent<Combatant>()));
            }
            finally
            {
                CharacterEntity.HostilityResolver = null;
                Object.Destroy(aiObject);
                Object.Destroy(friendlyObject);
                Object.Destroy(hostileObject);
            }
        }

        private static GameObject CreateCombatant(string id, SubFactionId faction, bool withAi = false, Vector3? position = null)
        {
            var gameObject = new GameObject(id);
            if (position.HasValue)
                gameObject.transform.position = position.Value;

            var entity = gameObject.AddComponent<CharacterEntity>();
            entity.Bind(new CharacterData(id, id, faction, CharacterRole.Common, 5));

            if (withAi)
                gameObject.AddComponent<SimpleCombatAI>();

            return gameObject;
        }
    }
}
