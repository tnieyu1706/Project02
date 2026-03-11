using System;
using BackboneLogger;
using EditorAttributes;
using TnieYuPackage.DesignPatterns;
using UnityEngine;
using UnityEngine.Events;

namespace SceneManagement
{
    [DefaultExecutionOrder(-500)]
    public class SceneLoader : SingletonBehavior<SceneLoader>
    {
        public SceneGroupManager manager;
        private bool isLoading;

        #region PROPERTIES

        [SerializeField, Required] private Camera loadingCamera;
        [SerializeField, Required] private Canvas loadingCanvas;

        [SerializeField] private UnityEvent<float> onLoadingProgressUpdate;

        #endregion

        protected override void Awake()
        {
            dontDestroyOnLoad = false;
            base.Awake();

            manager = new SceneGroupManager();
        }

        private void OnEnable()
        {
            manager.OnPreAllLoad += PreSceneGroupLoad;
            manager.OnAllCompletedLoad += CompleteSceneGroupLoad;
        }

        private void OnDisable()
        {
            manager.OnPreAllLoad -= PreSceneGroupLoad;
            manager.OnAllCompletedLoad -= CompleteSceneGroupLoad;
        }

        #region LOADING EVENTS

        private void PreSceneGroupLoad()
        {
            loadingCamera.enabled = true;
            loadingCanvas.enabled = true;
            isLoading = true;
        }

        private void CompleteSceneGroupLoad()
        {
            loadingCanvas.enabled = false;
            isLoading = false;
            loadingCamera.enabled = false;
        }

        #endregion

        public void Load(SceneGroup sceneGroup)
        {
            if (isLoading)
            {
                BLogger.Log($"[SceneLoader] Still loading scene", LogLevel.Warning,category: "System");
                return;
            }

            IProgress<float> loadingProgress =
                new Progress<float>(p => onLoadingProgressUpdate?.Invoke(p));

            manager.LoadSceneAsync(sceneGroup, loadingProgress).Forget();
        }
    }
}