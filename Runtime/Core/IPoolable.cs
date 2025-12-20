// ============================================================================
// PoolMaster - Object Pooling System for Unity
// Copyright (c) 2026 Max Thomas Coates
// https://github.com/mistyuk/PoolMaster
// Licensed under MIT License (see LICENSE file for details)
// ============================================================================

using UnityEngine;

namespace PoolMaster
{
    /// <summary>
    /// Defines lifecycle hooks for objects that can be pooled by the pooling system.
    /// Implementations can override only the methods they need for their specific pooling behavior.
    /// </summary>
    public interface IPoolable
    {
        /// <summary>
        /// Called when the object is spawned from the pool and activated.
        /// Invoked after positioning and SetActive(true), use for initialization and state setup.
        /// </summary>
        void OnSpawned();

        /// <summary>
        /// Called when the object is about to be returned to the pool.
        /// Invoked before deactivation, use for cleanup like stopping particles, sounds, or timers.
        /// </summary>
        void OnDespawned();

        /// <summary>
        /// Resets the object to its default state.
        /// Called during pooling operations to ensure clean state for reuse.
        /// </summary>
        void PoolReset();

        /// <summary>
        /// Gets whether this object is currently active in a pool.
        /// </summary>
        bool IsPooled { get; }

        /// <summary>
        /// Gets or sets the pool this object belongs to. Set automatically by the pooling system.
        /// </summary>
        IPool ParentPool { get; set; }
    }

    /// <summary>
    /// Defines essential pool operations and statistics for runtime pool management.
    /// Enables IPoolable objects to reference their parent pool without circular dependencies.
    /// </summary>
    public interface IPool
    {
        /// <summary>
        /// Returns an object to this pool for reuse. Alias for Despawn.
        /// </summary>
        /// <param name="obj">The GameObject to return to the pool.</param>
        void ReturnToPool(GameObject obj);

        /// <summary>
        /// Returns an object to this pool for reuse.
        /// </summary>
        /// <param name="instance">The GameObject to return to the pool.</param>
        /// <returns>True if successfully returned, false otherwise.</returns>
        bool Despawn(GameObject instance);

        /// <summary>
        /// Gets the prefab managed by this pool.
        /// </summary>
        GameObject Prefab { get; }

        /// <summary>
        /// Gets the unique identifier for this pool.
        /// </summary>
        string PoolId { get; }

        /// <summary>
        /// Gets the number of currently active (spawned) objects from this pool.
        /// </summary>
        int ActiveCount { get; }

        /// <summary>
        /// Gets the number of inactive (available) objects in this pool.
        /// </summary>
        int InactiveCount { get; }

        /// <summary>
        /// Gets the total capacity (active + inactive objects) of this pool.
        /// </summary>
        int Capacity { get; }
    }

    /// <summary>
    /// Internal pool operations not exposed through public API.
    /// Used for communication between PooledMarker and Pool for lifecycle management.
    /// </summary>
    internal interface IPoolInternal
    {
        /// <summary>
        /// Notifies the pool that an object was destroyed externally.
        /// Removes dead references to prevent poisoned pool counts.
        /// </summary>
        /// <param name="instance">The destroyed GameObject instance.</param>
        void NotifyObjectDestroyed(GameObject instance);
    }
}
