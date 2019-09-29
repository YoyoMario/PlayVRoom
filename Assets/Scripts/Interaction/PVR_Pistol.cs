using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

using MarioHaberle.PlayVRoom.Managers;

namespace MarioHaberle.PlayVRoom.VR.Interaction
{
    
    [RequireComponent(typeof(Rigidbody))]
    public class PVR_Pistol : PVR_Interactable
    {
        private enum PistoleMode
        {
            SingleFire,
            //BurstFire,
            AutoFire
        }

        [Header("-------------------------------")]
        [SerializeField] private SteamVR_Action_Boolean _triggerPress;
        [SerializeField] private float _cooldownSpeed = 1;
        [SerializeField] private PistoleMode _pistoleMode = PistoleMode.SingleFire;
        [Header("Shell settings")]
        [SerializeField] private Transform _shellEjectPosition;
        [SerializeField] private GameObject _prefabShell;
        [SerializeField] private float _shellForce = 1;
        [SerializeField] private float _shellRandomRotationAmount = 15;
        [Header("Recoil settings")]
        [SerializeField] private float _sliderMovementAmount = 0.5f;
        [SerializeField] private Transform _pistolSlider;
        [Header("Sound settings")]
        [SerializeField] private AudioClip[] _audioClipShot;
        [SerializeField] private Vector2 _minMaxPitch;
        [SerializeField] private Vector2 _minMaxVolume;
        [Header("Haptic pistol feedback")]
        [SerializeField] private SteamVR_Action_Vibration _hapticAction;
        [SerializeField] private float _secondsFromNow = 0;
        [SerializeField] private float _duration = 0;
        [SerializeField] private float _frequency = 150;
        [SerializeField] private float _amplitude = 75;
        [Header("Bullet spawn settings")]
        [SerializeField] private BulletSpawnManager.BulletType _bulletType;
        [SerializeField] private Transform _bulletSpawnPoint;
        [Header("Info")]
        [SerializeField] private bool _triggerState;

        private float _angle;
        private Vector3 _axis;

        private Quaternion _initialShellEjectRotation;

        private Vector3 _initialSliderPosition;
        private Vector3 _endSliderPosition;
        private float _cooldownValue = 1;
        private float _cooldown;

        private BulletSpawnManager _bulletSpawnManager;

        public override void Start()
        {
            base.Start();

            _initialShellEjectRotation = _shellEjectPosition.localRotation;
            _initialSliderPosition = _pistolSlider.localPosition;
            _endSliderPosition = _initialSliderPosition + (_pistolSlider.forward * _sliderMovementAmount);

            _bulletSpawnManager = BulletSpawnManager.Instance;
        }

        private void Update()
        {
            //Shoot logic
            if (Picked && Hand)
            {
                if (_pistoleMode == PistoleMode.AutoFire && _triggerState && _cooldown < 0)
                {
                    Shoot();
                }
            }

            //Cooldown
            if (_cooldown >= 0)
            {
                _cooldown -= Time.deltaTime * _cooldownSpeed;
                _pistolSlider.localPosition = Vector3.Lerp(_endSliderPosition, _initialSliderPosition, 1 - _cooldown);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            //Hold object
            if (Picked && Hand)
            {
                //position
                Vector3 dir = Hand.Rigidbody.position - Position;
                Vector3 velocityDir = dir * ControllerPhysics.PositionVelocityMagic * Time.fixedDeltaTime;
                    Rigidbody.velocity = velocityDir + SteamHand.GetTrackedObjectVelocity();

                //rotation
                Quaternion finalRotation = Quaternion.Euler(0, 0, 0);
                Quaternion rotationDelta = Quaternion.Euler(0, 0, 0);
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
                     Rigidbody.angularVelocity = wantedRotation + SteamHand.GetTrackedObjectAngularVelocity();
                }
            }
        }

        public override void OnPick(PVR_Hand pVR_Grab_Rigidbody_Object, bool matchRotationAndPosition = false)
        {
            base.OnPick(pVR_Grab_Rigidbody_Object, matchRotationAndPosition);

            _triggerPress.AddOnStateDownListener(OnTriggerPress, Hand.InputSource);
            _triggerPress.AddOnStateUpListener(OnTriggerRelease, Hand.InputSource);

            _triggerState = false; //just in case if user leaves a trigger pressed
        }

        public override void OnDrop()
        {
            _triggerPress.RemoveOnStateDownListener(OnTriggerPress, Hand.InputSource);
            _triggerPress.RemoveOnStateUpListener(OnTriggerRelease, Hand.InputSource);

            _triggerState = false; //just in case if user leaves a trigger pressed

            base.OnDrop();
        }

        private void OnTriggerPress(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
        {
            //Debug.Log("OnTriggerPress()");
            _triggerState = true;

            //Don't fire if cooldown isn't done
            if (_cooldown >= 0)
            {
                return;
            }

            if (_pistoleMode == PistoleMode.SingleFire)
            {
                Shoot();
            }
        }

        private void OnTriggerRelease(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
        {
            //Debug.Log("OnTriggerRelease()");
            _triggerState = false;
        }

        private void Shoot()
        {
            _pistolSlider.localPosition = _endSliderPosition;
            _cooldown = _cooldownValue;

            _shellEjectPosition.localRotation = _initialShellEjectRotation * Quaternion.Euler(Vector3.up * Random.Range(-_shellRandomRotationAmount, _shellRandomRotationAmount));
           
            //Prebaciti ovo u manager da hendla.           
            GameObject tmpShell = Instantiate(_prefabShell, _shellEjectPosition.position, _shellEjectPosition.rotation, BulletShellManager.Instance.transform);
            Rigidbody tmpRbShell = tmpShell.GetComponent<Rigidbody>();
            tmpRbShell.velocity = Rigidbody.velocity;
            tmpRbShell.AddForce(_shellEjectPosition.right * _shellForce);

            //Audio
            if(_audioClipShot.Length > 0)
            {
                //ovo isto da audio manager hendla
                //ti mu samo prosljedis min max, on neka se bavi time
                float pitch = (_minMaxPitch.x != _minMaxPitch.y) ? Random.Range((float)_minMaxPitch.x, (float)_minMaxPitch.y) : 1;
                float volume = (_minMaxVolume.x != _minMaxVolume.y) ? Random.Range((float)_minMaxVolume.x, (float)_minMaxVolume.y) : 1;
                AudioClip audioClip = _audioClipShot[Random.Range((int)0, (int)_audioClipShot.Length - 1)];
                AudioManager.PlayAudio3D(audioClip, Position, pitch, volume);
            }

            //Haptic feeedback
            _hapticAction.Execute(_secondsFromNow, _duration, _frequency, _amplitude, Hand.InputSource);

            //Create bullet
            _bulletSpawnManager.CreateBullet(
                _bulletSpawnPoint.position,
                _bulletSpawnPoint.rotation,
                _bulletType
                );
        }

    } 
}
