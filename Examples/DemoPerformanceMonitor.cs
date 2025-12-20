// ============================================================================
// PoolMaster - Object Pooling System for Unity
// Copyright (c) 2026 Max Thomas Coates
// https://github.com/mistyuk/PoolMaster
// Licensed under MIT License (see LICENSE file for details)
// ============================================================================

using UnityEngine;

namespace PoolMaster.Examples
{
    /// <summary>
    /// Simple FPS counter and profiler display for demo performance monitoring
    /// </summary>
    public class DemoPerformanceMonitor : MonoBehaviour
    {
        private float deltaTime = 0.0f;
        private GUIStyle style;

        void Update()
        {
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        }

        void OnGUI()
        {
            if (style == null)
            {
                style = new GUIStyle();
                style.alignment = TextAnchor.UpperLeft;
                style.fontSize = 18;
                style.normal.textColor = Color.white;
            }

            int w = Screen.width,
                h = Screen.height;
            Rect rect = new Rect(10, 10, w, h * 2 / 100);

            float msec = deltaTime * 1000.0f;
            float fps = 1.0f / deltaTime;
            string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);

            // Color code based on FPS
            if (fps >= 60)
                style.normal.textColor = Color.green;
            else if (fps >= 30)
                style.normal.textColor = Color.yellow;
            else
                style.normal.textColor = Color.red;

            GUI.Label(rect, text, style);

            // Show active objects count
            rect.y += 25;
            style.fontSize = 14;
            style.normal.textColor = Color.white;
            int activeObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None).Length;
            GUI.Label(rect, $"Active GameObjects: {activeObjects}", style);
        }
    }
}
