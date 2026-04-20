using System.Collections.Generic;
using TnieYuPackage.DesignPatterns;
using TnieYuPackage.Utils;
using UnityEngine;

namespace Game.StrategyBuilding
{
    public class MapInteractionHandler : SingletonBehavior<MapInteractionHandler>
    {
        [SerializeField] private Sprite centerContextSprite;

        private Vector2Int preTilePos;
        private readonly Dictionary<Vector2Int, Sprite> contextSpritesTemp = new();

        #region Blur Background Handlers

        [SerializeField] private string blurBackgroundId;

        private BlurBackgroundComponent blurBackground;

        private Camera activeCamera;

        private void Start()
        {
            BlurBackgroundManager.Instance.BlurBackgrounds.TryGetValue(blurBackgroundId, out blurBackground);

            if (blurBackground == null)
            {
                Debug.LogError($"[BlurBackgroundManager] does not contain {blurBackgroundId}");
                return;
            }

            blurBackground.OnOpened += OnBlurBackgroundOn;
            blurBackground.OnClosed += OnBlurBackgroundOff;
            activeCamera = Registry<Camera>.GetFirst();
        }

        private void OnDestroy()
        {
            if (blurBackground == null) return;

            blurBackground.OnOpened -= OnBlurBackgroundOn;
            blurBackground.OnClosed -= OnBlurBackgroundOff;
        }

        private void OnBlurBackgroundOn()
        {
            this.enabled = false;
            TileContextDisplay.Instance.gameObject.SetActive(false);
        }

        private void OnBlurBackgroundOff()
        {
            this.enabled = true;
            TileContextDisplay.Instance.gameObject.SetActive(true);
        }

        #endregion

        private Dictionary<Vector2Int, Sprite> GetNeighborTilesContext(SbGridTileData tileData)
        {
            contextSpritesTemp.Clear();

            // handle: neighbor impactedBuildings
            foreach (var impacted in tileData.ImpactedBuildings)
            {
                contextSpritesTemp[impacted.Key] = SbGridMapDataController.Instance.impactedContextSprite;
            }

            // handle: neighbor BuildingRuntime.Influencers
            if (tileData.BuildingRuntime != null)
            {
                foreach (var influencer in tileData.BuildingRuntime.behaviour.TileInfluencers)
                {
                    contextSpritesTemp[influencer.Key] = SbGridMapDataController.Instance.influenceContextSprite;
                }
            }

            return contextSpritesTemp;
        }

        private void Update()
        {
            var worldPos = activeCamera.ScreenToWorldPoint(Input.mousePosition);
            var tilePos = (Vector2Int)SbGridMapSystem.Instance.gridTilemap.WorldToCell(worldPos);

            // display tile context when mouse move to another tile.
            if (tilePos != preTilePos)
            {
                if (SbGridMapSystem.Instance.GridMap.TryGetValue(tilePos, out var tileData))
                {
                    TileContextDisplay.Display(tilePos, SetupCenterContextDisplay, GetNeighborTilesContext(tileData));
                }
                else
                {
                    TileContextDisplay.Display(tilePos, SetupCenterContextDisplay);
                }

                preTilePos = tilePos;
            }

            if (!Input.GetKey(KeyCode.LeftShift) || !Input.GetMouseButtonDown(0)) return;

            HandlePointerClick(Input.mousePosition);
        }

        private void SetupCenterContextDisplay(SpriteRenderer centerContext)
        {
            centerContext.sprite = centerContextSprite;
            centerContext.color = Color.white;
        }

        //occur when scene click.
        private void HandlePointerClick(Vector2 screenPoint)
        {
            var worldPos = Registry<Camera>.GetFirst().ScreenToWorldPoint(screenPoint);
            var cellPos = (Vector2Int)SbGridMapSystem.Instance.gridTilemap.WorldToCell(worldPos);

            if (!SbGridMapSystem.Instance.GridMap.ContainsKey(cellPos)) return;
            HandleEnvInteract(cellPos);
        }

        private static void HandleEnvInteract(Vector2Int tilePos)
        {
            var tileData = SbGridMapSystem.Instance.ReadTile(tilePos);
            var exploitResult = tileData.TileLayer.HandleInteraction();

            if (!exploitResult) return;
            SbGridMapSystem.Instance.DeleteTile(tilePos);
        }
    }
}