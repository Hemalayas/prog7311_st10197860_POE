using Microsoft.EntityFrameworkCore;
using TechMove.GLMS.API.Models;

namespace TechMove.GLMS.API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<ServiceRequest> ServiceRequests => Set<ServiceRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(c => c.Id);

            entity.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.HasMany(c => c.Contracts)
                .WithOne(ct => ct.Client)
                .HasForeignKey(ct => ct.ClientId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Contract>(entity =>
        {
            entity.HasKey(c => c.Id);

            entity.Property(c => c.Status)
                .HasDefaultValue(ContractStatus.Draft);

            entity.Property(c => c.ServiceLevel)
                .HasMaxLength(200);

            entity.HasMany(c => c.ServiceRequests)
                .WithOne(sr => sr.Contract)
                .HasForeignKey(sr => sr.ContractId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ServiceRequest>(entity =>
        {
            entity.HasKey(sr => sr.Id);

            entity.Property(sr => sr.Description)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(sr => sr.Cost)
                .HasColumnType("decimal(18,2)");

            entity.Property(sr => sr.CostZAR)
                .HasColumnType("decimal(18,2)");
        });
    }
}
