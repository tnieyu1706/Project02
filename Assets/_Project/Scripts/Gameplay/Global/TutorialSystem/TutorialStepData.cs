using UnityEngine;

namespace Game.Global.TutorialSystem
{
    /// <summary>
    /// Defines the visual presentation modes for a tutorial step.
    /// </summary>
    public enum TutorialDisplayType
    {
        /// <summary>Only displays a text message to the user.</summary>
        Text,

        /// <summary>Displays a visual hint (e.g., arrow, highlight) at an anchor position.</summary>
        Hint,

        /// <summary>Displays both a text message and a visual spatial hint.</summary>
        TextHint
    }

    /// <summary>
    /// Represents a single step in a tutorial sequence, containing its message and display configuration.
    /// </summary>
    public class TutorialStepData : ScriptableObject
    {
        /// <summary>The instruction message displayed to the player.</summary>
        [TextArea(3, 5)] public string Message;

        /// <summary>The display mode used to render this step.</summary>
        public TutorialDisplayType DisplayType;

        /// <summary>The visual position of the node within the editor graph window.</summary>
        [HideInInspector] public Vector2 NodePosition;
    }
}