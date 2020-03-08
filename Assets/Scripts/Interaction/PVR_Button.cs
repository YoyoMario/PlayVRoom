using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DivIt.PlayVRoom.Managers;

namespace DivIt.PlayVRoom.VR.Interaction
{
    public class PVR_Button : PVR_Interactable
    {
        [Header("---------------------------")]
        [SerializeField] private bool _consoleWrite = false;
        [Space(20)]
        [SerializeField] private Transform _transformParent = null;
        [SerializeField] private float _topPositionYValue = 0.025f; //max peak position
        [SerializeField] private float _bottomPositionYValue = 0.002f; // min bottom position
        [SerializeField] private float _buttonVelocityReturn = 0.2f; //to return in original position
        [Space(20)]
        [SerializeField] private int _clickPercentageTreshold = 75; //0% unclicked - 100% fully clicked
        [SerializeField] private bool _clicked;
        [Header("Audio")]
        [SerializeField] AudioClip[] _audioClipPressSounds = null;
        [SerializeField] AudioClip[] _audioClipReleaseSounds = null;

        private const float ERROR_ACCUMULATION = 0.0001f;

        public Action OnButtonPress;
        public Action OnButtonRelease;

        public override void Awake()
        {
            base.Awake();

            Rigidbody.isKinematic = false;
            Rigidbody.useGravity = false;
            Rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            Rigidbody.mass = 0.1f;
        }

        public override void Start()
        {
            base.Start();
        }

        public override void OnPick(PVR_Hand pVR_Grab_Rigidbody_Object, bool touchpadTouching)
        {
            //base.OnPick(pVR_Grab_Rigidbody_Object);
            return;
        }

        public override void OnDrop()
        {
            //base.OnDrop(controllerVelocity);
            return;
        }

        public float currentPosition;
        public override void FixedUpdate()
        {
            Rigidbody.angularVelocity = Vector3.zero;
            Rigidbody.rotation = _transformParent.rotation;

            //Move button up
            if (Transform.localPosition.y < _topPositionYValue)
            {
                Rigidbody.velocity = (Rigidbody.rotation * Vector3.up).normalized * _buttonVelocityReturn;
            }
            else //Stop moving the button up if we're at the peak
            {
                Rigidbody.velocity = Vector3.zero;
                currentPosition = _topPositionYValue;
                Rigidbody.position = _transformParent.position + (Rigidbody.rotation * (Vector3.up * (currentPosition + ERROR_ACCUMULATION)));
            }

            //if we are below the minimum treshold hold the button there
            if (Transform.localPosition.y < _bottomPositionYValue)
            {
                currentPosition = _bottomPositionYValue;
                Rigidbody.position = _transformParent.position + (Rigidbody.rotation * (Vector3.up * (currentPosition + ERROR_ACCUMULATION)));
            }

            ClickLogic();
        }

        //private void Update()
        //{
        //    if (Transform.localPosition.y >= _unclickedPosition) //Stop moving the button up if we're at the peak
        //    {
        //        Transform.localPosition = _initialPosition;
        //    }
        //    else if (Transform.localPosition.y < _clickedPosition) //if we are below the minimum treshold hold the button there
        //    {
        //        Transform.localPosition = _initialPosition;
        //    }

        //    //Fixing the issue where you can push the button sideways
        //    Transform.localPosition = new Vector3(_initialPosition.x, Transform.localPosition.y, _initialPosition.z);
        //}

            public float clickAmountPercentage;
        public float zeroOffset;
        public float newBottom;
        public float newTop;
        public float currentYpos;
        /// <summary>
        /// Determines if button is clicker or not.
        /// Plays audio.
        /// </summary>
        private void ClickLogic()
        {
            //Calculate press percentage - press/release events
            zeroOffset = 0 - _bottomPositionYValue;
            newBottom = _bottomPositionYValue + zeroOffset;
            newTop = _topPositionYValue + zeroOffset;
            currentYpos = Transform.localPosition.y + zeroOffset;
            clickAmountPercentage = 1 - (currentYpos / newTop);
            //clickAmountPercentage = Mathf.Lerp(_bottomPositionYValue + zeroOffset, _topPositionYValue + zeroOffset)
            /*float*/
            //clickAmountPercentage = 1 - (Transform.localPosition.y - Mathf.Abs(_bottomPositionYValue)) / (Mathf.Abs(_bottomPositionYValue) + _topPositionYValue - Mathf.Abs(_bottomPositionYValue)));
            clickAmountPercentage *= 100;
            if (clickAmountPercentage > _clickPercentageTreshold && !_clicked)
            {
                if (_consoleWrite)
                {
                    Debug.Log("Button pressed!");
                }
                _clicked = true;

                //Play audio
                if (_audioClipPressSounds.Length > 0)
                {
                    AudioManager.PlayAudio3D(_audioClipPressSounds, Position);
                }

                if (OnButtonPress != null)
                {
                    OnButtonPress();
                }
            }
            else if (clickAmountPercentage < _clickPercentageTreshold && _clicked)
            {
                if (_consoleWrite)
                {
                    Debug.Log("Button released!");
                }
                _clicked = false;

                //Play audio
                if (_audioClipReleaseSounds.Length > 0)
                {
                    AudioManager.PlayAudio3D(_audioClipReleaseSounds, Position);
                }

                if (OnButtonRelease != null)
                {
                    OnButtonRelease();
                }
            }
        }
    } 
}
