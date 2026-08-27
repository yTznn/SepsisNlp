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
    private HashSet<string> _cacheProcessados = new();

    public RunExperimentCommandHandler(IApplicationDbContext context, IPythonNlpClient nlpClient)
    {
        _context = context;
        _nlpClient = nlpClient;
    }

    public async Task<string> Handle(RunExperimentCommand request, CancellationToken cancellationToken)
    {
        // 1. CARREGA O CHECKPOINT DO QUE JÁ FOI FEITO (NUNCA MAIS APAGA O BANCO)
        var resultadosExistentes = await _context.InferenceResults
            .Select(r => $"{r.PatientEvolutionId}_{r.ModeloUtilizado}_{r.Cenario}_{r.PromptId}")
            .ToListAsync(cancellationToken);

        _cacheProcessados = new HashSet<string>(resultadosExistentes);
        int totalAtendimentosProcessados = 0;

        // =========================================================
        // FRENTE 1: 100 PACIENTES COM SEPSE
        // =========================================================
        var grupo1 = await _context.Attendances
            .Include(a => a.Evolutions)
            .Where(a => a.DischargeCid != null &&
                       (a.DischargeCid.StartsWith("A40") ||
                        a.DischargeCid.StartsWith("A41") ||
                        a.DischargeCid.StartsWith("R65.2") ||
                        a.DischargeCid.StartsWith("R57.2")) &&
                        a.Evolutions.Any())
            .Take(100)
            .ToListAsync(cancellationToken);

        await ProcessarAtendimentos(grupo1, 1, cancellationToken);
        totalAtendimentosProcessados += grupo1.Count;

        // =========================================================
        // FRENTE 2: 100 PACIENTES CONTROLE LIMPO (TRAUMA)
        // =========================================================
        var grupo2 = await _context.Attendances
            .Include(a => a.Evolutions)
            .Where(a => a.DischargeCid != null &&
                       (a.DischargeCid.StartsWith("S") ||
                        a.DischargeCid.StartsWith("M") ||
                        a.DischargeCid.StartsWith("T")) &&
                       !a.DischargeCid.StartsWith("A") &&
                       !a.DischargeCid.StartsWith("J15") &&
                        a.Evolutions.Any())
            .Take(100)
            .ToListAsync(cancellationToken);

        await ProcessarAtendimentos(grupo2, 2, cancellationToken);
        totalAtendimentosProcessados += grupo2.Count;

        // =========================================================
        // FRENTE 3: 100 PACIENTES MIMETIZADORES
        // =========================================================
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
                            e.RawText.ToLower().Contains("infecção grave")))
            .Take(100)
            .ToListAsync(cancellationToken);

        await ProcessarAtendimentos(grupo3, 3, cancellationToken);
        totalAtendimentosProcessados += grupo3.Count;

        return $"BATERIA RETOMADA E CONCLUÍDA! O pipeline processou apenas as evoluções que faltavam.";
    }

    private async Task ProcessarAtendimentos(List<Attendance> atendimentos, int grupoId, CancellationToken cancellationToken)
    {
        var modelosAlvo = new[] {
            "emilyalsentzer/Bio_ClinicalBERT",
            "dmis-lab/biobert-v1.1",
            "pucpr/biobertpt-all",
            "google/medgemma-1.5-4b-it"
        };

        var cenarios = new[] { 1, 2, 3 };

        foreach (var atendimento in atendimentos)
        {
            foreach (var evolucao in atendimento.Evolutions)
            {
                var textoLimpo = TextNormalizerHelper.NormalizarTextoClinico(evolucao.RawText);

                foreach (var modelo in modelosAlvo)
                {
                    bool isLLM = modelo.Contains("gemma");
                    var prompts = isLLM ? new[] { 2, 3 } : new[] { 1 };

                    foreach (var promptId in prompts)
                    {
                        foreach (var cenario in cenarios)
                        {
                            // A CHAVE DE ACESSO DO CHECKPOINT
                            string chaveAcesso = $"{evolucao.Id}_{modelo}_{cenario}_{promptId}";

                            // SE JÁ EXISTE NO BANCO, PULA!
                            if (_cacheProcessados.Contains(chaveAcesso))
                            {
                                continue;
                            }

                            string textoBase = (cenario == 1) ? evolucao.RawText : textoLimpo;

                            var requestPayload = new EvolucaoRequest(textoBase, cenario, modelo, promptId);
                            var response = await _nlpClient.ProcessarEvolucaoAsync(requestPayload, cancellationToken);

                            if (response == null)
                                throw new Exception($"[DEBUG HTTP] Requisição falhou para o modelo {modelo}!");

                            _context.InferenceResults.Add(new InferenceResult
                            {
                                Id = Guid.NewGuid(),
                                AttendanceId = atendimento.Id,
                                PatientEvolutionId = evolucao.Id,
                                GrupoAmostral = grupoId,
                                Cenario = cenario,
                                PromptId = promptId,
                                TextoUtilizado = response.texto_processado ?? textoBase,
                                Predicao = response.predicao,
                                Confianca = (decimal)response.confianca,
                                ModeloUtilizado = response.modelo
                            });

                            // SALVA NO BANCO IMEDIATAMENTE APÓS CADA TEXTO!
                            await _context.SaveChangesAsync(cancellationToken);
                            _cacheProcessados.Add(chaveAcesso);
                        }
                    }
                }
            }
        }
    }
}