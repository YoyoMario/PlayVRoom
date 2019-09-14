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
        [Header("Added runtime")]
        [Header("-----------------------")]
        public ControllerPhysics ControllerPhysics;
        public bool Picked;
        public Rigidbody Rigidbody;
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

        public virtual void Awake()
        {
            Rigidbody = GetComponent<Rigidbody>();
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
            Rigidbody.isKinematic = false;
            Rigidbody.maxAngularVelocity = ControllerPhysics.MaxAngularVelocity;

            //We don't want to position objects right if we grab them by the force
            if (matchRotationAndPosition)
            {
                _objectRotationDifference = Quaternion.Inverse(Hand.Rigidbody.rotation) * Rigidbody.rotation;
                _objectPositionDifference = Hand.transform.InverseTransformDirection(Rigidbody.position - Hand.Rigidbody.position);
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

            Rigidbody.maxAngularVelocity = ControllerPhysics.DefaultAngularVelocity;

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
