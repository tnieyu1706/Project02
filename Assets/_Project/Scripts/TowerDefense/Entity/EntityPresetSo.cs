using TnieYuPackage.CustomAttributes.Runtime;
using UnityEngine;

namespace _Project.Scripts.TowerDefense.Entity
{
    [CreateAssetMenu(fileName = "Entity", menuName = "Game/TD/Entity/Preset")]
    public class EntityPresetSo : ScriptableObject
    {
        public string entityId;
        public float maxHp;
        public float def;
        public float moveSpeed;
        public int earningMoney;
        public RuntimeAnimatorController animatorController;

        [SerializeReference, AbstractSupport(typeof(IEntityBehaviourInstaller))]
        public IEntityBehaviourInstaller behaviourInstaller;
    }
}