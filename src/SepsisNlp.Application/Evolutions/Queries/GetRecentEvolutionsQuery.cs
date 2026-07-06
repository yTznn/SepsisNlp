using MediatR;
using Microsoft.EntityFrameworkCore;
using SepsisNlp.Application.Common.Interfaces;

namespace SepsisNlp.Application.Evolutions.Queries.GetRecentEvolutions;

// 1. Adicionamos o RawText aqui!
public record EvolutionDto(
    string OriginalEvolutionCode,
    string DataHora,
    string Type,
    string ProfessionalRole,
    string RawText
);

public record GetRecentEvolutionsQuery() : IRequest<List<EvolutionDto>>;

public class GetRecentEvolutionsQueryHandler : IRequestHandler<GetRecentEvolutionsQuery, List<EvolutionDto>>
{
    private readonly IApplicationDbContext _context;

    public GetRecentEvolutionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<EvolutionDto>> Handle(GetRecentEvolutionsQuery request, CancellationToken cancellationToken)
    {
        return await _context.PatientEvolutions
            .OrderByDescending(e => e.EvolutionDate)
            .ThenByDescending(e => e.EvolutionTime)
            .Take(50)
            .Select(e => new EvolutionDto(
                e.OriginalEvolutionCode,
                $"{e.EvolutionDate:dd/MM/yyyy} {e.EvolutionTime:hh\\:mm}",
                e.Type.ToString(),
                e.ProfessionalRole ?? "NÃO INFORMADO",
                e.RawText // 2. E mapeamos ele aqui!
            ))
            .ToListAsync(cancellationToken);
    }
}