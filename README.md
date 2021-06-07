# Primes
This is a project that I have been working on with PeakRead for a few months.
Here make programs to find and store prime numbers for later processing.

If you have any suggestions or ideas for the project, feel free to let us know!

## The project

Even though this project is not developed by just me I will only talk for myself.

### Motivation

Some time after building my PC I got into overclocking and testing my rig's performance. After trying programs like Prime95 I was very curious to learn how they worked so I decided to do some research on the topic. Most sources stated that primality testing was commonly used for these programs I decided to give it a shot. I wrote a terrible method to check if a number was prime or not. I played around with it a little and did some speed tests but I had nothing to compare it's performance against, so I asked PeakRead if he had anything for me to compare to. He had already played around with this before so he had a few methods that I tested too. I then realised that my approach was TERRIBLE and we decided to take his method and optimise it as much as we could. A few improvements were made and at that point we thought: "Can we turn this into anything useful?" And so the project started.

### How it progressed

Before this repository was even created we wrote a few programs to do the same as the current ones do. These programs all worked with UInt32 values, were single-threaded and had no ability to be run in multiple machines at once. This meant that not only was the process a lot slower but also that we would hit the 32 bit capacity limit quickly. We hit that limit a few weeks after the best version of the program was completed and we decided to take it further. With a few failed attempts at writting a 64-bit, multi-threading capable version things were not looking promising. One day we eventually got to Primes(.exe) v1.0.0 and some time later this repo was created.

### Current goal

The current goal of the project is divided into a few milestones:
* Running some of the computations in the GPU (probably using OpenCL)
* Having automatically generated and distributed `PrimeJob`s
* Checking all numbers up to the maximum value using UInt64 (2^64 - 1)
* Doing some data analysis with the huge amount of data we will have then

### Data analysis

At some point we plan to take all the data we're collecting to do some research with it, we just don't know what we will do yet.



## How it works

### The code projects

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

PrimesCpp is the latest project. It is a C++ based Console program that will work very similarly to Primes(.exe), just written in C++. The purpose of this project is to learn C++ and to attempt to achieve higher performance and cross-platform capabilites than it's .NET counterpart. It is also an objective to at some point use this project with OpenCL to achieve the best speeds.

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

### Storing primes and distributing work

In order to be able to distribute work between treads and different machines and to store primes in an organised and compact way we created `PrimeJob`s.
Primejobs are structures in memory that hold all the data required to find primes. In the latest version (v1.2.0) the data contained in this structure is:
* File version
* File compression
* Batch
* First number to check
* Amount of numbers to check
* Amount of numbers already checked
* Primes found so far

With these values the programs are able to pause jobs, run them independently from other computers and group them into 'batches'. Batches are just a way to ensure we never have too many PrimeJob files in one directorym the programs place them in separate folders for easier access.

This stucture can also be written to disk with minimal modification on older versions and no modificaion at all on v1.2.0.

In v1.2.0 compression was finally introduced. Before the files themselves had no compression and the primes found would be stored as raw UInt64s but after a few weeks of leaving the programs working the total file size was reaching 100GB. This is when we decided to implement compression in the file structure itself.

There algorithms two compression algorithms implemented at the moment, both very similar. The first one, Optimized Numerical Sequence Storage or ONSS, was written by PeakRead and uses reference values and offsets to store primes. The second one, Numerical Chain Compression (NNC) by me, works by having a starting value and storing the difference between said value and the coming ones. Both of our algorithms are inspired in existing algorithms, we just optimised them for our purpose.
