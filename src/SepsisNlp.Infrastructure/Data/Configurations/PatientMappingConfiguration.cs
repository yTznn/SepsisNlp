using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SepsisNlp.Domain.Security;

namespace SepsisNlp.Infrastructure.Data.Configurations;

public class PatientMappingConfiguration : IEntityTypeConfiguration<PatientMapping>
{
    public void Configure(EntityTypeBuilder<PatientMapping> builder)
    {
        builder.ToTable("PatientMappings", "security");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.RealMedicalRecord).IsRequired().HasMaxLength(50);
        builder.Property(m => m.RealName).HasMaxLength(200);

        builder.HasOne(m => m.Patient)
               .WithOne()
               .HasForeignKey<PatientMapping>(m => m.PatientId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => m.RealMedicalRecord);
    }
}