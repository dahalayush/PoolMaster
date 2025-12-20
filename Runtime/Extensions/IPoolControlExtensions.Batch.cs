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
    /// Extension methods for batch operations on IPoolControl instances.
    /// </summary>
    public static class IPoolControlBatchExtensions
    {
        /// <summary>
        /// Spawns multiple objects at once using arrays of positions and rotations.
        /// </summary>
        /// <param name="pool">The pool to spawn from.</param>
        /// <param name="positions">Array of world positions.</param>
        /// <param name="rotations">Array of world rotations (optional, uses identity if null).</param>
        /// <param name="parent">Optional parent transform for all spawned objects.</param>
        /// <returns>Number of objects successfully spawned.</returns>
        public static int SpawnBatch(
            this IPoolControl pool,
            Vector3[] positions,
            Quaternion[] rotations,
            Transform parent = null
        )
        {
            if (pool == null || positions == null)
                return 0;
            int len = positions.Length;
            if (len == 0)
                return 0;

            bool useRots = rotations != null && rotations.Length >= len;
            int spawned = 0;

            for (int i = 0; i < len; i++)
            {
                var rot = useRots ? rotations[i] : Quaternion.identity;
                if (pool.Spawn(positions[i], rot, parent) != null)
                    spawned++;
            }
            return spawned;
        }

        /// <summary>
        /// Spawns multiple objects and writes results into a preallocated array. Zero-allocation when you need references.
        /// </summary>
        /// <param name="pool">The pool to spawn from.</param>
        /// <param name="positions">Array of world positions.</param>
        /// <param name="rotations">Array of world rotations (optional, uses identity if null).</param>
        /// <param name="parent">Optional parent transform for all spawned objects.</param>
        /// <param name="results">Preallocated array to write spawned GameObjects into.</param>
        /// <returns>Number of objects successfully spawned.</returns>
        public static int SpawnBatchNonAlloc(
            this IPoolControl pool,
            Vector3[] positions,
            Quaternion[] rotations,
            Transform parent,
            GameObject[] results
        )
        {
            if (pool == null || positions == null || results == null)
                return 0;
            int len = positions.Length;
            if (results.Length < len)
                len = results.Length;

            bool useRots = rotations != null && rotations.Length >= len;
            int spawned = 0;

            for (int i = 0; i < len; i++)
            {
                var rot = useRots ? rotations[i] : Quaternion.identity;
                results[i] = pool.Spawn(positions[i], rot, parent);
                if (results[i] != null)
                    spawned++;
            }
            return spawned;
        }

        /// <summary>
        /// Spawns objects in a 3D grid pattern (X/Z plane). Useful for testing or procedural placement.
        /// </summary>
        /// <param name="pool">The pool to spawn from.</param>
        /// <param name="center">Center point of the grid.</param>
        /// <param name="spacing">Distance between grid points.</param>
        /// <param name="gridSize">Size of the grid (gridSize x gridSize objects).</param>
        /// <param name="parent">Optional parent transform.</param>
        /// <returns>Number of objects successfully spawned.</returns>
        public static int SpawnGrid(
            this IPoolControl pool,
            Vector3 center,
            float spacing,
            int gridSize,
            Transform parent = null
        )
        {
            if (pool == null || gridSize <= 0)
                return 0;

            int spawned = 0;
            float halfGrid = (gridSize - 1) * spacing * 0.5f;

            for (int x = 0; x < gridSize; x++)
            {
                for (int z = 0; z < gridSize; z++)
                {
                    Vector3 position =
                        center + new Vector3(x * spacing - halfGrid, 0f, z * spacing - halfGrid);

                    if (pool.Spawn(position, Quaternion.identity, parent) != null)
                        spawned++;
                }
            }

            return spawned;
        }
    }
}
