using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace UserManagementAPI.Data;

public interface IUserDbContext
{
    DbSet<User> Users { get; set; }
    EntityEntry Entry(object entity);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}