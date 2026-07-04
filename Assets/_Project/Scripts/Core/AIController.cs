using System.Collections;
using System.Linq;
using ProjectMaidan.Units;
using UnityEngine;

namespace ProjectMaidan.Core
{
    /// <summary>
    /// Medium-difficulty opponent that deploys from its independent mana pool.
    /// See YouTrack MAI-36 and MAI-37.
    /// </summary>
    public class AIController : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private MatchConfig _config;
        [SerializeField] private float _decisionInterval = 2f;
        [SerializeField] private float _minDelay = 0.5f;
        [SerializeField] private float _maxDelay = 1.5f;

        [Header("Thresholds")]
        [SerializeField] private int _aggressiveDefendThreshold = 15;
        [SerializeField, Range(0f, 1f)] private float _randomOverrideChance = 0.2f;

        [Header("Scene References")]
        [SerializeField] private UnitSpawner _unitSpawner;
        [SerializeField] private DeploymentZone[] _opponentZones;
        [SerializeField] private TerritorySystem _territorySystem;

        public int AggressiveDefendThreshold => _aggressiveDefendThreshold;
        public float DecisionInterval => _decisionInterval;
        public float RandomOverrideChance => _randomOverrideChance;
        public UnitBase LastDeployedUnit { get; private set; }
        public DeploymentZone LastDeploymentZone { get; private set; }

        private static readonly UnitRole[] DeploymentPriority =
        {
            UnitRole.Frontline,
            UnitRole.Support,
            UnitRole.Ranged
        };

        private Coroutine _decisionLoopRoutine;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            _decisionLoopRoutine ??= StartCoroutine(DecisionLoop());
        }

        private void OnDisable()
        {
            if (_decisionLoopRoutine != null)
            {
                StopCoroutine(_decisionLoopRoutine);
                _decisionLoopRoutine = null;
            }
        }

        public void Configure(
            MatchConfig config,
            UnitSpawner unitSpawner,
            TerritorySystem territorySystem,
            DeploymentZone[] opponentZones)
        {
            _config = config;
            _unitSpawner = unitSpawner;
            _territorySystem = territorySystem;
            _opponentZones = opponentZones;
        }

        public UnitBase RunDecisionCycle(bool allowRandomOverride = true)
        {
            ResolveReferences();
            LastDeployedUnit = null;
            LastDeploymentZone = null;

            if (!isActiveAndEnabled)
            {
                return null;
            }

            if (allowRandomOverride && Random.value < _randomOverrideChance)
            {
                return DeployRandom();
            }

            if (!CanAffordAny() ||
                UnitManager.Instance == null ||
                !UnitManager.Instance.CanDeploy(false))
            {
                return null;
            }

            if (IsLosingBadly())
            {
                return Deploy(UnitRole.Frontline, GetCenterFrontZone());
            }

            UnitBase rearSupport = FindPlayerRearSupport();
            if (rearSupport != null)
            {
                return Deploy(UnitRole.Ranged, GetClosestZoneToX(rearSupport.transform.position.x));
            }

            return Deploy(UnitRole.Frontline, GetCenterFrontZone());
        }

        private IEnumerator DecisionLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(_decisionInterval);
                yield return new WaitForSeconds(Random.Range(_minDelay, _maxDelay));
                RunDecisionCycle();
            }
        }

        private UnitBase DeployRandom()
        {
            if (!CanAffordAny() ||
                UnitManager.Instance == null ||
                !UnitManager.Instance.CanDeploy(false) ||
                _opponentZones == null ||
                _opponentZones.Length == 0)
            {
                return null;
            }

            if (!TryGetCheapestAffordableRole(out UnitRole role))
            {
                return null;
            }

            DeploymentZone zone = _opponentZones[Random.Range(0, _opponentZones.Length)];
            return Deploy(role, zone);
        }

        private UnitBase Deploy(UnitRole role, DeploymentZone zone)
        {
            ManaSystem mana = ManaSystem.OpponentInstance;
            if (_unitSpawner == null || mana == null || zone == null)
            {
                return null;
            }

            int manaCost = _unitSpawner.GetManaCost(role);
            if (manaCost <= 0 || manaCost == int.MaxValue || !mana.TryDeductMana(manaCost))
            {
                return null;
            }

            UnitBase unit = _unitSpawner.Spawn(role, false, zone.transform.position);
            if (unit == null)
            {
                mana.AddMana(manaCost);
                return null;
            }

            LastDeployedUnit = unit;
            LastDeploymentZone = zone;
            return unit;
        }

        private bool CanAffordAny()
        {
            ManaSystem mana = ManaSystem.OpponentInstance;
            return mana != null &&
                   _unitSpawner != null &&
                   DeploymentPriority.Any(role =>
                   {
                       int cost = _unitSpawner.GetManaCost(role);
                       return cost > 0 && cost < int.MaxValue && mana.CanAfford(cost);
                   });
        }

        private bool TryGetCheapestAffordableRole(out UnitRole cheapestRole)
        {
            cheapestRole = UnitRole.Frontline;
            ManaSystem mana = ManaSystem.OpponentInstance;
            if (mana == null || _unitSpawner == null)
            {
                return false;
            }

            int cheapestCost = int.MaxValue;
            bool found = false;
            foreach (UnitRole role in DeploymentPriority)
            {
                int cost = _unitSpawner.GetManaCost(role);
                if (cost > 0 && cost < cheapestCost && mana.CanAfford(cost))
                {
                    cheapestCost = cost;
                    cheapestRole = role;
                    found = true;
                }
            }

            return found;
        }

        private bool IsLosingBadly()
        {
            return _territorySystem != null &&
                   _territorySystem.PlayerScore - _territorySystem.OpponentScore >=
                   Mathf.Max(1, _aggressiveDefendThreshold);
        }

        private static UnitBase FindPlayerRearSupport()
        {
            return UnitManager.Instance == null
                ? null
                : UnitManager.Instance.GetAllUnits()
                    .FirstOrDefault(unit =>
                        unit != null &&
                        unit.IsAlive &&
                        unit.IsPlayerUnit &&
                        unit.Role == UnitRole.Support &&
                        unit.transform.position.z < -2.5f);
        }

        private DeploymentZone GetCenterFrontZone()
        {
            return _opponentZones?.FirstOrDefault(zone =>
                zone != null && zone.ZoneId == ZoneId.CenterFront);
        }

        private DeploymentZone GetClosestZoneToX(float targetX)
        {
            return _opponentZones?
                .Where(zone => zone != null)
                .OrderBy(zone => Mathf.Abs(zone.transform.position.x - targetX))
                .ThenBy(zone => (int)zone.ZoneId >= (int)ZoneId.LeftRear ? 1 : 0)
                .FirstOrDefault();
        }

        private void ResolveReferences()
        {
            _unitSpawner ??= FindFirstObjectByType<UnitSpawner>();
            _territorySystem ??= FindFirstObjectByType<TerritorySystem>();
            if (_opponentZones == null || _opponentZones.Length == 0)
            {
                _opponentZones = FindObjectsByType<DeploymentZone>(FindObjectsSortMode.None)
                    .Where(zone => !zone.IsPlayerZone)
                    .OrderBy(zone => zone.ZoneId)
                    .ToArray();
            }
        }
    }
}
