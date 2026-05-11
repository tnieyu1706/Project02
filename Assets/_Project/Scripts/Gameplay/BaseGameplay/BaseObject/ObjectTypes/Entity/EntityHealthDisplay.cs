using Cysharp.Threading.Tasks;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.UI;

namespace Game.BaseGameplay
{
    [RequireComponent(typeof(Image))]
    public class EntityHealthDisplay : MonoBehaviour
    {
        [SerializeField, Self] private Image healthBar;
        [SerializeField] private InterfaceRef<IEntityProperty> entityProperty;

        private void Start()
        {
            entityProperty.Value.Hp.OnValueChanged += HandleEntityHealthChanged;
        }

        private async void OnEnable()
        {
            await UniTask.DelayFrame(1);
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
            if (health >= maxHp)
            {
                healthBar.enabled = false;
                return;
            }
            
            healthBar.fillAmount = health / maxHp;
            if (!healthBar.enabled)
                healthBar.enabled = true;
        }
    }
}