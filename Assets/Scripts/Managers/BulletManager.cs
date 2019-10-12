using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DivIt.PlayVRoom.Misc;
using DivIt.Utils;

namespace DivIt.PlayVRoom.Managers
{
    public class BulletManager : Singleton<BulletManager>
    {
        [Header("----------------------------------------------------------------")]
        [SerializeField] private int _maxBulletShellCount = 20;
        [SerializeField] private GameObject _prefabImpact;

        private List<GameObject> _bullets;

        public override void Awake()
        {
            base.Awake();
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


        /// <summary>
        /// Creates a bullet prefab.
        /// </summary>
        /// <param name="prefabBullet"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        public void CreateBullet(GameObject prefabBullet, Vector3 position, Quaternion rotation)
        {
            GameObject bullet = Instantiate(
                    prefabBullet,
                    position,
                    rotation,
                    transform
                );
            bullet.GetComponent<Bullet>().BulletManager = this;
        }


        /// <summary>
        /// Creates a bullet shell.
        /// </summary>
        /// <param name="prefabShell"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="initialVelocity"></param>
        /// <param name="shellForce"></param>
        public void CreateBulletShell(GameObject prefabShell, Vector3 position, Quaternion rotation, Vector3 initialVelocity, Vector3 shellForce)
        {
            if (_bullets.Count >= _maxBulletShellCount)
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


        /// <summary>
        /// Creates a muzzel flash.
        /// </summary>
        /// <param name="prefabMuzzels"></param>
        /// <param name="muzzelSpawnPoint"></param>
        public void CreateMuzzelFlash(GameObject[] prefabMuzzels, Transform muzzelSpawnPoint)
        {
            float randomRotation = Random.Range(0, 360);
            Quaternion rotationRandomAngle = Quaternion.Euler(0, 0, randomRotation);
            GameObject tmpPrefab = prefabMuzzels[Random.Range((int)0, (int)prefabMuzzels.Length - 1)];
            GameObject tmpMuzzel =
                Instantiate(
                    tmpPrefab,
                    muzzelSpawnPoint.position,
                    muzzelSpawnPoint.rotation * rotationRandomAngle,
                    transform
                    );
            Destroy(tmpMuzzel, 1.5f);
            StartCoroutine(FollowTheTransform(tmpMuzzel, muzzelSpawnPoint));
        }
        IEnumerator FollowTheTransform(GameObject tmpMuzzel, Transform muzzelSpawnPoint)
        {
            while (tmpMuzzel)
            {
                tmpMuzzel.transform.position = muzzelSpawnPoint.position;
                yield return null;
            }
        }


        /// <summary>
        /// Creates a impact particle on a surface of hit object.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="normal"></param>
        public void CreateImpactParticles(Vector3 position, Vector3 normal)
        {
            GameObject tmpImpact = Instantiate(
                _prefabImpact,
                position + normal * 0.05f,
                Quaternion.LookRotation(normal),
                transform
                );
            Destroy(tmpImpact, 2f);
        }
    } 
}
