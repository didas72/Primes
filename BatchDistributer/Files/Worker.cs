using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Primes.BatchDistributer.Files
{
    public class Worker
    {
        #region charMapping
        private readonly static Dictionary<char, int> charMap = new Dictionary<char, int>() 
        {
            { 'a', 0 },
            { 'b', 1 },
            { 'c', 2 },
            { 'd', 3 },
            { 'e', 4 },
            { 'f', 5 },
            { 'g', 6 },
            { 'h', 7 },
            { 'i', 8 },
            { 'j', 9 },
            { 'k', 10 },
            { 'l', 11 },
            { 'm', 12 },
            { 'n', 13 },
            { 'o', 14 },
            { 'p', 15 },
            { 'q', 16 },
            { 'r', 17 },
            { 's', 18 },
            { 't', 19 },
            { 'u', 20 },
            { 'v', 21 },
            { 'w', 22 },
            { 'x', 23 },
            { 'y', 24 },
            { 'z', 25 },
            { 'A', 26 },
            { 'B', 27 },
            { 'C', 28 },
            { 'D', 29 },
            { 'E', 30 },
            { 'F', 31 },
            { 'G', 32 },
            { 'H', 33 },
            { 'I', 34 },
            { 'J', 35 },
            { 'K', 36 },
            { 'L', 37 },
            { 'M', 38 },
            { 'N', 39 },
            { 'O', 40 },
            { 'P', 41 },
            { 'Q', 42 },
            { 'R', 43 },
            { 'S', 44 },
            { 'T', 45 },
            { 'U', 46 },
            { 'V', 47 },
            { 'W', 48 },
            { 'X', 49 },
            { 'Y', 50 },
            { 'Z', 51 },
            { '0', 52 },
            { '1', 53 },
            { '2', 54 },
            { '3', 55 },
            { '4', 56 },
            { '5', 57 },
            { '6', 58 },
            { '7', 59 },
            { '8', 60 },
            { '9', 61 },
        };
        private readonly static Dictionary<int, char> invCharMap = new Dictionary<int, char>()
        {
            { 0, 'a' },
            { 1, 'b' },
            { 2, 'c' },
            { 3, 'd' },
            { 4, 'e' },
            { 5, 'f' },
            { 6, 'g' },
            { 7, 'h' },
            { 8, 'i' },
            { 9, 'j' },
            { 10, 'k' },
            { 11, 'l' },
            { 12, 'm' },
            { 13, 'n' },
            { 14, 'o' },
            { 15, 'p' },
            { 16, 'q' },
            { 17, 'r' },
            { 18, 's' },
            { 19, 't' },
            { 20, 'u' },
            { 21, 'v' },
            { 22, 'w' },
            { 23, 'x' },
            { 24, 'y' },
            { 25, 'z' },
            { 26, 'A' },
            { 27, 'B' },
            { 28, 'C' },
            { 29, 'D' },
            { 30, 'E' },
            { 31, 'F' },
            { 32, 'G' },
            { 33, 'H' },
            { 34, 'I' },
            { 35, 'J' },
            { 36, 'K' },
            { 37, 'L' },
            { 38, 'M' },
            { 39, 'N' },
            { 40, 'O' },
            { 41, 'P' },
            { 42, 'Q' },
            { 43, 'R' },
            { 44, 'S' },
            { 45, 'T' },
            { 46, 'U' },
            { 47, 'V' },
            { 48, 'W' },
            { 49, 'X' },
            { 50, 'Y' },
            { 51, 'Z' },
            { 52, '0' },
            { 53, '1' },
            { 54, '2' },
            { 55, '3' },
            { 56, '4' },
            { 57, '5' },
            { 58, '6' },
            { 59, '7' },
            { 60, '8' },
            { 61, '9' },
        };
        #endregion

        public string Id { get; } // alphanumeric, lower and upper-case, 4 chars long, ASCII encoding
        public TimeStamp LastContacted { get; set; }



        public Worker(string id)
        {
            Id = id; 
        }



        public void RegisterContactTime()
        {
            LastContacted = TimeStamp.Now();
        }



        public static int GetWorkerIdValue(string workerId)
        {
            if (!IsValidWorkerId(workerId)) throw new InvalidWorkerId(workerId);

            char[] chars = workerId.ToCharArray();

            int ret = charMap[chars[0]];
            ret += charMap[chars[1]] * 62;
            ret += charMap[chars[2]] * 3844;
            ret += charMap[chars[3]] * 238328;

            return ret;
        }
        public static string GetWorkerIdString(int value)
        {
            string ret = string.Empty;

            int ch = value % 62;
            value -= ch;
            ret += invCharMap[ch];

            ch = value % 3844;
            value -= ch;
            ch /= 62;
            ret += invCharMap[ch];

            ch = value % 238328;
            value -= ch;
            ch /= 3844;
            ret += invCharMap[ch];

            ch = value / 238328;
            ret += invCharMap[ch];

            return ret;
        }



        public byte[] Serialize()
        {
            byte[] buffer = new byte[12]; //4 id + 8 timestamps

            Array.Copy(Encoding.ASCII.GetBytes(Id), 0, buffer, 0, 4);
            Array.Copy(LastContacted.Serialize(), 0, buffer, 4, 8);

            return buffer;
        }
        public static Worker Deserialize(byte[] buffer) => Deserialize(buffer, 0);
        public static Worker Deserialize(byte[] buffer, int startIndex)
        {
            Worker client = new Worker(Encoding.ASCII.GetString(buffer, startIndex, 4))
            {
                LastContacted = TimeStamp.Deserialize(buffer, startIndex + 4)
            };

            return client;
        }



        public static bool IsValidWorkerId(string workerId)
        {
            if (workerId.Length != 4) return false;

            Regex r = new Regex("^[a-zA-Z0-9]*$");
            return r.IsMatch(workerId);
        }
    }



    public class InvalidWorkerId : Exception
    {
        public InvalidWorkerId(string workerId) : base ($"{workerId} is not a valid workerId.") { }
    }
}
