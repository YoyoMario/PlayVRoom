using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

using MarioHaberle.PlayVRoom.ScriptableObjects;
using MarioHaberle.PlayVRoom.Managers;

namespace MarioHaberle.PlayVRoom.VR.Interaction
{
    public class PVR_Interactable : MonoBehaviour
    {
        [Header("References from project")]
        public Material OutlineMaterial;
        [Header("References from object it self")]
        public MeshRenderer Mesh;
        public Collider[] Colliders;
        [Header("References from object it self - not necessary")]
        public Transform HandPosition;
        [Header("Added runtime")]
        [Header("-----------------------")]
        public ControllerPhysics ControllerPhysics;
        public bool Picked;
        public AudioManager AudioManager;
        [SerializeField] private Rigidbody _rigidbody;
        public PVR_Hand Hand;
        public Hand SteamHand;
        public bool SeenByCamera;

        public delegate void PVR_Interactable_Action();
        public event PVR_Interactable_Action OnPickAction;
        public event PVR_Interactable_Action OnDropAction;

        #region Private Variables

        private Quaternion _objectRotationDifference;
        private Vector3 _objectPositionDifference;
        private Material _outlineMaterialAdded;
        private Coroutine _forceTowardsPlayer_Coroutine;
        private PhysicMaterial[] _originalPhysicsMaterials;
        private Transform _originalParent;

        //Average velocity storage
        private List<Vector3> _currentFrame_position;
        private List<Vector3> _previousFrame_position;
        private Vector3[] _sampledVelocity;
        private Vector3 _averageVelocity;

        //Average angular velocity storage
        private List<Vector3> _sampledAngularVelocities;
        private Vector3 _averageAngularVelocity;

        #endregion

        #region Private Variables - Const

        private const int _averageVelocityFrameSamples = 3;
        private const float _angularVelocityMultiplier = 1;

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

        #endregion

        public virtual void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            ControllerPhysics = Resources.Load("ControllerPhysics") as ControllerPhysics;
        }

        public virtual void Start()
        {
            AudioManager = AudioManager.Instance;
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

        public virtual void OnHoverStart()
        {
            if (_outlineMaterialAdded != null) return;

            //Adding outline material
            List<Material> materials = new List<Material>();
            foreach (Material mat in Mesh.materials)
            {
                materials.Add(mat);
            }
            materials.Add(OutlineMaterial);
            Mesh.materials = materials.ToArray();
            _outlineMaterialAdded = Mesh.materials[Mesh.materials.Length-1];
        }

        public virtual void OnHoverEnd()
        {
            //Removing outline material
            List<Material> materials = new List<Material>();
            foreach (Material mat in Mesh.materials)
            {
                materials.Add(mat);
            }
            materials.Remove(_outlineMaterialAdded);
            Mesh.materials = materials.ToArray();
            _outlineMaterialAdded = null;
        }


        public virtual void OnPick(PVR_Hand pVR_Grab_Rigidbody_Object, bool matchRotationAndPosition = false)
        {
            OnHoverEnd();

            if (Picked)
            {
                Hand.ForceDrop();
            }

            //initialize
            _currentFrame_position = new List<Vector3>();
            _previousFrame_position = new List<Vector3>();
            _sampledVelocity = new Vector3[_averageVelocityFrameSamples];

            _sampledAngularVelocities = new List<Vector3>();


            if (!_originalParent) _originalParent = Transform.parent;
            Transform.SetParent(pVR_Grab_Rigidbody_Object.transform);

            Picked = true;
            Hand = pVR_Grab_Rigidbody_Object;
            SteamHand = Hand.GetComponent<Hand>();
            _rigidbody.useGravity = false;
            _rigidbody.isKinematic = false;
            _rigidbody.maxAngularVelocity = ControllerPhysics.MaxAngularVelocity;

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

            Transform.SetParent(_originalParent.transform);

            _rigidbody.useGravity = true;
            _rigidbody.maxAngularVelocity = ControllerPhysics.DefaultAngularVelocity;
            _rigidbody.velocity = _averageVelocity;
            _rigidbody.angularVelocity = _averageAngularVelocity * _angularVelocityMultiplier;

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
