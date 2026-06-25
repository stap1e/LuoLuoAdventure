using UnityEngine;

namespace LuoLuoTrip.UI
{
    public class DemoFlowHud : MonoBehaviour
    {
        [SerializeField] private bool _visible = true;
        [SerializeField] private bool _shortcutHelpVisible = true;
        [SerializeField] private DemoFlowManager _flowManager;

        public DemoFlowManager FlowManager => _flowManager;
        public bool ShortcutHelpVisible => _shortcutHelpVisible;

        public void SetFlowManager(DemoFlowManager manager)
        {
            _flowManager = manager;
        }

        public void SetShortcutHelpVisible(bool visible)
        {
            _shortcutHelpVisible = visible;
        }

        public void ToggleShortcutHelp()
        {
            _shortcutHelpVisible = !_shortcutHelpVisible;
        }

        public static string[] BuildShortcutHelpLines(bool compact)
        {
            if (compact)
            {
                return new[]
                {
                    "DEMO / DEBUG shortcuts (H toggle)",
                    "1/2/3/F7/F8: Mission debug + CityGate teleport",
                    "F5/F9/F10: Save / Load / Clear Save",
                    "G/F: DefendObjective / FocusFire",
                    "Tab/Q/E/R: Select / Control / Release",
                    "LMB/Space: Attack / Dodge"
                };
            }

            return new[]
            {
                "DEMO / DEBUG shortcuts (H toggle)",
                "1: Debug Mission 1 outcome | 2: Debug Mission 2 outcome | 3: Debug existing mission trigger",
                "F7: Debug CityGate BalancedMediation | F8: Teleport to CityGate",
                "F5: Save | F9: Load | F10: Clear Save",
                "G: DefendObjective | F: FocusFire",
                "Tab/Q: Select target | E: DirectControl / Command / Interact | R: Release control",
                "Left Click: Attack | Space: Dodge"
            };
        }

        private void Start()
        {
            if (_flowManager == null)
                _flowManager = FindObjectOfType<DemoFlowManager>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.H))
                ToggleShortcutHelp();
        }

        private void OnGUI()
        {
            if (!_visible) return;

            if (_flowManager == null)
                _flowManager = FindObjectOfType<DemoFlowManager>();

            DrawFlowPanel();
            if (_shortcutHelpVisible)
                DrawShortcutHelp();
        }

        private void DrawFlowPanel()
        {
            var layout = DebugUILayout.DemoFlow;
            var x = layout.x;
            var y = layout.y;
            var width = (int)layout.width;

            GUI.Box(new Rect(x - 4, y - 4, width + 8, layout.height), "");
            GUI.Label(new Rect(x, y, width, 18), "=== Demo Flow ===");
            y += 20;

            if (_flowManager == null)
            {
                GUI.Label(new Rect(x, y, width, 18), "Next: Mission 1: Convoy Energy Conflict");
                y += 18;
                GUI.Label(new Rect(x, y, width, 18), "Go to: Convoy Mission Area");
                y += 18;
                GUI.Label(new Rect(x, y, width + 120, 18), "Protect convoy, share energy, avoid casualties.");
            }
            else
            {
                _flowManager.RefreshFromMissionChain();
                GUI.Label(new Rect(x, y, width, 18), $"Next: {_flowManager.CurrentStepDisplayName}");
                y += 18;
                GUI.Label(new Rect(x, y, width, 18), $"Go to: {_flowManager.CurrentWorldTargetName}");
                y += 18;
                GUI.Label(new Rect(x, y, width + 140, 18), _flowManager.CurrentObjectiveHint);
            }

            y += 24;
            GUI.color = Color.cyan;
            GUI.Label(new Rect(x, y, width + 120, 18), _shortcutHelpVisible ? "H: Hide DEMO / DEBUG shortcut help" : "H: Show DEMO / DEBUG shortcut help");
            GUI.color = Color.white;
        }

        private void DrawShortcutHelp()
        {
            var layout = DebugUILayout.DemoShortcutHelp;
            var x = layout.x;
            var y = layout.y;
            var width = (int)layout.width;
            var lines = BuildShortcutHelpLines(DebugUILayout.IsCompact(Screen.width));

            GUI.Box(new Rect(x - 4, y - 4, width + 8, layout.height), "");
            GUI.color = Color.cyan;
            foreach (var line in lines)
            {
                GUI.Label(new Rect(x, y, width + 260, 18), line);
                y += 18;
            }
            GUI.color = Color.white;
        }
    }
}
