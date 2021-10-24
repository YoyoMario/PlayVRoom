using UnityEngine;
using DG.Tweening;
using System;

namespace DivIt.PlayVRoom.Misc
{
    public class AppearingTarget : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform _targetRotatorTransform = null;
        [SerializeField] private AppearingTargetHitPoint[] _hitPoints = null;

        [Header("Options:")]
        [SerializeField] private float _appearingAnimationDuration = 0.75f;
        [SerializeField] private Ease _appearingAnimationType = Ease.InOutQuad;
        [SerializeField] private Vector3 _fullyVisibleLocalPosition = Vector3.zero;
        [SerializeField] private Quaternion _fullyVisibleLocalRotation = Quaternion.identity;
        [Space]
        [SerializeField] private float _disappearingAnimationDuration = 0.3f;
        [SerializeField] private Ease _disappearingAnimationType = Ease.InOutQuad;
        [SerializeField] private Vector3 _fullyHiddenLocalPosition = Vector3.zero;
        [SerializeField] private Quaternion _fullyHiddenLocalRotation = Quaternion.identity;

        public Action<AppearingTarget, Vector3, float> OnTargetHit;

        private bool _isVisible = true;
        private Tweener _positionTweener = null;
        private Tweener _rotationTweener = null;

        private void OnDisable()
        {
            StopAnimation();
        }

        public void Show()
        {
            if (_isVisible)
            {
                return;
            }

            _isVisible = true;
            AnimateTarget(_fullyVisibleLocalPosition, _fullyVisibleLocalRotation, _appearingAnimationDuration, _appearingAnimationType);
        }

        public void Hide(bool hideInstantly)
        {
            if (!_isVisible)
            {
                return;
            }

            _isVisible = false;
            AnimateTarget(_fullyHiddenLocalPosition, _fullyHiddenLocalRotation, hideInstantly ? 0f : _disappearingAnimationDuration, _disappearingAnimationType);
        }

        public void ApplyBulletDamage(Vector3 hitPosition)
        {
            float maxHitPoints = 0f;
            foreach(AppearingTargetHitPoint hitPoint in _hitPoints)
            {
                float hitPoints = hitPoint.CalculateHitPoints(hitPosition);
                if(hitPoints > maxHitPoints)
                {
                    maxHitPoints = hitPoints;
                }
            }

            OnTargetHit?.Invoke(this, hitPosition, maxHitPoints);
        }

        private void StopAnimation()
        {
            _positionTweener?.Kill();
            _rotationTweener?.Kill();
        }

        private void AnimateTarget(Vector3 endLocalPosition, Quaternion endLocalRotation, float animationDuration, Ease animationType)
        {
            StopAnimation();

            _positionTweener = _targetRotatorTransform.DOLocalMove(endLocalPosition, animationDuration).SetEase(animationType);
            _rotationTweener = _targetRotatorTransform.DOLocalRotateQuaternion(endLocalRotation, animationDuration).SetEase(animationType);
        }
    }
}