using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

using Primes.Common;

namespace UnitTestsPrimes.Common
{
    [TestClass]
    public class MathfTests
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

        [TestMethod]
        [DataRow((ulong)0, (ulong)0)]
        [DataRow((ulong)1, (ulong)1)]
        [DataRow((ulong)2, (ulong)2)]
        public void SpecialValues_UlongSqrtHigh_ReturnValid(ulong value, ulong returnV)
        {
            //arrange
            ulong ret;

            //act
            ret = Mathf.UlongSqrtHigh(value);

            //assert
            Assert.AreEqual(ret, returnV);
        }

        [TestMethod]
        [DataRow((ulong)3, (ulong)2)]
        [DataRow((ulong)5, (ulong)3)]
        [DataRow((ulong)100, (ulong)10)]
        public void LowValues_UlongSqrtHigh_ReturnValid(ulong value, ulong returnV)
        {
            //arrange
            ulong ret;

            //act
            ret = Mathf.UlongSqrtHigh(value);

            //assert
            Assert.AreEqual(ret, returnV);
        }

        [TestMethod]
        [DataRow((ulong)18446744073709551615, (ulong)4294967296)]
        public void HighValues_UlongSqrtHigh_ReturnValid(ulong value, ulong returnV)
        {
            //arrange
            ulong ret;

            //act
            ret = Mathf.UlongSqrtHigh(value);

            //assert
            Assert.AreEqual(returnV, ret);
        }
        
    }
}
