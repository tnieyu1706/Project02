using System;

namespace TnieYuPackage.DictionaryUtilities
{
    [Serializable]
    public class SerializableDictionaryAbstract<TKey, TValue> :
        BaseSerializableDictionary<SerializableKeyPairAbstract<TKey, TValue>, TKey, TValue>
    {
    }
}