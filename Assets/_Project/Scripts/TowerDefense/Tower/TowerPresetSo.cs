using TnieYuPackage.CustomAttributes.Runtime;
using UnityEngine;

namespace _Project.Scripts.TowerDefense.Tower
{
    [CreateAssetMenu(fileName = "Tower", menuName = "Game/TD/Tower")]
    public class TowerPresetSo : ScriptableObject
    {
        public RuntimeAnimatorController animatorController;
        
        [SerializeReference, AbstractSupport(typeof(ITowerBehaviourInstaller))]
        public ITowerBehaviourInstaller behaviourInstaller;
    }
}