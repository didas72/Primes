using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimesTools
{
    public class AddressValuesPair
    {
        public string Address { get; set; }
        public string Values { get; set; }



        public AddressValuesPair() { }
        public AddressValuesPair(string address, string values) { Address = address; Values = values; }
    }
}
