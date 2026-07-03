using System.Collections;
using UnityEngine;

namespace ProjectMaidan.Units
{
    public class SupportUnit : UnitBase
    {
        private const float ScanInterval = 1f;
        private float _retreatUntil;

        protected override void Start()
        {
            base.Start();
            MoveTo(BattlefieldNavigation.RearAnchor(IsPlayerUnit, transform.position.x));
            StartCoroutine(SupportLoop());
        }

        protected override void OnDamaged()
        {
            _retreatUntil = Time.time + 2f;
            MoveTo(BattlefieldNavigation.Retreat(IsPlayerUnit, transform.position.x));
        }

        private IEnumerator SupportLoop()
        {
            var wait = new WaitForSeconds(ScanInterval);
            while (IsAlive)
            {
                if (Time.time < _retreatUntil)
                {
                    yield return wait;
                    continue;
                }

                UnitBase target = UnitAI.FindLowestHPAlly(IsPlayerUnit);
                if (target == null)
                {
                    MoveTo(BattlefieldNavigation.RearAnchor(IsPlayerUnit, transform.position.x));
                }
                else
                {
                    float distance = Vector3.Distance(transform.position, target.transform.position);
                    if (distance <= HealRange)
                    {
                        StopMoving();
                        target.Heal(HealAmount);
                        target.ShowHealPulse();
                    }
                    else
                    {
                        float rearDirection = IsPlayerUnit ? -1f : 1f;
                        Vector3 supportPosition = target.transform.position +
                                                  Vector3.forward * rearDirection * Mathf.Max(1f, HealRange * 0.7f);
                        MoveTo(supportPosition);
                    }
                }

                yield return wait;
            }
        }
    }
}
