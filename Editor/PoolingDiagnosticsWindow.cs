// ============================================================================
// PoolMaster - Object Pooling System for Unity
// Copyright (c) 2026 Max Thomas Coates
// https://github.com/mistyuk/PoolMaster
// Licensed under MIT License (see LICENSE file for details)
// ============================================================================

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PoolMaster.Editor
{
    public class PoolingDiagnosticsWindow : EditorWindow
    {
        [MenuItem("Window/PoolMaster/Diagnostics", priority = 300)]
        public static void Open() => GetWindow<PoolingDiagnosticsWindow>("Pool Diagnostics");

        private Vector2 scroll;
        private bool autoRefresh = true;
        private float refreshInterval = 0.5f;
        private double lastRefreshTime;
        private PoolSnapshot lastSnapshot;

        // Search and filtering
        private string searchFilter = "";
        private bool showActiveOnly = false;
        private int sortMode = 0; // 0=Name, 1=Active Desc, 2=Utilization Desc, 3=Expansions Desc
        private readonly string[] sortOptions =
        {
            "Name",
            "Active (High)",
            "Utilization (High)",
            "Expansions (High)",
        };
        private List<KeyValuePair<string, PoolMetrics>> poolListCache =
            new List<KeyValuePair<string, PoolMetrics>>();

        void OnGUI()
        {
            // Header controls
            EditorGUILayout.BeginHorizontal();
            autoRefresh = EditorGUILayout.Toggle(
                new GUIContent("Auto Refresh", "Automatically refresh pool data"),
                autoRefresh
            );
            if (autoRefresh)
            {
                refreshInterval = EditorGUILayout.Slider(
                    new GUIContent("Interval", "Refresh interval in seconds"),
                    refreshInterval,
                    0.1f,
                    2f
                );
            }
            if (GUILayout.Button("Refresh Now", GUILayout.Width(100)))
            {
                RefreshData();
            }
            EditorGUILayout.EndHorizontal();

            // Search and filter controls
            EditorGUILayout.BeginHorizontal();
            searchFilter = EditorGUILayout.TextField(
                new GUIContent("Search", "Filter pools by name"),
                searchFilter
            );
            showActiveOnly = EditorGUILayout.Toggle(
                new GUIContent("Active Only", "Show only pools with active objects"),
                showActiveOnly,
                GUILayout.Width(100)
            );
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Sort By:", GUILayout.Width(60));
            sortMode = EditorGUILayout.Popup(sortMode, sortOptions, GUILayout.Width(150));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            if (Application.isPlaying && PoolingManager.Instance != null)
            {
                if (lastSnapshot.TotalPools > 0)
                {
                    DrawPoolingSummary();
                    EditorGUILayout.Space();
                    DrawPoolDetails();
                }
                else
                {
                    EditorGUILayout.HelpBox("No active pools found.", MessageType.Info);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Enter Play Mode to view active pools.", MessageType.Info);
            }
        }

        private void RefreshData()
        {
            if (Application.isPlaying && PoolingManager.Instance != null)
            {
                lastSnapshot = PoolingManager.Instance.GetSnapshot();
                lastRefreshTime = EditorApplication.timeSinceStartup;
                Repaint();
            }
        }

        private void DrawPoolingSummary()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Global Pool Statistics", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(
                $"Total Pools: {lastSnapshot.TotalPools}",
                GUILayout.Width(120)
            );
            EditorGUILayout.LabelField(
                $"Active Objects: {lastSnapshot.TotalActiveObjects}",
                GUILayout.Width(120)
            );
            EditorGUILayout.LabelField(
                $"Inactive Objects: {lastSnapshot.TotalInactiveObjects}",
                GUILayout.Width(120)
            );
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(
                new GUIContent(
                    $"Total Objects: {lastSnapshot.TotalObjects}",
                    "Total pooled instances across all pools"
                ),
                GUILayout.Width(140)
            );
            EditorGUILayout.LabelField(
                new GUIContent(
                    $"Utilization: {lastSnapshot.GlobalUtilization:F1}%",
                    "Percentage of objects currently active"
                ),
                GUILayout.Width(140)
            );
            if (
                GUILayout.Button(
                    new GUIContent("Unity Profiler", "Open Profiler for accurate memory analysis"),
                    GUILayout.Width(120)
                )
            )
            {
                EditorApplication.ExecuteMenuItem("Window/Analysis/Profiler");
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawPoolDetails()
        {
            EditorGUILayout.LabelField("Individual Pool Details", EditorStyles.boldLabel);

            // Null safety for edge cases
            if (lastSnapshot.PoolBreakdown == null)
                return;

            // Manual filtering to avoid LINQ allocations
            poolListCache.Clear();
            foreach (var kv in lastSnapshot.PoolBreakdown)
            {
                // Search filter (case-insensitive without allocating ToLower strings)
                if (!string.IsNullOrEmpty(searchFilter))
                {
                    if (kv.Key.IndexOf(searchFilter, StringComparison.OrdinalIgnoreCase) < 0)
                        continue;
                }

                // Active only filter
                if (showActiveOnly && kv.Value.CurrentActive == 0)
                    continue;

                poolListCache.Add(kv);
            }

            // Manual sorting to avoid LINQ allocations (with stable secondary sort by name)
            switch (sortMode)
            {
                case 1: // Active (High)
                    poolListCache.Sort(
                        (a, b) =>
                        {
                            int result = b.Value.CurrentActive.CompareTo(a.Value.CurrentActive);
                            return result != 0
                                ? result
                                : string.Compare(a.Key, b.Key, StringComparison.Ordinal);
                        }
                    );
                    break;
                case 2: // Utilization (High)
                    poolListCache.Sort(
                        (a, b) =>
                        {
                            float utilA =
                                a.Value.TotalCreated > 0
                                    ? (float)a.Value.CurrentActive / a.Value.TotalCreated
                                    : 0;
                            float utilB =
                                b.Value.TotalCreated > 0
                                    ? (float)b.Value.CurrentActive / b.Value.TotalCreated
                                    : 0;
                            int result = utilB.CompareTo(utilA);
                            return result != 0
                                ? result
                                : string.Compare(a.Key, b.Key, StringComparison.Ordinal);
                        }
                    );
                    break;
                case 3: // Expansions (High)
                    poolListCache.Sort(
                        (a, b) =>
                        {
                            int result = b.Value.ExpansionCount.CompareTo(a.Value.ExpansionCount);
                            return result != 0
                                ? result
                                : string.Compare(a.Key, b.Key, StringComparison.Ordinal);
                        }
                    );
                    break;
                default: // Name
                    poolListCache.Sort(
                        (a, b) => string.Compare(a.Key, b.Key, StringComparison.Ordinal)
                    );
                    break;
            }

            if (poolListCache.Count == 0)
            {
                EditorGUILayout.HelpBox("No pools match the current filter.", MessageType.Info);
                return;
            }

            scroll = EditorGUILayout.BeginScrollView(scroll);

            foreach (var kv in poolListCache)
            {
                EditorGUILayout.BeginVertical("box");

                // Pool name header with colored indicator
                EditorGUILayout.BeginHorizontal();
                var indicatorColor =
                    kv.Value.CurrentActive > 0 ? "<color=green>●</color>" : "<color=gray>○</color>";
                EditorGUILayout.LabelField(
                    $"{indicatorColor} 🎯 {kv.Key}",
                    new GUIStyle(EditorStyles.boldLabel) { richText = true }
                );
                EditorGUILayout.EndHorizontal();

                var m = kv.Value;

                // Core stats with tooltips
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(
                    new GUIContent($"Active: {m.CurrentActive}", "Currently spawned objects"),
                    GUILayout.Width(80)
                );
                EditorGUILayout.LabelField(
                    new GUIContent(
                        $"Spawned: {m.TotalSpawned}",
                        "Total times objects were spawned"
                    ),
                    GUILayout.Width(90)
                );
                EditorGUILayout.LabelField(
                    new GUIContent($"Created: {m.TotalCreated}", "Total objects instantiated"),
                    GUILayout.Width(80)
                );
                EditorGUILayout.LabelField(
                    new GUIContent(
                        $"Reuse: {m.ReuseEfficiency:P0}",
                        "Percentage of spawns that reused existing objects"
                    ),
                    GUILayout.Width(90)
                );
                EditorGUILayout.EndHorizontal();

                // Performance stats
                if (m.ExpansionCount > 0 || m.CullCount > 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(
                        $"Expansions: {m.ExpansionCount}",
                        GUILayout.Width(100)
                    );
                    EditorGUILayout.LabelField($"Culls: {m.CullCount}", GUILayout.Width(80));

                    if (m.AverageExpansionInterval > 0)
                    {
                        EditorGUILayout.LabelField(
                            $"Avg Expand: {m.AverageExpansionInterval:F1}s",
                            GUILayout.Width(100)
                        );
                    }
                    EditorGUILayout.EndHorizontal();
                }

                // Progress bar for pool utilization with tooltip icon
                if (m.TotalCreated > 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    float utilization = (float)m.CurrentActive / m.TotalCreated;
                    var rect = GUILayoutUtility.GetRect(0, 16, GUILayout.ExpandWidth(true));
                    EditorGUI.ProgressBar(
                        rect,
                        utilization,
                        $"Utilization: {utilization:P0} (Active/Total Created)"
                    );
                    GUILayout.Label(
                        new GUIContent(
                            "ⓘ",
                            "Shows how 'hot' this pool is - higher means more objects are actively in use"
                        ),
                        GUILayout.Width(20)
                    );
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(2);
            }

            EditorGUILayout.EndScrollView();
        }

        void OnEnable()
        {
            EditorApplication.update += OnEditorUpdate;
            // Refresh immediately if opening window while already in play mode
            if (Application.isPlaying && PoolingManager.Instance != null)
            {
                RefreshData();
            }
        }

        void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
        }

        void OnEditorUpdate()
        {
            // Throttled refresh check outside OnGUI
            if (autoRefresh && Application.isPlaying && PoolingManager.Instance != null)
            {
                if (EditorApplication.timeSinceStartup - lastRefreshTime > refreshInterval)
                {
                    RefreshData();
                }
            }
        }
    }
}
#endif
