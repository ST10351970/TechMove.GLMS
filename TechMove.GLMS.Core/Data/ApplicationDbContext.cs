using Microsoft.EntityFrameworkCore;
using TechMove.GLMS.Core.Entities;
using TechMove.GLMS.Core.Enums;

namespace TechMove.GLMS.Core.Data;

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

        // Client configuration
        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(c => c.Id);

            entity.Property(c => c.Name)
                  .IsRequired()
                  .HasMaxLength(150);

            entity.Property(c => c.ContactDetails)
                  .IsRequired()
                  .HasMaxLength(200);

            entity.Property(c => c.Region)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.HasIndex(c => c.Name);
        });

        //Contract configuration
        modelBuilder.Entity<Contract>(entity =>
        {
            entity.HasKey(c => c.Id);

            entity.Property(c => c.ServiceLevel)
                  .IsRequired()
                  .HasMaxLength(50);

            entity.Property(c => c.Status)
                  .IsRequired()
                  .HasConversion<int>();

            entity.Property(c => c.SignedAgreementPath)
                  .HasMaxLength(500);

            entity.Property(c => c.SignedAgreementOriginalName)
                  .HasMaxLength(255);

            // One-to-Many: Client -> Contracts
            entity.HasOne(c => c.Client)
                  .WithMany(client => client.Contracts)
                  .HasForeignKey(c => c.ClientId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(c => c.Status);
            entity.HasIndex(c => new { c.StartDate, c.EndDate });
        });

        //ServiceRequest configuration
        modelBuilder.Entity<ServiceRequest>(entity =>
        {
            entity.HasKey(sr => sr.Id);

            entity.Property(sr => sr.Description)
                  .IsRequired()
                  .HasMaxLength(1000);

            entity.Property(sr => sr.CostInSourceCurrency)
                  .HasColumnType("decimal(18,2)");

            entity.Property(sr => sr.CostInZAR)
                  .HasColumnType("decimal(18,2)");

            entity.Property(sr => sr.ExchangeRateUsed)
                  .HasColumnType("decimal(18,6)");

            entity.Property(sr => sr.SourceCurrency)
                  .IsRequired()
                  .HasMaxLength(3);

            entity.Property(sr => sr.Status)
                  .IsRequired()
                  .HasMaxLength(50);

            // One-to-Many: Contract -> ServiceRequests
            entity.HasOne(sr => sr.Contract)
                  .WithMany(c => c.ServiceRequests)
                  .HasForeignKey(sr => sr.ContractId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(sr => sr.ContractId);
        });
    }
}