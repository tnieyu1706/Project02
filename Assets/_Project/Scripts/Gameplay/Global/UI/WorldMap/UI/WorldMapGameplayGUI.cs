using System;
using _Project.Scripts.Gameplay.Global.PlayerDataSystem;
using Cysharp.Threading.Tasks;
using Gameplay.Global;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Gameplay.Global.UI.WorldMap
{
    public class WorldMapGameplayGUI : MonoBehaviour
    {
        [SerializeField] private Button returnMainMenuBtn;

        private void OnEnable()
        {
            if (returnMainMenuBtn != null)
            {
                returnMainMenuBtn.onClick.AddListener(OnReturnMainMenuClicked);
            }
        }

        private void OnDisable()
        {
            if (returnMainMenuBtn != null)
            {
                returnMainMenuBtn.onClick.RemoveListener(OnReturnMainMenuClicked);
            }
        }

        private void OnReturnMainMenuClicked()
        {
            // Tắt tương tác nút để tránh bấm nhiều lần
            returnMainMenuBtn.interactable = false;

            // Lưu dữ liệu người chơi trước khi thoát
            if (PlayerDataManager.Instance != null)
            {
                PlayerDataManager.Instance.Save();
                Debug.Log("Player data saved successfully before returning to Main Menu.");
            }
            else
            {
                Debug.LogWarning("PlayerDataManager Instance is null! Cannot save data.");
            }

            // Thực hiện chuyển cảnh về Main Menu
            GameplayTransition.LoadMainMenuGame().Forget();
        }
    }
}