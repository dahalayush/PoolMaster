// ============================================================================
// PoolMaster - Object Pooling System for Unity
// Copyright (c) 2026 Max Thomas Coates
// https://github.com/mistyuk/PoolMaster
// Licensed under MIT License (see LICENSE file for details)
// ============================================================================

using NUnit.Framework;
using System.Collections.Generic;

namespace PoolMaster.Tests
{
    /// <summary>
    /// Tests for CollectionPool - thread-safe generic collection pooling.
    /// </summary>
    public class CollectionPoolTests
    {
        [SetUp]
        public void Setup()
        {
            // Clear all pools before each test
            CollectionPool.ClearAll();
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up after each test
            CollectionPool.ClearAll();
        }

        #region List<T> Tests

        [Test]
        public void GetList_ReturnsEmptyList()
        {
            var list = CollectionPool.GetList<int>();
            
            Assert.IsNotNull(list);
            Assert.AreEqual(0, list.Count);
        }

        [Test]
        public void ReturnList_ClearsList()
        {
            var list = CollectionPool.GetList<int>();
            list.Add(1);
            list.Add(2);
            list.Add(3);
            
            CollectionPool.Return(list);
            
            // Get it back - should be cleared
            var reused = CollectionPool.GetList<int>();
            Assert.AreEqual(0, reused.Count);
        }

        [Test]
        public void ListPool_ReusesInstances()
        {
            var list1 = CollectionPool.GetList<int>();
            var reference = list1;
            
            CollectionPool.Return(list1);
            
            var list2 = CollectionPool.GetList<int>();
            
            // Should be the same instance (reused)
            Assert.AreSame(reference, list2);
        }

        [Test]
        public void ListPool_DoesNotMixTypes()
        {
            var intList = CollectionPool.GetList<int>();
            intList.Add(42);
            CollectionPool.Return(intList);
            
            var stringList = CollectionPool.GetList<string>();
            
            // Should be different type, not mixed
            Assert.AreEqual(0, stringList.Count);
            Assert.IsInstanceOf<List<string>>(stringList);
        }

        #endregion

        #region HashSet<T> Tests

        [Test]
        public void GetHashSet_ReturnsEmptySet()
        {
            var set = CollectionPool.GetHashSet<int>();
            
            Assert.IsNotNull(set);
            Assert.AreEqual(0, set.Count);
        }

        [Test]
        public void ReturnHashSet_ClearsSet()
        {
            var set = CollectionPool.GetHashSet<string>();
            set.Add("foo");
            set.Add("bar");
            
            CollectionPool.Return(set);
            
            var reused = CollectionPool.GetHashSet<string>();
            Assert.AreEqual(0, reused.Count);
        }

        [Test]
        public void HashSetPool_ReusesInstances()
        {
            var set1 = CollectionPool.GetHashSet<int>();
            var reference = set1;
            
            CollectionPool.Return(set1);
            
            var set2 = CollectionPool.GetHashSet<int>();
            
            Assert.AreSame(reference, set2);
        }

        [Test]
        public void HashSetPool_DoesNotMixTypes()
        {
            var intSet = CollectionPool.GetHashSet<int>();
            intSet.Add(123);
            CollectionPool.Return(intSet);
            
            var stringSet = CollectionPool.GetHashSet<string>();
            
            Assert.AreEqual(0, stringSet.Count);
            Assert.IsInstanceOf<HashSet<string>>(stringSet);
        }

        #endregion

        #region Dictionary<K,V> Tests

        [Test]
        public void GetDictionary_ReturnsEmptyDictionary()
        {
            var dict = CollectionPool.GetDictionary<int, string>();
            
            Assert.IsNotNull(dict);
            Assert.AreEqual(0, dict.Count);
        }

        [Test]
        public void ReturnDictionary_ClearsDictionary()
        {
            var dict = CollectionPool.GetDictionary<int, string>();
            dict[1] = "one";
            dict[2] = "two";
            
            CollectionPool.Return(dict);
            
            var reused = CollectionPool.GetDictionary<int, string>();
            Assert.AreEqual(0, reused.Count);
        }

        [Test]
        public void DictionaryPool_ReusesInstances()
        {
            var dict1 = CollectionPool.GetDictionary<int, string>();
            var reference = dict1;
            
            CollectionPool.Return(dict1);
            
            var dict2 = CollectionPool.GetDictionary<int, string>();
            
            Assert.AreSame(reference, dict2);
        }

        [Test]
        public void DictionaryPool_DoesNotMixTypes()
        {
            var intStringDict = CollectionPool.GetDictionary<int, string>();
            intStringDict[1] = "test";
            CollectionPool.Return(intStringDict);
            
            var stringIntDict = CollectionPool.GetDictionary<string, int>();
            
            Assert.AreEqual(0, stringIntDict.Count);
            Assert.IsInstanceOf<Dictionary<string, int>>(stringIntDict);
        }

        #endregion

        #region Pool Management Tests

        [Test]
        public void GetTotalPooledCount_InitiallyZero()
        {
            // Sanity: count starts at 0
            Assert.AreEqual(0, CollectionPool.GetTotalPooledCount());
        }

        [Test]
        public void GetTotalPooledCount_IncrementsOnReturn()
        {
            // Sanity: returning increases count
            var initialCount = CollectionPool.GetTotalPooledCount();
            var list = CollectionPool.GetList<int>();
            CollectionPool.Return(list);
            
            Assert.Greater(CollectionPool.GetTotalPooledCount(), initialCount);
        }



        [Test]
        public void ClearAll_ResetsCount()
        {
            var list = CollectionPool.GetList<int>();
            CollectionPool.Return(list);
            
            CollectionPool.ClearAll();
            
            Assert.AreEqual(0, CollectionPool.GetTotalPooledCount());
        }

        [Test]
        public void ReturnNull_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => CollectionPool.Return((List<int>)null));
            Assert.DoesNotThrow(() => CollectionPool.Return((HashSet<string>)null));
            Assert.DoesNotThrow(() => CollectionPool.Return((Dictionary<int, string>)null));
        }

        #endregion

        #region Sanity Tests

        [Test]
        public void MultipleGetsAndReturns_Sanity()
        {
            // Sanity check: multiple sequential operations don't corrupt state
            var lists = new List<List<int>>();
            
            for (int i = 0; i < 10; i++)
            {
                lists.Add(CollectionPool.GetList<int>());
            }
            
            foreach (var list in lists)
            {
                list.Add(42);
                CollectionPool.Return(list);
            }
            
            // Should have returned all items
            Assert.Greater(CollectionPool.GetTotalPooledCount(), 0);
        }

        #endregion
    }
}
