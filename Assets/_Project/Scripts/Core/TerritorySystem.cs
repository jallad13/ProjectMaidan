using System;
using System.Collections;
using UnityEngine;

namespace ProjectMaidan.Core
{
    public enum TerritoryControl
    {
        PlayerA,
        PlayerB,
        Contested
    }

    public sealed class TerritoryTick
    {
        public TerritoryControl Control;
        public int PlayerACount;
        public int PlayerBCount;
    }

    public class TerritorySystem : MonoBehaviour
    {
        public static event Action<TerritoryTick> OnTerritoryTick;
        public static event Action<int, int> OnScoresChanged;

        [Header("Config")]
        [SerializeField] private MatchConfig _config;
        [SerializeField] private CenterTerritoryZone _centerZone;
        [SerializeField] private MatchManager _matchManager;

        private Coroutine _tickRoutine;
        private bool _matchEnded;

        public int PlayerScore { get; private set; }
        public int OpponentScore { get; private set; }
        public int WinThreshold => _config != null ? Mathf.Max(1, _config.WinThreshold) : 120;
        public CenterTerritoryZone CenterZone => _centerZone;

        private void Awake()
        {
            if (_centerZone == null)
            {
                _centerZone = FindFirstObjectByType<CenterTerritoryZone>();
            }

            if (_matchManager == null)
            {
                _matchManager = FindFirstObjectByType<MatchManager>();
            }
        }

        private void OnEnable()
        {
            _tickRoutine = StartCoroutine(TickLoop());
        }

        private void OnDisable()
        {
            if (_tickRoutine != null)
            {
                StopCoroutine(_tickRoutine);
                _tickRoutine = null;
            }
        }

        public void Configure(MatchConfig config, CenterTerritoryZone centerZone, MatchManager matchManager)
        {
            _config = config;
            _centerZone = centerZone;
            _matchManager = matchManager;
        }

        public void ResetScores()
        {
            PlayerScore = 0;
            OpponentScore = 0;
            _matchEnded = false;
            OnScoresChanged?.Invoke(PlayerScore, OpponentScore);
        }

        public TerritoryTick EvaluateTick()
        {
            int playerCount = _centerZone != null ? _centerZone.PlayerCount : 0;
            int opponentCount = _centerZone != null ? _centerZone.OpponentCount : 0;
            TerritoryControl control = playerCount > opponentCount
                ? TerritoryControl.PlayerA
                : opponentCount > playerCount
                    ? TerritoryControl.PlayerB
                    : TerritoryControl.Contested;

            if (!_matchEnded)
            {
                if (control == TerritoryControl.PlayerA)
                {
                    PlayerScore++;
                }
                else if (control == TerritoryControl.PlayerB)
                {
                    OpponentScore++;
                }
            }

            var tick = new TerritoryTick
            {
                Control = control,
                PlayerACount = playerCount,
                PlayerBCount = opponentCount
            };

            OnTerritoryTick?.Invoke(tick);
            OnScoresChanged?.Invoke(PlayerScore, OpponentScore);

            if (!_matchEnded && (PlayerScore >= WinThreshold || OpponentScore >= WinThreshold))
            {
                _matchEnded = true;
                _matchManager?.EndMatch(new MatchResult
                {
                    WinnerId = PlayerScore >= WinThreshold ? "player" : "ai",
                    PlayerFinalScore = PlayerScore,
                    OpponentFinalScore = OpponentScore,
                    WinCondition = "score"
                });
            }

            return tick;
        }

        private IEnumerator TickLoop()
        {
            while (!_matchEnded)
            {
                float interval = _config != null
                    ? Mathf.Max(0.05f, _config.TerritoryCheckInterval)
                    : 1f;
                yield return new WaitForSeconds(interval);
                EvaluateTick();
            }

            _tickRoutine = null;
        }
    }
}
