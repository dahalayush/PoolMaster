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
    /// Demonstrates basic pool operations: spawn, despawn, and object reuse.
    /// Shows the fundamental pooling cycle with visual feedback.
    /// </summary>
    public class BasicPoolingDemo : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private float spawnInterval = 1f;
        [SerializeField] private float despawnDelay = 3f;
        [SerializeField] private int spawnHeight = 2;
        [SerializeField] private float spawnRadius = 3f;

        private float nextSpawnTime;

        void Update()
        {
            if (DemoSceneSetup.BasicSpherePrefab == null)
                return;

            if (Time.time >= nextSpawnTime)
            {
                SpawnObject();
                nextSpawnTime = Time.time + spawnInterval;
            }
        }

        void SpawnObject()
        {
            // Random position in a circle
            var angle = Random.Range(0f, Mathf.PI * 2f);
            var position = transform.position + new Vector3(
                Mathf.Cos(angle) * spawnRadius,
                spawnHeight,
                Mathf.Sin(angle) * spawnRadius
            );

            // Spawn from pool
            var obj = PoolingManager.Instance.Spawn(
                DemoSceneSetup.BasicSpherePrefab,
                position,
                Quaternion.identity
            );

            if (obj != null)
            {
                // Add simple falling motion
                var rb = obj.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = false;
                    rb.useGravity = true;
                    rb.linearVelocity = Vector3.down * 2f;
                }

                // Auto-despawn after delay
                StartCoroutine(DespawnAfterDelay(obj, despawnDelay));
            }
        }

        System.Collections.IEnumerator DespawnAfterDelay(GameObject obj, float delay)
        {
            yield return new WaitForSeconds(delay);

            if (obj != null)
            {
                PoolingManager.Instance.Despawn(obj);
            }
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * spawnHeight, spawnRadius);
        }
    }
}
