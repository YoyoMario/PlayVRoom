using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarioHaberle.PlayVRoom.Managers
{
    

    public class BulletSpawnManager : MonoBehaviour
    {
        public enum BulletType
        {
            nineMM,
            fiveFiveSix,
            sevenSixTwo
        }

        [System.Serializable]
        public class BulletPrefabAndType
        {
            public BulletType BulletType;
            public GameObject PrefabBullet;
        }

        public static BulletSpawnManager Instance;

        [SerializeField] private bool _consoleWrite;
        [SerializeField] private BulletPrefabAndType[] _bulletPrefabAndType;

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

        public void CreateBullet(Vector3 position, Quaternion rotation, BulletType bulletType)
        {
            int indexType = -1;
            for(int i = 0; i < _bulletPrefabAndType.Length; i++)
            {
                if(_bulletPrefabAndType[i].BulletType == bulletType)
                {
                    indexType = i;
                    break;
                }
            }

            GameObject bullet = Instantiate(
                    _bulletPrefabAndType[indexType].PrefabBullet,
                    position,
                    rotation,
                    transform
                );
        }
    } 
}
