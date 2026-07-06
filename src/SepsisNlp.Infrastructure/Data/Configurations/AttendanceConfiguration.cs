using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SepsisNlp.Domain.Entities;

namespace SepsisNlp.Infrastructure.Data.Configurations;

public class AttendanceConfiguration : IEntityTypeConfiguration<Attendance>
{
    public void Configure(EntityTypeBuilder<Attendance> builder)
    {
        builder.ToTable("Attendances", "clinical");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Pseudonym).IsRequired().HasMaxLength(20);
        builder.HasIndex(a => a.Pseudonym).IsUnique();

        builder.Property(a => a.DischargeCid).IsRequired(false).HasMaxLength(10);
        builder.Property(a => a.DischargeCidDescription).IsRequired(false);
        builder.Property(a => a.DischargeDate).IsRequired(false);

        builder.HasOne(a => a.Patient)
               .WithMany(p => p.Attendances)
               .HasForeignKey(a => a.PatientId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}