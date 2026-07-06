using SepsisNlp.Domain.Common;

namespace SepsisNlp.Domain.Entities;

public class Patient : Entity
{
    public string Pseudonym { get; private set; }
    public DateOnly? BirthDate { get; private set; }
    public string? Gender { get; private set; } // NOVO CAMPO: "M" ou "F"

    private readonly List<Attendance> _attendances = new();
    public IReadOnlyCollection<Attendance> Attendances => _attendances.AsReadOnly();

    protected Patient() { }

    public Patient(string pseudonym)
    {
        if (string.IsNullOrWhiteSpace(pseudonym))
            throw new ArgumentException("O pseudônimo não pode ser vazio.");

        Pseudonym = pseudonym;
    }

    public void UpdateBirthDate(DateOnly birthDate)
    {
        if (BirthDate == null)
            BirthDate = birthDate;
    }

    // MÉTODO NOVO: Atualiza o sexo se ele ainda não estiver preenchido
    public void UpdateGender(string gender)
    {
        if (string.IsNullOrWhiteSpace(Gender) && (gender == "M" || gender == "F"))
        {
            Gender = gender;
        }
    }
}