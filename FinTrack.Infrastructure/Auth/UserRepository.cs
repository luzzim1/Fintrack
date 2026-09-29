using FinTrack.Application.Auth;
using FinTrack.Application.Common;
using FinTrack.Domain.Entities;
using FinTrack.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Infrastructure.Auth;

public class UserRepository(FinTrackDbContext db) : IUserRepository
{
    public Task<User?> FindByEmail(string email, CancellationToken cancellationToken) =>
        db.Users.SingleOrDefaultAsync(user => user.Email == email, cancellationToken);

    public async Task Add(User user, CancellationToken cancellationToken)
    {
        db.Users.Add(user);
        try { await db.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        { throw new ConflictException("Não foi possível cadastrar este email."); }
    }
}
