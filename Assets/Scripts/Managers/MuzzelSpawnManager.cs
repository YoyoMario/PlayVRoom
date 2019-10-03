using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DivIt.PlayVRoom.Managers
{
    public class MuzzelSpawnManager : MonoBehaviour
    {
        public static MuzzelSpawnManager Instance;

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
    } 
}
