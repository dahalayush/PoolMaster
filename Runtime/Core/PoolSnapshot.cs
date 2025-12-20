// ============================================================================
// PoolMaster - Object Pooling System for Unity
// Copyright (c) 2026 Max Thomas Coates
// https://github.com/mistyuk/PoolMaster
// Licensed under MIT License (see LICENSE file for details)
// ============================================================================

using System;
using System.Collections.Generic;

namespace PoolMaster
{
    /// <summary>
    /// Lightweight snapshot of pool statistics for diagnostics and monitoring.
    /// Represents either individual pool statistics or global aggregated statistics.
    /// </summary>
    [Serializable]
    public readonly struct PoolSnapshot
    {
        /// <summary>
        /// Gets the total number of pools in the system (for global snapshots).
        /// </summary>
        public readonly int TotalPools;

        /// <summary>
        /// Gets the total number of currently active (spawned) objects across all pools.
        /// </summary>
        public readonly int TotalActiveObjects;

        /// <summary>
        /// Total number of inactive (pooled) objects ready for reuse across all pools.
        /// </summary>
        public readonly int TotalInactiveObjects;

        /// <summary>
        /// Unix timestamp (seconds since epoch) when this snapshot was captured.
        /// Useful for time-series analysis and UI display.
        /// </summary>
        public readonly double UtcCapturedAt;

        /// <summary>
        /// Dictionary containing detailed metrics for each individual pool.
        /// Key is the pool's unique PoolId (not just prefab name) to avoid collisions.
        /// </summary>
        public readonly Dictionary<string, PoolMetrics> PoolBreakdown;

        /// <summary>
        /// Creates a global snapshot aggregating multiple pools.
        /// </summary>
        /// <param name="totalPools">Total number of pools.</param>
        /// <param name="totalActive">Total active objects.</param>
        /// <param name="totalInactive">Total inactive objects.</param>
        /// <param name="poolBreakdown">Per-pool metrics breakdown.</param>
        public PoolSnapshot(
            int totalPools,
            int totalActive,
            int totalInactive,
            Dictionary<string, PoolMetrics> poolBreakdown
        )
        {
            TotalPools = totalPools;
            TotalActiveObjects = totalActive;
            TotalInactiveObjects = totalInactive;
            UtcCapturedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            PoolBreakdown = poolBreakdown ?? new Dictionary<string, PoolMetrics>();
        }

        /// <summary>
        /// Creates a snapshot for a single pool. Convenience method for per-pool diagnostics.
        /// </summary>
        /// <param name="poolId">The unique pool identifier.</param>
        /// <param name="metrics">The pool's current metrics.</param>
        /// <param name="inactiveCount">Current inactive count (default: 0).</param>
        /// <returns>A snapshot containing only the specified pool.</returns>
        public static PoolSnapshot Single(string poolId, PoolMetrics metrics, int inactiveCount = 0)
        {
            return new PoolSnapshot(
                totalPools: 1,
                totalActive: (int)metrics.CurrentActive,
                totalInactive: inactiveCount,
                poolBreakdown: new Dictionary<string, PoolMetrics> { { poolId, metrics } }
            );
        }

        /// <summary>
        /// Gets the total objects in the system (active + inactive).
        /// </summary>
        public int TotalObjects => TotalActiveObjects + TotalInactiveObjects;

        /// <summary>
        /// Gets the global utilization percentage across all pools.
        /// </summary>
        public float GlobalUtilization =>
            TotalObjects > 0 ? (TotalActiveObjects / (float)TotalObjects) * 100f : 0f;

        /// <summary>
        /// Returns a formatted string representation for debugging.
        /// </summary>
        public override string ToString()
        {
            return $"Global Pool Snapshot: {TotalPools} pools, "
                + $"Active={TotalActiveObjects}, Inactive={TotalInactiveObjects}, "
                + $"Total={TotalObjects}, Utilization={GlobalUtilization:F1}%";
        }

        /// <summary>
        /// Returns a compact string representation for UI display.
        /// </summary>
        public string ToCompactString()
        {
            return $"{TotalPools} pools: {TotalActiveObjects}/{TotalObjects} ({GlobalUtilization:F0}%)";
        }
    }
}
