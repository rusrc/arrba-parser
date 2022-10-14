namespace Arrba.Parser.Provider.Extension
{
    public static class StringExtension
    {
        public static string RemoveSpaces(this string that)
        {
            return that.Replace("\n", "").Replace("\t", "");
        }
    }
}
