using UnityEditor;
using UnityEngine;

namespace PlayerMovement3D.Editor
{
    [CustomEditor(typeof(PlayerMovement))]
    public class PlayerMovementEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var playerMovement = (PlayerMovement)target;
            
            GUILayout.Label("Move Options", EditorStyles.boldLabel);
            playerMovement.SetMovementSpeed(EditorGUILayout.FloatField("Movement Speed", playerMovement.GetMovementSpeed()));
            GUILayout.Space(10);
            
            GUILayout.Label("Mouse Options", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            playerMovement.SetHorizontalSpeed(EditorGUILayout.Slider("Horizontal Speed", 
                playerMovement.GetHorizontalSpeed(), 0, 5));
            playerMovement.SetInvertHorizontal(EditorGUILayout.Toggle(playerMovement.GetInvertHorizontal()));
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            playerMovement.SetVerticalSpeed(EditorGUILayout.Slider("Vertical Speed", 
                playerMovement.GetVerticalSpeed(), 0, 5));
            playerMovement.SetInvertVertical(EditorGUILayout.Toggle(playerMovement.GetInvertVertical()));
            EditorGUILayout.EndHorizontal();
        }
    }
}
