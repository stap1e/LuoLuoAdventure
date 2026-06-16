using System;
using System.Collections.Generic;
using LuoLuoTrip.Combat;
using UnityEngine;

namespace LuoLuoTrip
{
    public class TutorialFlowRuntime : MonoBehaviour
    {
        private enum TutorialStep
        {
            Welcome,
            Movement,
            LockOn,
            Attack,
            Dodge,
            CommanderSelect,
            CommanderControl,
            CommanderRelease,
            Complete
        }

        [SerializeField] private Vector2 _position = new Vector2(Screen.width / 2 - 200, 30);
        [SerializeField] private int _width = 400;

        private TutorialStep _currentStep = TutorialStep.Welcome;
        private CombatController _combatController;
        private CommanderControlController _commanderController;
        private bool _movementDetected;
        private bool _lockOnDetected;
        private bool _attackDetected;
        private bool _dodgeDetected;
        private bool _commanderSelectDetected;
        private bool _commanderControlDetected;
        private bool _commanderReleaseDetected;

        public void Initialize(CombatController combat, CommanderControlController commander)
        {
            _combatController = combat;
            _commanderController = commander;
        }

        public void ResetTutorial()
        {
            _currentStep = TutorialStep.Welcome;
            _movementDetected = false;
            _lockOnDetected = false;
            _attackDetected = false;
            _dodgeDetected = false;
            _commanderSelectDetected = false;
            _commanderControlDetected = false;
            _commanderReleaseDetected = false;
        }

        private void Update()
        {
            DetectInput();
            AdvanceStep();
        }

        private void DetectInput()
        {
            if (Input.GetAxisRaw("Horizontal") != 0f || Input.GetAxisRaw("Vertical") != 0f)
                _movementDetected = true;

            if (Input.GetKeyDown(KeyCode.Q))
                _lockOnDetected = true;

            if (Input.GetKeyDown(KeyCode.Mouse0))
                _attackDetected = true;

            if (Input.GetKeyDown(KeyCode.Space))
                _dodgeDetected = true;

            if (Input.GetKeyDown(KeyCode.Tab))
                _commanderSelectDetected = true;

            if (Input.GetKeyDown(KeyCode.E))
                _commanderControlDetected = true;

            if (Input.GetKeyDown(KeyCode.R))
                _commanderReleaseDetected = true;
        }

        private void AdvanceStep()
        {
            _currentStep = _currentStep switch
            {
                TutorialStep.Welcome => Input.anyKeyDown ? TutorialStep.Movement : _currentStep,
                TutorialStep.Movement => _movementDetected ? TutorialStep.LockOn : _currentStep,
                TutorialStep.LockOn => _lockOnDetected ? TutorialStep.Attack : _currentStep,
                TutorialStep.Attack => _attackDetected ? TutorialStep.Dodge : _currentStep,
                TutorialStep.Dodge => _dodgeDetected ? TutorialStep.CommanderSelect : _currentStep,
                TutorialStep.CommanderSelect => _commanderSelectDetected ? TutorialStep.CommanderControl : _currentStep,
                TutorialStep.CommanderControl => _commanderControlDetected ? TutorialStep.CommanderRelease : _currentStep,
                TutorialStep.CommanderRelease => _commanderReleaseDetected ? TutorialStep.Complete : _currentStep,
                _ => _currentStep
            };
        }

        private void OnGUI()
        {
            var text = GetStepText();
            if (string.IsNullOrEmpty(text)) return;

            GUI.Box(new Rect(_position.x - 8, _position.y - 4, _width + 16, 60), "");
            var stepIndex = (int)_currentStep;
            var totalSteps = (int)TutorialStep.Complete;
            GUI.Label(new Rect(_position.x, _position.y, _width, 18),
                $"Tutorial [{stepIndex}/{totalSteps}]");
            GUI.Label(new Rect(_position.x, _position.y + 20, _width, 36), text);
        }

        private string GetStepText()
        {
            return _currentStep switch
            {
                TutorialStep.Welcome => "Welcome! Press any key to start the tutorial.",
                TutorialStep.Movement => "Use WASD to move your character.",
                TutorialStep.LockOn => "Press Q to lock onto an enemy.",
                TutorialStep.Attack => "Press Left Mouse Button to attack.",
                TutorialStep.Dodge => "Press Space to dodge. You are invulnerable during dodge!",
                TutorialStep.CommanderSelect => "Press Tab to select a commander target.",
                TutorialStep.CommanderControl => "Press E to attempt control of the selected target.",
                TutorialStep.CommanderRelease => "Press R to release control.",
                TutorialStep.Complete => "Tutorial complete! You are ready for battle.",
                _ => ""
            };
        }
    }
}
