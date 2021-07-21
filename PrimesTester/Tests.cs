using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Primes.Tester
{
    static class Tests
    {
        public const ulong small_start = 2, small_max = 50000000;               //2     =>  50M
        public const ulong med_start = 50000000, med_max = 200000000;           //50M   =>  200M
        public const ulong large_start = 200000000, large_max = 2000000000;     //200M  =>  2B
        public const ulong huge_start = 2000000000, huge_max = 10000000000;     //2B    =>  10B
    }
}
