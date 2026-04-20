using System;
using KBCore.Refs;
using TnieYuPackage.InterfaceUtilities;
using UnityEngine;
using UnityEngine.UI;

namespace Game.BaseGameplay
{
    [RequireComponent(typeof(Image))]
    public class EntityHealthDisplay : MonoBehaviour
    {
        [SerializeField, Self] private Image healthBar;
        [SerializeField] private InterfaceReference<IEntityProperty> entityProperty;

        private void Start()
        {
            entityProperty.Value.Hp.OnValueChanged += HandleEntityHealthChanged;
        }

        private void OnEnable()
        {
            healthBar.enabled = false;
        }

        private void OnDestroy()
        {
            if (entityProperty is { Value: not null })
            {
                entityProperty.Value.Hp.OnValueChanged -= HandleEntityHealthChanged;
            }
        }

        private void HandleEntityHealthChanged(float health)
        {
            var maxHp = entityProperty.Value.MaxHp;
            healthBar.fillAmount = health / maxHp;

            if (!healthBar.enabled)
                healthBar.enabled = true;
        }
    }
}