using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarioHaberle.PlayVRoom.VR.Interaction
{
    public class PVR_Interactable : MonoBehaviour
    {
        [Header("References from project")]
        public Material OutlineMaterial;
        [Header("References from object it self")]
        public MeshRenderer Mesh;
        [Header("Added runtime")]
        [Header("-----------------------")]
        public bool Picked;
        public Rigidbody Rigidbody;
        public PVR_Hand Hand;
        public bool SeenByCamera;

        public delegate void PVR_Interactable_Action();
        public event PVR_Interactable_Action OnPickAction;
        public event PVR_Interactable_Action OnDropAction;

        private Material _outlineMaterialAdded;
        private Coroutine _forceTowardsPlayer_Coroutine;

        public virtual void Awake()
        {
            Rigidbody = GetComponent<Rigidbody>();
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


        public virtual void OnPick(PVR_Hand pVR_Grab_Rigidbody_Object)
        {
            OnHoverEnd();

            if (Picked)
            {
                Hand.ForceDrop();
            }
            
            Picked = true;
            Hand = pVR_Grab_Rigidbody_Object;
            Rigidbody.isKinematic = false;

            if(OnPickAction != null)
            {
                OnPickAction.Invoke();
            }
        }

        public virtual void OnDrop(Vector3 controllerVelocity)
        {
            Picked = false;
            Hand = null;

            if (OnDropAction != null)
            {
                OnDropAction.Invoke();
            }
        }

        IEnumerator ForceTowardsPlayer(Transform controller)
        {
            Vector3 targetPosition = controller.position;
            float minDistance = .45f;
            float vectorUpMultiplier = 0.3f;
            float velocityMultiplier = 5;
            float maxVelocity = 20;
            float minVelocity = 2.5f;
            float currentDistanceFromController = Vector3.Distance(targetPosition, transform.position);

            while (currentDistanceFromController > minDistance)
            {
                Vector3 dir = targetPosition - transform.position;
                dir += Vector3.up * vectorUpMultiplier;
                dir = dir.normalized;
                float velMultiplier = currentDistanceFromController * currentDistanceFromController * velocityMultiplier;
                velMultiplier = Mathf.Clamp(velMultiplier, minVelocity, maxVelocity);
                Rigidbody.velocity = dir * velMultiplier;

                currentDistanceFromController = Vector3.Distance(targetPosition, transform.position);
                yield return null;
            }

            _forceTowardsPlayer_Coroutine = null;
        }

    } 
}
