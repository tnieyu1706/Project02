using System.Collections.Generic;
using UnityEngine;

namespace Game.Global.TutorialSystem
{
    /// <summary>
    /// ScriptableObject defining a full tutorial sequence as a collection of steps.
    /// </summary>
    [CreateAssetMenu(fileName = "NewTutorialData", menuName = "Tutorial System/Tutorial Data", order = 1)]
    public class TutorialData : ScriptableObject
    {
        /// <summary>Unique identifier for the tutorial, used for persistence and lookup.</summary>
        public string TutorialId;

        /// <summary>The ordered list of steps that make up this tutorial sequence.</summary>
        [Tooltip("The ordered list of sub-asset steps.")]
        public List<TutorialStepData> Steps = new List<TutorialStepData>();
    }
}