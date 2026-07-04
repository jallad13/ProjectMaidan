using System.Collections;
using ProjectMaidan.Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ProjectMaidan.UI
{
    public class ResultScreen : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TMP_Text _outcomeText;
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private TMP_Text _winConditionText;
        [SerializeField] private Button _continueButton;
        [SerializeField] private float _fadeDuration = 0.5f;

        [Header("Outcome Colors")]
        [SerializeField] private Color _victoryColor = new(0.2f, 0.75f, 1f, 1f);
        [SerializeField] private Color _defeatColor = new(1f, 0.25f, 0.25f, 1f);
        [SerializeField] private Color _drawColor = new(0.7f, 0.7f, 0.7f, 1f);

        private Coroutine _fadeRoutine;

        public string OutcomeText => _outcomeText != null ? _outcomeText.text : string.Empty;
        public string ScoreText => _scoreText != null ? _scoreText.text : string.Empty;
        public string WinConditionText => _winConditionText != null ? _winConditionText.text : string.Empty;
        public bool ContinueInteractable => _continueButton != null && _continueButton.interactable;
        public float Alpha => _canvasGroup != null ? _canvasGroup.alpha : 0f;

        private void Awake()
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
            }

            if (_continueButton != null)
            {
                _continueButton.onClick.AddListener(HandleContinue);
            }

            HideImmediate();
        }

        private void OnEnable()
        {
            MatchManager.OnMatchEnded += HandleMatchEnded;
        }

        private void OnDisable()
        {
            MatchManager.OnMatchEnded -= HandleMatchEnded;
            if (_fadeRoutine != null)
            {
                StopCoroutine(_fadeRoutine);
                _fadeRoutine = null;
            }
        }

        private void OnDestroy()
        {
            if (_continueButton != null)
            {
                _continueButton.onClick.RemoveListener(HandleContinue);
            }
        }

        public void Configure(
            CanvasGroup canvasGroup,
            TMP_Text outcomeText,
            TMP_Text scoreText,
            TMP_Text winConditionText,
            Button continueButton)
        {
            _canvasGroup = canvasGroup;
            _outcomeText = outcomeText;
            _scoreText = scoreText;
            _winConditionText = winConditionText;
            _continueButton = continueButton;
            if (_continueButton != null)
            {
                _continueButton.onClick.RemoveListener(HandleContinue);
                _continueButton.onClick.AddListener(HandleContinue);
            }

            HideImmediate();
        }

        private void HandleMatchEnded(MatchResult result)
        {
            if (result == null)
            {
                return;
            }

            SetResultText(result);
            if (_fadeRoutine != null)
            {
                StopCoroutine(_fadeRoutine);
            }

            _fadeRoutine = StartCoroutine(FadeInRoutine());
        }

        private void SetResultText(MatchResult result)
        {
            string winnerId = result.WinnerId ?? string.Empty;
            if (_outcomeText != null)
            {
                if (winnerId == "player")
                {
                    _outcomeText.text = "VICTORY";
                    _outcomeText.color = _victoryColor;
                }
                else if (winnerId == "ai")
                {
                    _outcomeText.text = "DEFEAT";
                    _outcomeText.color = _defeatColor;
                }
                else
                {
                    _outcomeText.text = "DRAW";
                    _outcomeText.color = _drawColor;
                }
            }

            if (_scoreText != null)
            {
                _scoreText.text = $"{result.PlayerFinalScore} vs {result.OpponentFinalScore}";
            }

            if (_winConditionText != null)
            {
                _winConditionText.text = FormatWinCondition(result.WinCondition);
            }
        }

        private IEnumerator FadeInRoutine()
        {
            if (_canvasGroup == null)
            {
                yield break;
            }

            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.interactable = true;
            if (_continueButton != null)
            {
                _continueButton.interactable = false;
            }

            float elapsed = 0f;
            while (elapsed < _fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                _canvasGroup.alpha = Mathf.Clamp01(elapsed / _fadeDuration);
                yield return null;
            }

            _canvasGroup.alpha = 1f;
            if (_continueButton != null)
            {
                _continueButton.interactable = true;
            }

            _fadeRoutine = null;
        }

        private void HideImmediate()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.blocksRaycasts = false;
                _canvasGroup.interactable = false;
            }

            if (_continueButton != null)
            {
                _continueButton.interactable = false;
            }
        }

        private static string FormatWinCondition(string winCondition)
        {
            return winCondition switch
            {
                "score" => "Score Threshold",
                "draw" => "Draw",
                _ => "Time Expired"
            };
        }

        private static void HandleContinue()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }
    }
}
