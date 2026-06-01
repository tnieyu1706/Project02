using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.StorySystem
{
    [Serializable]
    public class StoryPage
    {
        [Tooltip("Hình ảnh hiển thị cho trang này")]
        public Sprite image;
        
        [Tooltip("Nội dung câu chuyện")]
        [TextArea(3, 5)] 
        public string contentText;
        
        [Tooltip("Thời gian hiển thị (giây) trước khi tự động qua trang mới")]
        public float duration = 3f;
    }

    [CreateAssetMenu(fileName = "NewStoryData", menuName = "Game/Story/Story Data")]
    public class StoryDataSo : ScriptableObject
    {
        public List<StoryPage> pages = new List<StoryPage>();
    }
}