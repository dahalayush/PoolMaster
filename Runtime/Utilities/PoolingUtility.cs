// ============================================================================
// PoolMaster - Object Pooling System for Unity
// Copyright (c) 2026 Max Thomas Coates
// https://github.com/mistyuk/PoolMaster
// Licensed under MIT License (see LICENSE file for details)
// ============================================================================

using System.Collections.Generic;
using UnityEngine;

namespace PoolMaster
{
    /// <summary>
    /// Utility class providing helper methods and extension methods for object pooling.
    /// </summary>
    public static class PoolingUtility
    {
        /// <summary>
        /// Caches pool requests for prefabs to avoid recreation.
        /// </summary>
        private static readonly Dictionary<GameObject, PoolRequest> cachedPoolRequests =
            new Dictionary<GameObject, PoolRequest>();

        #region GameObject Extensions

        /// <summary>
        /// Returns a GameObject to its pool using fast PooledMarker path when available.
        /// </summary>
        /// <param name="obj">The GameObject to return to its pool.</param>
        /// <returns>True if successfully returned, false if not pooled.</returns>
        public static bool ReturnToPool(this GameObject obj)
        {
            if (obj == null)
                return false;

            // FAST PATH: Check for PooledMarker first (direct access, no reflection needed)
            if (obj.TryGetComponent(out PooledMarker marker) && marker.ParentPool != null)
            {
                marker.ReturnToPool();
                return true;
            }

            // FALLBACK PATH: Look for IPoolable component (slower but more flexible)
            var poolable = obj.GetComponent<IPoolable>();
            if (poolable != null && poolable.ParentPool != null)
            {
                poolable.ParentPool.ReturnToPool(obj);
                return true;
            }

            // If no pooling components found, we can't determine the pool
            PoolLog.Warn(
                $"GameObject {obj.name} doesn't have PooledMarker or IPoolable component with ParentPool reference. Cannot return to pool."
            );
            return false;
        }

        /// <summary>
        /// Checks if a GameObject is currently pooled using fast PooledMarker path when available.
        /// </summary>
        /// <param name="obj">The GameObject to check.</param>
        /// <returns>True if currently spawned from a pool.</returns>
        public static bool IsPooled(this GameObject obj)
        {
            if (obj == null)
                return false;

            // FAST PATH: Check PooledMarker first (direct access, allocation-free)
            if (obj.TryGetComponent(out PooledMarker marker))
            {
                return marker.IsActiveInPool();
            }

            // FALLBACK PATH: Check IPoolable component
            var poolable = obj.GetComponent<IPoolable>();
            return poolable != null && poolable.IsPooled;
        }

        /// <summary>
        /// Gets the pool ID for a GameObject using fast PooledMarker path when available.
        /// </summary>
        /// <param name="obj">The GameObject to check.</param>
        /// <returns>Pool ID string, or null if not pooled.</returns>
        public static string GetPoolId(this GameObject obj)
        {
            if (obj == null)
                return null;

            // FAST PATH: Check PooledMarker first (direct access, allocation-free)
            if (obj.TryGetComponent(out PooledMarker marker))
            {
                return marker.GetPoolId();
            }

            // FALLBACK PATH: Check IPoolable component
            var poolable = obj.GetComponent<IPoolable>();
            return poolable?.ParentPool?.PoolId;
        }

        #endregion

        #region Pool Request Helpers

        /// <summary>
        /// Gets or creates a cached pool request for a prefab, avoiding repeated component analysis.
        /// </summary>
        /// <param name="prefab">The prefab to get a pool request for.</param>
        /// <param name="customizer">Optional function to customize the default pool request.</param>
        /// <returns>A PoolRequest for the prefab.</returns>
        public static PoolRequest GetOrCreatePoolRequest(
            GameObject prefab,
            System.Func<PoolRequest, PoolRequest> customizer = null
        )
        {
            if (prefab == null)
            {
                Debug.LogError("GetOrCreatePoolRequest: prefab cannot be null");
                return default;
            }

            // Check cache first
            if (cachedPoolRequests.TryGetValue(prefab, out PoolRequest cachedRequest))
            {
                return customizer != null ? customizer(cachedRequest) : cachedRequest;
            }

            // Create new request with intelligent defaults
            PoolRequest newRequest = CreateIntelligentPoolRequest(prefab);

            // Apply customization if provided
            if (customizer != null)
            {
                newRequest = customizer(newRequest);
            }

            // Cache for future use
            cachedPoolRequests[prefab] = newRequest;

            return newRequest;
        }

        /// <summary>
        /// Removes a cached pool request for a prefab to prevent memory leaks.
        /// </summary>
        /// <param name="prefab">The prefab to remove from the cache.</param>
        public static void RemoveCachedPoolRequest(GameObject prefab)
        {
            if (prefab != null)
            {
                cachedPoolRequests.Remove(prefab);
            }
        }

