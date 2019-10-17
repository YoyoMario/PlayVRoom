using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

using DivIt.PlayVRoom.Managers;
using DivIt.PlayVRoom.ScriptableObjects;

namespace DivIt.PlayVRoom.VR.Interaction
{
    public class PVR_Grenade : PVR_Rigidbody_Object
    {
        [Header("-----------------------------------------")]
        [SerializeField] private SteamVR_Action_Boolean _triggerPress;
        [Space(10)]
        [SerializeField] private bool _pinPulled = false;
        [SerializeField] private bool _throwActivated = false;
        [Header("Settings")]
        [SerializeField] private GameObject _prefabThrowTrail;
        [SerializeField] private float _countdownTime = 2.5f;
        [Header("Pin Sounds")]
        [SerializeField] private AudioClip[] _pinPulloutSounds;
        [SerializeField] private Vector2 _minMaxPitchPinSound = new Vector2(0.7f, 1.2f);
        [SerializeField] private Vector2 _minMaxVolumePinSound = new Vector2(0.7f, 1.2f);
        [Header("Explosion Sounds")]
        [SerializeField] private AudioClip[] _audioClipExplosion;
        [SerializeField] private Vector2 _minMaxPitchExplosionSound = new Vector2(0.7f, 1.2f);
        [SerializeField] private Vector2 _minMaxVolumeExplosionSound = new Vector2(0.7f, 1.2f);
        [Header("Collision Sounds")]
        [SerializeField] private float _hitSpeedSoundTreshold = 1;
        [SerializeField] private AudioClip[] _audioClipHitSound;
        [Header("Pinpull feedback")]
        [SerializeField] private HapticFeedback _pinPullHaptics;
        [Header("Explosion settings")]
        [SerializeField] private float _explosionDistance = 1f;
        [SerializeField] private float _explosionForce = 100f;

        private ExplosionManager _explosionManager;
        private Coroutine _c_explosion;
        private Coroutine _c_audioCooldown;

        private const float _coolDownAudioTime = 0.25f;
        private const string _layerDefaultName = "Default";

        public override void Start()
        {
            base.Start();

            _explosionManager = ExplosionManager.Instance;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!AudioManager || _audioClipHitSound.Length == 0)
            {
                return;
            }

            if(_c_audioCooldown != null)
            {
                return;
            }

            if (Rigidbody.velocity.magnitude > _hitSpeedSoundTreshold)
            {
                AudioManager.PlayAudio3D(_audioClipHitSound, transform.position);
                _c_audioCooldown = StartCoroutine(AudioCooldown());
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            //On explosion to move rigidbodies
            if (other.attachedRigidbody)
            {
                Vector3 dir = other.attachedRigidbody.position - Rigidbody.position;
                Vector3 forceToApply = dir.normalized * _explosionForce;
                other.attachedRigidbody.AddForce(forceToApply);
            }
        }

        public override void OnPick(PVR_Hand pVR_Grab_Rigidbody_Object, bool matchRotationAndPosition = false)
        {
            base.OnPick(pVR_Grab_Rigidbody_Object, matchRotationAndPosition);

            _triggerPress.AddOnStateDownListener(OnTriggerPress, Hand.InputSource);
            //_triggerPress.AddOnStateUpListener(OnTriggerRelease, Hand.InputSource);
        }

        public override void OnDrop()
        {
            _triggerPress.RemoveOnStateDownListener(OnTriggerPress, Hand.InputSource);
            //_triggerPress.RemoveOnStateUpListener(OnTriggerRelease, Hand.InputSource);

            base.OnDrop();

            if(_pinPulled && !_throwActivated)
            {
                _throwActivated = true;
                GameObject tmpTrail = Instantiate(_prefabThrowTrail, transform.position, Quaternion.identity, transform);
                _c_explosion = StartCoroutine(C_Explosion());
            }
        }

        private void OnTriggerPress(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
        {
            if (_pinPulled)
            {
                return;
            }

            _pinPulled = true;

            //Make pin pullout sound
            AudioManager.PlayAudio3D(_pinPulloutSounds, transform.position, _minMaxPitchPinSound, _minMaxVolumePinSound);

            //Haptic feedback
            HapticFeedbackManager.HapticeFeedback(
                _pinPullHaptics.SecondsFromNow,
                _pinPullHaptics.Duration,
                _pinPullHaptics.Frequency,
                _pinPullHaptics.Amplitude,
                Hand.InputSource
                );
        }

        /// <summary>
        /// Explosion delay logic.
        /// Todo:explosion effects
        /// </summary>
        /// <returns></returns>
        IEnumerator C_Explosion()
        {
            yield return new WaitForSeconds(_countdownTime);

            //Freze the rigidbody;
            Rigidbody.velocity = Vector3.zero;
            Rigidbody.angularVelocity = Vector3.zero;
            Rigidbody.constraints = RigidbodyConstraints.FreezeAll;

            //yield return new WaitForFixedUpdate();

            ////Change all layers so hands don't trigger when this expands && children too!
            //foreach (Collider collider in Colliders)
            //{
            //    collider.gameObject.layer = LayerMask.NameToLayer(_layerDefaultName);
            //}

            //yield return new WaitForFixedUpdate();

            ////Switch all colliders to triggers
            //foreach (Collider collider in Colliders)
            //{
            //    collider.isTrigger = true;
            //}

            //yield return new WaitForFixedUpdate();

            //foreach (Collider collider in Colliders)
            //{
            //    ((SphereCollider)collider).radius = _explosionDistance;
            //}

            //Make explosion sound
            AudioManager.PlayAudio3D(_audioClipExplosion, transform.position, _minMaxPitchExplosionSound, _minMaxVolumeExplosionSound);

            //Make explosion visual - todo needs more stuff here
            //_explosionManager.CreateExplosion(transform.position);

            //Destroy(gameObject);
        }

        /// <summary>
        /// Used to ignore if audio collision happens to often, results in weird sound mixing.
        /// </summary>
        /// <returns></returns>
        IEnumerator AudioCooldown()
        {
            yield return new WaitForSeconds(_coolDownAudioTime);
            _c_audioCooldown = null;
        }
    }
}
