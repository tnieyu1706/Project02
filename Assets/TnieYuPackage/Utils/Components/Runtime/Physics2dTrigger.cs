using EditorAttributes;
using UnityEngine;

namespace TnieYuPackage.Utils
{
    /// <summary>
    /// Apply for physics tracking and get core(mainstream) object.
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(BoxCollider2D))]
    public class Physics2dTrigger : MonoBehaviour
    {
        [Required] public GameObject core;

        public void SetLayer(string layerName)
        {
            gameObject.layer = LayerMask.NameToLayer(layerName);
        }

        private void Reset()
        {
            if (!gameObject.TryGetComponent(out BoxCollider2D boxCollider2D)) return;
            boxCollider2D.isTrigger = true;
            boxCollider2D.size = Vector2.one * 0.2f;
        }
    }
}