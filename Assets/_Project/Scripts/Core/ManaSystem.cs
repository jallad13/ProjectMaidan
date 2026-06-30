using System;
using UnityEngine;

namespace ProjectMaidan.Core
{
    /// <summary>
    /// Mana regeneration with comeback mechanic.
    /// See GDD Section 6 and YouTrack MAI-29, MAI-30.
    /// </summary>
    public class ManaSystem : MonoBehaviour
    {
        public static event Action<int, int> OnManaChanged; // (current, max)

        public int CurrentMana { get; private set; }
        public int MaxMana => _config != null ? _config.MaxMana : 10;

        [Header("Config")]
        [SerializeField] private MatchConfig _config;

        [Header("Debug — read-only")]
        [SerializeField] private bool _comebackActive;

        // TODO (MAI-29): Implement coroutine-based regen, comeback mechanic detection
        // TODO (MAI-30): Expose DeductMana(int cost) — validates, deducts, fires event

        public bool CanAfford(int manaCost) => CurrentMana >= manaCost;

        public bool TryDeductMana(int cost)
        {
            if (!CanAfford(cost)) return false;
            CurrentMana -= cost;
            OnManaChanged?.Invoke(CurrentMana, MaxMana);
            return true;
        }

        private void AddMana(int amount)
        {
            CurrentMana = Mathf.Min(CurrentMana + amount, MaxMana);
            OnManaChanged?.Invoke(CurrentMana, MaxMana);
        }
    }
}
