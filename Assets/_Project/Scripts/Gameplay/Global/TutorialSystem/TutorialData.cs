using System.Collections.Generic;
using UnityEngine;

namespace Game.Global.TutorialSystem
{
    /// <summary>
    /// ScriptableObject to hold a sequence of tutorial steps.
    /// </summary>
    [CreateAssetMenu(fileName = "NewTutorialData", menuName = "Tutorial System/Tutorial Data", order = 1)]
    public class TutorialData : ScriptableObject
    {
        public string TutorialId;
        
        [Tooltip("Tham chiếu trực tiếp đến Step đầu tiên.")]
        public TutorialStepData StartStep;

        [Tooltip("Danh sách các sub-asset step hiện có.")]
        public List<TutorialStepData> Steps = new List<TutorialStepData>();
    }
}