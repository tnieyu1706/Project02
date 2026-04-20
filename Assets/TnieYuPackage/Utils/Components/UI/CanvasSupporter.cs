using Cysharp.Threading.Tasks;
using TnieYuPackage.DesignPatterns;
using UnityEngine;

namespace TnieYuPackage.Utils
{
    [DefaultExecutionOrder(-15)]
    [RequireComponent(typeof(Canvas))]
    public class CanvasSupporter : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;

        void Awake()
        {
            canvas ??= GetComponent<Canvas>();
        }

        async void Start()
        {
            await UniTask.Yield();
            canvas.worldCamera = Registry<Camera>.GetFirst();
        }
    }
}