using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

namespace MarioHaberle.PlayVRoom.VR.Interaction
{
    public class PVR_Grenade : PVR_Rigidbody_Object
    {
        [Header("-----------------------------------------")]
        [SerializeField] private SteamVR_Action_Boolean _triggerPress;
        [Space(10)]
        [SerializeField] private bool _pinPulled = false;
        [SerializeField] private bool _throwActivated = false;
        [Header("Settings")]
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
        [SerializeField] private float _secondsFromNow = 0;
        [SerializeField] private float _duration = 0.01f;
        [SerializeField] private float _frequency = 50;
        [SerializeField] private float _amplitude = 25;

        private Coroutine _c_explosion;
        private Coroutine _c_audioCooldown;

        private const float _coolDownAudioTime = 0.25f;

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

            if(_pinPulled && _throwActivated == false)
            {
                _throwActivated = true;
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
                _secondsFromNow,
                _duration,
                _frequency,
                _amplitude,
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

            //Make explosion sound
            AudioManager.PlayAudio3D(_audioClipExplosion, transform.position, _minMaxPitchExplosionSound, _minMaxVolumeExplosionSound);

            yield return null;
            Destroy(gameObject);
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
