using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MarioHaberle.PlayVRoom.VR.Interaction;
using MarioHaberle.PlayVRoom.Managers;

namespace MarioHaberle.PlayVRoom.Misc
{
    public class BulletShell : MonoBehaviour
    {
        [SerializeField] private float _hitSpeedSoundTreshold = 1;
        [SerializeField] private AudioClip[] _audioClipHitSound;

        public delegate void BulletShellEvent(GameObject bulletShell);
        public static event BulletShellEvent OnBulletShellSpawn;
        public static event BulletShellEvent OnBulletPick;
        public static event BulletShellEvent OnBulletDrop;

        private PVR_Rigidbody_Object _pvrRigidbody;
        private AudioManager _audioManager;

        private void Awake()
        {
            _pvrRigidbody = GetComponent<PVR_Rigidbody_Object>();
            _audioManager = AudioManager.Instance;
        }

        private void Start()
        {
            if (OnBulletShellSpawn != null)
            {
                OnBulletShellSpawn(gameObject);
            }
        }

        private void OnEnable()
        {
            _pvrRigidbody.OnPickAction += OnPick;
            _pvrRigidbody.OnDropAction += OnDrop;
        }

        private void OnDisable()
        {
            _pvrRigidbody.OnPickAction -= OnPick;
            _pvrRigidbody.OnDropAction -= OnDrop;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!_audioManager || _audioClipHitSound.Length == 0)
            {
                return;
            }

            if(_pvrRigidbody.Rigidbody.velocity.magnitude > _hitSpeedSoundTreshold)
            {
                AudioClip audioClip = _audioClipHitSound[Random.Range(0, _audioClipHitSound.Length - 1)];
                _audioManager.PlayAudio3D(audioClip, transform.position);
            }   
        }

        void OnPick()
        {
            if (OnBulletPick != null)
            {
                OnBulletPick(gameObject);
            }
        }

        void OnDrop()
        {
            if (OnBulletDrop != null)
            {
                OnBulletDrop(gameObject);
            }
        }
    } 
}
