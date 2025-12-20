// ============================================================================
// PoolMaster - Object Pooling System for Unity
// Copyright (c) 2026 Max Thomas Coates
// https://github.com/mistyuk/PoolMaster
// Licensed under MIT License (see LICENSE file for details)
// ============================================================================

using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using UnityEngine;

namespace PoolMaster
{
    /// <summary>
    /// Represents a deferred spawn command for a pooled object.
    /// </summary>
    public readonly struct SpawnCommand
    {
        public readonly Vector3 position;
        public readonly Quaternion rotation;
        public readonly Transform parent;

        public SpawnCommand(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            this.position = position;
            this.rotation = rotation;
            this.parent = parent;
        }
    }

    /// <summary>
    /// Represents a deferred batch spawn command for multiple pooled objects.
    ///
    /// CRITICAL: The position and rotation arrays are stored by reference for performance.
    /// DO NOT modify these arrays after passing them to EnqueueSpawnBatch.
    /// If you need to reuse/modify the arrays, either:
    /// 1. Use EnqueueSpawnBatchSafe() which copies the arrays (slower but safe)
    /// 2. Clone the arrays before enqueueing: EnqueueSpawnBatch((Vector3[])positions.Clone(), ...)
    /// 3. Create new arrays for each enqueue call
    /// </summary>
    public readonly struct SpawnBatchCommand
    {
        public readonly Vector3[] positions;
        public readonly Quaternion[] rotations;
        public readonly Transform parent;

        public SpawnBatchCommand(
            Vector3[] positions,
            Quaternion[] rotations,
            Transform parent = null
        )
        {
            this.positions = positions;
            this.rotations = rotations;
            this.parent = parent;
        }
    }

    /// <summary>
    /// Thread-safe command buffer for deferred pooling operations.
    /// Enables queueing spawn/despawn commands from background threads for execution on the main thread.
    ///
    /// Thread-Safety:
    /// - EnqueueSpawn(), EnqueueSpawnBatch(), EnqueueReturn(): Safe to call from any thread
    /// - FlushTo(): Must ONLY be called from main thread (Unity operations are not thread-safe)
    /// - Pool<T>.Spawn/Despawn: Never call directly from background threads, always use command buffers
    /// </summary>
    public sealed class PoolCommandBuffer
    {
        private readonly ConcurrentQueue<SpawnCommand> spawnQueue =
            new ConcurrentQueue<SpawnCommand>();
        private readonly ConcurrentQueue<SpawnBatchCommand> spawnBatchQueue =
            new ConcurrentQueue<SpawnBatchCommand>();
        private readonly ConcurrentQueue<GameObject> returnQueue =
            new ConcurrentQueue<GameObject>();

        /// <summary>
        /// Gets the number of pending single spawn commands.
        /// </summary>
        public int PendingSpawnCount => spawnQueue.Count;

        /// <summary>
        /// Gets the number of pending batch spawn commands.
        /// </summary>
        public int PendingBatchCount => spawnBatchQueue.Count;

        /// <summary>
        /// Gets the number of pending return commands.
        /// </summary>
        public int PendingReturnCount => returnQueue.Count;

        /// <summary>
        /// Gets whether any operations are pending. Fast O(1) check using IsEmpty.
        /// </summary>
        public bool HasPendingOperations =>
            !spawnQueue.IsEmpty || !spawnBatchQueue.IsEmpty || !returnQueue.IsEmpty;

        /// <summary>
        /// Gets the total number of all pending commands. Note: May be O(n) on ConcurrentQueue.
        /// </summary>
        public int TotalPendingCount => PendingSpawnCount + PendingBatchCount + PendingReturnCount;

        /// <summary>
        /// Enqueues a spawn command for execution on the main thread.
        /// </summary>
        public void EnqueueSpawn(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            spawnQueue.Enqueue(new SpawnCommand(position, rotation, parent));
        }

        /// <summary>
        /// Enqueues a batch spawn command for execution on the main thread.
        ///
        /// CRITICAL RELIABILITY WARNING:
        /// Arrays are stored by REFERENCE for performance. DO NOT modify the position or rotation
        /// arrays after calling this method. If the arrays are mutated before FlushTo() is called,
        /// spawned objects will have corrupted positions/rotations.
        ///
        /// If you need to reuse arrays:
        /// - Use EnqueueSpawnBatchSafe() which copies arrays (safer but allocates)
        /// - Or manually clone: EnqueueSpawnBatch((Vector3[])positions.Clone(), ...)
        /// - Or create new arrays for each call
        /// </summary>
        public void EnqueueSpawnBatch(
            Vector3[] positions,
            Quaternion[] rotations,
            Transform parent = null
        )
        {
            if (positions == null || positions.Length == 0)
                return;
            spawnBatchQueue.Enqueue(new SpawnBatchCommand(positions, rotations, parent));
        }

