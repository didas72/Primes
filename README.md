# Primes
This is a project that I have been working on with a friend of mine for a few months.
Here make programs to find and store prime numbers for later processing.

As of when this README was written, no specific processing will be applied to the data and the project is just running for fun and practice.
Hopefully someone will be able to learn from it.

If you have any suggestions or ideas for the project, feel free to let us know!

(Anything bellow this is W.I.P.)

## How it works

### The projects

Currently there are 5 projects inside this solution:
* Primes(.exe)
* PrimesSVC
* Job Management
* Primes.Common
* PrimesCpp

Primes(.exe) was the first project created. It is a C# console application that runs on .NET Framework 4.7.2 and has minimal UI. It runs several `Worker`s in parallel, each doing `PrimeJob`s on it's own and saving them to disk. Each worker has a progress bar shown in console as well as a progress percentage and a Batch indicator. (Details on what each of these words mean bellow)

PrimesSVC is a W.I.P. Windows service. Like Primes(.exe), it is a C# .NET Framework 4.7.2 project but instead of being a console window it runs in the background doing it's thing. It's work is also based on `PrimeJob`s and it dynamcally adjusts the number of workers based on how much free CPU time there is.

Job Management is yet another C# project, this one is just a simple console program used to test different aspects of programs when needed. Code in it is temporary and experimental.

Primes.Common is a C# library that holds definitions for `PrimeJob`, `KnownPrimesResourceFile`, `Mathf`, `Utils` and other general utilities. All these components are available to other C# projects so new features will automatically be implemented in all programs.

PrimesCpp is the latest project. It is a C++ based Console program that will work very similarly to Primes(.exe), just written in C++. The purpose of this project is to learn C++ and to attempt to achieve higher performance and cross-platform capabilites than it's .NET counterpart.

### Finding primes

As of when this README was written, there are only two methods for finding primes implemented and one is based on the other. 

The first and most basic method works like so:
1. If the number is smaller than 2 it is not prime.
2. If the number is multiple of 2 or 3 it is prime.
3. If the number is even it is not prime.
4. Starting from 5, check if the number is dividable by the current number, incrementing the test number by 2 every time, until the the square root of the number being checked.

The second method works in a similar way but it ignores numbers that are already known not to be primes. This greatly improoves speed and doesn't require too much code complexity. When testing a number to check wether it's a prime or not, you don't need to check all the numbers below it's square root. (eg: after checking that a number is not devidable by 3, we don't need to check if it's dividable by 9, because if it were, it would have been caught by the check against 3.) The way this method works is as follows:
1. If the number is smaller than 2 it is not prime.
2. If the number is multiple of 2 or 3 it is prime.
3. If the number is even it is not prime.
4. Starting from the first one, check if the number is dividable by all the primes in the 'knownPrimes' array. If we run out of known primes, simply use the first method starting with the first odd number after the last number in the array.
For this method to work, the given array must be ordered from lowest to highest and hold ALL the primes from 5 (or 2 or 3) to the last value in the array. If this array contains non-prime numbers the method will still work, it will just be slower.

### Storing primes
