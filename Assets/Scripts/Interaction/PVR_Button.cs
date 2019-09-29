using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MarioHaberle.PlayVRoom.Managers;

namespace MarioHaberle.PlayVRoom.VR.Interaction
{
    public class PVR_Button : PVR_Interactable
    {
        [Header("---------------------------")]
        public float _unclickedPosition = 0.025f; //max peak position
        public float _clickedPosition = 0.002f; // min bottom position
        public float _buttonVelocityReturn = 0.2f; //to return in original position
        [Space(20)]
        [SerializeField] private int _clickPercentageTreshold = 75; //0% unclicked - 100% fully clicked
        [SerializeField] private bool _clicked;
        [Header("Audio")]
        [SerializeField] AudioClip[] _audioClipPressSounds;
        [SerializeField] AudioClip[] _audioClipReleaseSounds;

        private Vector3 _initialPosition;
        private AudioManager _audioManager;

        public Action OnButtonPress;
        public Action OnButtonRelease;

        public override void Awake()
        {
            base.Awake();

            Rigidbody.isKinematic = false;
            Rigidbody.useGravity = false;
            Rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            Rigidbody.mass = 0.1f;

            _initialPosition = Transform.localPosition;
        }

        public override void Start()
        {
            base.Start();

            _audioManager = AudioManager.Instance;
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

        public override void FixedUpdate()
        {
            //Move button up
            if(Transform.localPosition.y < _unclickedPosition)
            {
                Rigidbody.velocity = Transform.up * _buttonVelocityReturn;
            }
            else //Stop moving the button up if we're at the peak
            {
                Rigidbody.velocity = Vector3.zero;
                _initialPosition.y = _unclickedPosition;
                Transform.localPosition = _initialPosition;
            }

            //if we are below the minimum treshold hold the button there
            if(Transform.localPosition.y < _clickedPosition)
            {
                _initialPosition.y = _clickedPosition;
                Transform.localPosition = _initialPosition;
            }

            //Fixing the issue where you can push the button sideways
            Transform.localPosition = new Vector3(_initialPosition.x, Transform.localPosition.y, _initialPosition.z);

            //Calculate press percentage - press/release events
            float clickAmountPercentage = 1 - ((Transform.localPosition.y - _clickedPosition) / (_unclickedPosition - _clickedPosition));
            clickAmountPercentage *= 100;
            if(clickAmountPercentage > _clickPercentageTreshold && !_clicked)
            {
                Debug.Log("Button pressed!");
                _clicked = true;

                //Play audio
                if(_audioClipPressSounds.Length > 0)
                {
                    _audioManager.PlayAudio3D(_audioClipPressSounds, Position);
                }                

                if (OnButtonPress != null)
                {
                    OnButtonPress();
                }
            }
            else if(clickAmountPercentage < _clickPercentageTreshold && _clicked)
            {
                Debug.Log("Button released!");
                _clicked = false;

                //Play audio
                if(_audioClipReleaseSounds.Length > 0)
                {
                    _audioManager.PlayAudio3D(_audioClipReleaseSounds, Position);
                }

                if (OnButtonRelease != null)
                {
                    OnButtonRelease();
                }
            }
        }
    } 
}
