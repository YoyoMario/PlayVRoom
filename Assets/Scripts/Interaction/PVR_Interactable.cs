using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

using DivIt.PlayVRoom.ScriptableObjects;
using DivIt.PlayVRoom.Managers;

namespace DivIt.PlayVRoom.VR.Interaction
{
    public class PVR_Interactable : MonoBehaviour
    {
        [Serializable]
        public class MeshHoverClass
        {
            public MeshRenderer Mesh;
            public Material[] DefaultMaterials;
            public Material[] OutlineMaterials;

            private void UpdateArrays(Material outlineMaterial)
            {
                if (!Mesh)
                {
                    Debug.LogError("Mesh not assigned!");
                    return;
                }

                //No need to initialize twice. Once is enough and it's optimized,
                //in a way only first time will allow initialization
                if(DefaultMaterials.Length != 0 && OutlineMaterials.Length != 0)
                {
                    return;
                }

                //Default materials
                DefaultMaterials = Mesh.materials;

                //Outline materials
                OutlineMaterials = new Material[DefaultMaterials.Length + 1];
                for(int i = 0; i < Mesh.materials.Length; i++)
                {
                    OutlineMaterials[i] = Mesh.materials[i];
                }
                OutlineMaterials[OutlineMaterials.Length - 1] = outlineMaterial;
            }

            public void AddOutline(MeshRenderer meshRenderer, Material outlineMaterial)
            {
                if (DefaultMaterials.Length == 0 || OutlineMaterials.Length == 0)
                {
                    //Debug.LogError("Can't add outline, default or outline material array missing.\n Don't worry I'll update this.", meshRenderer.gameObject);
                    UpdateArrays(outlineMaterial);
                    return;
                }

                Mesh.materials = OutlineMaterials;
            }

            public void RemoveOutline(MeshRenderer meshRenderer, Material outlineMaterial)
            {
                if (DefaultMaterials.Length == 0 || OutlineMaterials.Length == 0)
                {
                    //Debug.LogError("Can't remove outline, default or outline material array missing.\n Don't worry I'll update this.", meshRenderer.gameObject);
                    UpdateArrays(outlineMaterial);
                    return;
                }

                Mesh.materials = DefaultMaterials;
            }
        }

        [Header("References from project")]
        public Material OutlineMaterial;
        [Header("Pick up sounds")]
        private float _pickupVolume = 0.45f;
        [SerializeField] private AudioClip[] _audioClipPickUpSounds;
        [Header("References from object it self")]
        public MeshHoverClass[] MeshHover;
        public Collider[] Colliders;
        [Header("References from object it self - not necessary")]
        public Transform HandPosition;
        [Header("Added runtime")]
        [Header("-----------------------")]
        public ControllerPhysics ControllerPhysics;
        public HapticFeedback CollisionHaptics;
        public bool Picked;
        [SerializeField] private Rigidbody _rigidbody;
        public PVR_Hand Hand;
        public Hand SteamHand;

        public delegate void PVR_Interactable_Action();
        public event PVR_Interactable_Action OnPickAction;
        public event PVR_Interactable_Action OnDropAction;

        #region Private Variables

        private Quaternion _objectRotationDifference;
        private Vector3 _objectPositionDifference;
        private Coroutine _forceTowardsPlayer_Coroutine;
        private PhysicMaterial[] _originalPhysicsMaterials;

        //Average velocity storage
        private List<Vector3> _currentFrame_position;
        private List<Vector3> _previousFrame_position;
        private Vector3[] _sampledVelocity;
        private Vector3 _averageVelocity;

        //Average angular velocity storage
        private List<Vector3> _sampledAngularVelocities;
        private Vector3 _averageAngularVelocity;

        private AudioManager _audioManager;
        private HapticFeedbackManager _hapticFeedbackManger;

        #endregion

        #region Private Variables - Const

        private const int _averageVelocityFrameSamples = 3;

        #endregion

        #region Getters & Setters

