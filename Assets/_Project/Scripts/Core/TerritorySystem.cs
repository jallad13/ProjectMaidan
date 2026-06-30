using System;
using UnityEngine;

namespace ProjectMaidan.Core
{
    public enum TerritoryControl { PlayerA, PlayerB, Contested }

    public class TerritoryTick
    {
        public TerritoryControl Control;
        public int PlayerACount;
        public int PlayerBCount;
    }

    /// <summary>
    /// Manages the territory score system — the core win condition.
    /// Per-second zone check, score accumulation, win threshold evaluation.
    /// See GDD Section 5 and YouTrack MAI-25, MAI-26, MAI-27.
    /// </summary>
    public class TerritorySystem : MonoBehaviour
    {
        public static event Action<TerritoryTick> OnTerritoryTick;

        public int PlayerScore { get; private set; }
        public int OpponentScore { get; private set; }

        [Header("Config")]
        [SerializeField] private MatchConfig _config;

        // TODO (MAI-25): Implement per-second zone unit count via trigger collider
        // TODO (MAI-26): Accumulate score, fire OnMatchEnded when threshold reached
        // TODO (MAI-27): Drive territory line position via score differential

        private void FireTick(TerritoryControl control, int playerACount, int playerBCount)
        {
            OnTerritoryTick?.Invoke(new TerritoryTick
            {
                Control = control,
                PlayerACount = playerACount,
                PlayerBCount = playerBCount
            });
        }
    }
}
