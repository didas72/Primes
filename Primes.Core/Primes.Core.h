#pragma once

#include <stdint.h>
#include <stdio.h>

#ifdef PRIMESCPP_EXPORTS
#define PRIMESCPP_API __declspec(dllexport)
#else
#define PRIMESCPP_API __declspec(dllimport)
#endif

#ifdef __cplusplus
#define CPP_COMPAT extern "C"
#else
#define CPP_COMPAT 
#endif

CPP_COMPAT PRIMESCPP_API uint64_t UlongSqrtHigh(const uint64_t number);

/// <summary>
/// Uncompresses an array of numbers stored using NCC.
/// </summary>
/// <param name="path">Input file path.</param>
/// <param name="arr">Pointer to array to write to.</param>
/// <param name="length">Length of returned array.</param>
/// <param name="start">Offset in the file to start reading from.</param>
/// <returns>Pointer to the array or NULL if failed.</returns>
CPP_COMPAT PRIMESCPP_API int NCC_Uncompress(const char* path, uint64_t* arr, size_t length, size_t start);
