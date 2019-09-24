using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

using MarioHaberle.PlayVRoom.Managers;

namespace MarioHaberle.PlayVRoom.VR.Interaction
{
    [RequireComponent(typeof(Rigidbody))]
    public class PVR_Rifle : PVR_Interactable
    {
        [Header("-------------------------------")]
        [SerializeField] private PVR_Second_Hand _secondHand;

        private float _angle;
        private Vector3 _axis;

        private Quaternion _initialShellEjectRotation;

        private Vector3 _initialSliderPosition;
        private Vector3 _endSliderPosition;
        private float _cooldownValue = 1;
        private float _cooldown;

        public override void Start()
        {
            base.Start();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            //Hold object
            if (Picked && Hand)
            {
                //position
                Vector3 dir = Hand.Rigidbody.position - Position;
                Vector3 velocityDir = dir * ControllerPhysics.PositionVelocityMagic * Time.fixedDeltaTime;
                Rigidbody.velocity = velocityDir + SteamHand.GetTrackedObjectVelocity();

                if (!_secondHand.Picked) //hold with one hand
                {
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
                else //rotate towards second controller
                {
                    //rotation
                    Vector3 lookDir = _secondHand.Hand.transform.position - Hand.transform.position;
                    Quaternion rotationDir = Quaternion.LookRotation(lookDir, Hand.transform.forward + _secondHand.Hand.transform.forward);
                    rotationDir = rotationDir * HandPosition.localRotation; //to fix the rotation of the hand grab position if predefined
                    Quaternion rotationDelta = rotationDir * Quaternion.Inverse(Rotation);

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

        }
    }
}
