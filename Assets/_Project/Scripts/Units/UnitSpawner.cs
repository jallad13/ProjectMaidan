using UnityEngine;

namespace ProjectMaidan.Units
{
    public class UnitSpawner : MonoBehaviour
    {
        [SerializeField] private UnitBase _frontlinePrefab;
        [SerializeField] private UnitBase _supportPrefab;
        [SerializeField] private UnitBase _rangedPrefab;
        [SerializeField] private Color _playerColor = new(0.1f, 0.45f, 1f, 1f);
        [SerializeField] private Color _opponentColor = new(0.9f, 0.15f, 0.15f, 1f);

        public int GetManaCost(UnitRole role)
        {
            UnitBase prefab = GetPrefab(role);
            return prefab != null ? prefab.ManaCost : int.MaxValue;
        }

        public UnitBase Spawn(UnitRole role, bool isPlayerUnit, Vector3 zoneCenter)
        {
            UnitBase prefab = GetPrefab(role);

            if (prefab == null)
            {
                Debug.LogError($"No placeholder prefab configured for role {role}.");
                return null;
            }

            UnitBase unit = Instantiate(prefab, zoneCenter + Vector3.up * 0.6f, Quaternion.identity);
            unit.IsPlayerUnit = isPlayerUnit;
            unit.name = $"{(isPlayerUnit ? "Player" : "Opponent")}_{role}";

            Renderer bodyRenderer = unit.GetComponent<Renderer>();
            if (bodyRenderer != null)
            {
                bodyRenderer.material.color = isPlayerUnit ? _playerColor : _opponentColor;
            }

            return unit;
        }

        private UnitBase GetPrefab(UnitRole role)
        {
            return role switch
            {
                UnitRole.Frontline => _frontlinePrefab,
                UnitRole.Support => _supportPrefab,
                UnitRole.Ranged => _rangedPrefab,
                _ => null
            };
        }
    }
}
