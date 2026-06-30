using UnityEngine;

namespace ProjectMaidan.Units
{
    /// <summary>
    /// Holds Rear-Middle position, attacks highest-threat enemy in range.
    /// Target priority: enemy Ranged → Support → Frontline.
    /// See YouTrack MAI-24.
    /// </summary>
    public class RangedUnit : UnitBase
    {
        // TODO (MAI-24): Implement behavior:
        //   1. On spawn: move to Rear-Middle position and stop
        //   2. Each frame: scan enemies within AttackRange (longer than Frontline)
        //   3. Target priority: Ranged > Support > Frontline
        //   4. Attack VFX: directional line renderer to target at AttackRate
        //   5. If enemy reaches within melee range of this unit: retreat while continuing to fire
        //   6. Does NOT reposition unless directly threatened
    }
}
