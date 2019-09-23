using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarioHaberle.PlayVRoom.VR.Interaction
{
    public class PVR_Grab_Chest : PVR_Interactable
    {
        [Header("-----------------------------------")]
        private Vector3 _angularVelocityDirection;
        private Vector3 _cross;
        private Vector3 _initialCross;

        public override void FixedUpdate()
        {
            //Get right vector, the one where we apply angular velocity
            _angularVelocityDirection = Transform.right;

            if(Picked && Hand)
            {
                //calculate cross vector so we know if hand is below or above the crate Z axis
                _cross = Vector3.Cross(Transform.forward, Transform.position - Hand.Rigidbody.position);
                //turn that into local coordinate space
                _cross = Transform.InverseTransformDirection(_cross);
                //offset the controller position so it doesn't try to go up or down if your hand is not aligned with the Z axis of the chest
                _cross -= _initialCross;
                //apply angular velocity in the desired direction * strength
                Rigidbody.angularVelocity = _angularVelocityDirection * _cross.x * -ControllerPhysics.RotationVelocityMagic;
            }            
        }

        public override void OnPick(PVR_Hand pVR_Grab_Rigidbody_Object, bool matchRotationAndPosition = false)
        {
            base.OnPick(pVR_Grab_Rigidbody_Object, matchRotationAndPosition);

            //calculate cross vector so we know if hand is below or above the crate Z axis
            _initialCross = Vector3.Cross(Transform.forward, Transform.position - Hand.Rigidbody.position);
            //turn that into local coordinate space
            _initialCross = Transform.InverseTransformDirection(_initialCross);
        }
    } 
}
