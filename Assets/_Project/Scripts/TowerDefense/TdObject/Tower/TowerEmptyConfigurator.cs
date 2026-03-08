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
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = emptySprite;
            }

            if (tdObject.TryGetComponent(out Animator animator))
            {
                animator.enabled = false;
            }
        }

        public void UnConfig(GameObject tdObject)
        {
            var spriteRenderer = tdObject.GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = null;
            }

            if (tdObject.TryGetComponent(out Animator animator))
            {
                animator.enabled = true;
            }
        }
    }
}