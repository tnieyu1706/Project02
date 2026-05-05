using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Global
{
    [CreateAssetMenu(fileName = "GameSkillDataManager", menuName = "Game/Global/GameSkillDataManager")]
    public class GameSkillDataManager : ScriptableObject
    {
        public List<GameSkillData> skills = new();
        private Dictionary<string, GameSkillData> refs;

        public Dictionary<string, GameSkillData> Refs =>
            refs ??= skills.ToDictionary(s => s.skillId, s => s);
    }
}