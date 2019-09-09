using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarioHaberle.PlayVRoom.VR.Interaction
{
    public class PVR_Grab_Chest : PVR_Interactable
    {
        [Header("-----------------------------------")]
        //[SerializeField] private Transform _handHelper;
        [SerializeField] private Vector3 angularVelocityMock = new Vector3(1, 0, 0);
        [SerializeField] private float multiplier = 10;

        public Vector3 _cross;
        private void FixedUpdate()
        {
            angularVelocityMock = transform.right;
            if(Picked && Hand)
            {
                _cross = Vector3.Cross(transform.forward, transform.position - Hand.Rigidbody.position);
                _cross = transform.InverseTransformDirection(_cross);
                Rigidbody.angularVelocity = angularVelocityMock * _cross.x * -multiplier;
            }
            
        }
    } 
}
