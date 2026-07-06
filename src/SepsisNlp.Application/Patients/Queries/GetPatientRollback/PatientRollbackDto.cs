namespace SepsisNlp.Application.Patients.Queries.GetPatientRollback;

public record PatientRollbackDto(
    string Pseudonym,
    string RealMedicalRecord,
    string RealName
);