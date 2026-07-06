using SepsisNlp.Domain.Enums;

namespace SepsisNlp.Application.Evolutions.Events;

public record EvolutionCsvRowReceivedEvent(
    string OriginalEvolutionCode,
    EvolutionType Type,
    string AttendanceNumber,
    string? Cid,
    DateOnly EvolutionDate,
    TimeSpan EvolutionTime,
    string? ProfessionalRole,
    string? ProfessionalCouncil,
    string? ProfessionalName,
    string RawText,
    DateOnly? PatientBirthDate
);