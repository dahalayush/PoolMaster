// ============================================================================
// PoolMaster - Object Pooling System for Unity
// Copyright (c) 2026 Max Thomas Coates
// https://github.com/mistyuk/PoolMaster
// Licensed under MIT License (see LICENSE file for details)
// ============================================================================

using System;
using System.Diagnostics;

namespace PoolMaster
{
    /// <summary>
    /// Lightweight logging utility for the pooling system that compiles out in release builds.
    /// Uses conditional compilation to eliminate all logging overhead when disabled.
    ///
    /// To enable logs, add "ENABLE_POOL_LOGS" to your project's Scripting Define Symbols.
    /// This prevents hidden GC allocations from string interpolation when logs are disabled.
    ///
    /// For performance-critical hotspots, use the Func&lt;string&gt; overloads for zero-allocation
    /// lazy logging: PoolLog.Info(() => $"expensive {interpolation}");
    /// </summary>
    public static class PoolLog
    {
        /// <summary>
        /// Log an informational message. Compiles out completely when ENABLE_POOL_LOGS is not defined.
        /// </summary>
        /// <param name="message">The message to log</param>
        [Conditional("ENABLE_POOL_LOGS")]
        public static void Info(string message)
        {
            UnityEngine.Debug.Log($"[Pool] {message}");
        }

        /// <summary>
        /// Log a warning message. Compiles out completely when ENABLE_POOL_LOGS is not defined.
        /// </summary>
        /// <param name="message">The warning message to log</param>
        [Conditional("ENABLE_POOL_LOGS")]
        public static void Warn(string message)
        {
            UnityEngine.Debug.LogWarning($"[Pool] {message}");
        }

        /// <summary>
        /// Log an error message. Compiles out completely when ENABLE_POOL_LOGS is not defined.
        /// </summary>
        /// <param name="message">The error message to log</param>
        [Conditional("ENABLE_POOL_LOGS")]
        public static void Error(string message)
        {
            UnityEngine.Debug.LogError($"[Pool] {message}");
        }

        /// <summary>
        /// Log a verbose/debug message. Compiles out completely when ENABLE_POOL_LOGS is not defined.
        /// Use this for very frequent logging that might impact performance.
        /// </summary>
        /// <param name="message">The debug message to log</param>
        [Conditional("ENABLE_POOL_LOGS")]
        public static void Debug(string message)
        {
            UnityEngine.Debug.Log($"[Pool:Debug] {message}");
        }

        /// <summary>
        /// Zero-allocation lazy logging for info messages. The message factory is only called
        /// when ENABLE_POOL_LOGS is defined, completely eliminating string interpolation overhead
        /// when logging is disabled.
        /// </summary>
        /// <param name="messageFactory">Factory function that creates the message string</param>
        [Conditional("ENABLE_POOL_LOGS")]
        public static void Info(Func<string> messageFactory)
        {
            UnityEngine.Debug.Log($"[Pool] {messageFactory()}");
        }

        /// <summary>
        /// Zero-allocation lazy logging for warning messages. The message factory is only called
        /// when ENABLE_POOL_LOGS is defined, completely eliminating string interpolation overhead
        /// when logging is disabled.
        /// </summary>
        /// <param name="messageFactory">Factory function that creates the message string</param>
        [Conditional("ENABLE_POOL_LOGS")]
        public static void Warn(Func<string> messageFactory)
        {
            UnityEngine.Debug.LogWarning($"[Pool] {messageFactory()}");
        }

        /// <summary>
        /// Zero-allocation lazy logging for error messages. The message factory is only called
        /// when ENABLE_POOL_LOGS is defined, completely eliminating string interpolation overhead
        /// when logging is disabled.
        /// </summary>
        /// <param name="messageFactory">Factory function that creates the message string</param>
        [Conditional("ENABLE_POOL_LOGS")]
        public static void Error(Func<string> messageFactory)
        {
            UnityEngine.Debug.LogError($"[Pool] {messageFactory()}");
        }

        /// <summary>
        /// Zero-allocation lazy logging for debug messages. The message factory is only called
        /// when ENABLE_POOL_LOGS is defined, completely eliminating string interpolation overhead
        /// when logging is disabled.
        /// </summary>
        /// <param name="messageFactory">Factory function that creates the message string</param>
        [Conditional("ENABLE_POOL_LOGS")]
        public static void Debug(Func<string> messageFactory)
        {
            UnityEngine.Debug.Log($"[Pool:Debug] {messageFactory()}");
        }

        /// <summary>
        /// Check if pool logging is currently enabled.
        /// This method itself has no overhead - it's a compile-time constant.
        /// </summary>
        /// <returns>True if ENABLE_POOL_LOGS is defined, false otherwise</returns>
        public static bool IsEnabled()
        {
#if ENABLE_POOL_LOGS
            return true;
#else
            return false;
#endif
        }
    }
}
