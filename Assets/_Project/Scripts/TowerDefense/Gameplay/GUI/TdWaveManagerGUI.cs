using System.Collections.Generic;
using EditorAttributes;
using TnieYuPackage.DesignPatterns;
using UnityEngine;

namespace Game.Td
{
    public class TdWaveManagerGUI : SingletonBehavior<TdWaveManagerGUI>
    {
        [SerializeField, ReadOnly] private List<TdWaveGUI> waveGuis = new();

        public List<TdWaveGUI> WaveGuis
        {
            get
            {
                waveGuis ??= new();

                return waveGuis;
            }
        }

        protected override void Awake()
        {
            dontDestroyOnLoad = false;
            base.Awake();

            TdGameplayController.Instance.tdWaveController.OnWaveCompleted += Display;
        }

        private void Display()
        {
            WaveGuis.ForEach(w => w.gameObject.SetActive(true));
        }

        internal void Hide()
        {
            WaveGuis.ForEach(w => w.gameObject.SetActive(false));
        }
    }
}