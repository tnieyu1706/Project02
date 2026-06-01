using System;
using TnieYuPackage.DesignPatterns;
using UnityEngine;
using UnityEngine.UI;

namespace Game.WaveAttack
{
    public class WaGameplayUI : Singleton<WaGameplayUI>
    {
        [SerializeField] private Button waPanelBtn;

        private void OnEnable()
        {
            waPanelBtn.onClick.AddListener(HandleWaPanelBtnClicked);
        }

        private void HandleWaPanelBtnClicked()
        {
            if (!WaGameplayPanelUIToolkit.HasInstance) return;

            WaGameplayPanelUIToolkit.Instance.ShowPanel();
        }

        private void OnDisable()
        {
            waPanelBtn.onClick.RemoveAllListeners();
        }
    }
}