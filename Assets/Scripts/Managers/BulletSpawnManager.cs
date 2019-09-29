using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarioHaberle.PlayVRoom.Managers
{
    public class BulletSpawnManager : MonoBehaviour
    {
        public static BulletSpawnManager Instance;

        [SerializeField] private bool _consoleWrite;

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
        }

        public void CreateBullet(GameObject prefabBullet, Vector3 position, Quaternion rotation)
        {
            GameObject bullet = Instantiate(
                    prefabBullet,
                    position,
                    rotation,
                    transform
                );
        }
    } 
}
