// ============================================================================
// PoolMaster - Object Pooling System for Unity
// Copyright (c) 2026 Max Thomas Coates
// https://github.com/mistyuk/PoolMaster
// Licensed under MIT License (see LICENSE file for details)
// ============================================================================

using PoolMaster;
using UnityEngine;
using UnityEngine.UI;

namespace PoolMaster.Examples
{
    /// <summary>
    /// Displays real-time pool metrics on a UI canvas. Updates every frame to show pool performance.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public class PoolMetricsDisplay : MonoBehaviour
    {
        [Header("UI Configuration")]
        [SerializeField]
        private Text metricsText;

        [SerializeField]
        private float updateInterval = 0.1f;

        private float lastUpdateTime;

        void Start()
        {
            // Disabled - integrated into DemoNavigator UI
            enabled = false;
        }

        void Update()
        {
            if (Time.time - lastUpdateTime < updateInterval)
                return;

            lastUpdateTime = Time.time;
            UpdateMetricsDisplay();
        }

        void CreateUIText()
        {
            // Create background panel
            var panel = new GameObject("MetricsPanel");
            panel.transform.SetParent(transform, false);

            var rectTransform = panel.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 1);
            rectTransform.anchoredPosition = new Vector2(10, -10);
            rectTransform.sizeDelta = new Vector2(400, 300);

            var image = panel.AddComponent<Image>();
            image.color = new Color(0, 0, 0, 0.7f);

            // Create text
            var textObj = new GameObject("MetricsText");
            textObj.transform.SetParent(panel.transform, false);

            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 10);
            textRect.offsetMax = new Vector2(-10, -10);

            metricsText = textObj.AddComponent<Text>();
            metricsText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            metricsText.fontSize = 12;
            metricsText.color = Color.white;
            metricsText.alignment = TextAnchor.UpperLeft;
        }

        void UpdateMetricsDisplay()
        {
            if (PoolingManager.Instance == null || metricsText == null)
                return;

            var snapshot = PoolingManager.Instance.GetSnapshot();
            var sb = new System.Text.StringBuilder();

            // Header
            sb.AppendLine("<b>=== PoolMaster Metrics ===</b>");
            sb.AppendLine($"Active Pools: {snapshot.PoolBreakdown.Count}");
            sb.AppendLine($"Total Objects: {snapshot.TotalObjects}");
            sb.AppendLine(
                $"Active: {snapshot.TotalActiveObjects} | Inactive: {snapshot.TotalInactiveObjects}"
            );
            sb.AppendLine($"Global Utilization: {snapshot.GlobalUtilization:F1}%");
            sb.AppendLine();

            // Per-pool breakdown
            sb.AppendLine("<b>Pool Breakdown:</b>");
            foreach (var kvp in snapshot.PoolBreakdown)
            {
                var poolId = kvp.Key;
                var metrics = kvp.Value;

                var active = metrics.CurrentActive;
                var totalObjects = metrics.TotalCreated;
                var efficiency = metrics.ReuseEfficiency * 100f;

                sb.AppendLine($"• {poolId}");
                sb.AppendLine($"  Active: {active}/{totalObjects} | Reuse: {efficiency:F0}%");
            }

            metricsText.text = sb.ToString();
        }
    }
}
