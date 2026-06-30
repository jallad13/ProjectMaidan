using UnityEngine;

namespace ProjectMaidan.Core
{
    /// <summary>
    /// Singleton entry point. Survives scene loads. Owns references to all top-level managers.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Manager References")]
        public MatchManager MatchManager;
        public ManaSystem ManaSystem;
        public TerritorySystem TerritorySystem;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
