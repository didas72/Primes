using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

using Primes.Common;

namespace UnitTestsPrimes
{
    [TestClass]
    public class PrimesTests
    {
        [TestMethod]
        [DataRow((ulong)2)]
        [DataRow((ulong)11)]
        [DataRow((ulong)1203117401197)]
        public void PrimesSimple_PrimesNumbers_ReturnTrue(ulong value)
        {
            //arrange
            bool isPrime;

            //act
            isPrime = Mathf.IsPrime(value);

            //assert
            Assert.IsTrue(isPrime);
        }

        [TestMethod]
        [DataRow((ulong)0)]
        [DataRow((ulong)1)]
        [DataRow((ulong)1203117401198)]
        public void PrimesSimple_PrimesNumbers_ReturnFalse(ulong value)
        {
            //arrange
            bool isPrime;

            //act
            isPrime = Mathf.IsPrime(value);

            //assert
            Assert.IsFalse(isPrime);
        }
    }
}
