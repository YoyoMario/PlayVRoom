using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

namespace MarioHaberle.PlayVRoom.VR.Interaction
{
    [RequireComponent(typeof(Rigidbody))]
    public class PVR_Pistol : PVR_Interactable
    {
        [Header("-------------------------------")]
        [SerializeField] private SteamVR_Action_Boolean _triggerPress;
        [Header("Settings")]
        [SerializeField] private float _cooldownSpeed = 1;
        [Header("Recoil settings")]
        [SerializeField] private float _sliderMovementAmount = 0.5f;
        [SerializeField] private Transform _pistolSlider;

        private float _angle;
        private Vector3 _axis;

        private Vector3 _initialPistolPosition;

        private Vector3 _initialSliderPosition;
        private Vector3 _endSliderPosition;
        private float _cooldownValue = 1;
        private float _cooldown;

        private void Start()
        {
            _initialSliderPosition = _pistolSlider.localPosition;
            _endSliderPosition = _initialSliderPosition - (_pistolSlider.forward * _sliderMovementAmount);
        }

        private void FixedUpdate()
        {
            //Hold object
            if (Picked && Hand)
            {
                //position
                Vector3 dir = Hand.Rigidbody.position - Position;
                if (!HandPosition)
                {
                    dir =
                    dir +
                    (Hand.transform.forward * ObjectPositionDifference.z) +
                    (Hand.transform.up * ObjectPositionDifference.y) +
                    (Hand.transform.right * ObjectPositionDifference.x);
                }
                Vector3 velocityDir = dir * ControllerPhysics.PositionVelocityMagic * Time.fixedDeltaTime;
                Rigidbody.velocity = velocityDir;

                //rotation
                Quaternion finalRotation;
                Quaternion rotationDelta;
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
                    Rigidbody.angularVelocity = wantedRotation;
                }
            }

            //Cooldown
            if(_cooldown >= 0)
            {
                _cooldown -= Time.deltaTime * _cooldownSpeed;
                _pistolSlider.localPosition = Vector3.Lerp(_endSliderPosition, _initialSliderPosition, 1 - _cooldown);
            }
        }

        public override void OnPick(PVR_Hand pVR_Grab_Rigidbody_Object, bool matchRotationAndPosition = false)
        {
            base.OnPick(pVR_Grab_Rigidbody_Object, matchRotationAndPosition);

            _triggerPress.AddOnStateDownListener(OnTriggerPress, Hand.InputSource);
            _triggerPress.AddOnStateUpListener(OnTriggerRelease, Hand.InputSource);
        }

        public override void OnDrop(Vector3 controllerVelocity)
        {
            _triggerPress.RemoveOnStateDownListener(OnTriggerPress, Hand.InputSource);
            _triggerPress.RemoveOnStateUpListener(OnTriggerRelease, Hand.InputSource);

            base.OnDrop(controllerVelocity);

            Rigidbody.velocity = controllerVelocity;
        }

        private void OnTriggerPress(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
        {
            Debug.Log("OnTriggerPress()");

            //Don't fire if cooldown isn't done
            if(_cooldown >= 0)
            {
                return;
            }

            _pistolSlider.localPosition = _endSliderPosition;
            _cooldown = _cooldownValue;
        }

        private void OnTriggerRelease(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
        {
            Debug.Log("OnTriggerRelease()");
        }

    } 
}
