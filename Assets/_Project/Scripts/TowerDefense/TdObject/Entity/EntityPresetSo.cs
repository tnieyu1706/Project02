using EditorAttributes;

namespace Game.Td
{
    public abstract class EntityPresetSo : TdObjectPresetSo
    {
        public float maxHp;
        public float def;
        [LayerDropdown] public string entityLayer;
    }
}