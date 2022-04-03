﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;

using DidasUtils;
using DidasUtils.Logging;
using DidasUtils.Extensions;

using Primes.Common;
using Primes.Common.Files;

namespace Primes.SVC
{
    internal static class ResourceHolder
    {
        public static ulong[] knownPrimes;



        public static bool Init()
        {
            try
            {
                int maxResMem = Settings.GetMaxResourceMemory();

                if (maxResMem > 0) throw new NotImplementedException(); //must implement partial deserialization first

                string kprfPath = Path.Combine(Globals.resourcesDir, "knownPrimes.rsrc");

                if (!File.Exists(kprfPath)) Log.LogEvent(Log.EventType.Warning, "Resource file not found, performance will be severly affected.", "ResourceHolder");

                var file = KnownPrimesResourceFile.Deserialize(kprfPath);
                knownPrimes = file.Primes;
            }
            catch (Exception e)
            {
                Log.LogException("Failed to init resources.", "ResourceHolder", e);
                return false;
            }

            return true;
        }
    }
}
