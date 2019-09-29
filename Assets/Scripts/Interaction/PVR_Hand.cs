using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using System;

using MarioHaberle.PlayVRoom.Managers;
using MarioHaberle.PlayVRoom.VR.Visualization;

namespace MarioHaberle.PlayVRoom.VR.Interaction
{
    [RequireComponent(typeof(PVR_Hand_Visualizer), typeof(SphereCollider))]
    public class PVR_Hand: MonoBehaviour
    {
        [Header("Steam hand input")]
        [SerializeField] private SteamVR_Input_Sources _inputSource;
        [SerializeField] private SteamVR_Action_Boolean _pickUp;
        [SerializeField] private SteamVR_Action_Boolean _touchpadTouch;

        [Header("Interaction Layers")]
        [SerializeField] private string _defaultLayer;
        [SerializeField] private string _vrHandsLayer;
        [SerializeField] private string _vrInteractableLayer;

        [Header("Raycast Force")]
        [SerializeField] private float _raycastDistance;
        [SerializeField] private float _raycastSphereRadius;
        [SerializeField] private float _raycastAssistTimer;

        [Header("Hand collider settings")]
        [SerializeField] private float _positionVelocityMultiplier = 2200;
        [SerializeField] private float _rotationVelocityMultiplier = 30f;
        [SerializeField] private Vector3 _positionOffset;
        [SerializeField] private float _handColliderRadius = 0.15f;
        [SerializeField] private bool _showMesh = false;
        [SerializeField] private Rigidbody _handRigidbody;
        [SerializeField] private Collider _handCollider;
        [SerializeField] private string _layerNameHandCollider; //we want hands colliders to be interactable only and ONLY with interactable objects, that way we can put our hands under the table and hit objects from the bottom

        [Header("Haptic feedback settings")]
        [SerializeField] private float _secondsFromNow = 0;
        [SerializeField] private float _duration = 0;
        [SerializeField] private float _frequency = 150;
        [SerializeField] private float _amplitude = 75;

        [Header("Info")]
        [SerializeField] private PVR_Interactable _currentInteractableObject;
        [SerializeField] private List<PVR_Interactable> _touching;
        [SerializeField] private PVR_Interactable[] _previousFrame_touching;
        [SerializeField] private Transform _raycastedObject;
        [SerializeField] private GameObject _forceEffectHelper;
        [SerializeField] private Vector3 _forceEffectHelperOffset;
        [SerializeField] private bool _touchpadTouching;

        #region Private

        HapticFeedbackManager _hapticFeedbackManager;
        Rigidbody _rigidbody;        
        SphereCollider _objectDetectionTrigger;

        Ray ray;
        RaycastHit _raycastHitInteractable;
        Coroutine _raycastAssist;

        //Wall detection raycast
        private RaycastHit _raycastHitWallDetection;

        #endregion

        #region Constants

        public const string _handHelperName = "HandHelper";
        public const string _untaggedTag = "Untagged";
        public const string _vrInteractableTag = "Vr_Interactable";

        #endregion

        #region Getters && Setters

        public SteamVR_Input_Sources InputSource
        {
            get
            {
                return _inputSource;
            }
        }
        public Rigidbody Rigidbody
        {
            get
            {
                return _rigidbody;
            }
        }
        public bool TouchpadTouching
        {
            get
            {
                return _touchpadTouching;
            }
        }
        public GameObject ForceEffectHelper
        {
            get
            {
                return _forceEffectHelper;
            }
        }
        public PVR_Interactable CurrentInteractableObject
        {
            get
            {
                return _currentInteractableObject;
            }
        }
        public int TouchingCount
        {
            get
            {
                return _touching.Count;
            }
        }

        #endregion

