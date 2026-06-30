using UnityEngine;

namespace ProjectMaidan.Units
{
    /// <summary>
    /// ScriptableObject-driven unit stats. One asset per unit type.
    /// Balance changes require only changing the asset — no code deployment needed.
    /// See Technical Architecture Document Section 6 and YouTrack MAI-20.
    /// </summary>
    [CreateAssetMenu(fileName = "UnitDefinition_New", menuName = "ProjectMaidan/Unit Definition")]
    public class UnitDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string UnitId;
        public string DisplayNameEN;
        public string DisplayNameAR;
        public UnitRole Role;

        [Header("Economy")]
        [Range(1, 10)] public int ManaCost = 3;

        [Header("Combat Stats")]
        public float MaxHP = 100f;
        public float Damage = 10f;
        public float AttackRange = 2f;
        public float AttackRate = 1f;  // attacks per second

        [Header("Movement")]
        public float MoveSpeed = 3f;

        [Header("Support (Ranged only)")]
        public float HealAmount = 0f;
        public float HealRange = 0f;
        public float HealRate = 0f;

        [Header("Visuals")]
        public Color TeamColorPlayer = Color.blue;
        public Color TeamColorOpponent = Color.red;
    }
}
