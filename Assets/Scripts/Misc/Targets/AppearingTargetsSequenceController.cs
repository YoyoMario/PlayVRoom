using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

namespace DivIt.PlayVRoom.Misc
{
    public class AppearingTargetsSequenceController : MonoBehaviour
    {
        [Header("References: ")]
        [SerializeField] private List<AppearingTarget> _targets = new List<AppearingTarget>();

        [Header("Options: ")]
        [SerializeField] private float _sequenceDuration = 60f;
        [SerializeField] private float _delayBetweenTargetsShow = 3f;
        [SerializeField] private float _targetMaxVisibleDuration = 6f;

        public Action<Vector3, float> OnHitPointsAccepted;
        public Action OnTargetSequenceStarted;
        public Action OnTargetSequenceFinished;
        public Action<float> OnRemainingTimeUpdated;

        private bool _sequenceActive = false;
        private Dictionary<AppearingTarget, float> _durationOfActiveTargetsDictionary = new Dictionary<AppearingTarget, float>();
        private float _nextTargetSpawningTimer = 0f;
        private float _totalSequenceDurationTimer = 0f;

        private void Start()
        {
            HideAllTargets(true);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                StartSequence();
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                StopAndResetSequence();
            }

            if (!_sequenceActive)
            {
                return;
            }

            float elapsedTime = Time.deltaTime;
            IncreaseTimers(elapsedTime);
            UpdateTargetsVisibleDuration(elapsedTime);
            TryShowNextTarget();
            TryCompleteSequence();
        }

        public void StartSequence()
        {
            if (_sequenceActive)
            {
                return;
            }

            SubscribeToTargetsEvents();
            SetupTimersForSequenceStart();
            _sequenceActive = true;
            OnTargetSequenceStarted?.Invoke();
        }

        public void StopAndResetSequence()
        {
            if (!_sequenceActive)
            {
                return;
            }

            UnsubscribeFromTargetsEvents();
            ResetTimers();
            HideAllTargets(false);
            _sequenceActive = false;
            OnTargetSequenceStarted?.Invoke();
        }

        private void TryCompleteSequence()
        {
            if(_totalSequenceDurationTimer >= _sequenceDuration)
            {
                StopAndResetSequence();
            }
        }

        private void TryShowNextTarget()
        {
            if (_nextTargetSpawningTimer >= _delayBetweenTargetsShow)
            {
                _nextTargetSpawningTimer = 0f;
                ShowNextTarget();
            }
        }

        private void ShowNextTarget()
        {
            int numberOfActiveTargets = _durationOfActiveTargetsDictionary.Count;
            int totalNumberOfTargets = _targets.Count;

            if(numberOfActiveTargets >= totalNumberOfTargets)
            {
                Debug.LogError("Can't show new target, all are already active!");
                return;
            }

            int newTargetIndex = UnityEngine.Random.Range(0, totalNumberOfTargets);
            AppearingTarget newTargetForActivation = _targets[newTargetIndex];
            _durationOfActiveTargetsDictionary.Add(newTargetForActivation, 0f);
            _targets.RemoveAt(newTargetIndex);
            newTargetForActivation.Show();
        }

        private void HideAllTargets(bool hideInstantly)
        {
            foreach (AppearingTarget activeTarget in _targets)
            {
                activeTarget.Hide(hideInstantly);
            }
            foreach (AppearingTarget activeTarget in _durationOfActiveTargetsDictionary.Keys)
            {
                activeTarget.Hide(hideInstantly);
                if (!_targets.Contains(activeTarget))
                {
                    _targets.Add(activeTarget);
                }
            }
            _durationOfActiveTargetsDictionary.Clear();
        }

        private void UpdateTargetsVisibleDuration(float elapsedTime)
        {
            List<AppearingTarget> targetsToHide = new List<AppearingTarget>();

            // Increase active targets visible duration, collect those who are visible more than max visible time
            List<AppearingTarget> visibleTargets = _durationOfActiveTargetsDictionary.Keys.ToList();
            for(int i = 0; i < visibleTargets.Count; i++)
            {
                AppearingTarget visibleTarget = visibleTargets[i];
                float targetVisibleDuration = _durationOfActiveTargetsDictionary[visibleTarget];
                targetVisibleDuration += elapsedTime;
                _durationOfActiveTargetsDictionary[visibleTarget] = targetVisibleDuration;
                if (targetVisibleDuration >= _targetMaxVisibleDuration)
                {
                    targetsToHide.Add(visibleTarget);
                }
            }

            // Hide all targets that are visible longer than max visible time
            foreach(AppearingTarget targetToHide in targetsToHide)
            {
                targetToHide.Hide(false);
                if (!_targets.Contains(targetToHide))
                {
                    _targets.Add(targetToHide);
                }
                _durationOfActiveTargetsDictionary.Remove(targetToHide);
            }
        }

        private void OnTargetHit(AppearingTarget hitTarget, Vector3 hitPosition, float targetHitPoints)
        {
            // If target isn't in targets, it must be in active ones
            if (!_targets.Contains(hitTarget))
            {
                hitTarget.Hide(false);
                _targets.Add(hitTarget);
                float targetVisibleTime = _durationOfActiveTargetsDictionary[hitTarget];
                _durationOfActiveTargetsDictionary.Remove(hitTarget);
                _nextTargetSpawningTimer = _delayBetweenTargetsShow;

                float targetPointsMultiplier = 1.0f - Mathf.InverseLerp(0f, _targetMaxVisibleDuration, targetVisibleTime);
                OnHitPointsAccepted?.Invoke(hitPosition, targetHitPoints * targetPointsMultiplier);
            }
        }

        private void IncreaseTimers(float elapsedTime)
        {
            _nextTargetSpawningTimer += elapsedTime;
            _totalSequenceDurationTimer += elapsedTime;

            float remainingTime = Mathf.Max(0f, (_sequenceDuration - _totalSequenceDurationTimer));
            OnRemainingTimeUpdated?.Invoke(remainingTime);
        }

        private void SetupTimersForSequenceStart()
        {
            _nextTargetSpawningTimer = _delayBetweenTargetsShow;
            _totalSequenceDurationTimer = 0f;
        }

        private void ResetTimers()
        {
            _nextTargetSpawningTimer = 0f;
            _totalSequenceDurationTimer = 0f;
        }

        private void SubscribeToTargetsEvents()
        {
            foreach(AppearingTarget appearingTarget in _targets)
            {
                appearingTarget.OnTargetHit += OnTargetHit;
            }
        }

        private void UnsubscribeFromTargetsEvents()
        {
            foreach (AppearingTarget appearingTarget in _targets)
            {
                appearingTarget.OnTargetHit -= OnTargetHit;
            }
        }
    }
}