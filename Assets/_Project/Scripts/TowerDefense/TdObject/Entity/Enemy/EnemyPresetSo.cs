using System.Collections.Generic;
using TnieYuPackage.CustomAttributes;
using UnityEngine;

namespace Game.Td
{
    [CreateAssetMenu(fileName = "Enemy", menuName = "Game/TD/Entity/Enemy/Preset")]
    public class EnemyPresetSo : ScriptableObject
    {
        public string enemyId;
        public float maxHp;
        public float def;
        public float moveSpeed;
        public int mapDmg;
        public int earningMoney;
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