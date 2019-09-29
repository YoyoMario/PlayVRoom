using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarioHaberle.PlayVRoom.Managers
{
    /// <summary>
    /// Used to play sounds from one place.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance;

        [SerializeField] private bool _consoleWrite;

        private Coroutine _c_RecycleAudio;

        private void Awake()
        {
            if(Instance == null)
            {
                Instance = this;
                //DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            _c_RecycleAudio = StartCoroutine(RecycleAudio_Coroutine());
        }

        IEnumerator RecycleAudio_Coroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(1);
                AudioSource[] audioSources = transform.GetComponentsInChildren<AudioSource>();
                List<GameObject> destroy = new List<GameObject>();
                for(int i = 0; i < audioSources.Length; i++)
                {
                    if (!audioSources[i].isPlaying)
                    {
                        destroy.Add(audioSources[i].gameObject);
                    }
                }

                foreach(GameObject go in destroy)
                {
                    Destroy(go);
                }
            }
        }

        /// <summary>
        /// Will create audio source component that will play audio clip in a 2D setting
        /// </summary>
        /// <param name="audioClip"></param>
        public AudioSource PlayAudio2D(AudioClip audioClip)
        {
            if (_consoleWrite)
            {
                Debug.Log("AudioManager :: PlayAudio2D()");
            }

            GameObject tmpGo = new GameObject("_audio");
            Transform tmpT = tmpGo.transform;
            tmpT.SetParent(transform);

            AudioSource audioSource = tmpGo.AddComponent<AudioSource>();
            audioSource.clip = audioClip;
            audioSource.spatialBlend = 0.0f;
            audioSource.Play();

            return audioSource;
        }

        /// <summary>
        /// Creates audio source with 3D audio settings at specific world position.
        /// </summary>
        /// <param name="audioClip"></param>
        /// <param name="worldPosition"></param>
        /// <returns></returns>
        public AudioSource PlayAudio3D(AudioClip[] audioClipArray, Vector3 worldPosition, float pitch = 1, float volume = 1f, float delay = 0f)
        {
            if (_consoleWrite)
            {
                Debug.Log("AudioManager :: PlayAudio3D()");
            }

            AudioClip audioClip = audioClipArray[Random.Range((int)0, (int)audioClipArray.Length - 1)];

            GameObject tmpGo = new GameObject("_audio");
            Transform tmpT = tmpGo.transform;
            tmpT.SetParent(transform);
            tmpT.position = worldPosition;

            AudioSource audioSource = tmpGo.AddComponent<AudioSource>();
            audioSource.clip = audioClip;
            audioSource.spatialBlend = 1f;
            audioSource.volume = volume;
            audioSource.pitch = pitch;

            audioSource.PlayDelayed(delay);

            return audioSource;
        }

        public AudioSource PlayAudio3D(AudioClip[] audioClipArray,Vector3 worldPosition, Vector2 minMaxPitch, Vector2 minMaxVolume, float delay = 0f)
        {
            AudioClip audioClip = audioClipArray[Random.Range((int)0, (int)audioClipArray.Length - 1)];
            float pitch = (minMaxPitch.x != minMaxPitch.y) ? Random.Range((float)minMaxPitch.x, (float)minMaxPitch.y) : 1;
            float volume = (minMaxVolume.x != minMaxVolume.y) ? Random.Range((float)minMaxVolume.x, (float)minMaxVolume.y) : 1;

            if (_consoleWrite)
            {
                Debug.Log("AudioManager :: PlayAudio3D()");
            }

            GameObject tmpGo = new GameObject("_audio");
            Transform tmpT = tmpGo.transform;
            tmpT.SetParent(transform);
            tmpT.position = worldPosition;

            AudioSource audioSource = tmpGo.AddComponent<AudioSource>();
            audioSource.clip = audioClip;
            audioSource.spatialBlend = 1f;
            audioSource.volume = volume;
            audioSource.pitch = pitch;

            audioSource.PlayDelayed(delay);

            return audioSource;
        }
    }
}