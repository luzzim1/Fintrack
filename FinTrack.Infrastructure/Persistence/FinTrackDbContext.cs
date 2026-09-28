using Microsoft.EntityFrameworkCore;

namespace FinTrack.Infrastructure.Persistence;

public class FinTrackDbContext : DbContext
{
    public FinTrackDbContext(DbContextOptions<FinTrackDbContext> options)
        : base(options)
    {
    }
}
