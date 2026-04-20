using EditorAttributes;
using TnieYuPackage.DesignPatterns;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Global.GameController
{
    //singleton for easy to read. when build pack to static class only
    public class GameTimeController : SingletonBehavior<GameTimeController>
    {
        [Header("Current Setting")] [SerializeField, ReadOnly]
        private int targetFrameRate = 60;

        [SerializeField, ReadOnly] private float timeScale = 1;

        [Header("Default Setting")] [SerializeField]
        private int defaultFrameRate = 60;

        [SerializeField] private float defaultTimeScale = 1;

        [Header("Previous Setting")] [SerializeField, ReadOnly]
        private int preFrameRate;

        [SerializeField, ReadOnly] private float preTimeScale;

        public static int TargetFrameRate => Instance.targetFrameRate;

        public static float TimeScale => Instance.timeScale;

        public static int PreFrameRate => Instance.preFrameRate;

        public static float PreTimeScale => Instance.preTimeScale;

        //Set Manual
        [Button]
        private void SetFrameRateManual(int frameRate)
        {
            SetFrameRate(frameRate);
        }

        [Button]
        private void SetTimeScaleManual(float timeScaleSource)
        {
            SetTimeScale(timeScaleSource);
        }


        //Can remake by [InitializeOnLoad...]
        protected override void Awake()
        {
            base.Awake();

            SetGameToDefaultState();
        }

        public static void SetGameStop()
        {
            SetFrameRateStop();
            SetTimeScaleStop();
        }

        public static void SetGameToPreviousState()
        {
            SetFrameRateToPrevious();
            SetTimeScaleToPrevious();
        }

        public static void SetGameToDefaultState()
        {
            SetFrameRateToDefault();
            SetTimeScaleToDefault();
        }

        public static void SetFrameRateStop()
        {
            SetFrameRate(0);
        }

        public static void SetTimeScaleStop()
        {
            SetTimeScale(0);
        }

        public static void SetFrameRate(int frameRate)
        {
            Instance.preFrameRate = Instance.targetFrameRate;
            Instance.targetFrameRate = frameRate;
            Application.targetFrameRate = frameRate;
        }

        public static void SetTimeScale(float timeScaleSource)
        {
            Instance.preTimeScale = Instance.timeScale;
            Instance.timeScale = timeScaleSource;
            Time.timeScale = timeScaleSource;
        }

        public static void SetFrameRateToDefault()
        {
            SetFrameRate(Instance.defaultFrameRate);
            Instance.preFrameRate = Instance.defaultFrameRate;
        }

        public static void SetTimeScaleToDefault()
        {
            SetTimeScale(Instance.defaultTimeScale);
            Instance.preTimeScale = Instance.defaultTimeScale;
        }

        public static void SetFrameRateToPrevious()
        {
            SetFrameRate(Instance.preFrameRate);
        }

        public static void SetTimeScaleToPrevious()
        {
            SetTimeScale(Instance.preTimeScale);
        }
    }
}