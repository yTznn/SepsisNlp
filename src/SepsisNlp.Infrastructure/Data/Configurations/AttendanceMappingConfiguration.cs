using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SepsisNlp.Domain.Entities;
using SepsisNlp.Domain.Security;

namespace SepsisNlp.Infrastructure.Data.Configurations;

public class AttendanceMappingConfiguration : IEntityTypeConfiguration<AttendanceMapping>
{
    public void Configure(EntityTypeBuilder<AttendanceMapping> builder)
    {
        builder.ToTable("AttendanceMappings", "security");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.RealAttendanceNumber).IsRequired().HasMaxLength(50);

        builder.HasOne<Attendance>()
               .WithOne()
               .HasForeignKey<AttendanceMapping>(m => m.AttendanceId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => m.RealAttendanceNumber);
    }
}