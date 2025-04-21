namespace NewsAggregator.Domain.Models
{
    public class Role
    {
        public string? Id { get; private set; }
        public string? Name { get; private set; }
        public string? DisplayName { get; private set; }

        public Role()
        {

        }

        public Role(string? id, string? name, string? displayName)
        {
            Id = id;
            Name = name;
            DisplayName = displayName;
        }
    }
}
