#if UNITY_EDITOR
using System.IO;
using LuoLuoTrip.Combat.Animation;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace LuoLuoTrip.Editor
{
    /// <summary>
    /// 一键生成与 AnimatorCombatBridge / CombatAnimatorConfigSO 对齐的战斗 Animator Controller。
    /// </summary>
    public static class CombatAnimatorControllerGenerator
    {
        public const string DefaultFolder = "Assets/Data/Animation";
        public const string DefaultClipsFolder = "Assets/Data/Animation/Clips";
        public const string DefaultControllerPath = "Assets/Data/Animation/CombatCharacter.controller";
        public const string DefaultConfigPath = "Assets/Data/Animation/CombatAnimatorConfig.asset";

        public struct GenerateResult
        {
            public AnimatorController Controller;
            public CombatAnimatorConfigSO Config;
            public string ControllerPath;
        }

        [MenuItem("LuoLuoTrip/Setup/Generate Combat Animator Controller")]
        public static void GenerateFromMenu()
        {
            var result = Generate(DefaultControllerPath);
            Selection.activeObject = result.Controller;
            Debug.Log($"[LuoLuoTrip] Animator Controller 已生成: {result.ControllerPath}");
        }

        public static GenerateResult Generate(string controllerPath = DefaultControllerPath, CombatAnimatorConfigSO config = null)
        {
            LuoLuoTripSetupMenu.EnsureFolderPublic(DefaultFolder);
            LuoLuoTripSetupMenu.EnsureFolderPublic(DefaultClipsFolder);

            config ??= LoadOrCreateConfig(DefaultConfigPath);

            if (File.Exists(controllerPath))
                AssetDatabase.DeleteAsset(controllerPath);

            var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            BuildController(controller, config);

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return new GenerateResult
            {
                Controller = controller,
                Config = config,
                ControllerPath = controllerPath
            };
        }

        public static CombatAnimatorConfigSO LoadOrCreateConfig(string path)
        {
            var config = AssetDatabase.LoadAssetAtPath<CombatAnimatorConfigSO>(path);
            if (config != null) return config;

            config = ScriptableObject.CreateInstance<CombatAnimatorConfigSO>();
            AssetDatabase.CreateAsset(config, path);
            return config;
        }

        private static void BuildController(AnimatorController controller, CombatAnimatorConfigSO config)
        {
            AddParameters(controller, config);

            var root = controller.layers[0].stateMachine;
            root.name = "Base Layer";

            var idle = CreateState(root, config.idleState, new Vector3(300f, 0f, 0f),
                CreatePlaceholderClip("Idle", 1f, loop: true));
            var move = CreateState(root, config.moveState, new Vector3(300f, 120f, 0f),
                CreatePlaceholderClip("Move", 0.6f, loop: true, bobAmount: 0.05f));
            var attack = CreateState(root, "Attack", new Vector3(550f, -60f, 0f),
                CreateAttackClip());
            var dodge = CreateState(root, "Dodge", new Vector3(550f, 60f, 0f),
                CreatePlaceholderClip("Dodge", 0.35f, loop: false, tiltZ: -12f));
            var stagger = CreateState(root, "Stagger", new Vector3(550f, 180f, 0f),
                CreatePlaceholderClip("Stagger", 1.2f, loop: false, tiltX: -15f));
            var hitLight = CreateState(root, "HitLight", new Vector3(550f, 240f, 0f),
                CreatePlaceholderClip("HitLight", 0.25f, loop: false, kickback: -0.15f));
            var hitHeavy = CreateState(root, "HitHeavy", new Vector3(550f, 300f, 0f),
                CreatePlaceholderClip("HitHeavy", 0.45f, loop: false, kickback: -0.3f));
            var death = CreateState(root, "Death", new Vector3(800f, 120f, 0f),
                CreatePlaceholderClip("Death", 0.8f, loop: false, tiltX: 90f));

            root.defaultState = idle;

            // Idle <-> Move
            AddFloatTransition(idle, move, config.moveSpeedParam, AnimatorConditionMode.Greater, 0.1f);
            AddFloatTransition(move, idle, config.moveSpeedParam, AnimatorConditionMode.Less, 0.1f);

            // Any State -> action states (combat triggers)
            AddAnyTriggerTransition(root, attack, config.attackTrigger, canTransitionToSelf: false);
            AddAnyTriggerTransition(root, dodge, config.dodgeTrigger, canTransitionToSelf: false);
            AddAnyTriggerTransition(root, stagger, config.staggerTrigger, canTransitionToSelf: false);
            AddAnyTriggerTransition(root, hitLight, config.hitLightTrigger, canTransitionToSelf: true);
            AddAnyTriggerTransition(root, hitHeavy, config.hitHeavyTrigger, canTransitionToSelf: true);

            // Death: trigger + IsDead guard on return transitions
            var deathTransition = root.AddAnyStateTransition(death);
            ConfigureTransition(deathTransition, 0.05f, hasExitTime: false, canTransitionToSelf: false);
            deathTransition.AddCondition(AnimatorConditionMode.If, 0f, config.deathTrigger);

            // Return to Idle after one-shot actions
            AddExitToIdle(attack, idle, config.isDeadBool);
            AddExitToIdle(dodge, idle, config.isDeadBool);
            AddExitToIdle(stagger, idle, config.isDeadBool);
            AddExitToIdle(hitLight, idle, config.isDeadBool);
            AddExitToIdle(hitHeavy, idle, config.isDeadBool);

            // Block transitions out of Death
            death.writeDefaultValues = false;
        }

        private static void AddParameters(AnimatorController controller, CombatAnimatorConfigSO config)
        {
            controller.AddParameter(config.moveSpeedParam, AnimatorControllerParameterType.Float);
            controller.AddParameter(config.attackTrigger, AnimatorControllerParameterType.Trigger);
            controller.AddParameter(config.dodgeTrigger, AnimatorControllerParameterType.Trigger);
            controller.AddParameter(config.staggerTrigger, AnimatorControllerParameterType.Trigger);
            controller.AddParameter(config.hitLightTrigger, AnimatorControllerParameterType.Trigger);
            controller.AddParameter(config.hitHeavyTrigger, AnimatorControllerParameterType.Trigger);
            controller.AddParameter(config.deathTrigger, AnimatorControllerParameterType.Trigger);
            controller.AddParameter(config.isDeadBool, AnimatorControllerParameterType.Bool);
        }

        private static AnimatorState CreateState(AnimatorStateMachine machine, string name, Vector3 position, Motion motion)
        {
            var state = machine.AddState(name, position);
            state.motion = motion;
            return state;
        }

        private static void AddFloatTransition(
            AnimatorState from, AnimatorState to, string param,
            AnimatorConditionMode mode, float threshold)
        {
            var t = from.AddTransition(to);
            ConfigureTransition(t, 0.1f, hasExitTime: false);
            t.AddCondition(mode, threshold, param);
        }

        private static void AddAnyTriggerTransition(
            AnimatorStateMachine root, AnimatorState target, string trigger, bool canTransitionToSelf)
        {
            var t = root.AddAnyStateTransition(target);
            ConfigureTransition(t, 0.05f, hasExitTime: false, canTransitionToSelf: canTransitionToSelf);
            t.AddCondition(AnimatorConditionMode.If, 0f, trigger);
        }

        private static void AddExitToIdle(AnimatorState from, AnimatorState idle, string isDeadBool)
        {
            var t = from.AddTransition(idle);
            ConfigureTransition(t, 0.1f, hasExitTime: true, exitTime: 0.85f);
            t.AddCondition(AnimatorConditionMode.IfNot, 0f, isDeadBool);
        }

        private static void ConfigureTransition(
            AnimatorStateTransition t,
            float duration,
            bool hasExitTime,
            float exitTime = 0.9f,
            bool canTransitionToSelf = false)
        {
            t.duration = duration;
            t.hasExitTime = hasExitTime;
            t.exitTime = exitTime;
            t.hasFixedDuration = true;
            t.canTransitionToSelf = canTransitionToSelf;
            t.interruptionSource = TransitionInterruptionSource.None;
        }

        private static AnimationClip CreatePlaceholderClip(
            string clipName,
            float duration,
            bool loop,
            float bobAmount = 0f,
            float tiltX = 0f,
            float tiltZ = 0f,
            float kickback = 0f)
        {
            var path = $"{DefaultClipsFolder}/{clipName}.anim";
            var existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (existing != null)
                AssetDatabase.DeleteAsset(path);

            var clip = new AnimationClip { name = clipName };

            if (bobAmount > 0f)
            {
                clip.SetCurve("", typeof(Transform), "localPosition.y",
                    AnimationCurve.Linear(0f, 0f, duration, bobAmount));
            }

            if (tiltX != 0f)
            {
                clip.SetCurve("", typeof(Transform), "localEulerAngles.x",
                    AnimationCurve.EaseInOut(0f, 0f, duration, tiltX));
            }

            if (tiltZ != 0f)
            {
                clip.SetCurve("", typeof(Transform), "localEulerAngles.z",
                    AnimationCurve.EaseInOut(0f, 0f, duration * 0.5f, tiltZ));
            }

            if (kickback != 0f)
            {
                clip.SetCurve("", typeof(Transform), "localPosition.z",
                    AnimationCurve.EaseInOut(0f, 0f, duration * 0.4f, kickback));
            }

            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            AssetDatabase.CreateAsset(clip, path);
            return clip;
        }

        private static AnimationClip CreateAttackClip()
        {
            var clip = CreatePlaceholderClip("Attack", 0.5f, loop: false, kickback: 0.25f);

            AnimationUtility.SetAnimationEvents(clip, new[]
            {
                new AnimationEvent
                {
                    time = 0.2f,
                    functionName = nameof(AnimatorCombatBridge.AnimEvent_AttackActive)
                },
                new AnimationEvent
                {
                    time = 0.45f,
                    functionName = nameof(AnimatorCombatBridge.AnimEvent_AttackEnd)
                }
            });

            EditorUtility.SetDirty(clip);
            return clip;
        }

        /// <summary>将生成的 Controller 与 Config 应用到选中对象</summary>
        [MenuItem("LuoLuoTrip/Setup/Apply Combat Animator To Selected")]
        public static void ApplyToSelected()
        {
            var go = Selection.activeGameObject;
            if (go == null)
            {
                Debug.LogWarning("[LuoLuoTrip] 请先选中一个 GameObject");
                return;
            }

            var result = Generate(DefaultControllerPath);

            var animator = go.GetComponent<Animator>();
            if (animator == null)
                animator = go.AddComponent<Animator>();

            animator.runtimeAnimatorController = result.Controller;

            var bridge = go.GetComponent<AnimatorCombatBridge>();
            if (bridge == null)
                bridge = go.AddComponent<AnimatorCombatBridge>();

            var so = new SerializedObject(bridge);
            so.FindProperty("_config").objectReferenceValue = result.Config;
            so.FindProperty("_animator").objectReferenceValue = animator;
            so.ApplyModifiedPropertiesWithoutUndo();

            var driver = go.GetComponent<CombatAnimationDriver>();
            if (driver != null)
            {
                var driverSo = new SerializedObject(driver);
                driverSo.FindProperty("_preferAnimatorBridge").boolValue = true;
                driverSo.FindProperty("_useProceduralFallback").boolValue = false;
                driverSo.ApplyModifiedPropertiesWithoutUndo();
            }

            // 移除程序化动画（若存在）
            var procedural = go.GetComponent<ProceduralCombatAnimator>();
            if (procedural != null)
                Object.DestroyImmediate(procedural);

            Debug.Log($"[LuoLuoTrip] 已将 Combat Animator 应用到: {go.name}");
        }
    }
}
#endif
