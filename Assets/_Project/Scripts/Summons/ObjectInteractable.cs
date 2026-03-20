using EditorAttributes;
using KBCore.Refs;
using UnityEngine;

namespace Game.Summons
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(CircleCollider2D))]
    public class ObjectInteractable : MonoBehaviour
    {
        [Required] public GameObject core;
        [Self] public CircleCollider2D circleCollider;

        void Awake()
        {
            circleCollider ??= GetComponent<CircleCollider2D>();
        }
        
        private void Reset()
        {
            if (!gameObject.TryGetComponent(out circleCollider)) return;
            circleCollider.isTrigger = true;
            circleCollider.radius = 0.5f;
        }
    }
}