        public Quaternion ObjectRotationDifference
        {
            get
            {
                return _objectRotationDifference;
            }
        }
        public Vector3 ObjectPositionDifference
        {
            get
            {
                return _objectPositionDifference;
            }
        }
        //USed like this because we can now set offset to the pivot of the objects that have handle points
        public Vector3 Position
        {
            get
            {
                if (HandPosition == null)
                {
                    return _rigidbody.position;
                }
                else
                {
                    return HandPosition.position;
                }
            }
        }
        //USed like this because we can now set offset to the pivot of the objects that have handle points
        public Quaternion Rotation
        {
            get
            {
                if (HandPosition == null)
                {
                    return _rigidbody.rotation;
                }
                else
                {
                    return HandPosition.rotation;
                }
            }

        }
        public Transform Transform
        {
            get
            {
                return transform;
            }
        }
        public Rigidbody Rigidbody
        {
            get
            {
                return _rigidbody;
            }
        } 
        public AudioManager AudioManager
        {
            get
            {
                return _audioManager;
            }
        }
        public HapticFeedbackManager HapticFeedbackManager
        {
            get
            {
                return _hapticFeedbackManger;
            }
        }

        #endregion

        public virtual void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            ControllerPhysics = Resources.Load("ControllerPhysics") as ControllerPhysics;
            CollisionHaptics = Resources.Load("CollisionHaptics") as HapticFeedback;
        }

        public virtual void Start()
        {
            _audioManager = AudioManager.Instance;
            _hapticFeedbackManger = HapticFeedbackManager.Instance;
        }

        public virtual void FixedUpdate()
        {
            //Physics calculations here...
            if (!_rigidbody)
            {
                return;
            }

            if (!Picked && !Hand)
            {
                return;
            }

            //Sample average velocity of this object.
            if (_currentFrame_position.Count < _averageVelocityFrameSamples)
            {
                _currentFrame_position.Add(_rigidbody.position);
            }
            else
            {
                _currentFrame_position.RemoveAt(0);
                _currentFrame_position.Add(_rigidbody.position);
            }

            _averageVelocity = Vector3.zero;
            if (_previousFrame_position.Count == _averageVelocityFrameSamples)
            {
                for (int i = 0; i < _averageVelocityFrameSamples; i++)
                {
                    _sampledVelocity[i] = (_currentFrame_position[i] - _previousFrame_position[i]) / Time.fixedDeltaTime;
                    _averageVelocity += _sampledVelocity[i];
                }
            }
            _averageVelocity /= _averageVelocityFrameSamples;

            if (_previousFrame_position.Count < _averageVelocityFrameSamples)
            {
                _previousFrame_position.Add(_rigidbody.position);
            }
            else
            {
                _previousFrame_position.RemoveAt(0);
                _previousFrame_position.Add(_rigidbody.position);
            }

            //Sample average angular velocity of this object.
            Vector3 controllerAngularVelocity = SteamHand.GetTrackedObjectAngularVelocity();
            if (_sampledAngularVelocities.Count < _averageVelocityFrameSamples)
            {
                _sampledAngularVelocities.Add(controllerAngularVelocity);
            }
            else
            {
                _sampledAngularVelocities.RemoveAt(0);
                _sampledAngularVelocities.Add(controllerAngularVelocity);
            }

            _averageAngularVelocity = Vector3.zero;
            if (_sampledAngularVelocities.Count == _averageVelocityFrameSamples)
            {
                for (int i = 0; i < _averageVelocityFrameSamples; i++)
                {
                    _averageAngularVelocity += _sampledAngularVelocities[i];
                }
            }
            _averageAngularVelocity /= _averageVelocityFrameSamples;
        }

        public virtual void OnCollisionStay(Collision collision)
        {
            if(!Picked || !Hand)
            {
                return;
            }

            Vector3 PositionInHand = Hand.Rigidbody.position;
            if (!HandPosition)
            {
                PositionInHand =
                PositionInHand +
                (Hand.transform.forward * ObjectPositionDifference.z) +
                (Hand.transform.up * ObjectPositionDifference.y) +
                (Hand.transform.right * ObjectPositionDifference.x);
            }
            float distance = Vector3.Distance(PositionInHand, Position);

            if(distance > 0.02f)
            {
                HapticFeedbackManager.HapticeFeedback(
                CollisionHaptics.SecondsFromNow,
                CollisionHaptics.Duration,
                CollisionHaptics.Frequency,
                CollisionHaptics.Amplitude,
                Hand.InputSource
                );
            }
        }

        public virtual void OnHoverStart()
        {
            //Adding outline material
            for (int i = 0; i < MeshHover.Length; i++)
            {
                MeshHover[i].AddOutline(MeshHover[i].Mesh, OutlineMaterial);
            }
        }

        public virtual void OnHoverEnd()
        {
            //Removing outline material
            for (int i = 0; i < MeshHover.Length; i++)
            {

                MeshHover[i].RemoveOutline(MeshHover[i].Mesh, OutlineMaterial);
            }
        }
        
