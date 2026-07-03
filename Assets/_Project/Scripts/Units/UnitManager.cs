using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProjectMaidan.Units
{
    /// <summary>
    /// Registry of all active units on the battlefield.
    /// </summary>
    public class UnitManager : MonoBehaviour
    {
        public static UnitManager Instance { get; private set; }

        public const int MaxUnitsPerSide = 4;

        private readonly List<UnitBase> _activeUnits = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void Register(UnitBase unit)
        {
            if (unit != null && !_activeUnits.Contains(unit))
            {
                _activeUnits.Add(unit);
            }
        }

        public void Unregister(UnitBase unit) => _activeUnits.Remove(unit);

        public IReadOnlyList<UnitBase> GetAllUnits() => _activeUnits;

        public int GetUnitCount(bool isPlayer) => _activeUnits.Count(unit => unit != null && unit.IsPlayerUnit == isPlayer);
        public bool CanDeploy(bool isPlayer) => GetUnitCount(isPlayer) < MaxUnitsPerSide;
    }
}