using UnityEngine;

namespace TnieYuPackage.Utils.Components
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