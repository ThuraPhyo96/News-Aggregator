namespace NewsAggregator.Domain.Models
{
    public class RolePermission
    {
        public string? Id { get; private set; }
        public string? RoleId { get; private set; }
        public string? PermissionId { get; private set; }
    }
}
