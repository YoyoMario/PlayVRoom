using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DivIt.PlayVRoom.Misc
{
    [RequireComponent(typeof(Rigidbody))]
    public class SecurityCamera : MonoBehaviour
    {
        [SerializeField] private Transform _pivotPositionOffset;
        [Space(10)]
        [SerializeField] private float _movementSpeed = 1500f;
        [SerializeField] private float _rotationSpeed = 4f;
        [Space(10)]
        [SerializeField] private bool _destroyed;
        [SerializeField] private float _collisionVelocity = 10f;

        private Rigidbody _rigidbody;
        private Transform _playerVrHead;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.useGravity = false;
            _rigidbody.isKinematic = false;
        }

        private void Start()
        {
            _playerVrHead = GameObject.FindGameObjectWithTag("MainCamera").transform;
        }

        private void FixedUpdate()
        {
            if (_destroyed)
            {
                return;
            }

            //Correct position
            Vector3 posDir = _pivotPositionOffset.position - _rigidbody.position;
            _rigidbody.velocity = posDir * Time.fixedDeltaTime * _movementSpeed;

            //Rotation towards the target
            Vector3 lookDir = _playerVrHead.position - _rigidbody.position;
            Quaternion targetRotation = Quaternion.LookRotation(lookDir);
            Quaternion rotationDelta = targetRotation * Quaternion.Inverse(_rigidbody.rotation);
            Vector3 axis;
            float angle;
            rotationDelta.ToAngleAxis(out angle, out axis);
            if(angle >= 180)
            {
                angle -= 360;
            }
            _rigidbody.angularVelocity = Time.fixedDeltaTime * angle * axis * _rotationSpeed;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.relativeVelocity.magnitude > _collisionVelocity)
            {
                _destroyed = true;
                _rigidbody.useGravity = true;
            }
        }
    } 
}
