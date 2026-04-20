using System;
using Cysharp.Threading.Tasks;
using Eflatun.SceneReference;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SceneManagement.CompletedActions
{
    [Serializable]
    public class LoadSceneCompletedAction : ICompletedAction
    {
        public SceneReference loadedScene;
        public bool IsCompleted { get; set; }

        public async void Complete(string sceneName)
        {
            Debug.Log($"[LoadSceneCompletedAction] Complete: {sceneName}");
            var sceneAsync = SceneManager.LoadSceneAsync(loadedScene.Name, LoadSceneMode.Additive);
            if (sceneAsync == null) return;
            while (!sceneAsync.isDone)
            {
                await UniTask.Delay(SceneGroupManager.PROGRESS_DELAY_MILLISECONDS);
            }

            IsCompleted = true;
        }
    }
}