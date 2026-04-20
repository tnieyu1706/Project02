using System;
using System.Collections.Generic;
using System.Linq;
using Eflatun.SceneReference;
using TnieYuPackage.CustomAttributes;
using UnityEngine;

namespace SceneManagement
{
    //can refactor sceneGroup => sceneGroup tree to allow dependencies for a sceneData.
    [Serializable]
    public class SceneGroup : ICloneable
    {
        public string groupName;
        public List<SceneData> scenes;

        public string FindSceneNameByType(SceneType sceneType)
        {
            return scenes.FirstOrDefault(scene => scene.sceneType == sceneType)?.Name;
        }

        public SceneData FindSceneDataByType(SceneType sceneType)
        {
            return scenes.FirstOrDefault(scene => scene.sceneType == sceneType);
        }

        public object Clone()
        {
            return new SceneGroup
            {
                groupName = this.groupName,
                scenes = scenes.ToList()
            };
        }
    }

    [Serializable]
    public class SceneData
    {
        public SceneReference reference;
        public SceneType sceneType;
        public bool alwaysReload;

        [SerializeReference] [AbstractSupport(abstractTypes: typeof(ICompletedAction))]
        public List<ICompletedAction> completedActions = new();

        public string Name => reference.Name;
    }

    [Serializable]
    public enum SceneType
    {
        Active,
        Level,
        GlobalGameplay,
        Gameplay,
        HUD,
        UI,
        Support,
    }

    public interface ICompletedAction
    {
        void Complete(string sceneName);
    }

    public class DefaultCompletedAction : ICompletedAction
    {
        public Action OnPerform;

        public DefaultCompletedAction(Action onPerform)
        {
            this.OnPerform = onPerform;
        }

        public void Complete(string sceneName)
        {
            OnPerform?.Invoke();
        }
    }
}