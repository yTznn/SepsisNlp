using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SepsisNlp.Domain.Entities;

namespace SepsisNlp.Infrastructure.Data.Configurations;

public class PatientEvolutionConfiguration : IEntityTypeConfiguration<PatientEvolution>
{
    public void Configure(EntityTypeBuilder<PatientEvolution> builder)
    {
        // 1. Novo nome da tabela
        builder.ToTable("PatientEvolutions", "clinical");

        builder.HasKey(e => e.Id);

        // 2. Mapeamentos originais mantidos e otimizados
        builder.Property(e => e.OriginalEvolutionCode).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Type).IsRequired().HasConversion<string>();
        builder.Property(e => e.RawText).IsRequired().HasColumnType("text");
        builder.Property(e => e.Cid).HasMaxLength(10);

        // 3. Novas colunas (Data, Hora e Profissional)
        builder.Property(e => e.EvolutionDate).IsRequired();
        builder.Property(e => e.EvolutionTime).IsRequired();

        builder.Property(e => e.ProfessionalRole).HasMaxLength(200);

        // 4. Relacionamento com a tabela de Atendimento
        builder.HasOne(e => e.Attendance)
               .WithMany(a => a.Evolutions)
               .HasForeignKey(e => e.AttendanceId)
               .OnDelete(DeleteBehavior.Cascade);

        // 5. Índice de performance (Para a  regra de não duplicar registros!)
        builder.HasIndex(e => e.OriginalEvolutionCode);
    }
}