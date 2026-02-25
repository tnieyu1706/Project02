using System;

namespace DictionaryUtilities.Runtime
{
    [Serializable]
    public class SerializableDictionary<TKey, TValue> :
        BaseSerializableDictionary<SerializableKeyPair<TKey, TValue>, TKey, TValue>
    {
    }
}