using System.Collections;
using UnityEngine;

namespace ProjectMaidan.Units
{
    public class FrontlineUnit : UnitBase
    {
        private static readonly UnitRole[] TargetPriority =
        {
            UnitRole.Frontline,
            UnitRole.Support,
            UnitRole.Ranged
        };

        private const float ScanInterval = 0.1f;
        private float _nextAttackTime;
        private bool _hasEngaged;

        protected override void Start()
        {
            base.Start();
            MoveTo(BattlefieldNavigation.CenterHold(IsPlayerUnit));
            StartCoroutine(BehaviorLoop());
        }

        private IEnumerator BehaviorLoop()
        {
            var wait = new WaitForSeconds(ScanInterval);
            while (IsAlive)
            {
                UnitBase target = UnitAI.FindBestTarget(
                    transform.position,
                    AttackRange,
                    IsPlayerUnit,
                    TargetPriority);

                if (target != null)
                {
                    _hasEngaged = true;
                    StopMoving();
                    if (Time.time >= _nextAttackTime)
                    {
                        Attack(target, new Color(1f, 0.8f, 0.15f));
                        _nextAttackTime = Time.time + 1f / Mathf.Max(0.01f, AttackRate);
                    }
                }
                else if (_hasEngaged)
                {
                    MoveTo(BattlefieldNavigation.EnemyRear(IsPlayerUnit));
                }
                else
                {
                    Vector3 center = BattlefieldNavigation.CenterHold(IsPlayerUnit);
                    if (HasReached(center))
                    {
                        StopMoving();
                    }
                    else
                    {
                        MoveTo(center);
                    }
                }

                yield return wait;
            }
        }
    }
}
