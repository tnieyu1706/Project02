using System.Collections.Generic;
using TnieYuPackage.CustomAttributes;
using UnityEngine;

namespace Game.Td
{
    [CreateAssetMenu(fileName = "Tower", menuName = "Game/TD/Tower")]
    public class TowerPresetSo : TdObjectPresetSo
    {
        public string towerId;
        public Sprite towerIcon;
        public int towerPriceValue;

        [SerializeReference]
        [AbstractSupport(
            classAssembly: typeof(ITowerUIEvent),
            abstractTypes: typeof(ITowerUIEvent)
        )]
        public List<ITowerUIEvent> uiEvents = new();
    }

    public interface ITowerUIEvent
    {
        Sprite Icon { get; }
        void Perform();
    }
}