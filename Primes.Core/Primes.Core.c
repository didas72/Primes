#include "Primes.Core.h"
#include <stdint.h>
#include <stdlib.h>




uint64_t UlongSqrtHigh(const uint64_t number)
{
	if (number <= 2)
		return number;

	uint64_t max = 4294967296, min = 0, c, c2;

	while (1)
	{
		c = (max + min) >> 1;
		c2 = c * c;

		if (c2 < number)
			min = c;
		else if (c2 > number)
			max = c;
		else
			return c;

		if (max - min <= 1)
			return max;
	}
}


int NCC_Uncompress(const char* path, uint64_t* arr, size_t length, size_t start)
{
	FILE* file;
	errno_t err = fopen_s(&file, path, "rb");

	if (err) return 1;

	fseek(file, start, SEEK_SET);

	size_t arrHead = 0, read;
	uint64_t last = 0;
	uint16_t offset = 0;

	while (1)
	{
		if (!offset) //full read
		{
			read = fread_s(&last, sizeof(uint64_t), sizeof(uint64_t), 1, file);

			if (read != 1)
			{
				return 2;
			}

			arr[arrHead++] = last;
			offset = 1; //just set to non-zero
		}
		else //offset read
		{
			read = fread_s(&offset, sizeof(uint16_t), sizeof(uint16_t), 1, file);

			if (read != 1)
			{
				return 3;
			}

			if (offset)
				arr[arrHead++] = last += (uint64_t)offset;
		}

		if (arrHead >= length) //Filled array
			break;
	}

	//return
	return 0;
}
