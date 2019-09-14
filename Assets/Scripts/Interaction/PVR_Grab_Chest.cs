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
        private void FixedUpdate()
        {
            //Get right vector, the one where we apply angular velocity
            _angularVelocityDirection = transform.right;

            if(Picked && Hand)
            {
                //calculate cross vector so we know if hand is below or above the crate Z axis
                _cross = Vector3.Cross(transform.forward, transform.position - Hand.Rigidbody.position);
                //turn that into local coordinate space
                _cross = transform.InverseTransformDirection(_cross);
                _cross -= _initialCross;
                //apply angular velocity in the desired direction * strength
                Rigidbody.angularVelocity = _angularVelocityDirection * _cross.x * -ControllerPhysics.RotationVelocityMagic;
            }            
        }

        public override void OnPick(PVR_Hand pVR_Grab_Rigidbody_Object, bool matchRotationAndPosition = false)
        {
            base.OnPick(pVR_Grab_Rigidbody_Object, matchRotationAndPosition);

            //calculate cross vector so we know if hand is below or above the crate Z axis
            _initialCross = Vector3.Cross(transform.forward, transform.position - Hand.Rigidbody.position);
            //turn that into local coordinate space
            _initialCross = transform.InverseTransformDirection(_initialCross);
        }
    } 
}
