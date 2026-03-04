using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Td
{
    public class ObjectInteractable : MonoBehaviour, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData)
        {
            PhysicsInteractController.Instance.TurnOff();

            //handle pointer click

            Debug.Log(
                $"ObjectInteractable.OnPointerClick at GameObject: [{eventData.pointerCurrentRaycast.gameObject.name}]");
        }
    }
}