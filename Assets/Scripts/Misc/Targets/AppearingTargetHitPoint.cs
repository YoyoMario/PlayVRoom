using UnityEngine;

namespace DivIt.PlayVRoom.Misc
{
    public class AppearingTargetHitPoint : MonoBehaviour
    {
        private const float MAX_HIT_SCORE = 10f;
        private const float MIN_TARGET_SCORE = 5f;

        [Header("References: ")]
        [SerializeField] private Transform _hitPointCenterTransform = null;
        [SerializeField] private Transform _hitPointOuterEdgeTransform = null;

        public float CalculateHitPoints(Vector3 hitPosition)
        {
            float maxDistanceAvailableFromCenter = (_hitPointOuterEdgeTransform.position - _hitPointCenterTransform.position).magnitude;
            float distanceFromCenter = (_hitPointCenterTransform.position - hitPosition).magnitude;
            float hitScorePercentage = 1.0f - Mathf.InverseLerp(0f, maxDistanceAvailableFromCenter, distanceFromCenter);
            float hitScore = hitScorePercentage * MAX_HIT_SCORE + MIN_TARGET_SCORE;
            return hitScore;
        }
    }
}
