using System;
using Cysharp.Threading.Tasks;
using TnieYuPackage.DesignPatterns;
using UnityEngine;

namespace SceneManagement
{
    public class FloatProgress : IProgress<float>
    {
        public event Action<float> ProgressChanged;

        public void Report(float value)
        {
            ProgressChanged?.Invoke(value);
        }
    }

    [DefaultExecutionOrder(-1000)]
    public class SceneLoader : Singleton<SceneLoader>
    {
        public SceneGroupManager manager;
        
        public readonly FloatProgress LoadingProgress = new();
        private bool isLoading;

        protected override void Awake()
        {
            base.Awake();

            manager.OnLoadStarted += OnPreSceneGroupLoaded;
            manager.OnLoadEnded += OnPostSceneGroupLoaded;
        }

        private void OnPreSceneGroupLoaded()
        {
            isLoading = true;
        }

        private void OnPostSceneGroupLoaded()
        {
            isLoading = false;
        }

        public UniTask Load(SceneGroup sceneGroup, bool applyDelay = true)
        {
            if (isLoading)
            {
                Debug.Log($"[SceneGroupManager] Loading scene group {sceneGroup}");
                return UniTask.CompletedTask;
            }

            return manager.LoadSceneAsync(sceneGroup, LoadingProgress, applyDelay);
        }
    }
}