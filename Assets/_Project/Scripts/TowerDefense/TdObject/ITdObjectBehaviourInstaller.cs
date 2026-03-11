using UnityEngine;

namespace Game.Td
{
    public interface ITdObjectBehaviourInstaller
    {
        void Install(GameObject tdObject);
        
        void UnInstall(GameObject tdObject);
    }

    public interface ITdObjectBehaviour
    {
    }
}