        private void Awake()
        {
            _rigidbody = gameObject.AddComponent<Rigidbody>();
            _rigidbody.useGravity = false;


            //Interaction layers setup
            int layerNameHandLayer = LayerMask.NameToLayer(_vrHandsLayer);
            int layerNameVrInteractionLayer = LayerMask.NameToLayer(_vrInteractableLayer);
            int layerNameHandCollider = LayerMask.NameToLayer(_layerNameHandCollider);
            for(int i = 0; i <= 31; i++) //Unity supports 31 layers
            {
                //leave default layer intact for this purpose
                if (i != 0)
                {
                    Physics.IgnoreLayerCollision(i, layerNameHandLayer, true);
                    Physics.IgnoreLayerCollision(i, layerNameVrInteractionLayer, true);
                }                
                Physics.IgnoreLayerCollision(i, layerNameHandCollider, true);
            }
            //Allow physics interaction between hand layer and vr interactable object
            Physics.IgnoreLayerCollision(layerNameHandLayer, layerNameVrInteractionLayer, false);
            Physics.IgnoreLayerCollision(layerNameVrInteractionLayer, layerNameVrInteractionLayer, false);
            //Allow physics interactio nbetween hand collider layer and vr interactable layer, and it self ofcourse
            Physics.IgnoreLayerCollision(layerNameHandCollider, layerNameVrInteractionLayer, false);            
            Physics.IgnoreLayerCollision(layerNameHandCollider, layerNameHandCollider, false);            

            //Initialize trigger collider
            _objectDetectionTrigger = GetComponent<SphereCollider>();
            _objectDetectionTrigger.isTrigger = true;


            //Initialize Lists
            _touching = new List<PVR_Interactable>();

            //Initialize ray
            ray = new Ray();
            _raycastHitInteractable = new RaycastHit();
            _raycastHitWallDetection = new RaycastHit();


            //Create force effect helper
            _forceEffectHelper = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _forceEffectHelper.name = "ForceEffectHelper";
            _forceEffectHelper.transform.SetParent(transform);
            _forceEffectHelper.transform.localPosition = _forceEffectHelperOffset;
            _forceEffectHelper.transform.localScale = Vector3.one * 0.03f;
            Destroy(_forceEffectHelper.GetComponent<BoxCollider>());
            if (_inputSource == SteamVR_Input_Sources.RightHand)
            {
                _forceEffectHelperOffset.x *= -1;
                _forceEffectHelper.transform.rotation = Quaternion.Euler(180, 90, 90);
            }
            else if (_inputSource == SteamVR_Input_Sources.LeftHand)
            {
                _forceEffectHelper.transform.rotation = Quaternion.Euler(0, 90, 90);
            }


            //Create hand collider
            GameObject tmpGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tmpGo.layer = LayerMask.NameToLayer(_layerNameHandCollider);

            if (!_showMesh)
            {
                Destroy(tmpGo.GetComponent<MeshFilter>());
                Destroy(tmpGo.GetComponent<MeshRenderer>());
            }
            tmpGo.name = "HandCollider-" + transform.name.ToString();
            tmpGo.transform.localScale = Vector3.one * _handColliderRadius;
            _handRigidbody = tmpGo.AddComponent<Rigidbody>();
            _handRigidbody.useGravity = false;
            _handRigidbody.position = transform.position;
            _handCollider = tmpGo.GetComponent<BoxCollider>();
            _handCollider.enabled = false;
            DisableHandColliders();
        }

        private void Start()
        {
            _hapticFeedbackManager = HapticFeedbackManager.Instance;
        }

        public void OnEnable()
        {
            //Pick/drop object.
            _pickUp.AddOnStateDownListener(OnPickClick, _inputSource);
            _pickUp.AddOnStateUpListener(OnDropClick, _inputSource);

            //StarWars effect object.
            _pickUp.AddOnStateDownListener(OnForceEffect, _inputSource);

            //Touchpad
            _touchpadTouch.AddOnStateDownListener(OnTouchpadTouch, _inputSource);
            _touchpadTouch.AddOnStateUpListener(OnTouchpadRelease, _inputSource);
        }

        public void OnDisable()
        {
            _pickUp.RemoveOnStateDownListener(OnPickClick, _inputSource);   
            _pickUp.RemoveOnStateUpListener(OnDropClick, _inputSource);

            _pickUp.RemoveOnStateDownListener(OnForceEffect, _inputSource);

            _touchpadTouch.RemoveOnStateDownListener(OnTouchpadTouch, _inputSource);
            _touchpadTouch.RemoveOnStateDownListener(OnTouchpadRelease, _inputSource);
        }

        private void Update()
        {
            //Haptic feedback
            if (!_currentInteractableObject && _touching.Count != 0 /*&& IsHandAsCollider() == false*/)
            {
                _hapticFeedbackManager.HapticeFeedback(
                    _secondsFromNow,
                    _duration,
                    _frequency,
                    _amplitude,
                    _inputSource
                    );
            }

            //Unpaint the outline
            foreach (PVR_Interactable pVR_Interactable in _previousFrame_touching)
            {
                pVR_Interactable.OnHoverEnd();
            }            
        }

