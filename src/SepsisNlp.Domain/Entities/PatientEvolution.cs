using SepsisNlp.Domain.Common;
using SepsisNlp.Domain.Enums;

namespace SepsisNlp.Domain.Entities;

public class PatientEvolution : Entity
{
    public Guid AttendanceId { get; private set; }
    public Attendance Attendance { get; private set; } = null!;

    public string OriginalEvolutionCode { get; private set; }
    public EvolutionType Type { get; private set; }
    public string? Cid { get; private set; }

    public DateOnly EvolutionDate { get; private set; }
    public TimeSpan EvolutionTime { get; private set; }

    // Mantemos APENAS o Cargo aqui na tabela clínica!
    public string? ProfessionalRole { get; private set; }

    public string RawText { get; private set; }

    protected PatientEvolution() { }

    // CONSTRUTOR COM EXATAMENTE 8 PARÂMETROS
    public PatientEvolution(Guid attendanceId, string originalEvolutionCode, EvolutionType type, string? cid, DateOnly evolutionDate, TimeSpan evolutionTime, string? professionalRole, string rawText)
    {
        AttendanceId = attendanceId;
        OriginalEvolutionCode = originalEvolutionCode;
        Type = type;
        Cid = cid;
        EvolutionDate = evolutionDate;
        EvolutionTime = evolutionTime;
        ProfessionalRole = professionalRole;
        RawText = rawText;
    }
}