using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PMMS.Server.Domain.Entities;

namespace PMMS.Server.Infrastructure.Persistence.Configurations;
public class AssignmentConfiguration : IEntityTypeConfiguration<Assignment>
{
    public void Configure(EntityTypeBuilder<Assignment> builder) 
    {
        builder.ToTable("Assignments");



        builder.HasKey(a => a.Id);
        


        builder.Property(a => a.CreatedAt)
            .HasDefaultValueSql("now()")
            .IsRequired();
        


        builder.HasOne(a => a.User) 
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);



        builder.HasOne(a => a.Province)
            .WithMany()
            .HasForeignKey(a => a.ProvinceId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
    }
}
