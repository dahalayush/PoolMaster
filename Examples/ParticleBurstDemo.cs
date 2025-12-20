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
    /// Demonstrates particle burst pooling for high-frequency spawning scenarios.
    /// Shows dynamic pool expansion under heavy load and efficient cleanup.
    /// </summary>
    public class ParticleBurstDemo : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private float burstInterval = 3f;
        [SerializeField] private int particlesPerBurst = 50;
        [SerializeField] private float burstForce = 5f;
        [SerializeField] private float particleLifetime = 2f;

        private float nextBurstTime;

        void Update()
        {
            if (DemoSceneSetup.ParticlePrefab == null)
                return;

            if (Time.time >= nextBurstTime)
            {
                SpawnParticleBurst();
                nextBurstTime = Time.time + burstInterval;
            }
        }

        void SpawnParticleBurst()
        {
            for (int i = 0; i < particlesPerBurst; i++)
            {
                // Random direction
                var direction = Random.onUnitSphere;
                var velocity = direction * Random.Range(burstForce * 0.5f, burstForce);

                // Spawn particle
                var particle = PoolingManager.Instance.Spawn(
                    DemoSceneSetup.ParticlePrefab,
                    transform.position,
                    Quaternion.identity
                );

                if (particle != null)
                {
                    // Apply physics
                    var rb = particle.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.isKinematic = false;
                        rb.useGravity = true;
                        rb.linearVelocity = velocity;
                    }

                    // Random lifetime variation
                    float lifetime = particleLifetime * Random.Range(0.8f, 1.2f);
                    StartCoroutine(DespawnParticle(particle, lifetime));
                }
            }
        }

        System.Collections.IEnumerator DespawnParticle(GameObject particle, float lifetime)
        {
            yield return new WaitForSeconds(lifetime);

            if (particle != null && particle.activeInHierarchy)
            {
                PoolingManager.Instance.Despawn(particle);
            }
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
            
            // Draw burst indicator
            if (Time.time >= nextBurstTime - 0.5f)
            {
                float pulseSize = 0.5f + Mathf.PingPong(Time.time * 5f, 0.5f);
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(transform.position, pulseSize);
            }
        }
    }
}
