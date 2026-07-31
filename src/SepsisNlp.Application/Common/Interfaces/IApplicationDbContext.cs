using Microsoft.EntityFrameworkCore;
using SepsisNlp.Domain.Entities;
using SepsisNlp.Domain.Security;

namespace SepsisNlp.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Patient> Patients { get; }
    DbSet<PatientMapping> PatientMappings { get; }
    DbSet<Attendance> Attendances { get; }
    DbSet<AttendanceMapping> AttendanceMappings { get; }
    DbSet<PatientEvolution> PatientEvolutions { get; }
    DbSet<EvolutionProfessionalMapping> EvolutionProfessionalMappings { get; }
    DbSet<InferenceResult> InferenceResults { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}