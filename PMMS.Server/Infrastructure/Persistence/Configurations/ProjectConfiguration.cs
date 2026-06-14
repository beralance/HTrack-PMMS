using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PMMS.Server.Domain.Entities;
using PMMS.Server.Domain.Enums;

namespace PMMS.Server.Infrastructure.Persistence.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder) 
    {
        builder.ToTable("Projects");



        builder.HasKey(p => p.Id);



        builder.Property(p => p.ProjectName)
            .HasMaxLength(100)
            .HasColumnType("citext")
            .IsRequired();
        builder.HasIndex(p => p.ProjectName)
            .IsUnique();



        builder.Property(p => p.Developer)
            .HasMaxLength(100)
            .IsRequired();



        builder.Property(p => p.Owner)
            .HasMaxLength(100)
            .IsRequired();



        builder.Property(p => p.CrNo)
            .HasMaxLength(20)
            .IsRequired();



        builder.Property(p => p.CrNo)
            .HasMaxLength(20)
            .IsRequired();



        builder.Property(p => p.Barangay)
            .HasMaxLength(200)
            .IsRequired();



        builder.Property(p => p.Salable)
            .HasMaxLength(20)
            .IsRequired();



        builder.Property(p => p.IsFm)
            .HasDefaultValue(false)
            .IsRequired();



        builder.Property(p => p.HasCoc)
            .HasDefaultValue(false);



        builder.Property(p => p.HasDod)
            .HasDefaultValue(false);



        builder.Property(p => p.Status)
            .HasDefaultValue(ProjectStatuses.None)
            .HasConversion<string>()
            .HasMaxLength(50);


        builder.Property(p => p.Severity)
            .HasDefaultValue(ProjectSeverities.Normal)
            .HasConversion<string>()
            .HasMaxLength(50);


        builder.Property(p => p.Remarks)
            .HasMaxLength(200);


        builder.Property(p => p.IsExtended)
            .HasDefaultValue(false)
            .IsRequired();


        builder.Property(p => p.IsDeleted)
            .HasDefaultValue(false)
            .IsRequired();
        builder.HasIndex(p => p.IsDeleted);
        builder.HasQueryFilter(p => p.IsDeleted == false);



        builder.Property(p => p.IsModified)
            .HasDefaultValue(false);



        builder.Property(p => p.IsPublished)
            .HasDefaultValue(false);



        builder.Property(p => p.SetupStatus)
            .HasDefaultValue(ProjectSetupStatuses.None)
            .HasConversion<string>()
            .HasMaxLength(50);



        builder.Property(p => p.CreatedAt)
            .HasDefaultValueSql("now()")
            .IsRequired();



        builder.HasOne(p => p.ModifiedBy)
            .WithMany()
            .HasForeignKey(p => p.ModifiedById)
            .OnDelete(DeleteBehavior.Restrict);



        builder.HasOne(p => p.AddedBy)
            .WithMany()
            .HasForeignKey(p => p.AddedById)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();



        builder.HasOne(p => p.DeletedBy)
            .WithMany()
            .HasForeignKey(p => p.DeletedById)
            .OnDelete(DeleteBehavior.Restrict);



        builder.HasOne(p => p.Municipality)
            .WithMany()
            .HasForeignKey(p => p.MunicipalityId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();



        builder.HasOne(p => p.ProjectType)
            .WithMany()
            .HasForeignKey(p => p.ProjectTypeId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
    }
}
