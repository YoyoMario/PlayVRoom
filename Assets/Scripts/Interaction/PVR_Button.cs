using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarioHaberle.PlayVRoom.VR.Interaction
{
    public class PVR_Button : PVR_Interactable
    {
        [Header("---------------------------")]
        public float _unclickedPosition = 0.025f; //max peak position
        public float _clickedPosition = 0.002f; // min bottom position
        public float _buttonVelocityReturn = 0.2f; //to return in original position

        private Vector3 _initialPosition;

        public override void Awake()
        {
            base.Awake();

            Rigidbody.isKinematic = false;
            Rigidbody.useGravity = false;
            Rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            Rigidbody.mass = 0.1f;

            _initialPosition = Transform.localPosition;
        }
        
        public override void OnPick(PVR_Hand pVR_Grab_Rigidbody_Object, bool touchpadTouching)
        {
            //base.OnPick(pVR_Grab_Rigidbody_Object);
            return;
        }

        public override void OnDrop(Vector3 controllerVelocity)
        {
            //base.OnDrop(controllerVelocity);
            return;
        }

        private void FixedUpdate()
        {
            //Move button up
            if(Transform.localPosition.y < _unclickedPosition)
            {
                Rigidbody.velocity = Transform.up * _buttonVelocityReturn;
            }
            else //Stop moving the button up if we're at the peak
            {
                Rigidbody.velocity = Vector3.zero;
                _initialPosition.y = _unclickedPosition;
                Transform.localPosition = _initialPosition;
            }

            //if we are below the minimum treshold hold the button there
            if(Transform.localPosition.y < _clickedPosition)
            {
                _initialPosition.y = _clickedPosition;
                Transform.localPosition = _initialPosition;
            }

            //Fixing the issue where you can push the button sideways
            Transform.localPosition = new Vector3(_initialPosition.x, Transform.localPosition.y, _initialPosition.z);
        }
    } 
}
