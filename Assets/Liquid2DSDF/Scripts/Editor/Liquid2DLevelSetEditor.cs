using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Liquid2D
{
    [CustomEditor(typeof(Liquid2DLevelSet))]
    public class Liquid2DLevelSetEditor : Editor
    {
        private SerializedProperty m_PropResourcesLiquid2D;
        private SerializedProperty m_PropResolutionX;
        private SerializedProperty m_PropResolutionY;
        private SerializedProperty m_PropTotalLengthX;
        private SerializedProperty m_PropTotalLengthY;
        private SerializedProperty m_PropSolverMode;
        private SerializedProperty m_PropSourceTexture;
        private SerializedProperty m_PropDebugMode;
        private SerializedProperty m_PropDebugScale;
        private SerializedProperty m_PropDilateTime;

        private string kPropResourcesLiquid2DStr = "resourcesLiquid2D";
        private string kPropResolutionXStr = "resolutionX";
        private string kPropResolutionYStr = "resolutionY";
        private string kPropTotalLengthXStr = "totalLengthX";
        private string kPropTotalLengthYStr = "totalLengthY";
        private string kPropSolverModeStr = "solverMode";
        private string kPropSourceTexture = "sourceTexture";
        private string kPropDebugMode = "debugMode";
        private string kPropDebugScale = "debugScale";
        private string kPropDilateTime = "dilateTime";

        private void OnInitProp()
        {
            m_PropResourcesLiquid2D = serializedObject.FindProperty(kPropResourcesLiquid2DStr);
            m_PropResolutionX = serializedObject.FindProperty(kPropResolutionXStr);
            m_PropResolutionY = serializedObject.FindProperty(kPropResolutionYStr);
            m_PropTotalLengthX = serializedObject.FindProperty(kPropTotalLengthXStr);
            m_PropTotalLengthY = serializedObject.FindProperty(kPropTotalLengthYStr);
            m_PropSolverMode = serializedObject.FindProperty(kPropSolverModeStr);
            m_PropSourceTexture = serializedObject.FindProperty(kPropSourceTexture);
            m_PropDebugMode = serializedObject.FindProperty(kPropDebugMode);
            m_PropDebugScale = serializedObject.FindProperty(kPropDebugScale);
            m_PropDilateTime = serializedObject.FindProperty(kPropDilateTime);

            serializedObject.Update();
        }

        public override void OnInspectorGUI()
        {
            Liquid2DLevelSet script = target as Liquid2DLevelSet;
            OnInitProp();

            EditorGUILayout.PropertyField(m_PropResourcesLiquid2D, new GUIContent(m_PropResourcesLiquid2D.displayName));
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_PropSolverMode, new GUIContent(m_PropSolverMode.displayName));
            EditorGUILayout.PropertyField(m_PropResolutionX, new GUIContent(m_PropResolutionX.displayName));
            EditorGUILayout.PropertyField(m_PropResolutionY, new GUIContent(m_PropResolutionY.displayName));
            EditorGUILayout.PropertyField(m_PropTotalLengthX, new GUIContent(m_PropTotalLengthX.displayName));
            EditorGUILayout.PropertyField(m_PropTotalLengthY, new GUIContent(m_PropTotalLengthY.displayName));
            EditorGUILayout.PropertyField(m_PropDilateTime, new GUIContent(m_PropDilateTime.displayName));
            EditorGUILayout.PropertyField(m_PropSourceTexture, new GUIContent(m_PropSourceTexture.displayName));
            if (EditorGUI.EndChangeCheck())
            {
                script.ResetSystem();
            }
            EditorGUILayout.PropertyField(m_PropDebugMode, new GUIContent(m_PropDebugMode.displayName));
            EditorGUILayout.PropertyField(m_PropDebugScale, new GUIContent(m_PropDebugScale.displayName));

            if (GUILayout.Button("Create Shader Res Asset"))
            {
                ScriptableObject res = ScriptableObject.CreateInstance("LevelSet2DRes");
                AssetDatabase.CreateAsset(res, "Assets/Liquid2DResources.asset");
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
