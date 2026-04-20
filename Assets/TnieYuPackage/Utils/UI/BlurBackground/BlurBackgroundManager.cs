using System;
using System.Collections.Generic;
using System.Linq;
using EditorAttributes;
using TnieYuPackage.DesignPatterns;
using UnityEngine;

namespace TnieYuPackage.Utils
{
    [Serializable]
    public class BlurBackgroundConfig
    {
        public string id;
        public int sortingOrder;
        public RenderMode renderMode;
    }

    [DefaultExecutionOrder(-10)]
    public class BlurBackgroundManager : SingletonBehavior<BlurBackgroundManager>
    {
        [SerializeField, Required] private GameObject blurBackgroundPrefab;

        [SerializeField] private List<BlurBackgroundConfig> configs;

        private Dictionary<string, BlurBackgroundComponent> blurBackgrounds;

        public Dictionary<string, BlurBackgroundComponent> BlurBackgrounds
        {
            get
            {
                blurBackgrounds ??= configs.ToDictionary(c => c.id, CreateBlurBackground);

                return blurBackgrounds;
            }
        }

        protected override void Awake()
        {
            dontDestroyOnLoad = false;
            base.Awake();
        }

        private BlurBackgroundComponent CreateBlurBackground(BlurBackgroundConfig config)
        {
            GameObject gObj = Instantiate(blurBackgroundPrefab);
            var component = gObj.GetComponentInChildren<BlurBackgroundComponent>();
            component.Setup(config, config.id);

            component.CloseManual();

            return component;
        }
    }
}