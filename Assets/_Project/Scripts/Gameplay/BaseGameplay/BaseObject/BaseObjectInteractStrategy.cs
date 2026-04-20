using System;
using System.Threading;
using TnieYuPackage.CustomAttributes;
using TnieYuPackage.Utils;
using UnityEngine;

namespace Game.BaseGameplay
{
    public abstract class IBaseObjectInteractStrategyInstaller : ScriptableObject
    {
        [SerializeField] private SerializableGuid installerId = Guid.NewGuid();

        [LayerMaskDropdown] public int trackingLayerMask;
        public float interactRange;

        public abstract IBaseObjectInteractStrategy CreateInteractStrategy();

        public void InitInteract<TPreset>(BaseObjectRuntime<TPreset> runtime)
            where TPreset : BaseObjectPresetSo
        {
            var strategy = CreateInteractStrategy();

            strategy.OnInitBehaviour(runtime);
            runtime.InteractStrategies.TryAdd(installerId, strategy);
        }

        public void DestroyInteract<TPreset>(BaseObjectRuntime<TPreset> runtime)
            where TPreset : BaseObjectPresetSo
        {
            if (!runtime.InteractStrategies.TryGetValue(installerId, out var strategy)) return;

            strategy.OnDestroyBehaviour();
            runtime.InteractStrategies.Remove(installerId);
        }
    }

    public abstract class BaseObjectInteractStrategy<TInstaller> : IBaseObjectInteractStrategy<TInstaller>
        where TInstaller : IBaseObjectInteractStrategyInstaller
    {
        public TInstaller ActualInstaller { get; }
        public bool CanUse { get; protected set; }
        protected IBaseObjectRuntime OwnerRuntime { get; private set; }

        // Dùng để hủy ngang các UniTask/Tween khi Strategy bị destroy
        protected CancellationTokenSource cts;

        // MỚI: Strategy giữ reference tới Runtime sở hữu nó (Tower/Entity)

        public BaseObjectInteractStrategy(TInstaller installer)
        {
            ActualInstaller = installer;
        }

        public abstract bool TrackTarget(Vector3 position, out IObjectInteractable target);

        public abstract void Interact(IObjectInteractable interactable);

        // CẬP NHẬT: Thêm tham số IBaseObjectRuntime
        public virtual void OnInitBehaviour(IBaseObjectRuntime runtime)
        {
            OwnerRuntime = runtime;
            cts = new CancellationTokenSource();
        }

        public virtual void OnDestroyBehaviour()
        {
            // Hủy mọi Task và Tween đang chạy
            if (cts != null)
            {
                cts.Cancel();
                cts.Dispose();
                cts = null;
            }
        }
    }
}