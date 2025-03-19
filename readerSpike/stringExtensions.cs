public static class stringExtensions
{
    public static string Escape(this string str)
    {
        return string.Create(str.Length, str, (span, str) =>
        {
            for (var i = 0; i < span.Length; i++)
            {
                span[i] = str[i] switch
                {
                    char c when c < 32 => (char)(c + 0x2400),
                    char c => c
                };
            }
        });
    }
}