        private void LateUpdate()
        {
            //Paint the outline here so we don't fight with other hand. Paint in late update!
            if (!_currentInteractableObject)
            {
                if (_touching.Count != 0)
                {
                    _touching[_touching.Count - 1].OnHoverStart();
                }
            }
            _previousFrame_touching = new PVR_Interactable[_touching.Count];
            Array.Copy(_touching.ToArray(), _previousFrame_touching, _previousFrame_touching.Length);
            _previousFrame_touching = _touching.ToArray();
        }

        private void FixedUpdate()
        {
            //Star wars force effect raycasting
            ForceEffectRaycast();

            //Hand collider for fist
            //position
            Vector3 positionDelta = transform.position - _handRigidbody.position;
            positionDelta =
                positionDelta +
                (transform.forward * _positionOffset.z) +
                (transform.up * _positionOffset.y) +
                (transform.right * _positionOffset.x);
            Vector3 velocityDir = positionDelta * _positionVelocityMultiplier * Time.fixedDeltaTime;
            _handRigidbody.velocity = velocityDir;
            //otation
            Quaternion rotationDelta = transform.rotation * Quaternion.Inverse(_handRigidbody.rotation);
            float angle;
            Vector3 axis;
            rotationDelta.ToAngleAxis(out angle, out axis);
            if (angle >= 180)
            {
                angle -= 360;
            }
            Vector3 wantedRotation = (Time.fixedDeltaTime * angle * axis) * _rotationVelocityMultiplier;
            if (!float.IsNaN(wantedRotation.x) && !float.IsNaN(wantedRotation.y) && !float.IsNaN(wantedRotation.z))
            {
                _handRigidbody.angularVelocity = wantedRotation;
            }
        }

        /// <summary>
        /// Shots a raycast trying to find object to pick up.
        /// Pickup is done on _trigger click event that is setup in OnEnable.
        /// This just looks for objects and selects them.
        /// </summary>
        private void ForceEffectRaycast()
        {
            if (!_currentInteractableObject && _touching.Count == 0 && _touchpadTouching)
            {
                ray.origin = _forceEffectHelper.transform.position;
                ray.direction = _forceEffectHelper.transform.forward; //basically down;/* * _raycastForwardCorrection.z + transform.up * _raycastForwardCorrection.y + transform.right * _raycastForwardCorrection.x;*/

                //Wall detection
                bool wallHit = false;
                int defaultLayerInteger = 1 << LayerMask.NameToLayer(_defaultLayer);
                int vrInteractableLayerInteger = 1 << LayerMask.NameToLayer(_vrInteractableLayer);
                if (Physics.Raycast(ray.origin, ray.direction, out _raycastHitWallDetection, _raycastDistance, defaultLayerInteger | vrInteractableLayerInteger))
                {
                    if (!_raycastHitWallDetection.transform.CompareTag(_vrInteractableLayer))
                    {
                        wallHit = true;
                    }
                    else
                    {
                        wallHit = false;
                    }
                }

                if (Physics.SphereCast(ray.origin, _raycastSphereRadius, ray.direction, out _raycastHitInteractable, _raycastDistance, vrInteractableLayerInteger))
                {
                    //Don't hover nor select objects if we are holding something already
                    if (_raycastHitInteractable.transform && _raycastHitInteractable.transform.CompareTag(_vrInteractableTag) && wallHit == false)
                    {
                        if (_raycastedObject != _raycastHitInteractable.transform)
                        {
                            //Stop selection assist if running
                            if (_raycastAssist != null)
                            {
                                StopCoroutine(_raycastAssist);
                                _raycastAssist = null;
                            }

                            //Deselect currently or previously selected object via raycast
                            if (_raycastedObject)
                            {
                                _raycastedObject.transform.GetComponent<PVR_Interactable>().OnHoverEnd();
                            }

                            //Selected new hovered object
                            _raycastedObject = _raycastHitInteractable.transform;
                            PVR_Interactable pvrInteractable = _raycastedObject.transform.GetComponent<PVR_Interactable>();
                            pvrInteractable.OnHoverStart();
                        }
                    }
                    else //Deselect object that was previouisly hovered if not hovered anymore
                    {
                        if (_raycastedObject && _raycastAssist == null)
                        {
                            _raycastAssist = StartCoroutine(RaycastAssist());
                        }
                    }
                }
                else //Deselect object that was previouisly hovered if not hovered anymore
                {
                    if (_raycastedObject && _raycastAssist == null)
                    {
                        _raycastAssist = StartCoroutine(RaycastAssist());
                    }
                }
            }
            else
            {
                //Deselect currently or previously selected object via raycast
                if (_raycastedObject)
                {
                    PVR_Interactable pvrInteractable = _raycastedObject.transform.GetComponent<PVR_Interactable>();
                    pvrInteractable.OnHoverEnd();
                }
                _raycastedObject = null;
            }
        }

