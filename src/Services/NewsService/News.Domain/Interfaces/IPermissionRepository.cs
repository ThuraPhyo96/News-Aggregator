namespace News.Domain.Interfaces
{
    public interface IPermissionRepository
    {
        Task<IEnumerable<string>> GetAllPermissionsAsync(CancellationToken cancellationToken = default);
    }
}
