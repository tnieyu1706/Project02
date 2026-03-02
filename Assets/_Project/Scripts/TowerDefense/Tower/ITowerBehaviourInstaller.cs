using UnityEngine;

namespace _Project.Scripts.TowerDefense.Tower
{
    public interface ITowerBehaviourInstaller
    {
        void Install(GameObject towerObject);
        
        void UnInstall(GameObject towerObject);
    }

    public interface ITowerBehaviour
    {
        
    }
}