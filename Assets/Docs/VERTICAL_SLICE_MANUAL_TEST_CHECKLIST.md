# Vertical Slice Manual Test Checklist

## Environment
- Unity 2022.3.62f3 LTS
- Build target: StandaloneWindows64

## Generation Steps
1. `LuoLuoTrip/Setup/Generate Placeholder Assets`
2. `LuoLuoTrip/Setup/Create Commander Mission Prototype Scene`
3. `LuoLuoTrip/Tools/Validation/Run Vertical Slice Validation`

## Basic Operations
- WASD: Move
- Left Click: Attack
- Space: Dodge
- Q: Lock-on
- Tab: Select next target in range
- E: Interact (control/command/assist target, or share energy at node)
- R: Release control back to original player unit
- 1/2/3: Debug mission triggers
- F5: Quick save
- F9: Quick load
- F10: Clear save

## Control Permission Verification
- [ ] Tab to select rank 1 mecha minion -> E -> DirectControl (camera follows controlled unit)
- [ ] Tab to select WarKing -> E -> Denied (reason shown on HUD)
- [ ] Tab to select minion -> E -> SyncAssist (3s buff shown on HUD, damage buff active)
- [ ] Tab to select minion -> E -> TacticalCommand (FollowPlayer shown on HUD)
- [ ] R releases control -> camera returns to original player

## Mission Verification
- [ ] Walk into MissionTriggerZone -> objectives appear on HUD
- [ ] Kill all beast units -> MechaVictory -> MissionResultDebugPanel shows outcome
- [ ] Let beasts reach EnergyNode (5s) -> BeastVictory
- [ ] Clear target (Tab), stand at EnergyNode, hold E with low casualties -> BalancedResolution
- [ ] Leave mission zone for 10s -> Failed
- [ ] No duplicate completion on same mission

## Dynamic Hostility Verification
- [ ] Complete MechaVictory -> beast faction hostility increases -> beast units more aggressive
- [ ] Complete BalancedResolution -> hostility decreases -> less aggressive
- [ ] Check FactionStandingDebugPanel for standing changes

## Save/Load Verification
- [ ] F5 saves -> Console shows Commander level, faction count, character count
- [ ] F9 loads -> Console shows restored state, save version
- [ ] After load, Commander level/XP matches pre-save
- [ ] After load, faction standings match pre-save
- [ ] F10 clears save -> Console confirms

## Regression Testing
- [ ] CombatPrototype scene still runs correctly (WASD, attack, dodge, lock-on)
- [ ] EditMode tests pass in Test Runner
- [ ] PlayMode tests pass in Test Runner

## Known Limitations
- No production art (placeholder cylinders only)
- No complex pathfinding (SimpleCombatAI uses direct movement)
- No formal story/cutscenes
- Tests run via Unity Editor Test Runner only (no CLI)
- Camera follow is simple smooth follow (no cinematic transitions)
