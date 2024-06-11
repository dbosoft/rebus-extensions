using Dbosoft.Rebus.OperationsDB.Tests.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Dbosoft.Rebus.OperationsDB.Tests;

public class StateStoreContext : DbContext
{
 
    public StateStoreContext(DbContextOptions<StateStoreContext> options)
        : base(options)
    {
    }

    public DbSet<OperationModel>? Operations { get; set; }
    public DbSet<OperationTaskModel>? OperationTasks { get; set; }
    public DbSet<OperationLogEntry>? OperationLogs{ get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<OperationModel>().HasKey(x => x.Id);
        modelBuilder.Entity<OperationModel>().Property(x => x.Id).IsRequired().ValueGeneratedNever();

        // modelBuilder.Entity<OperationModel>()
        //     .Property(x => x.Timestamp)
        //     .IsRowVersion();

        modelBuilder.Entity<OperationModel>()
            .HasMany(x => x.Tasks)
            .WithOne(x => x.Operation)
            .HasForeignKey(x => x.OperationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OperationModel>()
            .Property(x => x.LastUpdate)
            .IsConcurrencyToken();

        modelBuilder.Entity<OperationTaskModel>().HasKey(x => x.Id);
        modelBuilder.Entity<OperationTaskModel>().Property(x => x.Id).IsRequired().ValueGeneratedNever();

        modelBuilder.Entity<OperationTaskModel>()
             .Property(x => x.LastUpdate)
             .IsConcurrencyToken();
    }

    protected override void ConfigureConventions(
        ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        
        // Convert all DateTimeOffset values to DateTime values.
        // Sqlite does not have native support for DateTimeOffset.
        // The conversion is lossy as we convert to UTC but the
        // custom comparer ignores the time zone information.
        // This way, we can both sort by the datetime value
        // and use it as a concurrency token.
        /*
        configurationBuilder.Properties<DateTimeOffset>()
            .HaveConversion<DateTimeOffsetToDateTimeConverter, DateTimeOffsetUtcComparer>();
        */
    }

    private sealed class DateTimeOffsetToDateTimeConverter(ConverterMappingHints? mappingHints)
        : ValueConverter<DateTimeOffset, DateTime>(
            t => t.UtcDateTime,
            t => new DateTimeOffset(t.Ticks, TimeSpan.Zero),
            mappingHints)
    {
        public DateTimeOffsetToDateTimeConverter() : this(null) { }
    }

    private sealed class DateTimeOffsetUtcComparer() : ValueComparer<DateTimeOffset>(
        (a, b) => a.Ticks == b.Ticks,
        a => a.Ticks.GetHashCode());

}