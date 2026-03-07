using KBCore.Refs;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Td
{
    [RequireComponent(typeof(Button))]
    public class TdWaveGUI : MonoBehaviour
    {
        [SerializeField, Self] private Button waveButton;

        void Awake()
        {
            TdWaveManagerGUI.Instance.WaveGuis.Add(this);

            waveButton ??= GetComponent<Button>();
        }

        void Start()
        {
            waveButton.onClick.AddListener(HandleWaveButtonClick);
        }

        private void OnDestroy()
        {
            waveButton.onClick.RemoveAllListeners();
        }

        private void HandleWaveButtonClick()
        {
            TdGameplayController.Instance.PlayWave();
            TdWaveManagerGUI.Instance.Hide();
        }
    }
}