using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DivIt.PlayVRoom.Managers;

namespace DivIt.PlayVRoom.Misc
{
    public class Destructible : MonoBehaviour
    {
        [SerializeField] private float _velocityToBreak;
        [SerializeField] private Rigidbody[] _destructedParts;
        [SerializeField] private float _forceFromMiddle = 20;
        [Header("Audio")]
        [SerializeField] private AudioClip[] _audioClipBreak;
        [SerializeField] private Vector2 minMaxVolume;
        [SerializeField] private Vector2 minMaxPitch;

        private AudioManager _audioManager;

        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private Rigidbody _rigidbody;
        private Collider _collider;

        private void Start()
        {
            _audioManager = AudioManager.Instance;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                StartCoroutine(Break());
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if(collision.relativeVelocity.magnitude > _velocityToBreak){
                StartCoroutine(Break());
            }
        }

        IEnumerator Break()
        {
            _meshFilter     = GetComponent<MeshFilter>();
            _meshRenderer   = GetComponent<MeshRenderer>();
            _rigidbody      = GetComponent<Rigidbody>();
            _collider       = GetComponent<Collider>();

            for(int i = 0; i < _destructedParts.Length; i++)
            {
                _destructedParts[i].gameObject.SetActive(true);

                Vector3 directionFromMiddle = transform.position - _destructedParts[i].transform.position;
                _destructedParts[i].AddForce(directionFromMiddle * _forceFromMiddle);
            }

            yield return null;

            Destroy(_meshFilter);
            Destroy(_meshRenderer);
            Destroy(_rigidbody);
            Destroy(_collider);

            yield return null;

            _audioManager.PlayAudio3D(_audioClipBreak, transform.position, minMaxPitch, minMaxVolume);

            yield return null;

            Destroy(this);
        }
    }
}