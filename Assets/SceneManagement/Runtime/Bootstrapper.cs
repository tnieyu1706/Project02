using UnityEngine;
using UnityEngine.SceneManagement;

namespace SceneManagement
{
    public static class Bootstrapper
    {
        public const string BOOTSTRAPPER_SCENE_NAME = "Bootstrapper";
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static async void Init()
        {
            Debug.Log("Bootstrapper Initialized...");
            await SceneManager.LoadSceneAsync(BOOTSTRAPPER_SCENE_NAME, LoadSceneMode.Single);
        }
    }
}