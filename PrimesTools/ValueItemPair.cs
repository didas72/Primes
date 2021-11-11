using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimesTools
{
    public class ValueItemPair
    {
        public string Name { get; set; }
        public string Value { get; set; }



        public ValueItemPair() { }
        public ValueItemPair(string name, string value) { Name = name; Value = value; }
    }
}
