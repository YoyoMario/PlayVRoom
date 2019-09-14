using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MarioHaberle.PlayVRoom.ScriptableObjects;

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
        [SerializeField] private Rigidbody _rigidbody;
        public PVR_Hand Hand;
        public bool SeenByCamera;

        public delegate void PVR_Interactable_Action();
        public event PVR_Interactable_Action OnPickAction;
        public event PVR_Interactable_Action OnDropAction;

        private Quaternion _objectRotationDifference;
        private Vector3 _objectPositionDifference;
        private Material _outlineMaterialAdded;
        private Coroutine _forceTowardsPlayer_Coroutine;
        private PhysicMaterial[] _originalPhysicsMaterials;

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
                if(HandPosition == null)
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
                if(HandPosition == null)
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

        public virtual void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            ControllerPhysics = Resources.Load("ControllerPhysics") as ControllerPhysics;
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
            
            Picked = true;
            Hand = pVR_Grab_Rigidbody_Object;
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

        public virtual void OnDrop(Vector3 controllerVelocity)
        {
            Picked = false;
            Hand = null;

            _rigidbody.maxAngularVelocity = ControllerPhysics.DefaultAngularVelocity;

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
