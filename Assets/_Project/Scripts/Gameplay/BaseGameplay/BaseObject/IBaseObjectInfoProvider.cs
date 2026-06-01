using System.Collections.Generic;

namespace Game.BaseGameplay
{
    // Định nghĩa tất cả các loại chỉ số có thể hiển thị trong game
    public enum BaseObjPropertyType
    {
        Type,
        Health,
        Defense,
        MoveSpeed,
        BaseDamage,
        AttackDamage,
        AttackSpeed,
        Cost,
        DropMoney,
        Capacity // Thêm mục này để hiển thị sức chứa lính của Barrack Tower
    }

    // Interface dùng cho các ObjectRuntime (Tower, Soldier, Enemy)
    public interface IBaseObjectInfoProvider
    {
        string GetObjectName();
        Dictionary<BaseObjPropertyType, string> GetObjectInfo();
    }

    // Interface dành riêng cho các Strategy (Ví dụ: AttackStrategy) 
    // để chúng tự đẩy thông tin sát thương/tốc đánh vào UI
    public interface IStrategyInfoProvider
    {
        void AppendStrategyInfo(Dictionary<BaseObjPropertyType, string> infoMap);
    }
}