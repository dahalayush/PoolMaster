// ============================================================================
// PoolMaster - Object Pooling System for Unity
// Copyright (c) 2026 Max Thomas Coates
// https://github.com/mistyuk/PoolMaster
// Licensed under MIT License (see LICENSE file for details)
// ============================================================================

using UnityEngine;
using PoolMaster;
using System.Threading.Tasks;

namespace PoolMaster.Examples
{
    /// <summary>
    /// Demonstrates command buffer usage for thread-safe, off-main-thread spawn requests.
    /// Useful for procedural generation, pathfinding, or other compute-heavy operations.
    /// </summary>
    public class CommandBufferDemo : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private float computeInterval = 1.5f;
        [SerializeField] private int computeIterations = 1000;
        [SerializeField] private int objectsToSpawn = 5;
        [SerializeField] private float spawnRadius = 2f;

        private PoolCommandBuffer commandBuffer;
        private float nextComputeTime;
        private bool isComputing;

        void Start()
        {
            commandBuffer = new PoolCommandBuffer();
        }

        void Update()
        {
            if (DemoSceneSetup.ParticlePrefab == null)
                return;

            // Start compute task
            if (Time.time >= nextComputeTime && !isComputing)
            {
                nextComputeTime = Time.time + computeInterval;
                StartAsyncCompute();
            }

            // Flush command buffer on main thread
            if (commandBuffer.HasPendingOperations)
            {
                var poolId = DemoSceneSetup.ParticlePrefab.name;
                var pool = PoolingManager.Instance.GetPool(poolId);
                if (pool != null)
                {
                    commandBuffer.FlushTo((IPoolControl)pool);
                }
            }
        }

        async void StartAsyncCompute()
        {
            isComputing = true;

            // Pre-generate random values on main thread (Unity APIs aren't thread-safe)
            var spawnPositions = new Vector3[objectsToSpawn];
            for (int i = 0; i < objectsToSpawn; i++)
            {
                var angle = Random.Range(0f, Mathf.PI * 2f);
                var offset = new Vector3(
                    Mathf.Cos(angle) * spawnRadius,
                    Random.Range(0f, 2f),
                    Mathf.Sin(angle) * spawnRadius
                );
                spawnPositions[i] = transform.position + offset;
            }

            // Simulate heavy computation on background thread
            await Task.Run(() => SimulateComputation(spawnPositions));

            isComputing = false;
        }

        void SimulateComputation(Vector3[] positions)
        {
            // Simulate expensive computation (e.g., procedural generation, pathfinding)
            for (int i = 0; i < computeIterations; i++)
            {
                float dummy = Mathf.Sin(i) * Mathf.Cos(i);
            }

            // Enqueue spawn commands from worker thread (thread-safe)
            for (int i = 0; i < positions.Length; i++)
            {
                commandBuffer.EnqueueSpawn(
                    positions[i],
                    Quaternion.identity
                );
            }
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, spawnRadius);
            
            if (isComputing)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(transform.position + Vector3.up * 2f, Vector3.one * 0.5f);
            }
        }
    }
}
