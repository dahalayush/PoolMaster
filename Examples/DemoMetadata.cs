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
    /// Metadata for each demo showcasing specific pooling use-cases
    /// </summary>
    [System.Serializable]
    public class DemoMetadata
    {
        public string title;
        [TextArea(2, 4)]
        public string description;
        [TextArea(3, 6)]
        public string features;
        public MonoBehaviour demoComponent;
        public Color accentColor = Color.cyan;
    }
}
