using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProjectMaidan.Units
{
    /// <summary>
    /// Shared AI utility methods used by all unit subclasses.
    /// Provides target-finding and threat-scanning helpers so each unit class stays clean.
    /// </summary>
    public static class UnitAI
    {
        /// <summary>
        /// Finds the enemy unit closest to the given position, filtered by role priority order.
        /// Priority order is evaluated left-to-right: first priority role is preferred.
        /// </summary>
        public static UnitBase FindBestTarget(Vector3 origin, float range, bool isPlayerUnit, UnitRole[] priorityOrder)
        {
            var allUnits = UnitManager.Instance?.GetAllUnits();
            if (allUnits == null) return null;

            var enemies = allUnits
                .Where(u => u != null && u.IsAlive && u.IsPlayerUnit != isPlayerUnit)
                .Where(u => Vector3.Distance(origin, u.transform.position) <= range)
                .ToList();

            if (enemies.Count == 0) return null;

            foreach (var role in priorityOrder)
            {
                var candidate = enemies
                    .Where(u => u.Role == role)
                    .OrderBy(u => Vector3.Distance(origin, u.transform.position))
                    .FirstOrDefault();
                if (candidate != null) return candidate;
            }

            return enemies.OrderBy(u => Vector3.Distance(origin, u.transform.position)).FirstOrDefault();
        }

        /// <summary>Returns the allied unit with the lowest HP percentage.</summary>
        public static UnitBase FindLowestHPAlly(bool isPlayerUnit)
        {
            return UnitManager.Instance?.GetAllUnits()
                .Where(u => u != null && u.IsAlive && u.IsPlayerUnit == isPlayerUnit)
                .Where(u => u.HPPercent < 0.999f)
                .OrderBy(u => u.HPPercent)
                .FirstOrDefault();
        }

        public static UnitBase FindClosestEnemy(Vector3 origin, bool isPlayerUnit)
        {
            return UnitManager.Instance?.GetAllUnits()
                .Where(u => u != null && u.IsAlive && u.IsPlayerUnit != isPlayerUnit)
                .OrderBy(u => Vector3.Distance(origin, u.transform.position))
                .FirstOrDefault();
        }
    }
}
