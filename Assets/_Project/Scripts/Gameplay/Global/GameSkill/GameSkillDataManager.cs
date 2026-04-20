using TnieYuPackage.Utils;
using UnityEngine;

namespace Game.Global
{
    [CreateAssetMenu(fileName = "GameSkillDataManager", menuName = "Game/Global/GameSkillDataManager")]
    public class GameSkillDataManager : BaseAssetManager<string, GameSkillData, GameSkillDataManager>
    {
        
        protected override string GetAssetIdentify(GameSkillData asset)
        {
            return asset.skillId;
        }
    }
}