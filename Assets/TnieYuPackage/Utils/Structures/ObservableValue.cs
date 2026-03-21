using System;
using UnityEngine;

namespace TnieYuPackage.Utils.Structures
{
    [Serializable]
    public struct ObservableValue<T>
    {
        [SerializeField] private T value;

        public T Value
        {
            get => this.value;
            set
            {
                this.value = value;
                OnValueChanged?.Invoke(value);
            }
        }

        public event Action<T> OnValueChanged;
    }
}