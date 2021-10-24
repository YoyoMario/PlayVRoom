using System;
using TMPro;
using UnityEngine;
using Valve.VR.InteractionSystem;

namespace DivIt.PlayVRoom.Misc
{
    public class AppearingTargetsScoreController : MonoBehaviour
    {
        private const string TEXT_FORMAT = "#.##";
        private const float SMALL_OFFSET_TOWARDS_PLAYER = 0.1f;

        [Header("References: ")]
        [SerializeField] private AppearingTargetsSequenceController _appearingTargetsSequenceController = null;
        [SerializeField] private GameObject _hitPointsTextPrefab = null;

        public Action<float> OnScoreChanged;

        private float _totalScore = 0f;

        private void OnEnable()
        {
            _appearingTargetsSequenceController.OnTargetSequenceStarted += OnTargetSequenceStarted;
            _appearingTargetsSequenceController.OnHitPointsAccepted += OnHitPointsAccepted;
            _appearingTargetsSequenceController.OnTargetSequenceFinished += OnTargetSequenceFinished;
        }

        private void OnDisable()
        {
            _appearingTargetsSequenceController.OnTargetSequenceStarted -= OnTargetSequenceStarted;
            _appearingTargetsSequenceController.OnHitPointsAccepted -= OnHitPointsAccepted;
            _appearingTargetsSequenceController.OnTargetSequenceFinished -= OnTargetSequenceFinished;
        }

        private void OnTargetSequenceStarted()
        {
            _totalScore = 0f;
        }

        private void OnTargetSequenceFinished()
        {

        }

        private void OnHitPointsAccepted(Vector3 hitPosition, float amountOfPoints)
        {
            _totalScore += amountOfPoints;

            Vector3 playerPosition = Player.instance.hmdTransform.position;
            Vector3 directionTowardsPlayer = (playerPosition - hitPosition).normalized;
            GameObject hitPointsTextGameObject = Instantiate(_hitPointsTextPrefab, transform);
            Transform hitPointsTextTransform = hitPointsTextGameObject.transform;
            hitPointsTextTransform.position = hitPosition + directionTowardsPlayer * SMALL_OFFSET_TOWARDS_PLAYER;
            hitPointsTextTransform.LookAt(playerPosition);

            TextMeshProUGUI text = hitPointsTextGameObject.GetComponentInChildren<TextMeshProUGUI>();
            text.text = amountOfPoints.ToString(TEXT_FORMAT);

            OnScoreChanged?.Invoke(_totalScore);
        }
    }
}