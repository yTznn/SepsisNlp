using SepsisNlp.Domain.Common;

namespace SepsisNlp.Domain.Entities;

public class Attendance : Entity
{
    public Guid PatientId { get; private set; }
    public string Pseudonym { get; private set; }
    public Patient Patient { get; private set; } = null!;

    // NOVOS CAMPOS DE ALTA
    public string? DischargeCid { get; private set; }
    public string? DischargeCidDescription { get; private set; }
    public DateOnly? DischargeDate { get; private set; }

    private readonly List<PatientEvolution> _evolutions = new();
    public IReadOnlyCollection<PatientEvolution> Evolutions => _evolutions.AsReadOnly();

    protected Attendance() { }

    public Attendance(Guid patientId, string pseudonym)
    {
        PatientId = patientId;
        Pseudonym = pseudonym;
    }

    // MÉTODO NOVO: Centraliza a atualização dos dados de fechamento do atendimento
    public void ComplementDischargeData(string cid, string? description, DateOnly? dischargeDate)
    {
        DischargeCid = cid;
        DischargeCidDescription = description;

        if (dischargeDate.HasValue)
        {
            DischargeDate = dischargeDate;
        }
    }
}