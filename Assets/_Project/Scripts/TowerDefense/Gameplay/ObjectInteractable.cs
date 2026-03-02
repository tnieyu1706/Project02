using UnityEngine;
using UnityEngine.EventSystems;

namespace _Project.Scripts.TowerDefense.Gameplay
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