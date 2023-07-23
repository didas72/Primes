using System.Runtime.InteropServices;

namespace Primes.Common
{
    //TODO: Move to internal
    public static partial class CoreWrapper
    {
        [DllImport("lib\\Primes.Core.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "UlongSqrtHigh")]
        public static extern ulong UlongSqrtHigh(ulong number);


        [DllImport("lib\\Primes.Core.dll", EntryPoint = "NCC_Uncompress", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int NCC_Uncompress(
            string path, [In, Out] ulong[] arr, ulong size, ulong start);
    }
}
