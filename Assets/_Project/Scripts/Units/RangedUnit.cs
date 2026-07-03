using System.Collections;
using UnityEngine;

namespace ProjectMaidan.Units
{
    public class RangedUnit : UnitBase
    {
        private static readonly UnitRole[] TargetPriority =
        {
            UnitRole.Ranged,
            UnitRole.Support,
            UnitRole.Frontline
        };

        private const float ScanInterval = 0.1f;
        private const float ThreatDistance = 2.25f;
        private float _nextAttackTime;
        private float _retreatUntil;

        protected override void Start()
        {
            base.Start();
            MoveTo(BattlefieldNavigation.RearMiddle(IsPlayerUnit));
            StartCoroutine(BehaviorLoop());
        }

        protected override void OnDamaged()
        {
            _retreatUntil = Time.time + 2f;
        }

        private IEnumerator BehaviorLoop()
        {
            var wait = new WaitForSeconds(ScanInterval);
            while (IsAlive)
            {
                UnitBase closestEnemy = UnitAI.FindClosestEnemy(transform.position, IsPlayerUnit);
                bool directlyThreatened = closestEnemy != null &&
                                          Vector3.Distance(transform.position, closestEnemy.transform.position) <= ThreatDistance;

                if (directlyThreatened || Time.time < _retreatUntil)
                {
                    MoveTo(BattlefieldNavigation.Retreat(IsPlayerUnit, transform.position.x));
                }
                else
                {
                    Vector3 rearMiddle = BattlefieldNavigation.RearMiddle(IsPlayerUnit);
                    if (HasReached(rearMiddle))
                    {
                        StopMoving();
                    }
                    else
                    {
                        MoveTo(rearMiddle);
                    }
                }

                UnitBase target = UnitAI.FindBestTarget(
                    transform.position,
                    AttackRange,
                    IsPlayerUnit,
                    TargetPriority);
                if (target != null && Time.time >= _nextAttackTime)
                {
                    Attack(target, new Color(0.65f, 0.9f, 1f));
                    _nextAttackTime = Time.time + 1f / Mathf.Max(0.01f, AttackRate);
                }

                yield return wait;
            }
        }
    }
}
