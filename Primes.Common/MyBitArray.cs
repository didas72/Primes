using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Primes.Common
{
    //By PeakRead, adapted by Didas72
    public class MyBitArray
    {
        //by PeakRead
        private List<byte> DATA;
        public int size;

        public MyBitArray()
        {
            DATA = new List<byte>();
            size = 0;
        }
        public void AppendBool(bool val)
        {
            EnsureCapacity(++size);

            DATA[size / 8] += (byte)((val ? 1 : 0) << ((7 - size) % 8));
        }
        //10101001
        //_____10101001
        //_____&_1
        //_____==1
        public void AppendByte(byte num)
        {
            for (int i = 0; i < 8; i++)
            {
                AppendBool((num >> 7 - i & 1) == 1);
            }
        }
        public void AppendInt(int num)
        {
            for (int i = 0; i < 32; i++)
            {
                AppendBool((num >> 31 - i & 1) == 1);
            }
        }
        public void AppendUlong(ulong num)
        {
            for (int i = 0; i < 64; i++)
            {
                AppendBool((num >> 63 - i & 1) == 1);
            }
        }
        public bool GetBool(int index)
        {
            if (index < 0) { throw new IndexOutOfRangeException("tried reading bellow 0"); }
            if (index >= size) { throw new IndexOutOfRangeException("tried reading above the size of the array"); }
            return ((DATA[index / 8] >> (7 - (index % 8))) & 1) == 1;
        }
        public byte GetByte(int index)
        {
            byte num = 0;
            for (int i = 0; i < 8; i++)
            {
                num += (byte)((GetBool(index + i) ? 1 : 0) << (7 - i));
            }
            return num;
        }
        public int GetInt(int index)
        {
            int num = 0;
            for (int i = 0; i < 32; i++)
            {
                num += (GetBool(index + i) ? 1 : 0) << (31 - i);
            }
            return num;
        }
        public ulong GetUlong(int index)
        {
            ulong num = 0;

            for (int i = 0; i < 64; i++)
            {
                num += ((GetBool(index + i) ? 1ul : 0ul) << (63 - i));
            }

            return num;
        }



        //by Didas72
        public void AppendBoolArray(bool[] arr)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                AppendBool(arr[i]);
            }
        }
        private void EnsureCapacity(int capacity)
        {
            if (DATA.Count * 8 >= capacity)
                return;

            int off = DATA.Count * 8 - capacity;
            byte[] buff = new byte[Mathf.DivideRoundUp(off, 8)];
            DATA.AddRange(buff);
        }
        public byte[] Serialize()
        {
            byte[] ret = new byte[DATA.Count + 4]; //4 to indicate size

            Array.Copy(BitConverter.GetBytes(size), 0, ret, 0, 4);
            Array.Copy(DATA.ToArray(), 0, ret, 4, DATA.Count);

            return ret;
        }
        public static MyBitArray Deserialize(byte[] bytes)
        {
            MyBitArray arr = new MyBitArray();
            arr.size = BitConverter.ToInt32(bytes, 0);

            byte[] tmpData = new byte[Mathf.DivideRoundUp(arr.size, 8)];
            Array.Copy(bytes, 4, tmpData, 0, tmpData.Length);
            arr.DATA = new List<byte>(tmpData);

            return arr;
        }
    }
}
