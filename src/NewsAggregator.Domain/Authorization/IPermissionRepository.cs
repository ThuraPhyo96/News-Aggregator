namespace NewsAggregator.Domain.Authorization
{
    public interface IPermissionRepository
    {
        Task<List<string>> GetAllPermissionClaimsAsync();
    }
}
