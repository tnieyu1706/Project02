using System.Collections.Generic;
using EditorAttributes;
using TnieYuPackage.CustomAttributes;
using UnityEngine;
using UnityEngine.U2D.Animation;

namespace Game.Td
{
    public abstract class TdObjectPresetSo : ScriptableObject
    {
        [PropertyOrder(1)]
        public SpriteLibraryAsset libraryAsset;
        
        [SerializeReference]
        [PropertyOrder(1)]
        [AbstractSupport(
            classAssembly: typeof(ITdObjectBehaviourInstaller),
            excludedTypes: new[] { typeof(ITowerBehaviourInstaller) },
            abstractTypes: typeof(ITdObjectBehaviourInstaller)
        )]
        public ITdObjectBehaviourInstaller behaviourInstaller;

        [SerializeReference]
        [PropertyOrder(1)]
        [AbstractSupport(
            classAssembly: typeof(ITdObjectConfigurator),
            abstractTypes: typeof(ITdObjectConfigurator)
        )]
        public List<ITdObjectConfigurator> configurators = new();
    }
}