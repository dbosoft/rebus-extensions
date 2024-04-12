using Dbosoft.Rebus.OperationsDB.Tests.Models;
using Microsoft.EntityFrameworkCore;

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
}