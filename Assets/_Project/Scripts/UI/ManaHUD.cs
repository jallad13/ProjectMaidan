using ProjectMaidan.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectMaidan.UI
{
    public sealed class ManaHUD : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Image _fillImage;
        [SerializeField] private TMP_Text _manaText;
        [SerializeField] private CanvasGroup _pulseGroup;

        [Header("Colors")]
        [SerializeField] private Color _emptyColor = new(0.08f, 0.12f, 0.2f, 1f);
        [SerializeField] private Color _fullColor = new(0.65f, 0.95f, 1f, 1f);

        [Header("Comeback Pulse")]
        [SerializeField, Range(0.1f, 1f)] private float _minimumPulseAlpha = 0.7f;
        [SerializeField, Min(0.1f)] private float _pulseFrequency = 1f;

        private bool _isPulsing;

        public float FillAmount => _fillImage != null ? _fillImage.fillAmount : 0f;
        public Color FillColor => _fillImage != null ? _fillImage.color : Color.clear;
        public string ManaText => _manaText != null ? _manaText.text : string.Empty;
        public bool IsPulsing => _isPulsing;
        public float PulseAlpha => _pulseGroup != null ? _pulseGroup.alpha : 1f;

        private void OnEnable()
        {
            ManaSystem.OnManaChanged += HandleManaChanged;
            ManaSystem.OnComebackChanged += HandleComebackChanged;
        }

        private void Start()
        {
            Refresh();
        }

        private void Update()
        {
            if (_pulseGroup == null)
            {
                return;
            }

            if (!_isPulsing)
            {
                _pulseGroup.alpha = 1f;
                return;
            }

            float pulse = (Mathf.Sin(Time.unscaledTime * Mathf.PI * 2f * _pulseFrequency) + 1f) * 0.5f;
            _pulseGroup.alpha = Mathf.Lerp(_minimumPulseAlpha, 1f, pulse);
        }

        private void OnDisable()
        {
            ManaSystem.OnManaChanged -= HandleManaChanged;
            ManaSystem.OnComebackChanged -= HandleComebackChanged;
            if (_pulseGroup != null)
            {
                _pulseGroup.alpha = 1f;
            }
        }

        public void Configure(Image fillImage, TMP_Text manaText, CanvasGroup pulseGroup)
        {
            _fillImage = fillImage;
            _manaText = manaText;
            _pulseGroup = pulseGroup;
            Refresh();
        }

        public void Refresh()
        {
            ManaSystem manaSystem = ManaSystem.Instance;
            if (manaSystem == null)
            {
                HandleManaChanged(0, 1);
                HandleComebackChanged(false);
                return;
            }

            HandleManaChanged(manaSystem.CurrentMana, manaSystem.MaxMana);
            HandleComebackChanged(manaSystem.IsComebackActive);
        }

        private void HandleManaChanged(int currentMana, int maxMana)
        {
            float normalizedMana = maxMana > 0
                ? Mathf.Clamp01((float)currentMana / maxMana)
                : 0f;

            if (_fillImage != null)
            {
                _fillImage.type = Image.Type.Filled;
                _fillImage.fillMethod = Image.FillMethod.Horizontal;
                _fillImage.fillOrigin = 0;
                _fillImage.fillAmount = normalizedMana;
                _fillImage.color = Color.Lerp(_emptyColor, _fullColor, normalizedMana);
            }

            if (_manaText != null)
            {
                _manaText.text = currentMana.ToString();
            }
        }

        private void HandleComebackChanged(bool active)
        {
            _isPulsing = active;
            if (!active && _pulseGroup != null)
            {
                _pulseGroup.alpha = 1f;
            }
        }
    }
}
