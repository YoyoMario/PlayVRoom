﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

namespace MarioHaberle.PlayVRoom.Managers
{
    public class HapticFeedbackManager : MonoBehaviour
    {
        public static HapticFeedbackManager Instance;

        [SerializeField] private SteamVR_Action_Vibration _hapticAction;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                //DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Activates haptic feedback on a controller with passed settings.
        /// </summary>
        /// <param name="secondsFromNow"></param>
        /// <param name="duration"></param>
        /// <param name="frequency"></param>
        /// <param name="amplitude"></param>
        /// <param name="inputSource"></param>
        public void HapticeFeedback(float secondsFromNow, float duration, float frequency, float amplitude, SteamVR_Input_Sources inputSource)
        {
            _hapticAction.Execute(
                secondsFromNow,
                duration,
                frequency,
                amplitude,
                inputSource
                );
        }
    } 
}
