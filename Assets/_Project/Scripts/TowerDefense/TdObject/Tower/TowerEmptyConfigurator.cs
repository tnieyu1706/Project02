using System;
using UnityEngine;

namespace Game.Td
{
    [Serializable]
    public class TowerEmptyConfigurator : ITdObjectConfigurator
    {
        [SerializeField] private Sprite emptySprite;

        public void Config(GameObject tdObject)
        {
            var spriteRenderer = tdObject.GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer == null) return;

            spriteRenderer.sprite = emptySprite;
        }

        public void UnConfig(GameObject tdObject)
        {
            var spriteRenderer = tdObject.GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer == null) return;

            spriteRenderer.sprite = null;
        }
    }
}