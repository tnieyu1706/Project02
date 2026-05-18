using UnityEngine;

namespace Game.Global.TutorialSystem.Test
{
    public class TutorialStarter : MonoBehaviour
    {
        [SerializeField] private TutorialData _tutorialData;

        void Start()
        {
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.StartTutorial(_tutorialData);
            }
        }
    }
}