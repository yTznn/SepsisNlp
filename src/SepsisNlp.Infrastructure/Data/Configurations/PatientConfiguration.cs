using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SepsisNlp.Domain.Entities;

namespace SepsisNlp.Infrastructure.Data.Configurations;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("Patients", "clinical");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Pseudonym).IsRequired().HasMaxLength(20);
        builder.HasIndex(p => p.Pseudonym).IsUnique();

        builder.Property(p => p.BirthDate).IsRequired(false);

        builder.Property(p => p.Gender).IsRequired(false).HasMaxLength(1);
    }
}