using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DivIt.PlayVRoom.Misc
{
    [RequireComponent(typeof(Rigidbody))]
    public class SecurityCamera : MonoBehaviour
    {
        public enum MovementType
        {
            ContinuousMovement,
            AngleTresholdMovement
        }

        [SerializeField] private MovementType _movementType;
        [SerializeField] private Transform _pivotPositionOffset;
        [SerializeField] private float _tresholdRotationStart = 10;
        [SerializeField] private float _tresholdRotationStop = 1;
        [Space(10)]
        [SerializeField] private float _movementSpeed = 1500f;
        [SerializeField] private float _rotationSpeed = 4f;
        [Space(10)]
        [SerializeField] private bool _destroyed;
        [SerializeField] private float _collisionVelocity = 10f;
        [Header("Audio")]
        [SerializeField] private AnimationCurve _animationCurveVolume;

        private Rigidbody _rigidbody;
        private AudioSource _audioSource;
        private bool _allowRotation;
        
        private Transform _playerVrHead;
        //public Transform test;


        private static float _angularVelocityVolumeMultiplier = 2;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.useGravity = false;
            _rigidbody.isKinematic = false;

            _audioSource = GetComponent<AudioSource>();
            _audioSource.velocityUpdateMode = AudioVelocityUpdateMode.Dynamic;
        }

        private void Start()
        {
            _playerVrHead = GameObject.FindGameObjectWithTag("MainCamera").transform;
        }

        private void Update()
        {
            if (_destroyed)
            {
                return;
            }

            _audioSource.volume = _animationCurveVolume.Evaluate(_rigidbody.angularVelocity.magnitude * _angularVelocityVolumeMultiplier);
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
            Vector3 lookDir = _playerVrHead.position /*test.position */- _rigidbody.position;
            Quaternion targetRotation = Quaternion.LookRotation(lookDir);
            Quaternion rotationDelta = targetRotation * Quaternion.Inverse(_rigidbody.rotation);
            Vector3 axis;
            float angle;
            rotationDelta.ToAngleAxis(out angle, out axis);
            if(angle >= 180)
            {
                angle -= 360;
            }

            if(_movementType == MovementType.ContinuousMovement)
            {
                _rigidbody.angularVelocity = Time.fixedDeltaTime * angle * axis * _rotationSpeed;
            }
            else if(_movementType == MovementType.AngleTresholdMovement)
            {
                if (angle > _tresholdRotationStart)
                {
                    _allowRotation = true;
                }
                else if (angle < _tresholdRotationStop)
                {
                    _allowRotation = false;
                    _rigidbody.angularVelocity = Vector3.zero; //stop rotation so it doesnt rotate away
                }

                if (_allowRotation)
                {
                    _rigidbody.angularVelocity = Time.fixedDeltaTime * angle * axis * _rotationSpeed;
                }
            }            
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.relativeVelocity.magnitude > _collisionVelocity)
            {
                _destroyed = true;
                _audioSource.Stop();
                _rigidbody.useGravity = true;
            }
        }
    } 
}
