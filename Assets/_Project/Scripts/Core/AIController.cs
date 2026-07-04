using System.Collections;
using UnityEngine;

namespace ProjectMaidan.Core
{
    /// <summary>
    /// Medium-difficulty AI opponent. Runs a 2-second decision loop.
    /// Has its own independent ManaSystem instance.
    /// See Technical Architecture Document Section 8 and YouTrack MAI-36, MAI-37.
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

        public int AggressiveDefendThreshold => _aggressiveDefendThreshold;

        private Coroutine _decisionLoopRoutine;

        // TODO (MAI-36): Implement decision loop:
        //   1. Check mana availability
        //   2. Read TerritorySystem.OpponentScore vs PlayerScore
        //   3. If losing by 15+: deploy Frontline to most-contested zone
        //   4. If player Support visible in rear: deploy Ranged targeting that area
        //   5. No threat: deploy Frontline to CenterFront
        //   6. 20% random: pick random zone instead
        //   7. Apply 0.5–1.5s random delay per cycle
        //   8. Respect MaxUnitsPerSide limit

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

        private IEnumerator DecisionLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(_decisionInterval);
                yield return new WaitForSeconds(Random.Range(_minDelay, _maxDelay));
                // TODO: decision logic here
            }
        }
    }
}
