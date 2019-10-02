using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DivIt.PlayVRoom.VR.Interaction;

namespace DivIt.PlayVRoom.VR.Visualization
{
    [RequireComponent(typeof(PVR_Hand))]
    public class PVR_Hand_Visualizer : MonoBehaviour
    {
        [Header("Line renderer settings")]
        [SerializeField] private float _lineRendererLength = 1f;
        [SerializeField] private float _lineRendererStartWidth = 0.5f;
        [SerializeField] private float _lineRendererEndWidth = 0.1f;

        private PVR_Hand _pvr_grab_rigidbody;
        private LineRenderer _lineRenderer;

        private void Awake()
        {
            _pvr_grab_rigidbody = GetComponent<PVR_Hand>();
        }

        private void Start()
        {
            //Initialize line renderer
            _lineRenderer = _pvr_grab_rigidbody.ForceEffectHelper.AddComponent<LineRenderer>();
            _lineRenderer.useWorldSpace = false;
            _lineRenderer.receiveShadows = false;
            _lineRenderer.allowOcclusionWhenDynamic = false;
            _lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            Vector3[] positions = new Vector3[2];
            positions[0] = new Vector3(0, 0, 0);
            positions[1] = new Vector3(0, 0, _lineRendererLength);
            _lineRenderer.SetPositions(positions);
            _lineRenderer.startWidth = _lineRendererStartWidth;
            _lineRenderer.endWidth = _lineRendererEndWidth;
        }

        private void Update()
        {
            _lineRenderer.enabled = _pvr_grab_rigidbody.TouchpadTouching && (_pvr_grab_rigidbody.CurrentInteractableObject == null) && (_pvr_grab_rigidbody.TouchingCount == 0);
        }
    } 
}
