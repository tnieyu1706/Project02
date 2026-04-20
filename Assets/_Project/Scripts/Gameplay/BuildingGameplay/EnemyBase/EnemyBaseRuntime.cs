using EditorAttributes;
using Game.BaseGameplay;
using KBCore.Refs;
using TnieYuPackage.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.BuildingGameplay
{
    // public class EnemyBaseRuntime : MonoBehaviour
    // {
    //     [SerializeField, ReadOnly] public BaseDataConfig baseConfig;
    //     [SerializeField, Child] private EnemyBaseScreenInteractable screenObjectInteract;
    //
    //     #region EVENTS
    //
    //     void OnEnable()
    //     {
    //         screenObjectInteract.OnInteract += HandleObjectInteractByScreen;
    //     }
    //
    //     private void HandleObjectInteractByScreen()
    //     {
    //         // display: enemy config panel
    //         EnemyBaseInfoUIToolkit.Instance.Display(this);
    //     }
    //
    //     void OnDisable()
    //     {
    //         screenObjectInteract.OnInteract -= HandleObjectInteractByScreen;
    //     }
    //
    //     #endregion
    //
    //     public void Setup(BaseDataConfig baseConfigSource)
    //     {
    //         baseConfig = baseConfigSource;
    //     }
    // }
    //
    // public class EnemyBaseScreenInteractable : CircleScreenObject2dInteractable, IPointerClickHandler
    // {
    //     public void OnPointerClick(PointerEventData eventData)
    //     {
    //         OnInteract?.Invoke();
    //     }
    // }
}