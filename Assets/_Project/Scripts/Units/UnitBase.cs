using System;
using UnityEngine;
using UnityEngine.AI;

namespace ProjectMaidan.Units
{
    /// <summary>
    /// Base class for all unit types. Handles HP, death, and NavMesh movement.
    /// Subclasses implement role-specific AI behavior.
    /// See YouTrack MAI-20, MAI-21.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public abstract class UnitBase : MonoBehaviour
    {
        public static event Action<UnitBase> OnUnitDied;

        [Header("Definition")]
        [SerializeField] protected UnitDefinition _definition;

        [Header("Team")]
        public bool IsPlayerUnit;

        // Runtime state
        public float CurrentHP { get; private set; }
        public float MaxHP => _definition.MaxHP;
        public float HPPercent => CurrentHP / MaxHP;
        public UnitRole Role => _definition.Role;
        public int ManaCost => _definition.ManaCost;
        public string UnitId => _definition.UnitId;
        public bool IsAlive => CurrentHP > 0f;

        protected NavMeshAgent _agent;

        protected virtual void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            CurrentHP = _definition != null ? _definition.MaxHP : 100f;
        }

        protected virtual void Start()
        {
            if (_definition != null)
            {
                _agent.speed = _definition.MoveSpeed;
                _agent.stoppingDistance = _definition.AttackRange * 0.9f;
            }
            UnitManager.Instance?.Register(this);
        }

        public void TakeDamage(float amount)
        {
            if (!IsAlive) return;
            CurrentHP = Mathf.Max(CurrentHP - amount, 0f);
            OnHPChanged();
            if (CurrentHP <= 0f) Die();
        }

        public void Heal(float amount)
        {
            if (!IsAlive) return;
            CurrentHP = Mathf.Min(CurrentHP + amount, MaxHP);
            OnHPChanged();
        }

        protected virtual void Die()
        {
            UnitManager.Instance?.Unregister(this);
            OnUnitDied?.Invoke(this);
            Destroy(gameObject);
        }

        // TODO (MAI-21): HP bar world-space canvas update
        protected virtual void OnHPChanged() { }

        public void MoveTo(Vector3 destination) => _agent.SetDestination(destination);
        public void StopMoving() => _agent.ResetPath();
    }
}
