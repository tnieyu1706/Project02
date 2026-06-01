using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay.Global;
using Reflex.Attributes;
using TnieYuPackage.DesignPatterns;
using UnityEngine;

namespace Game.StorySystem
{
    /// <summary>
    /// CONTROLLER (BACKEND): Hoạt động hoàn toàn độc lập, không giữ bất kỳ Reference nào tới UI (View).
    /// Cung cấp Event cho View lắng nghe và cung cấp Command (Next/Skip) cho View gọi.
    /// </summary>
    public class StoryController : Singleton<StoryController>
    {
        [SerializeField] private StoryDataSo storyData;
        [Inject] private GameplayTransition transition;

        // --- EVENTS CHO UI HOOK VÀO ---
        public event Action<Sprite, string> OnPageChanged;
        public event Action OnStoryEnded;

        private CancellationTokenSource _delayCts;
        private int _currentPageIndex = 0;
        private bool _isStoryEnded = false;

        private void Start()
        {
            // Đã vào tới Scene này nghĩa là Start New Game, không cần check gì thêm, chạy thẳng Story luôn!
            PlayStoryLoop().Forget();
        }

        private async UniTaskVoid PlayStoryLoop()
        {
            _currentPageIndex = 0;

            while (_currentPageIndex < storyData.pages.Count && !_isStoryEnded)
            {
                var currentPage = storyData.pages[_currentPageIndex];
                
                // PHÁT EVENT CẬP NHẬT GIAO DIỆN (UI nào hook vào thì tự hiển thị)
                OnPageChanged?.Invoke(currentPage.image, currentPage.contentText);

                _delayCts?.Cancel();
                _delayCts?.Dispose();
                _delayCts = new CancellationTokenSource();

                // Chờ đợi. Nếu bị Cancel (do người dùng gọi SkipDelay), isCancelled sẽ = true
                bool isCancelled = await UniTask
                    .Delay(TimeSpan.FromSeconds(currentPage.duration), cancellationToken: _delayCts.Token)
                    .SuppressCancellationThrow();

                // Chuyển sang trang tiếp theo (dù hết giờ hay bị người chơi chủ động bấm qua)
                _currentPageIndex++;
            }

            if (!_isStoryEnded)
            {
                EndStoryAndTransition();
            }
        }

        // --- COMMANDS CHO UI GỌI ---

        /// <summary>
        /// Bỏ qua thời gian chờ của trang hiện tại để đi luôn tới trang tiếp theo.
        /// </summary>
        public void SkipCurrentPageDelay()
        {
            if (_isStoryEnded) return;
            // Việc Cancel Token sẽ ngắt UniTask.Delay lập tức, giúp vòng lặp chạy sang trang kế
            _delayCts?.Cancel(); 
        }

        /// <summary>
        /// Bỏ qua toàn bộ cốt truyện và vào game luôn.
        /// </summary>
        public void SkipEntireStory()
        {
            if (_isStoryEnded) return;
            EndStoryAndTransition();
        }

        private void EndStoryAndTransition()
        {
            if (_isStoryEnded) return;
            _isStoryEnded = true;

            OnStoryEnded?.Invoke(); // Báo cho View biết để tắt UI

            _delayCts?.Cancel();
            transition.LoadWorldMapGame().Forget(); // Chuyển map
        }

        protected void OnDestroy()
        {
            _delayCts?.Cancel();
            _delayCts?.Dispose();
        }
    }
}