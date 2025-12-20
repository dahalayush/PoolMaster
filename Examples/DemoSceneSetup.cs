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
    /// Runtime setup for the PoolMaster demo scene. Creates all prefabs and initializes pools.
    /// This approach avoids prefab dependencies and ensures render pipeline compatibility.
    /// </summary>
    public class DemoSceneSetup : MonoBehaviour
    {
        [Header("Prefab Generation")]
        [Tooltip("Material to use for all demo objects (leave null for default)")]
        public Material sharedMaterial;

        [Header("Pool Configuration")]
        [SerializeField]
        private int basicPoolSize = 20;

        [SerializeField]
        private int projectilePoolSize = 50;

        [SerializeField]
        private int vfxPoolSize = 30;

        [SerializeField]
        private int particlePoolSize = 100;

        // Public references for demo scripts
        public static GameObject BasicSpherePrefab { get; private set; }
        public static GameObject ProjectilePrefab { get; private set; }
        public static GameObject VFXPrefab { get; private set; }
        public static GameObject ParticlePrefab { get; private set; }

        void Awake()
        {
            CreatePrefabs();
            InitializePools();
        }

        /// <summary>
        /// Creates runtime prefabs using Unity primitives. Works with any render pipeline.
        /// </summary>
        void CreatePrefabs()
        {
            // Basic Sphere - cyan
            BasicSpherePrefab = CreateSpherePrefab("BasicSphere", Color.cyan, 0.5f);

            // Projectile - red, smaller (trail removed for performance)
            ProjectilePrefab = CreateSpherePrefab("Projectile", Color.red, 0.3f);

            // VFX - yellow, glowing
            VFXPrefab = CreateSpherePrefab("VFX", Color.yellow, 0.4f);

            // Particle - white, tiny
            ParticlePrefab = CreateSpherePrefab("Particle", Color.white, 0.15f);

            // Mark all prefabs inactive
            BasicSpherePrefab.SetActive(false);
            ProjectilePrefab.SetActive(false);
            VFXPrefab.SetActive(false);
            ParticlePrefab.SetActive(false);
        }

        /// <summary>
        /// Creates a sphere prefab with specified color and scale.
        /// </summary>
        GameObject CreateSpherePrefab(string name, Color color, float scale)
        {
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = name;
            sphere.transform.localScale = Vector3.one * scale;

            // REMOVE COLLIDER - not needed for demo, causes physics overhead
            var collider = sphere.GetComponent<Collider>();
            if (collider != null)
                Object.DestroyImmediate(collider);

            // Apply color using SHARED material (no allocations!)
            var renderer = sphere.GetComponent<Renderer>();
            if (sharedMaterial == null)
            {
                sharedMaterial = new Material(Shader.Find("Standard"));
            }

            // Create one material per color and reuse it
            var material = new Material(sharedMaterial);
            material.color = color;
            renderer.sharedMaterial = material;

            // Add Rigidbody for projectile movement ONLY
            var rb = sphere.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true;
            rb.collisionDetectionMode = CollisionDetectionMode.Discrete;

            // Add poolable component
            sphere.AddComponent<SimplePoolableObject>();

            return sphere;
        }

        /// <summary>
        /// Initializes all pools using different configuration strategies.
        /// </summary>
        void InitializePools()
        {
            // Basic pool - standard configuration
            var basicRequest = PoolRequest.Create(BasicSpherePrefab, basicPoolSize);
            PoolingManager.Instance.GetOrCreatePool<SimplePoolableObject>(basicRequest);

            // Projectile pool - high performance, pre-warmed
            var projectileRequest = PoolRequest.CreateHighPerformance(
                ProjectilePrefab,
                initialSize: projectilePoolSize,
                maxSize: 100
            );
            PoolingManager.Instance.GetOrCreatePool<SimplePoolableObject>(projectileRequest);

            // VFX pool - event-based timing
            var vfxRequest = PoolRequest.CreateForEvent(VFXPrefab, "VFX", vfxPoolSize);
            PoolingManager.Instance.GetOrCreatePool<SimplePoolableObject>(vfxRequest);

            // Particle pool - memory efficient, lazy initialization
            var particleRequest = PoolRequest.CreateMemoryEfficient(ParticlePrefab);
            particleRequest.initialPoolSize = particlePoolSize; // Use configured size
            PoolingManager.Instance.GetOrCreatePool<SimplePoolableObject>(particleRequest);

            Debug.Log($"PoolMaster Demo: Initialized 4 pools");
        }

        void OnDestroy()
        {
            // Cleanup prefabs
            if (BasicSpherePrefab != null)
                Destroy(BasicSpherePrefab);
            if (ProjectilePrefab != null)
                Destroy(ProjectilePrefab);
            if (VFXPrefab != null)
                Destroy(VFXPrefab);
            if (ParticlePrefab != null)
                Destroy(ParticlePrefab);
        }
    }
}
