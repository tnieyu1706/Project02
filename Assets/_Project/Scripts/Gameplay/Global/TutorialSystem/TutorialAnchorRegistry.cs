using System.Collections.Generic;

namespace Game.Global.TutorialSystem
{
    /// <summary>
    /// Bộ máy tĩnh (Static) quản lý tất cả các Anchor đang Active trên màn hình.
    /// </summary>
    public static class TutorialAnchorRegistry
    {
        private static Dictionary<string, TutorialAnchor> anchors = new Dictionary<string, TutorialAnchor>();

        public static void RegisterAnchor(TutorialAnchor anchor)
        {
            if (string.IsNullOrEmpty(anchor.AnchorId)) return;

            if (!anchors.ContainsKey(anchor.AnchorId))
            {
                anchors.Add(anchor.AnchorId, anchor);
            }
        }

        public static void UnregisterAnchor(TutorialAnchor anchor)
        {
            if (!string.IsNullOrEmpty(anchor.AnchorId) && anchors.ContainsKey(anchor.AnchorId))
            {
                anchors.Remove(anchor.AnchorId);
            }
        }

        public static TutorialAnchor GetAnchor(string id)
        {
            if (anchors.TryGetValue(id, out TutorialAnchor anchor))
            {
                return anchor;
            }
            return null;
        }
    }
}