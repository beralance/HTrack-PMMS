using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PMMS.Server.Domain.Entities;

namespace PMMS.Server.Infrastructure.Persistence.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder) 
    {
        builder.ToTable("Users");



        builder.Property(u => u.IsDeleted)
            .HasDefaultValue(false)
            .IsRequired();
        builder.HasIndex(u => u.IsDeleted);
        builder.HasQueryFilter(u => u.IsDeleted == false);



        builder.Property(u => u.IsTemporary)
            .HasDefaultValue(false)
            .IsRequired();



        builder.HasIndex(u => u.IsTemporary);



        builder.Property(u => u.CreatedAt)
            .HasDefaultValueSql("now()")
            .IsRequired();



        builder.HasIndex(u => u.DeletedAt);
    }
}
