using System;
using EditorAttributes;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.StrategyBuilding
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(BoxCollider2D))]
    public class BuildingInteractable : MonoBehaviour, IPointerClickHandler
    {
        [Self] public BoxCollider2D selfCollider;
        public Action OnInteract;

        void Awake()
        {
            selfCollider ??= GetComponent<BoxCollider2D>();
        }

        private void Reset()
        {
            if (!gameObject.TryGetComponent(out selfCollider)) return;

            selfCollider.isTrigger = true;
            selfCollider.size = Vector2.one;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnInteract?.Invoke();
        }
    }
}