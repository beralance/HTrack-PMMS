using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PMMS.Server.Domain.Entities;
using PMMS.Server.Domain.Enums;

namespace PMMS.Server.Infrastructure.Persistence.Configurations;

public class ExpirationConfiguration : IEntityTypeConfiguration<Expiration>
{
    public void Configure(EntityTypeBuilder<Expiration> builder)
    {
        builder.ToTable("Expirations");



        builder.HasKey(e => e.Id);



        builder.Property(e => e.Type)
            .HasDefaultValue(ExpirationTypes.None)
            .HasConversion<string>()
            .HasMaxLength(20);
        builder.HasIndex(e => e.Type);



        builder.Property(e => e.Status)
            .HasDefaultValue(ExpirationStatuses.None)
            .HasConversion<string>()
            .HasMaxLength(20);
        builder.HasIndex(e => e.Status);



        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .IsRequired();

        builder.Property(e => e.IsActive) 
            .HasDefaultValue(true);
    
        // Delete expirations related to a Project
        builder.HasOne(e => e.Project)
            .WithMany()
            .HasForeignKey(e => e.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.HandledBy)
            .WithMany()
            .HasForeignKey(e => e.HandledById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.CancelledBy)
            .WithMany()
            .HasForeignKey(e => e.CancelledById)
            .OnDelete(DeleteBehavior.Restrict);
    }

}
