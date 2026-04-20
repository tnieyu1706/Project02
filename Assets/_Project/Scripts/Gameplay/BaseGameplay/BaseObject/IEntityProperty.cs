using TnieYuPackage.Utils;

namespace Game.BaseGameplay
{
    public interface IHealthProperty
    {
        ObservableValue<float> Hp { get; set; }
    }

    public interface IEntityProperty : IHealthProperty
    {
        float Defense { get; }
        float MaxHp { get; }
    }
}