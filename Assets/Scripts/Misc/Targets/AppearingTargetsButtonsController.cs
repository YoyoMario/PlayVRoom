using DivIt.PlayVRoom.VR.Interaction;
using UnityEngine;

namespace DivIt.PlayVRoom.Misc
{
    public class AppearingTargetsButtonsController : MonoBehaviour
    {
        [SerializeField] private PVR_Button _startButton = null;
        [SerializeField] private PVR_Button _endButton = null;
        [SerializeField] private AppearingTargetsSequenceController _sequenceController = null;

        private void OnEnable()
        {
            _startButton.OnButtonPress += OnStartButtonClicked;
            _endButton.OnButtonPress += OnEndButtonClicked;
        }

        private void OnDisable()
        {
            _startButton.OnButtonPress -= OnStartButtonClicked;
            _endButton.OnButtonPress -= OnEndButtonClicked;
        }

        private void OnStartButtonClicked()
        {
            _sequenceController.StartSequence();
        }

        private void OnEndButtonClicked()
        {
            _sequenceController.StopAndResetSequence();
        }
    }
}