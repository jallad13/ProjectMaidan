using System.Collections;
using ProjectMaidan.UI;
using ProjectMaidan.Units;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectMaidan.Core
{
    public class DeploymentController : MonoBehaviour
    {
        [Header("Prototype Flow")]
        [SerializeField] private int _startingMana = 10;
        [SerializeField] private DeploymentZone[] _playerZones;
        [SerializeField] private DeploymentCardUI[] _cards;
        [SerializeField] private UnitSpawner _unitSpawner;
        [SerializeField] private Text _statusText;

        private DeploymentCardUI _selectedCard;
        private Coroutine _statusRoutine;

        public DeploymentCardUI SelectedCard => _selectedCard;

        private void OnEnable()
        {
            DeploymentCardUI.OnCardTapped += HandleCardTapped;
            DeploymentZone.OnZoneTapped += HandleZoneTapped;
        }

        private void Start()
        {
            foreach (DeploymentCardUI card in _cards)
            {
                if (card != null)
                {
                    card.SetAffordable(card.ManaCost <= _startingMana);
                }
            }

            SetZoneHighlights(false);
            if (_statusText != null)
            {
                _statusText.text = string.Empty;
            }
        }

        private void OnDisable()
        {
            DeploymentCardUI.OnCardTapped -= HandleCardTapped;
            DeploymentZone.OnZoneTapped -= HandleZoneTapped;
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

            UnitBase spawnedUnit = _unitSpawner != null
                ? _unitSpawner.Spawn(_selectedCard.Role, true, zone.transform.position)
                : null;

            if (spawnedUnit == null)
            {
                ShowStatus("Unable to deploy unit", 2f);
                return;
            }

            DeploymentCardUI usedCard = _selectedCard;
            _selectedCard = null;
            usedCard.SetSelected(false);
            usedCard.StartCooldown();
            SetZoneHighlights(false);
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