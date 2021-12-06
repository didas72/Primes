namespace PrimesTools
{
    public class AddressValuesPair
    {
        public string Address { get; set; }
        public byte Value0 { get; set; }
        public byte Value1 { get; set; }
        public byte Value2 { get; set; }
        public byte Value3 { get; set; }
        public byte Value4 { get; set; }
        public byte Value5 { get; set; }
        public byte Value6 { get; set; }
        public byte Value7 { get; set; }



        public AddressValuesPair() { }
        public AddressValuesPair(string address, byte value0, byte value1, byte value2, byte value3, byte value4, byte value5, byte value6, byte value7)
        { Address = address; Value0 = value0; Value1 = value1; Value2 = value2; Value3 = value3; Value4 = value4; Value5 = value5; Value6 = value6; Value7 = value7; }
    }
}
