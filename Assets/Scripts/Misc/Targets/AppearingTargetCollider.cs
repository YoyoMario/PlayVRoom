using UnityEngine;

namespace DivIt.PlayVRoom.Misc
{
    [RequireComponent(typeof(Collider))]
    public class AppearingTargetCollider : MonoBehaviour
    {
        [SerializeField] private AppearingTarget _appearingTarget = null;

        public AppearingTarget AppearingTarget { get { return _appearingTarget; } }

        private void OnValidate()
        {
            if (!_appearingTarget)
            {
                _appearingTarget = GetComponentInParent<AppearingTarget>();
            }
        }
    }
}