namespace NewsAggregator.Domain.Models
{
    public class Permission
    {
        public string? Id { get; private set; }
        public string? DisplayName { get; private set; }
        public string? ClaimValue { get; private set; }

        public Permission()
        {

        }

        public Permission(string? id, string? displayName, string? claimValue)
        {
            Id = id;
            DisplayName = displayName;
            ClaimValue = claimValue;
        }
    }
}
