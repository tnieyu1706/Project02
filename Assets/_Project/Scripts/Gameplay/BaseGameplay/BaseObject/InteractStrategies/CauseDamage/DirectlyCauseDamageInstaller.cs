using System.Threading;
using UnityEngine;
using Game.BaseGameplay;
using Cysharp.Threading.Tasks;

namespace Game.BaseGameplay.Strategies
{
    [CreateAssetMenu(fileName = "DirectlyCauseDamage_Strategy", menuName = "Gameplay/Interact Strategies/Directly Cause Damage (Melee)")]
    public class DirectlyCauseDamageInstaller : CauseDamageInteractStrategyInstaller
    {
        public override IBaseObjectInteractStrategy CreateInteractStrategy() => new DirectlyCauseDamageStrategy(this);
    }

    public class DirectlyCauseDamageStrategy : CauseDamageInteractStrategy<DirectlyCauseDamageInstaller>
    {
        public DirectlyCauseDamageStrategy(DirectlyCauseDamageInstaller installer) : base(installer) { }

        protected override async UniTask PerformAttackAction(IObjectInteractable target, CancellationToken token)
        {
            // Cận chiến: Áp dụng sát thương ngay lập tức
            ApplyDamage(target);
            
            // Hoàn thành Task ngay lập tức vì không cần chờ đạn bay
            await UniTask.CompletedTask;
        }
    }
}