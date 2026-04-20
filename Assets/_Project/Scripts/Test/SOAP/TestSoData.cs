using System;
using TnieYuPackage.DictionaryUtilities;
using UnityEngine;

namespace _Project.Test.SOAP
{
    [Serializable]
    public class TestData
    {
        public SerializableDictionary<string, int> dictData;
    }

    [CreateAssetMenu(fileName = "TestSoData", menuName = "Test/Soap/TestSoData")]
    public class TestSoData : ScriptableObject
    {
        public int score;

        public string stringValue;

        public TestData data1;
    }
}