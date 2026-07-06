using MediatR;
using Microsoft.EntityFrameworkCore;
using SepsisNlp.Application.Common.Interfaces;
using SepsisNlp.Domain.Entities;
using SepsisNlp.Domain.Enums;
using SepsisNlp.Domain.Security;

namespace SepsisNlp.Application.Evolutions.Commands.ImportEvolution;

public record ImportEvolutionCommand(
    string OriginalEvolutionCode,
    EvolutionType Type,
    string AttendanceNumber,
    string? Cid,
    DateOnly EvolutionDate,
    TimeSpan EvolutionTime,
    string? ProfessionalRole,
    string? ProfessionalCouncil,
    string? ProfessionalName,
    string RawText,
    DateOnly? PatientBirthDate
) : IRequest<Guid>;

public class ImportEvolutionCommandHandler : IRequestHandler<ImportEvolutionCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public ImportEvolutionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(ImportEvolutionCommand request, CancellationToken cancellationToken)
    {
        var evolutionExists = await _context.PatientEvolutions
            .AnyAsync(e => e.OriginalEvolutionCode == request.OriginalEvolutionCode, cancellationToken);

        if (evolutionExists) return Guid.Empty;

        var attendanceMapping = await _context.AttendanceMappings
            .FirstOrDefaultAsync(m => m.RealAttendanceNumber == request.AttendanceNumber, cancellationToken);

        if (attendanceMapping == null)
            throw new Exception($"Atendimento {request.AttendanceNumber} não encontrado.");

        // =========================================================================
        // MÁGICA DA DATA DE NASCIMENTO: Atualiza o paciente se a data veio no CSV
        // =========================================================================
        if (request.PatientBirthDate.HasValue)
        {
            var attendance = await _context.Attendances
                .Include(a => a.Patient) // Puxa o paciente junto
                .FirstOrDefaultAsync(a => a.Id == attendanceMapping.AttendanceId, cancellationToken);

            if (attendance != null && attendance.Patient.BirthDate == null)
            {
                attendance.Patient.UpdateBirthDate(request.PatientBirthDate.Value);
            }
        }

        // =========================================================================
        // BLINDAGEM CONTRA O ERRO DE VARCHAR(200) DO POSTGRESQL
        // =========================================================================
        var safeRole = request.ProfessionalRole?.Length > 200
            ? request.ProfessionalRole.Substring(0, 200)
            : request.ProfessionalRole;

        var safeName = request.ProfessionalName?.Length > 200
            ? request.ProfessionalName.Substring(0, 200)
            : request.ProfessionalName;

        var safeCouncil = request.ProfessionalCouncil?.Length > 200
            ? request.ProfessionalCouncil.Substring(0, 200)
            : request.ProfessionalCouncil;

        // 1. Instancia a nova entidade PatientEvolution
        var evolution = new PatientEvolution(
            attendanceMapping.AttendanceId,
            request.OriginalEvolutionCode,
            request.Type,
            request.Cid,
            request.EvolutionDate,
            request.EvolutionTime,
            safeRole,
            request.RawText
        );

        _context.PatientEvolutions.Add(evolution);

        // 2. Salva o Profissional no Cofre de Segurança
        if (!string.IsNullOrWhiteSpace(safeName) || !string.IsNullOrWhiteSpace(safeCouncil))
        {
            var profMapping = new EvolutionProfessionalMapping(
                request.OriginalEvolutionCode,
                safeName ?? "NÃO INFORMADO",
                safeCouncil ?? "NÃO INFORMADO"
            );
            _context.EvolutionProfessionalMappings.Add(profMapping);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return evolution.Id;
    }
}