        public virtual void OnPick(PVR_Hand pVR_Grab_Rigidbody_Object, bool matchRotationAndPosition = false)
        {
            if(Picked && Hand)
            {
                //Drop from that hand
                Hand.ForceDrop();
                OnDrop();
            }

            OnHoverEnd();

            if (Picked)
            {
                Hand.ForceDrop();
            }

            //Play pick sound
            if (_audioClipPickUpSounds.Length != 0)
            {
                AudioManager.PlayAudio3D(_audioClipPickUpSounds, transform.position, 1, _pickupVolume);
            }
            //initialize
            _currentFrame_position = new List<Vector3>();
            _previousFrame_position = new List<Vector3>();
            _sampledVelocity = new Vector3[_averageVelocityFrameSamples];

            _sampledAngularVelocities = new List<Vector3>();

            Picked = true;
            Hand = pVR_Grab_Rigidbody_Object;
            SteamHand = Hand.GetComponent<Hand>();
            _rigidbody.useGravity = false;
            _rigidbody.isKinematic = false;
            _rigidbody.maxAngularVelocity = ControllerPhysics.MaxAngularVelocity;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            _rigidbody.interpolation = RigidbodyInterpolation.Extrapolate;
            if (HandPosition)
            {
                _rigidbody.centerOfMass = HandPosition.localPosition;
            }

            //We don't want to position objects right if we grab them by the force
            if (matchRotationAndPosition)
            {
                _objectRotationDifference = Quaternion.Inverse(Hand.Rigidbody.rotation) * _rigidbody.rotation;
                _objectPositionDifference = Hand.transform.InverseTransformDirection(_rigidbody.position - Hand.Rigidbody.position);
            }
            
            if (Colliders.Length > 0)
            {
                _originalPhysicsMaterials = new PhysicMaterial[Colliders.Length];
                for(int i = 0; i < _originalPhysicsMaterials.Length; i++)
                {
                    _originalPhysicsMaterials[i] = Colliders[i].material;
                    Colliders[i].material = null;                   
                }
            }

            if (OnPickAction != null)
            {
                OnPickAction.Invoke();
            }
        }

        public virtual void OnDrop()
        {
            Picked = false;
            Hand = null;
            
            //Drop from hand sound
            if(_audioClipPickUpSounds.Length != 0)
            {
                AudioManager.PlayAudio3D(_audioClipPickUpSounds, transform.position, 1, _pickupVolume);
            }            

            _rigidbody.useGravity = true;
            _rigidbody.maxAngularVelocity = ControllerPhysics.DefaultAngularVelocity;
            _rigidbody.velocity = _averageVelocity;
            _rigidbody.angularVelocity = _averageAngularVelocity;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            _rigidbody.interpolation = RigidbodyInterpolation.None;
            _rigidbody.ResetCenterOfMass();

            _objectRotationDifference = Quaternion.Euler(0, 0, 0);
            _objectPositionDifference = Vector3.zero;

            if (Colliders.Length > 0)
            {
                for (int i = 0; i < _originalPhysicsMaterials.Length; i++)
                {
                    Colliders[i].material = _originalPhysicsMaterials[i];
                }
            }

            if (OnDropAction != null)
            {
                OnDropAction.Invoke();
            }
        }

        private void OnDrawGizmos()
        {
            if (!Picked) return;
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_rigidbody.worldCenterOfMass, 0.05f);
        }

        //Delete if unused
        //IEnumerator ForceTowardsPlayer(Transform controller)
        //{
        //    Vector3 targetPosition = controller.position;
        //    float minDistance = .45f;
        //    float vectorUpMultiplier = 0.3f;
        //    float velocityMultiplier = 5;
        //    float maxVelocity = 20;
        //    float minVelocity = 2.5f;
        //    float currentDistanceFromController = Vector3.Distance(targetPosition, transform.position);

        //    while (currentDistanceFromController > minDistance)
        //    {
        //        Vector3 dir = targetPosition - transform.position;
        //        dir += Vector3.up * vectorUpMultiplier;
        //        dir = dir.normalized;
        //        float velMultiplier = currentDistanceFromController * currentDistanceFromController * velocityMultiplier;
        //        velMultiplier = Mathf.Clamp(velMultiplier, minVelocity, maxVelocity);
        //        Rigidbody.velocity = dir * velMultiplier;

        //        currentDistanceFromController = Vector3.Distance(targetPosition, transform.position);
        //        yield return null;
        //    }

        //    _forceTowardsPlayer_Coroutine = null;
        //}

    } 
}
