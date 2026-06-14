using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PMMS.Server.Domain.Entities;

namespace PMMS.Server.Infrastructure.Persistence.Configurations;
public class DraftConfiguration : IEntityTypeConfiguration<Draft>
{
    public void Configure(EntityTypeBuilder<Draft> builder) {
        builder.ToTable("Drafts");



        builder.HasKey(d => d.Id);



        builder.Property(d => d.ProjectName)
            .HasMaxLength(256)
            .HasColumnType("citext")
            .IsRequired();
        builder.HasIndex(d => d.ProjectName)
            .IsUnique();



        builder.Property(d => d.Developer)
            .HasMaxLength(128)
            .IsRequired();
        builder.HasIndex(d => d.Developer);


        
        builder.Property(p => p.Owner)
            .HasMaxLength(100)
            .IsRequired();



        builder.Property(d => d.Barangay)
            .HasMaxLength(256)
            .IsRequired();



        builder.Property(d => d.Salable)
            .HasMaxLength(50)
            .IsRequired();



        builder.Property(d => d.IsFm)
            .HasDefaultValue(false)
            .IsRequired();



        builder.Property(d => d.HasCoc)
            .HasDefaultValue(false);



        builder.Property(d => d.HasDod)
            .HasDefaultValue(false);
        


        builder.Property(d => d.DateOfCompletion) 
            .IsRequired();



        builder.Property(d => d.Remarks)
            .HasMaxLength(256);



        builder.Property(d => d.IsPublished)
            .HasDefaultValue(false);
        builder.HasQueryFilter(d => d.IsPublished == false);
        



        builder.Property(d => d.CreatedAt)
            .HasDefaultValueSql("now()")
            .IsRequired();



        builder.HasOne(d => d.AddedBy)
            .WithMany()
            .HasForeignKey(d => d.AddedById)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();



        builder.HasOne(d => d.Municipality)
            .WithMany()
            .HasForeignKey(d => d.MunicipalityId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();



        builder.HasOne(d => d.ProjectType)
            .WithMany()
            .HasForeignKey(d => d.ProjectTypeId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();



        builder.HasOne(d => d.PublishedBy)
            .WithMany()
            .HasForeignKey(d => d.PublishedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
