using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DivIt.PlayVRoom.TestingShit
{
    public class MuzzelSpawn : MonoBehaviour
    {
        public GameObject[] muzzles;
        // Start is called before the first frame update
        IEnumerator Start()
        {
            while (true)
            {
                for (int i = 0; i < muzzles.Length; i++)
                {
                    GameObject tmp = Instantiate(muzzles[i], transform.position, transform.rotation, transform);
                    yield return new WaitForSeconds(1);
                    Destroy(tmp);
                }

                yield return new WaitForSeconds(1);
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    } 
}
