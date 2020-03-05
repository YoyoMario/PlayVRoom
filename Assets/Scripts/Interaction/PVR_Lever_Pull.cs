using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DivIt.PlayVRoom.ScriptableObjects;

namespace DivIt.PlayVRoom.VR.Interaction
{
    public class PVR_Lever_Pull : PVR_Interactable
    {
        [Header("Settings")]
        [Header("-----------------------")]
        [SerializeField] private Transform _transformParent = null;
        [SerializeField] private float _minAngle = -30;
        [SerializeField] private float _maxAngle = 30;
        [Space(5)]
        [SerializeField] private HapticFeedback _movementHaptics = null;
        [SerializeField] private float _hapticsAtAngle = 25f;

        //calculation infos
        public float _rotationAmount; //from original position to +/- angle
        private Vector3 _cross;
        private Vector3 _angularVelocityDirection;

        //initial values
        private Vector3 _initialPivotPosition;
        private Quaternion _maxAngleFinalRotation;
        private Quaternion _lastHandledRotation; //for collisions if something hits this lever.

        private float _totalRotationAngle; //for haptics

        public override void Awake()
        {
            base.Awake();

            Rigidbody.centerOfMass = Vector3.zero;
            Rigidbody.useGravity = false;
            Rigidbody.isKinematic = false;
            Rigidbody.constraints = RigidbodyConstraints.None;

            _lastHandledRotation = Transform.localRotation;
            _initialPivotPosition = Transform.localPosition;
        }

        public override void FixedUpdate()
        {
            _angularVelocityDirection = Transform.right;

            if (Picked && Hand)
            {
                //Calculate amount to move and in which direction
                _cross = Vector3.Cross(Transform.up, Position - Hand.Rigidbody.position);
                _cross = Transform.InverseTransformDirection(_cross);
                _rotationAmount = Transform.localRotation.eulerAngles.x;
                if(_rotationAmount > 180)
                {
                    _rotationAmount -= 360;
                }
                _rotationAmount *= -1;

                //apply angular velocity in desired direction * strength
                Rigidbody.angularVelocity = _angularVelocityDirection * _cross.x * -ControllerPhysics.LeverPullAngularVelocityStrength;
            }
            else
            {
                //handling situation if user takes another collider and smacks it on our lever
                Rigidbody.angularVelocity = Vector3.zero;
                Rigidbody.rotation = _transformParent.rotation * Quaternion.Inverse(_lastHandledRotation);
            }

            //to fix lever movement
            Rigidbody.velocity = Vector3.zero;
            Rigidbody.position = _transformParent.TransformPoint(_initialPivotPosition);

            //limiting rotation angle
            if (_rotationAmount >= _maxAngle && _cross.x > 0)
            {
                _maxAngleFinalRotation = _transformParent.rotation * Quaternion.Euler(new Vector3(-_maxAngle, 0, 0));
                Rigidbody.angularVelocity = Vector3.zero;
                Rigidbody.rotation = _maxAngleFinalRotation;
            }
            else if (_rotationAmount <= _minAngle && _cross.x < 0)
            {
                _maxAngleFinalRotation = _transformParent.rotation * Quaternion.Euler(new Vector3(-_minAngle, 0, 0));
                Rigidbody.angularVelocity = Vector3.zero;
                Rigidbody.rotation = _maxAngleFinalRotation;
            }

            //Haptics
            if (Picked && Hand)
            {
                _totalRotationAngle += Rigidbody.angularVelocity.magnitude;
                if (_totalRotationAngle > _hapticsAtAngle)
                {
                    HapticFeedbackManager.HapticeFeedback(
                        _movementHaptics.SecondsFromNow,
                        _movementHaptics.Duration,
                        _movementHaptics.Frequency,
                        _movementHaptics.Amplitude,
                        Hand.InputSource
                        );
                    _totalRotationAngle = 0;
                }
            }
        }

        public override void OnCollisionStay(Collision collision)
        {
            //base.OnCollisionStay(collision);
        }

        public override void OnDrop()
        {
            base.OnDrop();

            //recording the lever rotation at the moment when user leaves interacting
            _lastHandledRotation = Quaternion.Inverse(Transform.localRotation);
        }
    } 
}
