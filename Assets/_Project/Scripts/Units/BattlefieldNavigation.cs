using UnityEngine;

namespace ProjectMaidan.Units
{
    public static class BattlefieldNavigation
    {
        public const float MinX = -3.5f;
        public const float MaxX = 3.5f;
        public const float MinZ = -5.45f;
        public const float MaxZ = 5.45f;

        public static Vector3 CenterHold(bool isPlayerUnit)
        {
            return new Vector3(0f, 0.6f, isPlayerUnit ? -1.25f : 1.25f);
        }

        public static Vector3 EnemyRear(bool isPlayerUnit)
        {
            return new Vector3(0f, 0.6f, isPlayerUnit ? 4.6f : -4.6f);
        }

        public static Vector3 RearAnchor(bool isPlayerUnit, float x)
        {
            return new Vector3(Mathf.Clamp(x, MinX, MaxX), 0.6f, isPlayerUnit ? -4.55f : 4.55f);
        }

        public static Vector3 RearMiddle(bool isPlayerUnit)
        {
            return RearAnchor(isPlayerUnit, 0f);
        }

        public static Vector3 Retreat(bool isPlayerUnit, float currentX)
        {
            float retreatX = currentX >= 0f ? MinX + 0.25f : MaxX - 0.25f;
            return new Vector3(retreatX, 0.6f, isPlayerUnit ? MinZ : MaxZ);
        }

        public static Vector3 Clamp(Vector3 position)
        {
            position.x = Mathf.Clamp(position.x, MinX, MaxX);
            position.z = Mathf.Clamp(position.z, MinZ, MaxZ);
            return position;
        }
    }
}
