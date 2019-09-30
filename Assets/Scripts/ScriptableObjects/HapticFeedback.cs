using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarioHaberle.PlayVRoom.ScriptableObjects
{
    [CreateAssetMenu(fileName = "HapticFeedback", menuName = "ScriptableObjects/HapticFeedback", order = 1)]
    public class HapticFeedback : ScriptableObject
    {
        public float SecondsFromNow = 0;
        public float Duration = 0;
        public float Frequency = .05f;
        public float Amplitude = .05f;
    } 
}
