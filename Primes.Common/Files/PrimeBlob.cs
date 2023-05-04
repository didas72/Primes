using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Primes.Common.Files
{
    /// <summary>
    /// Mass storage of prime numbers.
    /// Has error detection, random reads with acceptable latency and good compression.
    /// </summary>
    public class PrimeBlob
    {
        //TODO: Must consider write situation
        //Options:
        //Append only: No deleting/rewritting blocks at all
        //Shift delete: Deletion requires shifting of entire file and correction block pointers, potentailly with file copying
        //Filesystem approach: Fixed size blocks with allocation table
        //Linked list approach: Pointers everywhere and allow segmentation of data, possible defragmentation

        //TODO: Must consider entry table growth

        /*
         * File format 'high level' strategy:
         * The idea of the PrimeBlob is to be able to keep all primes in a felxible, fast access, compressed and reliable structure.
         * -To provide the fast access, primes are split into blocks, which indexed externally by their starting value and index
         * for quick searching.
         * -To provide compression, each block is compressed individually.
         * -To provide reliability, each block has an error detection code associated to it's data, and should a block be faulty,
         * spread throghout the block space, several 'XOR Blocks' can be found, which keep the XOR result of their assigned blocks.
         * 
         * File format spec:
         * -Header
         * -Block table (header followed by entries)
         * -Blocks
         * 
         * Header:
         *  +-------+-------+-----------------------+
         *  | Off   | Len   | Description           |
         *  +-------+-------+-----------------------+
         *  | 0     | 4     | 0x424C4F42 ('BLOB')   |
         *  | 4     | 3     | Version               |
         *  | 7     | 1     | 0x00 (Reserved)       |
         *  | 8     | 8     | File length           |
         *  | 16    | 4     | 0x00 (Reserved)       |
         *  | 20    | 2     | Flags*                |
         *  | 22    | 2     | Header Fletcher16     |
         *  +-------+-------+-----------------------+
         * 
         * NOTES:
         *  *Flags: [0] Open, [1] Bad Write*, [2-3] Number width*, [4-6] Error checking*, [7-8] Compression*, [7-15] Reserved
         *  *'Bad Write' flag is set and flushed when starting a write and reset once write is complete
         *  *'Number width' flag is set to 0 = 32bit, 1 = 64bit, 2 = 128bit, 3 = reserved for extensions
         *  *'Error checking' flas is set to 0 = none, 1 = fletcher32, 2 = reserved, 3 = reserved for extensions
         *  *'Compression' flag is set to 0 = none, 1 = NCC only, 2 = reserved (wip), 3 = reserved for extensions
         *  
         * Block table header:
         *  +-------+-------+-----------------------+
         *  | Off   | Len   | Description           |
         *  +-------+-------+-----------------------+
         *  | 0     | 8     | Block count           |
         *  | 8     | 8     | Block size            |
         *  | 16    | 8     | Last written block*   |
         *  | 24    | 8     | 0x00 (Reserved)       |
         *  +-------+-------+-----------------------+
         *  
         *  NOTES:
         *   -*if 'Bad Write' flag is set, value is not 0 and the pointed block is invalid, it's old data is kept in the 'Backup Block'.
         *  
         * Block entries:
         *  +-------+-------+-----------------------+
         *  | Off   | Len   | Description           |
         *  +-------+-------+-----------------------+
         *  | 0     | 8     | Block location        |
         *  | 8     | NS    | Lowest number stored* |
         *  | 8+NS  | NS    | Prime index           |
         *  +-------+-------+-----------------------+
         *  
         * NOTES: 
         *  -NS = Number width / 8
         *  -First block is 'Backup Block'.
         *  -*'Lowest number stored' should be all 0xFF if the block is an 'XOR Block'.
         * 
         * Block head:
         *  +-------+-------+-----------------------+
         *  | Off   | Len   | Description           |
         *  +-------+-------+-----------------------+
         *  | 0     | 4     | Block size            |
         *  | 4     | 4     | Numbers in block      |
         *  | 8     | 8     | First write timestamp*|
         *  | 16    | 8     | Last write timestamp* |
         *  | 24    | 8     | Error checking code   |
         *  | 32    | XXX   | (Compressed) data     |
         *  +-------+-------+-----------------------+
         *  
         *  NOTES:
         *   -*'First/Last write timestamps' double as first/last block indices if block is an 'XOR Block'.
         */

        private const int blockSize = 0x40000; //256ki numbers, 2MB if 64bit values



        public Version FileVersion { get; }




        public readonly struct Version
        {
            public readonly byte major;
            public readonly byte minor;
            public readonly byte patch;



            public Version(byte major, byte minor, byte patch)
            {
                this.major = major; this.minor = minor; this.patch = patch;
            }
        }
    }
}
