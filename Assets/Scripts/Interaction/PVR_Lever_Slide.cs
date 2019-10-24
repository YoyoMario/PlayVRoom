﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DivIt.PlayVRoom.ScriptableObjects;

namespace DivIt.PlayVRoom.VR.Interaction
{
    public class PVR_Lever_Slide : PVR_Interactable
    {
        [Header("-----------------------------")]
        [SerializeField] private float _maxMovement = 0.03f;
        [SerializeField] private float _minMovement = -0.03f;
        [Space(5)]
        [SerializeField] private HapticFeedback _movementHaptics = null;
        [SerializeField] private float _hapticsAtDeltaMovement = 25f;

        private Vector3 _velocityDirection = Vector3.zero;
        private Vector3 _cross = Vector3.zero;
        private float _deltaMovement = 0f;
        private Quaternion _initialRotation = Quaternion.identity;
        private Vector3 _lastHandledPosition = Vector3.zero;

        private float _totalMovement;

        public override void Awake()
        {
            base.Awake();

            Rigidbody.centerOfMass = Vector3.zero;
            Rigidbody.useGravity = false;
            Rigidbody.isKinematic = false;
            Rigidbody.constraints = RigidbodyConstraints.None;

            _initialRotation = Rotation;
            _lastHandledPosition = Position;
        }

        public override void FixedUpdate()
        {
            _velocityDirection = Transform.up;
            _deltaMovement = Transform.localPosition.z * -1; //-1 just to get right orientation with the cross product
            Rigidbody.rotation = _initialRotation;

            if (Picked && Hand)
            {
                _cross = Vector3.Cross(Transform.forward, Position - Hand.Rigidbody.position);
                _cross = Transform.InverseTransformDirection(_cross);
                Rigidbody.velocity = _velocityDirection * _cross.x * ControllerPhysics.LeverSlideVelocityStrength;
            }
            else
            {
                Rigidbody.velocity = Vector3.zero;
                Rigidbody.angularVelocity = Vector3.zero;

                Rigidbody.position = _lastHandledPosition;
            }

            //limiting movement position
            if (_deltaMovement >= _maxMovement && _cross.x > 0)
            {
                Rigidbody.angularVelocity = Vector3.zero;
                Rigidbody.velocity = Vector3.zero;

                Transform.localPosition = new Vector3(0, 0, _minMovement);
            }
            else if (_deltaMovement <= _minMovement && _cross.x < 0)
            {
                Rigidbody.angularVelocity = Vector3.zero;
                Rigidbody.velocity = Vector3.zero;

                Transform.localPosition = new Vector3(0, 0, _maxMovement);
            }

            //Haptics
            if(Picked && Hand)
            {
                _totalMovement += Rigidbody.velocity.magnitude;
                if(_totalMovement > _hapticsAtDeltaMovement)
                {
                    HapticFeedbackManager.HapticeFeedback(
                        _movementHaptics.SecondsFromNow,
                        _movementHaptics.Duration,
                        _movementHaptics.Frequency,
                        _movementHaptics.Amplitude,
                        Hand.InputSource
                        );
                    _totalMovement = 0;
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

            //record the lever position at the moment when user leaves interacting
            _lastHandledPosition = Position;
        }
    } 
}
