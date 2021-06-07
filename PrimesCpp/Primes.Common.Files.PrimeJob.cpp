
#include <iostream>
#include <string>

#include "Primes.Common.Files.PrimeJob.h"

namespace Primes
{
	namespace Common
	{
		namespace Files
		{
			string PrimeJob::Version::ToString()
			{
				return "v" + to_string(major) + "." + to_string(minor) + "." + to_string(patch);
			}

			bool PrimeJob::Version::IsEqual(Version a, Version b)
			{
				if (a.major != b.major || a.minor != b.minor || a.patch != b.patch)
					return false;
				return true;
			}

			bool PrimeJob::Version::IsEqual(Version ver)
			{
				if (major != ver.major || minor != ver.minor || patch != ver.patch)
					return false;
				return true;
			}

			bool PrimeJob::Version::IsLatest(Version ver)
			{
				return IsEqual(ver, PrimeJob_Version_Latest);
			}

			bool PrimeJob::Version::IsLatest()
			{
				return IsEqual(PrimeJob_Version_Latest);
			}

			bool PrimeJob::Version::IsCompatible(Version ver)
			{
				for (Version v : PrimeJob_Version_Compatilbe)
				{
					if (ver.IsEqual(v))
						return true;
				}

				return false;
			}

			bool PrimeJob::Version::IsCompatible()
			{
				for (Version v : PrimeJob_Version_Compatilbe)
				{
					if (IsEqual(v))
						return true;
				}

				return false;
			}
		}
	}
}
