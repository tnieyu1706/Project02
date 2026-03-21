using System;
using System.Collections.Generic;
using KBCore.Refs;
using UnityEngine;

namespace TnieYuPackage.Utils
{
    public class BlurBackgroundComponent : MonoBehaviour
    {
        [SerializeField, Self] internal Canvas canvas;

        private readonly List<IDisplayGUI> relatedGuis = new();
        public Action OnOpened;
        public Action OnClosed;
        
        internal void Setup(int orderLayer)
        {
            canvas.sortingOrder = orderLayer;
        }

        public void RegistryRelated(IDisplayGUI gui)
        {
            relatedGuis.Add(gui);
        }

        public void UnRegistryRelated(IDisplayGUI gui)
        {
            relatedGuis.Remove(gui);
        }

        public void Open()
        {
            canvas.enabled = true;
            OnOpened?.Invoke();
        }

        public void CloseAll()
        {
            CloseManual();
            
            relatedGuis.ForEach(gui => gui.Hide());
        }

        public void CloseManual()
        {
            canvas.enabled = false;
            OnClosed?.Invoke();
        }
    }
}