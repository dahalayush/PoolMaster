// ============================================================================
// PoolMaster - Object Pooling System for Unity
// Copyright (c) 2026 Max Thomas Coates
// https://github.com/mistyuk/PoolMaster
// Licensed under MIT License (see LICENSE file for details)
// ============================================================================

using UnityEngine;
using PoolMaster;
using System.Collections.Generic;

namespace PoolMaster.Examples
{
    /// <summary>
    /// Demonstrates CollectionPool for zero-allocation List, HashSet, and Dictionary rentals.
    /// Shows how to avoid GC pressure in performance-critical code paths.
    /// </summary>
    public class CollectionPoolingDemo : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private float operationInterval = 0.5f;
        [SerializeField] private int operationsPerCycle = 100;

        private float nextOperationTime;
        private int cycleCount;

        void Update()
        {
            if (Time.time >= nextOperationTime)
            {
                RunCollectionOperations();
                nextOperationTime = Time.time + operationInterval;
                cycleCount++;
            }
        }

        void RunCollectionOperations()
        {
            // Demonstrate List pooling
            DemoListPooling();

            // Demonstrate HashSet pooling
            DemoHashSetPooling();

            // Demonstrate Dictionary pooling
            DemoDictionaryPooling();
        }

        void DemoListPooling()
        {
            // Get pooled list - ZERO allocations
            var list = CollectionPool.GetList<int>();

            try
            {
                // Use the list
                for (int i = 0; i < operationsPerCycle; i++)
                {
                    list.Add(Random.Range(0, 100));
                }

                // Process data
                int sum = 0;
                foreach (var value in list)
                {
                    sum += value;
                }
            }
            finally
            {
                // Return to pool - automatically cleared
                CollectionPool.Return(list);
            }
        }

        void DemoHashSetPooling()
        {
            // Get pooled HashSet - ZERO allocations
            var hashSet = CollectionPool.GetHashSet<string>();

            try
            {
                // Use the HashSet for deduplication
                for (int i = 0; i < operationsPerCycle; i++)
                {
                    hashSet.Add($"Item_{Random.Range(0, 50)}");
                }

                // Process unique items
                int uniqueCount = hashSet.Count;
            }
            finally
            {
                // Return to pool - automatically cleared
                CollectionPool.Return(hashSet);
            }
        }

        void DemoDictionaryPooling()
        {
            // Get pooled Dictionary - ZERO allocations
            var dictionary = CollectionPool.GetDictionary<int, string>();

            try
            {
                // Use the Dictionary for lookups
                for (int i = 0; i < operationsPerCycle; i++)
                {
                    int key = Random.Range(0, 50);
                    dictionary[key] = $"Value_{key}";
                }

                // Process entries
                int entryCount = dictionary.Count;
            }
            finally
            {
                // Return to pool - automatically cleared
                CollectionPool.Return(dictionary);
            }
        }

        void OnGUI()
        {
            // Display collection pool stats
            var style = new GUIStyle(GUI.skin.label);
            style.fontSize = 14;
            style.normal.textColor = Color.white;

            var rect = new Rect(Screen.width - 420, 10, 400, 200);
            var bgRect = new Rect(rect.x - 10, rect.y - 10, rect.width + 20, rect.height + 20);
            
            GUI.color = new Color(0, 0, 0, 0.7f);
            GUI.DrawTexture(bgRect, Texture2D.whiteTexture);
            GUI.color = Color.white;

            var text = $"<b>Collection Pool Demo</b>\n\n";
            text += $"Cycles: {cycleCount}\n";
            text += $"Operations per cycle: {operationsPerCycle * 3}\n\n";
            text += $"<b>Pool Stats:</b>\n";
            text += $"Total Pooled: {CollectionPool.GetTotalPooledCount()}\n\n";
            text += $"<color=#00ff00>✓ Zero GC allocations</color>\n";
            text += $"<color=#00ff00>✓ Thread-safe rentals</color>\n";
            text += $"<color=#00ff00>✓ Auto-cleanup on return</color>";

            GUI.Label(rect, text, style);
        }
    }
}
