using System;

namespace DictionaryUtilities.Runtime
{
    [Serializable]
    public class SerializableDictionaryAbstract<TKey, TValue> :
        BaseSerializableDictionary<SerializableKeyPairAbstract<TKey, TValue>, TKey, TValue>
    {
    }
}