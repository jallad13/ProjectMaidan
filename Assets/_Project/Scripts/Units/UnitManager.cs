using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProjectMaidan.Units
{
    /// <summary>
    /// Registry of all active units on the battlefield.
    /// Units register on Start() and unregister on Die().
    /// </summary>
    public class UnitManager : MonoBehaviour
    {
        public static UnitManager Instance { get; private set; }

        public const int MaxUnitsPerSide = 4;

        private readonly List<UnitBase> _activeUnits = new();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void Register(UnitBase unit) => _activeUnits.Add(unit);
        public void Unregister(UnitBase unit) => _activeUnits.Remove(unit);

        public IReadOnlyList<UnitBase> GetAllUnits() => _activeUnits;

        public int GetUnitCount(bool isPlayer) => _activeUnits.Count(u => u.IsPlayerUnit == isPlayer);
        public bool CanDeploy(bool isPlayer) => GetUnitCount(isPlayer) < MaxUnitsPerSide;
    }
}
