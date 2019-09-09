using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarioHaberle.PlayVRoom.VR.Interaction
{
    [RequireComponent(typeof(Rigidbody))]
    public class PVR_Rigidbody_Object : PVR_Interactable
    {
        private static float AttachedPositionMagic = 2000f;
        private static float AttachedRotationMagic = 50f;

        private float _angle;
        private Vector3 _axis;

        private void FixedUpdate()
        {
            //Hold object
            if (Hand)
            {
                //position
                Vector3 positionDelta = Vector3.zero;
                positionDelta = Hand.Rigidbody.position - Rigidbody.position;
                //positionDelta =
                //    positionDelta +
                //    (transform.forward * _objectPositionDifference.z) +
                //    (transform.up * _objectPositionDifference.y) +
                //    (transform.right * _objectPositionDifference.x);
                Vector3 targetVelocity = positionDelta * AttachedPositionMagic * Time.fixedDeltaTime;
                Rigidbody.velocity = positionDelta * AttachedPositionMagic * Time.fixedDeltaTime;

                //Rigidbody update
                Rigidbody.maxAngularVelocity = 100f;

                //rotation
                //Quaternion finalRotation = _rigidbody.rotation * _objectRotationDifference;
                Quaternion rotationDelta = /*finalRotation **/ Hand.Rigidbody.rotation * Quaternion.Inverse(Rigidbody.rotation);
                rotationDelta.ToAngleAxis(out _angle, out _axis);
                if (_angle >= 180)
                {
                    _angle -= 360;
                }
                Vector3 wantedRotation = (Time.fixedDeltaTime * _angle * _axis) * AttachedRotationMagic;
                if (!float.IsNaN(wantedRotation.x) && !float.IsNaN(wantedRotation.y) && !float.IsNaN(wantedRotation.z))
                {
                    Rigidbody.angularVelocity = wantedRotation;
                    //_currentInteractableObject.Rigidbody.angularVelocity = Vector3.MoveTowards(_currentInteractableObject.Rigidbody.angularVelocity, wantedRotation, 20f);
                }
            }
        }

        public override void OnDrop(Vector3 controllerVelocity)
        {
            base.OnDrop(controllerVelocity);

            Rigidbody.velocity = controllerVelocity;
        }
    } 
}
