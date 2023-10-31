namespace Functions.Shared
{
    public static class StringExtensions
    {
        public static string Truncate(this string value, int length)
        {
            if (!string.IsNullOrEmpty(value))
            {
                if (value.Length > length)
                {
                    value = value.Remove(length);
                }

                return value;
            }

            return null;
        }
    }
}