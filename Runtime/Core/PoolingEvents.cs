// ============================================================================
// PoolMaster - Object Pooling System for Unity
// Copyright (c) 2026 Max Thomas Coates
// https://github.com/mistyuk/PoolMaster
// Licensed under MIT License (see LICENSE file for details)
// ============================================================================

using System;
using UnityEngine;

namespace PoolMaster
{
    /// <summary>
    /// Static event bus for object pooling system communication.
    /// Enables loose coupling between pools, managers, and other systems for debugging, analytics, and memory management.
    /// </summary>
    public static class PoolingEvents
    {
        #region Pool Lifecycle Events

        /// <summary>
        /// Invoked when a new pool is created by the PoolingManager.
        /// </summary>
        public static event Action<string, GameObject> OnPoolCreated;

        /// <summary>
        /// Invoked when a pool is destroyed or cleared by the PoolingManager.
        /// </summary>
        public static event Action<string, GameObject> OnPoolDestroyed;

        /// <summary>
        /// Invoked when a pool is pre-warmed with initial objects.
        /// </summary>
        public static event Action<string, int> OnPoolPrewarmed;

        #endregion

        #region Object Lifecycle Events

#if ENABLE_POOL_LOGS
        /// <summary>
        /// Invoked when an object is spawned from a pool. Useful for analytics, debugging, and tracking active objects.
        /// Note: Only available when ENABLE_POOL_LOGS is defined (to avoid performance overhead).
        /// </summary>
        public static event Action<GameObject, string> OnObjectSpawned;

        /// <summary>
        /// Invoked when an object is returned to a pool. Useful for cleanup validation and memory tracking.
        /// Note: Only available when ENABLE_POOL_LOGS is defined (to avoid performance overhead).
        /// </summary>
        public static event Action<GameObject, string> OnObjectDespawned;
#endif

        /// <summary>
        /// Invoked when a pool creates a new object instance. Useful for memory profiling and performance analysis.
        /// </summary>
        public static event Action<GameObject, string> OnObjectCreated;

        #endregion

        #region Pool Performance Events

        /// <summary>
        /// Invoked when a pool expands beyond its initial size. May indicate undersized initial capacity.
        /// </summary>
        public static event Action<string, int, int> OnPoolExpanded; // poolId, oldSize, newSize

        /// <summary>
        /// Invoked when a pool culls excess objects to stay within memory limits. Useful for memory tracking.
        /// </summary>
        public static event Action<string, int> OnPoolCulled; // poolId, objectsCulled

        /// <summary>
        /// Invoked when a pool runs out of objects and cannot expand. Indicates potential misconfiguration.
        /// </summary>
        public static event Action<string, int> OnPoolExhausted; // poolId, maxSize
        #endregion

        #region Event Publishing Methods

        /// <summary>
        /// Publishes a pool created event. Internal use only.
        /// </summary>
        internal static void PublishPoolCreated(string poolId, GameObject prefab)
        {
            try
            {
                OnPoolCreated?.Invoke(poolId, prefab);
            }
            catch (Exception e)
            {
                Debug.LogError($"[Pooling] Error publishing PoolCreated event for {poolId}: {e}");
            }
        }

        /// <summary>
        /// Publishes a pool destroyed event. Internal use only.
        /// </summary>
        internal static void PublishPoolDestroyed(string poolId, GameObject prefab)
        {
            try
            {
                OnPoolDestroyed?.Invoke(poolId, prefab);
            }
            catch (Exception e)
            {
                PoolLog.Error($"Error publishing PoolDestroyed event for {poolId}: {e}");
            }
        }

        /// <summary>
        /// Publish a pool prewarmed event. Should only be called by pools.
        /// </summary>
        internal static void PublishPoolPrewarmed(string poolId, int objectCount)
        {
            try
            {
                OnPoolPrewarmed?.Invoke(poolId, objectCount);
            }
            catch (Exception e)
            {
                PoolLog.Error($"Error publishing PoolPrewarmed event for {poolId}: {e}");
            }
        }

        /// <summary>
        /// Publish an object spawned event. Should only be called by pools.
        /// Note: This can be very frequent - only enabled with ENABLE_POOL_LOGS.
        /// </summary>
        internal static void PublishObjectSpawned(GameObject obj, string poolId)
        {
            try
            {
#if ENABLE_POOL_LOGS
                OnObjectSpawned?.Invoke(obj, poolId);
#endif
            }
            catch (Exception e)
            {
                PoolLog.Error(
                    $"Error publishing ObjectSpawned event for {obj?.name} from {poolId}: {e}"
                );
            }
        }

        /// <summary>
        /// Publish an object despawned event. Should only be called by pools.
        /// Note: This can be very frequent - only enabled with ENABLE_POOL_LOGS.
        /// </summary>
        internal static void PublishObjectDespawned(GameObject obj, string poolId)
        {
            try
            {
#if ENABLE_POOL_LOGS
                OnObjectDespawned?.Invoke(obj, poolId);
#endif
            }
            catch (Exception e)
            {
                PoolLog.Error(
                    $"Error publishing ObjectDespawned event for {obj?.name} from {poolId}: {e}"
                );
            }
        }