        /// <summary>
        /// After raycast force effect is done and nothing else is under controller raycast, hold this object as selected for N amount of time for easier pickup.
        /// </summary>
        /// <returns></returns>
        IEnumerator RaycastAssist()
        {
            yield return new WaitForSeconds(_raycastAssistTimer);
            //Deselect currently or previously selected object via raycast
            if (_raycastedObject)
            {
                PVR_Interactable pvrInteractable = _raycastedObject.transform.GetComponent<PVR_Interactable>();
                pvrInteractable.OnHoverEnd();
            }
            _raycastedObject = null;

            _raycastAssist = null;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(_vrInteractableTag))
            {
                PVR_Interactable pvrInteractable = other.GetComponent<PVR_Interactable>();
                if (pvrInteractable.GetComponent<PVR_Collider_Interactable>())
                {
                    pvrInteractable = pvrInteractable.GetComponent<PVR_Collider_Interactable>().InteractableObject;
                }

                if (!_touching.Contains(pvrInteractable))
                {
                    _touching.Add(pvrInteractable);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(_vrInteractableTag))
            {
                PVR_Interactable pvrInteractable = other.GetComponent<PVR_Interactable>();
                if (pvrInteractable.GetComponent<PVR_Collider_Interactable>())
                {
                    pvrInteractable = pvrInteractable.GetComponent<PVR_Collider_Interactable>().InteractableObject;
                }

                if (_touching.Contains(pvrInteractable))
                {
                    _touching.Remove(pvrInteractable);
                }
            }
        }

        private void OnPickClick(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
        {
            if (_touching.Count == 0)
            {
                EnableHandColliders();
                return;
            }

            //Lose the raycasted object
            if (_raycastedObject)
            {
                _raycastedObject.GetComponent<PVR_Interactable>().OnHoverEnd();
                _raycastedObject = null;
            }

            //Getting the last touched object
            _currentInteractableObject = _touching[_touching.Count - 1];
            //Update picked object
            _currentInteractableObject.OnPick(this, true);
            //Disable this trigger so we don't interact with this controller anymore whilst holding
            _objectDetectionTrigger.enabled = false;
            //Disabling hand colliders if they were active.
            DisableHandColliders();
        }

        private void OnDropClick(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
        {
            if (!_currentInteractableObject)
            {
                DisableHandColliders();
                return;
            }

            _currentInteractableObject.OnDrop();
            ForceDrop();
        }

        public void ForceDrop()
        {
            if (!_currentInteractableObject)
            {
                return;
            }

            _currentInteractableObject = null;
            _objectDetectionTrigger.enabled = true;
            _touching.Clear();
        }

        private void OnForceEffect(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
        {
            if (!_raycastedObject)
            {
                return;
            }

            //Dont use force if we are hovering somethign with controller
            if(_touching.Count > 0)
            {
                return;
            }

            //_raycastedObject.GetComponent<PVR_Interactable>().OnForceTowardsPlayer(transform);

            //Getting the last touched object
            _currentInteractableObject = _raycastedObject.GetComponent<PVR_Interactable>();
            //Update picked object
            _currentInteractableObject.OnPick(this, false);
            //Disable this trigger so we don't interact with this controller anymore whilst holding
            _objectDetectionTrigger.enabled = false;
            //Disabling hand colliders if they were active.
            DisableHandColliders();
        }

        private void OnTouchpadTouch(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
        {
            _touchpadTouching = true;
        }

        private void OnTouchpadRelease(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
        {
            _touchpadTouching = false;
        }

        private void EnableHandColliders()
        {
            _handCollider.enabled = true;
        }

        private void DisableHandColliders()
        {
            _handCollider.enabled = false;
        }

        //private bool IsHandAsCollider()
        //{
        //    if (_handCollider.enabled)
        //    {
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}
    }
}
