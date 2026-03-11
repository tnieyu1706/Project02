using KBCore.Refs;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Td
{
    public class TowerPhysicsInteractable : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField, Parent] private TowerRuntime towerRuntime;
        
        public void OnPointerClick(PointerEventData eventData)
        {
            PhysicsInteractController.Instance.TurnOff();

            TdTowerContextGUI.TurnOn();
            TdTowerContextGUI.Instance.DisplayTowerUpgradeElements(towerRuntime);
        }
    }
}