using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using TnieYuPackage.DesignPatterns;
using TnieYuPackage.GlobalExtensions;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

// Yêu cầu Unity 2021.1+

namespace TnieYuPackage.UI
{
    public class ScreenTextDisplayController : SingletonBehavior<ScreenTextDisplayController>
    {
        [Header("Cấu hình Tham chiếu")] [Tooltip("Prefab chứa component Text (hoặc TextMeshProUGUI)")] [SerializeField]
        private Text textPrefab;

        [Tooltip("Transform chứa các Text (Thường là Canvas hoặc Panel)")] [SerializeField]
        private Transform textParent;

        [Header("Cấu hình Pool")] [SerializeField]
        private bool collectionChecks = true;

        [SerializeField] private int defaultCapacity = 20;
        [SerializeField] private int maxSize = 100;

        [Header("Cấu hình Hiển thị")] [SerializeField]
        private float displayDuration = 2f;

        [SerializeField] private float moveUpAmount = 1f;

        [Range(0, 24)] [SerializeField] private int persistentTextFontSize = 18;

        private ObjectPool<Text> _textPool;
        private Dictionary<Guid, Text> _persistentTexts = new Dictionary<Guid, Text>();

        protected override void InitializeSingleton()
        {
            base.InitializeSingleton();

            _textPool = new ObjectPool<Text>(
                createFunc: CreateTextElement,
                actionOnGet: OnGetTextElement,
                actionOnRelease: OnReleaseTextElement,
                actionOnDestroy: OnDestroyTextElement,
                collectionCheck: collectionChecks,
                defaultCapacity: defaultCapacity,
                maxSize: maxSize
            );
        }

        #region Object Pool Callbacks

        private Text CreateTextElement() => Instantiate(textPrefab, textParent);
        private void OnGetTextElement(Text textObj) => textObj.gameObject.SetActive(true);
        private void OnReleaseTextElement(Text textObj) => textObj.gameObject.SetActive(false);

        private void OnDestroyTextElement(Text textObj)
        {
            if (textObj != null) Destroy(textObj.gameObject);
        }

        #endregion

        public void DisplayText(string content, Vector3 worldPos, Color? textColor = null)
        {
            Text textComponent = _textPool.Get();

            textComponent.text = content;
            textComponent.color = textColor ?? Color.white;
            textComponent.transform.position = worldPos;

            ReleaseTextAsync(textComponent, displayDuration, this.GetCancellationTokenOnDestroy()).Forget();
        }

        private async UniTaskVoid ReleaseTextAsync(Text textComponent, float delay, CancellationToken cancellationToken)
        {
            Vector3 startPos = textComponent.transform.position;
            Vector3 endPos = startPos + (Vector3.up * moveUpAmount);

            // Tween di chuyển
            var moveMotion = LMotion.Create(startPos, endPos, delay)
                .WithEase(Ease.OutQuart)
                .Bind(pos =>
                {
                    if (textComponent != null) textComponent.transform.position = pos;
                });

            // Tween làm mờ 
            var fadeMotion = LMotion.Create(1f, 0f, delay)
                .WithEase(Ease.InQuad)
                .Bind(alpha =>
                {
                    if (textComponent != null) textComponent.color = textComponent.color.With(a: alpha);
                });

            // Await 1 tween là đủ do có chung duration
            bool isCanceled = await moveMotion.ToUniTask(cancellationToken).SuppressCancellationThrow();

            // Huỷ Motion nếu token ngắt giữa chừng
            if (isCanceled)
            {
                if (fadeMotion.IsActive()) fadeMotion.Cancel();
                return;
            }

            // Thu hồi object
            if (textComponent != null && textComponent.gameObject.activeSelf)
            {
                _textPool.Release(textComponent);
            }
        }

        // =========================================================
        // BỔ SUNG: XỬ LÝ TEXT CỐ ĐỊNH (PERSISTENT TEXT) THEO GUID
        // =========================================================

        public void SetPersistentText(Guid guid, string content, Vector3 worldPos, Color? textColor = null)
        {
            // Nếu chưa có Text ứng với Guid này, mượn 1 cái từ Pool và add vào Dictionary
            if (!_persistentTexts.TryGetValue(guid, out Text textComponent))
            {
                textComponent = _textPool.Get();
                _persistentTexts[guid] = textComponent;
                textComponent.fontSize = persistentTextFontSize;
            }

            textComponent.text = content;
            textComponent.color = textColor ?? Color.white;
            textComponent.transform.position = worldPos;
        }

        public void RemovePersistentText(Guid guid)
        {
            // Nếu tìm thấy Text ứng với Guid, thu hồi nó về Pool và xoá khỏi Dictionary
            if (_persistentTexts.TryGetValue(guid, out Text textComponent))
            {
                if (textComponent != null && textComponent.gameObject.activeSelf)
                {
                    _textPool.Release(textComponent);
                }

                _persistentTexts.Remove(guid);
            }
        }
    }
}