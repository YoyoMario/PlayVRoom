using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarioHaberle.PlayVRoom.VR.Interaction
{
    public class PVR_Lever_Pull : PVR_Interactable
    {
        [Header("Settings")]
        [Header("-----------------------")]        
        [SerializeField] private float _angularVelocityStrength = 10;
        [SerializeField] private float _minAngle = -30;
        [SerializeField] private float _maxAngle = 30;

        //calculation infos
        private Vector3 _crossFromInitialDirection;
        private float _rotationAmount;
        private Vector3 _cross;
        private Vector3 _angularVelocityDirection;

        //initial values
        private Quaternion _initialRotation;
        private Vector3 _initialForwardDirection;
        private float _initialPivotRotat;
        private Vector3 _initialPivotPosition;
        private Quaternion _maxAngleFinalRotation;

        public override void Awake()
        {
            base.Awake();

            Rigidbody.centerOfMass = Vector3.zero;

            _initialRotation = Rigidbody.rotation;
            _initialForwardDirection = Rigidbody.transform.forward;
            _initialPivotPosition = Rigidbody.position;
            _initialPivotRotat = Rigidbody.transform.localRotation.eulerAngles.x;
        }

        private void FixedUpdate()
        {
            Rigidbody.position = _initialPivotPosition;
            _angularVelocityDirection = Rigidbody.transform.right;

            if (Picked && Hand)
            {
                //Calculating angle from center
                _crossFromInitialDirection = Vector3.Cross(_initialForwardDirection, Rigidbody.position - Hand.Rigidbody.position);
                _crossFromInitialDirection = transform.InverseTransformDirection(_crossFromInitialDirection);
                //calculate orientation since default position
                _rotationAmount = Rigidbody.transform.localRotation.eulerAngles.x - _initialPivotRotat;
                if (_crossFromInitialDirection.x < 0)
                {
                    _rotationAmount *= -1;
                }
                //Calculate amount to move and in which direction
                _cross = Vector3.Cross(Rigidbody.transform.forward, Rigidbody.position - Hand.Rigidbody.position);
                _cross = transform.InverseTransformDirection(_cross);
                //apply angular velocity in desired direction * strength
                Rigidbody.angularVelocity = _angularVelocityDirection * _cross.x * _angularVelocityStrength;
            }

            //limiting rotation angle
            if(_rotationAmount >= _maxAngle && _cross.x > 0)
            {
                _maxAngleFinalRotation = _initialRotation * Quaternion.Euler(new Vector3(-_maxAngle, 0, 0));
                Rigidbody.angularVelocity = Vector3.zero;
                Rigidbody.rotation = _maxAngleFinalRotation;
            }
            else if(_rotationAmount <= _minAngle && _cross.x < 0)
            {
                _maxAngleFinalRotation = _initialRotation * Quaternion.Euler(new Vector3(-_minAngle, 0, 0));
                Rigidbody.angularVelocity = Vector3.zero;
                Rigidbody.rotation = _maxAngleFinalRotation;
            }
        }

        public override void OnDrop(Vector3 controllerVelocity)
        {
            base.OnDrop(controllerVelocity);

            Rigidbody.angularVelocity = Vector3.zero;
        }
    } 
}
