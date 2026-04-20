using System;
using BackboneLogger;
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

    [DefaultExecutionOrder(-500)]
    public class SceneLoader : SingletonBehavior<SceneLoader>
    {
        public SceneGroupManager manager;
        
        public readonly FloatProgress LoadingProgress = new();
        private bool isLoading;

        protected override void Awake()
        {
            dontDestroyOnLoad = false;
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

        public UniTask Load(SceneGroup sceneGroup)
        {
            if (isLoading)
            {
                BLogger.Log($"[SceneLoader] Still loading scene", LogLevel.Warning, category: "System");
                return UniTask.CompletedTask;
            }

            return manager.LoadSceneAsync(sceneGroup, LoadingProgress);
        }
    }
}