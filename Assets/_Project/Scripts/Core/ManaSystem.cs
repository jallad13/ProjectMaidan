using System;
using UnityEngine;

namespace ProjectMaidan.Core
{
    /// <summary>
    /// Independent player or opponent mana pool with comeback regeneration.
    /// See GDD Section 6 and YouTrack MAI-29, MAI-30.
    /// </summary>
    public class ManaSystem : MonoBehaviour
    {
        public static ManaSystem Instance { get; private set; }
        public static ManaSystem OpponentInstance { get; private set; }

        public static event Action<int, int> OnManaChanged;
        public static event Action<bool> OnComebackChanged;

        public event Action<int, int> ManaChanged;
        public event Action<bool> ComebackChanged;

        [Header("Owner")]
        [SerializeField] private bool _isPlayerMana = true;

        [Header("Config")]
        [SerializeField] private MatchConfig _config;

        [Header("Debug — read-only")]
        [SerializeField] private bool _comebackActive;
        [SerializeField] private float _regenAccumulator;

        public bool IsPlayerMana => _isPlayerMana;
        public int CurrentMana { get; private set; }
        public int MaxMana => _config != null ? Mathf.Max(1, _config.MaxMana) : 0;
        public bool IsComebackActive => _comebackActive;
        public float CurrentRegenRate => _config != null
            ? Mathf.Max(0.01f, _comebackActive ? _config.ComebackRegenRate : _config.ManaRegenRate)
            : 0f;

        private void Awake()
        {
            ManaSystem registeredInstance = _isPlayerMana ? Instance : OpponentInstance;
            if (registeredInstance != null && registeredInstance != this)
            {
                Destroy(gameObject);
                return;
            }

            if (_isPlayerMana)
            {
                Instance = this;
            }
            else
            {
                OpponentInstance = this;
            }
        }

        private void OnEnable()
        {
            TerritorySystem.OnScoresChanged += HandleScoresChanged;
        }

        private void Start()
        {
            ResetMana();
        }

        private void Update()
        {
            if (_config == null || CurrentMana >= MaxMana)
            {
                return;
            }

            float rate = CurrentRegenRate;
            _regenAccumulator += Time.deltaTime;
            if (_regenAccumulator >= rate)
            {
                _regenAccumulator -= rate;
                AddMana(1);
            }
        }

        private void OnDisable()
        {
            TerritorySystem.OnScoresChanged -= HandleScoresChanged;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            if (OpponentInstance == this)
            {
                OpponentInstance = null;
            }
        }

        public bool CanAfford(int manaCost) => CurrentMana >= manaCost;

        public bool TryDeductMana(int cost)
        {
            if (cost <= 0 || !CanAfford(cost))
            {
                return false;
            }

            CurrentMana -= cost;
            NotifyManaChanged();
            return true;
        }

        public void AddMana(int amount)
        {
            int nextMana = Mathf.Clamp(CurrentMana + amount, 0, MaxMana);
            if (nextMana == CurrentMana)
            {
                return;
            }

            CurrentMana = nextMana;
            NotifyManaChanged();
        }

        public void Configure(MatchConfig config)
        {
            _config = config;
        }

        public void ResetMana()
        {
            CurrentMana = _config != null
                ? Mathf.Clamp(_config.StartingMana, 0, MaxMana)
                : 0;
            _regenAccumulator = 0f;
            SetComebackActive(false);
            NotifyManaChanged();
        }

        private void HandleScoresChanged(int playerScore, int opponentScore)
        {
            if (_config == null)
            {
                SetComebackActive(false);
                return;
            }

            int deficit = _isPlayerMana
                ? opponentScore - playerScore
                : playerScore - opponentScore;
            SetComebackActive(deficit >= Mathf.Max(1, _config.ComebackThreshold));
        }

        private void SetComebackActive(bool active)
        {
            if (_comebackActive == active)
            {
                return;
            }

            _comebackActive = active;
            ComebackChanged?.Invoke(active);
            if (_isPlayerMana)
            {
                OnComebackChanged?.Invoke(active);
            }
        }

        private void NotifyManaChanged()
        {
            ManaChanged?.Invoke(CurrentMana, MaxMana);
            if (_isPlayerMana)
            {
                OnManaChanged?.Invoke(CurrentMana, MaxMana);
            }
        }
    }
}
