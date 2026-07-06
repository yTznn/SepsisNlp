using MediatR;
using Microsoft.EntityFrameworkCore;
using SepsisNlp.Application.Common.Interfaces;

namespace SepsisNlp.Application.Patients.Commands.ComplementDischargeData;

public record ComplementDischargeDataCommand(
    string RealAttendanceNumber,
    string RealMedicalRecord,
    string Cid,
    string? CidDescription,
    string Gender,
    DateOnly? DischargeDate
) : IRequest;

public class ComplementDischargeDataCommandHandler : IRequestHandler<ComplementDischargeDataCommand>
{
    private readonly IApplicationDbContext _context;

    public ComplementDischargeDataCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ComplementDischargeDataCommand request, CancellationToken cancellationToken)
    {
        // 1. Busca o Atendimento no cofre pelo número real do MV
        var attendanceMapping = await _context.AttendanceMappings
            .FirstOrDefaultAsync(m => m.RealAttendanceNumber == request.RealAttendanceNumber, cancellationToken);

        if (attendanceMapping != null)
        {
            var attendance = await _context.Attendances
                .FirstOrDefaultAsync(a => a.Id == attendanceMapping.AttendanceId, cancellationToken);

            if (attendance != null)
            {
                // Agora passando a descrição também!
                attendance.ComplementDischargeData(request.Cid, request.CidDescription, request.DischargeDate);
            }
        }

        // 2. Busca o Paciente no cofre pelo prontuário real para atualizar o Sexo
        var patientMapping = await _context.PatientMappings
            .Include(m => m.Patient)
            .FirstOrDefaultAsync(m => m.RealMedicalRecord == request.RealMedicalRecord, cancellationToken);

        if (patientMapping != null)
        {
            patientMapping.Patient.UpdateGender(request.Gender);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}