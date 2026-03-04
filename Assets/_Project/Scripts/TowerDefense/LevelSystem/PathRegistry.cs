using KBCore.Refs;
using UnityEngine;
using UnityEngine.Splines;

namespace Game.Td
{
    [RequireComponent(typeof(SplineContainer))]
    public class PathRegistry : MonoBehaviour
    {
        [SerializeField] private string pathId;
        [Self] private SplineContainer path;

        private void Awake()
        {
            path ??= GetComponent<SplineContainer>();
        }

        void OnEnable()
        {
            TdGameplayController.Instance.Paths.Add(pathId, path);
        }

        void OnDisable()
        {
            if (TdGameplayController.Instance != null)
            {
                TdGameplayController.Instance.Paths.Remove(pathId);
            }
        }
    }
}