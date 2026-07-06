using MediatR;
using Microsoft.EntityFrameworkCore;
using SepsisNlp.Application.Common.Interfaces;

namespace SepsisNlp.Application.Patients.Queries.GetPatientRollback;

public record GetPatientRollbackQuery(string Pseudonym) : IRequest<PatientRollbackDto?>;

public class GetPatientRollbackQueryHandler : IRequestHandler<GetPatientRollbackQuery, PatientRollbackDto?>
{
    private readonly IApplicationDbContext _context;

    public GetPatientRollbackQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PatientRollbackDto?> Handle(GetPatientRollbackQuery request, CancellationToken cancellationToken)
    {
        // Vai direto no cofre e faz o JOIN com o paciente para cruzar o pseudônimo
        var mapping = await _context.PatientMappings
            .Include(m => m.Patient)
            .FirstOrDefaultAsync(m => m.Patient.Pseudonym == request.Pseudonym, cancellationToken);

        if (mapping == null) return null;

        return new PatientRollbackDto(
            mapping.Patient.Pseudonym,
            mapping.RealMedicalRecord,
            mapping.RealName
        );
    }
}