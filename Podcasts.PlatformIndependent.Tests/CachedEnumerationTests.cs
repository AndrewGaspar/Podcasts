using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Podcasts.PlatformIndependent.Tests
{
    [TestClass]
    public class CachedEnumerationTests
    {
        static int[] Nums = new[] { 5, 4, 3, 2, 1 };

        [TestMethod]
        public void SingleEnumerationTest()
        {
            var cachedNums = Nums.CacheResults();

            var count = 0;
            foreach(var pair in Nums.Pairwise(cachedNums))
            {
                count++;
                Assert.AreEqual(pair.Item1, pair.Item2);
            }

            Assert.AreEqual(Nums.Length, count);
        }

        [TestMethod]
        public void MultiEnumerationTest()
        {
            var cachedNums = Nums.CacheResults();
            
            foreach (var pair in Nums.Pairwise(cachedNums))
            {
                Assert.AreEqual(pair.Item1, pair.Item2);
            }

            foreach (var pair in Nums.Pairwise(cachedNums))
            {
                Assert.AreEqual(pair.Item1, pair.Item2);
            }

            Assert.AreEqual(Nums.Length, cachedNums.Count());
        }

        [TestMethod]
        public void EarlyTerminationAndReenumerationTest()
        {
            var cachedNums = Nums.CacheResults();

            foreach (var pair in Nums.Pairwise(cachedNums.Take(3)))
            {
                Assert.AreEqual(pair.Item1, pair.Item2);
            }

            foreach (var pair in Nums.Pairwise(cachedNums))
            {
                Assert.AreEqual(pair.Item1, pair.Item2);
            }
            
            Assert.AreEqual(Nums.Length, cachedNums.Count());
        }
    }
}
