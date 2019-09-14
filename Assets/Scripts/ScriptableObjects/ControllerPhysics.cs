using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarioHaberle.PlayVRoom.ScriptableObjects
{
    [CreateAssetMenu(fileName = "ControllerPhysics", menuName = "ScriptableObjects/ControllerPhysics", order = 1)]
    public class ControllerPhysics : ScriptableObject
    {
        public float PositionVelocityMagic = 2250f;
        public float RotationVelocityMagic = 50f;
        [Space(10)]
        public float DefaultAngularVelocity = 10f; //Assigned on drop
        public float MaxAngularVelocity = 100f; //Assigned on pick
        [Space(10)]
        public float LeverSlideVelocityStrength = 40f;
        public float LeverPullAngularVelocityStrength = 40f;
    }
}