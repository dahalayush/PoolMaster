// ============================================================================
// PoolMaster - Object Pooling System for Unity
// Copyright (c) 2026 Max Thomas Coates
// https://github.com/mistyuk/PoolMaster
// Licensed under MIT License (see LICENSE file for details)
// ============================================================================

using PoolMaster;
using UnityEngine;

namespace PoolMaster.Examples
{
    /// <summary>
    /// Demonstrates projectile pooling with realistic behavior: spawn, move, hit, despawn.
    /// Shows high-frequency spawning with automatic lifecycle management.
    /// </summary>
    public class ProjectileDemo : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField]
        private float fireRate = 0.2f;

        [SerializeField]
        private float projectileSpeed = 10f;

        [SerializeField]
        private float projectileLifetime = 2f;

        [SerializeField]
        private int projectileCount = 3;

        [SerializeField]
        private float spreadAngle = 15f;

        private float nextFireTime;
        private System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<
            GameObject,
            float
        >> activeProjectiles =
            new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<
                GameObject,
                float
            >>();

        void Update()
        {
            if (DemoSceneSetup.ProjectilePrefab == null)
                return;

            // Fire new projectiles
            if (Time.time >= nextFireTime)
            {
                FireProjectiles();
                nextFireTime = Time.time + fireRate;
            }

            // Despawn expired projectiles (NO COROUTINES!)
            for (int i = activeProjectiles.Count - 1; i >= 0; i--)
            {
                if (Time.time >= activeProjectiles[i].Value)
                {
                    var projectile = activeProjectiles[i].Key;
                    if (projectile != null && projectile.activeInHierarchy)
                    {
                        PoolingManager.Instance.Despawn(projectile);
                    }
                    activeProjectiles.RemoveAt(i);
                }
            }
        }

        void OnDisable()
        {
            // Clean up when demo is disabled
            activeProjectiles.Clear();
            nextFireTime = 0;
        }

        void FireProjectiles()
        {
            for (int i = 0; i < projectileCount; i++)
            {
                // Calculate spread
                float angle = 0f;
                if (projectileCount > 1)
                {
                    angle = Mathf.Lerp(-spreadAngle, spreadAngle, i / (float)(projectileCount - 1));
                }

                var direction = Quaternion.Euler(0, angle, 0) * transform.forward;
                var rotation = Quaternion.LookRotation(direction);

                // Spawn projectile
                var projectile = PoolingManager.Instance.Spawn(
                    DemoSceneSetup.ProjectilePrefab,
                    transform.position,
                    rotation
                );

                if (projectile != null)
                {
                    // Add velocity - cache component, don't GetComponent every frame!
                    var rb = projectile.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.isKinematic = false;
                        rb.useGravity = false;
                        rb.linearVelocity = direction * projectileSpeed;
                    }

                    // Track despawn time (no coroutines = no leaks!)
                    activeProjectiles.Add(
                        new System.Collections.Generic.KeyValuePair<GameObject, float>(
                            projectile,
                            Time.time + projectileLifetime
                        )
                    );
                }
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;

            // Draw fire direction
            for (int i = 0; i < projectileCount; i++)
            {
                float angle = 0f;
                if (projectileCount > 1)
                {
                    angle = Mathf.Lerp(-spreadAngle, spreadAngle, i / (float)(projectileCount - 1));
                }

                var direction = Quaternion.Euler(0, angle, 0) * transform.forward;
                Gizmos.DrawRay(transform.position, direction * 2f);
            }
        }
    }
}
