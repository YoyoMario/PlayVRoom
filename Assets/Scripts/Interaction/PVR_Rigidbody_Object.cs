﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarioHaberle.PlayVRoom.VR.Interaction
{
    [RequireComponent(typeof(Rigidbody))]
    public class PVR_Rigidbody_Object : PVR_Interactable
    {
        private float _angle;
        private Vector3 _axis;

        private void FixedUpdate()
        {
            //Hold object
            if (Picked && Hand)
            {
                //position
                Vector3 dir = Hand.Rigidbody.position - Rigidbody.position;
                dir =
                    dir +
                    (Hand.transform.forward * ObjectPositionDifference.z) +
                    (Hand.transform.up * ObjectPositionDifference.y) +
                    (Hand.transform.right * ObjectPositionDifference.x);
                Vector3 velocityDir = dir * ControllerPhysics.PositionVelocityMagic * Time.fixedDeltaTime;
                Rigidbody.velocity = velocityDir;

                //rotation
                Quaternion finalRotation = Hand.Rigidbody.rotation * ObjectRotationDifference;
                Quaternion rotationDelta = finalRotation * /*Hand.Rigidbody.rotation **/ Quaternion.Inverse(Rigidbody.rotation);
                rotationDelta.ToAngleAxis(out _angle, out _axis);
                if (_angle >= 180)
                {
                    _angle -= 360;
                }
                Vector3 wantedRotation = (Time.fixedDeltaTime * _angle * _axis) * ControllerPhysics.RotationVelocityMagic;
                if (!float.IsNaN(wantedRotation.x) && !float.IsNaN(wantedRotation.y) && !float.IsNaN(wantedRotation.z))
                {
                    Rigidbody.angularVelocity = wantedRotation;
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
