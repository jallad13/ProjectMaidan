using UnityEngine;

namespace ProjectMaidan.Units
{
    /// <summary>
    /// Stays in Rear zone, heals lowest-HP ally every second, retreats if attacked directly.
    /// Does NOT attack enemies.
    /// See YouTrack MAI-23.
    /// </summary>
    public class SupportUnit : UnitBase
    {
        // TODO (MAI-23): Implement behavior:
        //   1. On spawn: move to Rear zone position — do NOT advance to center
        //   2. Coroutine every 1s: scan allied units for lowest HP%
        //   3. If target within HealRange: apply Heal(HealAmount)
        //   4. If target outside HealRange: move toward target
        //   5. Heal VFX: circle pulse on target
        //   6. If TakeDamage called: retreat to furthest rear NavMesh position
        //   7. Never set attack target — Role = Support means no combat
    }
}
