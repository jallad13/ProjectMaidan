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
        }

        private void OnEnable()
        {
            TerritorySystem.OnScoresChanged += HandleScoresChanged;
        }

        private void Start()
        {
            Refresh();
        }

        private void OnDisable()
        {
            TerritorySystem.OnScoresChanged -= HandleScoresChanged;
        }

        public void Configure(TerritorySystem territorySystem, TMP_Text playerText, TMP_Text opponentText)
        {
            _territorySystem = territorySystem;
            _playerScoreText = playerText;
            _opponentScoreText = opponentText;
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
    }
}
