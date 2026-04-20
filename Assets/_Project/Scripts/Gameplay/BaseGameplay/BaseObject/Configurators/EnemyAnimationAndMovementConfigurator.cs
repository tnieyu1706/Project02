using UnityEngine;

namespace Game.BaseGameplay.Configurators
{
    [CreateAssetMenu(fileName = "EnemyAnimationAndMovementConfigurator",
        menuName = "Game/BaseGameplay/Configurators/EnemyAnimationAndMovementConfigurator")]
    public class EnemyAnimationAndMovementConfigurator : BaseObjectConfigurator
    {
        public override void Configure(IBaseObjectRuntime runtime)
        {
            runtime.OnTrackOn += OnTrackOn;
            runtime.OnTrackOff += OnTrackOff;
            runtime.OnInteract += OnInteract;
        }

        public override void UnConfigure(IBaseObjectRuntime runtime)
        {
            runtime.OnTrackOn -= OnTrackOn;
            runtime.OnTrackOff -= OnTrackOff;
            runtime.OnInteract -= OnInteract;
        }

        private void OnTrackOn(IBaseObjectRuntime runtime)
        {
            if (runtime is EnemyRuntime enemyRuntime)
            {
                // play idle animation
                enemyRuntime.EntityAnimator.SetTrigger(BaseConstant.ENTITY_IDLE_TRIGGER);
                enemyRuntime.movementController.Pause();
            }
        }

        private void OnTrackOff(IBaseObjectRuntime runtime)
        {
            if (runtime is EnemyRuntime enemyRuntime)
            {
                //play walk animation
                enemyRuntime.EntityAnimator.SetTrigger(BaseConstant.ENTITY_MOVE_TRIGGER);
                enemyRuntime.movementController.Play();
            }
        }

        private void OnInteract(IBaseObjectRuntime owner, IObjectInteractable target)
        {
            if (owner is EnemyRuntime enemyRuntime)
            {
                //play attack animation
                enemyRuntime.EntityAnimator.SetTrigger(BaseConstant.ENTITY_ATTACK_TRIGGER);

                Vector2 dirVector = target.CurrentPosition - owner.CurrentPosition;
                enemyRuntime.SetFaceDir(dirVector.normalized);
            }
        }
    }
}