using UnityEngine;

namespace _Project.Scripts.TowerDefense.Entity
{
    public interface IEntityBehaviourInstaller
    {
        void Install(GameObject entity);
        void Uninstall(GameObject entity);
    }
}