using PrimeTween;
using UnityEngine;

public class TestScript1 : MonoBehaviour
{
    public void TestMethod()
    {
        gameObject.layer = LayerMask.NameToLayer("Test");
        
    }

    
}