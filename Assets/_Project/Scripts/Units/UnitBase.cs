using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace ProjectMaidan.Units
{
    [RequireComponent(typeof(NavMeshAgent))]
    public abstract class UnitBase : MonoBehaviour
    {
        public static event Action<UnitBase> OnUnitDied;

        [Header("Definition")]
        [SerializeField] protected UnitDefinition _definition;

        [Header("Team")]
        public bool IsPlayerUnit;

        public float CurrentHP { get; private set; }
        public float HP => CurrentHP;
        public float MaxHP => _definition != null ? _definition.MaxHP : 100f;
        public float HPPercent => MaxHP > 0f ? CurrentHP / MaxHP : 0f;
        public UnitRole Role => _definition != null ? _definition.Role : UnitRole.Frontline;
        public int ManaCost => _definition != null ? _definition.ManaCost : 0;
        public string UnitId => _definition != null ? _definition.UnitId : name;
        public string DisplayName => _definition != null ? _definition.DisplayNameEN : name;
        public float Damage => _definition != null ? _definition.Damage : 0f;
        public float MoveSpeed => _definition != null ? _definition.MoveSpeed : 0f;
        public float AttackRange => _definition != null ? _definition.AttackRange : 0f;
        public float AttackRate => _definition != null ? _definition.AttackRate : 0f;
        public float HealAmount => _definition != null ? _definition.HealAmount : 0f;
        public float HealRange => _definition != null ? _definition.HealRange : 0f;
        public bool IsAlive => CurrentHP > 0f;
        public NavMeshAgent Agent => _agent;
        public UnitHealthBar HealthBar => _healthBar;

        protected NavMeshAgent _agent;
        private UnitHealthBar _healthBar;
        private LineRenderer _attackLine;
        private Coroutine _attackFlashRoutine;

        protected virtual void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            CurrentHP = MaxHP;

            _healthBar = GetComponentInChildren<UnitHealthBar>(true);
            if (_healthBar == null)
            {
                _healthBar = UnitHealthBar.Create(transform);
            }

            _healthBar.SetHealth(HPPercent);
            ConfigureAttackLine();
        }

        protected virtual void Start()
        {
            if (_agent != null && _definition != null)
            {
                _agent.speed = _definition.MoveSpeed;
                _agent.stoppingDistance = Mathf.Max(0.1f, _definition.AttackRange * 0.9f);
            }

            TryActivateAgent();
            UnitManager.Instance?.Register(this);
        }

        protected virtual void OnDestroy()
        {
            UnitManager.Instance?.Unregister(this);
        }

        public virtual void TakeDamage(float amount)
        {
            if (!IsAlive || amount <= 0f)
            {
                return;
            }

            CurrentHP = Mathf.Max(CurrentHP - amount, 0f);
            OnHPChanged();
            OnDamaged();
            if (CurrentHP <= 0f)
            {
                Die();
            }
        }

        public void Heal(float amount)
        {
            if (!IsAlive || amount <= 0f)
            {
                return;
            }

            CurrentHP = Mathf.Min(CurrentHP + amount, MaxHP);
            OnHPChanged();
        }

        protected virtual void Die()
        {
            UnitManager.Instance?.Unregister(this);
            OnUnitDied?.Invoke(this);
            Destroy(gameObject);
        }

        protected virtual void OnDamaged() { }

        protected virtual void OnHPChanged()
        {
            _healthBar?.SetHealth(HPPercent);
        }

        public bool TryActivateAgent()
        {
            if (_agent == null)
            {
                return false;
            }

            if (_agent.enabled && _agent.isOnNavMesh)
            {
                return true;
            }

            _agent.enabled = false;
            if (!NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                return false;
            }

            transform.position = hit.position;
            _agent.enabled = true;
            return _agent.isOnNavMesh;
        }

        public void MoveTo(Vector3 destination)
        {
            if (!TryActivateAgent())
            {
                return;
            }

            Vector3 boundedDestination = BattlefieldNavigation.Clamp(destination);
            if (NavMesh.SamplePosition(boundedDestination, out NavMeshHit hit, 1.5f, NavMesh.AllAreas))
            {
                _agent.isStopped = false;
                _agent.SetDestination(hit.position);
            }
        }

        public void StopMoving()
        {
            if (_agent != null && _agent.enabled && _agent.isOnNavMesh)
            {
                _agent.isStopped = true;
                _agent.ResetPath();
            }
        }

        public bool HasReached(Vector3 destination, float tolerance = 0.25f)
        {
            Vector2 current = new(transform.position.x, transform.position.z);
            Vector2 target = new(destination.x, destination.z);
            return Vector2.Distance(current, target) <= tolerance;
        }

        protected void Attack(UnitBase target, Color flashColor)
        {
            if (target == null || !target.IsAlive || target.IsPlayerUnit == IsPlayerUnit)
            {
                return;
            }

            target.TakeDamage(Damage);
            FlashAttack(target, flashColor);
        }

        public void ShowHealPulse()
        {
            StartCoroutine(HealPulseRoutine());
        }

        private void ConfigureAttackLine()
        {
            _attackLine = GetComponent<LineRenderer>();
            if (_attackLine == null)
            {
                _attackLine = gameObject.AddComponent<LineRenderer>();
            }

            _attackLine.positionCount = 2;
            _attackLine.startWidth = 0.08f;
            _attackLine.endWidth = 0.03f;
            _attackLine.useWorldSpace = true;
            _attackLine.enabled = false;
            Shader shader = Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color");
            if (shader != null)
            {
                _attackLine.material = new Material(shader);
            }
        }

        private void FlashAttack(UnitBase target, Color color)
        {
            if (_attackFlashRoutine != null)
            {
                StopCoroutine(_attackFlashRoutine);
            }

            _attackFlashRoutine = StartCoroutine(AttackFlashRoutine(target, color));
        }

        private IEnumerator AttackFlashRoutine(UnitBase target, Color color)
        {
            if (_attackLine == null || target == null)
            {
                yield break;
            }

            _attackLine.startColor = color;
            _attackLine.endColor = Color.white;
            _attackLine.SetPosition(0, transform.position + Vector3.up * 0.35f);
            _attackLine.SetPosition(1, target.transform.position + Vector3.up * 0.35f);
            _attackLine.enabled = true;
            yield return new WaitForSeconds(0.12f);
            _attackLine.enabled = false;
            _attackFlashRoutine = null;
        }

        private IEnumerator HealPulseRoutine()
        {
            var pulse = new GameObject("HealPulse", typeof(LineRenderer));
            pulse.transform.SetParent(transform, false);
            var line = pulse.GetComponent<LineRenderer>();
            line.loop = true;
            line.useWorldSpace = false;
            line.positionCount = 25;
            line.startWidth = 0.06f;
            line.endWidth = 0.06f;
            line.startColor = new Color(0.2f, 1f, 0.4f, 0.9f);
            line.endColor = line.startColor;
            Shader shader = Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color");
            if (shader != null)
            {
                line.material = new Material(shader);
            }

            for (int index = 0; index < line.positionCount; index++)
            {
                float angle = index / 24f * Mathf.PI * 2f;
                line.SetPosition(index, new Vector3(Mathf.Cos(angle) * 0.7f, 0.05f, Mathf.Sin(angle) * 0.7f));
            }

            yield return new WaitForSeconds(0.25f);
            Destroy(pulse);
        }
    }
}
