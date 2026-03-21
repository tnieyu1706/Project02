using EditorAttributes;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.StrategyBuilding
{
    /// <summary>
    /// Implement IBuildingType.
    /// Only Spawn & Destroy by BuildingRuntime & contain data in SbMap.
    /// </summary>
    public class BuildingRuntime : MonoBehaviour, IBuildingTile
    {
        public BuildingPresetSo currentPreset;
        [SerializeReference] public IBuildingBehaviour buildingBehaviour;

        [SerializeField, Child] protected BuildingInteractable interactable;

        public Vector2Int TilePosition { get; set; }

        protected void OnEnable()
        {
            interactable.OnInteract += InteractSbObject;
        }

        protected void OnDisable()
        {
            interactable.OnInteract -= InteractSbObject;
        }

        private void InteractSbObject(PointerEventData eventData)
        {
            BuildingInfoUIToolkit.Instance.Display(this);
        }

        public void Setup(BuildingPresetSo preset, Vector2Int spawnPos)
        {
            TilePosition = spawnPos;

            SetPreset(preset);
        }

        private void SetPreset(BuildingPresetSo preset)
        {
            //pre-setup
            if (currentPreset != null && currentPreset.behaviourInstaller != null)
            {
                currentPreset.behaviourInstaller.Destroy(gameObject);
            }

            currentPreset = preset;

            //post-setup
            currentPreset.behaviourInstaller.Init(gameObject);
        }

        //test
        [Button]
        private void CheckConvenientEnv()
        {
            Debug.Log($"Count: {buildingBehaviour.ConvenientTiles.Count}");
        }
    }
}