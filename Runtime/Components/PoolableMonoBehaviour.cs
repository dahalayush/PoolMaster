// ============================================================================
// PoolMaster - Object Pooling System for Unity
// Copyright (c) 2026 Max Thomas Coates
// https://github.com/mistyuk/PoolMaster
// Licensed under MIT License (see LICENSE file for details)
// ============================================================================

using UnityEngine;

namespace PoolMaster
{
    /// <summary>
    /// Lightweight base class for pooled MonoBehaviours with component caching and zero-allocation cleanup patterns.
    /// </summary>
    public abstract class PoolableMonoBehaviour : MonoBehaviour, IPoolable
    {
        #region Serialized Configuration

        [Header("Pooling Behavior")]
        [SerializeField]
        private bool resetTransformOnDespawn = false;

        [SerializeField]
        private bool clearTrailsOnDespawn = true;

        [SerializeField]
        private bool stopAndClearParticlesOnDespawn = true;

        [SerializeField]
        private bool sleepRigidbodiesOnDespawn = true;

        #endregion

        #region Cached Components

        // Cached once on first access, no allocations in hot paths
        private TrailRenderer[] trails;
        private ParticleSystem[] particles;
        private Rigidbody rb;
        private Rigidbody2D rb2d;
        private AudioSource audioSource;

        // Caching flags to avoid repeated GetComponent calls
        private bool componentsCached;

        // Flag to track if object is currently spawned from pool (cheap IsPooled check)
        private bool _activeInPool;

        #endregion

        #region IPoolable Implementation

        /// <summary>
        /// Gets or sets the pool that owns this object. Set by the pool system.
        /// </summary>
        public IPool ParentPool { get; set; }

        /// <summary>
        /// Gets whether this object is currently active in a pool.
        /// </summary>
        public bool IsPooled => _activeInPool;

        /// <summary>
        /// Called when the object is spawned from the pool. Override to add custom spawn behavior, but call base.OnSpawned().
        /// </summary>
        public virtual void OnSpawned()
        {
            _activeInPool = true;
            EnsureComponentsCached();
        }

        /// <summary>
        /// Called when the object is returned to the pool. Override to add custom despawn behavior, but call base.OnDespawned() first.
        /// </summary>
        public virtual void OnDespawned()
        {
            EnsureComponentsCached();

            // Stop audio immediately while still active
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            // Clear trails if configured
            if (clearTrailsOnDespawn)
            {
                ClearTrails();
            }

            // Stop and clear particles if configured
            if (stopAndClearParticlesOnDespawn)
            {
                StopAndClearParticles();
            }

            // Clear the active in pool flag after cleanup
            _activeInPool = false;
        }

        /// <summary>
        /// Called after deactivation to reset the object to default state. Override to add custom reset behavior, but call base.PoolReset() last.
        /// </summary>
        public virtual void PoolReset()
        {
            EnsureComponentsCached();

            // Reset rigidbodies if configured
            if (sleepRigidbodiesOnDespawn)
            {
                ResetRigidbodies();
            }

            // Reset transform if configured
            if (resetTransformOnDespawn)
            {
                ResetLocalTransform();
            }
        }

        #endregion

        #region Component Caching

