namespace Users.Application.Interfaces
{
    public interface IPermissionAppService
    {
        Task<List<string>> GetAllPermissionClaimsAsync();
    }
}
