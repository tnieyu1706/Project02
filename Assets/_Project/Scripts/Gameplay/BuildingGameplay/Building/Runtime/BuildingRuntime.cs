using Game.BuildingGameplay;
using KBCore.Refs;
using UnityEngine;

namespace Game.StrategyBuilding
{
    /// <summary>
    /// Implement IBuildingType.
    /// Only Spawn & Destroy by BuildingRuntime & contain data in SbMap.
    /// </summary>
    public class BuildingRuntime : MonoBehaviour
    {
        public BuildingPresetSo currentPreset;
        [SerializeReference] public IBuildingBehaviour behaviour;
        [SerializeField, Child] protected BuildingInteractable interactable;

        private Vector2Int tilePosTemp;

        protected void OnEnable()
        {
            interactable.OnInteract += InteractSbObject;
        }

        private void InteractSbObject()
        {
            BuildingInfoUIToolkit.Instance.Display(this.behaviour);
        }

        protected void OnDisable()
        {
            interactable.OnInteract -= InteractSbObject;
        }

        public void Setup(BuildingPresetSo preset, Vector2Int spawnPos)
        {
            tilePosTemp = spawnPos;
            SetPreset(preset);
        }

        public void DestroyBuilding()
        {
            currentPreset.DestroyBehaviour(this);
        }

        private void SetPreset(BuildingPresetSo preset)
        {
            //pre-setup
            if (currentPreset != null)
            {
                currentPreset.DestroyBehaviour(this);
            }

            currentPreset = preset;

            //post-setup
            currentPreset.InitBehaviour(this, tilePosTemp);
        }
    }
}