        /// <summary>
        /// Caches common components once to avoid repeated GetComponent calls.
        /// Note: GetComponentsInChildren allocates arrays. If prefab has no trails/particles, uses Array.Empty for zero allocation.
        /// Called automatically on first spawn.
        /// </summary>
        protected void EnsureComponentsCached()
        {
            if (componentsCached)
                return;

            // Cache trail renderers (use Array.Empty if none exist for zero allocation)
            var trailArray = GetComponentsInChildren<TrailRenderer>();
            trails = trailArray.Length > 0 ? trailArray : System.Array.Empty<TrailRenderer>();

            // Cache particle systems (use Array.Empty if none exist for zero allocation)
            var particleArray = GetComponentsInChildren<ParticleSystem>();
            particles = particleArray.Length > 0 ? particleArray : System.Array.Empty<ParticleSystem>();

            // Cache rigidbody (3D or 2D, whichever exists)
            rb = GetComponent<Rigidbody>();
            rb2d = GetComponent<Rigidbody2D>();

            // Cache audio source
            audioSource = GetComponent<AudioSource>();

            componentsCached = true;
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Clears all cached trail renderers. Zero allocation, safe to call multiple times.
        /// </summary>
        protected void ClearTrails()
        {
            if (trails == null)
                return;

            for (int i = 0; i < trails.Length; i++)
            {
                if (trails[i] != null)
                {
                    trails[i].Clear();
                }
            }
        }

        /// <summary>
        /// Stops and clears all cached particle systems using immediate stop behavior.
        /// Note: Simulate() can be expensive on effect-heavy prefabs. Disable stopAndClearParticlesOnDespawn if needed.
        /// </summary>
        protected void StopAndClearParticles()
        {
            if (particles == null)
                return;

            for (int i = 0; i < particles.Length; i++)
            {
                if (particles[i] != null)
                {
                    // Stop emitting and clear existing particles immediately
                    particles[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

                    // Force simulation to clear any remaining particles
                    particles[i].Simulate(0f, true, true);
                }
            }
        }

        /// <summary>
        /// Resets rigidbody velocities and puts them to sleep. Handles both 2D and 3D rigidbodies.
        /// </summary>
        protected void ResetRigidbodies()
        {
            // Reset 3D rigidbody
            if (rb != null)
            {
                // Only set velocities on non-kinematic rigidbodies
                if (!rb.isKinematic)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    rb.Sleep();
                }
            }

            // Reset 2D rigidbody
            if (rb2d != null)
            {
                // Only set velocities on non-kinematic rigidbodies (use bodyType for 2D)
                if (rb2d.bodyType != RigidbodyType2D.Kinematic)
                {
                    rb2d.linearVelocity = Vector2.zero;
                    rb2d.angularVelocity = 0f;
                    rb2d.Sleep();
                }
            }
        }

        /// <summary>
        /// Resets local transform to default values (position, rotation, and scale to identity).
        /// </summary>
        protected void ResetLocalTransform()
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        /// <summary>
        /// Gets the cached 3D rigidbody. Returns null if not present.
        /// </summary>
        protected Rigidbody CachedRigidbody
        {
            get
            {
                EnsureComponentsCached();
                return rb;
            }
        }

        /// <summary>
        /// Gets the cached 2D rigidbody. Returns null if not present.
        /// </summary>
        protected Rigidbody2D CachedRigidbody2D
        {
            get
            {
                EnsureComponentsCached();
                return rb2d;
            }
        }

        /// <summary>
        /// Gets the cached audio source. Returns null if not present.
        /// </summary>
        protected AudioSource CachedAudioSource
        {
            get
            {
                EnsureComponentsCached();
                return audioSource;
            }
        }

        /// <summary>
        /// Gets the cached particle systems array. Returns empty array if none present.
        /// </summary>
        protected ParticleSystem[] CachedParticleSystems
        {
            get
            {
                EnsureComponentsCached();
                return particles ?? System.Array.Empty<ParticleSystem>();
            }
        }

        /// <summary>
        /// Gets the cached trail renderers array. Returns empty array if none present.
        /// </summary>
        protected TrailRenderer[] CachedTrailRenderers
        {
            get
            {
                EnsureComponentsCached();
                return trails ?? System.Array.Empty<TrailRenderer>();
            }
        }

        #endregion

        #region Unity Lifecycle

        /// <summary>
        /// Unity Awake callback. Override if needed, but component caching now happens on first OnSpawned() to avoid upfront cost during prewarming.
        /// </summary>
        protected virtual void Awake()
        {
            // Component caching deferred to first OnSpawned() to avoid cost during prewarming
        }

        #endregion
    }
}
