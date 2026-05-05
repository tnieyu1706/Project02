using System;
using UnityEngine;

namespace TnieYuPackage.Utils
{
    /// <summary>
    /// A Support class apply for object need scene interaction.
    /// Need implement IPointer... to perform action.
    /// </summary>
    public interface IScreenObject2dInteractable
    {
        Action OnInteract { get; set; }
        void Interact();
    }

    public abstract class BaseScreenObject2dInteractable : MonoBehaviour, IScreenObject2dInteractable
    {
        public Action OnInteract { get; set; }

        public virtual void Interact()
        {
            OnInteract?.Invoke();
        }
    }
    
    [ExecuteInEditMode]
    [RequireComponent(typeof(CircleCollider2D))]
    public abstract class CircleScreenObject2dInteractable : BaseScreenObject2dInteractable
    {
        private void Reset()
        {
            if (!gameObject.TryGetComponent(out CircleCollider2D circleCollider)) return;
            circleCollider.isTrigger = true;
            circleCollider.radius = 0.5f;
        }
    }
}