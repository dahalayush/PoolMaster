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
    /// Demonstrates VFX pooling with event-based timing and auto-cleanup.
    /// Shows short-lived effects that spawn, animate, and despawn automatically.
    /// </summary>
    public class VFXDemo : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private float spawnInterval = 0.8f;
        [SerializeField] private float vfxDuration = 1.5f;
        [SerializeField] private float expansionSpeed = 2f;
        [SerializeField] private float spawnRadius = 3f;

        private float nextSpawnTime;

        void Update()
        {
            if (DemoSceneSetup.VFXPrefab == null)
                return;

            if (Time.time >= nextSpawnTime)
            {
                SpawnVFX();
                nextSpawnTime = Time.time + spawnInterval;
            }
        }

        void SpawnVFX()
        {
            // Random position in area
            var randomOffset = Random.insideUnitCircle * spawnRadius;
            var position = transform.position + new Vector3(randomOffset.x, 0.5f, randomOffset.y);

            // Spawn VFX
            var vfx = PoolingManager.Instance.Spawn(
                DemoSceneSetup.VFXPrefab,
                position,
                Quaternion.identity
            );

            if (vfx != null)
            {
                // Animate VFX expansion and fade
                StartCoroutine(AnimateVFX(vfx, vfxDuration));
            }
        }

        System.Collections.IEnumerator AnimateVFX(GameObject vfx, float duration)
        {
            var startScale = Vector3.one * 0.1f;
            var endScale = Vector3.one * expansionSpeed;
            var renderer = vfx.GetComponent<Renderer>();
            var startColor = renderer.material.color;
            
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Scale up
                vfx.transform.localScale = Vector3.Lerp(startScale, endScale, t);

                // Fade out
                if (renderer != null)
                {
                    var color = startColor;
                    color.a = 1f - t;
                    renderer.material.color = color;
                }

                yield return null;
            }

            // Despawn
            if (vfx != null && vfx.activeInHierarchy)
            {
                // Reset scale and color for reuse
                vfx.transform.localScale = startScale;
                if (renderer != null)
                {
                    renderer.material.color = startColor;
                }
                
                PoolingManager.Instance.Despawn(vfx);
            }
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, spawnRadius);
        }
    }
}
