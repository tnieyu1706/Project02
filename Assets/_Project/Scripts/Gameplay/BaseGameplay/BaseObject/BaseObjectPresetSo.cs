using System;
using System.Collections.Generic;
using EditorAttributes;
using TnieYuPackage.CustomAttributes;
using TnieYuPackage.DesignPatterns;
using UnityEngine;
using UnityEngine.U2D.Animation;

namespace Game.BaseGameplay
{
    public abstract class BaseObjectPresetSo : BaseParentAsset<IBaseObjectInteractStrategyInstaller>
    {
        public string objectId;
        public SpriteLibraryAsset libraryAsset;

        [SerializeReference] [PropertyOrder(4)]
        public List<IBaseObjectInteractStrategyInstaller> interactStrategies = new();

        [SerializeField]
        [PropertyOrder(4)]
        [SubTypeNameSelected(typeof(IBaseObjectInteractStrategyInstaller),
            typeof(IBaseObjectInteractStrategyInstaller))]
        private string strategyType;
        
        public List<BaseObjectConfigurator> configurators = new();

        protected override List<IBaseObjectInteractStrategyInstaller> SubAssets => interactStrategies;

        protected override IBaseObjectInteractStrategyInstaller CreateSubAsset()
        {
            Type type = Type.GetType(strategyType);
            if (type == null)
            {
                Debug.LogError($"Type {strategyType} not found.");
                return null;
            }

            if (!typeof(IBaseObjectInteractStrategyInstaller).IsAssignableFrom(type))
            {
                Debug.LogError($"Type {strategyType} does not implement IBaseObjectInteractStrategyInstaller.");
                return null;
            }

            return ScriptableObject.CreateInstance(type) as IBaseObjectInteractStrategyInstaller;
        }
    }

    public interface IObjectInteractable : IHealthProperty
    {
        Vector3 CurrentPosition { get; }
    }

    public interface IBaseObjectInteractStrategy
    {
        IBaseObjectInteractStrategyInstaller Installer { get; }

        bool CanUse { get; }
        bool TrackTarget(Vector3 position, out IObjectInteractable target); //can apply IHealthProperty directly
        void Interact(IObjectInteractable interactable); //can apply IHealthProperty directly

        public void OnInitBehaviour(IBaseObjectRuntime runtime);
        public void OnDestroyBehaviour();
    }

    public interface IBaseObjectInteractStrategy<out TInstaller> : IBaseObjectInteractStrategy
        where TInstaller : IBaseObjectInteractStrategyInstaller
    {
        IBaseObjectInteractStrategyInstaller IBaseObjectInteractStrategy.Installer => ActualInstaller;
        TInstaller ActualInstaller { get; }
    }
}