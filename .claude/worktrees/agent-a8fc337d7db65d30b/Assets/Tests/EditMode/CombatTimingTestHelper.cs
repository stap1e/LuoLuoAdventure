using LuoLuoTrip.Combat;

namespace LuoLuoTrip.Tests.EditMode
{
    /// <summary>
    /// Shared helpers for ACTIVE_WINDOW combat tests.
    /// Combatant runs a Windup -> Active -> Recovery -> Cooldown sequence.
    /// Damage is resolved once when entering the Active window.
    /// AttackCooldown blocks TryLightAttack until cooldown timer reaches 0.
    /// Each Tick advances at most one state transition (UpdateStateTimer returns
    /// after a transition). To traverse the full sequence, multiple ticks are
    /// required.
    /// </summary>
    public static class CombatTimingTestHelper
    {
        /// <summary>
        /// Tick just past the windup boundary so Combatant transitions to Attacking
        /// and resolves its single hit on the cached target.
        /// </summary>
        public static void AdvanceCombatUntilActiveWindow(Combatant attacker)
        {
            if (attacker == null) return;
            attacker.Tick(attacker.AttackWindup + 0.001f);
        }

        /// <summary>
        /// Tick through the entire attack sequence: windup -> active -> recovery -> idle.
        /// UpdateStateTimer only advances one state per Tick call, so we tick four times.
        /// Each tick is sized to consume the current phase's duration plus a small slack.
        /// After this returns, Combatant.State == Idle and the stat-based attack cooldown
        /// has been counted down by the same total elapsed time.
        /// </summary>
        public static void AdvanceCombatThroughAttack(Combatant attacker, float extraSlack = 0.05f)
        {
            if (attacker == null) return;

            // Phase 1: finish whatever timer is currently active (windup if just attacked).
            attacker.Tick(attacker.AttackWindup + 0.001f);
            // Phase 2: finish active window.
            attacker.Tick(attacker.AttackActive + 0.001f);
            // Phase 3: finish recovery.
            attacker.Tick(attacker.AttackRecovery + 0.001f);
            // Phase 4: drain any remaining stat-based cooldown so TryLightAttack can fire again.
            attacker.Tick(attacker.Stats.attackCooldown + extraSlack);
        }
    }
}
