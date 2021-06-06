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

Primes.Common is a C# library that holds definitions for `PrimeJob`, `KnownPrimesResourceFile`, `Mathf`, `Utils` and `Serializers`. All these components are available to other C# projects so new features will automatically be implemented in all programs.

PrimesCpp is the latest project. It is a C++ based Console program that will work very similarly to Primes(.exe), just written in C++. The purpose of this project is to learn C++ and to attempt to achieve higher performance and cross-platform capabilites than it's .NET counterpart.

### Finding primes


