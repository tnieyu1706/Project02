using EditorAttributes;
using UnityEngine;
using UnityEngine.Events;

namespace SceneManagement
{
    public class SceneLoaderGUI : MonoBehaviour
    {
        #region PROPERTIES

        [SerializeField, Required] private Camera loadingCamera;
        [SerializeField, Required] private Canvas loadingCanvas;

        [SerializeField] private UnityEvent<float> onLoadingProgressUpdate = new();

        #endregion

        #region LOADING EVENTS

        private void OnEnable()
        {
            SceneLoader.Instance.manager.OnLoadStarted += OnLoadStarted;
            SceneLoader.Instance.manager.OnLoadEnded += OnLoadEnded;
            SceneLoader.Instance.LoadingProgress.ProgressChanged += OnProgressChanged;
        }

        private void OnLoadStarted()
        {
            onLoadingProgressUpdate.Invoke(0);
            loadingCamera.enabled = true;
            loadingCanvas.enabled = true;
        }

        private void OnProgressChanged(float progress)
        {
            onLoadingProgressUpdate.Invoke(progress);
        }

        private void OnLoadEnded()
        {
            loadingCanvas.enabled = false;
            loadingCamera.enabled = false;
        }

        private void OnDisable()
        {
            if (SceneLoader.Instance == null) return;
            SceneLoader.Instance.manager.OnLoadStarted -= OnLoadStarted;
            SceneLoader.Instance.manager.OnLoadEnded -= OnLoadEnded;
            SceneLoader.Instance.LoadingProgress.ProgressChanged -= OnProgressChanged;
        }

        #endregion
    }
}