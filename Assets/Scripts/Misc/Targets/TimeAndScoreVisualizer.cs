using TMPro;
using UnityEngine;

namespace DivIt.PlayVRoom.Misc
{
    public class TimeAndScoreVisualizer : MonoBehaviour
    {
        private const string TIME_SUFFIX = "s";
        private const string SCORE_TEXT_FORMAT = "##.#";
        private const string TIME_TEXT_FORMAT = "##.##";

        [SerializeField] private TextMeshProUGUI _scoreText = null;
        [SerializeField] private TextMeshProUGUI _timeText = null;
        [SerializeField] private AppearingTargetsSequenceController _sequenceController = null;
        [SerializeField] private AppearingTargetsScoreController _scoreController = null;

        private void OnEnable()
        {
            _sequenceController.OnRemainingTimeUpdated += UpdateTimeVisual;
            _scoreController.OnScoreChanged += UpdateScoreVisual;

            _scoreText.text = string.Empty;
            _timeText.text = string.Empty;
        }

        private void OnDisable()
        {
            _sequenceController.OnRemainingTimeUpdated -= UpdateTimeVisual;
            _scoreController.OnScoreChanged -= UpdateScoreVisual;
        }

        private void UpdateTimeVisual(float remainingTime)
        {
            _timeText.text = remainingTime.ToString(TIME_TEXT_FORMAT);
        }

        private void UpdateScoreVisual(float totalScore)
        {
            _scoreText.text = totalScore.ToString(SCORE_TEXT_FORMAT);
        }
    }
}