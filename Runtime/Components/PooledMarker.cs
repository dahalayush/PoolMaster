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
    /// Lightweight marker component providing direct access to an object's pool for performance optimization.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PooledMarker : MonoBehaviour
    {
        /// <summary>
        /// Gets or sets the pool that owns this object. Set by the pool when created.
        /// </summary>
        [System.NonSerialized]
        public IPool ParentPool;

        /// <summary>
        /// Tracks active spawn state from the pool. Provides O(1) checks without relying on activeInHierarchy.
        /// </summary>
        [System.NonSerialized]
        internal bool IsSpawnedFromPool;

        /// <summary>
        /// Cached reference to the IPoolable component for zero-allocation lookups.
        /// Avoids Dictionary overhead in the pool. Set once at creation time.
        /// </summary>
        [System.NonSerialized]
        internal Component CachedPoolableComponent;

        /// <summary>
        /// Returns this object to its pool using a fast path that bypasses GetComponent calls.
        /// </summary>
        public void ReturnToPool()
        {
            if (ParentPool != null)
            {
                ParentPool.ReturnToPool(gameObject);
            }
            else
            {
#if UNITY_EDITOR
                if (!gameObject.ReturnToPool())
                    Debug.LogWarning(
                        $"PooledMarker on {name} missing ParentPool and no pool found via extensions."
                    );
#else
                // In non-Editor builds, soft-fail without logging to avoid spam
                gameObject.SetActive(false);
#endif
            }
        }

        /// <summary>
        /// Get the pool ID without additional component lookups.
        /// </summary>
        public string GetPoolId()
        {
            return ParentPool?.PoolId;
        }

        /// <summary>
        /// Check if this object is currently active in a pool.
        /// Uses O(1) internal state tracking rather than activeInHierarchy checks,
        /// providing unambiguous and cheaper pool state validation.
        /// </summary>
        public bool IsActiveInPool()
        {
            return ParentPool != null && IsSpawnedFromPool;
        }

        /// <summary>
        /// Called when this GameObject is destroyed.
        /// Notifies the parent pool to remove dead references and prevent poisoned pool counts.
        /// </summary>
        private void OnDestroy()
        {
            // If this object was destroyed externally (scene unload, parent destroyed, etc.),
            // notify the pool to clean up references
            if (ParentPool != null && ParentPool is PoolMaster.IPoolInternal poolInternal)
            {
                poolInternal.NotifyObjectDestroyed(gameObject);
            }
        }
    }
}
