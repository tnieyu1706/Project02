using _Project.Scripts.Gameplay.Global.GameController;
using _Project.Scripts.Gameplay.Global.PlayerDataSystem;
using Cysharp.Threading.Tasks;
using EditorAttributes;
using Game.BaseGameplay;
using Gameplay.Global;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Gameplay.Global.UI
{
    public class GameMenuGUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup gameMenuCanvas;
        [SerializeField] private CanvasGroup settingsCanvas;
        [SerializeField, Required] private Button loadButton;
        [SerializeField, Required] private Slider masterSlider;
        [SerializeField, Required] private Slider sfxSlider;

        void Start()
        {
            loadButton.interactable = PlayerDataManager.IsDataFileExist();

            OpenOnlyMainMenu();
            masterSlider.value = GameSettingsController.Instance.masterVolume * masterSlider.maxValue;
            sfxSlider.value = GameSettingsController.Instance.sfxVolume * sfxSlider.maxValue;
        }

        public void StartGame()
        {
            LoadWorldMap();
        }

        public void ContinueGame()
        {
            if (!PlayerDataManager.IsDataFileExist()) return;

            LoadWorldMap();
        }

        private void LoadWorldMap()
        {
            PlayerDataManager.Instance?.Load();
            GameplayTransition.LoadWorldMapGame().Forget();
        }

        public void OpenSettingsPanel()
        {
            settingsCanvas.alpha = 1;
            settingsCanvas.interactable = true;
            settingsCanvas.blocksRaycasts = true;
        }

        public void OpenOnlyMainMenu()
        {
            settingsCanvas.alpha = 0;
            settingsCanvas.interactable = false;
            settingsCanvas.blocksRaycasts = false;
        }

        public void HandleMasterSliderChanged(float value)
        {
            GameSettingsController.Instance.masterVolume = value / masterSlider.maxValue;
        }

        public void HandleSfxSliderChanged(float value)
        {
            GameSettingsController.Instance.sfxVolume = value / sfxSlider.maxValue;
        }
    }
}