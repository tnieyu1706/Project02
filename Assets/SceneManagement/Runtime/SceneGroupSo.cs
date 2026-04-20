using UnityEngine;

namespace SceneManagement
{
    [CreateAssetMenu(fileName = "SceneGroupSo", menuName = "SceneManagement/SceneGroup")]
    public class SceneGroupSo : ScriptableObject
    {
        public SceneGroup sceneGroupData;
    }
}