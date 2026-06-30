using UnityEngine;

namespace ProjectMaidan.Core
{
    /// <summary>
    /// All match-tunable constants in one ScriptableObject.
    /// Balance changes never require code changes — edit the asset.
    /// </summary>
    [CreateAssetMenu(fileName = "MatchConfig", menuName = "ProjectMaidan/Match Config")]
    public class MatchConfig : ScriptableObject
    {
        [Header("Match")]
        public float MatchDuration = 120f;       // seconds (2 minutes)
        public int   WinThreshold  = 120;        // territory points to win instantly

        [Header("Mana")]
        public int   StartingMana    = 5;
        public int   MaxMana         = 10;
        public float ManaRegenRate   = 3f;       // seconds per 1 mana gained
        public float ComebackRegenRate = 2.4f;   // regen when trailing by 30+ pts
        public int   ComebackThreshold = 30;     // score deficit that activates comeback

        [Header("Territory")]
        public float TerritoryCheckInterval = 1f;
        public float TerritoryLineUpdateInterval = 0.5f;

        [Header("PreMatch")]
        public int CountdownSeconds = 3;
    }
}
