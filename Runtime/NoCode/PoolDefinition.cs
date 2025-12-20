// ============================================================================
// PoolMaster - Object Pooling System for Unity
// Copyright (c) 2026 Max Thomas Coates
// https://github.com/mistyuk/PoolMaster
// Licensed under MIT License (see LICENSE file for details)
// ============================================================================

using UnityEngine;
using System.Collections.Generic;

namespace PoolMaster.NoCode
{
    /// <summary>
    /// Defines a pool for a specific prefab. Add this to configure how many objects to create and manage.
    /// </summary>
    [System.Serializable]
    public class PoolDefinition
    {
        [Header("What to Pool")]
        [Tooltip("The prefab to create copies of. Drag your prefab here.")]
        public GameObject Prefab;

        [Header("Amounts")]
        [Tooltip("How many objects to create when the pool starts (0 = create on demand).")]
        [Range(0, 1000)]
        public int PrewarmAmount = 10;

        [Tooltip("Maximum number of objects to keep in the pool (0 = unlimited).")]
        [Range(0, 10000)]
        public int MaxSize = 100;

        [Header("Organization")]
        [Tooltip("Where to put pooled objects when they're not being used (optional).")]
        public Transform DefaultParent;

        [Header("When Pool is Full")]
        [Tooltip("What happens when you try to spawn but the pool is at Max Size.")]
        public ExhaustedBehavior OnExhausted = ExhaustedBehavior.Expand;

        // Internal state
        private Stack<GameObject> inactiveObjects = new Stack<GameObject>();
        private List<GameObject> activeObjects = new List<GameObject>();
        private Transform parent;
        private bool isInitialized;
        private bool showDebug;

        public int ActiveCount => activeObjects.Count;
        public int InactiveCount => inactiveObjects.Count;
        public int TotalCount => ActiveCount + InactiveCount;

        public void Initialize(Transform managerTransform, bool debugMode)
        {
            if (isInitialized) return;

            showDebug = debugMode;
            parent = DefaultParent != null ? DefaultParent : managerTransform;
            isInitialized = true;

            if (showDebug)
                Debug.Log($"[PoolMaster] Initialized pool for '{Prefab?.name}' (Prewarm: {PrewarmAmount}, Max: {MaxSize})");
        }

        public void Prewarm()
        {
            if (!isInitialized || Prefab == null) return;

            for (int i = 0; i < PrewarmAmount; i++)
            {
                CreateNewInstance();
            }

            if (showDebug && PrewarmAmount > 0)
                Debug.Log($"[PoolMaster] Prewarmed {PrewarmAmount} '{Prefab.name}' objects");
        }

        public GameObject Spawn(Vector3 position, Quaternion rotation)
        {
            if (!isInitialized || Prefab == null) return null;

            GameObject obj = null;

            // Try to get from inactive pool
            while (inactiveObjects.Count > 0)
            {
                obj = inactiveObjects.Pop();
                if (obj != null) break;
            }

            // Create new if needed
            if (obj == null)
            {
                // Check if we're at max size
                if (MaxSize > 0 && TotalCount >= MaxSize)
                {
                    return HandleExhausted();
                }

                obj = CreateNewInstance();
            }

            if (obj != null)
            {
                // Setup and activate
                obj.transform.position = position;
                obj.transform.rotation = rotation;
                obj.SetActive(true);
                activeObjects.Add(obj);

                if (showDebug)
                    Debug.Log($"[PoolMaster] Spawned '{Prefab.name}' at {position}");
            }

            return obj;
        }

        public void ReturnToPool(GameObject obj)
        {
            if (obj == null || !activeObjects.Remove(obj)) return;

            obj.SetActive(false);
            obj.transform.SetParent(parent);
            inactiveObjects.Push(obj);

            if (showDebug)
                Debug.Log($"[PoolMaster] Returned '{obj.name}' to pool");
        }

        public bool ContainsInstance(GameObject obj)
        {
            if (obj == null) return false;
            
            // Check active list
            for (int i = 0; i < activeObjects.Count; i++)
            {
                if (activeObjects[i] == obj) return true;
            }
            
            // Check inactive stack (rare case)
            return inactiveObjects.Contains(obj);
        }

        private GameObject CreateNewInstance()
        {
            if (Prefab == null) return null;

            GameObject obj = Object.Instantiate(Prefab, parent);
            obj.SetActive(false);
            obj.name = $"{Prefab.name} (Pooled)";

            // Ensure it has the pooled object marker
            if (obj.GetComponent<PoolMasterPooledObject>() == null)
            {
                obj.AddComponent<PoolMasterPooledObject>();
            }

            inactiveObjects.Push(obj);
            return obj;
        }

        private GameObject HandleExhausted()
        {
            switch (OnExhausted)
            {
                case ExhaustedBehavior.Expand:
                    // Allow expansion by returning null, which will trigger CreateNewInstance
                    return CreateNewInstance();

                case ExhaustedBehavior.ReuseOldest:
                    // Reuse first active object (oldest)
                    if (activeObjects.Count > 0 && activeObjects[0] != null)
                    {
                        var obj = activeObjects[0];
                        ReturnToPool(obj);
                        return obj;
                    }
                    break;

                case ExhaustedBehavior.ReturnNull:
                    if (showDebug)
                        Debug.LogWarning($"[PoolMaster] Pool '{Prefab.name}' exhausted. Returning null.");
                    return null;

                case ExhaustedBehavior.Warn:
                    Debug.LogWarning($"[PoolMaster] Pool '{Prefab.name}' is at Max Size ({MaxSize}). Consider increasing it or changing 'On Exhausted' behavior.");
                    return CreateNewInstance();
            }

            return null;
        }
    }

    /// <summary>
    /// What happens when the pool runs out of objects.
    /// </summary>
    public enum ExhaustedBehavior
    {
        [Tooltip("Create more objects beyond Max Size (recommended for most cases).")]
        Expand,

        [Tooltip("Take the oldest active object and reuse it (useful for bullet limits).")]
        ReuseOldest,

        [Tooltip("Don't spawn anything and return null (strict memory limit).")]
        ReturnNull,

        [Tooltip("Create more objects but show a warning (for debugging).")]
        Warn
    }
}
