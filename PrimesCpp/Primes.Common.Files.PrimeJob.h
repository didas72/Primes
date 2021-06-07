#pragma once

#include "ShortTypeNames.h"

using namespace std;

namespace Primes
{
	namespace Common
	{
		namespace Files
		{
			class PrimeJob
			{
			public:
				enum Status : byte
				{
					Not_started,
					Started,
					Finished,
					None = 0,
				};




			private:

			public:
				struct Version
				{
				public:
					byte major;
					byte minor;
					byte patch;

					Version(byte Major, byte Minor, byte Patch)
					{
						major = Major; minor = Minor; patch = Patch;
					}

					string ToString();
					static bool IsEqual(Version a, Version b);
					bool IsEqual(Version ver);
					static bool IsLatest(Version ver);
					bool IsLatest();
					static bool IsCompatible(Version ver);
					bool IsCompatible();
				};

				struct Compression
				{
				private:
					byte flags;

				public:
					#define PrimeJob_Version_Default Comp(true, false);

					bool NCC();
					bool ONSS();

					Compression(bool NCC, bool ONSS)
					{
						flags = 0;
						flags = NCC ? (byte)(flags | 0b00000010) : flags;
						flags = ONSS ? (byte)(flags | 0b00000001) : flags;
					}
					Compression(byte source)
					{
						flags = source;
					}

					bool IsCompressed();
					static bool IsCompressed(Compression comp);
					byte GetByte();
					static byte GetByte(Compression comp);
				};
			};

			static const PrimeJob::Version PrimeJob_Version_Zero = Version(0, 0, 0);
			static const PrimeJob::Version PrimeJob_Version_Latest = Version(1, 2, 0);
			static const PrimeJob::Version PrimeJob_Version_Compatilbe[1] = { Version(255, 255, 255) };
		}
	}
}
