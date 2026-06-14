using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PMMS.Server.Domain.Entities;

namespace PMMS.Server.Infrastructure.Persistence.Configurations;

public class ProjectTypeConfiguration : IEntityTypeConfiguration<ProjectType>
{
    public void Configure (EntityTypeBuilder<ProjectType> builder)
    {
        builder.ToTable("ProjectTypes");
        


        builder.HasKey(pt => pt.Id);



        builder.Property(pt => pt.Type)
            .HasMaxLength(50)
            .IsRequired();
        builder.HasIndex(pt => pt.Type);



        builder.Property(pt => pt.Meaning)
            .HasMaxLength(100)
            .IsRequired();



        builder.HasData(
            new {Id = 1, Type = "OMS", Meaning = "Open Market Subdivision"},
            new {Id = 2, Type = "OMC", Meaning = "Open Market Condominium"},
            new {Id = 3, Type = "MCHS", Meaning = "Medium Cost Housing Subdivision"},
            new {Id = 4, Type = "MCHC", Meaning = "Medium Cost Housing Condominium"},
            new {Id = 5, Type = "EHS", Meaning = "Economic Housing Subdivision"},
            new {Id = 6, Type = "EHC", Meaning = "Economic Housing Condominium"},
            new {Id = 7, Type = "SHS", Meaning = "Socialized Housing Subdivision"},
            new {Id = 8, Type = "SHC", Meaning = "Socialized Housing Condominium"},
            new {Id = 9, Type = "CS", Meaning = "Commercial Subdivision"},
            new {Id = 10, Type = "CC", Meaning = "Commercial Condominium"},
            new {Id = 11, Type = "IS", Meaning = "Industrial Subdivision"},
            new {Id = 12, Type = "FLS", Meaning = "Farm Lot Subdivision"},
            new {Id = 13, Type = "MP", Meaning = "Memorial Park"},
            new {Id = 14, Type = "Col", Meaning = "Columbarium"}
        );
    }
}