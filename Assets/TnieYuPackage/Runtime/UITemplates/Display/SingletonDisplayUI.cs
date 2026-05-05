using TnieYuPackage.DesignPatterns;
using UnityEngine;

namespace TnieYuPackage.Utils
{
    public abstract class SingletonDisplayUI<TSingleton> : Singleton<TSingleton>, IDisplayGUI
        where TSingleton : SingletonDisplayUI<TSingleton>
    {
        [SerializeField] private string blurBackgroundId;

        private BlurBackgroundComponent blurBackground;

        public BlurBackgroundComponent BlurBackground
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

        protected override void Awake()
        {
            base.Awake();

            BlurBackground.RegistryRelated(this);
        }

        protected virtual void OnDestroy()
        {
            if (BlurBackgroundManager.HasInstance)
            {
                BlurBackground.UnRegistryRelated(this);
            }
        }

        public abstract void Hide();
    }
}