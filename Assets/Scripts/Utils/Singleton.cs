using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DivIt.Utils
{
    public class Singleton<T> : MonoBehaviour where T : Component
    {
        [Header("Singleton")]
        public bool IsPersistantBetweenScenes;

        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<T>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject();
                        go.name = typeof(T).Name;
                        _instance = go.AddComponent<T>();
                    }
                }

                return _instance;
            }

        }

        public virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                if (IsPersistantBetweenScenes)
                    DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }
    }
}
