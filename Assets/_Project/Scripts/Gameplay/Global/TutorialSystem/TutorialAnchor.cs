using UnityEngine;

namespace Game.Global.TutorialSystem
{
    /// <summary>
    /// Identifies a spatial point or UI element in the scene used as a hint anchor for tutorial steps.
    /// </summary>
    [DisallowMultipleComponent]
    public class TutorialAnchor : MonoBehaviour
    {
        /// <summary>The tutorial step associated with this anchor.</summary>
        [Tooltip("The tutorial step that this anchor points to.")]
        public TutorialStepData TargetStep;

        private void OnEnable()
        {
            TutorialAnchorRegistry.RegisterAnchor(this);
        }

        private void OnDisable()
        {
            TutorialAnchorRegistry.UnregisterAnchor(this);
        }

        /// <summary>
        /// Triggers the next step in the tutorial sequence if the current step matches this anchor's target.
        /// </summary>
        public void TriggerNextStep()
        {
            if (TargetStep != null)
            {
                TutorialManager.Instance.Next(TargetStep);
            }
        }
    }
}