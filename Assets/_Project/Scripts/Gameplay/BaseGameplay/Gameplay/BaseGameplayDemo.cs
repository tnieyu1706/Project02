using EditorAttributes;
using UnityEngine;

namespace Game.BaseGameplay
{
    public class BaseGameplayDemo : MonoBehaviour
    {
        [Button]
        private void DestroyBase()
        {
            if (!BaseGameplayController.HasInstance) return;
            
            BaseGameplayController.Instance.DestroyBase();
        }

        [Button]
        private void CloseWaves()
        {
            if (!BaseGameplayController.HasInstance) return;

            BaseGameplayController.Instance.CloseWaves();
        }
    }
}