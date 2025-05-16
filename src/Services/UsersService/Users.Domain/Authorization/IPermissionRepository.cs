namespace Users.Domain.Authorization
{
    public interface IPermissionRepository
    {
        Task<List<string>> GetAllPermissionClaimsAsync();
    }
}
