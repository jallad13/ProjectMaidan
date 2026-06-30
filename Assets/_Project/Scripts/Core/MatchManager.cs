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
        public static event Action<MatchState> OnMatchStateChanged;
        public static event Action<MatchResult> OnMatchEnded;

        public MatchState CurrentState { get; private set; } = MatchState.Loading;

        [Header("Config")]
        [SerializeField] private MatchConfig _config;

        // TODO (MAI-31): Implement full state machine transitions
        // TODO (MAI-32): Wire match timer here
        // TODO (MAI-34): Implement win condition evaluation

        public void TransitionTo(MatchState newState)
        {
            CurrentState = newState;
            OnMatchStateChanged?.Invoke(newState);
            Debug.Log($"[MatchManager] State → {newState}");
        }

        public void EndMatch(MatchResult result)
        {
            TransitionTo(MatchState.PostMatch);
            OnMatchEnded?.Invoke(result);
        }
    }
}
