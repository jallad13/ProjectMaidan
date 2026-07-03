using System.Collections;
using UnityEngine;

namespace ProjectMaidan.Units
{
    public class UnitSpawnAnimation : MonoBehaviour
    {
        [SerializeField] private float _duration = 0.3f;

        private Vector3 _targetScale;

        private void Awake()
        {
            _targetScale = transform.localScale;
            transform.localScale = Vector3.zero;
        }

        private void OnEnable()
        {
            StartCoroutine(AnimateIn());
        }

        private IEnumerator AnimateIn()
        {
            float elapsed = 0f;
            while (elapsed < _duration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / _duration);
                transform.localScale = Vector3.Lerp(Vector3.zero, _targetScale, progress);
                yield return null;
            }

            transform.localScale = _targetScale;
        }
    }
}
