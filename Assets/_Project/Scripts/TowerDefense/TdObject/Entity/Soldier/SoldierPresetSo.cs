using System.Collections.Generic;
using TnieYuPackage.CustomAttributes;
using UnityEngine;

namespace Game.Td
{
    [CreateAssetMenu(fileName = "Soldier", menuName = "Game/TD/Entity/Soldier/Preset")]
    public class SoldierPresetSo : ScriptableObject
    {
        public float maxHp;
        public float def;
        public RuntimeAnimatorController animatorController;
        
        [SerializeReference]
        [AbstractSupport(
            classAssembly: typeof(ITdObjectBehaviourInstaller),
            excludedTypes: new[] { typeof(ITowerBehaviourInstaller) },
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