using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SepsisNlp.Application.Common.Interfaces;

namespace SepsisNlp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public ReportsController(IApplicationDbContext context)
    {
        _context = context;
    }

    // =======================================================================
    // 1. O GARIMPO: Busca quais pacientes (Pseudônimos) tiveram um CID específico
    // =======================================================================
    [HttpGet("patients-by-cid")]
    public async Task<IActionResult> GetPatientsByCid([FromQuery] string cid, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(cid)) return BadRequest("O CID é obrigatório.");

        var patients = await _context.PatientEvolutions
            .Include(e => e.Attendance)
                .ThenInclude(a => a.Patient)
            .Where(e => e.Cid != null && e.Cid.ToUpper().Contains(cid.ToUpper()))
            .Select(e => new
            {
                Prontuario = e.Attendance.Patient.Pseudonym,
                DataNascimento = e.Attendance.Patient.BirthDate != null ? e.Attendance.Patient.BirthDate.Value.ToString("dd/MM/yyyy") : "Desconhecida"
            })
            .Distinct()
            .ToListAsync(cancellationToken);

        return Ok(patients);
    }

    // =======================================================================
    // 2. A EXTRAÇÃO CIRÚRGICA: O "Suco" completo de UM paciente específico
    // =======================================================================
    [HttpGet("export/patient/{pseudonym}")]
    public async Task<IActionResult> ExportPatientDataset([FromRoute] string pseudonym, CancellationToken cancellationToken)
    {
        // Pega TODAS as evoluções de TODOS os atendimentos desse paciente específico
        var evolutions = await _context.PatientEvolutions
            .Include(e => e.Attendance)
                .ThenInclude(a => a.Patient)
            .Where(e => e.Attendance.Patient.Pseudonym == pseudonym)
            .ToListAsync(cancellationToken);

        if (!evolutions.Any()) return NotFound("Nenhuma evolução encontrada para este prontuário.");

        // O Agrupamento que o LLM ama!
        var patient = evolutions.First().Attendance.Patient;

        var dataset = new
        {
            Prontuario = patient.Pseudonym,
            Paciente = new
            {
                DataDeNascimento = patient.BirthDate?.ToString("dd/MM/yyyy") ?? "Desconhecida"
            },
            Atendimentos = evolutions
                .GroupBy(e => e.Attendance)
                .Select(aGroup => new
                {
                    CodigoAtendimento = $"ATE-{aGroup.Key.Id.ToString().Substring(0, 8).ToUpper()}",
                    Evolucoes = aGroup
                        .OrderBy(e => e.EvolutionDate)
                        .ThenBy(e => e.EvolutionTime)
                        .Select(e => new
                        {
                            CodigoEvolucao = e.OriginalEvolutionCode,
                            Data = e.EvolutionDate.ToString("dd/MM/yyyy"),
                            Hora = e.EvolutionTime.ToString(@"hh\:mm"),
                            CID = e.Cid,
                            TextoEvolucao = e.RawText
                        }).ToList()
                }).ToList()
        };

        return Ok(dataset);
    }
}