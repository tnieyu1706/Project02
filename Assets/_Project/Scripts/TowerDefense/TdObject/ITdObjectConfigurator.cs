using UnityEngine;

namespace Game.Td
{
    public interface ITdObjectConfigurator
    {
        void Config(GameObject tdObject);
        void UnConfig(GameObject tdObject);
    }
}