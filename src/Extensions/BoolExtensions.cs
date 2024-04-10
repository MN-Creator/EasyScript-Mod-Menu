namespace EasyScript.Extensions
{
    internal static class BoolExtensions
    {
        private static readonly string _yes = "Yes";
        private static readonly string _no = "No";

        public static string AsYesNo(this bool value)
        {
            return value ? _yes : _no;
        }
    }
}
