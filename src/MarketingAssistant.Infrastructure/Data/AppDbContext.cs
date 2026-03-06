using MarketingAssistant.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace MarketingAssistant.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Briefing> Briefings => Set<Briefing>();
    public DbSet<ActionItem> ActionItems => Set<ActionItem>();
    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<KpiSnapshot> KpiSnapshots => Set<KpiSnapshot>();
    public DbSet<ConversationMessage> ConversationMessages => Set<ConversationMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
