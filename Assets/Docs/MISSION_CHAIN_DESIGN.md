# Mission Chain Design

## Current Mission Chain

```
ConvoyEnergyConflict → BorderRetaliation
```

## ConvoyEnergyConflict (Mission 1)

**Trigger**: Player enters MissionTriggerZone (12m radius)

**Objectives**:
1. Protect the convoy
2. Stop the beast raid
3. Share energy at node (optional)

**Outcomes and Consequences**:

| Outcome | Condition | XP | Faction Effect |
|---|---|---|---|
| MechaVictory | All beasts dead, convoy alive | 200 | Motor +Trust, Beast +Hostility |
| BeastVictory | Convoy destroyed or beast captures node | 200 | Beast +Trust, Motor +Hostility |
| BalancedResolution | Player shares energy, casualties <= 1 | 300 | All -Hostility |
| PartialSuccess | Player shares energy, casualties > 1 | 100 | Reduced effects |
| Failed | Player retreats 10s | 30 | All -Respect |

**Key Flags** (stored in MissionHistoryEntry):
- SharedEnergy: player pressed E at EnergyNode
- ConvoyDestroyed: convoy HP reached 0
- BeastRaidDefeated: MechaVictory outcome

## BorderRetaliation (Mission 2)

**Trigger**: Player enters BorderRetaliationTrigger zone (10m radius, positioned at x=25)

**Unlock Condition**: ConvoyEnergyConflict completed

**Branch Logic** based on MissionModifier from ConvoyEnergyConflict outcome:

### Case A: MechaVictory → Beast Retaliation
- BeastHostilityMultiplier = 1.5x
- Objectives: Defend outpost (60s), Repel beast raid
- MechaDefenseSuccess: Outpost held + beasts defeated
- PartialSuccess: High mecha casualties
- BeastAdvantage: Outpost lost

### Case B: BeastVictory → Mecha Distrust
- MechaSupportMultiplier = 0.5x
- MechaCaptainTacticalOnly = true
- Objectives: Recapture resource point
- RestoreTrust: Resource point captured
- MechaDistrustIncreases: Failed to recapture

### Case C: BalancedResolution → Ceasefire
- CeasefireActive = true
- InitialHostilityOffset = -15
- Objectives: Prevent ceasefire breakdown
- CeasefireStabilized: Low casualties completed
- CeasefireBroken: Any side casualties > 3

### Case D: Failed/PartialSuccess → Low Trust
- LowTrustMode = true
- InitialHostilityOffset = +10
- Objectives: Complete evacuation
- RecoverReputation: Evacuation completed
- CommanderAuthorityDamaged: Failed evacuation

## MissionChainState Save Data

Serialized in GameSaveData.missionChainState:
- CompletedMissions: List of MissionHistoryEntry
- ActiveMissionId: currently active mission
- UnlockedMissionIds: missions available to start
- NextSequenceIndex: auto-increment for history entries

Old saves (v2) missing missionChainState will use default empty state (convoy_energy_conflict unlocked by default).

## Commander XP / LevelUp Impact

| Level | XP Required | DirectControl Rank | TacticalCommand Rank | Sync Rate |
|---|---|---|---|---|
| 1 | 0 | 1 | 2 | 20% |
| 5 | 500 | 1 | 3 | 35% |
| 10 | 1500 | 2 | 4 | 50% |
| 20 | 5000 | 3 | 5 | 65% |
| 35 | 15000 | 4 | 5 | 80% |
| 45 | 30000 | 5 | 5 | 95% |

**Rank Control Verification**:
- Lv.1 Commander: Rank 1 DirectControl, Rank 2 TacticalCommand/SyncAssist, Rank 3 Denied
- Lv.5 Commander: Rank 2 TacticalCommand more stable
- Lv.10 Commander: Rank 2 DirectControl possible

## Units in CommanderPrototype Scene

| Unit | Rank | RequiredLevel | AllowDirectControl | AllowTacticalCommand | Role |
|---|---|---|---|---|---|
| MechaMinion | 1 | 1 | true | true | Common |
| BeastMinion | 1 | 1 | true | true | Common |
| CityLord | 4 | 35 | false | false | CityLord |
| WarKing | 5 | 45 | false | false | WarKing |
| MechaCaptain | 2 | 5 | false | true | Minion |
| BeastElite | 2 | 5 | false | true | Minion |
| DeputyCommander | 3 | 10 | false | false | CityLord |

## Manual Verification Flow

1. F10 clear save
2. Play ConvoyEnergyConflict → MechaVictory
3. Walk to BorderRetaliation trigger zone (x=25)
4. Verify beast units are more aggressive (BeastHostilityMultiplier 1.5x)
5. F10 clear save
6. Play ConvoyEnergyConflict → BeastVictory
7. Walk to BorderRetaliation trigger zone
8. Verify MechaCaptain only accepts TacticalCommand
9. F10 clear save
10. Play ConvoyEnergyConflict → BalancedResolution
11. Walk to BorderRetaliation trigger zone
12. Verify lower starting hostility
13. Verify Commander XP gain and level-up affects control permissions
14. F5/F9 to verify chain state persistence

## Limitations and Future Expansion

- Only 2 missions in chain currently
- BorderRetaliation branches are simplified (timer/casualty based)
- No NavMesh pathfinding for AI
- MissionResultSummaryPanel uses OnGUI (not Unity UI)
- No formal cinematic/story integration
- Future: add more missions, branching paths, commander special abilities per ControlMode