        /// <summary>
        /// Creates an intelligent pool request by analyzing the prefab components. Supports both 2D and 3D physics.
        /// </summary>
        /// <param name="prefab">The prefab to analyze.</param>
        /// <returns>A PoolRequest with intelligent defaults.</returns>
        public static PoolRequest CreateIntelligentPoolRequest(GameObject prefab)
        {
            if (prefab == null)
                return default;

            // Start with basic defaults
            var request = PoolRequest.Create(prefab);

            // Cache component lookups to avoid multiple GetComponent calls
            var rigidbody3D = prefab.GetComponent<Rigidbody>();
            var rigidbody2D = prefab.GetComponent<Rigidbody2D>();
            var collider3D = prefab.GetComponent<Collider>();
            var collider2D = prefab.GetComponent<Collider2D>();
            var particleSystem = prefab.GetComponentInChildren<ParticleSystem>();
            var audioSource = prefab.GetComponent<AudioSource>();
            var animator = prefab.GetComponent<Animator>();
            var animation = prefab.GetComponent<Animation>();

            // Analyze prefab to determine appropriate settings
            bool hasRigidbody = rigidbody3D != null || rigidbody2D != null;
            bool hasCollider = collider3D != null || collider2D != null;
            bool hasParticles = particleSystem != null;
            bool hasAudio = audioSource != null;
            bool hasAnimation = animator != null || animation != null;
            bool isProjectile = hasRigidbody && hasCollider;
            bool is2D = rigidbody2D != null || collider2D != null;

            // Determine category and settings based on components
            if (isProjectile)
            {
                // High-frequency projectiles (2D or 3D)
                request.category = is2D ? "Projectiles2D" : "Projectiles3D";
                request.initialPoolSize = 30;
                request.maxPoolSize = 100;
                request.initializationTiming = PoolInitializationTiming.OnAwake;
                request.allowDynamicExpansion = true;
                request.cullExcessObjects = true;
            }
            else if (hasParticles)
            {
                // Visual effects
                request.category = "Effects";
                request.initialPoolSize = 15;
                request.maxPoolSize = 50;
                request.initializationTiming = PoolInitializationTiming.Immediate;
                request.allowDynamicExpansion = true;
                request.cullExcessObjects = true;
            }
            else if (hasAudio)
            {
                // Audio objects
                request.category = "Audio";
                request.initialPoolSize = 10;
                request.maxPoolSize = 25;
                request.initializationTiming = PoolInitializationTiming.OnStart;
                request.allowDynamicExpansion = true;
                request.cullExcessObjects = false; // Keep audio objects around
            }
            else if (hasAnimation)
            {
                // Animated objects (possibly characters or interactive items)
                request.category = is2D ? "Animated2D" : "Animated3D";
                request.initialPoolSize = 5;
                request.maxPoolSize = 20;
                request.initializationTiming = PoolInitializationTiming.Lazy;
                request.allowDynamicExpansion = true;
                request.cullExcessObjects = false;
            }
            else
            {
                // Generic objects
                request.category = is2D ? "Generic2D" : "Generic3D";
                request.initialPoolSize = 10;
                request.maxPoolSize = 30;
                request.initializationTiming = PoolInitializationTiming.Lazy;
                request.allowDynamicExpansion = true;
                request.cullExcessObjects = true;
            }

            // Adjust container naming
            request.containerName = $"[{request.category}] {prefab.name} Pool";

            return request;
        }

        /// <summary>
        /// Clears the cached pool requests dictionary.
        /// Call during scene transitions or when unloading dynamically generated prefabs to prevent memory leaks.
        /// Recommended to call in SceneManager.sceneUnloaded or on major scene changes.
        /// </summary>
        public static void ClearPoolRequestCache()
        {
            cachedPoolRequests.Clear();
        }

        #endregion

        #region Validation Helpers

