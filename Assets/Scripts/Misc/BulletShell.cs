using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MarioHaberle.PlayVRoom.VR.Interaction;

namespace MarioHaberle.PlayVRoom.Misc
{
    public class BulletShell : MonoBehaviour
    {
        public delegate void BulletShellEvent(GameObject bulletShell);
        public static event BulletShellEvent OnBulletShellSpawn;
        public static event BulletShellEvent OnBulletPick;
        public static event BulletShellEvent OnBulletDrop;

        private PVR_Rigidbody_Object _pvrRigidbody;

        private void Awake()
        {
            _pvrRigidbody = GetComponent<PVR_Rigidbody_Object>();    
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
