#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Game.Global.Editor
{
    [CustomEditor(typeof(KeyBindingController))]
    public class KeyBindingControllerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            // Vẽ giao diện mặc định nếu bạn có thêm biến SerializeField sau này vào Controller
            DrawDefaultInspector();

            KeyBindingController controller = (KeyBindingController)target;

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Runtime Key Bindings (View Only)", EditorStyles.boldLabel);

            // Chặn không hiển thị khi đang ở Editor Mode (chưa Play game)
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Hãy Play game để xem các phím đang được đăng ký.", MessageType.Info);
                return;
            }

            var bindings = controller.GetKeyBindingsForEditor();

            if (bindings.Count == 0)
            {
                EditorGUILayout.LabelField("Chưa có phím nào được đăng ký.");
                return;
            }

            // Vẽ Box hiển thị danh sách các phím
            EditorGUILayout.BeginVertical("box");
            EditorGUI.indentLevel++;
            foreach (var kvp in bindings)
            {
                // Lấy số lượng action đang được gán vào Key này
                int actionCount = kvp.Value != null ? kvp.Value.GetInvocationList().Length : 0;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"[{kvp.Key.ToString()}]", GUILayout.Width(150));
                EditorGUILayout.LabelField($"{actionCount} action(s) đăng ký");
                EditorGUILayout.EndHorizontal();
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();

            // Cập nhật Inspector liên tục để data hiển thị realtime khi bạn bật/tắt các UI Panel
            Repaint();
        }
    }
}
#endif