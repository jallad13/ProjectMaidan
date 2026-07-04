using System.Collections;
using ProjectMaidan.Core;
using TMPro;
using UnityEngine;

namespace ProjectMaidan.UI
{
    public class CountdownOverlay : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TMP_Text _countdownText;
        [SerializeField] private MatchManager _matchManager;
        [SerializeField] private float _goDuration = 0.5f;
        [SerializeField] private float _fadeDuration = 0.3f;

        private Coroutine _countdownRoutine;

        public string CurrentText => _countdownText != null ? _countdownText.text : string.Empty;
        public float Alpha => _canvasGroup != null ? _canvasGroup.alpha : 0f;
        public bool IsVisible => _canvasGroup != null && _canvasGroup.alpha > 0.01f;

        private void Awake()
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
            }

            if (_countdownText == null)
            {
                _countdownText = GetComponentInChildren<TMP_Text>(true);
            }

            if (_matchManager == null)
            {
                _matchManager = MatchManager.Instance ?? FindFirstObjectByType<MatchManager>();
            }

            HideImmediate();
        }

        private void OnEnable()
        {
            MatchManager.OnMatchStateChanged += HandleMatchStateChanged;
        }

        private void Start()
        {
            _matchManager ??= MatchManager.Instance ?? FindFirstObjectByType<MatchManager>();
            if (_matchManager != null && _matchManager.CurrentState == MatchState.PreMatch)
            {
                BeginCountdown();
            }
        }

        private void OnDisable()
        {
            MatchManager.OnMatchStateChanged -= HandleMatchStateChanged;
            CancelCountdown(resetTimeScale: true);
        }

        public void Configure(CanvasGroup canvasGroup, TMP_Text countdownText, MatchManager matchManager)
        {
            _canvasGroup = canvasGroup;
            _countdownText = countdownText;
            _matchManager = matchManager;
            HideImmediate();
        }

        private void HandleMatchStateChanged(MatchState state)
        {
            if (state == MatchState.PreMatch)
            {
                BeginCountdown();
                return;
            }

            if (state == MatchState.InMatch)
            {
                CancelCountdown(resetTimeScale: true);
                HideImmediate();
                return;
            }

            if (state == MatchState.PostMatch)
            {
                CancelCountdown(resetTimeScale: false);
                HideImmediate();
            }
        }

        private void BeginCountdown()
        {
            CancelCountdown(resetTimeScale: false);
            _countdownRoutine = StartCoroutine(CountdownRoutine());
        }

        private IEnumerator CountdownRoutine()
        {
            Time.timeScale = 0f;
            Show();

            int seconds = _matchManager != null ? _matchManager.CountdownSeconds : 3;
            for (int value = seconds; value >= 1; value--)
            {
                SetText(value.ToString());
                yield return new WaitForSecondsRealtime(1f);
            }

            SetText("GO!");
            yield return new WaitForSecondsRealtime(_goDuration);

            yield return FadeOutRoutine();
            Time.timeScale = 1f;
            _countdownRoutine = null;

            _matchManager ??= MatchManager.Instance ?? FindFirstObjectByType<MatchManager>();
            _matchManager?.TransitionTo(MatchState.InMatch);
        }

        private IEnumerator FadeOutRoutine()
        {
            if (_canvasGroup == null)
            {
                yield break;
            }

            float elapsed = 0f;
            float startAlpha = _canvasGroup.alpha;
            while (elapsed < _fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                _canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, Mathf.Clamp01(elapsed / _fadeDuration));
                yield return null;
            }

            HideImmediate();
        }

        private void Show()
        {
            if (_canvasGroup == null)
            {
                return;
            }

            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.interactable = true;
        }

        private void HideImmediate()
        {
            if (_canvasGroup == null)
            {
                return;
            }

            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;
        }

        private void SetText(string value)
        {
            if (_countdownText != null)
            {
                _countdownText.text = value;
            }
        }

        private void CancelCountdown(bool resetTimeScale)
        {
            if (_countdownRoutine != null)
            {
                StopCoroutine(_countdownRoutine);
                _countdownRoutine = null;
            }

            if (resetTimeScale)
            {
                Time.timeScale = 1f;
            }
        }
    }
}
