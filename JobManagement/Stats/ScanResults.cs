using System;
using System.Collections.Generic;
using System.IO;

namespace JobManagement.Stats
{
    public class ScanResults
    {
        //compression and file sizes
        public List<long> ZippedSizes = new();
        public List<long> NCCSizes = new();
        public List<long> RawSizes = new();


        //primes in files
        public List<long> PrimesPerFiles = new();


        //primes statistics
        public List<PrimeDensity> PrimeDensities = new();
        //public List<ulong> primeGaps = new();
        public List<TwinPrimes> TwinPrimes = new();



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


        public double AverageZippedSize() => TotalZippedSize() / (double)ZippedSizes.Count;
        public double AverageNCCSize() => TotalNCCSize() / (double)NCCSizes.Count;
        public double AverageRawSize() => TotalRawSize() / (double)RawSizes.Count;


        public double AverageZippedRatio() => TotalZippedSize() / (double)TotalRawSize();
        public double AverageNCCRatio() => TotalNCCSize() / (double)TotalRawSize();
        #endregion

        #region Primes in files
        public long TotalPrimeCount()
        {
            long total = 0;

            for (int i = 0; i < PrimesPerFiles.Count; i++)
                total += PrimesPerFiles[i];

            return total;
        }

        public double AveragePrimesPerFile() => TotalPrimeCount() / (double)PrimesPerFiles.Count;
        #endregion

        #region Primes statistics
        public long TotalTwinPrimes() => TwinPrimes.Count;

        public double AveragePrimeDensity()
        {
            double total = 0;

            for (int i = 0; i < PrimeDensities.Count; i++)
                total += PrimeDensities[i].Density;

            return total / PrimeDensities.Count;
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
        public static void Serialize(Stream stream, ScanResults results)
        {
            byte[] buffer;

            Console.WriteLine(stream.Length);

            stream.Write(BitConverter.GetBytes(results.ZippedSizes.Count), 0, 4);
            buffer = new byte[results.ZippedSizes.Count * 8];
            Buffer.BlockCopy(results.ZippedSizes.ToArray(), 0, buffer, 0, buffer.Length);
            stream.Write(buffer, 0, buffer.Length);

            Console.WriteLine($"{results.ZippedSizes.Count} {stream.Length}");

            stream.Write(BitConverter.GetBytes(results.NCCSizes.Count), 0, 4);
            buffer = new byte[results.NCCSizes.Count * 8];
            Buffer.BlockCopy(results.NCCSizes.ToArray(), 0, buffer, 0, buffer.Length);
            stream.Write(buffer, 0, buffer.Length);

            Console.WriteLine($"{results.NCCSizes.Count} {stream.Length}");

            stream.Write(BitConverter.GetBytes(results.RawSizes.Count), 0, 4);
            buffer = new byte[results.RawSizes.Count * 8];
            Buffer.BlockCopy(results.RawSizes.ToArray(), 0, buffer, 0, buffer.Length);
            stream.Write(buffer, 0, buffer.Length);

            Console.WriteLine($"{results.RawSizes.Count} {stream.Length}");

            stream.Write(BitConverter.GetBytes(results.PrimesPerFiles.Count), 0, 4);
            buffer = new byte[results.PrimesPerFiles.Count * 8];
            Buffer.BlockCopy(results.PrimesPerFiles.ToArray(), 0, buffer, 0, buffer.Length);
            stream.Write(buffer, 0, buffer.Length);

            Console.WriteLine($"{results.PrimesPerFiles.Count} {stream.Length}");

            stream.Write(BitConverter.GetBytes(results.PrimeDensities.Count), 0, 4);
            foreach (PrimeDensity dens in results.PrimeDensities)
                stream.Write(PrimeDensity.Serialize(dens), 0, PrimeDensity.size);

            Console.WriteLine($"{results.PrimeDensities.Count} {stream.Length}");

            stream.Write(BitConverter.GetBytes(results.TwinPrimes.Count), 0, 4);
            foreach (TwinPrimes twins in results.TwinPrimes)
                stream.Write(Stats.TwinPrimes.Serialize(twins), 0, 8);

            Console.WriteLine($"{results.TwinPrimes.Count} {stream.Length}");

            stream.Flush();

            Console.WriteLine(stream.Length);
        }

        public static ScanResults Deserialize(Stream stream)
        {
            byte[] buffer;
            ScanResults ret = new();

            buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            long[] ZippedSizes = new long[BitConverter.ToInt32(buffer, 0)];
            buffer = new byte[ZippedSizes.Length * 8];
            stream.Read(buffer, 0, buffer.Length);
            Buffer.BlockCopy(buffer, 0, ZippedSizes, 0, buffer.Length);
            ret.ZippedSizes = new List<long>(ZippedSizes);

            buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            long[] NCCSizes = new long[BitConverter.ToInt32(buffer, 0)];
            buffer = new byte[NCCSizes.Length * 8];
            stream.Read(buffer, 0, buffer.Length);
            Buffer.BlockCopy(buffer, 0, NCCSizes, 0, buffer.Length);
            ret.NCCSizes = new List<long>(NCCSizes);

            buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            long[] RawSizes = new long[BitConverter.ToInt32(buffer, 0)];
            buffer = new byte[RawSizes.Length * 8];
            stream.Read(buffer, 0, buffer.Length);
            Buffer.BlockCopy(buffer, 0, RawSizes, 0, buffer.Length);
            ret.RawSizes = new List<long>(RawSizes);

            buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            long[] PrimesPerFiles = new long[BitConverter.ToInt32(buffer, 0)];
            buffer = new byte[PrimesPerFiles.Length * 8];
            stream.Read(buffer, 0, buffer.Length);
            Buffer.BlockCopy(buffer, 0, PrimesPerFiles, 0, buffer.Length);
            ret.PrimesPerFiles = new List<long>(PrimesPerFiles);

            buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            PrimeDensity[] PrimeDensities = new PrimeDensity[BitConverter.ToInt32(buffer, 0)];
            buffer = new byte[PrimeDensity.size];
            for (int i = 0; i < PrimeDensities.Length; i++)
            {
                stream.Read(buffer, 0, buffer.Length);
                PrimeDensities[i] = PrimeDensity.Deserialize(buffer);
            }
            ret.PrimeDensities = new List<PrimeDensity>(PrimeDensities);

            buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            TwinPrimes[] TwinPrimes = new TwinPrimes[BitConverter.ToInt32(buffer, 0)];
            buffer = new byte[8];
            for (int i = 0; i < TwinPrimes.Length; i++)
            {
                stream.Read(buffer, 0, buffer.Length);
                TwinPrimes[i] = Stats.TwinPrimes.Deserialize(buffer);
            }
            ret.TwinPrimes = new List<TwinPrimes>(TwinPrimes);

            return ret;
        }
        #endregion
    }
}
