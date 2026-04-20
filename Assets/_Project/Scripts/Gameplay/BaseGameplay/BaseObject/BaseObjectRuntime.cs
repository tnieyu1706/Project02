using System;
using System.Collections.Generic;
using System.Linq;
using BackboneLogger;
using EditorAttributes;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.U2D.Animation;

namespace Game.BaseGameplay
{
    public interface IBaseObjectRuntime
    {
        // public BaseObjectPresetSo CurrentPreset { get; }
        Vector3 CurrentPosition { get; }
        List<IBaseObjectInteractStrategy> InteractStrategyList { get; }

        Action<IBaseObjectRuntime> OnTrackOn { get; set; }
        Action<IBaseObjectRuntime> OnTrackOff { get; set; }
        Action<IBaseObjectRuntime, IObjectInteractable> OnInteract { get; set; }
    }

    public abstract class BaseObjectRuntime<TPreset> : MonoBehaviour, IBaseObjectRuntime
        where TPreset : BaseObjectPresetSo
    {
        [SerializeField, ReadOnly] public TPreset currentPreset;
        [SerializeField, Self] protected Animator animator;
        [SerializeField, Child] protected SpriteLibrary spriteLibrary;

        /// <summary>
        /// Only use for add/remove strategy.
        /// </summary>
        public readonly Dictionary<Guid, IBaseObjectInteractStrategy> InteractStrategies = new();

        public BaseObjectPresetSo CurrentPreset => currentPreset;
        public Vector3 CurrentPosition => transform.position;
        public List<IBaseObjectInteractStrategy> InteractStrategyList => InteractStrategies.Values.ToList();

        public Action<IBaseObjectRuntime> OnTrackOn { get; set; }
        public Action<IBaseObjectRuntime> OnTrackOff { get; set; }
        public Action<IBaseObjectRuntime, IObjectInteractable> OnInteract { get; set; }

        protected virtual void OnEnable()
        {
            BaseObjectInteractSystem.Instance.ObjectRuntimes.Add(this);
        }

        protected virtual void OnDisable()
        {
            if (BaseObjectInteractSystem.Instance != null)
                BaseObjectInteractSystem.Instance.ObjectRuntimes.Remove(this);
        }

        protected virtual void SetPreset(TPreset preset)
        {
            if (currentPreset != null)
            {
                foreach (var interactInstaller in currentPreset.interactStrategies)
                {
                    interactInstaller.DestroyInteract(this);
                }

                if (InteractStrategies.Count != 0)
                {
                    BLogger.Log($"[BaseObjectRuntime: {name}] not yet unInstall all interacts previous",
                        LogLevel.Critical, "Base");

                    //clear
                    // interactStrategies.Clear();
                }

                foreach (var configurator in currentPreset.configurators)
                {
                    configurator.UnConfigure(this);
                }
            }

            currentPreset = preset;

            SetPresetProperties(currentPreset);

            foreach (var interactStrategy in currentPreset.interactStrategies)
            {
                interactStrategy.InitInteract(this);
            }

            foreach (var configurator in currentPreset.configurators)
            {
                configurator.Configure(this);
            }
        }

        protected virtual void SetPresetProperties(TPreset preset)
        {
            spriteLibrary.spriteLibraryAsset = preset.libraryAsset;
        }
    }
}