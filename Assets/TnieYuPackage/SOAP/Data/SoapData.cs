using System;
using EditorAttributes;
using SetProperty;
using UnityEngine;

namespace TnieYuPackage.SOAP
{
    public interface ISoapData<T>
    {
        public T Value { get; set; }
        public Action<T> OnValueChange { get; set; }
    }

    [Serializable]
    public class SoapData<T> : ISoapData<T>
    {
        [SerializeField] [SetProperty(nameof(Value))]
        private T value;

        public T Value
        {
            get => value;
            set
            {
                this.value = value;
                OnValueChange?.Invoke(value);
            }
        }

        public Action<T> OnValueChange { get; set; }
    }

    public abstract class SoapDataSo<TData, T> : ScriptableObject
        where TData : ISoapData<T>
    {
        public TData data;

        #region ResetOnPlay

        [SerializeField] private bool resetOnPlay = false;

        [SerializeField] [ShowField(nameof(resetOnPlay))]
        protected T defaultValue;

        private void OnDisable()
        {
            if (resetOnPlay)
                ReloadData();
        }

        protected virtual void ReloadData()
        {
            data.Value = defaultValue;
        }

        #endregion
    }
}