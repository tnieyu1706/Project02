using System;
using TnieYuPackage.Utils;
using UnityEngine;

namespace Game.StrategyBuilding
{
    public class MapInteractionHandler : MonoBehaviour
    {
        [SerializeField] private string blurBackgroundId;

        private static MapData MapDataSource => SbMapController.Instance.mapData;
        private BlurBackgroundComponent blurBackground;

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
        }

        private void OnBlurBackgroundOff()
        {
            this.enabled = true;
        }

        private void Update()
        {
            if (!Input.GetKey(KeyCode.LeftShift) || !Input.GetMouseButtonDown(0)) return;

            HandlePointerClick(Input.mousePosition);
        }

        //occur when scene click.
        private void HandlePointerClick(Vector2 screenPoint)
        {
            var worldPos = Registry<Camera>.GetFirst().ScreenToWorldPoint(screenPoint);

            var cellPos = (Vector2Int)SbMapController.Instance.GetCellFromWorld(worldPos);

            if (!MapDataSource.Validate(cellPos) || !MapDataSource.CheckExists(cellPos)) return;
            Debug.Log("HandlePointerClick: " + cellPos);

            //handle interact with tileData
            HandleInteraction(cellPos);
        }

        private void HandleInteraction(Vector2Int cellPos)
        {
            var tileData = MapDataSource.GetTileRef(cellPos);
            var exploitResult = ExploitEnvTile(tileData.EnvType);

            if (!exploitResult) return;
            MapDataSource.SetTile(cellPos, MapEnvType.None);
        }

        /// <summary>
        /// Exploit Env Tile and return result
        /// </summary>
        /// <param name="envType"></param>
        /// <returns>result env can be exploit</returns>
        private bool ExploitEnvTile(MapEnvType envType)
        {
            switch (envType)
            {
                case MapEnvType.None:
                    return false;
                case MapEnvType.Tree:
                    SbGameplayController.Instance.woodNumber.Value += 10; // setup
                    return true;
                case MapEnvType.Mountain:
                    SbGameplayController.Instance.stoneNumber.Value += 10; // setup
                    return true;
                case MapEnvType.Water:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}