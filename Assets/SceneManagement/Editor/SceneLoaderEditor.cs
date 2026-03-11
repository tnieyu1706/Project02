using UnityEditor;
using UnityEngine;

namespace SceneManagement
{
    [CustomEditor(typeof(SceneLoader))]
    public class SceneLoaderEditor : Editor
    {
        private SceneGroup testSceneGroup;

        public override void OnInspectorGUI()
        {
            // Draw default inspector
            DrawDefaultInspector();

            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField("Test Area", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            // SceneGroup field
            testSceneGroup = (SceneGroup)EditorGUILayout.ObjectField(
                "Scene Group",
                testSceneGroup,
                typeof(SceneGroup),
                false
            );

            EditorGUILayout.Space(5);

            GUI.enabled = Application.isPlaying && testSceneGroup != null;

            if (GUILayout.Button("Load Scene Group"))
            {
                SceneLoader loader = (SceneLoader)target;
                loader.Load(testSceneGroup);
            }

            GUI.enabled = true;

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox(
                    "Enter Play Mode to test scene loading.",
                    MessageType.Info
                );
            }
        }
    }
}