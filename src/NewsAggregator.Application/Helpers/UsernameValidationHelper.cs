using System.Text.RegularExpressions;

namespace NewsAggregator.Application.Helpers
{
    public static partial class UsernameValidationHelper
    {
        public static bool IsValidUsername(string username)
        {
            // Example: allow a-z, A-Z, 0-9, ., _
            var regex = UsernameRegex();
            return regex.IsMatch(username);
        }

        [GeneratedRegex(@"^[a-zA-Z0-9._]+$")]
        private static partial Regex UsernameRegex();
    }
}
