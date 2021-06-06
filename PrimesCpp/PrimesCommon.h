// PrimesComon.h - Contains declarations of primes functions
#pragma once

unsigned long long UlongSqrtHigh(const unsigned long long& number);

bool IsPrime(const unsigned long long& number);

bool IsPrime(const unsigned long long& number, const unsigned long long* knownPrimes, const unsigned int& KPLength);
