using UnityEngine;

namespace ProjectMaidan.Backend
{
    /// <summary>
    /// PlayFab integration — authentication, player data, economy, leaderboards.
    /// PROTOTYPE PHASE: stub only. Backend is excluded from prototype scope.
    /// See Technical Architecture Document Section 4.
    /// Implement in Phase 3 (MVP Development).
    /// </summary>
    public class PlayFabManager : MonoBehaviour
    {
        public static PlayFabManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // TODO (Phase 3): Initialize PlayFab SDK with Title ID from config
        // TODO (Phase 3): All methods below throw NotImplementedException in prototype
        public void SubmitMatchResult() { } // placeholder
        public void GetPlayerProfile() { } // placeholder
    }
}
