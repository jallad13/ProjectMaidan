using System.Collections;
using ProjectMaidan.Units;
using UnityEngine;

namespace ProjectMaidan.Core
{
    [RequireComponent(typeof(LineRenderer))]
    public class TerritoryLineVisual : MonoBehaviour
    {
        [SerializeField] private TerritorySystem _territorySystem;
        [SerializeField] private MatchConfig _config;
        [SerializeField] private float _smoothSpeed = 5f;

        private LineRenderer _line;
        private Coroutine _positionRoutine;
        private Coroutine _flashRoutine;
        private TerritoryControl _lastController;
        private bool _hasController;
        private float _targetZ;

        public float CurrentNormalizedPosition =>
            Mathf.Clamp(transform.localPosition.z / BattlefieldNavigation.MaxZ, -1f, 1f);
        public float TargetZ => _targetZ;
        public bool IsFlashing { get; private set; }

        private void Awake()
        {
            _line = GetComponent<LineRenderer>();
            ConfigureLine();
            if (_territorySystem == null)
            {
                _territorySystem = FindFirstObjectByType<TerritorySystem>();
            }
        }

        private void OnEnable()
        {
            TerritorySystem.OnTerritoryTick += HandleTerritoryTick;
        }

        private void Start()
        {
            RefreshTarget();
            _positionRoutine = StartCoroutine(PositionUpdateLoop());
        }

        private void OnDisable()
        {
            TerritorySystem.OnTerritoryTick -= HandleTerritoryTick;
            if (_positionRoutine != null)
            {
                StopCoroutine(_positionRoutine);
                _positionRoutine = null;
            }
        }

        private void Update()
        {
            Vector3 position = transform.localPosition;
            position.z = Mathf.Lerp(position.z, _targetZ, 1f - Mathf.Exp(-_smoothSpeed * Time.deltaTime));
            transform.localPosition = position;
        }

        public void Configure(TerritorySystem territorySystem, MatchConfig config)
        {
            _territorySystem = territorySystem;
            _config = config;
            RefreshTarget();
        }

        private IEnumerator PositionUpdateLoop()
        {
            while (true)
            {
                RefreshTarget();
                float interval = _config != null
                    ? Mathf.Max(0.05f, _config.TerritoryLineUpdateInterval)
                    : 0.5f;
                yield return new WaitForSeconds(interval);
            }
        }

        private void RefreshTarget()
        {
            if (_territorySystem == null)
            {
                return;
            }

            float normalized = (_territorySystem.PlayerScore - _territorySystem.OpponentScore) /
                               (float)Mathf.Max(1, _territorySystem.WinThreshold);
            _targetZ = Mathf.Clamp(normalized, -1f, 1f) * BattlefieldNavigation.MaxZ;
        }

        private void HandleTerritoryTick(TerritoryTick tick)
        {
            if (tick.Control == TerritoryControl.Contested)
            {
                return;
            }

            if (_hasController && tick.Control != _lastController)
            {
                if (_flashRoutine != null)
                {
                    StopCoroutine(_flashRoutine);
                }

                _flashRoutine = StartCoroutine(FlashRoutine());
            }

            _lastController = tick.Control;
            _hasController = true;
        }

        private IEnumerator FlashRoutine()
        {
            IsFlashing = true;
            _line.startColor = Color.yellow;
            _line.endColor = Color.yellow;
            _line.startWidth = 0.16f;
            _line.endWidth = 0.16f;
            yield return new WaitForSeconds(0.2f);
            _line.startColor = Color.white;
            _line.endColor = Color.white;
            _line.startWidth = 0.08f;
            _line.endWidth = 0.08f;
            IsFlashing = false;
            _flashRoutine = null;
        }

        private void ConfigureLine()
        {
            _line.useWorldSpace = false;
            _line.positionCount = 2;
            _line.SetPosition(0, new Vector3(-4f, 0f, 0f));
            _line.SetPosition(1, new Vector3(4f, 0f, 0f));
            _line.startColor = Color.white;
            _line.endColor = Color.white;
            _line.startWidth = 0.08f;
            _line.endWidth = 0.08f;
            Shader shader = Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color");
            if (shader != null)
            {
                _line.material = new Material(shader);
            }
        }
    }
}
