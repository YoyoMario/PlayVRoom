using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DivIt.PlayVRoom.Misc
{
    public class PVR_Dummy : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Rigidbody _rigidbodyDummy = null;
        [SerializeField] private Transform _transformBase = null;
        [Header("Settings")]
        [SerializeField] private float _baseStrength = 500;
        [SerializeField] private Vector3 _comPosition = Vector3.one;

        private void Awake()
        {
            _rigidbodyDummy.centerOfMass = _comPosition;
        }

        private void FixedUpdate()
        {
            //Position update
            Vector3 direction = _transformBase.position - _rigidbodyDummy.position;
            _rigidbodyDummy.velocity = (direction * _baseStrength * Time.fixedDeltaTime);
        }
    } 
}
