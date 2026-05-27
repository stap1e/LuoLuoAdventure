using System;
using LuoLuoTrip.Combat;
using LuoLuoTrip.Combat.Animation;
using UnityEngine;

namespace LuoLuoTrip
{
    /// <summary>
    /// 场景中角色的 MonoBehaviour 桥接，绑定 CharacterData 到 GameObject。
    /// </summary>
    public class CharacterEntity : MonoBehaviour
    {
        [SerializeField] private CharacterData _data;
        [SerializeField] private bool _autoSetupCombat = true;
        [SerializeField] private bool _autoSetupAnimation = true;

        public static Func<SubFactionId, SubFactionId, bool> HostilityResolver { get; set; }

        public CharacterData Data => _data;
        public Combatant Combatant { get; private set; }
        public CombatAnimationDriver AnimationDriver { get; private set; }

        public void Bind(CharacterData data)
        {
            _data = data;
            gameObject.name = $"[{data.Role}] {data.DisplayName}";

            if (_autoSetupCombat)
                EnsureCombatant();

            if (_autoSetupAnimation)
                EnsureAnimationDriver();
        }

        public void EnsureAnimationDriver()
        {
            AnimationDriver = AnimationDriver ?? GetComponent<CombatAnimationDriver>();
            if (AnimationDriver == null)
                AnimationDriver = gameObject.AddComponent<CombatAnimationDriver>();
        }

        public void EnsureCombatant()
        {
            Combatant = Combatant ?? GetComponent<Combatant>();
            if (Combatant == null)
                Combatant = gameObject.AddComponent<Combatant>();
            Combatant.InitializeFromCharacter();
        }

        public bool IsHostileTo(CharacterEntity other)
        {
            if (other == null || _data == null || other._data == null) return false;
            if (_data.Faction == other._data.Faction) return false;

            if (HostilityResolver != null)
                return HostilityResolver(_data.Faction, other._data.Faction);

            return GameBootstrap.Context != null &&
                   GameBootstrap.Context.AreFactionsHostile(_data.Faction, other._data.Faction);
        }
    }
}
