// using UnityEngine;
// using UnityEngine.UI;
//
// namespace Game.BaseGameplay
// {
//     /// <summary>
//     /// Component gắn vào Prefab hiển thị 1 dòng thông tin (VD: [Icon Kiếm]: 10)
//     /// </summary>
//     public class BaseObjectInfoItemUI : MonoBehaviour
//     {
//         [SerializeField] private Image iconImage;
//         [SerializeField] private Text valueText;
//
//         public void Setup(Sprite icon, string value)
//         {
//             if (iconImage != null)
//             {
//                 iconImage.sprite = icon;
//                 iconImage.gameObject.SetActive(icon != null);
//             }
//
//             if (valueText != null)
//             {
//                 valueText.text = value;
//             }
//         }
//     }
// }