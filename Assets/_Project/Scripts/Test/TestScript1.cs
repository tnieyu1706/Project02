using System.Threading;
using _Project.Test.SOAP;
using Cysharp.Threading.Tasks;
using EditorAttributes;
using TnieYuPackage.DictionaryUtilities;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

public class TestScript1 : MonoBehaviour
{
    public TestSoData testSoData;

    public SerializableDictionary<string, int> values;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            DoSomething(100, "Hello World");
        }
    }

    [Button]
    private void DoSomething(int scoreValue, string stringValue)
    {
        testSoData.score = scoreValue;
        testSoData.stringValue = stringValue;

        var dictSource = testSoData.data1.dictData;
        dictSource.Dictionary.Clear();
        foreach (var value in values.Dictionary)
        {
            dictSource[value.Key] = value.Value;
        }

        dictSource.RewriteData();
    }
}