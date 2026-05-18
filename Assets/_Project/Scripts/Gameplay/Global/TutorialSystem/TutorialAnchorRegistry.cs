using System.Collections.Generic;

namespace Game.Global.TutorialSystem
{
    public static class TutorialAnchorRegistry
    {
        // Chuyển Key từ chuỗi string sang tham chiếu trực tiếp ScriptableObject
        private static Dictionary<TutorialStepData, TutorialAnchor> anchors = new Dictionary<TutorialStepData, TutorialAnchor>();

        public static void RegisterAnchor(TutorialAnchor anchor)
        {
            if (anchor.TargetStep == null) return;

            if (!anchors.ContainsKey(anchor.TargetStep))
            {
                anchors.Add(anchor.TargetStep, anchor);
            }
        }

        public static void UnregisterAnchor(TutorialAnchor anchor)
        {
            if (anchor.TargetStep != null && anchors.ContainsKey(anchor.TargetStep))
            {
                anchors.Remove(anchor.TargetStep);
            }
        }

        public static TutorialAnchor GetAnchor(TutorialStepData step)
        {
            if (step != null && anchors.TryGetValue(step, out TutorialAnchor anchor))
            {
                return anchor;
            }
            return null;
        }
    }
}