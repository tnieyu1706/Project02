using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using EditorAttributes;
using Game.BaseGameplay;
using Gameplay.Global;
using Newtonsoft.Json.Linq;
using TnieYuPackage.DesignPatterns;
using TnieYuPackage.Utils;
using UnityEngine;

namespace Game.BuildingGameplay
{
    /// <summary>
    /// DTO Class chứa dữ liệu thời gian cần Save/Load
    /// </summary>
    [Serializable]
    public class TimeControllerSaveData
    {
        public float currentTime;
        public int eventRaiseThTime;
        public float nextRaisedEventTime;
        public JObject EventDataJson;
    }

    [DefaultExecutionOrder(-12)]
    public class SbTimeController : SingletonBehavior<SbTimeController>, ISaveLoadData<TimeControllerSaveData>
    {
        public const string PERSISTENCE_KEY = "SbTimeController";
        private const float TIME_UNIT = 5f;
        private static float TotalTimeUnit => TIME_UNIT * Time.timeScale;

        [ReadOnly] public ObservableValue<float> currentTime = new(0);

        [ReadOnly] public float nextRaisedEventTime = 0;
        [ReadOnly] public int eventRaiseThTime = 1;

        // Bổ sung payload BaseEventConfig cho Action để các hệ thống khác (UI) dễ bắt dữ liệu
        public event Action<EventData> OnEventStarted;

        // Chuyển thành BaseEventConfig để có thể chứa cả WaveAttack (EnemyBaseEventConfig) hoặc TowerDefense
        public EventData CurrentEventConfig { get; private set; }

        private CancellationTokenSource routineCts;

        public void Init()
        {
            currentTime.Value = 0;
            currentTime.Refresh();
            eventRaiseThTime = 1;

            GenerateNextEvent();
            StartRealtimeRoutine();
        }

        /// <summary>
        /// Apply for bind data (load)
        /// </summary>
        /// <param name="data"></param>
        private void Setup(TimeControllerSaveData data)
        {
            currentTime.Value = data.currentTime;
            currentTime.Refresh();
            eventRaiseThTime = data.eventRaiseThTime;
            nextRaisedEventTime = data.nextRaisedEventTime;

            var shouldChangeEvent = false;

            if (data.EventDataJson != null
                && data.EventDataJson.TryGetValue("ShouldChange", out var shouldChangeToken))
            {
                shouldChangeEvent = shouldChangeToken.Value<bool>();
            }

            // Khôi phục Event Config từ JSON
            // Validate RaisedEvent is format with current time (tránh trường hợp load game đã quá thời gian Raise Event đó rồi)
            if (!shouldChangeEvent && nextRaisedEventTime > currentTime.Value - 1f && data.EventDataJson != null)
            {
                CurrentEventConfig ??= new();
                CurrentEventConfig.BindData(data.EventDataJson);
                OnEventStarted?.Invoke(CurrentEventConfig);
            }
            else
            {
                GenerateNextEvent();
            }

            StartRealtimeRoutine();
        }

        private void GenerateNextEvent()
        {
            // SbTimeController giờ đây không cần biết cách tạo Event nữa, phó mặc hoàn toàn cho AI
            var (raisedTime, eventConfig) = SbGameplayEventAI.GenerateNextEvent(currentTime.Value, eventRaiseThTime);

            nextRaisedEventTime = raisedTime;
            CurrentEventConfig = eventConfig;
            eventRaiseThTime++;
            OnEventStarted?.Invoke(CurrentEventConfig);

            Debug.Log(
                $"[SbTimeController] Generated Next Event: {CurrentEventConfig.eventName} at Time {nextRaisedEventTime}");
        }

        private void StartRealtimeRoutine()
        {
            if (routineCts != null)
            {
                routineCts.Cancel();
                routineCts.Dispose();
            }

            routineCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
            RealtimeRoutine(routineCts.Token).Forget();
        }

        private async UniTask RealtimeRoutine(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                SbGameplayController.ApplyResourceIncrement();

                await UniTask.Delay(
                    TimeSpan.FromSeconds(TotalTimeUnit),
                    cancellationToken: token,
                    delayType: DelayType.DeltaTime
                );

                currentTime.Value += TotalTimeUnit;

                // Kiểm tra xem đã đến lúc Raise Event chưa
                if (CurrentEventConfig != null && currentTime.Value >= nextRaisedEventTime)
                {
                    EventInfoUIToolkit.Instance.Display(CurrentEventConfig, true);

                    // wait with deltaTime to stop scripts below, until time resume.
                    await UniTask.Delay(100, cancellationToken: token, delayType: DelayType.DeltaTime);

                    var cachedEventToLoad = CurrentEventConfig;
                    GameplayTransition.LoadBaseGameplayWithEvent(cachedEventToLoad).Forget();
                }
            }
        }

        // --- Save / Load System ---

        public TimeControllerSaveData SaveData()
        {
            return new TimeControllerSaveData()
            {
                currentTime = currentTime.Value,
                eventRaiseThTime = eventRaiseThTime,
                nextRaisedEventTime = nextRaisedEventTime,
                EventDataJson = CurrentEventConfig?.SaveData()
            };
        }

        public void BindData(TimeControllerSaveData data)
        {
            if (data != null) Setup(data);
        }
    }
}