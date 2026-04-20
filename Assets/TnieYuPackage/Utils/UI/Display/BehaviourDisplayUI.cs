using UnityEngine;

namespace TnieYuPackage.Utils
{
    public abstract class BehaviourDisplayUI : MonoBehaviour, IDisplayGUI
    {
        [SerializeField] private string blurBackgroundId;

        private BlurBackgroundComponent blurBackground;

        protected BlurBackgroundComponent BlurBackground
        {
            get
            {
                if (blurBackground != null) return blurBackground;

                if (!BlurBackgroundManager.Instance.BlurBackgrounds.TryGetValue(this.blurBackgroundId,
                        out blurBackground))
                {
                    Debug.LogError($"[BlurBackgroundManager] does not contain {blurBackgroundId}");
                }

                return blurBackground;
            }
        }

        protected virtual void Awake()
        {
            BlurBackground.RegistryRelated(this);
        }

        protected virtual void OnDestroy()
        {
            if (BlurBackgroundManager.Instance != null)
            {
                BlurBackground.UnRegistryRelated(this);
            }
        }

        public abstract void Hide();
    }
}