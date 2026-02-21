using Microsoft.EntityFrameworkCore;
using PGManagement.API.Models;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace PGManagement.API.Data;

public class PGManagementDbContext : DbContext
{
    public PGManagementDbContext(DbContextOptions<PGManagementDbContext> options) : base(options)
    {
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Machine> Machines => Set<Machine>();
    public DbSet<MachineSession> MachineSessions => Set<MachineSession>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var tenant = modelBuilder.Entity<Tenant>();

        tenant.HasKey(t => t.Id);
        tenant.Property(t => t.FirebaseUid)
            .HasMaxLength(128)
            .IsRequired();

        tenant.Property(t => t.PhoneNumber)
            .HasMaxLength(32)
            .IsRequired();

        tenant.Property(t => t.IsApproved)
            .IsRequired()
            .HasDefaultValue(false);

        tenant.Property(t => t.ApprovedAtUtc);

        tenant.Property(t => t.FullName)
            .HasMaxLength(200);

        tenant.Property(t => t.Email)
            .HasMaxLength(256);

        tenant.Property(t => t.IdProofType)
            .HasMaxLength(100);

        tenant.Property(t => t.IdProofNumber)
            .HasMaxLength(100);

        tenant.Property(t => t.AdvanceAmount)
            .HasColumnType("decimal(18,2)");

        tenant.Property(t => t.RoomRent)
            .HasColumnType("decimal(18,2)");

        tenant.HasIndex(t => t.FirebaseUid).IsUnique();
        tenant.HasIndex(t => t.PhoneNumber);

        var machine = modelBuilder.Entity<Machine>();
        machine.HasKey(m => m.MachineId);
        machine.Property(m => m.MachineName)
            .HasMaxLength(200)
            .IsRequired();
        machine.Property(m => m.IsAvailable)
            .IsRequired()
            .HasDefaultValue(true);
        machine.Property(m => m.EndTime);
        machine.Property(m => m.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        machine.HasOne(m => m.CurrentUser)
            .WithMany()
            .HasForeignKey(m => m.CurrentUserId)
            .OnDelete(DeleteBehavior.NoAction);

        machine.HasIndex(m => m.IsAvailable);
        machine.HasIndex(m => m.EndTime);
        machine.HasData(
            new Machine { MachineId = 1, MachineName = "Machine 1", IsAvailable = true },
            new Machine { MachineId = 2, MachineName = "Machine 2", IsAvailable = true },
            new Machine { MachineId = 3, MachineName = "Machine 3", IsAvailable = true },
            new Machine { MachineId = 4, MachineName = "Machine 4", IsAvailable = true }
        );

        var session = modelBuilder.Entity<MachineSession>();
        session.HasKey(s => s.SessionId);
        session.Property(s => s.StartTime)
            .IsRequired();
        session.Property(s => s.EndTime);

        session.Property(s => s.Status)
            .HasConversion(new EnumToStringConverter<MachineSessionStatus>())
            .HasMaxLength(32)
            .IsRequired();

        session.HasOne(s => s.Machine)
            .WithMany()
            .HasForeignKey(s => s.MachineId)
            .OnDelete(DeleteBehavior.Cascade);

        session.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        session.HasIndex(s => new { s.MachineId, s.Status });
        session.HasIndex(s => new { s.UserId, s.Status });
    }
}
