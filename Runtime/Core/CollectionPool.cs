// ============================================================================
// PoolMaster - Object Pooling System for Unity
// Copyright (c) 2026 Max Thomas Coates
// https://github.com/mistyuk/PoolMaster
// Licensed under MIT License (see LICENSE file for details)
// ============================================================================

using System;
using System.Collections.Generic;

namespace PoolMaster
{
    /// <summary>
    /// High-performance static pool for collections to reduce allocations.
    /// Thread-safe, intended primarily for main-thread usage. Always return collections when done to prevent leaks.
    /// </summary>
    public static class CollectionPool
    {
        // Per-type pools to prevent type mixing bugs
        private static readonly Dictionary<Type, object> typedPools = new Dictionary<Type, object>();
        private static readonly object poolLock = new object();
        private static int totalPooledCount = 0; // Fast counter without reflection
        
        private const int MaxPoolSize = 16; // Prevent unbounded growth
        
        // ==================== List<T> Pooling ====================
        
        /// <summary>
        /// Gets a cleared List from the pool or creates a new one. Must call Return() when done to prevent leaks.
        /// </summary>
        public static List<T> GetList<T>()
        {
            lock (poolLock)
            {
                var type = typeof(List<T>);
                if (!typedPools.TryGetValue(type, out var poolObj))
                {
                    return new List<T>();
                }
                
                var pool = (Stack<List<T>>)poolObj;
                if (pool.Count > 0)
                {
                    var list = pool.Pop();
                    list.Clear();
                    totalPooledCount--;
                    return list;
                }
            }
            return new List<T>();
        }
        
        /// <summary>
        /// Returns a List to the pool. The list is automatically cleared.
        /// </summary>
        public static void Return<T>(List<T> list)
        {
            if (list == null) return;
            
            list.Clear(); // Clear before pooling to release references
            
            lock (poolLock)
            {
                var type = typeof(List<T>);
                if (!typedPools.TryGetValue(type, out var poolObj))
                {
                    poolObj = new Stack<List<T>>();
                    typedPools[type] = poolObj;
                }
                
                var pool = (Stack<List<T>>)poolObj;
                if (pool.Count < MaxPoolSize)
                {
                    pool.Push(list);
                    totalPooledCount++;
                }
            }
        }
        
        // ==================== HashSet<T> Pooling ====================
        
        /// <summary>
        /// Gets a cleared HashSet from the pool or creates a new one. Must call Return() when done to prevent leaks.
        /// </summary>
        public static HashSet<T> GetHashSet<T>()
        {
            lock (poolLock)
            {
                var type = typeof(HashSet<T>);
                if (!typedPools.TryGetValue(type, out var poolObj))
                {
                    return new HashSet<T>();
                }
                
                var pool = (Stack<HashSet<T>>)poolObj;
                if (pool.Count > 0)
                {
                    var set = pool.Pop();
                    set.Clear();
                    totalPooledCount--;
                    return set;
                }
            }
            return new HashSet<T>();
        }
        
        /// <summary>
        /// Returns a HashSet to the pool. The set is automatically cleared.
        /// </summary>
        public static void Return<T>(HashSet<T> set)
        {
            if (set == null) return;
            
            set.Clear();
            
            lock (poolLock)
            {
                var type = typeof(HashSet<T>);
                if (!typedPools.TryGetValue(type, out var poolObj))
                {
                    poolObj = new Stack<HashSet<T>>();
                    typedPools[type] = poolObj;
                }
                
                var pool = (Stack<HashSet<T>>)poolObj;
                if (pool.Count < MaxPoolSize)
                {
                    pool.Push(set);
                    totalPooledCount++;
                }
            }
        }
        
        // ==================== Dictionary<TKey, TValue> Pooling ====================
        
        /// <summary>
        /// Gets a cleared Dictionary from the pool or creates a new one. Must call Return() when done to prevent leaks.
        /// </summary>
        public static Dictionary<TKey, TValue> GetDictionary<TKey, TValue>()
        {
            lock (poolLock)
            {
                var type = typeof(Dictionary<TKey, TValue>);
                if (!typedPools.TryGetValue(type, out var poolObj))
                {
                    return new Dictionary<TKey, TValue>();
                }
                
                var pool = (Stack<Dictionary<TKey, TValue>>)poolObj;
                if (pool.Count > 0)
                {
                    var dict = pool.Pop();
                    dict.Clear();
                    totalPooledCount--;
                    return dict;
                }
            }
            return new Dictionary<TKey, TValue>();
        }
        
        /// <summary>
        /// Returns a Dictionary to the pool. The dictionary is automatically cleared.
        /// </summary>
        public static void Return<TKey, TValue>(Dictionary<TKey, TValue> dict)
        {
            if (dict == null) return;
            
            dict.Clear();
            
            lock (poolLock)
            {
                var type = typeof(Dictionary<TKey, TValue>);
                if (!typedPools.TryGetValue(type, out var poolObj))
                {
                    poolObj = new Stack<Dictionary<TKey, TValue>>();
                    typedPools[type] = poolObj;
                }
                
                var pool = (Stack<Dictionary<TKey, TValue>>)poolObj;
                if (pool.Count < MaxPoolSize)
                {
                    pool.Push(dict);
                    totalPooledCount++;
                }
            }
        }
        
        // ==================== Diagnostics ====================
        
        /// <summary>
        /// Get total number of pooled collections for debugging/monitoring. O(1) operation using fast counter.
        /// </summary>
        public static int GetTotalPooledCount()
        {
            lock (poolLock)
            {
                return totalPooledCount;
            }
        }
        
        /// <summary>
        /// Clear all pools (useful for scene transitions or testing).
        /// </summary>
        public static void ClearAll()
        {
            lock (poolLock)
            {
                typedPools.Clear();
                totalPooledCount = 0;
            }
        }
    }
}
