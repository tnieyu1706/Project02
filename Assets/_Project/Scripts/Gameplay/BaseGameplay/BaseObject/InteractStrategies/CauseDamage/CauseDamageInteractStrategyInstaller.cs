using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TnieYuPackage.Utils;
using UnityEngine;

namespace Game.BaseGameplay.Strategies
{
    /// <summary>
    /// Abstract Installer cấu hình chung cho mọi loại gây sát thương
    /// </summary>
    public abstract class CauseDamageInteractStrategyInstaller : IBaseObjectInteractStrategyInstaller
    {
        [Header("Base Damage Settings")] public float baseDamage = 10f;
        public float attackCooldown = 1f;
    }

    /// <summary>
    /// Abstract Strategy quản lý luồng đánh, CanUse và UniTask cooldown
    /// </summary>
    public abstract class CauseDamageInteractStrategy<TInstaller> : BaseObjectInteractStrategy<TInstaller>
        where TInstaller : CauseDamageInteractStrategyInstaller
    {
        private bool isTracked = false;

        public CauseDamageInteractStrategy(TInstaller installer) : base(installer)
        {
        }

        public override void OnInitBehaviour(IBaseObjectRuntime runtime)
        {
            base.OnInitBehaviour(runtime);
            CanUse = true; // Khi init xong là sẵn sàng hoạt động
        }

        public override bool TrackTarget(Vector3 position, out IObjectInteractable target)
        {
            target = null;
            var collider =
                Physics2D.OverlapCircle(position, ActualInstaller.interactRange, ActualInstaller.trackingLayerMask);

            if (collider != null && collider.TryGetComponent(out Physics2dTrigger trigger))
            {
                target = trigger.core.GetComponent<IObjectInteractable>();
                if (!isTracked)
                {
                    OwnerRuntime.OnTrackOn?.Invoke(OwnerRuntime);
                    isTracked = true;
                }

                return true;
            }

            if (isTracked)
            {
                OwnerRuntime.OnTrackOff?.Invoke(OwnerRuntime);
                isTracked = false;
            }

            return false;
        }

        public override void Interact(IObjectInteractable interactable)
        {
            // Được gọi từ BaseGameplayInteractSystem (nếu System đã check CanUse thì ở đây double check cho an toàn)
            if (!CanUse || interactable == null || interactable.Hp.Value <= 0) return;

            Debug.Log("Interact Cause Damage Strategy");
            // Kích hoạt luồng đánh bằng UniTaskVoid để tách khỏi main thread update
            ExecuteAttackSequence(interactable).Forget();
        }

        /// <summary>
        /// Luồng tấn công bao gồm: Khóa CanUse -> Thực hiện đòn đánh -> Chờ Cooldown -> Mở khóa CanUse
        /// </summary>
        private async UniTaskVoid ExecuteAttackSequence(IObjectInteractable interactable)
        {
            CanUse = false;

            OwnerRuntime.OnInteract?.Invoke(OwnerRuntime, interactable);
            // Thực hiện hành động của loại vũ khí (Cận chiến thì trừ máu ngay, đánh xa thì bắn đạn bay đi)
            PerformAttackAction(interactable, cts.Token).Forget();

            // Chờ theo attackCooldown
            await UniTask.Delay(TimeSpan.FromSeconds(ActualInstaller.attackCooldown), cancellationToken: cts.Token);

            CanUse = true;
        }

        /// <summary>
        /// Child Class sẽ override hàm này để quyết định cách thức tấn công (đánh thẳng hay bắn đạn)
        /// </summary>
        protected abstract UniTask PerformAttackAction(IObjectInteractable target, CancellationToken token);

        /// <summary>
        /// Logic trừ máu và Defense dùng chung
        /// </summary>
        protected void ApplyDamage(IObjectInteractable target)
        {
            if (target == null || target.Hp.Value <= 0) return;

            float finalDamage = ActualInstaller.baseDamage;
            if (target is IEntityProperty entityProperty)
            {
                finalDamage = Mathf.Max(1f, finalDamage - entityProperty.Defense);
            }

            target.Hp.Value = Mathf.Max(0, target.Hp.Value - finalDamage);
            Debug.Log($"[{GetType().Name}] Gây {finalDamage} sát thương. Máu: {target.Hp.Value}");
        }
    }
}