        /// <summary>
        /// Validates that a prefab is suitable for pooling by checking for common issues.
        /// Note: This is a tooling/editor method that allocates. Not safe to call every frame.
        /// </summary>
        /// <param name="prefab">The prefab to validate.</param>
        /// <returns>True if the prefab is suitable for pooling.</returns>
        public static bool ValidatePrefabForPooling(GameObject prefab)
        {
            if (prefab == null)
            {
                Debug.LogError("ValidatePrefabForPooling: prefab is null");
                return false;
            }

            List<string> warnings = new List<string>();
            List<string> errors = new List<string>();

            // Check if it's actually a prefab asset (editor-only check)
#if UNITY_EDITOR
            if (!UnityEditor.PrefabUtility.IsPartOfPrefabAsset(prefab))
            {
                errors.Add("Object is not a prefab asset. Only prefabs should be pooled.");
            }
#endif
            // Check for IPoolable interface
            var poolable = prefab.GetComponent<IPoolable>();
            if (poolable == null)
            {
                warnings.Add(
                    "Prefab doesn't implement IPoolable interface. Consider inheriting from PoolableMonoBehaviour for optimal pooling behavior."
                );
            }

            // Check for problematic singleton components
            // Note: Removed reflection-based checks for better performance
            // Users should manually verify singleton patterns are pool-compatible

            // Check for Canvas components (usually problematic for pooling)
            var canvases = prefab.GetComponentsInChildren<Canvas>();
            if (canvases.Length > 0)
            {
                warnings.Add(
                    "⚠ Potential issue: Prefab contains Canvas components. UI elements often have pooling issues due to layout rebuilding."
                );
            }

            // Check for Camera components (usually shouldn't be pooled)
            var cameras = prefab.GetComponentsInChildren<Camera>();
            if (cameras.Length > 0)
            {
                errors.Add(
                    "Prefab contains Camera components. Cameras should not be pooled as they have complex state and rendering implications."
                );
            }

            // Check for Light components (performance concern when pooled)
            var lights = prefab.GetComponentsInChildren<Light>();
            if (lights.Length > 0)
            {
                warnings.Add(
                    "⚠ Performance concern: Prefab contains Light components. Lights can impact performance when pooled frequently. Consider disabling lights in OnDespawned()."
                );
            }

            // Check for NetworkBehaviour components (usually problematic)
            // Note: Removed reflection-based checks for better performance
            // Users should manually verify network components are pool-compatible

            // Check for AudioListener (should never be pooled)
            var audioListeners = prefab.GetComponentsInChildren<AudioListener>();
            if (audioListeners.Length > 0)
            {
                errors.Add(
                    "Prefab contains AudioListener. Only one AudioListener should exist in a scene - this component should not be pooled."
                );
            }

            // Check for missing Rigidbody on objects with Colliders (potential physics issues)
            var collidersWithoutRB = prefab.GetComponentsInChildren<Collider>();
            var collidersWithoutRB2D = prefab.GetComponentsInChildren<Collider2D>();
            bool hasRigidbody =
                prefab.GetComponent<Rigidbody>() != null
                || prefab.GetComponent<Rigidbody2D>() != null;

            if ((collidersWithoutRB.Length > 0 || collidersWithoutRB2D.Length > 0) && !hasRigidbody)
            {
                warnings.Add(
                    "Prefab has Colliders but no Rigidbody. Consider adding a Rigidbody for better physics pooling behavior."
                );
            }

            // Log warnings and errors
            string prefabName = prefab.name;

            foreach (string warning in warnings)
            {
                Debug.LogWarning($"Pooling Validation Warning for {prefabName}: {warning}");
            }

            foreach (string error in errors)
            {
                Debug.LogError($"Pooling Validation Error for {prefabName}: {error}");
            }

            return errors.Count == 0;
        }

        #endregion

        #region Performance Helpers

        /// <summary>
        /// Calculates optimal pool size based on expected usage patterns.
        /// </summary>
        /// <param name="maxConcurrentObjects">Maximum number of objects active simultaneously.</param>
        /// <param name="averageLifetime">Average lifetime of each object in seconds.</param>
        /// <param name="spawnRate">Objects spawned per second.</param>
        /// <returns>Recommended initial pool size.</returns>
        public static int CalculateOptimalPoolSize(
            int maxConcurrentObjects,
            float averageLifetime,
            float spawnRate
        )
        {
            // Calculate based on throughput
            int throughputBasedSize = Mathf.CeilToInt(spawnRate * averageLifetime);

            // Take the maximum of concurrent objects and throughput-based size
            int optimalSize = Mathf.Max(maxConcurrentObjects, throughputBasedSize);

            // Add a buffer for spikes (25% extra)
            optimalSize = Mathf.CeilToInt(optimalSize * 1.25f);

            // Clamp to reasonable bounds
            return Mathf.Clamp(optimalSize, 5, 1000);
        }

        /// <summary>
        /// Estimates memory usage for a pool.
        /// </summary>
        /// <param name="prefab">The prefab that will be pooled.</param>
        /// <param name="poolSize">The size of the pool.</param>
        /// <returns>Estimated memory usage in bytes.</returns>
        public static long EstimatePoolMemoryUsage(GameObject prefab, int poolSize)
        {
            if (prefab == null)
                return 0;

            long estimatedSizePerObject = 0;

            // Estimate based on components (rough approximation)
            var meshRenderers = prefab.GetComponentsInChildren<MeshRenderer>();
            var meshFilters = prefab.GetComponentsInChildren<MeshFilter>();
            var colliders = prefab.GetComponentsInChildren<Collider>();
            var rigidbodies = prefab.GetComponentsInChildren<Rigidbody>();
            var particleSystems = prefab.GetComponentsInChildren<ParticleSystem>();

            // Base GameObject overhead
            estimatedSizePerObject += 200; // Rough estimate for GameObject + Transform

            // Mesh data (if not shared)
            foreach (var meshFilter in meshFilters)
            {
                if (meshFilter.sharedMesh != null)
                {
                    estimatedSizePerObject += 100; // Reference overhead (actual mesh is shared)
                }
            }

            // Component overhead
            estimatedSizePerObject += meshRenderers.Length * 150;
            estimatedSizePerObject += colliders.Length * 100;
            estimatedSizePerObject += rigidbodies.Length * 200;
            estimatedSizePerObject += particleSystems.Length * 300;

            return estimatedSizePerObject * poolSize;
        }

        #endregion
    }
}
