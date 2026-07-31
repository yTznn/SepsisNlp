using Microsoft.EntityFrameworkCore;
using SepsisNlp.Application.Common.Interfaces;
using SepsisNlp.Domain.Entities;
using SepsisNlp.Domain.Security;

namespace SepsisNlp.Infrastructure.Data.Context;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Patient> Patients { get; set; }
    public DbSet<PatientMapping> PatientMappings { get; set; }
    public DbSet<Attendance> Attendances { get; set; }
    public DbSet<AttendanceMapping> AttendanceMappings { get; set; }
    public DbSet<PatientEvolution> PatientEvolutions { get; set; }
    public DbSet<EvolutionProfessionalMapping> EvolutionProfessionalMappings { get; set; }
    public DbSet<InferenceResult> InferenceResults { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        modelBuilder.Entity<EvolutionProfessionalMapping>()
               .ToTable("EvolutionProfessionalMappings", "security");
    }
}