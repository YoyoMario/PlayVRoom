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
        private Vector3 _crossFromInitialDirection; //to determin +/- of lever angle
        private float _rotationAmount; //from original position to +/- angle
        private Vector3 _cross;
        private Vector3 _angularVelocityDirection;

        //initial values
        private Quaternion _initialRotation;
        private Vector3 _initialForwardDirection;
        private float _initialPivotRotat;
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

            _initialRotation = Transform.localRotation;
            _lastHandledRotation = Transform.localRotation;
            _initialForwardDirection = Transform.forward;
            _initialPivotPosition = Transform.localPosition;
            _initialPivotRotat = Transform.localRotation.eulerAngles.x;
        }

        public Transform _dev;
        public override void FixedUpdate()
        {
            if (!_dev) return;
            _angularVelocityDirection = Transform.right;

            if (_dev)
            {
                //Calculating angle from center
                _crossFromInitialDirection = Vector3.Cross(_initialForwardDirection, Position - /*Hand.Rigidbody.position*/ _dev.position);
                _crossFromInitialDirection = Transform.InverseTransformDirection(_crossFromInitialDirection);
                //calculate orientation since default position
                _rotationAmount = Transform.localRotation.eulerAngles.x - _initialPivotRotat;
                if (_crossFromInitialDirection.x < 0)
                {
                    _rotationAmount *= -1;
                }
                //Calculate amount to move and in which direction
                _cross = Vector3.Cross(Transform.forward, Position - /*Hand.Rigidbody.position*/ _dev.position);
                _cross = Transform.InverseTransformDirection(_cross);
                //apply angular velocity in desired direction * strength
                Rigidbody.angularVelocity = _angularVelocityDirection * _cross.x * -ControllerPhysics.LeverPullAngularVelocityStrength;
            }
            //else
            //{
            //    //handling situation if user takes another collider and smacks it on our lever
            //    Rigidbody.angularVelocity = Vector3.zero;
            //    Rigidbody.rotation = _lastHandledRotation;
            //}

            //to fix lever movement
            Rigidbody.velocity = Vector3.zero;
            Rigidbody.position = _transformParent.TransformPoint(_initialPivotPosition);

            //limiting rotation angle
            if (_rotationAmount >= _maxAngle && _cross.x > 0)
            {
                _maxAngleFinalRotation = _transformParent.rotation * (_initialRotation) * Quaternion.Euler(new Vector3(-_maxAngle, 0, 0));
                Rigidbody.angularVelocity = Vector3.zero;
                Rigidbody.rotation = _maxAngleFinalRotation;
            }
            else if (_rotationAmount <= _minAngle && _cross.x < 0)
            {
                _maxAngleFinalRotation = _transformParent.rotation * (_initialRotation) *  Quaternion.Euler(new Vector3(-_minAngle, 0, 0));
                Rigidbody.angularVelocity = Vector3.zero;
                Rigidbody.rotation = _maxAngleFinalRotation;
            }

            ////Haptics
            //if(Picked && Hand)
            //{
            //    _totalRotationAngle += Rigidbody.angularVelocity.magnitude;
            //    if (_totalRotationAngle > _hapticsAtAngle)
            //    {
            //        HapticFeedbackManager.HapticeFeedback(
            //            _movementHaptics.SecondsFromNow,
            //            _movementHaptics.Duration,
            //            _movementHaptics.Frequency,
            //            _movementHaptics.Amplitude,
            //            Hand.InputSource
            //            );
            //        _totalRotationAngle = 0;
            //    }
            //}
        }

        public override void OnCollisionStay(Collision collision)
        {
            //base.OnCollisionStay(collision);
        }

        public override void OnDrop()
        {
            base.OnDrop();

            //recording the lever rotation at the moment when user leaves interacting
            _lastHandledRotation = Transform.localRotation;
        }
    } 
}
