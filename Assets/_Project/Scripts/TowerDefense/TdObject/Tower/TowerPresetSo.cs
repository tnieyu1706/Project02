using System.Collections.Generic;
using TnieYuPackage.CustomAttributes;
using UnityEngine;

namespace Game.Td
{
    [CreateAssetMenu(fileName = "Tower", menuName = "Game/TD/Tower")]
    public class TowerPresetSo : ScriptableObject
    {
        public RuntimeAnimatorController animatorController;

        [SerializeReference]
        [AbstractSupport(
            classAssembly: typeof(ITdObjectBehaviourInstaller),
            abstractTypes: typeof(ITdObjectBehaviourInstaller)
        )]
        public ITdObjectBehaviourInstaller behaviourInstaller;
        
        [SerializeReference]
        [AbstractSupport(
            classAssembly: typeof(ITdObjectConfigurator),
            abstractTypes: typeof(ITdObjectConfigurator)
        )]
        public List<ITdObjectConfigurator> configurators = new();
    }
}