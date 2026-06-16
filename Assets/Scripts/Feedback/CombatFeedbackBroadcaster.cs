using System.Collections.Generic;
using LuoLuoTrip.Audio;
using LuoLuoTrip.Combat;
using LuoLuoTrip.Combat.Feedback;
using LuoLuoTrip.UI;
using UnityEngine;

namespace LuoLuoTrip.Feedback
{
    /// <summary>
    /// Centralized feedback dispatcher. Subscribes to every Combatant's
    /// OnHitLanded / OnHitReceived / OnDeath, and pushes:
    /// - DamageNumberFeedback floating text
    /// - HitFlashFeedback Visual tint on defender
    /// - AudioFeedbackService events (already partially fired by Combatant)
    /// - Hit log toast
    ///
    /// CombatHitFeedbackHub still owns hit-stop + camera shake.
    /// This broadcaster is additive and idempotent (won't double-subscribe).
    /// </summary>
    public class CombatFeedbackBroadcaster : MonoBehaviour
    {
        public struct HitLogEntry
        {
            public float Time;
            public string Text;
        }

        private static CombatFeedbackBroadcaster _instance;
        public static CombatFeedbackBroadcaster Instance => _instance;

        [SerializeField] private bool _showHitLog = true;
        [SerializeField] private int _hitLogCapacity = 5;
        [SerializeField] private float _hitLogLifetime = 3f;

        private readonly HashSet<Combatant> _registered = new HashSet<Combatant>();
        private readonly List<HitLogEntry> _hitLog = new List<HitLogEntry>();

        public IReadOnlyList<HitLogEntry> HitLog => _hitLog;
        public int RegisteredCount => _registered.Count;

        public static CombatFeedbackBroadcaster EnsureInstance()
        {
            if (_instance != null) return _instance;
            var go = new GameObject("[CombatFeedbackBroadcaster]");
            _instance = go.AddComponent<CombatFeedbackBroadcaster>();
            return _instance;
        }

        public void Register(Combatant c)
        {
            if (c == null || !_registered.Add(c)) return;
            c.OnHitLanded += HandleHitLanded;
            c.OnHitReceived += HandleHitReceived;
            c.OnDeath += HandleDeath;
        }

        public void Unregister(Combatant c)
        {
            if (c == null || !_registered.Remove(c)) return;
            c.OnHitLanded -= HandleHitLanded;
            c.OnHitReceived -= HandleHitReceived;
            c.OnDeath -= HandleDeath;
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this);
                return;
            }
            _instance = this;
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        private void Start()
        {
            // Catch any Combatants spawned before us.
            foreach (var c in FindObjectsOfType<Combatant>())
                Register(c);
            // Ensure DamageNumberFeedback exists.
            if (DamageNumberFeedback.Instance == null)
                DamageNumberFeedback.EnsureInstance();
        }

        private void HandleHitLanded(CombatHitEvent hit)
        {
            // Attacker side: spawn damage number above defender.
            if (hit.Defender == null) return;

            var pos = hit.Defender.transform.position;
            if (hit.Result.finalDamage <= 0f)
            {
                DamageNumberFeedback.Push(pos, 0f, DamageNumberFeedback.DamageKind.Miss);
                AppendLog($"{NameOf(hit.Attacker)} -> {NameOf(hit.Defender)}  MISS");
                return;
            }

            DamageNumberFeedback.Push(pos, hit.Result.finalDamage, DamageNumberFeedback.DamageKind.Damage);
            if (hit.Result.wasPoiseBroken && !hit.Result.wasFatal)
                DamageNumberFeedback.Push(pos + Vector3.up * 0.4f, 0f, DamageNumberFeedback.DamageKind.Stagger);

            AppendLog($"{NameOf(hit.Attacker)} -> {NameOf(hit.Defender)}  -{Mathf.RoundToInt(hit.Result.finalDamage)}");
        }

        private void HandleHitReceived(CombatHitEvent hit)
        {
            if (hit.Defender == null) return;
            var flash = hit.Defender.GetComponent<HitFlashFeedback>();
            if (flash != null && hit.Result.finalDamage > 0f)
                flash.PlayHitFlash();
        }

        private void HandleDeath(Combatant c)
        {
            if (c == null) return;
            DamageNumberFeedback.Push(c.transform.position, 0f, DamageNumberFeedback.DamageKind.Dead);
            var flash = c.GetComponent<HitFlashFeedback>();
            if (flash != null) flash.PlayDeathFlash();
            AppendLog($"{NameOf(c)} died");
            AudioFeedbackService.Play(AudioEventId.Stagger, c.transform.position);
        }

        private static string NameOf(Combatant c)
        {
            if (c == null) return "?";
            if (c.CharacterEntity != null && c.CharacterEntity.Data != null)
                return c.CharacterEntity.Data.DisplayName;
            return c.gameObject.name;
        }

        private void AppendLog(string text)
        {
            if (!_showHitLog) return;
            _hitLog.Add(new HitLogEntry { Time = Time.unscaledTime, Text = text });
            if (_hitLog.Count > _hitLogCapacity) _hitLog.RemoveAt(0);
        }

        private void Update()
        {
            if (_hitLog.Count == 0) return;
            var now = Time.unscaledTime;
            for (int i = _hitLog.Count - 1; i >= 0; i--)
            {
                if (now - _hitLog[i].Time > _hitLogLifetime) _hitLog.RemoveAt(i);
            }
        }

        private void OnGUI()
        {
            if (!_showHitLog || _hitLog.Count == 0) return;
            var x = Screen.width - 320f;
            var y = Screen.height - 24f - _hitLog.Count * 18f;
            var style = new GUIStyle(GUI.skin.label) { fontSize = 12, alignment = TextAnchor.MiddleLeft };
            var box = new Rect(x, y - 4f, 310f, _hitLog.Count * 18f + 8f);
            var prev = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.55f);
            GUI.DrawTexture(box, Texture2D.whiteTexture);
            GUI.color = Color.white;
            for (int i = 0; i < _hitLog.Count; i++)
            {
                GUI.Label(new Rect(x + 6f, y + i * 18f, 300f, 18f), _hitLog[i].Text, style);
            }
            GUI.color = prev;
        }
    }
}
