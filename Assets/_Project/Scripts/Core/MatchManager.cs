using System;
using System.Collections;
using System.Linq;
using ProjectMaidan.Units;
using UnityEngine;

namespace ProjectMaidan.Core
{
    public enum MatchState
    {
        Loading,
        PreMatch,
        InMatch,
        PostMatch
    }

    public class MatchResult
    {
        public string WinnerId;
        public int PlayerFinalScore;
        public int OpponentFinalScore;
        public string WinCondition;
    }

    /// <summary>
    /// Owns the match state machine and coordinates all match-scoped systems.
    /// See GDD Section 7 and YouTrack MAI-31 through MAI-35.
    /// </summary>
    public class MatchManager : MonoBehaviour
    {
        public static MatchManager Instance { get; private set; }
        public static event Action<MatchState> OnMatchStateChanged;
        public static event Action<MatchResult> OnMatchEnded;
        public static event Action<float> OnTimerTick;

        [Header("Config")]
        [SerializeField] private MatchConfig _config;

        [Header("Coordinated Systems")]
        [SerializeField] private TerritorySystem _territorySystem;
        [SerializeField] private ManaSystem _playerManaSystem;
        [SerializeField] private ManaSystem _opponentManaSystem;
        [SerializeField] private DeploymentZone[] _deploymentZones;
        [SerializeField] private AIController _aiController;

        public MatchState CurrentState { get; private set; } = MatchState.Loading;
        public int PlayerScore => _territorySystem != null ? _territorySystem.PlayerScore : 0;
        public int OpponentScore => _territorySystem != null ? _territorySystem.OpponentScore : 0;
        public int WinThreshold => _config != null ? _config.WinThreshold : 120;
        public int CountdownSeconds => _config != null ? Mathf.Max(1, _config.CountdownSeconds) : 3;
        public float TimeRemaining { get; private set; }

        private Coroutine _timerRoutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            ResolveSystems();
            TimeRemaining = GetMatchDuration();
            TransitionTo(MatchState.PreMatch);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
                Time.timeScale = 1f;
            }
        }

        public void Configure(MatchConfig config, TerritorySystem territorySystem)
        {
            _config = config;
            _territorySystem = territorySystem;
            TimeRemaining = GetMatchDuration();
        }

        public bool TransitionTo(MatchState newState)
        {
            if (newState == CurrentState)
            {
                return true;
            }

            if (!IsValidNextState(newState))
            {
                Debug.LogWarning($"[MatchManager] Rejected transition {CurrentState} -> {newState}");
                return false;
            }

            CurrentState = newState;
            ApplyState(newState);
            OnMatchStateChanged?.Invoke(newState);
            Debug.Log($"[MatchManager] State -> {newState}");
            return true;
        }

        public void EndMatch(MatchResult result)
        {
            if (CurrentState != MatchState.InMatch)
            {
                return;
            }

            if (result != null)
            {
                result.PlayerFinalScore = PlayerScore;
                result.OpponentFinalScore = OpponentScore;
            }

            if (TransitionTo(MatchState.PostMatch))
            {
                OnMatchEnded?.Invoke(result);
            }
        }

        public string EvaluateWinner()
        {
            if (PlayerScore > OpponentScore)
            {
                return "player";
            }

            if (OpponentScore > PlayerScore)
            {
                return "ai";
            }

            float playerHp = SumRemainingHp(true);
            float opponentHp = SumRemainingHp(false);
            if (playerHp > opponentHp)
            {
                return "player";
            }

            if (opponentHp > playerHp)
            {
                return "ai";
            }

            return "draw";
        }

        private bool IsValidNextState(MatchState newState)
        {
            return CurrentState switch
            {
                MatchState.Loading => newState == MatchState.PreMatch,
                MatchState.PreMatch => newState == MatchState.InMatch,
                MatchState.InMatch => newState == MatchState.PostMatch,
                _ => false
            };
        }

        private void ApplyState(MatchState state)
        {
            bool systemsActive = state == MatchState.InMatch;
            if (_territorySystem != null)
            {
                _territorySystem.enabled = systemsActive;
            }

            _playerManaSystem?.SetRegenEnabled(systemsActive);
            _opponentManaSystem?.SetRegenEnabled(systemsActive);

            foreach (DeploymentZone zone in _deploymentZones ?? Array.Empty<DeploymentZone>())
            {
                zone?.SetInteractionEnabled(systemsActive);
            }

            if (_aiController != null)
            {
                _aiController.enabled = systemsActive;
            }

            if (state == MatchState.InMatch)
            {
                Time.timeScale = 1f;
                StartTimer();
            }
            else if (state == MatchState.PostMatch)
            {
                StopTimer();
                Time.timeScale = 0f;
            }
        }

        private void ResolveSystems()
        {
            if (_territorySystem == null)
            {
                _territorySystem = FindFirstObjectByType<TerritorySystem>();
            }

            ManaSystem[] manaSystems = FindObjectsByType<ManaSystem>(FindObjectsSortMode.None);
            _playerManaSystem ??= manaSystems.FirstOrDefault(system => system.IsPlayerMana);
            _opponentManaSystem ??= manaSystems.FirstOrDefault(system => !system.IsPlayerMana);

            if (_deploymentZones == null || _deploymentZones.Length == 0)
            {
                _deploymentZones = FindObjectsByType<DeploymentZone>(FindObjectsSortMode.None);
            }

            if (_aiController == null)
            {
                _aiController = FindFirstObjectByType<AIController>(FindObjectsInactive.Include);
            }
        }

        private void StartTimer()
        {
            StopTimer();
            TimeRemaining = GetMatchDuration();
            OnTimerTick?.Invoke(TimeRemaining);

            if (TimeRemaining <= 0f)
            {
                EndMatch(CreateTimerResult());
                return;
            }

            _timerRoutine = StartCoroutine(TimerRoutine());
        }

        private void StopTimer()
        {
            if (_timerRoutine != null)
            {
                StopCoroutine(_timerRoutine);
                _timerRoutine = null;
            }
        }

        private IEnumerator TimerRoutine()
        {
            while (TimeRemaining > 0f)
            {
                yield return new WaitForSeconds(1f);
                TimeRemaining = Mathf.Max(0f, TimeRemaining - 1f);
                OnTimerTick?.Invoke(TimeRemaining);
            }

            _timerRoutine = null;
            EndMatch(CreateTimerResult());
        }

        private float GetMatchDuration()
        {
            return _config != null ? Mathf.Max(0f, _config.MatchDuration) : 120f;
        }

        private MatchResult CreateResult(string winnerId, string winCondition)
        {
            return new MatchResult
            {
                WinnerId = winnerId,
                PlayerFinalScore = PlayerScore,
                OpponentFinalScore = OpponentScore,
                WinCondition = winCondition
            };
        }

        private MatchResult CreateTimerResult()
        {
            string winnerId = EvaluateWinner();
            return CreateResult(winnerId, winnerId == "draw" ? "draw" : "time");
        }

        private static float SumRemainingHp(bool isPlayer)
        {
            return UnitManager.Instance == null
                ? 0f
                : UnitManager.Instance.GetAllUnits()
                    .Where(unit => unit != null && unit.IsAlive && unit.IsPlayerUnit == isPlayer)
                    .Sum(unit => unit.HP);
        }
    }
}
