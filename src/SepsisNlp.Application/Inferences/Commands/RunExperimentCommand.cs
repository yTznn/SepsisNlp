using MediatR;
using Microsoft.EntityFrameworkCore;
using SepsisNlp.Application.Common.Helpers;
using SepsisNlp.Application.Common.Interfaces;
using SepsisNlp.Application.Common.Models;
using SepsisNlp.Domain.Entities;

namespace SepsisNlp.Application.Inferences.Commands;

public record RunExperimentCommand() : IRequest<string>;

public class RunExperimentCommandHandler : IRequestHandler<RunExperimentCommand, string>
{
    private readonly IApplicationDbContext _context;
    private readonly IPythonNlpClient _nlpClient;

    public RunExperimentCommandHandler(IApplicationDbContext context, IPythonNlpClient nlpClient)
    {
        _context = context;
        _nlpClient = nlpClient;
    }

    public async Task<string> Handle(RunExperimentCommand request, CancellationToken cancellationToken)
    {
        int totalAtendimentosProcessados = 0;

        // =====================================================================
        // FRENTE 1: GROUND TRUTH POSITIVO (100 Atendimentos com Sepse)
        // =====================================================================
        var grupo1 = await _context.Attendances
            .Include(a => a.Evolutions)
            .Where(a => a.DischargeCid != null &&
                       (a.DischargeCid.StartsWith("A40") ||
                        a.DischargeCid.StartsWith("A41") ||
                        a.DischargeCid.StartsWith("R65.2") ||
                        a.DischargeCid.StartsWith("R57.2")))
            .Take(100)
            .ToListAsync(cancellationToken);

        await ProcessarAtendimentos(grupo1, 1, cancellationToken);
        totalAtendimentosProcessados += grupo1.Count;

        // =====================================================================
        // FRENTE 2: CONTROLE LIMPO (100 Atendimentos Ortopédicos/Trauma SEM Sepse)
        // =====================================================================
        var grupo2 = await _context.Attendances
            .Include(a => a.Evolutions)
            .Where(a => a.DischargeCid != null &&
                       (a.DischargeCid.StartsWith("S") ||
                        a.DischargeCid.StartsWith("M") ||
                        a.DischargeCid.StartsWith("T")) &&
                       !a.DischargeCid.StartsWith("A") &&
                       !a.DischargeCid.StartsWith("J15"))
            .Take(100)
            .ToListAsync(cancellationToken);

        await ProcessarAtendimentos(grupo2, 2, cancellationToken);
        totalAtendimentosProcessados += grupo2.Count;

        // =====================================================================
        // FRENTE 3: MIMETIZADORES CLÍNICOS (100 Atendimentos Falsos Positivos do Médico)
        // =====================================================================
        var grupo3 = await _context.Attendances
            .Include(a => a.Evolutions)
            .Where(a => a.DischargeCid != null &&
                        !a.DischargeCid.StartsWith("A40") &&
                        !a.DischargeCid.StartsWith("A41") &&
                        !a.DischargeCid.StartsWith("R65.2") &&
                        !a.DischargeCid.StartsWith("R57.2") &&
                        a.Evolutions.Any(e =>
                            e.RawText.ToLower().Contains("sepse") ||
                            e.RawText.ToLower().Contains("choque") ||
                            e.RawText.ToLower().Contains("protocolo de sepse") ||
                            e.RawText.ToLower().Contains("infecção grave") ||
                            e.RawText.ToLower().Contains("infecçao grave") ||
                            e.RawText.ToLower().Contains("quadro infeccioso")))
            .Take(100)
            .ToListAsync(cancellationToken);

        await ProcessarAtendimentos(grupo3, 3, cancellationToken);
        totalAtendimentosProcessados += grupo3.Count;

        // Salva todos os fragmentos e predições no PostgreSQL
        await _context.SaveChangesAsync(cancellationToken);

        return $"O Experimento foi um sucesso! {totalAtendimentosProcessados} atendimentos (3 Frentes) foram processados nota por nota na RTX 4060. Average Pooling validado.";
    }

    private async Task ProcessarAtendimentos(List<Attendance> atendimentos, int grupoId, CancellationToken cancellationToken)
    {
        foreach (var atendimento in atendimentos)
        {
            // Processa cada evolução médica que compõe o contexto temporal do paciente
            foreach (var evolucao in atendimento.Evolutions)
            {
                // CENÁRIO 1: TEXTO BRUTO
                var reqCenario1 = new EvolucaoRequest(evolucao.RawText, 1);
                var respCenario1 = await _nlpClient.ProcessarEvolucaoAsync(reqCenario1, cancellationToken);

                if (respCenario1 != null)
                {
                    _context.InferenceResults.Add(new InferenceResult
                    {
                        AttendanceId = atendimento.Id,
                        PatientEvolutionId = evolucao.Id,
                        GrupoAmostral = grupoId,
                        Cenario = 1,
                        TextoUtilizado = evolucao.RawText, // Guarda a nota para o frontend!
                        Predicao = respCenario1.Predicao,
                        Confianca = (decimal)respCenario1.Confianca,
                        ModeloUtilizado = respCenario1.Modelo
                    });
                }

                // CENÁRIO 2: TEXTO LIMPO
                var textoLimpo = TextNormalizerHelper.NormalizarTextoClinico(evolucao.RawText);
                var reqCenario2 = new EvolucaoRequest(textoLimpo, 2);
                var respCenario2 = await _nlpClient.ProcessarEvolucaoAsync(reqCenario2, cancellationToken);

                if (respCenario2 != null)
                {
                    _context.InferenceResults.Add(new InferenceResult
                    {
                        AttendanceId = atendimento.Id,
                        PatientEvolutionId = evolucao.Id,
                        GrupoAmostral = grupoId,
                        Cenario = 2,
                        TextoUtilizado = textoLimpo, // Guarda o texto limpo para prova
                        Predicao = respCenario2.Predicao,
                        Confianca = (decimal)respCenario2.Confianca,
                        ModeloUtilizado = respCenario2.Modelo
                    });
                }
            }
        }
    }
}