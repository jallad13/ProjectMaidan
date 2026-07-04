using System;
using System.Collections;
using ProjectMaidan.Units;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectMaidan.UI
{
    public class DeploymentCardUI : MonoBehaviour, IPointerClickHandler
    {
        public static event Action<DeploymentCardUI> OnCardTapped;
        public static event Action<DeploymentCardUI> OnUnaffordableCardTapped;

        [Header("Unit")]
        [SerializeField] private string _unitName;
        [SerializeField] private UnitRole _role;
        [SerializeField] private int _manaCost;
        [SerializeField] private float _cooldownSeconds = 15f;

        [Header("UI")]
        [SerializeField] private Button _button;
        [SerializeField] private Image _background;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _roleText;
        [SerializeField] private TMP_Text _manaText;
        [SerializeField] private TMP_Text _cooldownText;

        [Header("Placeholder Colors")]
        [SerializeField] private Color _availableColor = new(0.16f, 0.25f, 0.42f, 1f);
        [SerializeField] private Color _selectedColor = new(0.15f, 0.65f, 1f, 1f);
        [SerializeField] private Color _unavailableColor = new(0.2f, 0.2f, 0.24f, 0.8f);

        private bool _isSelected;
        private bool _isCoolingDown;
        private bool _isAffordable = true;
        private Vector3 _baseScale;
        private Coroutine _cooldownRoutine;

        public string UnitName => _unitName;
        public UnitRole Role => _role;
        public int ManaCost => _manaCost;
        public bool IsSelected => _isSelected;
        public bool IsCoolingDown => _isCoolingDown;
        public bool IsAffordable => _isAffordable;
        public bool IsAvailable => !_isCoolingDown && _isAffordable;

        private void Awake()
        {
            _baseScale = transform.localScale;
            if (_button != null)
            {
                _button.onClick.AddListener(HandleTap);
            }

            RefreshLabels();
            RefreshVisual();
        }

        private void OnDestroy()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(HandleTap);
            }
        }

        public void HandleTap()
        {
            if (!_isAffordable)
            {
                OnUnaffordableCardTapped?.Invoke(this);
                return;
            }

            if (IsAvailable)
            {
                OnCardTapped?.Invoke(this);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!_isAffordable && !_isCoolingDown)
            {
                OnUnaffordableCardTapped?.Invoke(this);
            }
        }

        public void SetSelected(bool selected)
        {
            _isSelected = selected && IsAvailable;
            RefreshVisual();
        }

        public void SetAffordable(bool affordable)
        {
            _isAffordable = affordable;
            if (!affordable)
            {
                _isSelected = false;
            }

            RefreshVisual();
        }

        public void StartCooldown()
        {
            if (_cooldownRoutine != null)
            {
                StopCoroutine(_cooldownRoutine);
            }

            _cooldownRoutine = StartCoroutine(CooldownRoutine());
        }

        private IEnumerator CooldownRoutine()
        {
            _isSelected = false;
            _isCoolingDown = true;
            RefreshVisual();

            float remaining = _cooldownSeconds;
            while (remaining > 0f)
            {
                if (_cooldownText != null)
                {
                    _cooldownText.text = $"{Mathf.CeilToInt(remaining)}s";
                }

                remaining -= Time.unscaledDeltaTime;
                yield return null;
            }

            _isCoolingDown = false;
            _cooldownRoutine = null;
            if (_cooldownText != null)
            {
                _cooldownText.text = string.Empty;
            }

            RefreshVisual();
        }

        private void RefreshLabels()
        {
            if (_nameText != null) _nameText.text = _unitName;
            if (_roleText != null) _roleText.text = _role.ToString();
            if (_manaText != null) _manaText.text = $"Mana {_manaCost}";
            if (_cooldownText != null) _cooldownText.text = string.Empty;
        }

        private void RefreshVisual()
        {
            if (_background != null)
            {
                _background.color = !IsAvailable ? _unavailableColor : (_isSelected ? _selectedColor : _availableColor);
            }

            if (_button != null)
            {
                _button.interactable = IsAvailable;
            }

            transform.localScale = _isSelected ? _baseScale * 1.1f : _baseScale;
            if (_cooldownText != null)
            {
                _cooldownText.enabled = _isCoolingDown;
            }
        }
    }
}
