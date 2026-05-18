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
    public class TutorialStepData : ScriptableObject
    {
        [TextArea(3, 5)] public string Message;

        public TutorialDisplayType DisplayType;

        [Tooltip("Tham chiếu trực tiếp đến Step tiếp theo.")]
        public TutorialStepData NextStep;

        [HideInInspector] public Vector2 NodePosition; // Dùng cho GraphView lưu tọa độ UI
    }
}