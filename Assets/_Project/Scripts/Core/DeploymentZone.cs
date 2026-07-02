using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectMaidan.Core
{
    public enum ZoneId
    {
        LeftFront, CenterFront, RightFront,
        LeftRear, CenterRear, RightRear
    }

    /// <summary>
    /// One of six deployment zones on a battlefield half.
    /// Player zones emit taps only while highlighted; opponent zones are reserved for AI.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class DeploymentZone : MonoBehaviour, IPointerClickHandler
    {
        public static event Action<DeploymentZone> OnZoneTapped;

        [Header("Identity")]
        public ZoneId ZoneId;
        public bool IsPlayerZone;

        [Header("Highlight")]
        [SerializeField] private Renderer _highlight;

        private bool _isHighlighted;

        public bool IsHighlighted => _isHighlighted;

        public void SetHighlight(bool active)
        {
            _isHighlighted = active && IsPlayerZone;
            if (_highlight != null)
            {
                _highlight.enabled = _isHighlighted;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                TryTap();
            }
        }

        public bool TryTap()
        {
            if (!_isHighlighted || !IsPlayerZone)
            {
                return false;
            }

            OnZoneTapped?.Invoke(this);
            return true;
        }

        private void OnDisable()
        {
            SetHighlight(false);
        }
    }
}