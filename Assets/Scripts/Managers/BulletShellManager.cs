using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MarioHaberle.PlayVRoom.Misc;

namespace MarioHaberle.PlayVRoom.Managers
{
    public class BulletShellManager : MonoBehaviour
    {
        [SerializeField] private int _maxBulletCount = 10;
        [SerializeField] private List<GameObject> _bullets;

        private void Awake()
        {
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

        void AddToQueue(GameObject bulletShell)
        {
            _bullets.Add(bulletShell);

            if(_bullets.Count >= _maxBulletCount)
            {
                GameObject tmpBulet = _bullets[0];
                _bullets.RemoveAt(0);
                Destroy(tmpBulet);
            }
        }

        void RemoveFromQueue(GameObject bulletShell)
        {
            _bullets.Remove(bulletShell);
        }

    } 
}
