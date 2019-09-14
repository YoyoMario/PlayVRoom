using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MarioHaberle.PlayVRoom.VR.Interaction;

namespace MarioHaberle.PlayVRoom.VR.Hologram
{
    public class PVR_Hologram_Spawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject[] _objectToCreate;
        [SerializeField] private Transform _spawnPoint;
        [SerializeField] private Material _hologramMaterial;
        [Header("Settings")]
        [SerializeField] private float _startingAlpha;
        [SerializeField] private float _endingAlpha;
        [SerializeField] private AnimationCurve _showAnimationCurve;
        [SerializeField] private float _appearSpeed;
        [Space(5)]
        [SerializeField] private float _glitchMaxEffect;
        [SerializeField] private float _glitchNormalEffect;
        [Header("Info - Current Object")]
        [SerializeField] private GameObject _currentObject;
        [SerializeField] private PVR_Interactable _currentInteractable;
        [SerializeField] private Collider[] _currentColliders;
        [SerializeField] private Rigidbody _currentRigidbody;
        [SerializeField] private MeshRenderer _currentMeshRenderer;
        [SerializeField] private Material _currentHologramMaterial;
        [SerializeField] private Material _currentMainMaterial;

        private Coroutine DisplayEffect_Coroutine;
        private Coroutine GlitchEffect_Coroutine;
        private Coroutine BlendHologramWithRealMaterial_Coroutine;

        private void Start()
        {
            SpawnObject();
        }

        private void Update()
        {
            if (_currentRigidbody)
            {
                _currentRigidbody.transform.Rotate(new Vector3(0, 1, 0), Space.Self);
            }
        }

        //private void OnTriggerExit(Collider other)
        //{
        //
        //}

        public void SpawnObject()
        {
            CreateObject();
        }
        public void DestroyObject()
        {
            RemoveObject();
        }

        private void CreateObject()
        {
            _currentObject = Instantiate(_objectToCreate[Random.Range(0, _objectToCreate.Length -1)], _spawnPoint.position, _spawnPoint.rotation, null);
            _currentInteractable = _currentObject.GetComponent<PVR_Interactable>();
            _currentColliders = _currentInteractable.Colliders;
            _currentRigidbody = _currentObject.GetComponent<Rigidbody>();
            _currentMeshRenderer = _currentObject.GetComponent<MeshRenderer>();

            //Update materials
            List<Material> materials = new List<Material>();
            foreach(Material mat in _currentMeshRenderer.materials)
            {
                materials.Add(mat);
            }
            materials.Add(_hologramMaterial);
            _currentMeshRenderer.materials = materials.ToArray();
            _currentMainMaterial = _currentMeshRenderer.materials[0]; //main material will always be the zero index material
            _currentHologramMaterial = _currentMeshRenderer.materials[_currentMeshRenderer.materials.Length-1]; //we assigned it few lines up the right way, not magic number way
            _currentMainMaterial.SetFloat("_AlphaValue", 0); //Set starting alpha to zero
            _currentHologramMaterial.SetFloat("_TimeRandomisation", Random.Range(0,360)); //Setting time offset for different object holo effect
            _currentHologramMaterial.SetColor("_MainColor", _currentMainMaterial.GetColor("_BaseColor"));

            foreach(Collider collider in _currentColliders)
            {
                collider.isTrigger = true;
            }
            _currentRigidbody.isKinematic = true;
            DisplayEffect_Coroutine = StartCoroutine(DisplayEffect());
            GlitchEffect_Coroutine =  StartCoroutine(GlitchEffect());

            _currentInteractable.OnPickAction += OnCurrentObjectPicked;
        }
 
        private void OnCurrentObjectPicked()
        {
            foreach (Collider collider in _currentColliders)
            {
                collider.isTrigger = false;
            }
            BlendHologramWithRealMaterial_Coroutine = StartCoroutine(BlendHologramWithRealMaterial());
        }

