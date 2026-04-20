using System.Linq;
using EditorAttributes;
using TnieYuPackage.DesignPatterns;
using UnityEngine;

public class TestRegistryBehaviour : RegistryBehaviour<TestRegistryBehaviour>
{
    [Button]
    void CheckRegistry()
    {
        Debug.Log($"{Registry<TestRegistryBehaviour>.All.Count()}");
    }
}