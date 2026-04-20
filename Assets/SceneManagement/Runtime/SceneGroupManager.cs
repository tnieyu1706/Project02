using System;
using System.Collections.Generic;
using System.Diagnostics;
using BackboneLogger;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using ZLinq;

namespace SceneManagement
{
    [Serializable]
    public class SceneGroupManager
    {
        public const int PROGRESS_DELAY_MILLISECONDS = 100;
        [SerializeField] [Range(0, 3f)] private float delayCompletedAll = 0.5f;

        public SceneGroup currentActiveSceneGroup;

        public event Action OnLoadStarted = delegate { };
        
        public event Action<string> OnSceneUnloaded = delegate { };
        public event Action OnUnloadCompleted = delegate { };
        
        
        public event Action<string> OnSceneLoaded = delegate { };
        public event Action<string> OnActiveSceneChanged = delegate { };
        
        public event Action OnLoadEnded = delegate { };

        public async UniTask LoadSceneAsync(SceneGroup sceneGroup, IProgress<float> progress)
        {
            OnLoadStarted?.Invoke();
            var timer = Stopwatch.StartNew();
            BLogger.Log($"[SceneGroupManager] Begin unload: {timer.Elapsed}", category: "System");

            await UnLoadScenesAsync(sceneGroup);
            BLogger.Log($"[SceneGroupManager] Begin load: {timer.Elapsed}", category: "System");

            currentActiveSceneGroup = sceneGroup;

            //prepare: next loading scenes
            List<string> maintainingScenes = SceneManagementUtils.GetLoadingScenesName();
            List<SceneData> loadingScenes = new();
            foreach (var scene in sceneGroup.scenes)
            {
                if (maintainingScenes.Contains(scene.Name)) continue;

                loadingScenes.Add(scene);
            }

            //LoadingScene
            var operationGroup = new AsyncOperationGroup(loadingScenes.Count);
            foreach (var scene in loadingScenes)
            {
                var operation = SceneManager.LoadSceneAsync(scene.Name, LoadSceneMode.Additive);
                if (operation == null) continue;
                operation.completed += _ =>
                {
                    OnSceneLoaded?.Invoke(scene.Name);
                    scene.completedActions.ForEach(a => a?.Complete(scene.Name));
                };

                operationGroup.Operations.Add(operation);
            }

            //Waiting
            while (!operationGroup.IsDone)
            {
                progress?.Report(operationGroup.Progress);
                await UniTask.Delay(PROGRESS_DELAY_MILLISECONDS);
            }

            BLogger.Log($"[SceneGroupManager] Completed load: {timer.Elapsed}", category: "System");
            timer.Stop();

            var sceneActiveName = sceneGroup.FindSceneNameByType(SceneType.Active);
            var sceneNeedActive = SceneManager.GetSceneByName(sceneActiveName);
            
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (sceneNeedActive == null)
            {
                BLogger.Log($"[SceneGroupManager] Failed to load scene: {sceneActiveName}",
                    LogLevel.Critical, category: "System");
            }
            else
            {
                SceneManager.SetActiveScene(sceneNeedActive);
                OnActiveSceneChanged?.Invoke(sceneActiveName);
            }

            await UniTask.Delay(TimeSpan.FromSeconds(delayCompletedAll));

            OnLoadEnded?.Invoke();
        }

        private async UniTask UnLoadScenesAsync(SceneGroup sceneGroup)
        {
            var oldScenesName = SceneManagementUtils.GetLoadingScenesName();

            //prepare: unload necessary scenes
            var unloadSceneNames = new List<string>();
            foreach (var scene in oldScenesName)
            {
                if (scene == Bootstrapper.BOOTSTRAPPER_SCENE_NAME) continue;
                if (sceneGroup.scenes
                    .AsValueEnumerable()
                    .Any(s => s.Name == scene && !s.alwaysReload)) continue;

                unloadSceneNames.Add(scene);
            }

            //Handle UnloadScenes
            var operationGroup = new AsyncOperationGroup(unloadSceneNames.Count);
            foreach (var s in unloadSceneNames)
            {
                var operation = SceneManager.UnloadSceneAsync(s);
                operationGroup.Operations.Add(operation);

                OnSceneUnloaded?.Invoke(s);
            }

            //Waiting
            while (!operationGroup.IsDone)
            {
                await UniTask.Delay(PROGRESS_DELAY_MILLISECONDS); // tight loop
            }

            // Optional: UnloadUnusedAssets - unload all unused asset from memory 
            await Resources.UnloadUnusedAssets();

            OnUnloadCompleted?.Invoke();
            BLogger.Log($"[SceneGroupManager] Unload old scenes completed", category: "System");
        }
    }

    public readonly struct AsyncOperationGroup
    {
        public readonly List<AsyncOperation> Operations;

        public float Progress =>
            Operations.Count == 0
                ? 0
                : Operations.AsValueEnumerable().Average(o => o.progress);

        public bool IsDone =>
            Operations.Count == 0 || Operations.AsValueEnumerable().All(o => o.isDone);

        public AsyncOperationGroup(int capacity)
        {
            Operations = new List<AsyncOperation>(capacity);
        }
    }
}