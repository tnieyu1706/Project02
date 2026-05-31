using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.StorySystem
{
    /// <summary>
    /// VIEW (UI): Hoàn toàn thụ động. Hook vào Event của Controller để hiển thị.
    /// Truyền tương tác (Click) của người dùng ngược lại cho Controller dưới dạng Command.
    /// </summary>
    public class StoryGUI : MonoBehaviour
    {
        [Header("UI References")] [SerializeField]
        private Image displayImage;

        [SerializeField] private Text displayText;

        [Header("Buttons")] [SerializeField] private Button backgroundClickButton;
        [SerializeField] private Button skipStoryButton;

        private void OnEnable()
        {
            // Subscribe Event từ Backend (Controller)
            if (StoryController.HasInstance)
            {
                StoryController.Instance.OnPageChanged += HandlePageChanged;
                StoryController.Instance.OnStoryEnded += HandleStoryEnded;
            }
            else
            {
                Debug.LogWarning("[StoryGUI] Không tìm thấy StoryController trong Scene!");
            }

            // Đăng ký tương tác nút bấm
            backgroundClickButton.onClick.AddListener(OnBackgroundClicked);
            skipStoryButton.onClick.AddListener(OnSkipClicked);
        }

        private void HandlePageChanged(Sprite image, string text)
        {
            if (image != null)
            {
                displayImage.sprite = image;
            }

            displayText.text = text;
        }

        private void HandleStoryEnded()
        {
            // Có thể ẩn UI hoặc chạy Animation Fade Out tuỳ thiết kế
            gameObject.SetActive(false);
        }

        private void OnBackgroundClicked()
        {
            // Gọi Command của Controller
            StoryController.Instance?.SkipCurrentPageDelay();
        }

        private void OnSkipClicked()
        {
            // Gọi Command của Controller
            StoryController.Instance?.SkipEntireStory();
        }

        private void OnDisable()
        {
            // Unsubscribe
            if (StoryController.HasInstance)
            {
                StoryController.Instance.OnPageChanged -= HandlePageChanged;
                StoryController.Instance.OnStoryEnded -= HandleStoryEnded;
            }

            backgroundClickButton.onClick.RemoveAllListeners();
            skipStoryButton.onClick.RemoveAllListeners();
        }
    }
}