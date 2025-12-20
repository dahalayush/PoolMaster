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
    /// Lightweight performance metrics for pool diagnostics.
    /// Tracks essential statistics with zero logging overhead.
    /// </summary>
    public readonly struct PoolMetrics
    {
        /// <summary>
        /// Gets the total number of objects spawned from this pool since creation.
        /// </summary>
        public readonly long TotalSpawned;

        /// <summary>
        /// Gets the total number of objects returned to this pool since creation.
        /// </summary>
        public readonly long TotalDespawned;

        /// <summary>
        /// Gets the total number of new instances created (not reused from pool).
        /// </summary>
        public readonly long TotalCreated;

        /// <summary>
        /// Gets the total number of instances destroyed due to culling.
        /// </summary>
        public readonly long TotalDestroyed;

        /// <summary>
        /// Gets the number of times the pool expanded beyond its initial size.
        /// </summary>
        public readonly int ExpansionCount;

        /// <summary>
        /// Gets the number of times excess objects were culled from the pool.
        /// </summary>
        public readonly int CullCount;

        /// <summary>
        /// Time in seconds when the pool was last expanded.
        /// </summary>
        public readonly float LastExpandTime;

        /// <summary>
        /// Time in seconds when objects were last culled.
        /// </summary>
        public readonly float LastCullTime;

        /// <summary>
        /// Time in seconds when the pool was last active (spawn, despawn, expansion, or cull).
        /// Used for identifying truly inactive pools during cleanup operations.
        /// </summary>
        public readonly float LastActivityTime;

        /// <summary>
        /// Time in seconds when the pool was created.
        /// </summary>
        public readonly float CreationTime;

        /// <summary>
        /// Internal constructor for creating pool metrics snapshots.
        /// Exposed as public for testing purposes.
        /// </summary>
        public PoolMetrics(
            long totalSpawned,
            long totalDespawned,
            long totalCreated,
            long totalDestroyed,
            int expansionCount,
            int cullCount,
            float lastExpandTime,
            float lastCullTime,
            float lastActivityTime,
            float creationTime
        )
        {
            TotalSpawned = totalSpawned;
            TotalDespawned = totalDespawned;
            TotalCreated = totalCreated;
            TotalDestroyed = totalDestroyed;
            ExpansionCount = expansionCount;
            CullCount = cullCount;
            LastExpandTime = lastExpandTime;
            LastCullTime = lastCullTime;
            LastActivityTime = lastActivityTime;
            CreationTime = creationTime;
        }

        /// <summary>
        /// Gets the current number of active instances (spawned but not yet returned).
        /// Note: Calculated from deltas, not tracked state. Safe for typical usage but theoretically could overflow after 2^63 operations.
        /// </summary>
        public long CurrentActive => TotalSpawned - TotalDespawned;

        /// <summary>
        /// Gets the pool efficiency ratio (reused objects / total spawned). Higher values indicate better efficiency.
        /// Clamped to [0, 1] range. Returns 0 if TotalCreated exceeds TotalSpawned (invalid data).
        /// </summary>
        public float ReuseEfficiency =>
            TotalSpawned > 0 ? Mathf.Max(0f, (TotalSpawned - TotalCreated) / (float)TotalSpawned) : 0f;

        /// <summary>
        /// Average time between pool expansions in seconds. Returns 0 if no expansions or insufficient time elapsed.
        /// </summary>
        public float AverageExpansionInterval =>
            ExpansionCount > 0 && Time.time > CreationTime 
                ? (Time.time - CreationTime) / ExpansionCount 
                : 0f;

        /// <summary>
        /// Rate of spawns per second since pool creation.
        /// Useful for performance monitoring and capacity planning.
        /// </summary>
        public float SpawnsPerSecond =>
            Time.time > CreationTime ? (TotalSpawned / (Time.time - CreationTime)) : 0f;

        /// <summary>
        /// Average new object creation ratio per spawn.
        /// Values close to 0 indicate good reuse efficiency, values close to 1 indicate poor reuse.
        /// </summary>
        public float CreatesPerSpawn => TotalSpawned > 0 ? (float)TotalCreated / TotalSpawned : 0f;

        /// <summary>
        /// Merges two PoolMetrics instances to create an aggregated view for dashboards and snapshots.
        /// </summary>
        /// <param name="a">First metrics instance.</param>
        /// <param name="b">Second metrics instance.</param>
        /// <returns>New PoolMetrics representing the merged statistics.</returns>
        public static PoolMetrics Merge(PoolMetrics a, PoolMetrics b)
        {
            var creation = Math.Min(a.CreationTime, b.CreationTime);
            return new PoolMetrics(
                a.TotalSpawned + b.TotalSpawned,
                a.TotalDespawned + b.TotalDespawned,
                a.TotalCreated + b.TotalCreated,
                a.TotalDestroyed + b.TotalDestroyed,
                a.ExpansionCount + b.ExpansionCount,
                a.CullCount + b.CullCount,
                Math.Max(a.LastExpandTime, b.LastExpandTime),
                Math.Max(a.LastCullTime, b.LastCullTime),
                Math.Max(a.LastActivityTime, b.LastActivityTime),
                creation
            );
        }

        /// <summary>
        /// Returns a formatted string representation for debugging.
        /// </summary>
        public override string ToString()
        {
            return $"PoolMetrics {{ "
                + $"Spawned: {TotalSpawned}, "
                + $"Despawned: {TotalDespawned}, "
                + $"Created: {TotalCreated}, "
                + $"Active: {CurrentActive}, "
                + $"Expansions: {ExpansionCount}, "
                + $"ReuseEff: {ReuseEfficiency:P1} }}";
        }
    }

    /// <summary>
    /// Mutable counterpart to PoolMetrics for internal pool tracking.
    /// </summary>
    internal struct PoolMetricsTracker
    {
        public long totalSpawned;
        public long totalDespawned;
        public long totalCreated;
        public long totalDestroyed;
        public int expansionCount;
        public int cullCount;
        public float lastExpandTime;
        public float lastCullTime;
        public float lastActivityTime;
        public readonly float creationTime;

        public PoolMetricsTracker(float creationTime)
        {
            this.totalSpawned = 0;
            this.totalDespawned = 0;
            this.totalCreated = 0;
            this.totalDestroyed = 0;
            this.expansionCount = 0;
            this.cullCount = 0;
            this.lastExpandTime = creationTime;
            this.lastCullTime = creationTime;
            this.lastActivityTime = creationTime;
            this.creationTime = creationTime;
        }

        public void RecordSpawn()
        {
            totalSpawned++;
            lastActivityTime = Time.time;
        }

        public void RecordDespawn()
        {
            totalDespawned++;
            lastActivityTime = Time.time;
        }

        public void RecordCreation() => totalCreated++;

        public void RecordDestruction() => totalDestroyed++;

        public void RecordExpansion()
        {
            expansionCount++;
            lastExpandTime = Time.time;
            lastActivityTime = Time.time;
        }

        public void RecordCull()
        {
            cullCount++;
            lastCullTime = Time.time;
            lastActivityTime = Time.time;
        }

        public PoolMetrics ToReadOnly()
        {
            return new PoolMetrics(
                totalSpawned,
                totalDespawned,
                totalCreated,
                totalDestroyed,
                expansionCount,
                cullCount,
                lastExpandTime,
                lastCullTime,
                lastActivityTime,
                creationTime
            );
        }
    }
}
