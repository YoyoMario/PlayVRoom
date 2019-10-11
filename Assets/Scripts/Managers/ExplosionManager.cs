using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DivIt.Utils;

namespace DivIt.PlayVRoom.Managers
{
    public class ExplosionManager : Singleton<ExplosionManager>
    {
        [Header("-------------------------------------------")]
        [SerializeField] private GameObject[] _prefabExplosion;

        public void CreateExplosion(Vector3 position)
        {
            GameObject randomExplosion = _prefabExplosion[Random.Range((int)0, (int)(_prefabExplosion.Length - 1))];
            GameObject tmpObject = Instantiate(
                randomExplosion,
                position, 
                Quaternion.identity,
                transform
                );
            Destroy(tmpObject, 5.0f);
        }
    } 
}