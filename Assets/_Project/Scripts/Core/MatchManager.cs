using System;
using UnityEngine;

namespace ProjectMaidan.Core
{
    public enum MatchState { Loading, PreMatch, InMatch, PostMatch }

    public class MatchResult
    {
        public string WinnerId;     // "player", "ai", or "draw"
        public int PlayerFinalScore;
        public int OpponentFinalScore;
        public string WinCondition; // "score", "hp-tiebreak", "draw"
    }

    /// <summary>
    /// Owns the match state machine and coordinates all match-scoped systems.
    /// See GDD Section 7 and YouTrack MAI-31, MAI-34.
    /// </summary>
    public class MatchManager : MonoBehaviour
    {
        public static MatchManager Instance { get; private set; }
        public static event Action<MatchState> OnMatchStateChanged;
        public static event Action<MatchResult> OnMatchEnded;

        public MatchState CurrentState { get; private set; } = MatchState.Loading;
        public int PlayerScore => _territorySystem != null ? _territorySystem.PlayerScore : 0;
        public int OpponentScore => _territorySystem != null ? _territorySystem.OpponentScore : 0;
        public int WinThreshold => _config != null ? _config.WinThreshold : 120;

        [Header("Config")]
        [SerializeField] private MatchConfig _config;
        [SerializeField] private TerritorySystem _territorySystem;

        // TODO (MAI-31): Implement full state machine transitions
        // TODO (MAI-32): Wire match timer here
        // TODO (MAI-34): Implement win condition evaluation

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void Configure(MatchConfig config, TerritorySystem territorySystem)
        {
            _config = config;
            _territorySystem = territorySystem;
        }

        public void TransitionTo(MatchState newState)
        {
            CurrentState = newState;
            OnMatchStateChanged?.Invoke(newState);
            Debug.Log($"[MatchManager] State → {newState}");
        }

        public void EndMatch(MatchResult result)
        {
            if (CurrentState == MatchState.PostMatch)
            {
                return;
            }

            TransitionTo(MatchState.PostMatch);
            OnMatchEnded?.Invoke(result);
        }
    }
}
