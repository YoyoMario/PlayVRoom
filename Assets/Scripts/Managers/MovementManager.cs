using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using DivIt.Utils;

namespace DivIt.PlayVRoom.Managers
{
    public class MovementManager : Singleton<MovementManager>
    {
        [SerializeField] private SteamVR_Input_Sources _sourceLeftHand = SteamVR_Input_Sources.Any;
        [SerializeField] private SteamVR_Input_Sources _sourceRightHand = SteamVR_Input_Sources.Any;
        [Space(5)]
        [SerializeField] private SteamVR_Action_Vector2 _trackpadPosition = null;
        [SerializeField] private SteamVR_Action_Boolean _trackpadClick = null;
        [Space(20)]
        [SerializeField] private Transform _playerArea = null;
        [SerializeField] private Transform _playerHead = null;
        [SerializeField] private SphereCollider _playareaCollider = null;
        [Space(20)]
        [SerializeField] private AnimationCurve _sensitvityCurve = null;
        [SerializeField] private float _walkAcceleration = 0.25f;
        [SerializeField] private float _maxSpeed = 0.25f;

        private Rigidbody _rigidbodyPlayarea;
        private Vector3 _hitPoint;
        private Vector2 _trackpadPositionLeft;
        private Vector2 _trackpadPositionRight;
        private bool _trackpadClickedLeft;
        private bool _trackpadClickedRight;

        public Rigidbody RigidbodyPlayarea
        {
            get
            {
                return _rigidbodyPlayarea;
            }
        }

        private void Awake()
        {
            _rigidbodyPlayarea = _playerArea.GetComponent<Rigidbody>();
        }

        private void Update()
        {
            _trackpadPositionLeft = _trackpadPosition[_sourceLeftHand].axis;
            _trackpadPositionRight = _trackpadPosition[_sourceRightHand].axis;
        }

        private void OnEnable()
        {
            //On Click trackpad
            _trackpadClick.AddOnStateDownListener(OnTrackpadClick, _sourceLeftHand);
            _trackpadClick.AddOnStateDownListener(OnTrackpadClick, _sourceRightHand);
            //On release trackpad
            _trackpadClick.AddOnStateUpListener(OnTrackpadRelease, _sourceLeftHand);
            _trackpadClick.AddOnStateUpListener(OnTrackpadRelease, _sourceRightHand);
        }

        private void OnDisable()
        {
            //On Click trackpad
            _trackpadClick.RemoveOnStateDownListener(OnTrackpadClick, _sourceLeftHand);
            _trackpadClick.RemoveOnStateDownListener(OnTrackpadClick, _sourceRightHand);
            //On release trackpad
            _trackpadClick.RemoveOnStateUpListener(OnTrackpadRelease, _sourceLeftHand);
            _trackpadClick.RemoveOnStateUpListener(OnTrackpadRelease, _sourceRightHand);
        }

        private void FixedUpdate()
        {
            _playareaCollider.center = new Vector3(
                _playerHead.localPosition.x,
                0.2f,
                _playerHead.localPosition.z
                );

            if (_trackpadClickedLeft)
            {
                Vector3 velocityToAdd =
                    _playerHead.right * _sensitvityCurve.Evaluate(_trackpadPositionLeft.x) * _walkAcceleration +
                    _playerHead.forward * _sensitvityCurve.Evaluate(_trackpadPositionLeft.y) * _walkAcceleration;
                velocityToAdd.y = _rigidbodyPlayarea.velocity.y;
                _rigidbodyPlayarea.velocity = velocityToAdd;
            }
            if (_trackpadClickedRight)
            {
                Vector3 velocityToAdd =
                    _playerHead.right * _sensitvityCurve.Evaluate(_trackpadPositionRight.x) * _walkAcceleration +
                    _playerHead.forward * _sensitvityCurve.Evaluate(_trackpadPositionRight.y) * _walkAcceleration;
                velocityToAdd.y = _rigidbodyPlayarea.velocity.y;
                _rigidbodyPlayarea.velocity = velocityToAdd;
            }

            _rigidbodyPlayarea.velocity = Vector3.ClampMagnitude(_rigidbodyPlayarea.velocity, _maxSpeed);
            
            //Ray headDown = new Ray(_playerHead.transform.position, Vector3.down);
            //RaycastHit hit;
            //bool hitGround = Physics.Raycast(headDown, out hit, 2.5f);
            //if (hitGround)
            //{
            //    _hitPoint = hit.point;
            //}

            //_rigidbody.position = new Vector3(
            //    _rigidbody.position.x,
            //    _hitPoint.y,
            //    _rigidbody.position.z
            //    );
            //_rigidbody.velocity = Vector3.zero;
        }

        private void OnTrackpadClick(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
        {
            if(fromSource == _sourceLeftHand)
            {
                _trackpadClickedLeft = true;
            }
            else if(fromSource == _sourceRightHand)
            {
                _trackpadClickedRight = true;
            }
        }
        private void OnTrackpadRelease(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
        {
            if (fromSource == _sourceLeftHand)
            {
                _trackpadClickedLeft = false;
            }
            else if (fromSource == _sourceRightHand)
            {
                _trackpadClickedRight = false;
            }
        }
    } 
}
