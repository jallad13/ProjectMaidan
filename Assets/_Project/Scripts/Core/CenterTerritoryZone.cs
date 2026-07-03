using System.Collections.Generic;
using System.Linq;
using ProjectMaidan.Units;
using UnityEngine;

namespace ProjectMaidan.Core
{
    [RequireComponent(typeof(BoxCollider))]
    public class CenterTerritoryZone : MonoBehaviour
    {
        private readonly HashSet<UnitBase> _units = new();

        public int PlayerCount => _units.Count(unit => unit != null && unit.IsPlayerUnit);
        public int OpponentCount => _units.Count(unit => unit != null && !unit.IsPlayerUnit);

        private void Awake()
        {
            BoxCollider trigger = GetComponent<BoxCollider>();
            trigger.isTrigger = true;

            Rigidbody sensorBody = GetComponent<Rigidbody>();
            if (sensorBody == null)
            {
                sensorBody = gameObject.AddComponent<Rigidbody>();
            }

            sensorBody.isKinematic = true;
            sensorBody.useGravity = false;
            sensorBody.constraints = RigidbodyConstraints.FreezeAll;
        }

        private void OnTriggerEnter(Collider other)
        {
            UnitBase unit = other.GetComponentInParent<UnitBase>();
            RegisterUnit(unit);
        }

        private void OnTriggerExit(Collider other)
        {
            UnitBase unit = other.GetComponentInParent<UnitBase>();
            UnregisterUnit(unit);
        }

        public void RegisterUnit(UnitBase unit)
        {
            if (unit != null)
            {
                _units.Add(unit);
            }
        }

        public void UnregisterUnit(UnitBase unit)
        {
            if (unit != null)
            {
                _units.Remove(unit);
            }
        }

        public void Clear()
        {
            _units.Clear();
        }
    }
}
