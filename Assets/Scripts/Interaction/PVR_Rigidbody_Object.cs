using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarioHaberle.PlayVRoom.VR.Interaction
{
    [RequireComponent(typeof(Rigidbody))]
    public class PVR_Rigidbody_Object : PVR_Interactable
    {
        private float _angle;
        private Vector3 _axis;

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            //Hold object
            if (Picked && Hand)
            {
                //position
                Vector3 dir = Hand.Rigidbody.position - Position;
                if (!HandPosition)
                {
                    dir =
                    dir +
                    (Hand.transform.forward * ObjectPositionDifference.z) +
                    (Hand.transform.up * ObjectPositionDifference.y) +
                    (Hand.transform.right * ObjectPositionDifference.x);
                }                
                Vector3 velocityDir = dir * ControllerPhysics.PositionVelocityMagic * Time.fixedDeltaTime;
                Rigidbody.velocity = velocityDir + SteamHand.GetTrackedObjectVelocity();

                //rotation
                Quaternion finalRotation = Quaternion.Euler(0, 0, 0);
                Quaternion rotationDelta = Quaternion.Euler(0, 0, 0);
                if (!HandPosition)
                {
                    finalRotation = Hand.Rigidbody.rotation * ObjectRotationDifference;
                    rotationDelta = finalRotation * Quaternion.Inverse(Rotation);
                }
                else
                {
                    rotationDelta = Hand.Rigidbody.rotation * Quaternion.Inverse(Rotation);
                }
                rotationDelta.ToAngleAxis(out _angle, out _axis);
                if (_angle >= 180)
                {
                    _angle -= 360;
                }
                Vector3 wantedRotation = (Time.fixedDeltaTime * _angle * _axis) * ControllerPhysics.RotationVelocityMagic;
                if (!float.IsNaN(wantedRotation.x) && !float.IsNaN(wantedRotation.y) && !float.IsNaN(wantedRotation.z))
                {
                    Rigidbody.angularVelocity = wantedRotation + SteamHand.GetTrackedObjectAngularVelocity();
                }
            }
        }

        public override void OnDrop()
        {
            base.OnDrop();

            //Rigidbody.velocity = controllerVelocity;
        }
    } 
}
