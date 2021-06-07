#include "Primes.Exec.Worker.h"
#include "Primes.Common.Files.PrimeJob.h"

using namespace std;

namespace Primes
{
	namespace Exec
	{
		bool Worker::IsWorking()
		{
			return Thread.joinable();
		}

		void Worker::StartWork(PrimeJob job)
		{
			doWork = true;

			Thread = thread(&Worker::DoWork, this, job);
		}

		void Worker::StopWork()
		{
			doWork = false;
		}

		void Worker::DoWork(PrimeJob job)
		{
			Batch = job.Batch;
			//hey, do this
		}
	}
}
