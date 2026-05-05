using System;
using UnityEngine;

namespace TnieYuPackage.Utils
{
    [Serializable]
    public class ObservableValue<T> : IComparable<ObservableValue<T>>
    {
        [SerializeField] private T value;

        public ObservableValue(T value)
        {
            this.value = value;
        }

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

        public void Refresh()
        {
            OnValueChanged?.Invoke(this.value);
        }

        public int CompareTo(ObservableValue<T> other)
        {
            if (this.Value == null && other.Value == null) return 0;
            if (this.Value == null && other.Value != null) return -1;
            if (this.Value != null && other.Value == null) return 1;

            if (this.Value is IComparable thisComparable && other.Value is IComparable otherComparable)
            {
                return thisComparable.CompareTo(otherComparable);
            }
            
            throw new ArgumentException($"Cannot compare {nameof(ObservableValue<T>)} to an {nameof(ObservableValue<T>)}");
        }
    }
}