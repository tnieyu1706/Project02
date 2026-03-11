using System;
using EditorAttributes;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.U2D.Animation;

public class TestScript1 : MonoBehaviour
{
    public SplineAnimate SplineAnimate;
    [SerializeField, TagDropdown] public string[] tag;
    
    private SplineContainer spline => SplineAnimate.Container;

    [SerializeField]
    private Vector2 offset;

    void Start()
    {
        SplineAnimate.Updated += MoveOffsetEachUpdated;
    }

    private void OnDestroy()
    {
        SplineAnimate.Updated -= MoveOffsetEachUpdated;
    }

    void MoveOffsetEachUpdated(Vector3 pos, Quaternion rot)
    {
        gameObject.transform.position = (Vector2)pos + offset;
    }

    [Button]
    public void TestMove(float progress)
    {
        float t = progress;

        Vector3 pos = spline.EvaluatePosition(t);
        transform.position = (Vector2)pos + offset;
    }

    public void Test()
    {
        
    }

    [Button]
    private void Play()
    {
        SplineAnimate.Play();
    }

    [Button]
    private void Stop()
    {
        SplineAnimate.Pause();
    }
}