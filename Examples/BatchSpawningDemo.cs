// ============================================================================
// PoolMaster - Object Pooling System for Unity
// Copyright (c) 2026 Max Thomas Coates
// https://github.com/mistyuk/PoolMaster
// Licensed under MIT License (see LICENSE file for details)
// ============================================================================

using UnityEngine;
using PoolMaster;

namespace PoolMaster.Examples
{
    /// <summary>
    /// Demonstrates batch spawning operations for efficient multi-object creation.
    /// Useful for spawning groups of objects (explosions, particle bursts, enemy waves).
    /// </summary>
    public class BatchSpawningDemo : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private float spawnInterval = 2f;
        [SerializeField] private int objectsPerBatch = 10;
        [SerializeField] private float spawnRadius = 2f;
        [SerializeField] private float despawnDelay = 4f;

        private float nextSpawnTime;
        private PoolCommandBuffer commandBuffer;

        void Start()
        {
            commandBuffer = new PoolCommandBuffer();
        }

        void Update()
        {
            if (DemoSceneSetup.BasicSpherePrefab == null)
                return;

            if (Time.time >= nextSpawnTime)
            {
                SpawnBatch();
                nextSpawnTime = Time.time + spawnInterval;
            }
        }

        void SpawnBatch()
        {
            // Generate positions in a circle
            var positions = new Vector3[objectsPerBatch];
            var rotations = new Quaternion[objectsPerBatch];

            for (int i = 0; i < objectsPerBatch; i++)
            {
                var angle = (i / (float)objectsPerBatch) * Mathf.PI * 2f;
                positions[i] = transform.position + new Vector3(
                    Mathf.Cos(angle) * spawnRadius,
                    1f,
                    Mathf.Sin(angle) * spawnRadius
                );
                rotations[i] = Quaternion.identity;
            }

            // Batch spawn using command buffer (could be from worker thread)
            commandBuffer.EnqueueSpawnBatch(positions, rotations);

            // Flush to pool on main thread
            var poolId = DemoSceneSetup.BasicSpherePrefab.name;
            var pool = PoolingManager.Instance.GetPool(poolId);
            if (pool != null)
            {
                int spawned = commandBuffer.FlushTo((IPoolControl)pool);
                
                // Auto-despawn batch after delay
                StartCoroutine(DespawnBatchAfterDelay(despawnDelay));
            }
        }

        System.Collections.IEnumerator DespawnBatchAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);

            // Find all spawned objects from this batch and despawn them
            var pooledObjects = FindObjectsByType<SimplePoolableObject>(FindObjectsSortMode.None);
            foreach (var obj in pooledObjects)
            {
                if (obj.gameObject.activeInHierarchy && 
                    Vector3.Distance(obj.transform.position, transform.position) < spawnRadius * 2f)
                {
                    PoolingManager.Instance.Despawn(obj.gameObject);
                }
            }
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, spawnRadius);
            
            // Draw batch spawn positions
            for (int i = 0; i < objectsPerBatch; i++)
            {
                var angle = (i / (float)objectsPerBatch) * Mathf.PI * 2f;
                var pos = transform.position + new Vector3(
                    Mathf.Cos(angle) * spawnRadius,
                    1f,
                    Mathf.Sin(angle) * spawnRadius
                );
                Gizmos.DrawWireSphere(pos, 0.2f);
            }
        }
    }
}
