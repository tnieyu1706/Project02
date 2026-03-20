using UnityEngine;

namespace TnieYuPackage.Utils
{
    [DefaultExecutionOrder(-20)]
    [RequireComponent(typeof(Canvas))]
    public class CanvasSupporter : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;

        void Awake()
        {
            canvas ??= GetComponent<Canvas>();

            canvas.worldCamera = Registry<Camera>.GetFirst();
        }
    }
}