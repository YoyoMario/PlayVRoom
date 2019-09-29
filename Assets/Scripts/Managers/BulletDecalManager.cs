using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarioHaberle.PlayVRoom.Managers
{
    /// <summary>
    /// Uset to draw bullet decals from one place.
    /// </summary>
    public class BulletDecalManager : MonoBehaviour
    {
        public static BulletDecalManager Instance;

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

        /// <summary>
        /// Creates bullet decal on desired position, rotation and scale.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="scale"></param>
        public void CreateBulletDecal(Vector3 position, Quaternion rotation, Vector3 scale)
        {

        }
    } 
}
