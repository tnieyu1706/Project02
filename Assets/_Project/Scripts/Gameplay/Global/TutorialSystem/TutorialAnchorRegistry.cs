using System.Collections.Generic;

namespace Game.Global.TutorialSystem
{
    /// <summary>
    /// Static registry that maps tutorial steps to their corresponding active anchors in the current scene.
    /// </summary>
    public static class TutorialAnchorRegistry
    {
        /// <summary>Internal mapping of step data to active scene anchors.</summary>
        private static Dictionary<TutorialStepData, TutorialAnchor> _anchors = new Dictionary<TutorialStepData, TutorialAnchor>();

        /// <summary>Registers an anchor for spatial lookup.</summary>
        public static void RegisterAnchor(TutorialAnchor anchor)
        {
            if (anchor.TargetStep == null) return;

            if (!_anchors.ContainsKey(anchor.TargetStep))
            {
                _anchors.Add(anchor.TargetStep, anchor);
            }
        }

        /// <summary>Unregisters an anchor when it is disabled or destroyed.</summary>
        public static void UnregisterAnchor(TutorialAnchor anchor)
        {
            if (anchor.TargetStep != null && _anchors.ContainsKey(anchor.TargetStep))
            {
                _anchors.Remove(anchor.TargetStep);
            }
        }

        /// <summary>Retrieves the active anchor associated with the specified tutorial step.</summary>
        public static TutorialAnchor GetAnchor(TutorialStepData step)
        {
            if (step != null && _anchors.TryGetValue(step, out TutorialAnchor anchor))
            {
                return anchor;
            }
            return null;
        }
    }
}