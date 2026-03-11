using System;
using System.Collections.Generic;
using System.Linq;
using Eflatun.SceneReference;
using TnieYuPackage.CustomAttributes;
using UnityEngine;

namespace SceneManagement
{
    [CreateAssetMenu(fileName = "SceneGroup", menuName = "SceneManagement/SceneGroup")]
    public class SceneGroup : ScriptableObject
    {
        public string groupName;
        public List<SceneData> scenes;

        public string FindSceneNameByType(SceneType sceneType)
        {
            return scenes.FirstOrDefault(scene => scene.sceneType == sceneType)?.Name;
        }
    }

    [Serializable]
    public class SceneData
    {
        public SceneReference reference;
        public SceneType sceneType;
        public bool alwaysReload;
        [SerializeReference] [AbstractSupport(abstractTypes: typeof(ISceneCompletedAction))]
        public ISceneCompletedAction completedAction;

        public string Name => reference.Name;
    }

    [Serializable]
    public enum SceneType
    {
        Active,
        MainMenu,
        UI,
        Level
    }

    public interface ISceneCompletedAction
    {
        void Complete(string sceneName);
    }
}