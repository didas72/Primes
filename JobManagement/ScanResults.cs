using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace JobManagement
{
    public class ScanResults
    {
        //compression and file sizes
        public List<long> ZippedSizes = new List<long>();
        public List<long> NCCSizes = new List<long>();
        public List<long> RawSizes = new List<long>();


        //primes in files
        public List<long> PrimesPerFiles = new List<long>();


        //primes statistics
        public List<PrimeDensity> PrimeDensities = new List<PrimeDensity>();
        //public List<ulong> primeGaps = new List<ulong>();
        public List<TwinPrimes> TwinPrimes = new List<TwinPrimes>();



        #region Data analysis functions

        #region Compression
        public long TotalZippedSize()
        {
            long total = 0;

            for (int i = 0; i < ZippedSizes.Count; i++)
                total += ZippedSizes[i];

            return total;
        }
        public long TotalNCCSize()
        {
            long total = 0;

            for (int i = 0; i < NCCSizes.Count; i++)
                total += NCCSizes[i];

            return total;
        }
        public long TotalRawSize()
        {
            long total = 0;

            for (int i = 0; i < RawSizes.Count; i++)
                total += RawSizes[i];

            return total;
        }


        public double AverageZippedSize() => (double)TotalZippedSize() / (double)ZippedSizes.Count;
        public double AverageNCCSize() => (double)TotalNCCSize() / (double)NCCSizes.Count;
        public double AverageRawSize() => (double)TotalRawSize() / (double)RawSizes.Count;


        public double AverageZippedRatio() => (double)TotalZippedSize() / (double)TotalRawSize();
        public double AverageNCCRatio() => (double)TotalNCCSize() / (double)TotalRawSize();
        #endregion

        #region Primes in files
        public long TotalPrimeCount()
        {
            long total = 0;

            for (int i = 0; i < PrimesPerFiles.Count; i++)
                total += PrimesPerFiles[i];

            return total;
        }

        public double AveragePrimesPerFile() => (double)TotalPrimeCount() / (double)PrimesPerFiles.Count;
        #endregion

        #region Primes statistics
        public long TotalTwinPrimes() => TwinPrimes.Count;

        public double AveragePrimeDensity()
        {
            double total = 0;

            for (int i = 0; i < PrimeDensities.Count; i++)
                total += PrimeDensities[i].Density;

            return total / (double)PrimeDensities.Count;
        }

        /*public double AveragePrimeGap()
        {
            ulong total = 0;

            for (int i = 0; i < primeGaps.Count; i++)
                total += (ulong)primeGaps[i];

            return (double)total / (double)primeGaps.Count;
        }*/
        #endregion

        #endregion



        #region IO
        public static void Serialize(Stream destination, ScanResults results)
        {
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(destination, results);
        }

        public static ScanResults Deserialize(Stream source)
        {
            BinaryFormatter bf = new BinaryFormatter();
            return (ScanResults)bf.Deserialize(source);
        }
        /*public byte[] Serialize()
        {
            List<byte> bytes = new List<byte>();
            byte[] buffer;

            //bytes.AddRange(BitConverter.GetBytes());
            bytes.AddRange(BitConverter.GetBytes(zippedTotalSize));
            bytes.AddRange(BitConverter.GetBytes(NCCTotalSize));
            bytes.AddRange(BitConverter.GetBytes(rawTotalSize));

            buffer = new byte[ZippedSizes.Count * sizeof(long)];
            Buffer.BlockCopy(ZippedSizes.ToArray(), 0, buffer, 0, buffer.Length);
            bytes.AddRange(buffer);

            buffer = new byte[NCCSizes.Count * sizeof(long)];
            Buffer.BlockCopy(NCCSizes.ToArray(), 0, buffer, 0, buffer.Length);
            bytes.AddRange(buffer);

            buffer = new byte[RawSizes.Count * sizeof(long)];
            Buffer.BlockCopy(RawSizes.ToArray(), 0, buffer, 0, buffer.Length);
            bytes.AddRange(buffer);

            bytes.AddRange(BitConverter.GetBytes(averageZippedSize));
            bytes.AddRange(BitConverter.GetBytes(averageNCCSize));
            bytes.AddRange(BitConverter.GetBytes(averageRawSize));

            bytes.AddRange(BitConverter.GetBytes(averageZippedRatio));
            bytes.AddRange(BitConverter.GetBytes(averageNCCRatio));

            bytes.AddRange(BitConverter.GetBytes(totalPrimeCount));

            buffer = new byte[PrimesPerFiles.Count * sizeof(long)];
            Buffer.BlockCopy(PrimesPerFiles.ToArray(), 0, buffer, 0, buffer.Length);
            bytes.AddRange(buffer);

            bytes.AddRange(BitConverter.GetBytes(averagePrimesPerFile));

            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, PrimeDensities);
            Buffer.BlockCopy(PrimesPerFiles.ToArray(), 0, buffer, 0, buffer.Length);
            bytes.AddRange(buffer);

            throw new NotImplementedException();
            return bytes.ToArray();
        }*/
        #endregion
    }
}
