using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Global.TutorialSystem
{
    /// <summary>
    /// Enum defining the types of tutorial displays available.
    /// </summary>
    public enum TutorialDisplayType
    {
        Text, // Just show text instructions
        Hint, // Show a visual hint (like an arrow or highlight) pointing to an anchor
        TextHint // Combine both Text and Hint
    }

    /// <summary>
    /// Data structure representing a single step in the tutorial.
    /// </summary>
    [Serializable]
    public class TutorialStepData
    {
        public string Id;

        [TextArea(3, 5)] public string Message;

        public TutorialDisplayType DisplayType;

        [Tooltip("ID của TutorialAnchor mà vòng sáng sẽ bay tới.")]
        public string AnchorId;

        [Tooltip("Trạng thái chờ kích hoạt tiếp theo (VD: Click_Play). Bỏ trống nếu chỉ cần gọi Next().")]
        public string ExpectedNextId;
    }

    /// <summary>
    /// ScriptableObject to hold a sequence of tutorial steps.
    /// </summary>
    [CreateAssetMenu(fileName = "NewTutorialData", menuName = "Tutorial System/Tutorial Data")]
    public class TutorialData : ScriptableObject
    {
        [Tooltip("Unique identifier for this tutorial sequence.")]
        public string TutorialId;

        [Tooltip("The ordered list of steps in this tutorial sequence.")]
        public List<TutorialStepData> Steps = new List<TutorialStepData>();
    }
}