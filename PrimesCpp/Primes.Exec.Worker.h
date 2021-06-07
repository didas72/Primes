#pragma once

#include <thread>
#include <iostream>

#include "ShortTypeNames.h"
#include "Primes.Common.Files.PrimeJob.h"

using namespace std;
using namespace Primes;
using namespace Primes::Common;
using namespace Primes::Common::Files;

namespace Primes
{
	namespace Exec
	{
		class Worker
		{
		public:
			thread Thread;

			float Progress;

			uint Batch;

			bool IsWorking();

		private:
			volatile bool doWork = false;
			string dumpPath;
			string jobPath;
			int workerId;
			const int primeBufferSize = 500;

		public:
			Worker(string DumpPath, string JobPath, int WorkerId)
			{
				dumpPath = DumpPath; jobPath = JobPath; workerId = WorkerId;
			}

			void StartWork(PrimeJob job);
			void StopWork();

			void DoWork(PrimeJob job);
		};
	}
}