        /// <summary>
        /// Publish an object created event. Should only be called by pools.
        /// </summary>
        internal static void PublishObjectCreated(GameObject obj, string poolId)
        {
            try
            {
                OnObjectCreated?.Invoke(obj, poolId);
            }
            catch (Exception e)
            {
                PoolLog.Error(
                    $"Error publishing ObjectCreated event for {obj?.name} in {poolId}: {e}"
                );
            }
        }

        /// <summary>
        /// Publish a pool expanded event. Should only be called by pools.
        /// </summary>
        internal static void PublishPoolExpanded(string poolId, int oldSize, int newSize)
        {
            try
            {
                OnPoolExpanded?.Invoke(poolId, oldSize, newSize);
            }
            catch (Exception e)
            {
                PoolLog.Error($"Error publishing PoolExpanded event for {poolId}: {e}");
            }
        }

        /// <summary>
        /// Publish a pool culled event. Should only be called by pools.
        /// </summary>
        internal static void PublishPoolCulled(string poolId, int objectsCulled)
        {
            try
            {
                OnPoolCulled?.Invoke(poolId, objectsCulled);
            }
            catch (Exception e)
            {
                PoolLog.Error($"Error publishing PoolCulled event for {poolId}: {e}");
            }
        }

        /// <summary>
        /// Publish a pool exhausted event. Should only be called by pools.
        /// </summary>
        internal static void PublishPoolExhausted(string poolId, int maxSize)
        {
            try
            {
                OnPoolExhausted?.Invoke(poolId, maxSize);
            }
            catch (Exception e)
            {
                PoolLog.Error($"Error publishing PoolExhausted event for {poolId}: {e}");
            }
        }

        #endregion

        #region Event Subscription Helpers

        /// <summary>
        /// Subscribe to all pooling events with a single debug logger.
        /// Useful for development and debugging.
        ///
        /// IMPORTANT: Call UnsubscribeFromAllEvents() during cleanup to prevent memory leaks.
        /// Recommended to call during OnDestroy, OnApplicationQuit, or domain reload events.
        /// </summary>
        /// <param name="enableVerboseLogging">If true, enables high-frequency spawn/despawn logging (only works with ENABLE_POOL_LOGS)</param>
        public static void SubscribeToAllEvents(bool enableVerboseLogging = false)
        {
            OnPoolCreated += (poolId, prefab) =>
                PoolLog.Info($"Pool Created: {poolId} for prefab {prefab.name}");
            OnPoolDestroyed += (poolId, prefab) =>
                PoolLog.Info($"Pool Destroyed: {poolId} for prefab {prefab.name}");
            OnPoolPrewarmed += (poolId, count) =>
                PoolLog.Info($"Pool Prewarmed: {poolId} with {count} objects");

            // High-frequency events only subscribed if verbose logging is enabled AND pool logs are enabled
            if (enableVerboseLogging)
            {
#if ENABLE_POOL_LOGS
                OnObjectSpawned += (obj, poolId) =>
                    PoolLog.Debug($"Object Spawned: {obj.name} from {poolId}");
                OnObjectDespawned += (obj, poolId) =>
                    PoolLog.Debug($"Object Despawned: {obj.name} to {poolId}");
#endif
            }

            OnObjectCreated += (obj, poolId) =>
                PoolLog.Info($"Object Created: {obj.name} in {poolId}");
            OnPoolExpanded += (poolId, oldSize, newSize) =>
                PoolLog.Warn($"Pool Expanded: {poolId} from {oldSize} to {newSize}");
            OnPoolCulled += (poolId, culled) =>
                PoolLog.Info($"Pool Culled: {poolId} removed {culled} excess objects");
            OnPoolExhausted += (poolId, maxSize) =>
                PoolLog.Error($"Pool Exhausted: {poolId} reached max size {maxSize}");
        }

        /// <summary>
        /// Unsubscribe from all pooling events to prevent memory leaks.
        ///
        /// CRITICAL: Always call this during cleanup to prevent memory leaks from event subscriptions.
        /// Recommended calling locations:
        /// - MonoBehaviour.OnDestroy()
        /// - Application shutdown (OnApplicationQuit)
        /// - Domain reload events in editor
        /// - Scene unload events
        ///
        /// This nulls all event delegates, which completely clears all subscriptions.
        /// </summary>
        public static void UnsubscribeFromAllEvents()
        {
            OnPoolCreated = null;
            OnPoolDestroyed = null;
            OnPoolPrewarmed = null;
#if ENABLE_POOL_LOGS
            OnObjectSpawned = null;
            OnObjectDespawned = null;
#endif
            OnObjectCreated = null;
            OnPoolExpanded = null;
            OnPoolCulled = null;
            OnPoolExhausted = null;

            PoolLog.Info("All pooling event subscriptions cleared");
        }

        #endregion
    }
}
