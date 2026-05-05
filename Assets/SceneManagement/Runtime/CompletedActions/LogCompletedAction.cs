using System;
using UnityEngine;

namespace SceneManagement.CompletedActions
{
    [Serializable]
    public class LogCompletedAction : ICompletedAction
    {
        [TextArea(3, 10)] public string message;

        public void Complete(string sceneName)
        {
            Debug.Log(message);
        }
    }
}