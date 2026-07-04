using System.Collections;
using ProjectMaidan.UI;
using ProjectMaidan.Units;
using TMPro;
using UnityEngine;

namespace ProjectMaidan.Core
{
    public class DeploymentController : MonoBehaviour
    {
        [Header("Prototype Flow")]
        [SerializeField] private DeploymentZone[] _playerZones;
        [SerializeField] private DeploymentCardUI[] _cards;
        [SerializeField] private UnitSpawner _unitSpawner;
        [SerializeField] private TMP_Text _statusText;

        private DeploymentCardUI _selectedCard;
        private Coroutine _statusRoutine;

        public DeploymentCardUI SelectedCard => _selectedCard;

        private void OnEnable()
        {
            DeploymentCardUI.OnCardTapped += HandleCardTapped;
            DeploymentCardUI.OnUnaffordableCardTapped += HandleUnaffordableCardTapped;
            DeploymentZone.OnZoneTapped += HandleZoneTapped;
            ManaSystem.OnManaChanged += HandleManaChanged;
        }

        private void Start()
        {
            ManaSystem manaSystem = ManaSystem.Instance;
            HandleManaChanged(
                manaSystem != null ? manaSystem.CurrentMana : 0,
                manaSystem != null ? manaSystem.MaxMana : 0);

            SetZoneHighlights(false);
            if (_statusText != null)
            {
                _statusText.text = string.Empty;
            }
        }

        private void OnDisable()
        {
            DeploymentCardUI.OnCardTapped -= HandleCardTapped;
            DeploymentCardUI.OnUnaffordableCardTapped -= HandleUnaffordableCardTapped;
            DeploymentZone.OnZoneTapped -= HandleZoneTapped;
            ManaSystem.OnManaChanged -= HandleManaChanged;
        }

        private void HandleCardTapped(DeploymentCardUI card)
        {
            if (card == _selectedCard)
            {
                ClearSelection();
                return;
            }

            if (_selectedCard != null)
            {
                _selectedCard.SetSelected(false);
            }

            _selectedCard = card;
            _selectedCard.SetSelected(true);
            SetZoneHighlights(true);
        }

        private void HandleZoneTapped(DeploymentZone zone)
        {
            if (_selectedCard == null || zone == null || !zone.IsPlayerZone)
            {
                return;
            }

            if (UnitManager.Instance == null || !UnitManager.Instance.CanDeploy(true))
            {
                ShowStatus("Max units on field", 2f);
                return;
            }

            DeploymentCardUI selectedCard = _selectedCard;
            ManaSystem manaSystem = ManaSystem.Instance;
            if (manaSystem == null || !manaSystem.TryDeductMana(selectedCard.ManaCost))
            {
                ShowStatus("Not enough mana", 1.5f);
                return;
            }

            UnitBase spawnedUnit = _unitSpawner != null
                ? _unitSpawner.Spawn(selectedCard.Role, true, zone.transform.position)
                : null;

            if (spawnedUnit == null)
            {
                manaSystem.AddMana(selectedCard.ManaCost);
                ShowStatus("Unable to deploy unit", 2f);
                return;
            }

            _selectedCard = null;
            selectedCard.SetSelected(false);
            selectedCard.StartCooldown();
            SetZoneHighlights(false);
        }

        private void HandleUnaffordableCardTapped(DeploymentCardUI card)
        {
            if (card != null)
            {
                ShowStatus("Not enough mana", 1.5f);
            }
        }

        private void HandleManaChanged(int currentMana, int maxMana)
        {
            bool selectedCardBecameUnaffordable = false;
            foreach (DeploymentCardUI card in _cards)
            {
                if (card == null)
                {
                    continue;
                }

                bool affordable = card.ManaCost <= currentMana;
                card.SetAffordable(affordable);
                selectedCardBecameUnaffordable |= card == _selectedCard && !affordable;
            }

            if (selectedCardBecameUnaffordable)
            {
                ClearSelection();
            }
        }

        private void ClearSelection()
        {
            if (_selectedCard != null)
            {
                _selectedCard.SetSelected(false);
                _selectedCard = null;
            }

            SetZoneHighlights(false);
        }

        private void SetZoneHighlights(bool highlighted)
        {
            foreach (DeploymentZone zone in _playerZones)
            {
                zone?.SetHighlight(highlighted);
            }
        }

        private void ShowStatus(string message, float duration)
        {
            if (_statusText == null)
            {
                return;
            }

            if (_statusRoutine != null)
            {
                StopCoroutine(_statusRoutine);
            }

            _statusRoutine = StartCoroutine(StatusRoutine(message, duration));
        }

        private IEnumerator StatusRoutine(string message, float duration)
        {
            _statusText.text = message;
            yield return new WaitForSecondsRealtime(duration);
            _statusText.text = string.Empty;
            _statusRoutine = null;
        }
    }
}
