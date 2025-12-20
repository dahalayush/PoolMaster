// ============================================================================
// PoolMaster - Object Pooling System for Unity
// Copyright (c) 2026 Max Thomas Coates
// https://github.com/mistyuk/PoolMaster
// Licensed under MIT License (see LICENSE file for details)
// ============================================================================

using UnityEngine;

namespace PoolMaster.NoCode
{
    /// <summary>
    /// Marks this object as pooled by PoolMaster. This is automatically added to pooled objects.
    /// Shows helpful information about the object's pool status.
    /// </summary>
    [AddComponentMenu("PoolMaster/PoolMaster Pooled Object")]
    [DisallowMultipleComponent]
    public class PoolMasterPooledObject : MonoBehaviour
    {
        [Header("Pool Information (Read-Only)")]
        [Tooltip("Whether this object is currently spawned from a pool.")]
        [SerializeField]
        private bool isSpawned;

        [Tooltip("The name of the prefab this was created from.")]
        [SerializeField]
        private string prefabName;

        /// <summary>
        /// Gets whether this object is currently active in a pool.
        /// </summary>
        public bool IsSpawned
        {
            get => isSpawned;
            internal set => isSpawned = value;
        }

        /// <summary>
        /// Gets the prefab name this object was created from.
        /// </summary>
        public string PrefabName
        {
            get => prefabName;
            internal set => prefabName = value;
        }

        void OnEnable()
        {
            isSpawned = true;
        }

        void OnDisable()
        {
            isSpawned = false;
        }

        /// <summary>
        /// Returns this object to its pool immediately.
        /// </summary>
        public void ReturnToPool()
        {
            if (PoolMasterManager.Instance != null)
            {
                PoolMasterManager.Instance.ReturnToPool(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
