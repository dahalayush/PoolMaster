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
    /// Spawns objects from a pool without needing any code.
    /// Add this to any GameObject and configure when/where to spawn.
    /// </summary>
    [AddComponentMenu("PoolMaster/PoolMaster Spawner")]
    public class PoolMasterSpawner : MonoBehaviour
    {
        [Header("What to Spawn")]
        [Tooltip("The prefab to spawn from the pool. Make sure this prefab is added to PoolMaster Manager's pool list.")]
        [SerializeField]
        private GameObject prefabToSpawn;

        [Header("Where to Spawn")]
        [Tooltip("Where the spawned object appears.")]
        [SerializeField]
        private SpawnPositionMode positionMode = SpawnPositionMode.AtThisObject;

        [Tooltip("Specific location to spawn at (used when Position Mode is 'At Target').")]
        [SerializeField]
        private Transform spawnTarget;

        [Tooltip("Random area size (used when Position Mode is 'Random In Area').")]
        [SerializeField]
        private Vector3 randomAreaSize = new Vector3(5f, 0f, 5f);

        [Tooltip("Offset from the spawn position.")]
        [SerializeField]
        private Vector3 spawnOffset = Vector3.zero;

        [Header("Rotation")]
        [Tooltip("How the spawned object is rotated.")]
        [SerializeField]
        private SpawnRotationMode rotationMode = SpawnRotationMode.Identity;

        [Tooltip("Custom rotation to use (when Rotation Mode is 'Custom').")]
        [SerializeField]
        private Vector3 customRotation = Vector3.zero;

        [Header("When to Spawn")]
        [Tooltip("When this spawner creates objects.")]
        [SerializeField]
        private SpawnTrigger spawnOn = SpawnTrigger.Manual;

        [Tooltip("Delay before spawning (in seconds).")]
        [SerializeField]
        private float spawnDelay = 0f;

        [Tooltip("How many objects to spawn each time.")]
        [SerializeField]
        [Range(1, 100)]
        private int spawnCount = 1;

        [Tooltip("Repeat spawning every X seconds (0 = spawn once).")]
        [SerializeField]
        private float repeatInterval = 0f;

        [Header("Input Settings (For 'On Key Press')")]
        [Tooltip("Key to press for spawning (when Spawn On is 'On Key Press').")]
        [SerializeField]
        private KeyCode spawnKey = KeyCode.Space;

        [Header("Parent Settings")]
        [Tooltip("Where to parent spawned objects (optional).")]
        [SerializeField]
        private Transform spawnParent;

        private float nextSpawnTime;

        void Start()
        {
            if (spawnOn == SpawnTrigger.OnStart)
            {
                SpawnWithDelay();
            }
        }

        void OnEnable()
        {
            if (spawnOn == SpawnTrigger.OnEnable)
            {
                SpawnWithDelay();
            }
        }

        void Update()
        {
            // Handle key press spawning
            if (spawnOn == SpawnTrigger.OnKeyPress && Input.GetKeyDown(spawnKey))
            {
                SpawnWithDelay();
            }

            // Handle repeat spawning
            if (repeatInterval > 0 && Time.time >= nextSpawnTime)
            {
                if (spawnOn == SpawnTrigger.OnStart || spawnOn == SpawnTrigger.OnEnable)
                {
                    SpawnNow();
                    nextSpawnTime = Time.time + repeatInterval;
                }
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (spawnOn == SpawnTrigger.OnTriggerEnter)
            {
                SpawnWithDelay();
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (spawnOn == SpawnTrigger.OnTriggerEnter)
            {
                SpawnWithDelay();
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            if (spawnOn == SpawnTrigger.OnCollision)
            {
                SpawnWithDelay();
            }
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            if (spawnOn == SpawnTrigger.OnCollision)
            {
                SpawnWithDelay();
            }
        }

        /// <summary>
        /// Spawns objects immediately. Call this from other scripts or UI buttons.
        /// </summary>
        public void SpawnNow()
        {
            if (prefabToSpawn == null)
            {
                Debug.LogWarning("[PoolMaster Spawner] No prefab assigned to spawn.", this);
                return;
            }

            if (PoolMasterManager.Instance == null)
            {
                Debug.LogWarning("[PoolMaster Spawner] No PoolMaster Manager found in scene.", this);
                return;
            }

            for (int i = 0; i < spawnCount; i++)
            {
                Vector3 position = GetSpawnPosition();
                Quaternion rotation = GetSpawnRotation();

                GameObject spawned = PoolMasterManager.Instance.Spawn(prefabToSpawn, position, rotation);

                if (spawned != null && spawnParent != null)
                {
                    spawned.transform.SetParent(spawnParent);
                }
            }
        }

        private void SpawnWithDelay()
        {
            if (spawnDelay > 0)
            {
                Invoke(nameof(SpawnNow), spawnDelay);
            }
            else
            {
                SpawnNow();
            }
        }

        private Vector3 GetSpawnPosition()
        {
            Vector3 basePosition = transform.position;

            switch (positionMode)
            {
                case SpawnPositionMode.AtThisObject:
                    basePosition = transform.position;
                    break;

                case SpawnPositionMode.AtTarget:
                    if (spawnTarget != null)
                        basePosition = spawnTarget.position;
                    break;

                case SpawnPositionMode.RandomInArea:
                    basePosition = transform.position + new Vector3(
                        Random.Range(-randomAreaSize.x / 2, randomAreaSize.x / 2),
                        Random.Range(-randomAreaSize.y / 2, randomAreaSize.y / 2),
                        Random.Range(-randomAreaSize.z / 2, randomAreaSize.z / 2)
                    );
                    break;
            }

            return basePosition + spawnOffset;
        }

        private Quaternion GetSpawnRotation()
        {
            switch (rotationMode)
            {
                case SpawnRotationMode.Identity:
                    return Quaternion.identity;

                case SpawnRotationMode.ThisObjectRotation:
                    return transform.rotation;

                case SpawnRotationMode.TargetRotation:
                    return spawnTarget != null ? spawnTarget.rotation : Quaternion.identity;

                case SpawnRotationMode.Custom:
                    return Quaternion.Euler(customRotation);

                case SpawnRotationMode.Random:
                    return Quaternion.Euler(
                        Random.Range(0f, 360f),
                        Random.Range(0f, 360f),
                        Random.Range(0f, 360f)
                    );

                default:
                    return Quaternion.identity;
            }
        }

        // Gizmo to show spawn area
        void OnDrawGizmosSelected()
        {
            if (!enabled) return;
            
            if (positionMode == SpawnPositionMode.RandomInArea)
            {
                Gizmos.color = new Color(0, 1, 0, 0.3f);
                Gizmos.matrix = Matrix4x4.TRS(transform.position + spawnOffset, Quaternion.identity, Vector3.one);
                Gizmos.DrawCube(Vector3.zero, randomAreaSize);
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(Vector3.zero, randomAreaSize);
            }
            else if (spawnTarget != null || positionMode == SpawnPositionMode.AtThisObject)
            {
                Gizmos.color = Color.cyan;
                Vector3 pos = positionMode == SpawnPositionMode.AtTarget && spawnTarget != null 
                    ? spawnTarget.position + spawnOffset
                    : transform.position + spawnOffset;
                Gizmos.DrawWireSphere(pos, 0.3f);
            }
        }
    }

    public enum SpawnPositionMode
    {
        [Tooltip("Spawn at this GameObject's position.")]
        AtThisObject,

        [Tooltip("Spawn at a specific target's position.")]
        AtTarget,

        [Tooltip("Spawn at a random position within an area.")]
        RandomInArea
    }

    public enum SpawnRotationMode
    {
        [Tooltip("No rotation (0, 0, 0).")]
        Identity,

        [Tooltip("Use this GameObject's rotation.")]
        ThisObjectRotation,

        [Tooltip("Use the target's rotation.")]
        TargetRotation,

        [Tooltip("Use a custom rotation you specify.")]
        Custom,

        [Tooltip("Random rotation on all axes.")]
        Random
    }

    public enum SpawnTrigger
    {
        [Tooltip("Only spawn when you call SpawnNow() from code or a button.")]
        Manual,

        [Tooltip("Spawn once when the scene starts.")]
        OnStart,

        [Tooltip("Spawn when this GameObject is enabled.")]
        OnEnable,

        [Tooltip("Spawn when something enters the trigger collider.")]
        OnTriggerEnter,

        [Tooltip("Spawn when something collides with this object.")]
        OnCollision,

        [Tooltip("Spawn when a key is pressed.")]
        OnKeyPress
    }
}
