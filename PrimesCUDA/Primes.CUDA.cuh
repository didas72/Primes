#pragma once
#include <cstdint>

#include <cstdint>
#include "Primes.CUDA.cuh"

#include <stdio.h>

#include <device_launch_parameters.h>

__device__ uint64_t clamp(uint64_t value, uint64_t min, uint64_t max)
{
	value = (value > max) ? max : value;

	return (value < min) ? min : value;
}

__device__ int32_t divide_round_up(int32_t dividend, int32_t divisor) { return dividend / divisor + ((dividend % divisor == 0) ? 0 : 1); }

__device__ uint64_t ulong_sqrt_high(uint64_t value)
{
	if (value <= 2)
		return value;

	uint64_t max = 4294967296, min = 0, c, c2;

	while (true)
	{
		c = (max + min) / 2;
		c2 = c * c;

		if (c2 < value)
			min = c;
		else if (c2 > value)
			max = c;
		else
			return c;

		if (max - min <= 1)
			return max;
	}
}

__device__ bool is_prime(uint64_t value)
{
	if (value < 2)
		return false;

	if (value < 4)
		return true;

	if ((value % 2) == 0)
		return false;

	uint64_t current = clamp(value, 4, UINT64_MAX);
	uint64_t sqrt = ulong_sqrt_high(value);

	while (current <= sqrt)
	{
		if (value % current == 0)
			return false;

		current += 2;
	}

	return true;
}



__global__ void
check_primes_kernel(const uint64_t first_value, const int32_t per_thread_mult, uint8_t* store)
{
	int addrBase = (blockDim.x * blockIdx.x + threadIdx.x) * per_thread_mult;
	//base + myThreadFinal * per_thread * 8 (8 per byte)
	uint64_t value_base = first_value + (((blockDim.x * blockIdx.x + threadIdx.x) * per_thread_mult) * 8);

	uint32_t byte_count = divide_round_up(per_thread_mult, 8);

	for (int i = 0, B = 0, b = 7; i < per_thread_mult; i++)
	{
		if (is_prime(value_base + i))
			store[addrBase + B] = store[addrBase + B] & (0x01 << b);
		else
			store[addrBase + B] = store[addrBase + B] & (0x00 << b);

		b--;
		if (b <= -1)
		{
			b = 7;
			B++;
		}
	}
}

