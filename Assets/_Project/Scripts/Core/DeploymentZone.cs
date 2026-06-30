using System;
using UnityEngine;

namespace ProjectMaidan.Core
{
    public enum ZoneId
    {
        LeftFront, CenterFront, RightFront,
        LeftRear,  CenterRear,  RightRear
    }

    /// <summary>
    /// Represents one of the 6 deployment zones per player half.
    /// Handles tap detection and deployment event firing.
    /// See YouTrack MAI-17.
    /// </summary>
    public class DeploymentZone : MonoBehaviour
    {
        public static event Action<DeploymentZone> OnZoneTapped;

        [Header("Identity")]
        public ZoneId ZoneId;
        public bool IsPlayerZone;

        [Header("Highlight")]
        [SerializeField] private Renderer _highlight;

        private bool _isHighlighted;

        public void SetHighlight(bool active)
        {
            _isHighlighted = active;
            if (_highlight != null) _highlight.enabled = active;
        }

        // TODO (MAI-17): Hook into Unity touch input (Input.GetMouseButtonDown or new InputSystem)
        // Check _isHighlighted before firing event — zones only respond when active

        private void OnMouseDown()
        {
            if (_isHighlighted) OnZoneTapped?.Invoke(this);
        }
    }
}