        private void RemoveObject()
        {
            _currentInteractable.OnPickAction -= OnCurrentObjectPicked;

            Destroy(_currentObject);
            _currentObject = null;
            _currentInteractable = null;
            _currentColliders = new Collider[0];
            _currentRigidbody = null;
            _currentMeshRenderer = null;
            _currentHologramMaterial = null;
        }

        IEnumerator DisplayEffect()
        {
            float timer = 0;
            float appearSpeed = _appearSpeed;
            float startingAlpha = _startingAlpha;
            float endingAlpha = _endingAlpha;
            float currentAlpha = startingAlpha;
            while (timer < 1)
            {
                timer += Time.deltaTime * appearSpeed;
                currentAlpha = Mathf.Lerp(startingAlpha, endingAlpha, _showAnimationCurve.Evaluate(timer));
                _currentHologramMaterial.SetFloat("_AlphaValue", currentAlpha);
                yield return null;
            }

            DisplayEffect_Coroutine = null;
        }

        IEnumerator GlitchEffect()
        {
            float timer = 0;
            float appearSpeed = _appearSpeed;

            float glitchOffSetX = 0;
            float glitchOffSetY = 0;
            float glitchOffSetZ = 0;

            while (timer < 1)
            {
                timer += Time.deltaTime * appearSpeed;

                glitchOffSetX = Random.Range(-_glitchMaxEffect, _glitchMaxEffect);
                glitchOffSetY = Random.Range(-_glitchMaxEffect, _glitchMaxEffect);
                glitchOffSetZ = Random.Range(-_glitchMaxEffect, _glitchMaxEffect);

                _currentHologramMaterial.SetVector("_GlitchOffset", new Vector4(glitchOffSetX, glitchOffSetY, glitchOffSetZ, 0));

                yield return null;
            }

            while (_currentHologramMaterial)
            {
                glitchOffSetX = Random.Range(-_glitchNormalEffect, _glitchNormalEffect);
                glitchOffSetY = Random.Range(-_glitchNormalEffect, _glitchNormalEffect);
                glitchOffSetZ = Random.Range(-_glitchNormalEffect, _glitchNormalEffect);

                _currentHologramMaterial.SetVector("_GlitchOffset", new Vector4(glitchOffSetX, glitchOffSetY, glitchOffSetZ, 0));

                yield return null;
            }

            GlitchEffect_Coroutine = null;
        }

        IEnumerator BlendHologramWithRealMaterial()
        {
            //Before we blend, let's stop glitch effect on the object
            _currentInteractable.OnPickAction -= OnCurrentObjectPicked;
            if (DisplayEffect_Coroutine != null) StopCoroutine(DisplayEffect_Coroutine);
            if (GlitchEffect_Coroutine != null) StopCoroutine(GlitchEffect_Coroutine);

            ////////////////////////////////////////////////////////////////////////////
            float timer = 0;
            float appearSpeed = 3;

            float hologramAlphaStart = _currentHologramMaterial.GetFloat("_AlphaValue");
            float hologramAlphaEnd = 0;
            float hologramCurrentAlpha = hologramAlphaStart;

            float mainAlphaStart = _currentMainMaterial.GetFloat("_AlphaValue");
            float mainAlphaEnd = 1;
            float mainCurrentAlpha = mainAlphaStart;

            while (timer < 1)
            {
                timer += Time.deltaTime * appearSpeed;

                hologramCurrentAlpha = Mathf.Lerp(hologramAlphaStart, hologramAlphaEnd, timer);
                _currentHologramMaterial.SetFloat("_AlphaValue", hologramCurrentAlpha);

                mainCurrentAlpha = Mathf.Lerp(mainAlphaStart, mainAlphaEnd, timer);
                _currentMainMaterial.SetFloat("_AlphaValue", mainCurrentAlpha);

                yield return null;
            }

            ////////////////////////////////////////////////////////////////////////////
            SpawnObject();

            BlendHologramWithRealMaterial_Coroutine = null;            
        }
    } 
}
