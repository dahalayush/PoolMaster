// ============================================================================
// PoolMaster - Object Pooling System for Unity
// Copyright (c) 2026 Max Thomas Coates
// https://github.com/mistyuk/PoolMaster
// Licensed under MIT License (see LICENSE file for details)
// ============================================================================

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace PoolMaster.NoCode.Editor
{
    [CustomEditor(typeof(PoolMasterManager))]
    public class PoolMasterManagerEditor : UnityEditor.Editor
    {
        private SerializedProperty poolsProperty;
        private SerializedProperty prewarmOnStartProperty;
        private SerializedProperty showWarningsProperty;
        private SerializedProperty showDebugInfoProperty;
        private double lastUpdateTime;

        void OnEnable()
        {
            poolsProperty = serializedObject.FindProperty("pools");
            prewarmOnStartProperty = serializedObject.FindProperty("prewarmOnStart");
            showWarningsProperty = serializedObject.FindProperty("showWarnings");
            showDebugInfoProperty = serializedObject.FindProperty("showDebugInfo");
            
            // Auto-refresh runtime stats
            EditorApplication.update += OnEditorUpdate;
        }

        void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
        }

        private void OnEditorUpdate()
        {
            if (Application.isPlaying && EditorApplication.timeSinceStartup - lastUpdateTime > 0.5)
            {
                lastUpdateTime = EditorApplication.timeSinceStartup;
                Repaint();
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var manager = (PoolMasterManager)target;
            if (manager == null) return;

            // Consistent label width
            float originalLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 150f;

            // Header
            EditorGUILayout.HelpBox(
                "Quick Start: Add pools → Drag prefabs → Set Prewarm → Press Play",
                MessageType.Info
            );

            EditorGUILayout.Space(5);

            // Pools section
            EditorGUILayout.LabelField("Pools", EditorStyles.boldLabel);
            
            if (poolsProperty.arraySize == 0)
            {
                EditorGUILayout.HelpBox("No pools configured. Click 'Add New Pool' below.", MessageType.Warning);
            }
            
            EditorGUILayout.PropertyField(poolsProperty, true);

            if (GUILayout.Button("Add New Pool", GUILayout.Height(30)))
            {
                poolsProperty.arraySize++;
                serializedObject.ApplyModifiedProperties();
            }

            EditorGUILayout.Space(10);

            // Performance section
            EditorGUILayout.LabelField("Performance", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(prewarmOnStartProperty);

            EditorGUILayout.Space(10);

            // Debug section
            EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(showWarningsProperty);
            EditorGUILayout.PropertyField(showDebugInfoProperty);

            // Runtime stats (auto-refreshes)
            if (Application.isPlaying)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Runtime Statistics", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox(manager.GetStatsOverview(), MessageType.None);
            }

            EditorGUIUtility.labelWidth = originalLabelWidth;
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
