using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PMMS.Server.Domain.Entities;

namespace PMMS.Server.Infrastructure.Persistence.Configurations;

public class ProvinceConfiguration : IEntityTypeConfiguration<Province>
{
    public void Configure (EntityTypeBuilder<Province> builder)
    {
        builder.ToTable("Provinces");



        builder.HasKey(p => p.Id);



        builder.Property(p => p.ProvinceName)
            .HasMaxLength(100)
            .IsRequired();



        builder.HasData(
            new Province {Id = 1, ProvinceName = "Albay"},
            new Province {Id = 2, ProvinceName = "Camarines Norte"},
            new Province {Id = 3, ProvinceName = "Camarines Sur"},
            new Province {Id = 4, ProvinceName = "Catanduanes"},
            new Province {Id = 5, ProvinceName = "Masbate"},
            new Province {Id = 6, ProvinceName = "Sorsogon"}
        );
    }
}
