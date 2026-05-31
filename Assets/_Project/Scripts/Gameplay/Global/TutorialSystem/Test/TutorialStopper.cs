using UnityEngine;

namespace Game.Global.TutorialSystem.Test
{
    public class TutorialStopper : MonoBehaviour
    {
        public void StopTutorialImmediately()
        {
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.StopTutorial();
            }
        }

        public void StopTutorialWithSave()
        {
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.EndTutorial();
            }
        }
    }
}