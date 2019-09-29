using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MarioHaberle.PlayVRoom.Misc;

namespace MarioHaberle.PlayVRoom.Managers
{
    public class BulletShellManager : MonoBehaviour
    {
        public static BulletShellManager Instance;

        [SerializeField] private int _maxBulletCount = 10;
        [SerializeField] private List<GameObject> _bullets;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                //DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }

            _bullets = new List<GameObject>();
        }

        private void OnEnable()
        {
            BulletShell.OnBulletShellSpawn += AddToQueue;
            BulletShell.OnBulletPick += RemoveFromQueue;
            BulletShell.OnBulletDrop += AddToQueue;
        }

        private void OnDisable()
        {
            BulletShell.OnBulletShellSpawn -= AddToQueue;
            BulletShell.OnBulletPick -= RemoveFromQueue;
            BulletShell.OnBulletDrop -= AddToQueue;
        }

        public void CreateBulletShell(GameObject prefabShell, Vector3 position, Quaternion rotation, Vector3 initialVelocity, Vector3 shellForce)
        {
            if(_bullets.Count >= _maxBulletCount)
            {
                //Do object pulling
                GameObject tmpBulletShell = _bullets[0];
                _bullets.RemoveAt(0);
                _bullets.Add(tmpBulletShell);

                Rigidbody tmpRbShell = tmpBulletShell.GetComponent<Rigidbody>();
                tmpRbShell.position = position;
                tmpRbShell.rotation = rotation;
                tmpRbShell.velocity = initialVelocity;
                tmpRbShell.AddForce(shellForce);
            }
            else
            {
                //Create new bullet shell
                GameObject tmpBulletShell = Instantiate(prefabShell, position, rotation, transform);
                Rigidbody tmpRbShell = tmpBulletShell.GetComponent<Rigidbody>();
                tmpRbShell.velocity = initialVelocity;
                tmpRbShell.AddForce(shellForce);
            }            
        }

        void AddToQueue(GameObject bulletShell)
        {
            _bullets.Add(bulletShell);
        }

        void RemoveFromQueue(GameObject bulletShell)
        {
            _bullets.Remove(bulletShell);
        }

    } 
}
