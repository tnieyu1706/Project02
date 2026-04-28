using TnieYuPackage.DesignPatterns;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Global.UI.WorldMap
{
    public class LevelMapSelectionHighLight : SingletonBehavior<LevelMapSelectionHighLight>
    {
        protected override void Awake()
        {
            base.Awake();
            Hide(); // Tắt highlight khi mới bắt đầu scene
        }

        /// <summary>
        /// Di chuyển tới vị trí của đối tượng mục tiêu và hiển thị Highlight
        /// </summary>
        /// <param name="pos">Transform của đối tượng đang được chọn/hover</param>
        public void Display(Vector3 pos)
        {
            transform.position = pos;

            gameObject.SetActive(true);
        }

        /// <summary>
        /// Ẩn Highlight đi
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}