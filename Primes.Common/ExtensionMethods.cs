namespace Primes.Common
{
    public static class ExtensionMethods
    {
        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
                return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        public static string SetLength(this string value, int length)
        {
            if (value.Length >= length)
                return value.Substring(0, length);

            return value += " ".Loop(length - value.Length);
        }

        public static string Loop(this string value, int times)
        {
            string f = string.Empty;

            for (int i = 0; i < times; i++) f += value;

            return f;
        }
    }
}
