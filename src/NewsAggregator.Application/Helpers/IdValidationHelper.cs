using System.Text.RegularExpressions;

namespace NewsAggregator.Application.Helpers
{
    public static partial class IdValidationHelper
    {
        private static readonly Regex HexadecimalRegex = IdRegex();

        // Method to check if the ID is a valid 24-character hexadecimal string (MongoDB ObjectId)
        public static bool IsValidHexadecimalId(string id)
        {
            return HexadecimalRegex.IsMatch(id);
        }

        [GeneratedRegex("^[a-fA-F0-9]{24}$", RegexOptions.Compiled)]
        private static partial Regex IdRegex();
    }
}
