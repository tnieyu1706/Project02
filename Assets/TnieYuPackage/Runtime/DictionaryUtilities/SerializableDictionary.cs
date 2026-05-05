using System;

namespace TnieYuPackage.DictionaryUtilities
{
    [Serializable]
    public class SerializableDictionary<TKey, TValue> :
        BaseSerializableDictionary<SerializableKeyPair<TKey, TValue>, TKey, TValue>
    {
    }
}