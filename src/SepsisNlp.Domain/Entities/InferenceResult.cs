using System;

namespace SepsisNlp.Domain.Entities;

public class InferenceResult
{
    // CORRIGIDO AQUI: O Id original da sua tabela era Guid, vamos manter!
    public Guid Id { get; set; }

    public Guid AttendanceId { get; set; }
    public Guid PatientEvolutionId { get; set; }

    public int GrupoAmostral { get; set; }
    public int Cenario { get; set; }
    public int PromptId { get; set; }
    public string TextoUtilizado { get; set; } = string.Empty;
    public string Predicao { get; set; } = string.Empty;
    public decimal Confianca { get; set; }
    public string ModeloUtilizado { get; set; } = string.Empty;
}