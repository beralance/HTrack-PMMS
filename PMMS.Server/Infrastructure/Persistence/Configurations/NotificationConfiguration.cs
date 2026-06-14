using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PMMS.Server.Domain.Entities;
using PMMS.Server.Domain.Enums;

namespace PMMS.Server.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure (EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");



        builder.HasKey(n => n.Id);



        builder.Property(n => n.Title)
            .HasMaxLength(100)
            .IsRequired();
        builder.HasIndex(n => n.Title);



        builder.Property(n => n.Content)
            .HasMaxLength(300)
            .IsRequired();



        builder.Property(n => n.Type)
            .HasDefaultValue(NotificationTypes.None)
            .HasConversion<string>()
            .HasMaxLength(50);
        


        builder.Property(n => n.IsRead)
            .HasDefaultValue(false)
            .IsRequired();


        
        builder.Property(n => n.IsDeleted)
            .HasDefaultValue(false)
            .IsRequired();
        builder.HasIndex(p => p.IsDeleted);
        builder.HasQueryFilter(p => p.IsDeleted == false);



        builder.Property(n => n.CreatedAt)
            .HasDefaultValueSql("now()")
            .IsRequired();



        builder.HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Restrict);



        builder.HasOne(n => n.DeletedBy)
            .WithMany()
            .HasForeignKey(n => n.DeletedById)
            .OnDelete(DeleteBehavior.Restrict);



        builder.HasOne(n => n.Project)
            .WithMany()
            .HasForeignKey(n => n.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);



        builder.HasOne(n => n.Province)
            .WithMany()
            .HasForeignKey(n => n.ProvinceId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
        

        
        builder.HasOne(n => n.Sender)
            .WithMany()
            .HasForeignKey(u => u.SenderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
