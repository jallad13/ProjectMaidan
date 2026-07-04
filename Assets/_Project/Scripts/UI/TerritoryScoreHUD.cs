using System.Collections;
using ProjectMaidan.Core;
using TMPro;
using UnityEngine;

namespace ProjectMaidan.UI
{
    public class TerritoryScoreHUD : MonoBehaviour
    {
        [SerializeField] private TerritorySystem _territorySystem;
        [SerializeField] private TMP_Text _playerScoreText;
        [SerializeField] private TMP_Text _opponentScoreText;

        public string PlayerScoreText => _playerScoreText != null ? _playerScoreText.text : string.Empty;
        public string OpponentScoreText => _opponentScoreText != null ? _opponentScoreText.text : string.Empty;
        public FontStyles PlayerStyle => _playerScoreText != null ? _playerScoreText.fontStyle : FontStyles.Normal;
        public FontStyles OpponentStyle => _opponentScoreText != null ? _opponentScoreText.fontStyle : FontStyles.Normal;
        public Vector3 PlayerScoreScale => _playerScoreText != null ? _playerScoreText.transform.localScale : Vector3.one;
        public Vector3 OpponentScoreScale => _opponentScoreText != null ? _opponentScoreText.transform.localScale : Vector3.one;

        private readonly Vector3 _defaultBaseScale = Vector3.one;
        private Vector3 _playerBaseScale = Vector3.one;
        private Vector3 _opponentBaseScale = Vector3.one;
        private Coroutine _playerPulseRoutine;
        private Coroutine _opponentPulseRoutine;

        private void Awake()
        {
            if (_territorySystem == null)
            {
                _territorySystem = FindFirstObjectByType<TerritorySystem>();
            }

            if (_playerScoreText != null)
            {
                _playerScoreText.fontSize = Mathf.Max(24f, _playerScoreText.fontSize);
            }

            if (_opponentScoreText != null)
            {
                _opponentScoreText.fontSize = Mathf.Max(24f, _opponentScoreText.fontSize);
            }

            CacheBaseScales();
        }

        private void OnEnable()
        {
            TerritorySystem.OnScoresChanged += HandleScoresChanged;
            TerritorySystem.OnTerritoryTick += HandleTerritoryTick;
        }

        private void Start()
        {
            Refresh();
        }

        private void OnDisable()
        {
            TerritorySystem.OnScoresChanged -= HandleScoresChanged;
            TerritorySystem.OnTerritoryTick -= HandleTerritoryTick;
            StopPulse(ref _playerPulseRoutine, _playerScoreText, _playerBaseScale);
            StopPulse(ref _opponentPulseRoutine, _opponentScoreText, _opponentBaseScale);
        }

        public void Configure(TerritorySystem territorySystem, TMP_Text playerText, TMP_Text opponentText)
        {
            _territorySystem = territorySystem;
            _playerScoreText = playerText;
            _opponentScoreText = opponentText;
            CacheBaseScales();
            Refresh();
        }

        public void Refresh()
        {
            if (_territorySystem != null)
            {
                HandleScoresChanged(_territorySystem.PlayerScore, _territorySystem.OpponentScore);
            }
        }

        private void HandleScoresChanged(int playerScore, int opponentScore)
        {
            if (_territorySystem == null || _playerScoreText == null || _opponentScoreText == null)
            {
                return;
            }

            int threshold = _territorySystem.WinThreshold;
            _playerScoreText.text = $"{playerScore} / {threshold}";
            _opponentScoreText.text = $"{opponentScore} / {threshold}";

            bool playerLeads = playerScore > opponentScore;
            bool opponentLeads = opponentScore > playerScore;
            _playerScoreText.fontStyle = playerLeads ? FontStyles.Bold : FontStyles.Normal;
            _opponentScoreText.fontStyle = opponentLeads ? FontStyles.Bold : FontStyles.Normal;
            _playerScoreText.color = playerLeads ? new Color(0.2f, 0.75f, 1f) : Color.white;
            _opponentScoreText.color = opponentLeads ? new Color(1f, 0.35f, 0.35f) : Color.white;
        }

        private void HandleTerritoryTick(TerritoryTick tick)
        {
            if (tick == null || tick.Control == TerritoryControl.Contested)
            {
                return;
            }

            if (tick.Control == TerritoryControl.PlayerA)
            {
                StartPlayerPulse();
            }
            else if (tick.Control == TerritoryControl.PlayerB)
            {
                StartOpponentPulse();
            }
        }

        private void StartPlayerPulse()
        {
            StopPulse(ref _playerPulseRoutine, _playerScoreText, _playerBaseScale);
            if (_playerScoreText != null)
            {
                _playerPulseRoutine = StartCoroutine(PulseRoutine(_playerScoreText, _playerBaseScale, true));
            }
        }

        private void StartOpponentPulse()
        {
            StopPulse(ref _opponentPulseRoutine, _opponentScoreText, _opponentBaseScale);
            if (_opponentScoreText != null)
            {
                _opponentPulseRoutine = StartCoroutine(PulseRoutine(_opponentScoreText, _opponentBaseScale, false));
            }
        }

        private IEnumerator PulseRoutine(TMP_Text text, Vector3 baseScale, bool player)
        {
            const float duration = 0.2f;
            const float peakScale = 1.15f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float normalized = Mathf.Clamp01(elapsed / duration);
                float ease = normalized <= 0.5f ? normalized * 2f : (1f - normalized) * 2f;
                if (text != null)
                {
                    text.transform.localScale = baseScale * Mathf.Lerp(1f, peakScale, ease);
                }

                yield return null;
            }

            if (text != null)
            {
                text.transform.localScale = baseScale;
            }

            if (player)
            {
                _playerPulseRoutine = null;
            }
            else
            {
                _opponentPulseRoutine = null;
            }
        }

        private void StopPulse(ref Coroutine routine, TMP_Text text, Vector3 baseScale)
        {
            if (routine != null)
            {
                StopCoroutine(routine);
                routine = null;
            }

            if (text != null)
            {
                text.transform.localScale = baseScale == Vector3.zero ? _defaultBaseScale : baseScale;
            }
        }

        private void CacheBaseScales()
        {
            _playerBaseScale = _playerScoreText != null ? _playerScoreText.transform.localScale : Vector3.one;
            _opponentBaseScale = _opponentScoreText != null ? _opponentScoreText.transform.localScale : Vector3.one;
        }
    }
}
