#include "cuda_runtime.h"
#include "device_launch_parameters.h"

#include <stdint.h>
#include <cstdlib>

#include "Primes.CUDA.LIB.h"
#include "Primes.CUDA.cuh"

#ifdef __INTELLISENSE__
#define CUDA_KERNEL(...)
#else
#define CUDA_KERNEL(...) <<< __VA_ARGS__ >>>
#endif

int main() { return 0; }

uint8_t* parallel_is_prime(uint64_t start, int32_t threadsPerBlock, int32_t blocksPerGrid, int32_t perThreadMult)
{
	size_t size = threadsPerBlock * blocksPerGrid * perThreadMult * sizeof(uint8_t);

	//allocate buffer in host memory
	uint8_t* host_buffer = (uint8_t*)malloc(size);

	//alocate buffer in device memory
	uint8_t* device_buffer;
	cudaMalloc(&device_buffer, size);


	//call kernel
	//DO NOT MULT PERTHREAD BY 8 OR YOU WILL FUCK IT UP (done in kernel)
	check_primes_kernel <<<blocksPerGrid, threadsPerBlock>>>(start, perThreadMult, device_buffer);


	//get data
	cudaMemcpy(host_buffer, device_buffer, size, cudaMemcpyDeviceToHost);
	//free memory (else mem leak ;))
	cudaFree(device_buffer);


	//free host memory?
	return host_buffer;
}
