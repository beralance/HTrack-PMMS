using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PMMS.Server.Domain.Entities;

namespace PMMS.Server.Infrastructure.Persistence.Configurations;
public class MunicipalityConfiguration : IEntityTypeConfiguration<Municipality>
{
    public void Configure (EntityTypeBuilder<Municipality> builder)
    {
        builder.ToTable("Municipalities");



        builder.HasKey(m => m.Id);



        builder.Property(m => m.MunicipalityName)
            .HasMaxLength(100)
            .IsRequired();
        builder.HasIndex(m => m.MunicipalityName);



        builder.HasOne(m => m.Province)
            .WithMany()
            .HasForeignKey(m => m.ProvinceId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
    }   
}
