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
    /// Example pooled projectile implementation showing best practices for physics-based objects.
    /// </summary>
    public class PooledProjectile : PoolableMonoBehaviour
    {
        #region Configuration

        [Header("Projectile Behavior")]
        [SerializeField]
        private float lifetime = 5f;

        [SerializeField]
        private bool autoReturnOnImpact = true;

        [SerializeField]
        private bool autoReturnOnLifetime = true;

        [SerializeField]
        private LayerMask impactLayers = -1;

        [Header("Physics")]
        [SerializeField]
        private bool useGravity = false;

        [SerializeField]
        private float initialSpeed = 10f;

        #endregion

        #region Runtime State

        private float spawnTime;
        private bool hasImpacted;

        #endregion

        #region Pooling Lifecycle

        public override void OnSpawned()
        {
            base.OnSpawned();

            spawnTime = Time.time;
            hasImpacted = false;

            // Configure physics
            ConfigurePhysics();
        }

        public override void OnDespawned()
        {
            base.OnDespawned();

            // Clear any custom state
            hasImpacted = false;
        }

        public override void PoolReset()
        {
            // Reset projectile-specific state first
            spawnTime = 0f;
            hasImpacted = false;

            // Call base reset last (handles rigidbodies, transform, etc.)
            base.PoolReset();
        }

        #endregion

        #region Unity Lifecycle

        private void Update()
        {
            // Check lifetime expiration
            if (autoReturnOnLifetime && Time.time - spawnTime >= lifetime)
            {
                OnLifetimeExpired();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            HandleImpact(other.gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            HandleImpact(other.gameObject);
        }

        private void OnCollisionEnter(Collision collision)
        {
            HandleImpact(collision.gameObject);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            HandleImpact(collision.gameObject);
        }

        #endregion

        #region Physics Configuration

        private void ConfigurePhysics()
        {
            // Configure 3D rigidbody
            if (CachedRigidbody != null)
            {
                CachedRigidbody.useGravity = useGravity;

                // Apply initial velocity in forward direction
                if (initialSpeed > 0f)
                {
                    CachedRigidbody.linearVelocity = transform.forward * initialSpeed;
                }
            }

            // Configure 2D rigidbody
            if (CachedRigidbody2D != null)
            {
                CachedRigidbody2D.gravityScale = useGravity ? 1f : 0f;

                // Apply initial velocity in up direction (2D forward)
                if (initialSpeed > 0f)
                {
                    CachedRigidbody2D.linearVelocity = transform.up * initialSpeed;
                }
            }
        }

        #endregion

        #region Impact Handling

        private void HandleImpact(GameObject impactTarget)
        {
            if (hasImpacted)
                return;

            // Check if we should react to this layer
            if (((1 << impactTarget.layer) & impactLayers) == 0)
                return;

            hasImpacted = true;

            // Call virtual method for custom behavior
            OnProjectileHit(impactTarget);

            // Auto-return if configured
            if (autoReturnOnImpact)
            {
                ReturnToPool();
            }
        }

        /// <summary>
        /// Called when the projectile hits a valid target.
        /// Override this method to implement custom impact behavior.
        /// </summary>
        /// <param name="target">The GameObject that was hit</param>
        protected virtual void OnProjectileHit(GameObject target)
        {
            // Default behavior: do nothing
            // Override in derived classes for damage, effects, etc.
        }

        /// <summary>
        /// Called when the projectile's lifetime expires.
        /// Override this method to implement custom expiration behavior.
        /// </summary>
        protected virtual void OnLifetimeExpired()
        {
            ReturnToPool();
        }

        #endregion

        #region Public Interface

        /// <summary>
        /// Launch the projectile with specified velocity.
        /// Call this after spawning to override initial speed configuration.
        /// </summary>
        /// <param name="velocity">The velocity vector to apply</param>
        public void LaunchWithVelocity(Vector3 velocity)
        {
            if (CachedRigidbody != null)
            {
                CachedRigidbody.linearVelocity = velocity;
            }
            else if (CachedRigidbody2D != null)
            {
                CachedRigidbody2D.linearVelocity = velocity;
            }
        }

        /// <summary>
        /// Return this projectile to its pool.
        /// Safe to call multiple times.
        /// </summary>
        public void ReturnToPool()
        {
            if (IsPooled)
            {
                gameObject.ReturnToPool();
            }
        }

        /// <summary>
        /// Get the time remaining before this projectile expires.
        /// </summary>
        public float TimeRemaining => Mathf.Max(0f, lifetime - (Time.time - spawnTime));

        /// <summary>
        /// Whether this projectile has already impacted something.
        /// </summary>
        public bool HasImpacted => hasImpacted;

        #endregion
    }
}
