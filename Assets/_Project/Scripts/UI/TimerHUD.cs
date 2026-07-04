using ProjectMaidan.Core;
using TMPro;
using UnityEngine;

namespace ProjectMaidan.UI
{
    public class TimerHUD : MonoBehaviour
    {
        [SerializeField] private TMP_Text _timerText;
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _finalSecondsColor = new(1f, 0.2f, 0.2f, 1f);

        public string TimerText => _timerText != null ? _timerText.text : string.Empty;
        public Color CurrentColor => _timerText != null ? _timerText.color : Color.clear;

        private void Awake()
        {
            if (_timerText == null)
            {
                _timerText = GetComponent<TMP_Text>();
            }

            if (_timerText != null)
            {
                _normalColor = _timerText.color;
            }
        }

        private void OnEnable()
        {
            MatchManager.OnTimerTick += HandleTimerTick;
        }

        private void Start()
        {
            if (MatchManager.Instance != null)
            {
                HandleTimerTick(MatchManager.Instance.TimeRemaining);
            }
        }

        private void OnDisable()
        {
            MatchManager.OnTimerTick -= HandleTimerTick;
        }

        public void Configure(TMP_Text timerText)
        {
            _timerText = timerText;
            if (_timerText != null)
            {
                _normalColor = _timerText.color;
            }
        }

        private void HandleTimerTick(float secondsRemaining)
        {
            if (_timerText == null)
            {
                return;
            }

            int wholeSeconds = Mathf.Max(0, Mathf.FloorToInt(secondsRemaining));
            int minutes = wholeSeconds / 60;
            int seconds = wholeSeconds % 60;
            _timerText.text = $"{minutes}:{seconds:00}";
            _timerText.color = wholeSeconds <= 10 ? _finalSecondsColor : _normalColor;
        }
    }
}
