using MediatR;
using Microsoft.EntityFrameworkCore;
using SepsisNlp.Application.Common.Interfaces;
using SepsisNlp.Domain.Entities;
using SepsisNlp.Domain.Security;

namespace SepsisNlp.Application.Patients.Commands.ImportPatient;

// O DTO que vai carregar os dados fatiados do CSV
public record ImportPatientWithAttendancesCommand(
    string RealMedicalRecord,
    string RealName,
    List<string> RealAttendanceNumbers
) : IRequest;

// O Handler que executa a gravação no banco
public class ImportPatientWithAttendancesCommandHandler : IRequestHandler<ImportPatientWithAttendancesCommand>
{
    private readonly IApplicationDbContext _context;

    public ImportPatientWithAttendancesCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ImportPatientWithAttendancesCommand request, CancellationToken cancellationToken)
    {
        // 1. Verifica se o paciente já existe no cofre
        var patientMapping = await _context.PatientMappings
            .Include(m => m.Patient)
            .FirstOrDefaultAsync(m => m.RealMedicalRecord == request.RealMedicalRecord, cancellationToken);

        Patient patient;

        if (patientMapping == null)
        {
            // Cria paciente novo e anonimiza
            var pseudonym = $"PAC-{Guid.NewGuid().ToString("N")[..15].ToUpper()}";
            patient = new Patient(pseudonym);
            _context.Patients.Add(patient);

            var newMapping = new PatientMapping(patient.Id, request.RealMedicalRecord, request.RealName);
            _context.PatientMappings.Add(newMapping);
        }
        else
        {
            patient = patientMapping.Patient;
        }

        // 2. Processa a lista de atendimentos amarrados a ele
        foreach (var realAttendance in request.RealAttendanceNumbers)
        {
            // Verifica se este atendimento já foi importado antes (Idempotência)
            var attendanceExists = await _context.AttendanceMappings
                .AnyAsync(m => m.RealAttendanceNumber == realAttendance, cancellationToken);

            if (!attendanceExists)
            {
                // Cria atendimento novo e anonimiza
                var atdPseudonym = $"ATD-{Guid.NewGuid().ToString("N")[..15].ToUpper()}";
                var attendance = new Attendance(patient.Id, atdPseudonym);
                _context.Attendances.Add(attendance);

                var atdMapping = new AttendanceMapping(attendance.Id, realAttendance);
                _context.AttendanceMappings.Add(atdMapping);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}