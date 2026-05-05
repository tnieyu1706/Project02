using System;
using System.Collections.Generic;
using EditorAttributes;
using TnieYuPackage.CustomAttributes;
using TnieYuPackage.DesignPatterns;
using UnityEngine;
using UnityEngine.U2D.Animation;

namespace Game.BaseGameplay
{
    public abstract class BaseObjectPresetSo : ScriptableObject
    {
        public string objectId;
        public SpriteLibraryAsset libraryAsset;

        [SerializeReference] [PropertyOrder(4)]
        public List<IBaseObjectInteractStrategyInstaller> interactStrategies = new();
        
        public List<BaseObjectConfigurator> configurators = new();
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