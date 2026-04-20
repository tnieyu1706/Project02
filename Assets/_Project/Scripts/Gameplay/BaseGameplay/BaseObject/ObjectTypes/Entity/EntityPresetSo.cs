using EditorAttributes;
using Game.Global;

namespace Game.BaseGameplay
{
    public abstract class EntityPresetSo : BaseObjectPresetSo
    {
        public float maxHp;
        public float def;

        public ArmyType armyType;
        [LayerDropdown] public string entityLayer;
    }
}