        /// <summary>
        /// Enqueues a batch spawn command with SAFE array copies.
        /// This variant clones the position and rotation arrays to prevent mutation issues.
        /// Use this if you plan to reuse/modify the source arrays after enqueueing.
        ///
        /// Performance: Allocates new arrays (2 allocations). For hot paths where arrays
        /// are guaranteed not to be modified, use the faster EnqueueSpawnBatch().
        /// </summary>
        public void EnqueueSpawnBatchSafe(
            Vector3[] positions,
            Quaternion[] rotations,
            Transform parent = null
        )
        {
            if (positions == null || positions.Length == 0)
                return;

            // Clone arrays to prevent mutation
            Vector3[] positionsCopy = (Vector3[])positions.Clone();
            Quaternion[] rotationsCopy = rotations != null ? (Quaternion[])rotations.Clone() : null;

            spawnBatchQueue.Enqueue(new SpawnBatchCommand(positionsCopy, rotationsCopy, parent));
        }

        /// <summary>
        /// Enqueues a return command for execution on the main thread.
        /// </summary>
        public void EnqueueReturn(GameObject instance)
        {
            if (instance != null)
                returnQueue.Enqueue(instance);
        }

        /// <summary>
        /// Flushes all pending commands to the specified pool. Must be called from the main thread.
        /// </summary>
        /// <returns>The number of operations processed.</returns>
        public int FlushTo(IPoolControl pool)
        {
            if (pool == null)
            {
                PoolLog.Error("PoolCommandBuffer.FlushTo: pool is null");
                return 0;
            }

            int processed = 0;

            // Returns first — makes room for spawns
            while (returnQueue.TryDequeue(out var inst))
            {
                try
                {
                    if (inst != null)
                    {
                        pool.Despawn(inst);
                        processed++;
                    }
                }
                catch (Exception e)
                {
                    PoolLog.Error($"PoolCommandBuffer: return error for {inst?.name}: {e}");
                }
            }

            // Batch spawns
            while (spawnBatchQueue.TryDequeue(out var bcmd))
            {
                try
                {
                    int count = pool.SpawnBatch(bcmd.positions, bcmd.rotations, bcmd.parent);
                    processed += count;
                }
                catch (Exception e)
                {
                    PoolLog.Error($"PoolCommandBuffer: batch spawn error: {e}");
                }
            }

            // Single spawns
            while (spawnQueue.TryDequeue(out var cmd))
            {
                try
                {
                    var go = pool.Spawn(cmd.position, cmd.rotation, cmd.parent);
                    if (go != null)
                    {
                        processed++;
                    }
                }
                catch (Exception e)
                {
                    PoolLog.Error($"PoolCommandBuffer: spawn error at {cmd.position}: {e}");
                }
            }

            return processed;
        }

        /// <summary>
        /// Flushes all pending commands to the specified strongly-typed pool. Must be called from the main thread.
        /// </summary>
        /// <returns>The number of operations processed.</returns>
        public int FlushTo<T>(Pool<T> pool)
            where T : Component, IPoolable
        {
            if (pool == null)
            {
                PoolLog.Error("PoolCommandBuffer.FlushTo: pool is null");
                return 0;
            }
            int processed = 0;

            while (returnQueue.TryDequeue(out var inst))
            {
                try
                {
                    if (inst != null)
                    {
                        pool.Despawn(inst);
                        processed++;
                    }
                }
                catch (Exception e)
                {
                    PoolLog.Error(
                        $"PoolCommandBuffer: Error processing return command for {inst?.name}: {e}"
                    );
                }
            }

            while (spawnBatchQueue.TryDequeue(out var bcmd))
            {
                try
                {
                    // Use extension to avoid new interface coupling
                    int count = ((IPoolControl)pool).SpawnBatch(
                        bcmd.positions,
                        bcmd.rotations,
                        bcmd.parent
                    );
                    processed += count;
                }
                catch (Exception e)
                {
                    PoolLog.Error($"PoolCommandBuffer: Error processing batch spawn: {e}");
                }
            }

            while (spawnQueue.TryDequeue(out var scmd))
            {
                try
                {
                    var go = pool.Spawn(scmd.position, scmd.rotation, scmd.parent);
                    if (go != null)
                    {
                        processed++;
                    }
                }
                catch (Exception e)
                {
                    PoolLog.Error(
                        $"PoolCommandBuffer: Error processing spawn at {scmd.position}: {e}"
                    );
                }
            }

            return processed;
        }

        /// <summary>
        /// Clears all pending commands.
        /// </summary>
        public void Clear()
        {
            while (spawnQueue.TryDequeue(out _)) { }
            while (spawnBatchQueue.TryDequeue(out _)) { }
            while (returnQueue.TryDequeue(out _)) { }
        }

        public override string ToString()
        {
            return $"PoolCommandBuffer {{ Spawns: {PendingSpawnCount}, Batches: {PendingBatchCount}, Returns: {PendingReturnCount} }}";
        }
    }
}
