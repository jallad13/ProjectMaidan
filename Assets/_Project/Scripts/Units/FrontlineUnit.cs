using UnityEngine;

namespace ProjectMaidan.Units
{
    /// <summary>
    /// Advances to center, engages nearest enemy Frontline, holds at center.
    /// Target priority: enemy Frontline → Support → Ranged.
    /// See YouTrack MAI-22.
    /// </summary>
    public class FrontlineUnit : UnitBase
    {
        // TODO (MAI-22): Implement behavior:
        //   1. On spawn: set destination to center zone position
        //   2. On reaching center: hold position (stop NavMesh)
        //   3. Each frame: scan for enemies within AttackRange
        //   4. Enemy found: stop, attack at AttackRate
        //   5. Target priority: Frontline > Support > Ranged
        //   6. Attack VFX: line renderer flash to target
        //   7. All enemies dead: resume advance toward enemy rear
    }
}
