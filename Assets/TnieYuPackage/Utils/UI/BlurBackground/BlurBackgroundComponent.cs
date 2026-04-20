using System;
using System.Collections.Generic;
using EditorAttributes;
using KBCore.Refs;
using UnityEngine;

namespace TnieYuPackage.Utils
{
    public class BlurBackgroundComponent : MonoBehaviour
    {
        [SerializeField, Self] internal Canvas canvas;
        public string blurBackgroundId;

        private readonly List<IDisplayGUI> relatedGuis = new();
        public Action OnOpened;
        public Action OnClosed;

        internal void Setup(BlurBackgroundConfig config, string id)
        {
            blurBackgroundId = id;
            canvas.sortingOrder = config.sortingOrder;
            canvas.renderMode = config.renderMode;

            if (config.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                canvas.gameObject.AddComponent<CanvasSupporter>();
            }
        }

        public void RegistryRelated(IDisplayGUI gui)
        {
            relatedGuis.Add(gui);
        }

        public void UnRegistryRelated(IDisplayGUI gui)
        {
            relatedGuis.Remove(gui);
        }

        [Button]
        public void Show()
        {
            canvas.enabled = true;
            OnOpened?.Invoke();
        }

        [Button]
        public void CloseAll()
        {
            CloseManual();

            relatedGuis.ForEach(gui => gui.Hide());
        }

        [Button]
        public void CloseManual()
        {
            canvas.enabled = false;
            OnClosed?.Invoke();
        }
    }
}