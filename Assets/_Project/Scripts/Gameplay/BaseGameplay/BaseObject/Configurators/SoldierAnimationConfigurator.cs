using UnityEngine;

namespace Game.BaseGameplay.Configurators
{
    [CreateAssetMenu(fileName = "SoldierAnimationConfigurator",
        menuName = "Game/BaseGameplay/Configurators/SoldierAnimationConfigurator")]
    public class SoldierAnimationConfigurator : BaseObjectConfigurator
    {
        public override void Configure(IBaseObjectRuntime runtime)
        {
            runtime.OnTrackOff += OnTrackOff;
            runtime.OnInteract += OnInteract;
        }

        public override void UnConfigure(IBaseObjectRuntime runtime)
        {
            runtime.OnTrackOff -= OnTrackOff;
            runtime.OnInteract -= OnInteract;
        }

        private void OnTrackOff(IBaseObjectRuntime runtime)
        {
            if (runtime is IEntityRuntime entityRuntime)
            {
                //play idle animation
                entityRuntime.EntityAnimator.SetTrigger(BaseConstant.ENTITY_DIED_TRIGGER);
            }
        }

        private void OnInteract(IBaseObjectRuntime owner, IObjectInteractable target)
        {
            if (owner is IEntityRuntime entityRuntime)
            {
                //play attack animation
                entityRuntime.EntityAnimator.SetTrigger(BaseConstant.ENTITY_ATTACK_TRIGGER);

                Vector2 dirVector = target.CurrentPosition - owner.CurrentPosition;
                entityRuntime.SetFaceDir(dirVector.normalized);
            }
        }
    }
}