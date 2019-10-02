using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DivIt.PlayVRoom.Managers
{
    public class BulletImpactManager : MonoBehaviour
    {
        public static BulletImpactManager Instance;

        [SerializeField] private GameObject _prefabImpact;

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

        public void CreateImpact(Vector3 position, Vector3 normal)
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
