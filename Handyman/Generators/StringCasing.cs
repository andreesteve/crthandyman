namespace Handyman.Generators
{
    internal static class StringCasing
    {
        public static string ToFirstCharUpper(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            var chars = value.ToCharArray();
            chars[0] = char.ToUpper(chars[0]);
            return new string(chars);
        }

        public static string ToFirstCharLower(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            var chars = value.ToCharArray();
            chars[0] = char.ToLower(chars[0]);
            return new string(chars);
        }
    }
}