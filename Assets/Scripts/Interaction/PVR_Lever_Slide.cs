﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarioHaberle.PlayVRoom.VR.Interaction
{
    public class PVR_Lever_Slide : PVR_Interactable
    {
        [Header("-----------------------------")]
        [SerializeField] private float _maxMovement = 0.03f;
        [SerializeField] private float _minMovement = -0.03f;

        private Vector3 _velocityDirection;
        private Vector3 _cross;
        private float _deltaMovement;
        private Quaternion _initialRotation;
        private Vector3 _lastHandledPosition;

        public override void Awake()
        {
            base.Awake();

            Rigidbody.centerOfMass = Vector3.zero;
            Rigidbody.useGravity = false;
            Rigidbody.isKinematic = false;
            Rigidbody.constraints = RigidbodyConstraints.None;

            _initialRotation = Rigidbody.rotation;
            _lastHandledPosition = Rigidbody.position;
        }

        private void FixedUpdate()
        {
            _velocityDirection = Rigidbody.transform.up;
            _deltaMovement = Rigidbody.transform.localPosition.z * -1; //-1 just to get right orientation with the cross product
            Rigidbody.rotation = _initialRotation;

            if (Picked && Hand)
            {
                _cross = Vector3.Cross(Rigidbody.transform.forward, Rigidbody.position - Hand.Rigidbody.position);
                _cross = transform.InverseTransformDirection(_cross);
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

                Rigidbody.transform.localPosition = new Vector3(0, 0, _minMovement);
            }
            else if (_deltaMovement <= _minMovement && _cross.x < 0)
            {
                Rigidbody.angularVelocity = Vector3.zero;
                Rigidbody.velocity = Vector3.zero;

                Rigidbody.transform.localPosition = new Vector3(0, 0, _maxMovement);
            }
        }

        public override void OnDrop(Vector3 controllerVelocity)
        {
            base.OnDrop(controllerVelocity);

            //record the lever position at the moment when user leaves interacting
            _lastHandledPosition = Rigidbody.position;
        }
    } 
}
