using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

using DivIt.PlayVRoom.Managers;
using DivIt.PlayVRoom.ScriptableObjects;

namespace DivIt.PlayVRoom.VR.Interaction
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
        [SerializeField] private SteamVR_Action_Boolean _triggerPress = null;
        [SerializeField] private float _cooldownSpeed = 1;
        [SerializeField] private PistoleMode _pistoleMode = PistoleMode.SingleFire;
        [Header("Shell settings")]
        [SerializeField] private Transform _shellEjectPosition = null;
        [SerializeField] private GameObject _prefabShell = null;
        [SerializeField] private float _shellForce = 1;
        [SerializeField] private float _shellRandomRotationAmount = 15;
        [Header("Recoil settings")]
        [SerializeField] private float _sliderMovementAmount = 0.5f;
        [SerializeField] private Transform _pistolSlider = null;
        [Header("Sound settings")]
        [SerializeField] private AudioClip[] _audioClipShot = null;
        [SerializeField] private Vector2 _minMaxPitch = Vector2.zero;
        [SerializeField] private Vector2 _minMaxVolume = Vector2.zero;
        [Header("Haptic pistol feedback")]
        [SerializeField] private HapticFeedback _shootingHaptics = null;
        [Header("Bullet spawn settings")]
        [SerializeField] private GameObject _prefabBullet = null;
        [SerializeField] private Transform _bulletSpawnPoint = null;
        [Header("Muzzel flash settings")]
        [SerializeField] private GameObject[] _prefabMuzzelFlashes = null;
        [SerializeField] private Transform _muzzelFlashSpawnPosition = null;
        [Header("Info")]
        [SerializeField] private bool _triggerState = false;

        private float _angle;
        private Vector3 _axis;

        private Quaternion _initialShellEjectRotation;

        private Vector3 _initialSliderPosition;
        private Vector3 _endSliderPosition;
        private float _cooldownValue = 1;
        private float _cooldown;

        private BulletManager _bulletManager;

        public override void Start()
        {
            base.Start();

            _initialShellEjectRotation = _shellEjectPosition.localRotation;
            _initialSliderPosition = _pistolSlider.localPosition;
            _endSliderPosition = _initialSliderPosition + (_pistolSlider.forward * _sliderMovementAmount);

            _bulletManager = BulletManager.Instance;
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

            //Randomize shell eject rotation
            _shellEjectPosition.localRotation = _initialShellEjectRotation * Quaternion.Euler(Vector3.up * Random.Range(-_shellRandomRotationAmount, _shellRandomRotationAmount));
            
            //Audio
            AudioManager.PlayAudio3D(
                _audioClipShot,
                Position,
                _minMaxPitch,
                _minMaxVolume
                );

            //Creating bullet shells
            _bulletManager.CreateBulletShell(
                _prefabShell,
                _shellEjectPosition.position,
                _shellEjectPosition.rotation,
                Rigidbody.velocity,
                _shellEjectPosition.right * _shellForce
                );

            //Create bullet
            _bulletManager.CreateBullet(
                _prefabBullet,
                _bulletSpawnPoint.position,
                _bulletSpawnPoint.rotation
                );

            //Muzzel spawn
            _bulletManager.CreateMuzzelFlash(
                _prefabMuzzelFlashes,
                _muzzelFlashSpawnPosition
                );

            //Haptic feeedback
            HapticFeedbackManager.HapticeFeedback(
                _shootingHaptics.SecondsFromNow,
                _shootingHaptics.Duration,
                _shootingHaptics.Frequency,
                _shootingHaptics.Amplitude,
                Hand.InputSource
                );
        }
    } 
}
