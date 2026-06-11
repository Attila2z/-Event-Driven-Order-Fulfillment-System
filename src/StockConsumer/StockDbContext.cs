using Microsoft.EntityFrameworkCore;

namespace StockConsumer;

// The DbContext is EF's connection to the database — it represents a session
// and exposes the tables you can query/write.
public class StockDbContext : DbContext
{
    public StockDbContext(DbContextOptions<StockDbContext> options) : base(options) { }

    // Each DbSet = one table. This one maps to ProcessedMessages.
    public DbSet<ProcessedMessage> ProcessedMessages => Set<ProcessedMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProcessedMessage>(entity =>
        {
            entity.HasKey(p => p.MessageId);          // MessageId is the primary key
            entity.HasIndex(p => p.MessageId).IsUnique(); // and unique — the atomicity guarantee
        });
    }
}