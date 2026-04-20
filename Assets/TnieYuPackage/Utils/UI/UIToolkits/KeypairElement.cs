using System;
using TnieYuPackage.GlobalExtensions;
using UnityEngine.UIElements;

namespace TnieYuPackage.Utils
{
    /// <summary>
    /// A generic key-value pair element, where both key and value are VisualElements. This can be used to create various UI components that require a label (key) and an associated field (value), such as settings entries, form fields, etc.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class KeypairElement<TKey, TValue> : BaseElement
        where TKey : VisualElement, new()
        where TValue : VisualElement, new()
    {
        private const string ELEMENT_PREFIX = "keypair-element";
        protected override string Prefix => ELEMENT_PREFIX;

        public readonly TKey Key;
        public readonly TValue Value;

        public KeypairElement()
        {
            this.AddClass(ELEMENT_PREFIX);
            Key = this.CreateChild<TKey>(GetStyleName("key"));
            Value = this.CreateChild<TValue>(GetStyleName("value"));
        }
    }

    public class KeypairParamsElement<TKey, TValue> : BaseElement
        where TKey : VisualElement
        where TValue : VisualElement
    {
        private const string ELEMENT_PREFIX = "keypair-params-element";
        protected override string Prefix => ELEMENT_PREFIX;

        public TKey Key;
        public TValue Value;

        public KeypairParamsElement(Func<TKey> keyFactory, Func<TValue> valueFactory)
        {
            this.AddClass(ELEMENT_PREFIX);
            Key = keyFactory?.Invoke();
            Value = valueFactory?.Invoke();
        }
    }
}