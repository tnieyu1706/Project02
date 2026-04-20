using Eflatun.SceneReference;
using UnityEngine;

namespace Game.BaseGameplay
{
    public enum LevelType
    {
        Easy,
        Medium,
        Hard
    }

    public abstract class BaseGameplayLevel : ScriptableObject
    {
        public int startMoney;
        public int baseMaxHealth;
        public SceneReference levelScene;

        public virtual string GetDisplayedText()
        {
            return $"Money: {startMoney}\n" +
                   $"Health: {baseMaxHealth}";
        }

        public abstract void SetupGameplay();
    }
}