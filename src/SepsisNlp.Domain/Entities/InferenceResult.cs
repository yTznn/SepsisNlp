namespace SepsisNlp.Domain.Entities;

public class InferenceResult
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AttendanceId { get; set; } // Vincula ao atendimento inteiro
    public Guid PatientEvolutionId { get; set; } // Vincula à nota específica

    public int GrupoAmostral { get; set; } // 1 (Sepse Real), 2 (Controle), 3 (Mimetizador)
    public int Cenario { get; set; } // 1 (Bruto) ou 2 (Limpo)

    public string TextoUtilizado { get; set; } = string.Empty; // O exato pedaço lido!

    public string Predicao { get; set; } = string.Empty;
    public decimal Confianca { get; set; }
    public string ModeloUtilizado { get; set; } = string.Empty;
    public DateTime DataProcessamento { get; set; } = DateTime.UtcNow;
}