using SetProperty;
using UnityEngine;
using UnityEngine.UI;

public class TestAnimationComponent : MonoBehaviour
{
    [SetProperty(nameof(Value1))] public int value1;
    private Image healthBar;

    public void TestFunc()
    {
        
    }
    
    public int Value1
    {
        get => value1;
        set
        {
            Debug.Log(value);

            value1 = value;
        }
    }

    public int Value2 { get; set; }
    private int value3;

    public int Value3
    {
        get => value3;
        set => value3 = value;
    }
}